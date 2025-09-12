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

    [SerializeField][Tooltip("���ӵ� ���� ����")] private bool useAcceleration; // �ܼ� �񱳿� bool

    [Header("Dash Stats")]
    [SerializeField, Range(0f, 100f)][Tooltip("�뽬�Ÿ�")] private float dashDistance;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isDashing = false;

    [Header("Current State")]
    public bool onGround;
    public bool pressingKey; // �̵�Ű�� ������ �ִ��� ����

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

        // ���� ������ �ִ� ���⿡, maxSpeed�� ����, desiredVelocity�� ���ϱ� (�ٷ� maxSpeed�� �������� �ʰ�, ���ӵ� ���η� ���ϱ�)
        desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed, 0f);
    }

    private void FixedUpdate()
    {
        onGround = _groundCheck.GetOnGround();

        //���� velocity ���� ��������
        moveVelocity = _rb.linearVelocity;

        // ��� �߿��� �Ϲ� �̵� ������ �������� ����
        if (isDashing)
        {
            return;
        }

        // Hack: useAcceleration�� �ܼ� �̵��� �󸶳� ������� ���ϱ� ���� bool ������
        // Hack: ���� ���� ���۽ÿ��� �ʿ���� ����� if��
        if (useAcceleration)
        {
            runWithAcceleration();
        }
        else
        {
            if (onGround)
            {
                // moveVelocity�� ���� ����, �ܼ� Velocity �� ����
                runWithoutAcceleration();
            }
            else
            {
                // ���߿����� �����ϱ�
                runWithAcceleration();
            }
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

    private IEnumerator DashCoroutine()
    {
        isDashing = true;

        // �߷� �Ͻ� ��Ȱ��ȭ (���� ����, ��ø� �������� ����� ���� ��)
        _rb.gravityScale = 0;

        float calculatedDashSpeed = dashDistance / dashDuration;
        // ��� �ӵ� ����
        _rb.linearVelocityX = directionX * calculatedDashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // ��� ���� �� ���� ����
        isDashing = false;
        _rb.gravityScale = 1; // ���� �߷����� ����
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
        // �̹� ��� ���̸� �Լ��� ����
        if (isDashing)
        {
            return;
        }
        StartCoroutine(DashCoroutine());
    }
    #endregion
}
