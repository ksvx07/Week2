using UnityEngine;

public class CircleSpining : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] CircleController controller;

    [Range(0.1f, 10.0f)]
    [Tooltip("한바퀴 도는데 걸리는 최저 시간")] [SerializeField] float rotationTimeInMinSpeed = 1.0f;
    [Range(0.1f, 10.0f)]
    [Tooltip("MaxSpeed에서 한바퀴 도는데 걸리는 시간")] [SerializeField] float rotationTimeInMaxSpeed = 1.0f;

    private float currentRotationTime;
    private float maxSpeed;

    private void Start()
    {
        maxSpeed= controller.MaxSpeed;
    }

    void Update()
    {
        if (rb == null || rb.linearVelocity.magnitude < 0.1f)
        {
            return;
        }

        if (rb.linearVelocityX > 0)
        {
            // 시계 방향으로 회전합니다. (z축 회전)
            transform.Rotate(0, 0, -GetCurrentRotationSpeed() * Time.deltaTime);
        }

        if (rb.linearVelocityX < 0)
        {
            // 반시계 방향으로 회전합니다.
            transform.Rotate(0, 0, GetCurrentRotationSpeed() * Time.deltaTime);
        }
    }

    private float GetCurrentRotationSpeed()
    {
        float speedRatio = Mathf.Clamp01(Mathf.Abs(rb.linearVelocityX) / maxSpeed);

        return Mathf.Lerp(360f/rotationTimeInMinSpeed, 360f/ rotationTimeInMaxSpeed, speedRatio);
    }

}
