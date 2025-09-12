using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class StarController : MonoBehaviour
{
    private PlayerInput inputActions;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private CircleCollider2D col;

    // Inspector 조절 가능 변수들
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
    [SerializeField] private float coyoteTime = 0.1f;       // 코요테 타임 길이
    [SerializeField] private float jumpBufferTime = 0.1f;   // 점프 버퍼 길이

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

    [Header("Star")]
    [SerializeField] private float starRayGravityDistance = 1f;
    [SerializeField] private float starWallJumpSpeed = 5f;
    [SerializeField] private Transform starPivotTransform;
    [SerializeField] private float starWallGravity = 5f;
    [SerializeField] private float starMaxWallGravityDistance = 5f;

    private LayerMask wallLayer;

    int rayCount = 60;
    private RaycastHit2D[] hitWalls;
    Vector2[] rayDirs;
    private float currentGravity;
    private float coyoteTimeCounter; // 땅을 떠난 후 남은 점프 가능 시간
    private float jumpBufferCounter; // 점프 입력 유지 시간
    private float dashTimeCounter;
    // 내부 상태
    private bool isGrounded;
    private bool isJumping;
    private bool isTouchingWall;
    private bool isDashing;
    private int dashCount;
    private bool isStarClimbing;

    private Vector2? selectedWallNormal;
    //private Vector2 closestWallNormal;

    private void Awake()
    {
        inputActions = new PlayerInput();
        col = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        currentGravity = jumpDcceleration;
        wallLayer = LayerMask.GetMask("Ground");
        dashCount = maxDashCount;

        hitWalls = new RaycastHit2D[rayCount];
        rayDirs = new Vector2[rayCount];

    // Rigidbody 설정
    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0f; // 중력은 직접 처리
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
        StarFastFall();
    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        StarDash();
    }

    private void Update()
    {
        TimeCounters();
    }

    // 시간 카운터들
    private void TimeCounters()
    {
        // 점프 버퍼 (예약) & 코요테 타임
        jumpBufferCounter -= Time.deltaTime;
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            dashCount = maxDashCount;
        }

        else
            coyoteTimeCounter -= Time.deltaTime;

        // 일정 시간 대시함, 대시 끝나면 Damping 줌
        if (isDashing)
        {
            dashTimeCounter -= Time.deltaTime;
            if (dashTimeCounter < 0)
            {
                isDashing = false;
                StardampAfterDash();
            }
        }
    }

    private void FixedUpdate()
    {
        //WallCheck();
        StarDetectWalls();
        StarRoll();
        StarDetectGround();
        if (!isDashing)
        {
            StarApplyGravity();
            //Move();
            StarAirControl();
            StarWallClimbing();
            StarJump();
            StarWallJump();

        }


        //Debug.Log($"x: {rb.linearVelocity.x:F2}, y: {rb.linearVelocity.y:F2}");
    }


    private void StarWallClimbing()
    {
        if (isJumping) return;

        if (hitWalls.All(hit => hit.collider == null))
            return;

        Vector2 bestTangent = Vector2.zero;
        selectedWallNormal = null;
        //wallsNormalAverage = Vector2.zero;
        Vector2 mostClosestNormal = Vector2.zero;
        //Vector2 closestWallNormal = Vector2.zero;

        float wallDistance = 10f;
        var wallNum = 0;
        float bestDot = 1.0f;

        //My ver......................................................,......
        for (int i = 0; i < rayCount; i++)
        {
            RaycastHit2D hitWall = hitWalls[i];
            if (hitWall.collider != null)
            {
                wallNum += 1;
                //wallsNormalAverage += hitWall.normal;

                if (selectedWallNormal == null)
                {
                    selectedWallNormal = hitWall.normal;
                    wallDistance = hitWall.distance;
                }
                else
                {
                    float normalsAngle = Vector2.SignedAngle(selectedWallNormal.Value, hitWall.normal);
                    if (normalsAngle * moveInput.x > 0)
                        selectedWallNormal = hitWall.normal;

                    if (hitWall.distance < wallDistance)
                    {
                        wallDistance = hitWall.distance;
                        //closestWallNormal = hitWall.normal;

                    }

                }

                Vector2 rayDir = rayDirs[i];
                Vector2 wallNormala = hitWall.normal;

                // 레이와 수직인지 확인
                float dot = Mathf.Abs(Vector2.Dot(rayDir.normalized, wallNormala.normalized));
                if (dot < bestDot)  // bestDot은 초기값 1.0f
                {
                    bestDot = dot;
                    mostClosestNormal = wallNormala;
                }
            }
        }

        if (selectedWallNormal == null)
            return;

        // 4. tangent 계산 (벽 타는 방향)
        Vector2 wallNormal = selectedWallNormal.Value;
        Vector2 tangent = (moveInput.x > 0)
            ? new Vector2(wallNormal.y, -wallNormal.x)   // 시계 방향
            : new Vector2(-wallNormal.y, wallNormal.x);  // 반시계 방향


        Vector2 moveDir = tangent.normalized;
        //Vector2 moveDir = bestTangent.normalized;

        // 가속/감속 계수 결정
        float accel = speedAcceleration;
        float decel = SpeedDeceleration;
        if (!isStarClimbing) // 공중일 때 배수 적용
        {
            accel *= airAccelMulti;
            decel *= airDecelMulti;
        }

        // 목표 속도 = tangent 방향 * 최대 속도 * 입력 크기
        Vector2 targetVel = moveDir * maxSpeed * Mathf.Abs(moveInput.x);
        //Vector2 targetVel = moveDir * maxSpeed * moveInput.magnitude;

        // Lerp 비율 (가속 or 감속)
        float lerpAmount = ((moveInput.x != 0) ? accel : decel) * Time.fixedDeltaTime;

        // 현재 속도 → 목표 속도로 부드럽게 변경
        Vector2 newVel = Vector2.Lerp(rb.linearVelocity, targetVel, lerpAmount);

        //newVel -= wallNormal * starWallGravity;
        // 벽 중력 설정
        //if (wallNum != 0)
        //{
        //    wallsNormalAverage /= wallNum;
        //}


        if (Vector2.Dot(selectedWallNormal.Value, mostClosestNormal) > 0.99f)
        {
            if (wallDistance > starMaxWallGravityDistance)
                newVel -= selectedWallNormal.Value * starWallGravity;
        }
        else
        {
            newVel -= (selectedWallNormal.Value + mostClosestNormal) / 2 * starWallGravity;
        }



            // 최종 속도 적용
            rb.linearVelocity = newVel;

        if (Mathf.Abs(moveInput.x) > 0)
        {
            Debug.Log($"x: {rb.linearVelocity.x:F2}, y: {rb.linearVelocity.y:F2}");
        }
    }

    private void StarRoll()
    {
        if (hitWalls.All(hit => hit.collider == null))
            return; // 전부 충돌 없음 → 함수 종료
        if (Mathf.Abs(moveInput.x) < 0.01f)
            return;

        float scale = Mathf.Max(starPivotTransform.lossyScale.x, starPivotTransform.lossyScale.y);
        float radius = col.radius * scale;

        // 선속도 크기
        float speed = rb.linearVelocity.magnitude;

        // 각속도 (라디안/초 → 도/초 변환)
        float angularSpeed = speed / radius * Mathf.Rad2Deg;

        // 이동 방향에 따라 부호 결정
        float dir = Mathf.Sign(moveInput.x);

        //starPivotTransform.Rotate()
        starPivotTransform.Rotate(Vector3.forward, -angularSpeed * dir * Time.fixedDeltaTime);

        // Rigidbody2D 회전 적용
        //rb.MoveRotation(rb.rotation - angularSpeed * dir * Time.fixedDeltaTime);
    }

    private void StarDetectWalls()
    {
        Vector2 origin = starPivotTransform.position;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = 360f / rayCount * i; //
            // starSpriteTransform.up을 Z축 기준으로 angle만큼 회전
            //Vector3 dir3 = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
            ////Vector3 dir3 = Quaternion.Euler(0f, 0f, angle) * starPivotTransform.up;
            //Vector2 dir = new Vector2(dir3.x, dir3.y);

            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                              Mathf.Sin(angle * Mathf.Deg2Rad));

            rayDirs[i] = dir;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, starRayGravityDistance, wallLayer);
            hitWalls[i] = hit;

            Debug.DrawRay(origin, dir * starRayGravityDistance,
                hitWalls[i] ? Color.red : Color.green,
                Time.fixedDeltaTime); // 한 물리 프레임 동안 표시

        }
        isStarClimbing = hitWalls.Any(hit => hit.collider != null);
    }

    // 이동
    //private void Move() 
    //{
    //    float accel = speedAcceleration;
    //    float decel = SpeedDeceleration;
    //    if (!isGrounded) // 공중이면 배수 적용
    //    {
    //        accel *= airAccelMulti;
    //        decel *= airDecelMulti;
    //    }
    //    float targetX = moveInput.x * maxSpeed;
    //    float lerpAmount = (moveInput.x != 0 ? accel : decel) * Time.fixedDeltaTime;
    //    // 속도가 빠를수록 가속도 감소
    //    float newX = Mathf.Lerp(rb.linearVelocity.x, targetX, lerpAmount);
    //    rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y); 
    //}

    private void StarAirControl()
    {
        if (!isStarClimbing) // 공중이면 배수 적용
        {
            float accel = speedAcceleration;
            float decel = SpeedDeceleration;
            accel *= airAccelMulti;
            decel *= airDecelMulti;
            float targetX = moveInput.x * maxSpeed;
            float lerpAmount = (moveInput.x != 0 ? accel : decel) * Time.fixedDeltaTime;
            // 속도가 빠를수록 가속도 감소
            float newX = Mathf.Lerp(rb.linearVelocity.x, targetX, lerpAmount);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }

    // 바닥 감지 (법선벡터 Vector.up일때)
    private void StarDetectGround()
    {
        bool hasUpNormal = hitWalls.Any(hit =>
        hit.collider != null &&
        Vector2.Dot(hit.normal.normalized, Vector2.up) > 0.99f
);

        if (hasUpNormal)
        {
            isGrounded = true;
        }
        else
            isGrounded = false;


        if (isJumping && rb.linearVelocity.y <= 0.05f)
        {
            isJumping = false;
            currentGravity = jumpDcceleration;
        }
    }

    // 중력
    private void StarApplyGravity()
    {
        float newY;
        if (isJumping)
        {
            // 점프 중 중력(올라갈 때)
            newY = rb.linearVelocity.y - jumpDcceleration * Time.fixedDeltaTime;
        }
        else
        {
            // 점프 후 중력(떨어질 때)
            // 점프 중 중력(약함)에서 점프 후 중력(강함)으로 연속적으로 증가
            if (currentGravity < maxGravity)
                currentGravity += gravityAcceleration * Time.fixedDeltaTime;
            else
                currentGravity = maxGravity;

            newY = rb.linearVelocity.y - currentGravity * Time.fixedDeltaTime;
        }

        // 벽잡고 있으면 중력 낮음
        if (isTouchingWall)
            if (newY < -wallSlideMaxSpeed)
                newY = -wallSlideMaxSpeed;

        if (hitWalls.Any(hit => hit.collider != null))
        {
            if (newY < 0)
                newY = 0;
        }


        // y축 최대 속도
        newY = Mathf.Clamp(newY, -maxDownSpeed, maxJumpSpeed);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
    }

    // 점프
    private void StarJump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            if (selectedWallNormal != null)
            {
                // +y로 linearVelocity 설정
                Debug.Log("Jump!");
                isJumping = true;

                //rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJumpSpeed);
                rb.linearVelocity = selectedWallNormal.Value * maxJumpSpeed;
                jumpBufferCounter = 0;
                coyoteTimeCounter = 0;
            }
        }
    }

    // 점프 키 때면 isJumping = false -> 중력 강해짐 -> 빨리 떨어짐
    private void StarFastFall()
    {
        if (isJumping)
        {
            isJumping = false;
        }
    }

    // 벽 감지 (Raycast)
    private void StarWallCheck()
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

    // 벽점프 키 입력 반대 위로 linearVelocity 설정
    private void StarWallJump()
    {


        if (isStarClimbing && jumpBufferCounter > 0 && !isGrounded && selectedWallNormal != null)
        {
            isJumping = true;
            rb.linearVelocity = selectedWallNormal.Value * starWallJumpSpeed;
            Debug.Log("Wall Jump");
        }
    }

    // 대시, 대시 중 모든 변수 무시하고 linearVelocity는 무조건 moveInput.normalized * dashSpeed됨.
    private void StarDash()
    {
        if (moveInput == Vector2.zero) return;
        if (dashCount <= 0) return;
        isDashing = true;
        dashCount -= 1;
        dashTimeCounter = dashTime;
        rb.linearVelocity = moveInput.normalized * dashSpeed;
    }

    // 대시 후 댐핑, 대시 끝나면 최대속도 조절
    private void StardampAfterDash()
    {
        float dampedSpeedX = rb.linearVelocity.x;
        float dampedSpeedY = rb.linearVelocity.y;
        dampedSpeedX = Mathf.Clamp(dampedSpeedX, -maxSpeedAfterDashX, maxSpeedAfterDashX);
        dampedSpeedY = Mathf.Min(dampedSpeedY, maxSpeedAfterDashUp);
        rb.linearVelocity = new Vector2(dampedSpeedX, dampedSpeedY);
    }
}
