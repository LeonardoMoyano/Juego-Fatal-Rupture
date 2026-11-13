using UnityEngine;

/// <summary>
/// Sistema de Campo de Visión y Oscuridad estilo Project Zomboid:
/// - Cono frontal amplio estilo abanico humano (configurable en ángulo y distancia).
/// - Círculo cercano de 360° alrededor del cuerpo para no estar ciego de espaldas.
/// - Bordes con DEGRADADO / DIFUMINADO SUAVE (penumbra) antes de la oscuridad total.
/// - Detección y bloqueo milimétrico con paredes (Layer Wall).
/// - Se congela / pausa automáticamente cuando el inventario (TAB) está abierto.
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

    // Guarda el último ángulo apuntado para mantenerlo fijo cuando el inventario está abierto
    private float lastAimAngle = 0f;

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
        // Si el inventario está abierto, no recalculamos hacia el mouse:
        // mantenemos congelada la visión en la última dirección que miraba el personaje
        if (InventarioUI.EstaAbierto) return;

        GenerateSoftZomboidMesh();
    }

    private void GenerateSoftZomboidMesh()
    {
        if (playerTransform == null || Camera.main == null) return;

        Camera cam = Camera.main;

        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -0.5f);
        transform.rotation = Quaternion.identity;

        Vector3 parentLossyScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            parentLossyScale.x != 0f ? 1f / parentLossyScale.x : 1f,
            parentLossyScale.y != 0f ? 1f / parentLossyScale.y : 1f,
            1f
        );

        Vector2 playerPos = (Vector2)playerTransform.position;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);

        Vector2 aimDirection = (new Vector2(mouseWorld.x, mouseWorld.y) - playerPos);
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        lastAimAngle = aimAngle;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        float outerRadius = Mathf.Max(camWidth, camHeight) * 4f + 150f;

        int ringCount = totalRays + 1;
        int totalVertices = ringCount * 3;

        if (vertices == null || vertices.Length != totalVertices)
        {
            vertices = new Vector3[totalVertices];
            colors = new Color[totalVertices];
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

            float theoreticalMaxDistance = closeRadius;
            if (absDiff <= halfFov)
            {
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

            RaycastHit2D hit = Physics2D.Raycast(playerPos, dir, theoreticalMaxDistance, obstacleLayer);

            float clearDistance;
            float solidDarknessDistance;

            if (hit.collider != null)
            {
                clearDistance = hit.distance;
                solidDarknessDistance = hit.distance + 0.1f;
            }
            else
            {
                float effectiveFade = Mathf.Min(fadeDistance, theoreticalMaxDistance * 0.4f);
                clearDistance = Mathf.Max(theoreticalMaxDistance - effectiveFade, 0.5f);
                solidDarknessDistance = theoreticalMaxDistance;
            }

            vertices[i] = dir * clearDistance;
            colors[i] = new Color(0f, 0f, 0f, 0f);

            vertices[ringCount + i] = dir * solidDarknessDistance;
            colors[ringCount + i] = new Color(0f, 0f, 0f, 1f);

            vertices[ringCount * 2 + i] = dir * outerRadius;
            colors[ringCount * 2 + i] = new Color(0f, 0f, 0f, 1f);

            if (i < totalRays)
            {
                int c1 = i;
                int n1 = i + 1;
                int c2 = ringCount + i;
                int n2 = ringCount + i + 1;
                int c3 = ringCount * 2 + i;
                int n3 = ringCount * 2 + i + 1;

                int tri1 = i * 6;
                triangles[tri1 + 0] = c1;
                triangles[tri1 + 1] = c2;
                triangles[tri1 + 2] = n1;

                triangles[tri1 + 3] = n1;
                triangles[tri1 + 4] = c2;
                triangles[tri1 + 5] = n2;

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
