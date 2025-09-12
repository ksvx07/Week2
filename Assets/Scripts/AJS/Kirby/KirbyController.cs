using System.Collections;
using UnityEngine;

public class KirbyController : MonoBehaviour
{
    #region References
    Rigidbody2D _rb;
    KirbyGroundCheck _groundCheck;
    #endregion

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("최고속도")] private float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)][Tooltip("얼마나 빨리 최고속도에 도달")] private float maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("입력값 없을시, 얼마나 빨리 정지")] private float maxDecceleration = 52f;
    [SerializeField, Range(1f, 100f)][Tooltip("방향 전환시, 얼마나 빨리 정지")] private float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)][Tooltip("공중에서, 얼마나 빨리 최고속도에 도달")] private float maxAirAcceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("공중에서, 입력값 없을시, 얼마나 빨리 정지")] private float maxAirDeceleration; // 줄여서 AirBreak
    [SerializeField, Range(0f, 100f)][Tooltip("공중에서, 방향 전환시, 얼마나 빨리 정지")] private float maxAirTurnSpeed = 80f;// 줄여서 AirControl

    [SerializeField][Tooltip("가속도 적용 여부")] private bool useAcceleration; // 단순 비교용 bool

    [Header("Dash Stats")]
    [SerializeField, Range(0f, 100f)][Tooltip("대쉬거리")] private float dashDistance;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isDashing = false;

    [Header("Current State")]
    public bool onGround;
    public bool pressingKey; // 이동키를 누르고 있는지 여부

    #region Private - Speed Caculation Variables
    private Vector2 desiredVelocity; // 이동하고 싶어 하는 Velocity값
    private Vector2 moveVelocity; // 실제 이동할 Velocity 값
    private float directionX; // 누르고 있는 방향 왼쪽: -1, 오른쪽 +1
    private float maxSpeedChangeAmount; // 현재 적용 할 수 있는 최대 속도 변경량
    private float acceleration; // 현재 적용되는 가속도
    private float deceleration; // 현재 적용되는 감속도
    private float turnSpeed; // 방향전환 속도
    #endregion

    #region Public - Return Speed Variables
    public float DirectionX
    {
        get { return directionX; }
    }
    public float MaxSpeed
    {
        get { return maxSpeed; }
    }
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _groundCheck = GetComponent<KirbyGroundCheck>(); // 땅 닿았는지 알기위한 스크립트
    }

    private void Update()
    {
        // 입력값이 있으면, 누른 방향으로 방향전환
        if (directionX != 0)
        {
            transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
            pressingKey = true;
        }
        else
        {
            pressingKey = false;
        }

        // 현재 누르고 있는 방향에, maxSpeed를 곱해, desiredVelocity를 구하기 (바로 maxSpeed에 도달하지 않고, 가속도 여부로 정하기)
        desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed, 0f);
    }

    private void FixedUpdate()
    {
        onGround = _groundCheck.GetOnGround();

        //현재 velocity 값을 가져오기
        moveVelocity = _rb.linearVelocity;

        // 대시 중에는 일반 이동 로직을 실행하지 않음
        if (isDashing)
        {
            return;
        }

        // Hack: useAcceleration는 단순 이동시 얼마나 어색한지 비교하기 위한 bool 변수다
        // Hack: 실제 게임 제작시에는 필요없는 디버그 if문
        if (useAcceleration)
        {
            runWithAcceleration();
        }
        else
        {
            if (onGround)
            {
                // moveVelocity의 값을 토대로, 단순 Velocity 값 변경
                runWithoutAcceleration();
            }
            else
            {
                // 공중에서는 적용하기
                runWithAcceleration();
            }
        }
    }
    // 최고속도 도달을 위한 가속도 적용시
    private void runWithAcceleration()
    {
        // 공중에 있는지에 따라, 적용할 가속도, 감속도, 방향전환속도 값 설정
        acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        deceleration = onGround ? maxDecceleration : maxAirDeceleration;
        turnSpeed = onGround ? maxTurnSpeed : maxAirTurnSpeed;

        // 이동키를 눌렀으면
        if (pressingKey)
        {
            //현재 이동 x 방향과, 움직여야 하는 방향 x값의 부호가 다르다는 것은. 방향키를 바꿨다는 뜻으로, turnSpeed를 적용한다
            if (Mathf.Sign(directionX) != Mathf.Sign(moveVelocity.x))
            {
                maxSpeedChangeAmount = turnSpeed * Time.deltaTime;
            }
            else
            {
                //같다면, 여전히 같은 방향으로 갔고 있다는 뜻으로, acceleration를 적용한다
                maxSpeedChangeAmount = acceleration * Time.deltaTime;
            }
        }
        else
        {
            //방향키를 누르고 있는 상태가 아니면, 감속해야 하므로, deceleration을 적용한다
            maxSpeedChangeAmount = deceleration * Time.deltaTime;
        }

        //현재 velocity 값과, 가야 되는 velocity 값의 차이를 구하되, 현재 최대 속도변경량을 넘지 않은 값을 반환
        moveVelocity.x = Mathf.MoveTowards(moveVelocity.x, desiredVelocity.x, maxSpeedChangeAmount);

        //최종 계산한 moveVelocity 값을 Update에 적용한다
        _rb.linearVelocity = moveVelocity;
    }

    // 가속도 적용 없이 바로 최고 속도로 이동
    private void runWithoutAcceleration()
    {
        // 가속도나 감속이 없으면
        //단순하게, 누른 방향 * 최대속도 linearVelocity 값을 Rigidbody 전달
        moveVelocity.x = desiredVelocity.x;
        _rb.linearVelocity = moveVelocity;
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;

        // 중력 일시 비활성화 (선택 사항, 대시를 직선으로 만들고 싶을 때)
        _rb.gravityScale = 0;

        float calculatedDashSpeed = dashDistance / dashDuration;
        // 대시 속도 적용
        _rb.linearVelocityX = directionX * calculatedDashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // 대시 종료 후 상태 복구
        isDashing = false;
        _rb.gravityScale = 1; // 원래 중력으로 복구
        _rb.linearVelocityX = 0f;
    }


    #region Public - PlayerInput
    public void OnMoveInput(Vector2 movementInput)
    {
        directionX = movementInput.x;
        print(directionX);
    }

    public void OnDashClicked()
    {
        // 이미 대시 중이면 함수를 종료
        if (isDashing)
        {
            return;
        }
        StartCoroutine(DashCoroutine());
    }
    #endregion
}
