using UnityEngine;

/// <summary>
/// Crea y mantiene una capa negra translúcida/opaca sobre la cámara,
/// que oscurece el escenario.
/// </summary>
public class DarknessOverlay : MonoBehaviour
{
    [Header("Ajustes de Oscuridad")]
    [Tooltip("Color de la penumbra exterior (opacidad configurable)")]
    public Color darknessColor = new Color(0.01f, 0.01f, 0.02f, 0.85f);

    [Header("Orden de Renderizado")]
    public int sortingOrder = 5;

    private SpriteRenderer overlayRenderer;

    void Start()
    {
        overlayRenderer = GetComponent<SpriteRenderer>();
        if (overlayRenderer == null)
        {
            overlayRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        overlayRenderer.sprite = sprite;
        overlayRenderer.color = darknessColor;
        overlayRenderer.sortingOrder = sortingOrder;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            Camera cam = Camera.main;
            float height = cam.orthographicSize * 2f + 20f;
            float width = height * cam.aspect + 20f;

            transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, -0.5f);
            transform.localScale = new Vector3(width, height, 1f);
        }
    }

    public void SetDarknessAlpha(float alpha)
    {
        darknessColor.a = Mathf.Clamp01(alpha);
        if (overlayRenderer != null)
        {
            overlayRenderer.color = darknessColor;
        }
    }
}
