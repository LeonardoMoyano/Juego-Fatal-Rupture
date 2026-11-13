using UnityEngine;

/// <summary>
/// Crea y mantiene una capa negra translúcida/opaca sobre la cámara,
/// que oscurece el escenario y admite el material de corte Stencil.
/// </summary>
public class DarknessOverlay : MonoBehaviour
{
    [Header("Material Stencil")]
    [Tooltip("Arrastra aqui el material Mat_DarknessOverlay")]
    public Material darknessMaterial;

    [Header("Ajustes de Oscuridad")]
    public Color darknessColor = Color.black;

    [Header("Orden de Renderizado")]
    public int sortingOrder = 20;

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

        if (darknessMaterial != null)
        {
            overlayRenderer.material = darknessMaterial;
        }
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            Camera cam = Camera.main;
            float height = cam.orthographicSize * 2f + 30f;
            float width = height * cam.aspect + 30f;

            // Z ligeramente más alejada de la cámara que el VisionCone
            transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
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
