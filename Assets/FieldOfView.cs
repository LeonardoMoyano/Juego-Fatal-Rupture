using UnityEngine;

/// <summary>
/// Genera dinámicamente una malla de campo de visión (Cono de visión) que detecta
/// y se corta contra las paredes y obstáculos (BoxCollider2D / Layer Wall).
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerTransform;

    [Header("Capa de Obstáculos")]
    public LayerMask obstacleLayer;

    [Header("Visión Principal (Cono)")]
    [Range(30f, 180f)]
    public float fovAngle = 100f;

    public float viewDistance = 35f;

    [Header("Visión Periférica Cercana (360°)")]
    public float closeRadius = 5.5f;

    [Header("Calidad")]
    [Range(90, 720)]
    public int totalRays = 360;

    [Header("Capa de Orden (Sorting)")]
    public int sortingOrder = 10;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private MeshRenderer meshRenderer;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Zomboid_FOV_Mesh";
        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = sortingOrder;
        }

        if (playerTransform == null)
        {
            if (transform.parent != null)
                playerTransform = transform.parent;
            else
            {
                GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null) playerTransform = playerGO.transform;
            }
        }

        if (obstacleLayer.value == 0)
        {
            int wallLayerIndex = LayerMask.NameToLayer("Wall");
            if (wallLayerIndex != -1)
            {
                obstacleLayer = 1 << wallLayerIndex;
            }
        }
    }

    void LateUpdate()
    {
        GenerateZomboidMesh();
    }

    private void GenerateZomboidMesh()
    {
        if (playerTransform == null) return;

        // Mantener posición exacta del jugador con Z = -0.5f (entre el mapa y la cámara)
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -0.5f);

        Vector3 mouseWorld = Camera.main != null ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
        Vector2 aimDirection = (mouseWorld - playerTransform.position);
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        int totalVertices = totalRays + 2;
        if (vertices == null || vertices.Length != totalVertices)
        {
            vertices = new Vector3[totalVertices];
            triangles = new int[totalRays * 3];
        }

        vertices[0] = Vector3.zero;

        float angleStep = 360f / totalRays;
        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= totalRays; i++)
        {
            float currentAngle = (angleStep * i);
            Vector3 dir = GetVectorFromAngle(currentAngle);

            float angleDiff = Mathf.DeltaAngle(aimAngle, currentAngle);

            float maxRayDistance = closeRadius;
            float halfFov = fovAngle / 2f;
            float absDiff = Mathf.Abs(angleDiff);

            if (absDiff <= halfFov)
            {
                float blend = Mathf.Cos((absDiff / halfFov) * (Mathf.PI / 2f));
                maxRayDistance = Mathf.Lerp(closeRadius, viewDistance, blend);
            }

            RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, dir, maxRayDistance, obstacleLayer);

            Vector3 hitPointLocal;
            if (hit.collider != null)
            {
                hitPointLocal = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                hitPointLocal = dir * maxRayDistance;
            }

            vertices[vertexIndex] = hitPointLocal;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);
    }
}
