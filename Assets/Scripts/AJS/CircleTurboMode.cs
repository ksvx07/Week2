using UnityEngine;

public class CircleTurboMode : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] CircleController controller;

    private TrailRenderer trail;
    private float maxSpeed;

    private void Start()
    {
        trail = GetComponent<TrailRenderer>();
        maxSpeed = controller.MaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (maxSpeed -rb.linearVelocity.magnitude < 0.2f)
        {
            TurboModeActive();
        }
        else
        {
            trail.emitting = false;
        }
    }

    private void TurboModeActive()
    {
        trail.emitting = true;
        print("Sonic");
    }
}
