using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel de Inventario con cuadrícula 3x3 que se abre/cierra con la tecla [TAB].
/// - Grilla 3x3 de slots (9 casillas).
/// - Al pasar el mouse por encima muestra un Tooltip con el nombre.
/// - Al hacer clic en un ítem abre un menú contextual con: "Info", "Usar" y "Desechar".
/// - Funciona automáticamente sin necesidad de configurar prefabs complicados en el Canvas.
/// </summary>
public class InventarioUI : MonoBehaviour
{
    [Header("Configuración")]
    public KeyCode toggleKey = KeyCode.Tab;

    private GameObject panelPrincipal;
    private bool estaAbierto = false;

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

    void Start()
    {
        ConstruirUIProcedural();
        CerrarInventario();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (estaAbierto) CerrarInventario();
            else AbrirInventario();
        }

        if (estaAbierto && tooltip != null && tooltip.activeSelf)
        {
            tooltip.transform.position = Input.mousePosition + new Vector3(15f, -15f, 0f);
        }
    }

    public void AbrirInventario()
    {
        estaAbierto = true;
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
        Cursor.visible = true;
        ActualizarGrilla();
        if (menuContextual != null) menuContextual.SetActive(false);
    }

    public void CerrarInventario()
    {
        estaAbierto = false;
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (menuContextual != null) menuContextual.SetActive(false);
        if (tooltip != null) tooltip.SetActive(false);
        Cursor.visible = false;
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
                slotIconos[i].color = new Color(0.2f, 0.6f, 0.95f, 0.95f); // Cuadrado azul como pediste
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
        // Agregar -1 cantidad
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

    // --- Construcción Procedural de la UI ---

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

        // Panel Principal
        panelPrincipal = new GameObject("Panel_Inventario_3x3");
        panelPrincipal.transform.SetParent(canvas.transform, false);

        RectTransform rt = panelPrincipal.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(360f, 400f);
        rt.anchoredPosition = Vector2.zero;

        Image bg = panelPrincipal.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.06f, 0.08f, 0.95f); // Ventana oscura

        // Título "INVENTARIO (TAB)"
        GameObject titleGO = new GameObject("Titulo");
        titleGO.transform.SetParent(panelPrincipal.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchoredPosition = new Vector2(0f, 160f);
        titleRT.sizeDelta = new Vector2(300f, 35f);
        Text titleTxt = titleGO.AddComponent<Text>();
        titleTxt.text = "INVENTARIO [TAB]";
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.fontSize = 20;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = Color.white;

        // Grilla 3x3
        GameObject gridGO = new GameObject("Grilla");
        gridGO.transform.SetParent(panelPrincipal.transform, false);
        RectTransform gridRT = gridGO.AddComponent<RectTransform>();
        gridRT.anchoredPosition = new Vector2(0f, 15f);
        gridRT.sizeDelta = new Vector2(270f, 270f);

        GridLayoutGroup grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(80f, 80f);
        grid.spacing = new Vector2(10f, 10f);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < 9; i++)
        {
            int slotIdx = i;
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(gridGO.transform, false);
            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            Button btn = slot.AddComponent<Button>();
            btn.onClick.AddListener(() => AlHacerClicEnSlot(slotIdx));

            // Detector de mouse sobre el slot
            SlotHoverTrigger hover = slot.AddComponent<SlotHoverTrigger>();
            hover.Init(slotIdx, this);

            // Icono del ítem (cuadrado azul)
            GameObject itemIconGO = new GameObject("Icono");
            itemIconGO.transform.SetParent(slot.transform, false);
            RectTransform iconRT = itemIconGO.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(65f, 65f);
            slotIconos[i] = itemIconGO.AddComponent<Image>();
            slotIconos[i].color = new Color(0.2f, 0.6f, 0.95f, 0.95f);
            itemIconGO.SetActive(false);

            // Texto de cantidad
            GameObject cantGO = new GameObject("Cantidad");
            cantGO.transform.SetParent(slot.transform, false);
            RectTransform cantRT = cantGO.AddComponent<RectTransform>();
            cantRT.anchorMin = new Vector2(0.5f, 0f);
            cantRT.anchorMax = new Vector2(1f, 0.35f);
            cantRT.offsetMin = Vector2.zero;
            cantRT.offsetMax = Vector2.zero;
            slotTextos[i] = cantGO.AddComponent<Text>();
            slotTextos[i].font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            slotTextos[i].fontSize = 14;
            slotTextos[i].alignment = TextAnchor.LowerRight;
            slotTextos[i].color = Color.white;
        }

        // Menú Contextual ("Info", "Usar", "Desechar")
        ConstruirMenuContextual(canvas);

        // Tooltip
        ConstruirTooltip(canvas);
    }

    private void ConstruirMenuContextual(Canvas canvas)
    {
        menuContextual = new GameObject("Menu_Contextual");
        menuContextual.transform.SetParent(canvas.transform, false);

        RectTransform rt = menuContextual.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180f, 150f);
        Image bg = menuContextual.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.04f, 0.06f, 0.98f);

        VerticalLayoutGroup vl = menuContextual.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(10, 10, 8, 8);
        vl.spacing = 6;
        vl.childControlWidth = true;
        vl.childControlHeight = false;

        CrearBotonMenu("INFO", AccionInfo);
        CrearBotonMenu("USAR", AccionUsar);
        CrearBotonMenu("DESECHAR", AccionDesechar);

        // Texto informativo abajo del menú
        GameObject infoGO = new GameObject("TextoInfo");
        infoGO.transform.SetParent(menuContextual.transform, false);
        textoInfo = infoGO.AddComponent<Text>();
        textoInfo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoInfo.fontSize = 11;
        textoInfo.color = new Color(0.9f, 0.85f, 0.5f, 1f);
        textoInfo.alignment = TextAnchor.MiddleCenter;

        menuContextual.SetActive(false);
    }

    private void CrearBotonMenu(string texto, UnityEngine.Events.UnityAction accion)
    {
        GameObject bgo = new GameObject($"Btn_{texto}");
        bgo.transform.SetParent(menuContextual.transform, false);
        RectTransform rt = bgo.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150f, 26f);

        Image img = bgo.AddComponent<Image>();
        img.color = new Color(0.2f, 0.22f, 0.28f, 1f);

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
        t.fontSize = 13;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
    }

    private void ConstruirTooltip(Canvas canvas)
    {
        tooltip = new GameObject("Tooltip_Item");
        tooltip.transform.SetParent(canvas.transform, false);

        RectTransform rt = tooltip.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(130f, 30f);
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
        tooltipTexto.fontSize = 13;
        tooltipTexto.alignment = TextAnchor.MiddleCenter;
        tooltipTexto.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        tooltip.SetActive(false);
    }
}

/// <summary>
/// Componente auxiliar para detectar el hover del mouse sobre cada slot.
/// </summary>
public class SlotHoverTrigger : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    private int slotIndex;
    private InventarioUI inventarioUI;

    public void Init(int index, InventarioUI ui)
    {
        slotIndex = index;
        inventarioUI = ui;
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (inventarioUI != null) inventarioUI.AlPasarMouseSobreSlot(slotIndex, true);
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (inventarioUI != null) inventarioUI.AlPasarMouseSobreSlot(slotIndex, false);
    }
}
