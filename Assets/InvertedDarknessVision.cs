using UnityEngine;

/// <summary>
/// Sistema de Oscuridad Invertida / Campo de Visión estilo Project Zomboid:
/// - Cubre toda la pantalla con negro puro conectándose a los bordes de la cámara.
/// - Recorta el cono de visión y el círculo cercano de espaldas.
/// - Los rayos de visión colisionan contra paredes (Layer Wall).
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class InvertedDarknessVision : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerTransform;

    [Header("Capa de Obstáculos")]
    public LayerMask obstacleLayer;

    [Header("Visión Principal (Cono)")]
    [Tooltip("Ángulo del cono hacia donde mira el mouse")]
    [Range(30f, 180f)]
    public float fovAngle = 105f;

    [Tooltip("Alcance de visión hacia adelante")]
    public float viewDistance = 45f;

    [Header("Visión Periférica Cercana (360°)")]
    [Tooltip("Radio de visión alrededor del personaje (visión inmediata)")]
    public float closeRadius = 7f;

    [Header("Calidad")]
    [Range(90, 720)]
    public int totalRays = 360;

    [Header("Color de la Oscuridad")]
    public Color darknessColor = Color.black;

    [Header("Orden de Renderizado")]
    public int sortingOrder = 20;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;
    private MeshRenderer meshRenderer;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Inverted_Darkness_Mesh";
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
        GenerateInvertedMesh();
    }

    private void GenerateInvertedMesh()
    {
        if (playerTransform == null || Camera.main == null) return;

        Camera cam = Camera.main;
        // Situar en la posición del jugador pero con Z adelantada (-0.5f) para tapar el mapa
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -0.5f);

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mouseWorld - playerTransform.position);
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Radio exterior gigante que garantice tapar cualquier resolución y zoom de cámara
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        float outerRadius = Mathf.Max(camWidth, camHeight) * 3f + 100f;

        int ringCount = totalRays + 1;
        int totalVertices = ringCount * 2;

        if (vertices == null || vertices.Length != totalVertices)
        {
            vertices = new Vector3[totalVertices];
            colors = new Color[totalVertices];
            triangles = new int[totalRays * 6];
        }

        float angleStep = 360f / totalRays;

        for (int i = 0; i <= totalRays; i++)
        {
            float currentAngle = angleStep * i;
            Vector3 dir = GetVectorFromAngle(currentAngle);

            float angleDiff = Mathf.DeltaAngle(aimAngle, currentAngle);
            float maxRayDistance = closeRadius;
            float halfFov = fovAngle / 2f;
            float absDiff = Mathf.Abs(angleDiff);

            if (absDiff <= halfFov)
            {
                // Cono frontal largo
                float blend = Mathf.Cos((absDiff / halfFov) * (Mathf.PI / 2f));
                maxRayDistance = Mathf.Lerp(closeRadius, viewDistance, blend);
            }

            // Raycast contra paredes
            RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, dir, maxRayDistance, obstacleLayer);

            Vector3 innerPointLocal;
            if (hit.collider != null)
            {
                innerPointLocal = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                innerPointLocal = dir * maxRayDistance;
            }

            // Borde interior (hueco de visión)
            vertices[i] = innerPointLocal;
            colors[i] = darknessColor;

            // Borde exterior (manto negro que cubre toda la pantalla)
            vertices[ringCount + i] = dir * outerRadius;
            colors[ringCount + i] = darknessColor;

            if (i < totalRays)
            {
                int currentInner = i;
                int nextInner = i + 1;
                int currentOuter = ringCount + i;
                int nextOuter = ringCount + i + 1;

                int triIndex = i * 6;

                triangles[triIndex + 0] = currentInner;
                triangles[triIndex + 1] = currentOuter;
                triangles[triIndex + 2] = nextInner;

                triangles[triIndex + 3] = nextInner;
                triangles[triIndex + 4] = currentOuter;
                triangles[triIndex + 5] = nextOuter;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);
    }
}
