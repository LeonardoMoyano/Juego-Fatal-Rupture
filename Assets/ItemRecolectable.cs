using UnityEngine;

/// <summary>
/// Script que se coloca en cada objeto del suelo que puede ser recogido.
/// Al entrar en contacto con el jugador (o al presionar E cerca), 
/// el item se agrega automaticamente al inventario.
/// 
/// MODO DE USO:
///   1. Agrega este script al GameObject del item en la escena.
///   2. Asegúrate de que el item tiene un Collider2D (puede ser Trigger).
///   3. Configura los campos en el Inspector.
///
/// MODO DE RECOLECCION:
///   - AutoRecolectar = true  → al tocar al jugador lo levanta automaticamente.
///   - AutoRecolectar = false → el jugador debe presionar [E] cerca del objeto.
/// </summary>
public class ItemRecolectable : MonoBehaviour
{
    [Header("Datos del Item")]
    [Tooltip("Nombre del item que aparecera en el inventario")]
    public string nombreItem = "Item";

    [Tooltip("Cantidad que se agrega al inventario al recogerlo")]
    public int cantidad = 1;

    [Tooltip("Icono del item (opcional, para la notificacion UI)")]
    public Sprite iconoItem;

    [Header("Modo de Recoleccion")]
    [Tooltip("True: se recoge al tocar al jugador. False: requiere presionar E.")]
    public bool autoRecolectar = true;

    [Tooltip("Rango de interaccion cuando autoRecolectar = false (en unidades Unity)")]
    public float rangoInteraccion = 1.5f;

    [Header("Efectos (Opcionales)")]
    [Tooltip("Sonido que se reproduce al recoger el item")]
    public AudioClip sonidoRecoleccion;

    // Tag que debe tener el jugador en Unity
    private const string TAG_JUGADOR = "Player";

    private bool yaRecogido = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Modo manual: el jugador presiona E
        if (!autoRecolectar && Input.GetKeyDown(KeyCode.E))
        {
            VerificarRecoleccionManual();
        }
    }

    // --- Modo automatico: trigger o colision ---

    void OnTriggerEnter2D(Collider2D other)
    {
        if (autoRecolectar && other.CompareTag(TAG_JUGADOR))
            Recolectar();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (autoRecolectar && collision.gameObject.CompareTag(TAG_JUGADOR))
            Recolectar();
    }

    // --- Modo manual ---

    private void VerificarRecoleccionManual()
    {
        // Busca al jugador por tag en un radio alrededor del item
        Collider2D jugador = Physics2D.OverlapCircle(
            transform.position,
            rangoInteraccion,
            LayerMask.GetMask("Default") // Ajusta si el jugador esta en otra Layer
        );

        if (jugador != null && jugador.CompareTag(TAG_JUGADOR))
            Recolectar();
    }

    // --- Logica de recoleccion ---

    private void Recolectar()
    {
        if (yaRecogido) return;
        yaRecogido = true;

        // Agregar al inventario
        if (InventarioManager.Instance != null)
            InventarioManager.Instance.AgregarItem(nombreItem, cantidad);
        else
            Debug.LogWarning("[ItemRecolectable] No se encontro InventarioManager en la escena.");

        // Reproducir sonido si hay uno asignado
        if (sonidoRecoleccion != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoRecoleccion);
            // Destruir despues de que termine el sonido
            Destroy(gameObject, sonidoRecoleccion.length + 0.1f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Gizmo para visualizar el rango en el Editor ---
    void OnDrawGizmosSelected()
    {
        if (!autoRecolectar)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, rangoInteraccion);
        }
    }
}
