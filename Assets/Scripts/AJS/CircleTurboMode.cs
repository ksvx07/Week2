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
        //        // 충돌 직전의 속도와 방향을 저장
        //        Vector2 preCollisionVelocity = rb.linearVelocity;



        //        // 파괴 로직 실행
        //        breakableObject.TurbomodeDestoy();

        //        // 오브젝트 파괴 후, 충돌 직전의 속도를 다시 적용하여 속도 감소를 방지
        //        rb.linearVelocity = preCollisionVelocity;
        //        print("pre: " + preCollisionVelocity);
        //        print("rb: " + rb.linearVelocity);
        //        // 부딪힌 오브젝트가 파괴되었으므로 터보 모드 상태를 유지
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
