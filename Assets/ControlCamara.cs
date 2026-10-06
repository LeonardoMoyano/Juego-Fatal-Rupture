using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform objetivo;
    public float velocidadCamara = 0.025f;
    public Vector3 desplazamiento;

    public float zoom = 3.5f; // valor base
    public float velocidadZoom = 5f;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        Vector3 posicionDeseada = objetivo.position + desplazamiento;
        Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, velocidadCamara);
        transform.position = posicionSuavizada;

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoom, velocidadZoom * Time.deltaTime);
    }
}