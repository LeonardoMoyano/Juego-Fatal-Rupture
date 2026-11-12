using UnityEngine;

/// <summary>
/// Sistema de Campo de Visión estilo Project Zomboid:
/// - Cono principal hacia la dirección que mira el jugador / mouse.
/// - Círculo de visión periférica alrededor del jugador (360° en corto alcance).
/// - Ambos respetan colisiones de paredes con Raycasts.
/// - Puntos UV mapeados radialmente para que el shader difumine suavemente hacia los bordes.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerTransform;

    [Header("Capa de Obstáculos")]
    [Tooltip("Capa de las paredes que bloquearán la visión")]
    public LayerMask obstacleLayer;

    [Header("Visión Principal (Cono)")]
    [Tooltip("Ángulo del cono de visión principal en grados (Project Zomboid suele usar 90° - 110°)")]
    [Range(30f, 180f)]
    public float fovAngle = 100f;

    [Tooltip("Distancia máxima de visión hacia adelante")]
    public float viewDistance = 35f;

    [Header("Visión Periférica Cercana (360°)")]
    [Tooltip("Radio de visión inmediata alrededor del personaje (visión cercana)")]
    public float closeRadius = 5.5f;

    [Header("Calidad")]
    [Tooltip("Cantidad total de rayos para todo el círculo de visión (más = bordes más limpios)")]
    [Range(90, 720)]
    public int totalRays = 360;

    [Header("Capa de Orden (Sorting)")]
    public int sortingOrder = 10;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;
    private Color[] colors;
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

        // Mantener posición exacta del jugador con ligera Z hacia la cámara
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -1f);

        // Obtener ángulo hacia el mouse
        Vector3 mouseWorld = Camera.main != null ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
        Vector2 aimDirection = (mouseWorld - playerTransform.position);
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        int totalVertices = totalRays + 2;
        if (vertices == null || vertices.Length != totalVertices)
        {
            vertices = new Vector3[totalVertices];
            uv = new Vector2[totalVertices];
            colors = new Color[totalVertices];
            triangles = new int[totalRays * 3];
        }

        // Vértice central en el jugador: Máxima visibilidad
        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0f, 1f); // X = distancia relativa (0 = centro), Y = opacidad (1 = total)
        colors[0] = new Color(1f, 1f, 1f, 1f);

        float angleStep = 360f / totalRays;
        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= totalRays; i++)
        {
            float currentAngle = (angleStep * i);
            Vector3 dir = GetVectorFromAngle(currentAngle);

            // Diferencia angular respecto a dónde apunta el mouse (-180 a 180)
            float angleDiff = Mathf.DeltaAngle(aimAngle, currentAngle);

            // Calcular el alcance teórico de este rayo:
            // Si está dentro de fovAngle/2, es el cono largo.
            // En los bordes del cono se suaviza la transición hacia el radio cercano (closeRadius).
            float maxRayDistance = closeRadius;
            float halfFov = fovAngle / 2f;
            float absDiff = Mathf.Abs(angleDiff);

            if (absDiff <= halfFov)
            {
                // Dentro del cono principal
                // Caída suave hacia los límites laterales del cono
                float blend = Mathf.Cos((absDiff / halfFov) * (Mathf.PI / 2f));
                maxRayDistance = Mathf.Lerp(closeRadius, viewDistance, blend);
            }

            // Raycast contra paredes
            RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, dir, maxRayDistance, obstacleLayer);

            Vector3 hitPointLocal;
            float actualDistance;
            if (hit.collider != null)
            {
                hitPointLocal = transform.InverseTransformPoint(hit.point);
                actualDistance = hit.distance;
            }
            else
            {
                hitPointLocal = dir * maxRayDistance;
                actualDistance = maxRayDistance;
            }

            vertices[vertexIndex] = hitPointLocal;

            // Mapeo de atenuación: mientras más lejos llega el rayo, más se desvanece
            float distRatio = Mathf.Clamp01(actualDistance / viewDistance);
            float edgeFade = 1f - Mathf.Pow(distRatio, 1.4f);
            
            // Si está fuera del cono (en la visión trasera), es más tenue
            if (absDiff > halfFov)
            {
                edgeFade *= 0.6f;
            }

            uv[vertexIndex] = new Vector2(distRatio, edgeFade);
            colors[vertexIndex] = new Color(1f, 1f, 1f, edgeFade);

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
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);
    }

    public void SetVisionParameters(float newFov, float newDistance, float newCloseRadius)
    {
        fovAngle = newFov;
        viewDistance = newDistance;
        closeRadius = newCloseRadius;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(playerTransform.position, viewDistance);
        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(playerTransform.position, closeRadius);
    }
}
