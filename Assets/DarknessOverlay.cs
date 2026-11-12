using UnityEngine;

/// <summary>
/// Crea y mantiene una capa negra translúcida/opaca (Fog of War) sobre la cámara,
/// que sirve como Capa 2 de oscuridad. La malla de FieldOfView se ubica
/// y renderiza para interactuar con la visibilidad del jugador.
/// </summary>
public class DarknessOverlay : MonoBehaviour
{
    [Header("Ajustes de Oscuridad")]
    [Tooltip("Color de la oscuridad exterior")]
    public Color darknessColor = new Color(0.02f, 0.02f, 0.04f, 0.88f);

    [Header("Referencias")]
    public Material fovMaterial;

    private SpriteRenderer overlayRenderer;

    void Start()
    {
        // Si no tiene SpriteRenderer, crear uno básico
        overlayRenderer = GetComponent<SpriteRenderer>();
        if (overlayRenderer == null)
        {
            overlayRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Crear una textura sólida negra de 1x1
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        overlayRenderer.sprite = sprite;
        overlayRenderer.color = darknessColor;

        // Sorting layer u orden para estar por encima del mapa pero debajo del cono y UI
        overlayRenderer.sortingOrder = 5;
    }

    void LateUpdate()
    {
        // Ajustar el tamaño del overlay para cubrir el tamaño visible de la cámara siempre
        if (Camera.main != null)
        {
            Camera cam = Camera.main;
            float height = cam.orthographicSize * 2f + 10f;
            float width = height * cam.aspect + 10f;

            transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, -0.2f);
            transform.localScale = new Vector3(width, height, 1f);
        }
    }

    /// <summary>
    /// Permite ajustar la opacidad de la oscuridad en tiempo real
    /// </summary>
    public void SetDarknessAlpha(float alpha)
    {
        darknessColor.a = Mathf.Clamp01(alpha);
        if (overlayRenderer != null)
        {
            overlayRenderer.color = darknessColor;
        }
    }
}
