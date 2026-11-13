using UnityEngine;

/// <summary>
/// Sistema de Campo de Visión y Oscuridad estilo Project Zomboid:
/// - Cono frontal amplio estilo abanico humano (configurable en ángulo y distancia).
/// - Círculo cercano de 360° alrededor del cuerpo para no estar ciego de espaldas.
/// - Bordes con DEGRADADO / DIFUMINADO SUAVE (penumbra) antes de la oscuridad total.
/// - Detección y bloqueo milimétrico con paredes (Layer Wall).
/// - 100% inmune a la escala o rotación del Player.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class InvertedDarknessVision : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerTransform;

    [Header("Capa de Obstáculos")]
    public LayerMask obstacleLayer;

    [Header("Cono Frontal de Visión (Abanico)")]
    [Tooltip("Ángulo de visión frontal (Project Zomboid y visión humana usan entre 110° y 135°)")]
    [Range(45f, 170f)]
    public float fovAngle = 125f;

    [Tooltip("Distancia máxima de visión hacia adelante")]
    public float viewDistance = 40f;

    [Header("Visión Periférica Cercana (360°)")]
    [Tooltip("Radio de visión inmediata alrededor del cuerpo")]
    public float closeRadius = 5.5f;

    [Header("Difuminado de Bordes (Penumbra Zomboid)")]
    [Tooltip("Ancho del área de difuminado suave en el borde exterior del cono")]
    [Range(1f, 15f)]
    public float fadeDistance = 6f;

    [Header("Calidad")]
    [Tooltip("Cantidad de rayos del escaneo (360 recomendado)")]
    [Range(90, 720)]
    public int totalRays = 360;

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
        mesh.name = "Soft_Zomboid_Darkness_Mesh";
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
        GenerateSoftZomboidMesh();
    }

    private void GenerateSoftZomboidMesh()
    {
        if (playerTransform == null || Camera.main == null) return;

        Camera cam = Camera.main;

        // Mantener posición del jugador y anular rotación del padre
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -0.5f);
        transform.rotation = Quaternion.identity;

        // Neutralizar escala heredada del Player
        Vector3 parentLossyScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            parentLossyScale.x != 0f ? 1f / parentLossyScale.x : 1f,
            parentLossyScale.y != 0f ? 1f / parentLossyScale.y : 1f,
            1f
        );

        Vector2 playerPos = (Vector2)playerTransform.position;

        // Dirección y ángulo hacia el mouse
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);

        Vector2 aimDirection = (new Vector2(mouseWorld.x, mouseWorld.y) - playerPos);
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Radio exterior gigante que cubra cualquier resolución de pantalla
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        float outerRadius = Mathf.Max(camWidth, camHeight) * 4f + 150f;

        // Estructura de 3 Anillos concéntricos:
        // Anillo 1 (0 .. totalRays): Borde 100% transparente (visión clara)
        // Anillo 2 (ringCount .. 2*ringCount - 1): Borde con inicio de oscuridad total (fin del degradado)
        // Anillo 3 (2*ringCount .. 3*ringCount - 1): Borde gigante negro que tapa toda la pantalla
        int ringCount = totalRays + 1;
        int totalVertices = ringCount * 3;

        if (vertices == null || vertices.Length != totalVertices)
        {
            vertices = new Vector3[totalVertices];
            colors = new Color[totalVertices];
            // 2 bandas de quads = 2 * totalRays * 6 índices de triángulos
            triangles = new int[totalRays * 12];
        }

        float angleStep = 360f / totalRays;
        float halfFov = fovAngle / 2f;

        for (int i = 0; i <= totalRays; i++)
        {
            float currentAngle = angleStep * i;
            Vector3 dir = GetVectorFromAngle(currentAngle);

            float angleDiff = Mathf.DeltaAngle(aimAngle, currentAngle);
            float absDiff = Mathf.Abs(angleDiff);

            // Forma de abanico amplio (Pie Slice / Cono de Project Zomboid):
            // Dentro del ángulo de visión es el alcance frontal completo;
            // en los costados transiciona al radio cercano.
            float theoreticalMaxDistance = closeRadius;
            if (absDiff <= halfFov)
            {
                // En el cono frontal: borde plano/circular amplio como en el diagrama
                // Suavizamos ligeramente solo los últimos 15° del borde lateral del cono
                float edgeAngle = 15f;
                if (absDiff > halfFov - edgeAngle)
                {
                    float t = (absDiff - (halfFov - edgeAngle)) / edgeAngle;
                    theoreticalMaxDistance = Mathf.Lerp(viewDistance, closeRadius, t);
                }
                else
                {
                    theoreticalMaxDistance = viewDistance;
                }
            }

            // Raycast contra paredes
            RaycastHit2D hit = Physics2D.Raycast(playerPos, dir, theoreticalMaxDistance, obstacleLayer);

            float clearDistance;
            float solidDarknessDistance;

            if (hit.collider != null)
            {
                // Si choca con una pared: el corte con la pared es neto (la sombra de la pared no traspasa)
                clearDistance = hit.distance;
                solidDarknessDistance = hit.distance + 0.1f;
            }
            else
            {
                // Si es campo abierto: la visión clara llega hasta (distancia - fadeDistance)
                // y desde ahí se difumina suavemente hasta la oscuridad total
                float effectiveFade = Mathf.Min(fadeDistance, theoreticalMaxDistance * 0.4f);
                clearDistance = Mathf.Max(theoreticalMaxDistance - effectiveFade, 0.5f);
                solidDarknessDistance = theoreticalMaxDistance;
            }

            // 1) Vértice del área de visión 100% clara (Alpha = 0, completamente transparente)
            vertices[i] = dir * clearDistance;
            colors[i] = new Color(0f, 0f, 0f, 0f);

            // 2) Vértice del final del degradado (Alpha = 1, oscuridad total empieza aquí)
            vertices[ringCount + i] = dir * solidDarknessDistance;
            colors[ringCount + i] = new Color(0f, 0f, 0f, 1f);

            // 3) Vértice exterior gigante para cubrir toda la cámara (Alpha = 1, negro sólido)
            vertices[ringCount * 2 + i] = dir * outerRadius;
            colors[ringCount * 2 + i] = new Color(0f, 0f, 0f, 1f);

            // Construir los triángulos de ambas bandas
            if (i < totalRays)
            {
                int c1 = i;
                int n1 = i + 1;
                int c2 = ringCount + i;
                int n2 = ringCount + i + 1;
                int c3 = ringCount * 2 + i;
                int n3 = ringCount * 2 + i + 1;

                // Banda 1: Degradado difuminado (desde Alpha 0 a Alpha 1)
                int tri1 = i * 6;
                triangles[tri1 + 0] = c1;
                triangles[tri1 + 1] = c2;
                triangles[tri1 + 2] = n1;

                triangles[tri1 + 3] = n1;
                triangles[tri1 + 4] = c2;
                triangles[tri1 + 5] = n2;

                // Banda 2: Manto negro exterior gigante sólido (Alpha 1 a Alpha 1)
                int tri2 = (totalRays * 6) + (i * 6);
                triangles[tri2 + 0] = c2;
                triangles[tri2 + 1] = c3;
                triangles[tri2 + 2] = n2;

                triangles[tri2 + 3] = n2;
                triangles[tri2 + 4] = c3;
                triangles[tri2 + 5] = n3;
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
