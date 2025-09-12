using UnityEngine;

public class CircleSpining : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] KirbyController kirbyController;

    [Range(0.1f, 10.0f)]
    [Tooltip("�ѹ��� ���µ� �ɸ��� ���� �ð�")] [SerializeField] float rotationTimeInMinSpeed = 1.0f;
    [Range(0.1f, 10.0f)]
    [Tooltip("MaxSpeed���� �ѹ��� ���µ� �ɸ��� �ð�")] [SerializeField] float rotationTimeInMaxSpeed = 1.0f;
    [Range(0.1f, 10.0f)]
    [Tooltip("TurboSpeed���� �ѹ��� ���µ� �ɸ��� �ð�")] [SerializeField] float rotationTimeInTurboSpeed = 1.0f;

    void Update()
    {
        // �ӵ��� 0�� �����ϸ� ����
        if (rb.linearVelocity.magnitude < 0.2f)
        {
            return;
        }

        transform.Rotate(0, 0, -GetCurrentRotationSpeed() * Time.deltaTime);
    }

    private float GetCurrentRotationSpeed()
    {
        if (kirbyController.TurboMode)
        {
            // �ͺ������ ���� �ͺ����� ȸ������
            return 360 / rotationTimeInTurboSpeed;
        }
        float speedRatio = Mathf.Clamp01(Mathf.Abs(rb.linearVelocityX) / kirbyController.MaxSpeed);
        return Mathf.Lerp(360f/rotationTimeInMinSpeed, 360f/ rotationTimeInMaxSpeed, speedRatio);
    }

}
