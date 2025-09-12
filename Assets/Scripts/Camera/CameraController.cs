using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera Cam;
    [SerializeField] private Transform Player;
    [SerializeField] private float SmoothTime = 0.2f;
    [SerializeField] private CameraClamp Clamp;

    [Header("DeadZone / SoftZone")]
    [SerializeField] private Vector2 deadZoneSize = new Vector2(2f, 1f);   // 플레이어가 이 범위 안에 있으면 카메라 고정
    [SerializeField] private Vector2 softZoneSize = new Vector2(4f, 2f);   // DeadZone을 넘어가 SoftZone까지 갈 때 카메라 스무스하게 따라감

    [Header("Zoom In Out")]
    [SerializeField] private float MaxZoomIn = 3f;
    [SerializeField] private float MaxZoomOut = 10f;
    [SerializeField] private float ZoomLerpSpeed = 1f;
    [SerializeField] private float SpeedThreshold = 5f;

    private Vector3 _velocity = new Vector3(2, 2, 2);
    private float targetZoom;
    private Rigidbody2D _rb;

    public static CameraController instance = null;
    public bool IsTriggerZoom { get; private set; }

    private void Awake()
    {
        if(null == instance)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _rb = Player.GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    private void Update()
    {
        var desiredPos = HandleFollow();

        transform.position = Clamp.HandleClamp(desiredPos);

        if (!IsTriggerZoom)
        {
            HandleZoomInOut();
        }
    }

    private Vector3 HandleFollow()
    {
        Vector3 camPos = transform.position;
        Vector3 playerPos = Player.position;

        Vector2 dz = deadZoneSize;   // DeadZone 크기
        Vector2 sz = softZoneSize;   // SoftZone 크기

        float newX = camPos.x;
        float newY = camPos.y;

        // x
        float deltaX = playerPos.x - camPos.x;

        if (Mathf.Abs(deltaX) > dz.x)
        {
            // DeadZone 벗어나면 SmoothDamp로 따라가기
            newX = Mathf.SmoothDamp(camPos.x, playerPos.x, ref _velocity.x, SmoothTime);
        }
        else
        {
            // DeadZone 안에서도 살짝 따라가도록 비율 조정
            float moveFactorX = deltaX / dz.x;
            newX += moveFactorX * SmoothTime / 2f;
        }

        // y
        float deltaY = playerPos.y - camPos.y;

        if (Mathf.Abs(deltaY) > dz.y)
        {
            // DeadZone 밖이면 SmoothDamp
            newY = Mathf.SmoothDamp(camPos.y, playerPos.y, ref _velocity.y, SmoothTime);
        }
        // DeadZone 안이면 Y축은 고정

        return new Vector3(newX, newY, camPos.z);
    }



    private void HandleCameraShaking()
    {

    }

    private void HandleZoomInOut()
    {
        var playerVelocity = _rb.linearVelocity;

        _velocity = playerVelocity;
       
        float speed = playerVelocity.magnitude;
        if (speed > SpeedThreshold)
        {
            targetZoom = MaxZoomOut;
        }
        else
        {
            targetZoom = MaxZoomIn;
        }

        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, targetZoom, Time.deltaTime * ZoomLerpSpeed);
    }

    public void TriggerZoomOut(float targetZoom)
    {
        IsTriggerZoom = true;
        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, targetZoom, Time.deltaTime * ZoomLerpSpeed);
    }

    public void TriggerZoomIn(float targetZoom)
    {
        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, targetZoom, Time.deltaTime * ZoomLerpSpeed);
        IsTriggerZoom = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(transform.position.x, transform.position.y, 0f);
        Vector3 size = new Vector3(deadZoneSize.x * 2f, deadZoneSize.y * 2f, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}
