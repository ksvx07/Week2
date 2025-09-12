using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            KirbyTurboMode turboMode = collision.GetComponent<KirbyTurboMode>();
            if(turboMode != null)
            {
                if (turboMode.TurboMode)
                {
                    TurbomodeDestoy();
                }
            }
        }
    }

    public void TurbomodeDestoy()
    {
        gameObject.SetActive(false);
    }
}
