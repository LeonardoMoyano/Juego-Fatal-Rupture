using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona el inventario del jugador y la UI de notificaciones.
/// Si no hay un prefab asignado, crea automáticamente una UI flotante elegante en pantalla.
/// </summary>
public class InventarioManager : MonoBehaviour
{
    public static InventarioManager Instance { get; private set; }

    [Header("UI - Panel de Notificacion")]
    [Tooltip("Prefab del panel de notificacion (ItemNotificacionUI)")]
    public GameObject notificacionPrefab;

    [Tooltip("Contenedor vertical en el lado derecho de la pantalla")]
    public Transform notificacionesContainer;

    [Header("Configuracion")]
    [Tooltip("Duracion en segundos que se muestra cada notificacion")]
    public float duracionNotificacion = 3f;

    private Dictionary<string, int> inventario = new Dictionary<string, int>();
    private List<GameObject> notificacionesActivas = new List<GameObject>();
    private const int MAX_NOTIFICACIONES = 4;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        AsegurarContenedorUI();
    }

    private void AsegurarContenedorUI()
    {
        if (notificacionesContainer == null)
        {
            // Buscar un Canvas existente en la escena
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas_Notificaciones");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // Crear el contenedor alineado a la derecha
            GameObject containerGO = new GameObject("Contenedor_Notificaciones");
            containerGO.transform.SetParent(canvas.transform, false);

            RectTransform rt = containerGO.AddComponent<RectTransform>();
            // Anclar a la derecha media
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-25f, 0f);
            rt.sizeDelta = new Vector2(240f, 300f);

            VerticalLayoutGroup layout = containerGO.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = 10;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            notificacionesContainer = containerGO.transform;
        }
    }

    /// <summary>
    /// Agrega un item al inventario y muestra la notificacion en pantalla.
    /// </summary>
    public void AgregarItem(string nombreItem, int cantidad = 1)
    {
        if (string.IsNullOrEmpty(nombreItem)) return;

        if (inventario.ContainsKey(nombreItem))
            inventario[nombreItem] += cantidad;
        else
            inventario[nombreItem] = cantidad;

        Debug.Log($"<color=cyan>[Inventario] {nombreItem} x{cantidad} | Total: {inventario[nombreItem]}</color>");

        MostrarNotificacion(nombreItem, inventario[nombreItem]);
    }

    public int ObtenerCantidad(string nombreItem)
    {
        return inventario.ContainsKey(nombreItem) ? inventario[nombreItem] : 0;
    }

    public Dictionary<string, int> ObtenerInventarioCompleto()
    {
        return new Dictionary<string, int>(inventario);
    }

    // --- UI Interna ---

    private void MostrarNotificacion(string nombreItem, int cantidadTotal)
    {
        AsegurarContenedorUI();
        if (notificacionesContainer == null) return;

        if (notificacionesActivas.Count >= MAX_NOTIFICACIONES)
        {
            GameObject primera = notificacionesActivas[0];
            notificacionesActivas.RemoveAt(0);
            if (primera != null) Destroy(primera);
        }

        GameObject notif;
        if (notificacionPrefab != null)
        {
            notif = Instantiate(notificacionPrefab, notificacionesContainer);
            ItemNotificacionUI ui = notif.GetComponent<ItemNotificacionUI>();
            if (ui != null) ui.Configurar(nombreItem, cantidadTotal);
        }
        else
        {
            // Si Brian no creó el Prefab, lo generamos proceduralmente en pantalla
            notif = CrearNotificacionProcedural(nombreItem, cantidadTotal);
        }

        notificacionesActivas.Add(notif);
        StartCoroutine(DestruirNotificacion(notif, duracionNotificacion));
    }

    private GameObject CrearNotificacionProcedural(string nombre, int cantidad)
    {
        GameObject panel = new GameObject($"Notif_{nombre}");
        panel.transform.SetParent(notificacionesContainer, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220f, 45f);

        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.12f, 0.85f); // Fondo oscuro traslúcido

        CanvasGroup cg = panel.AddComponent<CanvasGroup>();

        // Texto con el nombre y la cantidad
        GameObject textoGO = new GameObject("Texto");
        textoGO.transform.SetParent(panel.transform, false);

        RectTransform textRT = textoGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        textRT.offsetMin = new Vector2(12f, 0f);
        textRT.offsetMax = new Vector2(-12f, 0f);

        Text txt = textoGO.AddComponent<Text>();
        txt.text = $"<b>{nombre.ToUpper()}</b>  x{cantidad}";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 15;
        txt.color = new Color(0.95f, 0.95f, 0.8f, 1f);
        txt.alignment = TextAnchor.MiddleLeft;

        // Animar Fade In / Fade Out
        StartCoroutine(AnimarFade(cg));

        return panel;
    }

    private IEnumerator AnimarFade(CanvasGroup cg)
    {
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            if (cg != null) cg.alpha = Mathf.Clamp01(t / 0.25f);
            yield return null;
        }
        if (cg != null) cg.alpha = 1f;

        yield return new WaitForSeconds(duracionNotificacion - 0.6f);

        t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            if (cg != null) cg.alpha = Mathf.Clamp01(1f - (t / 0.4f));
            yield return null;
        }
        if (cg != null) cg.alpha = 0f;
    }

    private IEnumerator DestruirNotificacion(GameObject notif, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (notif != null)
        {
            notificacionesActivas.Remove(notif);
            Destroy(notif);
        }
    }
}
