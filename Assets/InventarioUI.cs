using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel de Inventario con cuadrícula 3x3 que se abre/cierra con la tecla [TAB].
/// - Grilla 3x3 de slots (9 casillas) con tamaño y letras ampliadas.
/// - Pausa el movimiento, apuntado y ataque del jugador mientras está abierto.
/// - Al pasar el mouse por encima muestra un Tooltip claro con el nombre.
/// - Al hacer clic en un ítem abre un menú contextual con: "Info", "Usar" y "Desechar".
/// </summary>
public class InventarioUI : MonoBehaviour
{
    [Header("Configuración")]
    public KeyCode toggleKey = KeyCode.Tab;

    public static bool EstaAbierto { get; private set; } = false;

    private GameObject panelPrincipal;

    // Componentes de la grilla
    private Image[] slotIconos = new Image[9];
    private Text[] slotTextos = new Text[9];
    private string[] slotItems = new string[9];

    // Menú contextual ("Info", "Usar", "Desechar")
    private GameObject menuContextual;
    private Text textoInfo;
    private string itemSeleccionado = "";

    // Tooltip flotante al pasar el mouse
    private GameObject tooltip;
    private Text tooltipTexto;

    // Referencias al jugador para pausar control
    private PlayerMovement playerMovement;
    private PlayerAim playerAim;
    private PlayerAttack playerAttack;
    private Rigidbody2D playerRb;

    void Start()
    {
        // Encontrar scripts del jugador
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerMovement = playerGO.GetComponent<PlayerMovement>();
            playerAim = playerGO.GetComponent<PlayerAim>();
            playerAttack = playerGO.GetComponent<PlayerAttack>();
            playerRb = playerGO.GetComponent<Rigidbody2D>();
        }

        ConstruirUIProcedural();
        CerrarInventario();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (EstaAbierto) CerrarInventario();
            else AbrirInventario();
        }

        if (EstaAbierto && tooltip != null && tooltip.activeSelf)
        {
            tooltip.transform.position = Input.mousePosition + new Vector3(20f, -20f, 0f);
        }
    }

    public void AbrirInventario()
    {
        EstaAbierto = true;
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
        Cursor.visible = true;

        // Pausar control del personaje
        SetPlayerControls(false);

        ActualizarGrilla();
        if (menuContextual != null) menuContextual.SetActive(false);
    }

    public void CerrarInventario()
    {
        EstaAbierto = false;
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (menuContextual != null) menuContextual.SetActive(false);
        if (tooltip != null) tooltip.SetActive(false);
        Cursor.visible = false;

        // Reactivar control del personaje
        SetPlayerControls(true);
    }

    private void SetPlayerControls(bool enabled)
    {
        if (playerMovement != null) playerMovement.enabled = enabled;
        if (playerAim != null) playerAim.enabled = enabled;
        if (playerAttack != null) playerAttack.enabled = enabled;
        if (!enabled && playerRb != null) playerRb.linearVelocity = Vector2.zero;
    }

    public void ActualizarGrilla()
    {
        if (InventarioManager.Instance == null) return;

        Dictionary<string, int> inventario = InventarioManager.Instance.ObtenerInventarioCompleto();
        List<string> listaItems = new List<string>(inventario.Keys);

        for (int i = 0; i < 9; i++)
        {
            if (i < listaItems.Count && inventario[listaItems[i]] > 0)
            {
                string nombre = listaItems[i];
                int cant = inventario[nombre];

                slotItems[i] = nombre;
                slotIconos[i].color = new Color(0.2f, 0.6f, 0.95f, 0.95f); // Cuadrado azul
                slotTextos[i].text = cant > 1 ? $"x{cant}" : "";
                slotIconos[i].gameObject.SetActive(true);
            }
            else
            {
                slotItems[i] = "";
                slotIconos[i].gameObject.SetActive(false);
                slotTextos[i].text = "";
            }
        }
    }

    // --- Acciones de los Ítems ---

    public void AlHacerClicEnSlot(int index)
    {
        if (index < 0 || index >= 9) return;
        string nombre = slotItems[index];
        if (string.IsNullOrEmpty(nombre)) return;

        itemSeleccionado = nombre;
        AbrirMenuContextual();
    }

    private void AbrirMenuContextual()
    {
        if (menuContextual == null) return;
        menuContextual.SetActive(true);
        menuContextual.transform.position = Input.mousePosition;
        if (textoInfo != null) textoInfo.text = "";
    }

    public void AccionInfo()
    {
        if (textoInfo == null) return;
        if (itemSeleccionado == "Muleta")
            textoInfo.text = "Muleta hospitalaria de metal. Aumenta tu daño a 40 y te da mayor alcance.";
        else if (itemSeleccionado.Contains("Botiquin") || itemSeleccionado.Contains("Cura"))
            textoInfo.text = "Suministro médico de primeros auxilios. Restaura salud.";
        else
            textoInfo.text = $"Objeto recogido: {itemSeleccionado}.";
    }

    public void AccionUsar()
    {
        if (string.IsNullOrEmpty(itemSeleccionado)) return;

        if (itemSeleccionado == "Muleta")
        {
            if (textoInfo != null) textoInfo.text = "¡Ya estás empuñando la Muleta como arma de ataque!";
        }
        else if (itemSeleccionado.Contains("Botiquin") || itemSeleccionado.Contains("Cura"))
        {
            PlayerHealth ph = Object.FindFirstObjectByType<PlayerHealth>();
            if (ph != null)
            {
                ph.Heal(35f);
                RestarItem(itemSeleccionado);
                if (textoInfo != null) textoInfo.text = "Usaste un botiquín. +35 de Vida.";
            }
        }
        else
        {
            if (textoInfo != null) textoInfo.text = $"No se puede usar {itemSeleccionado} en este momento.";
        }

        ActualizarGrilla();
    }

    public void AccionDesechar()
    {
        if (string.IsNullOrEmpty(itemSeleccionado)) return;

        RestarItem(itemSeleccionado);
        if (textoInfo != null) textoInfo.text = $"Desechaste 1 {itemSeleccionado}.";

        ActualizarGrilla();
        if (menuContextual != null) menuContextual.SetActive(false);
    }

    private void RestarItem(string nombre)
    {
        if (InventarioManager.Instance == null) return;
        InventarioManager.Instance.AgregarItem(nombre, -1);
    }

    public void AlPasarMouseSobreSlot(int index, bool entra)
    {
        if (tooltip == null) return;

        if (entra && index >= 0 && index < 9 && !string.IsNullOrEmpty(slotItems[index]))
        {
            tooltip.SetActive(true);
            tooltipTexto.text = slotItems[index];
        }
        else
        {
            tooltip.SetActive(false);
        }
    }

    // --- Construcción Procedural de la UI (Ampliadas dimensiones y tipografías) ---

    private void ConstruirUIProcedural()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cgo = new GameObject("Canvas_Inventario");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
        }

        // Panel Principal más amplio (500 x 560 px)
        panelPrincipal = new GameObject("Panel_Inventario_3x3");
        panelPrincipal.transform.SetParent(canvas.transform, false);

        RectTransform rt = panelPrincipal.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500f, 560f);
        rt.anchoredPosition = Vector2.zero;

        Image bg = panelPrincipal.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.06f, 0.08f, 0.96f); // Ventana oscura

        // Título "INVENTARIO [TAB]" más grande
        GameObject titleGO = new GameObject("Titulo");
        titleGO.transform.SetParent(panelPrincipal.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchoredPosition = new Vector2(0f, 230f);
        titleRT.sizeDelta = new Vector2(450f, 50f);
        Text titleTxt = titleGO.AddComponent<Text>();
        titleTxt.text = "INVENTARIO [TAB]";
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.fontSize = 28;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = Color.white;

        // Grilla 3x3 ampliada (casillas de 120 x 120 px)
        GameObject gridGO = new GameObject("Grilla");
        gridGO.transform.SetParent(panelPrincipal.transform, false);
        RectTransform gridRT = gridGO.AddComponent<RectTransform>();
        gridRT.anchoredPosition = new Vector2(0f, 15f);
        gridRT.sizeDelta = new Vector2(400f, 400f);

        GridLayoutGroup grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(115f, 115f);
        grid.spacing = new Vector2(16f, 16f);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < 9; i++)
        {
            int slotIdx = i;
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(gridGO.transform, false);
            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0.14f, 0.14f, 0.19f, 0.95f);

            Button btn = slot.AddComponent<Button>();
            btn.onClick.AddListener(() => AlHacerClicEnSlot(slotIdx));

            SlotHoverTrigger hover = slot.AddComponent<SlotHoverTrigger>();
            hover.Init(slotIdx, this);

            // Icono del ítem (cuadrado azul grande)
            GameObject itemIconGO = new GameObject("Icono");
            itemIconGO.transform.SetParent(slot.transform, false);
            RectTransform iconRT = itemIconGO.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(90f, 90f);
            slotIconos[i] = itemIconGO.AddComponent<Image>();
            slotIconos[i].color = new Color(0.2f, 0.6f, 0.95f, 0.95f);
            itemIconGO.SetActive(false);

            // Texto de cantidad ampliado
            GameObject cantGO = new GameObject("Cantidad");
            cantGO.transform.SetParent(slot.transform, false);
            RectTransform cantRT = cantGO.AddComponent<RectTransform>();
            cantRT.anchorMin = new Vector2(0.4f, 0f);
            cantRT.anchorMax = new Vector2(1f, 0.35f);
            cantRT.offsetMin = new Vector2(0f, 4f);
            cantRT.offsetMax = new Vector2(-6f, 0f);
            slotTextos[i] = cantGO.AddComponent<Text>();
            slotTextos[i].font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            slotTextos[i].fontSize = 20;
            slotTextos[i].alignment = TextAnchor.LowerRight;
            slotTextos[i].color = Color.white;
        }

        // Menú Contextual más amplio
        ConstruirMenuContextual(canvas);

        // Tooltip ampliado
        ConstruirTooltip(canvas);
    }

    private void ConstruirMenuContextual(Canvas canvas)
    {
        menuContextual = new GameObject("Menu_Contextual");
        menuContextual.transform.SetParent(canvas.transform, false);

        RectTransform rt = menuContextual.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(240f, 210f);
        Image bg = menuContextual.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.04f, 0.06f, 0.98f);

        VerticalLayoutGroup vl = menuContextual.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(12, 12, 12, 12);
        vl.spacing = 10;
        vl.childControlWidth = true;
        vl.childControlHeight = false;

        CrearBotonMenu("INFO", AccionInfo);
        CrearBotonMenu("USAR", AccionUsar);
        CrearBotonMenu("DESECHAR", AccionDesechar);

        GameObject infoGO = new GameObject("TextoInfo");
        infoGO.transform.SetParent(menuContextual.transform, false);
        textoInfo = infoGO.AddComponent<Text>();
        textoInfo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoInfo.fontSize = 14;
        textoInfo.color = new Color(0.95f, 0.9f, 0.5f, 1f);
        textoInfo.alignment = TextAnchor.MiddleCenter;

        menuContextual.SetActive(false);
    }

    private void CrearBotonMenu(string texto, UnityEngine.Events.UnityAction accion)
    {
        GameObject bgo = new GameObject($"Btn_{texto}");
        bgo.transform.SetParent(menuContextual.transform, false);
        RectTransform rt = bgo.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 36f);

        Image img = bgo.AddComponent<Image>();
        img.color = new Color(0.2f, 0.24f, 0.32f, 1f);

        Button btn = bgo.AddComponent<Button>();
        btn.onClick.AddListener(accion);

        GameObject txtGO = new GameObject("Texto");
        txtGO.transform.SetParent(bgo.transform, false);
        RectTransform trt = txtGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text t = txtGO.AddComponent<Text>();
        t.text = texto;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 17;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
    }

    private void ConstruirTooltip(Canvas canvas)
    {
        tooltip = new GameObject("Tooltip_Item");
        tooltip.transform.SetParent(canvas.transform, false);

        RectTransform rt = tooltip.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(170f, 40f);
        rt.pivot = new Vector2(0f, 1f);

        Image img = tooltip.AddComponent<Image>();
        img.color = new Color(0.02f, 0.02f, 0.04f, 0.95f);

        GameObject txtGO = new GameObject("Texto");
        txtGO.transform.SetParent(tooltip.transform, false);
        RectTransform trt = txtGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        tooltipTexto = txtGO.AddComponent<Text>();
        tooltipTexto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tooltipTexto.fontSize = 16;
        tooltipTexto.alignment = TextAnchor.MiddleCenter;
        tooltipTexto.color = new Color(0.95f, 0.95f, 0.95f, 1f);

        tooltip.SetActive(false);
    }
}
