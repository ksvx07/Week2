using UnityEngine;
using UnityEngine.InputSystem.XR;

public class CircleTurboMode : MonoBehaviour
{
    #region Reference
    Rigidbody2D rb;
    CircleController controller;
    #endregion
    [Header("Trail")]
    [SerializeField] private TrailRenderer trail;

    private bool turboMode = false;
    public bool TurboMode
    {
        get { return turboMode; }
    }

    private float maxSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<CircleController>();
        maxSpeed = controller.MaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (maxSpeed - rb.linearVelocity.magnitude < 0.2f)
        {
            TurboModeActive();
        }
        else
        {
            trail.emitting = false;
            turboMode = false;
            rb.excludeLayers = 0;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //print(rb.linearVelocity);
        //if (turboMode)
        //{
        //    print(rb.linearVelocity);
        //    BreakableObject breakableObject = collision.gameObject.GetComponent<BreakableObject>();
        //    if (breakableObject != null)
        //    {
        //        // �浹 ������ �ӵ��� ������ ����
        //        Vector2 preCollisionVelocity = rb.linearVelocity;



        //        // �ı� ���� ����
        //        breakableObject.TurbomodeDestoy();

        //        // ������Ʈ �ı� ��, �浹 ������ �ӵ��� �ٽ� �����Ͽ� �ӵ� ���Ҹ� ����
        //        rb.linearVelocity = preCollisionVelocity;
        //        print("pre: " + preCollisionVelocity);
        //        print("rb: " + rb.linearVelocity);
        //        // �ε��� ������Ʈ�� �ı��Ǿ����Ƿ� �ͺ� ��� ���¸� ����
        //        TurboModeActive();

        //    }
        //}
    }

    private void TurboModeActive()
    {
        trail.emitting = true;
        turboMode = true;
        //print("Turbo : " + rb.linearVelocity);
        rb.excludeLayers = LayerMask.GetMask("Breakable");
    }
}
