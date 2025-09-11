using UnityEngine;
using UnityEngine.InputSystem;

public class CircleController : MonoBehaviour
{
    private PlayerInput inputActions;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private CircleCollider2D col;

    // Inspector ���� ���� ������
    [Header("Move")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float speedAcceleration = 5f;
    [SerializeField] private float SpeedDeceleration = 5f;

    [Header("Jump / Gravity")]
    [SerializeField] private float maxJumpSpeed = 5f;
    [SerializeField] private float jumpDcceleration = 5f;
    [SerializeField] private float maxGravity = 5f;
    [SerializeField] private float gravityAcceleration = 5f;
    [SerializeField] private float maxDownSpeed = 5f;
    [SerializeField] private float coyoteTime = 0.1f;       // �ڿ��� Ÿ�� ����
    [SerializeField] private float jumpBufferTime = 0.1f;   // ���� ���� ����

    [Header("Wall Jump")]
    [SerializeField] private float wallCheckDistance = 0.4f;
    [SerializeField] private float wallJumpXSpeed = 5f;
    [SerializeField] private float wallJumpYSpeed = 5f;
    [SerializeField] private float wallSlideMaxSpeed = 5f;
    [SerializeField] private float wallSlideDecceleration = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashTime = 0.5f;
    [SerializeField] private float maxSpeedAfterDashX = 5f;
    [SerializeField] private float maxSpeedAfterDashUp = 5f;
    [SerializeField] private int maxDashCount = 1;

    [Header("AirTimeMultiplier")]
    [SerializeField] private float airAccelMulti = 0.65f;
    [SerializeField] private float airDecelMulti = 0.65f;

    private LayerMask wallLayer;

    private float currentGravity;
    private float coyoteTimeCounter; // ���� ���� �� ���� ���� ���� �ð�
    private float jumpBufferCounter; // ���� �Է� ���� �ð�
    private float dashTimeCounter;
    // ���� ����
    private bool isGrounded;
    private bool isJumping;
    private bool isTouchingWall;
    private bool isDashing;
    private int dashCount;

    private void Awake()
    {
        inputActions = new PlayerInput();
        col = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        currentGravity = jumpDcceleration;
        wallLayer = LayerMask.GetMask("Ground");
        dashCount = maxDashCount;

        // Rigidbody ����
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0f; // �߷��� ���� ó��
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.started += OnJump;
        inputActions.Player.Jump.canceled += OffJump;
        inputActions.Player.Dash.performed += OnDash;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Jump.started -= OnJump;
        inputActions.Player.Jump.canceled -= OffJump;
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        jumpBufferCounter = jumpBufferTime;
    }

    private void OffJump(InputAction.CallbackContext ctx)
    {
        FastFall();
    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        Dash();
    }

    private void Update()
    {
        jumpBufferCounter -= Time.deltaTime;
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            dashCount = maxDashCount;
        }

        else
            coyoteTimeCounter -= Time.deltaTime;
        if (isDashing)
        {
            dashTimeCounter -= Time.deltaTime;
            if (dashTimeCounter < 0)
            {
                isDashing = false;
                dampAfterDash();
            }
        }
    }

    private void FixedUpdate()
    {
        WallCheck();
        DetectGround();
        if (!isDashing)
        {
            Jump();
            WallJump();
            ApplyGravity();
            Move();
        }


        //Debug.Log($"x: {rb.linearVelocity.x:F2}, y: {rb.linearVelocity.y:F2}");
    }

    //private void Move()
    //{
    //    // ��ǥ �ӵ� (�Է� �� �ִ� �ӵ�)
    //    float targetSpeed = moveInput.x * maxSpeed;

    //    // ���� ����/���� ��� ����
    //    float accel = speedAcceleration;
    //    float decel = SpeedDeceleration;

    //    if (!isGrounded) // �����̸� ��� ����
    //    {
    //        accel *= airAccelMulti;
    //        decel *= airDecelMulti;
    //    }

    //    // Lerp ���� ��� (���Ӱ� ���� ����)
    //    float lerpFactor = (Mathf.Abs(moveInput.x) > 0.01f) ? accel : decel;

    //    // �ӵ� ����
    //    targetSpeed = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, lerpFactor * Time.fixedDeltaTime);

    //    // y�ӵ� ����
    //    rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
    //}

    private void Move()
    {
        float accel = speedAcceleration;
        float decel = SpeedDeceleration;
        if (!isGrounded) // �����̸� ��� ����
        {
            accel *= airAccelMulti;
            decel *= airDecelMulti;
        }
        float targetX = moveInput.x * maxSpeed;
        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, (moveInput.x != 0 ? accel : decel) * Time.fixedDeltaTime); // ���� y �ӵ� ����
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    private void DetectGround()
    {
        Bounds bounds = col.bounds;
        float extraHeight = 0.05f;

        RaycastHit2D hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down,
            extraHeight, wallLayer);

        isGrounded = hit.collider != null;


        if (isJumping && rb.linearVelocity.y <= 0)
        {
            isJumping = false;
            currentGravity = jumpDcceleration;
        }
    }

    private void ApplyGravity()
    {
        float newY;
        if (isJumping)
        {
            newY = rb.linearVelocity.y - jumpDcceleration * Time.fixedDeltaTime;
        }
        else
        {
            if (currentGravity < maxGravity)
                currentGravity += gravityAcceleration * Time.fixedDeltaTime;
            else
                currentGravity = maxGravity;

            newY = rb.linearVelocity.y - currentGravity * Time.fixedDeltaTime;
        }

        if (isTouchingWall)
            if (newY < -wallSlideMaxSpeed)
                newY = -wallSlideMaxSpeed;

        newY = Mathf.Clamp(newY, -maxDownSpeed, maxJumpSpeed);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
    }

    private void Jump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            Debug.Log("Jump!");
            isJumping = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJumpSpeed);
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
    }

    private void FastFall()
    {
        if (isJumping)
        {
            isJumping = false;
            //rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }
    }


    private void WallCheck()
    {
        Vector2 origin = transform.position;
        RaycastHit2D hitWall = new RaycastHit2D(); // �⺻������ �ʱ�ȭ

        if (moveInput.x > 0)
        {
            hitWall = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, wallLayer);
            Debug.DrawRay(origin, Vector2.right * wallCheckDistance, Color.red);
        }
        else if (moveInput.x < 0)
        {
            hitWall = Physics2D.Raycast(origin, Vector2.left, wallCheckDistance, wallLayer);
            Debug.DrawRay(origin, Vector2.left * wallCheckDistance, Color.red);
        }

        isTouchingWall = hitWall.collider != null;
    }

    private void WallJump()
    {
        if (isTouchingWall && jumpBufferCounter > 0 && !isGrounded)
        {
            isJumping = true;
            rb.linearVelocity = new Vector2(wallJumpXSpeed * -Mathf.Sign(moveInput.x), wallJumpYSpeed);
            Debug.Log("Wall Jump");
        }
    }

    private void Dash()
    {
        if (moveInput == Vector2.zero) return;
        if (dashCount <= 0) return;
        isDashing = true;
        dashCount -= 1;
        dashTimeCounter = dashTime;
        rb.linearVelocity = moveInput.normalized * dashSpeed;
    }

    private void dampAfterDash()
    {
        float dampedSpeedX = rb.linearVelocity.x;
        float dampedSpeedY = rb.linearVelocity.y;
        dampedSpeedX = Mathf.Clamp(dampedSpeedX, -maxSpeedAfterDashX, maxSpeedAfterDashX);
        dampedSpeedY = Mathf.Min(dampedSpeedY, maxSpeedAfterDashUp);
        rb.linearVelocity = new Vector2(dampedSpeedX, dampedSpeedY);
    }
}
