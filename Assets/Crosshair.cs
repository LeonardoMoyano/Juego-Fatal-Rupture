using UnityEngine;

public class NewMonoBehaviourScript1 : MonoBehaviour
{
    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        Cursor.visible = false;
    }

    void Update()
    {
        rect.position = Input.mousePosition;
    }
}