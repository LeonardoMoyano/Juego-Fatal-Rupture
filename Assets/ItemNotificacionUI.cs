using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la visualizacion de una notificacion individual en pantalla.
/// Este script va en el Prefab del panel de notificacion.
/// 
/// Estructura recomendada del Prefab:
///   [Panel] (ItemNotificacionUI)
///     |- [IconoItem]   (Image)
///     |- [NombreItem]  (TextMeshProUGUI)
///     |- [CantidadItem](TextMeshProUGUI)
/// </summary>
public class ItemNotificacionUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Texto con el nombre del item")]
    public TextMeshProUGUI textoNombre;

    [Tooltip("Texto con la cantidad total en inventario")]
    public TextMeshProUGUI textoCantidad;

    [Tooltip("Icono opcional del item (puede dejarse vacio)")]
    public Image iconoItem;

    [Header("Animacion")]
    [Tooltip("Duracion del fade-in al aparecer")]
    public float tiempoFadeIn = 0.25f;

    [Tooltip("Duracion del fade-out al desaparecer")]
    public float tiempoFadeOut = 0.4f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    void OnEnable()
    {
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Inicializa la notificacion con los datos del item.
    /// </summary>
    /// <param name="nombre">Nombre del item</param>
    /// <param name="cantidadTotal">Cantidad acumulada en inventario</param>
    /// <param name="icono">Sprite opcional del item</param>
    public void Configurar(string nombre, int cantidadTotal, Sprite icono = null)
    {
        if (textoNombre != null)
            textoNombre.text = nombre.ToUpper();

        if (textoCantidad != null)
            textoCantidad.text = $"x{cantidadTotal}";

        if (iconoItem != null)
        {
            if (icono != null)
            {
                iconoItem.sprite = icono;
                iconoItem.gameObject.SetActive(true);
            }
            else
            {
                iconoItem.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Inicia el fade-out antes de que InventarioManager destruya este objeto.
    /// Llamado automaticamente por el Manager.
    /// </summary>
    public void IniciarFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < tiempoFadeIn)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / tiempoFadeIn);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < tiempoFadeOut)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (t / tiempoFadeOut));
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
