using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 direccion = mousePos - transform.position;

        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        angulo -= 90f;

        transform.rotation = Quaternion.Euler(0f, 0f, angulo);
    }
}