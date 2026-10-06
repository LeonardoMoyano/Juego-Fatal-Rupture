using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 2f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    void Interact()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

        foreach (Collider2D hit in hits)
        {
            Door door = hit.GetComponent<Door>();

            if (door != null)
            {
                door.ToggleDoor();
                return;
            }
        }
    }
}