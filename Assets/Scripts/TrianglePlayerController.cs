using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrianglePlayerController : MonoBehaviour
{


    #region 컴포넌트
    private PlayerInput inputActions;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private PolygonCollider2D col;
    #endregion

    #region 상태 변수
    // Inspector variables for tuning
    [Header("Move")]
    [SerializeField] private float maxSpeed = 5f; //최대 이동 속도
    [SerializeField] private float speedAcceleration = 5f; //이동 가속도
    [SerializeField] private float SpeedDeceleration = 5f; //이동 감속도
    [Header("Triangle Hop")]
    [SerializeField] private float hopHeight = 5f; //깡충깡충 점프 높이
    [SerializeField] private float hopCooldownTime = 0.25f; //

    [Header("Jump / Gravity")]
    [SerializeField] private float maxJumpSpeed = 5f; //최대 점프 속도
    [SerializeField] private float jumpDcceleration = 5f; //점프 감속도
    [SerializeField] private float maxGravity = 5f; //최대 중력
    [SerializeField] private float gravityAcceleration = 5f; //중력 가속도
    [SerializeField] private float maxDownSpeed = 5f; //최대 낙하 속도
    [SerializeField] private float coyoteTime = 0.1f;       //코요테 타이머
    [SerializeField] private float jumpBufferTime = 0.1f;   //점프 버퍼 타이머

    [Header("Wall Jump")]
    [SerializeField] private float wallCheckDistance = 0.4f; //벽 체크 거리
    [SerializeField] private float wallJumpXSpeed = 5f; //벽 점프 X 속도
    [SerializeField] private float wallJumpYSpeed = 5f; //벽 점프 Y 속도
    [SerializeField] private float wallSlideMaxSpeed = 5f; //벽 미끄럼 최대 속도
    [SerializeField] private float wallSlideDecceleration = 5f; //벽 미끄럼 감속도

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 5f; //대쉬 속도
    [SerializeField] private float dashTime = 0.5f;  //대쉬 시간
    [SerializeField] private float maxSpeedAfterDashX = 5f; //대쉬 후 최대 X 속도
    [SerializeField] private float maxSpeedAfterDashUp = 5f; //대쉬 후 최대 Y 속도
    [SerializeField] private int maxDashCount = 1; //최대 대쉬 횟수

    [Header("AirTimeMultiplier")]
    [SerializeField] private float airAccelMulti = 0.65f; //공중 가속도 멀티플라이어
    [SerializeField] private float airDecelMulti = 0.65f; //공중 감속도 멀티플라이어

    #endregion

    #region 내부 변수
    private LayerMask wallLayer; //벽 레이어 마스크(Ground)

    private float currentGravity; //현재 중력 값
    private float coyoteTimeCounter; // 코요테 타이머 카운터
    private float jumpBufferCounter; // 점프 버퍼 카운터
    private float dashTimeCounter; //대쉬 타이머 카운터
    private float hopCooldown = 0f; // 이동 뛰기 쿨다운 추가
    #endregion

    #region 상태 플래그
    private bool isHopping; //깡충깡충 점프 중인지 여부
    private bool isGrounded; //땅에 닿았는지 여부
    private bool isJumping; //점프 중인지 여부
    private bool isTouchingWall; //벽에 닿았는지 여부
    private bool isDashing; //대쉬 중인지 여부
    #endregion

    private int dashCount; //남은 대쉬 횟수

    #region 초기화
    private void Awake()
    {
        inputActions = new PlayerInput();
        col = GetComponent<PolygonCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        currentGravity = jumpDcceleration;
        wallLayer = LayerMask.GetMask("Ground");
        dashCount = maxDashCount;

        // Rigidbody 설정
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0f; // 중력 스케일 초기화
    }
    #endregion

    #region 입력 이벤트 등록/해제
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
    #endregion

    #region 입력 처리
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
    #endregion

    #region Update/FixedUpdate
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

    #endregion
    
    #region 바닥감지
    private void DetectGround()
    {
        Bounds bounds = col.bounds;
        float extraHeight = 0.05f;

        RaycastHit2D hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down,
            extraHeight, wallLayer);

        isGrounded = hit.collider != null;


       // 땅에 착지했을 때 hop 상태 해제
        if (isGrounded && isHopping)
        {
            isHopping = false;
        }

        if (isJumping && rb.linearVelocity.y <= 0)
        {
            isJumping = false;
            currentGravity = jumpDcceleration;
        }
    }
    #endregion

    #region 이동
    // private void Move() //네모 이동
    // {
    //     float accel = speedAcceleration;
    //     float decel = SpeedDeceleration;
    //     if (!isGrounded) //공중에서 이동 시 가속도, 감속도 감소
    //     {
    //         accel *= airAccelMulti;
    //         decel *= airDecelMulti;
    //     }
    //     float targetX = moveInput.x * maxSpeed;
    //     float lerpAmount = (moveInput.x != 0 ? accel : decel) * Time.fixedDeltaTime;
    //     // 이동 방향에 따라 속도 보간
    //     float newX = Mathf.Lerp(rb.linearVelocity.x, targetX, lerpAmount);
    //     rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    // }
    
    private void Move() //삼각형 이동
    {
        if (hopCooldown > 0)
        {
            hopCooldown -= Time.fixedDeltaTime;
            return; // 쿨다운 중에는 이동하지 않음
        }

        float accel = speedAcceleration;
        float decel = SpeedDeceleration;
        if (!isGrounded) //공중에서 이동 시 가속도, 감속도 감소
        {
            accel *= airAccelMulti;
            decel *= airDecelMulti;
        }
        float targetX = moveInput.x * maxSpeed;
        float lerpAmount = (moveInput.x != 0 ? accel : decel) * Time.fixedDeltaTime;
        // 이동 방향에 따라 속도 보간
        float newX = Mathf.Lerp(rb.linearVelocity.x, targetX, lerpAmount);
        
        if (moveInput.x != 0 && isGrounded)
        {
            isHopping = true; // hop 상태 설정
            rb.linearVelocity = new Vector2(newX, hopHeight);
            hopCooldown = hopCooldownTime; // 쿨다운 시간 설정
        }
        else
        {
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }
    #endregion

    #region 중력적용
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
    #endregion


    #region 점프
    private void Jump()
    {
        if (jumpBufferCounter > 0 && (coyoteTimeCounter > 0 || isHopping))
        {
            Debug.Log("Jump!");
            isJumping = true;
            isHopping = false; // 점프 시 hop 상태 해제
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJumpSpeed);
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
    }
    #endregion

    #region ?
    private void FastFall()
    {
        if (isJumping)
        {
            isJumping = false;
            //rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }
    }
    #endregion

    #region 벽감지
    private void WallCheck()
    {
        Vector2 origin = transform.position;
        RaycastHit2D hitWall = new RaycastHit2D(); // 기본값으로 초기화

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
    #endregion

    #region 벽점프
    private void WallJump()
    {
        if (isTouchingWall && jumpBufferCounter > 0 && !isGrounded)
        {
            isJumping = true;
            rb.linearVelocity = new Vector2(wallJumpXSpeed * -Mathf.Sign(moveInput.x), wallJumpYSpeed);
            Debug.Log("Wall Jump");
        }
    }
    #endregion

    #region 대쉬
    private void Dash()
    {
        if (moveInput == Vector2.zero) return;
        if (dashCount <= 0) return;
        isDashing = true;
        dashCount -= 1;
        dashTimeCounter = dashTime;
        rb.linearVelocity = moveInput.normalized * dashSpeed;
    }
    #endregion

    #region 대쉬 후 감속
    private void dampAfterDash()
    {
        float dampedSpeedX = rb.linearVelocity.x;
        float dampedSpeedY = rb.linearVelocity.y;
        dampedSpeedX = Mathf.Clamp(dampedSpeedX, -maxSpeedAfterDashX, maxSpeedAfterDashX);
        dampedSpeedY = Mathf.Min(dampedSpeedY, maxSpeedAfterDashUp);
        rb.linearVelocity = new Vector2(dampedSpeedX, dampedSpeedY);
    }
    #endregion
}
