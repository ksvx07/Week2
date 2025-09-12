using System.Collections;
using UnityEngine;

public class KirbyController : MonoBehaviour
{
    #region References
    Rigidbody2D _rb;
    KirbyGroundCheck _groundCheck;
    #endregion

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("�ְ�ӵ�")] private float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)][Tooltip("�󸶳� ���� �ְ�ӵ��� ����")] private float maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("�Է°� ������, �󸶳� ���� ����")] private float maxDecceleration = 52f;
    [SerializeField, Range(1f, 100f)][Tooltip("���� ��ȯ��, �󸶳� ���� ����")] private float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)][Tooltip("���߿���, �󸶳� ���� �ְ�ӵ��� ����")] private float maxAirAcceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("���߿���, �Է°� ������, �󸶳� ���� ����")] private float maxAirDeceleration; // �ٿ��� AirBreak
    [SerializeField, Range(0f, 100f)][Tooltip("���߿���, ���� ��ȯ��, �󸶳� ���� ����")] private float maxAirTurnSpeed = 80f;// �ٿ��� AirControl

    [Header("Turbo Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("�ͺ��ӵ�")] private float turboSpeed = 20f;

    [Header("Current State")]
    public bool onGround;
    public bool pressingKey; // �̵�Ű�� ������ �ִ��� ����
    private bool turboMode;

    #region Private - Speed Caculation Variables
    private Vector2 desiredVelocity; // �̵��ϰ� �;� �ϴ� Velocity��
    private Vector2 moveVelocity; // ���� �̵��� Velocity ��
    private float directionX; // ������ �ִ� ���� ����: -1, ������ +1
    private float maxSpeedChangeAmount; // ���� ���� �� �� �ִ� �ִ� �ӵ� ���淮
    private float acceleration; // ���� ����Ǵ� ���ӵ�
    private float deceleration; // ���� ����Ǵ� ���ӵ�
    private float turnSpeed; // ������ȯ �ӵ�
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
        _groundCheck = GetComponent<KirbyGroundCheck>(); // �� ��Ҵ��� �˱����� ��ũ��Ʈ
    }

    private void Update()
    {
        // �Է°��� ������, ���� �������� ������ȯ
        if (directionX != 0)
        {
            transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
            pressingKey = true;
        }
        else
        {
            pressingKey = false;
        }

        if (turboMode)
        {
            // �ͺ� speed�� �̵�
            desiredVelocity = new Vector2(transform.localScale.x, 0f) * turboSpeed;
        }
        else
        {
            // ���� ������ �ִ� ���⿡, maxSpeed�� ����, desiredVelocity�� ���ϱ� (�ٷ� maxSpeed�� �������� �ʰ�, ���ӵ� ���η� ���ϱ�)
            desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed, 0f);
        }
    }

    private void FixedUpdate()
    {
        onGround = _groundCheck.GetOnGround();

        //���� velocity ���� ��������
        moveVelocity = _rb.linearVelocity;

        if (turboMode)
        {
            // �ͺ� ��忡���� �����̳�, ���� ����
            runWithoutAcceleration();
        }
        else
        {
             runWithAcceleration();
        }
    }
    // �ְ�ӵ� ������ ���� ���ӵ� �����
    private void runWithAcceleration()
    {
        // ���߿� �ִ����� ����, ������ ���ӵ�, ���ӵ�, ������ȯ�ӵ� �� ����
        acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        deceleration = onGround ? maxDecceleration : maxAirDeceleration;
        turnSpeed = onGround ? maxTurnSpeed : maxAirTurnSpeed;

        // �̵�Ű�� ��������
        if (pressingKey)
        {
            //���� �̵� x �����, �������� �ϴ� ���� x���� ��ȣ�� �ٸ��ٴ� ����. ����Ű�� �ٲ�ٴ� ������, turnSpeed�� �����Ѵ�
            if (Mathf.Sign(directionX) != Mathf.Sign(moveVelocity.x))
            {
                maxSpeedChangeAmount = turnSpeed * Time.deltaTime;
            }
            else
            {
                //���ٸ�, ������ ���� �������� ���� �ִٴ� ������, acceleration�� �����Ѵ�
                maxSpeedChangeAmount = acceleration * Time.deltaTime;
            }
        }
        else
        {
            //����Ű�� ������ �ִ� ���°� �ƴϸ�, �����ؾ� �ϹǷ�, deceleration�� �����Ѵ�
            maxSpeedChangeAmount = deceleration * Time.deltaTime;
        }

        //���� velocity ����, ���� �Ǵ� velocity ���� ���̸� ���ϵ�, ���� �ִ� �ӵ����淮�� ���� ���� ���� ��ȯ
        moveVelocity.x = Mathf.MoveTowards(moveVelocity.x, desiredVelocity.x, maxSpeedChangeAmount);

        //���� ����� moveVelocity ���� Update�� �����Ѵ�
        _rb.linearVelocity = moveVelocity;
    }

    // ���ӵ� ���� ���� �ٷ� �ְ� �ӵ��� �̵�
    private void runWithoutAcceleration()
    {
        // ���ӵ��� ������ ������
        //�ܼ��ϰ�, ���� ���� * �ִ�ӵ� linearVelocity ���� Rigidbody ����
        moveVelocity.x = desiredVelocity.x;
        _rb.linearVelocity = moveVelocity;
    }

    #region Public - PlayerInput
    public void OnMoveInput(Vector2 movementInput)
    {
        directionX = movementInput.x;
    }

    public void OnTurboModePressed()
    {        
        turboMode = !turboMode;
    }
    #endregion
}
