using NUnit.Framework;
using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    [SerializeField] private Vector3 groundCheck;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] private float enemyMaxSpeed = 3f;
    [SerializeField] private float enemyMaxRunSpeed = 3f;
    [SerializeField] private float enemyAccel = 3f;
    [SerializeField] private float enemyDecel = 3f;
    private LayerMask groundLayer;

    private bool goForward;
    private bool isTurning;
    private float currentSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        currentSpeed = enemyMaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void DetectGround()
    {
        if (isTurning) return;
        RaycastHit2D hitForwardGround = Physics2D.Raycast(transform.position + groundCheck* Mathf.Sign(transform.localScale.x), Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D hitWall = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance* Mathf.Sign(transform.localScale.x), groundLayer);
        Debug.DrawRay(transform.position + groundCheck* Mathf.Sign(transform.localScale.x), Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(transform.position, Vector2.right * wallCheckDistance* Mathf.Sign(transform.localScale.x), Color.blue);
        if (hitForwardGround.collider == null || hitWall.collider != null)
            isTurning = true;
    }

    void FixedUpdate()
    {
        DetectGround();
        EnemyMove();
    }

    private void EnemyMove()
    {
        if (!isTurning)
        {
            if (currentSpeed < enemyMaxSpeed)
                currentSpeed += enemyAccel * Time.fixedDeltaTime;
            else
                currentSpeed = enemyMaxSpeed;
        }

        else
        {
            currentSpeed -= enemyDecel * Time.fixedDeltaTime;
            if (Mathf.Abs(currentSpeed) <= 1f)
            {
                currentSpeed = 0;
                isTurning = false;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
        transform.Translate(transform.right * currentSpeed * Mathf.Sign(transform.localScale.x) * Time.fixedDeltaTime);
    }
}
