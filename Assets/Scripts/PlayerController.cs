using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IPlayerController
{
    private PlayerInput inputActions;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    // Inspector ???? ???? ??????
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
    [SerializeField] private float coyoteTime = 0.1f;       // ????? ??? ????
    [SerializeField] private float jumpBufferTime = 0.1f;   // ???? ???? ????

    [Header("Wall Jump")]
    [SerializeField] private float wallCheckDistance = 0.4f;
    [SerializeField] private float wallJumpXSpeed = 5f;
    [SerializeField] private float wallJumpYSpeed = 5f;
    [SerializeField] private float wallSlideMaxSpeed = 5f;

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
    private float coyoteTimeCounter; // ???? ???? ?? ???? ???? ???? ?��?
    private float jumpBufferCounter; // ???? ??? ???? ?��?
    private float dashTimeCounter;
    // ???? ????
    public bool IsGrounded { get; private set; }
    public bool IsJumping { get; private set; }
    private bool isTouchingWall;
    private bool isDashing;
    private int dashCount;
    private bool isFastFalling;
    private int facingDirection = 1; // 1: 오른쪽, -1: 왼쪽
    private Vector3 originalScale; // 원본 크기 저장

    private void Awake()
    {
        inputActions = new PlayerInput();
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        currentGravity = jumpDcceleration;
        wallLayer = LayerMask.GetMask("Ground");
        dashCount = maxDashCount;

        // 원본 크기 저장
        originalScale = transform.localScale;

        // 원본 크기 저장
        originalScale = transform.localScale;

        // Rigidbody ????
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0f; // ????? ???? ???
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
        if (PlayerManager.Instance.IsHold) return;

        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        jumpBufferCounter = jumpBufferTime;
        isFastFalling = false;
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
        TimeCounters();
    }

    // ?��? ??????
    private void TimeCounters()
    {
        // ???? ???? (????) & ????? ???
        jumpBufferCounter -= Time.deltaTime;
        if (jumpBufferCounter < 0)
            isFastFalling = false;
        if (IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            dashCount = maxDashCount;
        }

        else
            coyoteTimeCounter -= Time.deltaTime;

        // ???? ?��? ?????, ??? ?????? Damping ??
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

    // ???
    private void Move() 
    {
        float accel = speedAcceleration;
        float decel = SpeedDeceleration;
        if (!IsGrounded) // ??????? ??? ????
        {
            accel *= airAccelMulti;
            decel *= airDecelMulti;
        }
        // 바라보는 방향 업데이트 및 스프라이트 회전
        if (moveInput.x > 0)
        {
            facingDirection = 1;
            transform.localScale = originalScale; // 오른쪽
        }
        else if (moveInput.x < 0)
        {
            facingDirection = -1;
            Vector3 flippedScale = originalScale;
            flippedScale.x = -originalScale.x;
            transform.localScale = flippedScale; // 왼쪽 (X축 반전)
        }

        float targetX = moveInput.x * maxSpeed;
        float lerpAmount = (moveInput.x != 0 ? accel : decel) * Time.fixedDeltaTime;
        // 이동 방향에 따른 새로운 X축 속도 계산
        float newX = Mathf.Lerp(rb.linearVelocity.x, targetX, lerpAmount);
        rb.linearVelocityX = newX; 
    }

    // ??? ???? (BoxCast)
    private void DetectGround()
    {
        Bounds bounds = col.bounds;
        float extraHeight = 0.05f;

        RaycastHit2D hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down,
            extraHeight, wallLayer);

        IsGrounded = hit.collider != null;
        

        if (IsJumping && rb.linearVelocity.y <= 0)
        {
            IsJumping = false;
            currentGravity = jumpDcceleration;
        }
    }

    // ???
    private void ApplyGravity()
    {
        float newY;
        if (IsJumping)
        {
            // ???? ?? ???(??? ??)
            newY = rb.linearVelocity.y - jumpDcceleration * Time.fixedDeltaTime;
        }
        else
        {
            // ???? ?? ???(?????? ??)
            // ???? ?? ???(????)???? ???? ?? ???(????)???? ?????????? ????
            if (currentGravity < maxGravity)
                currentGravity += gravityAcceleration * Time.fixedDeltaTime;
            else
                currentGravity = maxGravity;

            newY = rb.linearVelocity.y - currentGravity * Time.fixedDeltaTime;
        }

        // ????? ?????? ??? ????
        if (isTouchingWall)
            if (newY < -wallSlideMaxSpeed)
                newY = -wallSlideMaxSpeed;

        // y?? ??? ???
        newY = Mathf.Clamp(newY, -maxDownSpeed, maxJumpSpeed);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
    }

    // ????
    private void Jump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            // +y?? linearVelocity ????
            Debug.Log("Jump!");
            IsJumping = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJumpSpeed);
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            if (isFastFalling)
                IsJumping = false;
        }
    }

    // ???? ? ???? isJumping = false -> ??? ?????? -> ???? ??????
    private void FastFall()
    {
        if (IsJumping)
        {
            IsJumping = false;
        }
        if (jumpBufferCounter > 0)
            isFastFalling = true;

    }

    // ?? ???? (Raycast)
    private void WallCheck()
    {
        Vector2 origin = transform.position;
        RaycastHit2D hitWall = new RaycastHit2D(); // ???????? ????

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

    // ?????? ? ??? ??? ???? linearVelocity ????
    private void WallJump()
    {
        if (isTouchingWall && jumpBufferCounter > 0 && !IsGrounded)
        {
            IsJumping = true;
            rb.linearVelocity = new Vector2(wallJumpXSpeed * -Mathf.Sign(moveInput.x), wallJumpYSpeed);
            Debug.Log("Wall Jump");
        }
    }

    // ???, ??? ?? ??? ???? ??????? linearVelocity?? ?????? moveInput.normalized * dashSpeed??.
    private void Dash()
    {
        if (dashCount <= 0) return;
        isDashing = true;
        dashCount -= 1;
        dashTimeCounter = dashTime;

        // 항상 바라보는 방향으로 대시
        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0);
    }

    // ??? ?? ????, ??? ?????? ????? ????
    private void dampAfterDash()
    {
        float dampedSpeedX = rb.linearVelocity.x;
        float dampedSpeedY = rb.linearVelocity.y;
        dampedSpeedX = Mathf.Clamp(dampedSpeedX, -maxSpeedAfterDashX, maxSpeedAfterDashX);
        dampedSpeedY = Mathf.Min(dampedSpeedY, maxSpeedAfterDashUp);
        rb.linearVelocity = new Vector2(dampedSpeedX, dampedSpeedY);
    }

    public void OnEnableSetVelocity(float newVelX, float newVelY)
    {
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        currentGravity = jumpDcceleration;
        wallLayer = LayerMask.GetMask("Ground");
        dashCount = maxDashCount;

        // Rigidbody ????
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0f; // ????? ???? ???

        rb.linearVelocity = new Vector2(newVelX, newVelY);
    }
}
