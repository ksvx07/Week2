using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            KirbyController turboMode = collision.GetComponent<KirbyController>();
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
