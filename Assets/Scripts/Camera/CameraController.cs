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
    [SerializeField] private Vector2 deadZoneSize = new Vector2(2f, 1f);   // �÷��̾ �� ���� �ȿ� ������ ī�޶� ����
    [SerializeField] private Vector2 softZoneSize = new Vector2(4f, 2f);   // DeadZone�� �Ѿ SoftZone���� �� �� ī�޶� �������ϰ� ����

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

        Player = GameObject.FindWithTag("Player").transform;
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

    public void SetPlayer(Transform player)
    {
        Player = player;
    }

    private Vector3 HandleFollow()
    {
        Vector3 camPos = transform.position;
        Vector3 playerPos = Player.position;

        Vector2 dz = deadZoneSize;   // DeadZone ũ��
        Vector2 sz = softZoneSize;   // SoftZone ũ��

        float newX = camPos.x;
        float newY = camPos.y;

        // x
        float deltaX = playerPos.x - camPos.x;

        if (Mathf.Abs(deltaX) > dz.x)
        {
            // DeadZone ����� SmoothDamp�� ���󰡱�
            newX = Mathf.SmoothDamp(camPos.x, playerPos.x, ref _velocity.x, SmoothTime);
        }
        else
        {
            // DeadZone �ȿ����� ��¦ ���󰡵��� ���� ����
            float moveFactorX = deltaX / dz.x;
            newX += moveFactorX * SmoothTime / 2f;
        }

        // y
        float deltaY = playerPos.y - camPos.y;

        if (Mathf.Abs(deltaY) > dz.y)
        {
            // DeadZone ���̸� SmoothDamp
            newY = Mathf.SmoothDamp(camPos.y, playerPos.y, ref _velocity.y, SmoothTime);
        }
        // DeadZone ���̸� Y���� ����

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
