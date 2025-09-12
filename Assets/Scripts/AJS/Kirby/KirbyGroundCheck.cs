using UnityEngine;

public class KirbyGroundCheck : MonoBehaviour
{
    private bool onGround;

    [Header("Collider Settings")]
    [SerializeField][Tooltip("�� üũ�� Raycast ����")] private float groundLength = 0.95f;
    [SerializeField][Tooltip("�� üũ�� �� Raycast�� �Ÿ�")] private Vector3 colliderOffset;
    [Header("Layer Masks")]
    [SerializeField][Tooltip("�� ���� Layer")] private LayerMask groundLayer;
    private void Update()
    {
        // 2���� RayCaste�� �߻���, Player�� �� ���� �ִ��� �ľ��ϱ�
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);
    }
    private void OnDrawGizmos()
    {
        //Draw the ground colliders on screen for debug purposes
        if (onGround) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);
    }

    // �ٴ� ����, ���� ���� �Լ�
    public bool GetOnGround() { return onGround; }
}
