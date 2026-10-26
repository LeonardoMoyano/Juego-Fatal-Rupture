using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform objetivo;
    public Vector3 desplazamiento;

    [Header("Suavizado")]
    public float velocidadCamara = 8f; // ahora es independiente del framerate

    [Header("Zoom")]
    public float zoom = 3.5f;
    public float velocidadZoom = 5f;

    [Header("Limites del mapa (en unidades de mundo)")]
    [Tooltip("Activar para que la cámara no salga del mapa")]
    public bool usarLimites = true;
    [Tooltip("Borde izquierdo del mapa")]
    public float limiteIzquierdo = -10f;
    [Tooltip("Borde derecho del mapa")]
    public float limiteDerecho  =  10f;
    [Tooltip("Borde inferior del mapa")]
    public float limiteAbajo    = -10f;
    [Tooltip("Borde superior del mapa")]
    public float limiteArriba   =  10f;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (objetivo == null) return;

        // Posición deseada siguiendo al jugador
        Vector3 posicionDeseada = objetivo.position + desplazamiento;

        // Zoom suavizado
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoom, velocidadZoom * Time.deltaTime);

        // Calcular cuánto ocupa la cámara en el mundo
        float altCam = cam.orthographicSize;
        float anchCam = altCam * cam.aspect;

        // Clamp de posición dentro de los límites del mapa
        if (usarLimites)
        {
            posicionDeseada.x = Mathf.Clamp(posicionDeseada.x, limiteIzquierdo + anchCam, limiteDerecho  - anchCam);
            posicionDeseada.y = Mathf.Clamp(posicionDeseada.y, limiteAbajo    + altCam,  limiteArriba   - altCam);
        }

        // Mantener la Z de la cámara (importante en 2D)
        posicionDeseada.z = transform.position.z;

        // Suavizado independiente del framerate
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, velocidadCamara * Time.deltaTime);
    }
}
