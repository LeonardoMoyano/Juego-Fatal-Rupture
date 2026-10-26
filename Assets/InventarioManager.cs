using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona el inventario del jugador y la UI de notificaciones.
/// Attach this script to a persistent GameObject (e.g., GameManager).
/// Requiere un Canvas con el prefab de notificacion asignado.
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

    // Diccionario interno: nombre del item -> cantidad
    private Dictionary<string, int> inventario = new Dictionary<string, int>();

    // Cola de notificaciones activas para evitar spam visual
    private List<GameObject> notificacionesActivas = new List<GameObject>();
    private const int MAX_NOTIFICACIONES = 4;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Agrega un item al inventario y muestra la notificacion en pantalla.
    /// </summary>
    /// <param name="nombreItem">Nombre del item recogido</param>
    /// <param name="cantidad">Cantidad recogida (por defecto 1)</param>
    public void AgregarItem(string nombreItem, int cantidad = 1)
    {
        if (string.IsNullOrEmpty(nombreItem)) return;

        // Actualizar inventario
        if (inventario.ContainsKey(nombreItem))
            inventario[nombreItem] += cantidad;
        else
            inventario[nombreItem] = cantidad;

        Debug.Log($"[Inventario] {nombreItem} x{cantidad} | Total: {inventario[nombreItem]}");

        // Mostrar notificacion en UI
        MostrarNotificacion(nombreItem, inventario[nombreItem]);
    }

    /// <summary>
    /// Retorna la cantidad de un item en el inventario (0 si no existe).
    /// </summary>
    public int ObtenerCantidad(string nombreItem)
    {
        return inventario.ContainsKey(nombreItem) ? inventario[nombreItem] : 0;
    }

    /// <summary>
    /// Retorna una copia del inventario completo.
    /// </summary>
    public Dictionary<string, int> ObtenerInventarioCompleto()
    {
        return new Dictionary<string, int>(inventario);
    }

    // --- UI Interna ---

    private void MostrarNotificacion(string nombreItem, int cantidadTotal)
    {
        if (notificacionPrefab == null || notificacionesContainer == null)
        {
            Debug.LogWarning("[InventarioManager] Faltan referencias de UI. Asigna 'notificacionPrefab' y 'notificacionesContainer'.");
            return;
        }

        // Limitar cantidad de notificaciones visibles
        if (notificacionesActivas.Count >= MAX_NOTIFICACIONES)
        {
            GameObject primera = notificacionesActivas[0];
            notificacionesActivas.RemoveAt(0);
            Destroy(primera);
        }

        // Instanciar notificacion
        GameObject notif = Instantiate(notificacionPrefab, notificacionesContainer);
        notificacionesActivas.Add(notif);

        // Configurar contenido
        ItemNotificacionUI ui = notif.GetComponent<ItemNotificacionUI>();
        if (ui != null)
            ui.Configurar(nombreItem, cantidadTotal);

        // Auto-destruir despues del tiempo configurado
        StartCoroutine(DestruirNotificacion(notif, duracionNotificacion));
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
