using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen = false;

    private BoxCollider2D col;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        UpdateDoor();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        UpdateDoor();
    }

    void UpdateDoor()
    {
        if (isOpen)
        {
            col.isTrigger = true;
        }
        else
        {
            col.isTrigger = false;
        }
    }
}