using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class StarController : MonoBehaviour
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
    private float coyoteTimeCounter; // ���� ���� �� ���� ���� ���� �ð�
    private float jumpBufferCounter; // ���� �Է� ���� �ð�
    private float dashTimeCounter;
    // ���� ����
    private bool isGrounded;
    private bool isJumping;
    private bool isTouchingWall;
    private bool isDashing;
    private int dashCount;
    private bool isStarClimbing;

    Vector2 avgNormal;

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

    // �ð� ī���͵�
    private void TimeCounters()
    {
        // ���� ���� (����) & �ڿ��� Ÿ��
        jumpBufferCounter -= Time.deltaTime;
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            dashCount = maxDashCount;
        }

        else
            coyoteTimeCounter -= Time.deltaTime;

        // ���� �ð� �����, ��� ������ Damping ��
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
        StarDetectWalls();
        StarRoll();
        StarDetectGround();
        if (!isDashing)
        {
            StarApplyGravity();
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

        // �浹 ������ ����
        if (hitWalls.All(hit => hit.collider == null))
            return;

        // �� normal ��� ���ϱ�
        avgNormal = Vector2.zero;
        float closestDistance = float.MaxValue;
        int count = 0;

        foreach (RaycastHit2D hit in hitWalls)
        {
            if (hit.collider != null)
            {
                avgNormal += hit.normal;
                count++;

                if (hit.distance < closestDistance)
                    closestDistance = hit.distance;
            }
        }

        if (count == 0) return;

        avgNormal.Normalize(); // ��� normal

        // tangent ���
        Vector2 tangent = (moveInput.x > 0)
            ? new Vector2(avgNormal.y, -avgNormal.x)   // �ð� ����
            : new Vector2(-avgNormal.y, avgNormal.x);  // �ݽð� ����

        Vector2 moveDir = tangent.normalized;

        // ����/���� ���
        float accel = speedAcceleration;
        float decel = SpeedDeceleration;
        if (!isStarClimbing)
        {
            accel *= airAccelMulti;
            decel *= airDecelMulti;
        }

        // ��ǥ �ӵ�
        Vector2 targetVel = moveDir * maxSpeed * Mathf.Abs(moveInput.x);

        // ���� ����
        float lerpAmount = ((moveInput.x != 0) ? accel : decel) * Time.fixedDeltaTime;

        // �ӵ� ����
        Vector2 newVel = Vector2.Lerp(rb.linearVelocity, targetVel, lerpAmount);

        // �� ������ ���� �� ����
        if (closestDistance > starMaxWallGravityDistance)
        {
            newVel -= avgNormal * starWallGravity;
        }

        rb.linearVelocity = newVel;
    }



    private void StarRoll()
    {
        if (hitWalls.All(hit => hit.collider == null))
            return; // ���� �浹 ���� �� �Լ� ����
        if (Mathf.Abs(moveInput.x) < 0.01f)
            return;

        float scale = Mathf.Max(starPivotTransform.lossyScale.x, starPivotTransform.lossyScale.y);
        float radius = col.radius * scale;

        // ���ӵ� ũ��
        float speed = rb.linearVelocity.magnitude;

        // ���ӵ� (����/�� �� ��/�� ��ȯ)
        float angularSpeed = speed / radius * Mathf.Rad2Deg;

        // �̵� ���⿡ ���� ��ȣ ����
        float dir = Mathf.Sign(moveInput.x);

        //starPivotTransform.Rotate()
        starPivotTransform.Rotate(Vector3.forward, -angularSpeed * dir * Time.fixedDeltaTime);

        // Rigidbody2D ȸ�� ����
        //rb.MoveRotation(rb.rotation - angularSpeed * dir * Time.fixedDeltaTime);
    }

    private void StarDetectWalls()
    {
        Vector2 origin = starPivotTransform.position;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = 360f / rayCount * i; //
            // starSpriteTransform.up�� Z�� �������� angle��ŭ ȸ��
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
                Time.fixedDeltaTime); // �� ���� ������ ���� ǥ��

        }
        isStarClimbing = hitWalls.Any(hit => hit.collider != null);
    }


    private void StarAirControl()
    {
        if (!isStarClimbing) // �����̸� ��� ����
        {
            float accel = speedAcceleration;
            float decel = SpeedDeceleration;
            accel *= airAccelMulti;
            decel *= airDecelMulti;
            float targetX = moveInput.x * maxSpeed;
            float lerpAmount = (moveInput.x != 0 ? accel : decel) * Time.fixedDeltaTime;
            // �ӵ��� �������� ���ӵ� ����
            float newX = Mathf.Lerp(rb.linearVelocity.x, targetX, lerpAmount);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }

    // �ٴ� ���� (�������� Vector.up�϶�)
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

    // �߷�
    private void StarApplyGravity()
    {
        float newY;
        if (isJumping)
        {
            // ���� �� �߷�(�ö� ��)
            newY = rb.linearVelocity.y - jumpDcceleration * Time.fixedDeltaTime;
        }
        else
        {
            // ���� �� �߷�(������ ��)
            // ���� �� �߷�(����)���� ���� �� �߷�(����)���� ���������� ����
            if (currentGravity < maxGravity)
                currentGravity += gravityAcceleration * Time.fixedDeltaTime;
            else
                currentGravity = maxGravity;

            newY = rb.linearVelocity.y - currentGravity * Time.fixedDeltaTime;
        }

        // ����� ������ �߷� ����
        if (isTouchingWall)
            if (newY < -wallSlideMaxSpeed)
                newY = -wallSlideMaxSpeed;

        if (hitWalls.Any(hit => hit.collider != null))
        {
            if (newY < 0)
                newY = 0;
        }


        // y�� �ִ� �ӵ�
        newY = Mathf.Clamp(newY, -maxDownSpeed, maxJumpSpeed);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
    }

    // ����
    private void StarJump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            if (avgNormal != Vector2.zero)
            {
                //if (selectedWallNormal != null)
                // +y�� linearVelocity ����
                Debug.Log("Jump!");
                isJumping = true;

                //rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJumpSpeed);
                rb.linearVelocity = avgNormal * maxJumpSpeed;
                jumpBufferCounter = 0;
                coyoteTimeCounter = 0;
            }
           
        }
    }

    // ���� Ű ���� isJumping = false -> �߷� ������ -> ���� ������
    private void StarFastFall()
    {
        if (isJumping)
        {
            isJumping = false;
        }
    }


    // ������ Ű �Է� �ݴ� ���� linearVelocity ����
    private void StarWallJump()
    {


        if (isStarClimbing && jumpBufferCounter > 0 && !isGrounded && avgNormal != Vector2.zero)
        {
            isJumping = true;
            rb.linearVelocity = avgNormal * starWallJumpSpeed;
            Debug.Log("Wall Jump");
        }
    }

    // ���, ��� �� ��� ���� �����ϰ� linearVelocity�� ������ moveInput.normalized * dashSpeed��.
    private void StarDash()
    {
        if (moveInput == Vector2.zero) return;
        if (dashCount <= 0) return;
        isDashing = true;
        dashCount -= 1;
        dashTimeCounter = dashTime;
        rb.linearVelocity = moveInput.normalized * dashSpeed;
    }

    // ��� �� ����, ��� ������ �ִ�ӵ� ����
    private void StardampAfterDash()
    {
        float dampedSpeedX = rb.linearVelocity.x;
        float dampedSpeedY = rb.linearVelocity.y;
        dampedSpeedX = Mathf.Clamp(dampedSpeedX, -maxSpeedAfterDashX, maxSpeedAfterDashX);
        dampedSpeedY = Mathf.Min(dampedSpeedY, maxSpeedAfterDashUp);
        rb.linearVelocity = new Vector2(dampedSpeedX, dampedSpeedY);
    }
}
