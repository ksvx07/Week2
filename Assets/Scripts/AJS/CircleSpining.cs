using UnityEngine;

public class CircleSpining : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] CircleController controller;

    [Range(0.1f, 10.0f)]
    [Tooltip("�ѹ��� ���µ� �ɸ��� ���� �ð�")] [SerializeField] float rotationTimeInMinSpeed = 1.0f;
    [Range(0.1f, 10.0f)]
    [Tooltip("MaxSpeed���� �ѹ��� ���µ� �ɸ��� �ð�")] [SerializeField] float rotationTimeInMaxSpeed = 1.0f;

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
            // �ð� �������� ȸ���մϴ�. (z�� ȸ��)
            transform.Rotate(0, 0, -GetCurrentRotationSpeed() * Time.deltaTime);
        }

        if (rb.linearVelocityX < 0)
        {
            // �ݽð� �������� ȸ���մϴ�.
            transform.Rotate(0, 0, GetCurrentRotationSpeed() * Time.deltaTime);
        }
    }

    private float GetCurrentRotationSpeed()
    {
        float speedRatio = Mathf.Clamp01(Mathf.Abs(rb.linearVelocityX) / maxSpeed);

        return Mathf.Lerp(360f/rotationTimeInMinSpeed, 360f/ rotationTimeInMaxSpeed, speedRatio);
    }

}
