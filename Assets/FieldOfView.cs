using UnityEngine;

/// <summary>
/// Genera dinámicamente una malla de campo de visión (Cono de visión) que detecta
/// y se corta contra las paredes y obstáculos (BoxCollider2D / Layer Wall).
/// 
/// Características:
/// - Sigue la dirección a la que apunta el jugador / mouse.
/// - Los rayos de visión se detienen al impactar obstáculos.
/// - FOV (ángulo), alcance (viewDistance) y resolución de rayos configurables.
/// - Soporta un pequeño radio de visión periférica alrededor del jugador (para que no esté ciego a sus espaldas).
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform del jugador para posicionar y orientar el cono. Si está vacío, toma el padre o a sí mismo.")]
    public Transform playerTransform;

    [Header("Capa de Obstáculos")]
    [Tooltip("Capa de las paredes que bloquearán la visión")]
    public LayerMask obstacleLayer;

    [Header("Configuración de Visión")]
    [Tooltip("Ángulo del cono de visión en grados (ej: 80 para linterna/visión normal)")]
    [Range(10f, 360f)]
    public float fov = 85f;

    [Tooltip("Distancia máxima de visión hacia adelante")]
    public float viewDistance = 25f;

    [Tooltip("Radio de visión inmediata/periférica alrededor del jugador")]
    public float innerRadius = 2.5f;

    [Header("Calidad y Suavizado")]
    [Tooltip("Cantidad de rayos lanzados. Más rayos = bordes más precisos en esquinas")]
    [Range(50, 400)]
    public int rayCount = 180;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "FOV_Mesh";
        GetComponent<MeshFilter>().mesh = mesh;

        if (playerTransform == null)
        {
            // Si está como hijo del player, usar el padre
            if (transform.parent != null)
                playerTransform = transform.parent;
            else
            {
                GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null) playerTransform = playerGO.transform;
            }
        }

        // Si obstacleLayer no fue configurado en el inspector, buscar la capa "Wall"
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
        GenerateVisionMesh();
    }

    private void GenerateVisionMesh()
    {
        if (playerTransform == null) return;

        // Mantener la posición en el jugador
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -0.5f);

        // Obtener dirección del mouse para orientar el cono
        Vector3 mouseWorld = Camera.main != null ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
        Vector2 aimDirection = (mouseWorld - playerTransform.position);
        float baseAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // El arco del cono va desde (baseAngle - fov / 2) hasta (baseAngle + fov / 2)
        float startAngle = baseAngle - (fov / 2f);
        float angleStep = fov / rayCount;

        // Vertices: [0] = centro (jugador), + (rayCount + 1) vértices del cono
        int totalVertices = rayCount + 2;
        if (vertices == null || vertices.Length != totalVertices)
        {
            vertices = new Vector3[totalVertices];
            uv = new Vector2[totalVertices];
            triangles = new int[rayCount * 3];
        }

        vertices[0] = Vector3.zero; // Origen local
        uv[0] = new Vector2(0.5f, 0.5f);

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 dir = GetVectorFromAngle(currentAngle);

            // Raycast desde la posición del jugador hacia la dirección
            RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, dir, viewDistance, obstacleLayer);

            Vector3 hitPointLocal;
            if (hit.collider != null)
            {
                // Choque con pared
                hitPointLocal = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                // Visión libre hasta el alcance máximo
                hitPointLocal = dir * viewDistance;
            }

            vertices[vertexIndex] = hitPointLocal;

            // UVs normalizados
            uv[vertexIndex] = new Vector2(
                (hitPointLocal.x / (viewDistance * 2f)) + 0.5f,
                (hitPointLocal.y / (viewDistance * 2f)) + 0.5f
            );

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
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);
    }

    /// <summary>
    /// Permite a otros scripts (como la linterna) cambiar el alcance y el ángulo del cono en tiempo real.
    /// </summary>
    public void SetVisionParameters(float newFov, float newDistance)
    {
        fov = newFov;
        viewDistance = newDistance;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(playerTransform.position, viewDistance);
    }
}
