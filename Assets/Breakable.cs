using UnityEngine;

public class Breakable : MonoBehaviour
{
    public GameObject me;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
        Debug.Log("adsjfkldasf");
        
    }
}
