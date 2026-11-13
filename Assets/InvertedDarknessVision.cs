using UnityEngine;

/// <summary>
/// Sistema de Oscuridad Invertida / Campo de Visión estilo Project Zomboid:
/// - Cubre toda la pantalla con negro puro.
/// - Los vértices se calculan en espacio de MUNDO para ser 100% inmunes a escala o rotación del Player.
/// - Apunta con precisión matemática exactamente hacia el cursor del mouse.
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
    public float fovAngle = 100f;

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

        // Anular cualquier rotación heredada del Player
        transform.rotation = Quaternion.identity;

        // IMPORTANTE: Neutralizar la escala heredada del Player para que el cono no se aplaste
        Vector3 parentLossyScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            parentLossyScale.x != 0f ? 1f / parentLossyScale.x : 1f,
            parentLossyScale.y != 0f ? 1f / parentLossyScale.y : 1f,
            1f
        );

        // Posición del jugador
        Vector2 playerPos = (Vector2)playerTransform.position;

        // Coordenadas del mouse en el mundo
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);

        Vector2 aimDirection = (new Vector2(mouseWorld.x, mouseWorld.y) - playerPos);
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Radio exterior gigante que tape toda la pantalla
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        float outerRadius = Mathf.Max(camWidth, camHeight) * 4f + 150f;

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
                // Cono frontal apuntando al cursor
                float blend = Mathf.Cos((absDiff / halfFov) * (Mathf.PI / 2f));
                maxRayDistance = Mathf.Lerp(closeRadius, viewDistance, blend);
            }

            // Raycast contra paredes
            RaycastHit2D hit = Physics2D.Raycast(playerPos, dir, maxRayDistance, obstacleLayer);

            Vector3 innerPointLocal;
            if (hit.collider != null)
            {
                // Punto local respecto al centro del jugador
                innerPointLocal = new Vector3(hit.point.x - playerPos.x, hit.point.y - playerPos.y, 0f);
            }
            else
            {
                innerPointLocal = dir * maxRayDistance;
            }

            // Borde interior (hueco de visión recortado)
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
