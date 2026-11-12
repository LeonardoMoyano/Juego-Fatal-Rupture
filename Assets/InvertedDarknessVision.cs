using UnityEngine;

/// <summary>
/// Crea y dibuja la capa de oscuridad envolvente (Capa 2)
/// invirtiendo la malla del campo de visión (Mesh Invertida / Donut):
/// - Dibuja un rectángulo negro gigante alrededor que cubre toda la cámara.
/// - Deja un agujero transparente exactamente en la forma del cono de visión y el círculo cercano.
/// - NO depende de Stencil ni de buffers que URP 2D pueda apagar.
/// - Funciona 100% garantizado con cualquier shader estándar de Sprites.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class InvertedDarknessVision : MonoBehaviour
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
        // Situar la malla centrada en la posición del jugador
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -0.5f);

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mouseWorld - playerTransform.position);
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Tamaño de la caja exterior negra para cubrir de sobra toda la pantalla
        float outerRadius = cam.orthographicSize * 3f + 50f;

        // Vértices:
        // [0 .. totalRays] = borde interior del agujero de visión
        // [totalRays + 1 .. 2 * totalRays + 1] = borde exterior gigante negro
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
                float blend = Mathf.Cos((absDiff / halfFov) * (Mathf.PI / 2f));
                maxRayDistance = Mathf.Lerp(closeRadius, viewDistance, blend);
            }

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

            // Vértice interior (borde del agujero de visión)
            vertices[i] = innerPointLocal;
            colors[i] = darknessColor;

            // Vértice exterior gigante (cubre la pantalla)
            vertices[ringCount + i] = dir * outerRadius;
            colors[ringCount + i] = darknessColor;

            // Triángulos conectando el anillo interior con el anillo exterior
            if (i < totalRays)
            {
                int currentInner = i;
                int nextInner = i + 1;
                int currentOuter = ringCount + i;
                int nextOuter = ringCount + i + 1;

                int triIndex = i * 6;

                // Triángulo 1
                triangles[triIndex + 0] = currentInner;
                triangles[triIndex + 1] = currentOuter;
                triangles[triIndex + 2] = nextInner;

                // Triángulo 2
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
