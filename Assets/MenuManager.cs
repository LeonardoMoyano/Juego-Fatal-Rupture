using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema completo de Menú para Fatal Rupture en Unity.
/// Recrea fielmente el diseño de menu.html, nueva_partida.html y ajustes.html
/// con efectos de Glitch de texto, fondo pixel art, transición a negro y navegación fluida.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Escena del Juego")]
    [Tooltip("Nombre de la escena a cargar al presionar CONTINUAR o INICIAR PARTIDA")]
    public string gameSceneName = "SampleScene";

    [Header("Textura de Fondo")]
    public Texture2D fondoTexture;

    // Paneles
    private GameObject menuPrincipalPanel;
    private GameObject nuevaPartidaPanel;
    private GameObject ajustesPanel;
    private Image blackFadeImage;

    void Awake()
    {
        // Asegurar que haya un EventSystem para que los botones respondan al clic
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        BuildAllMenus();
    }

    private void BuildAllMenus()
    {
        // 1. Canvas principal
        GameObject canvasObj = new GameObject("Menu_Canvas", typeof(RectTransform));
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Fondo
        CreateBackground(canvasObj);

        // 3. Paneles de cada interfaz
        menuPrincipalPanel = CreateMainMenu(canvasObj);
        nuevaPartidaPanel = CreateNuevaPartidaMenu(canvasObj);
        ajustesPanel = CreateAjustesMenu(canvasObj);

        // 4. Cortina de transición a negro (por encima de los paneles)
        CreateFadeOverlay(canvasObj);

        // Mostrar solo el menú principal al inicio
        ShowPanel(menuPrincipalPanel);
    }

    private void CreateBackground(GameObject parent)
    {
        GameObject bgObj = new GameObject("Background", typeof(RectTransform));
        bgObj.transform.SetParent(parent.transform, false);
        SetFullScreenRect(bgObj);

        RawImage img = bgObj.AddComponent<RawImage>();
        if (fondoTexture == null)
        {
            fondoTexture = Resources.Load<Texture2D>("fondo_menu");
        }
        if (fondoTexture != null)
        {
            img.texture = fondoTexture;
        }
        img.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        // Viñeta oscura periférica
        GameObject vignette = new GameObject("Vignette", typeof(RectTransform));
        vignette.transform.SetParent(parent.transform, false);
        SetFullScreenRect(vignette);

        Image vImg = vignette.AddComponent<Image>();
        vImg.color = new Color(0f, 0f, 0f, 0.35f);
        vImg.raycastTarget = false;
    }

    #region MENÚ PRINCIPAL
    private GameObject CreateMainMenu(GameObject parent)
    {
        GameObject panel = new GameObject("Panel_MenuPrincipal", typeof(RectTransform));
        panel.transform.SetParent(parent.transform, false);
        SetFullScreenRect(panel);

        // Contenedor a la izquierda (como en menu.html: w-1/3 ml-margin-safe)
        GameObject container = new GameObject("Buttons_Container", typeof(RectTransform));
        container.transform.SetParent(panel.transform, false);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.06f, 0.5f);
        rt.anchorMax = new Vector2(0.06f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 460);

        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 22f;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;

        // Botones
        CreateMenuButton(container, "CONTINUAR", () => StartCoroutine(PlayGameTransition()));
        CreateMenuButton(container, "NUEVA PARTIDA", () => ShowPanel(nuevaPartidaPanel));
        CreateMenuButton(container, "AJUSTES", () => ShowPanel(ajustesPanel));
        CreateMenuButton(container, "SALIR", () => Application.Quit());

        return panel;
    }
    #endregion

    #region NUEVA PARTIDA
    private GameObject CreateNuevaPartidaMenu(GameObject parent)
    {
        GameObject panel = new GameObject("Panel_NuevaPartida", typeof(RectTransform));
        panel.transform.SetParent(parent.transform, false);
        SetFullScreenRect(panel);

        GameObject container = new GameObject("Container", typeof(RectTransform));
        container.transform.SetParent(panel.transform, false);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.06f, 0.5f);
        rt.anchorMax = new Vector2(0.06f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 480);

        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 18f;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;

        CreateMenuButton(container, "FACIL", () => Debug.Log("[Dificultad] Fácil seleccionada."));
        CreateMenuButton(container, "NORMAL", () => Debug.Log("[Dificultad] Normal seleccionada."));
        CreateMenuButton(container, "PESADILLA", () => Debug.Log("[Dificultad] Pesadilla seleccionada."));
        CreateMenuButton(container, "INICIAR PARTIDA", () => StartCoroutine(PlayGameTransition()));
        CreateMenuButton(container, "VOLVER AL MENU", () => ShowPanel(menuPrincipalPanel));

        return panel;
    }
    #endregion

    #region AJUSTES
    private GameObject CreateAjustesMenu(GameObject parent)
    {
        GameObject panel = new GameObject("Panel_Ajustes", typeof(RectTransform));
        panel.transform.SetParent(parent.transform, false);
        SetFullScreenRect(panel);

        GameObject container = new GameObject("Container", typeof(RectTransform));
        container.transform.SetParent(panel.transform, false);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.06f, 0.5f);
        rt.anchorMax = new Vector2(0.06f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 480);

        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 18f;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;

        CreateMenuButton(container, "AUDIO", () => Debug.Log("[Ajustes] Audio"));
        CreateMenuButton(container, "GRAFICOS", () => Debug.Log("[Ajustes] Gráficos"));
        CreateMenuButton(container, "CONTROLES", () => Debug.Log("[Ajustes] Controles"));
        CreateMenuButton(container, "VOLVER AL MENU", () => ShowPanel(menuPrincipalPanel));

        return panel;
    }
    #endregion

    #region BOTÓN PERSONALIZADO CON GLITCH
    private void CreateMenuButton(GameObject parent, string text, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject btnObj = new GameObject("Btn_" + text, typeof(RectTransform));
        btnObj.transform.SetParent(parent.transform, false);

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.minHeight = 62f;
        le.preferredHeight = 62f;

        Image bg = btnObj.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.08f, 0.08f, 0.88f); // tertiary-container oscuro

        // Borde fino
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 0.38f, 0.38f, 0.9f);
        outline.effectDistance = new Vector2(1, -1);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.05f, 0.08f, 0.08f, 0.88f);
        cb.highlightedColor = new Color(0.48f, 0.08f, 0.05f, 0.95f); // hover rojizo
        cb.pressedColor = new Color(0.65f, 0.05f, 0.05f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(onClickAction);

        // Texto con efecto Glitch
        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(btnObj.transform, false);
        SetFullScreenRect(txtObj);

        Text txt = txtObj.AddComponent<Text>();
        txt.text = text;
        Font runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (runtimeFont == null) runtimeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.font = runtimeFont;
        txt.fontSize = 22;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.85f, 0.84f, 0.83f, 1f);

        // Agregar script de Glitch de texto idéntico al HTML
        GlitchText glitch = txtObj.AddComponent<GlitchText>();
        glitch.originalText = text;

        // Cuadradito decorativo a la derecha (como en tu HTML)
        GameObject dot = new GameObject("Dot", typeof(RectTransform));
        dot.transform.SetParent(btnObj.transform, false);
        RectTransform dotRt = dot.GetComponent<RectTransform>();
        dotRt.anchorMin = new Vector2(0.95f, 0.5f);
        dotRt.anchorMax = new Vector2(0.95f, 0.5f);
        dotRt.pivot = new Vector2(0.5f, 0.5f);
        dotRt.anchoredPosition = Vector2.zero;
        dotRt.sizeDelta = new Vector2(8, 8);
        Image dotImg = dot.AddComponent<Image>();
        dotImg.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
    }
    #endregion

    #region TRANSICIÓN Y NAVEGACIÓN
    private void CreateFadeOverlay(GameObject parent)
    {
        GameObject fadeObj = new GameObject("BlackFade", typeof(RectTransform));
        fadeObj.transform.SetParent(parent.transform, false);
        SetFullScreenRect(fadeObj);

        blackFadeImage = fadeObj.AddComponent<Image>();
        blackFadeImage.color = new Color(0f, 0f, 0f, 0f);
        blackFadeImage.raycastTarget = false;
    }

    public void ShowPanel(GameObject panelToShow)
    {
        if (menuPrincipalPanel != null) menuPrincipalPanel.SetActive(panelToShow == menuPrincipalPanel);
        if (nuevaPartidaPanel != null) nuevaPartidaPanel.SetActive(panelToShow == nuevaPartidaPanel);
        if (ajustesPanel != null) ajustesPanel.SetActive(panelToShow == ajustesPanel);
    }

    private IEnumerator PlayGameTransition()
    {
        blackFadeImage.raycastTarget = true;
        float elapsed = 0f;
        float fadeDuration = 1.0f;

        // Fundido a negro progresivo
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            blackFadeImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }
        blackFadeImage.color = Color.black;

        // 2 segundos de pantalla en negro y silencio
        yield return new WaitForSeconds(2.0f);

        // Si la escena activa no es la del juego, la carga
        if (SceneManager.GetActiveScene().name != gameSceneName && Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            // Si el menú está en la misma escena del juego, ocultar todo el menú
            if (menuPrincipalPanel != null) menuPrincipalPanel.SetActive(false);
            if (nuevaPartidaPanel != null) nuevaPartidaPanel.SetActive(false);
            if (ajustesPanel != null) ajustesPanel.SetActive(false);

            Transform bg = transform.Find("Menu_Canvas/Background");
            if (bg != null) bg.gameObject.SetActive(false);
            Transform vignette = transform.Find("Menu_Canvas/Vignette");
            if (vignette != null) vignette.gameObject.SetActive(false);

            // Transición de salida: disolver la oscuridad de 2 segundos para revelar el hospital
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                blackFadeImage.color = new Color(0f, 0f, 0f, 1f - Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
            blackFadeImage.color = new Color(0, 0, 0, 0);
            blackFadeImage.raycastTarget = false;

            // Destruir el Canvas del menú para que no consuma recursos
            GameObject menuCanvas = GameObject.Find("Menu_Canvas");
            if (menuCanvas != null) Destroy(menuCanvas);
        }
    }

    private void SetFullScreenRect(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }
    #endregion
}

/// <summary>
/// Simula el efecto de glitch cibernético/psicológico de los textos en el HTML original
/// </summary>
public class GlitchText : MonoBehaviour
{
    public string originalText;
    private Text textComponent;
    private float nextGlitchTime = 0f;
    private static readonly char[] GlitchChars = { '#', '%', '&', '/', '<', '>', '$', '!', '?' };

    void Start()
    {
        textComponent = GetComponent<Text>();
        if (string.IsNullOrEmpty(originalText) && textComponent != null)
        {
            originalText = textComponent.text;
        }
    }

    void Update()
    {
        if (textComponent == null || string.IsNullOrEmpty(originalText)) return;

        if (Time.time >= nextGlitchTime)
        {
            // Ocasionalmente genera un destello de glitch
            if (Random.value > 0.65f)
            {
                StartCoroutine(GlitchRoutine());
            }
            nextGlitchTime = Time.time + Random.Range(1.5f, 4.0f);
        }
    }

    private IEnumerator GlitchRoutine()
    {
        char[] chars = originalText.ToCharArray();
        int corruptCount = Random.Range(1, Mathf.Max(2, originalText.Length / 3));

        for (int i = 0; i < corruptCount; i++)
        {
            int idx = Random.Range(0, chars.Length);
            if (chars[idx] != ' ')
            {
                chars[idx] = GlitchChars[Random.Range(0, GlitchChars.Length)];
            }
        }

        textComponent.text = new string(chars);
        textComponent.color = new Color(1f, 0.4f, 0.4f, 1f); // Tinte rojizo glitch

        yield return new WaitForSeconds(Random.Range(0.04f, 0.09f));

        textComponent.text = originalText;
        textComponent.color = new Color(0.85f, 0.84f, 0.83f, 1f);
    }
}
