using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crea automáticamente una pantalla de muerte estilo 'YOU ARE DEAD'
/// con botón de reintentar para reaparecer al jugador.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    private Canvas canvas;
    private GameObject deathPanel;
    private Text deathText;
    private Button retryButton;
    private CanvasGroup canvasGroup;
    private Vector3 initialPlayerPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Guardar la posición inicial del jugador como punto de reaparición
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            initialPlayerPosition = player.transform.position;
        }

        BuildUI();
    }

    private void BuildUI()
    {
        // 1. Crear Canvas si no existe
        GameObject canvasObj = new GameObject("GameOver_Canvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Por encima de todo

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Panel oscuro de fondo
        deathPanel = new GameObject("DeathPanel");
        deathPanel.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRect = deathPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImg = deathPanel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.01f, 0.01f, 0.88f); // Rojo muy oscuro y tenebroso

        canvasGroup = deathPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        deathPanel.SetActive(false);

        // 3. Texto: YOU ARE DEAD
        GameObject textObj = new GameObject("Text_YouAreDead");
        textObj.transform.SetParent(deathPanel.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.6f);
        textRect.anchorMax = new Vector2(0.5f, 0.6f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(900, 150);

        deathText = textObj.AddComponent<Text>();
        deathText.text = "YOU ARE DEAD";
        deathText.fontSize = 72;
        deathText.alignment = TextAnchor.MiddleCenter;
        deathText.color = new Color(0.9f, 0.1f, 0.1f, 1f); // Rojo sangre
        deathText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // 4. Botón REINTENTAR
        GameObject btnObj = new GameObject("Btn_Retry");
        btnObj.transform.SetParent(deathPanel.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.38f);
        btnRect.anchorMax = new Vector2(0.5f, 0.38f);
        btnRect.anchoredPosition = Vector2.zero;
        btnRect.sizeDelta = new Vector2(280, 65);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.02f, 0.02f, 0.95f);

        retryButton = btnObj.AddComponent<Button>();
        ColorBlock colors = retryButton.colors;
        colors.normalColor = new Color(0.18f, 0.02f, 0.02f, 0.95f);
        colors.highlightedColor = new Color(0.6f, 0.05f, 0.05f, 1f);
        colors.pressedColor = new Color(0.35f, 0.02f, 0.02f, 1f);
        retryButton.colors = colors;
        retryButton.onClick.AddListener(OnRetryClicked);

        // Texto del botón
        GameObject btnTextObj = new GameObject("Text_Btn");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.text = "REINTENTAR";
        btnText.fontSize = 28;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    /// <summary>
    /// Muestra la pantalla de muerte
    /// </summary>
    public void ShowDeathScreen()
    {
        if (deathPanel == null) return;
        deathPanel.SetActive(true);
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;
        float duration = 1.0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Reaparece al jugador en la posición guardada y restaura su vida
    /// </summary>
    public void OnRetryClicked()
    {
        deathPanel.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = initialPlayerPosition;
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.currentHealth = health.maxHealth;
            }

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = true;

            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            if (attack != null) attack.enabled = true;

            // Restaurar color si quedó teñido de rojo
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
        }

        Debug.Log("<color=green>[PJ] Jugador reaparecido en el punto de guardado inicial.</color>");
    }
}
