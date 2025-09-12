using System.Collections;
using UnityEngine;

public class KirbyController : MonoBehaviour, IPlayerController
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

    [Header("Turbo Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("터보속도")] private float turboSpeed = 20f;

    [Header("Bounce Settings")]
    [Tooltip("X축으로 튕겨 나가는 힘")]
    [SerializeField] private float bounceStrength = 5f;
    [Tooltip("Y축으로 튕겨 나가는 힘")]
    [SerializeField] private float bounceHeight = 10f;
    [Tooltip("튕겨 나가는 효과가 지속되는 최소시간")]
    [SerializeField] private float bounceDuration = 0.3f;
    private bool isBouncing = false; // 현재도 튕겨 나가는지 
    private bool isFixedBouncing = false; // 튕겨 나가지는 최소 유지 시간

    [Header("Current State")]
    public bool onGround;
    public bool pressingKey; // 이동키를 누르고 있는지 여부
    private bool turboMode;

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
    public bool TurboMode
    {
        get { return turboMode; }
    }
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _groundCheck = GetComponent<KirbyGroundCheck>(); // 땅 닿았는지 알기위한 스크립트
    }

    private void Update()
    {
        // 입력키를 누르든 말든 최소 유지해야 하는 boucning시간
        if (isFixedBouncing)
        {
            return;
        }

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

        // 바운스 상태에서 입력이 없으면, 계속 바운스 vector 유지
        if (isBouncing)
        {
            if(pressingKey) {isBouncing = false;}
            else  return;
        }


        if (turboMode)
        {
            // 터보 speed로 이동
            desiredVelocity = new Vector2(transform.localScale.x, 0f) * turboSpeed;
        }
        else
        {
            // 현재 누르고 있는 방향에, maxSpeed를 곱해, desiredVelocity를 구하기 (바로 maxSpeed에 도달하지 않고, 가속도 여부로 정하기)
            desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (isFixedBouncing) return; // 최소 바운스 유지 시간에는 return

        onGround = _groundCheck.GetOnGround();

        // 바운스 상태에서는 땅이 닿으면, 바운스 상태 종료
        if (isBouncing)
        {
            if(onGround) { isBouncing = false; }
            return;
        }
        //현재 velocity 값을 가져오기
        moveVelocity = _rb.linearVelocity;

        if (turboMode)
        {
            // 터보 모드에서는 가속이나, 감속 없음
            runWithoutAcceleration();
        }
        else
        {
            runWithAcceleration();
        }
    }

    // Hack: 임시로 Wall tag 만듬
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 벽 태그를 가진 오브젝트와 충돌했는지, 현재 바운스 중이 아닌지, 터보모드인지 확인
        if (collision.gameObject.CompareTag("Wall") && turboMode)
        {
            if (isBouncing) return;
            turboMode = false;
            // 충돌 시 바운스 코루틴 시작
            StartCoroutine(Bounce(collision));
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

    private IEnumerator Bounce(Collision2D collision)
    {
        isFixedBouncing = true;
        isBouncing = true;

        // 벽의 법선 벡터를 가져와 튕겨나갈 방향을 결정
        Vector2 normal = collision.contacts[0].normal;

        // 수평 반대 방향과 수직 높이를 포함한 고정 속도 벡터 생성
        Vector2 fixedBounceVelocity = new Vector2(
            // 벽의 x축 법선 방향을 반전시켜 반대 방향으로 튕기게 함
            normal.x * bounceStrength,
            // 고정된 높이로 튕기게 함
            bounceHeight
        );

        // Rigidbody에 속도 적용
        _rb.linearVelocity = fixedBounceVelocity;

        yield return new WaitForSeconds(bounceDuration);
        isFixedBouncing = false;
    }

    #region Public - PlayerInput
    public void OnMoveInput(Vector2 movementInput)
    {
        directionX = movementInput.x;
    }

    public void OnTurboModePressed()
    {
        // 바운스 상태에서는 TurboMode 불가능
        if (isBouncing) return;

        turboMode = !turboMode;
    }

    public void OnEnableSetVelocity(float newVelX, float newVelY)
    {
        _rb = GetComponent<Rigidbody2D>();
        _groundCheck = GetComponent<KirbyGroundCheck>(); // 땅 닿았는지 알기위한 스크립트
        _rb.linearVelocity = new Vector2(newVelX, newVelY);
    }
    #endregion
}
