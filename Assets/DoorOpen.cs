using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] private GameObject door;
    private bool opened;

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger Entered");
        if (collision.CompareTag("Player"))
        {
            if (opened) return;
            opened = true;
            Destroy(door);
            Destroy(gameObject);
        }
    }
}
