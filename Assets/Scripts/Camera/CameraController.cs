using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera Cam;
    [SerializeField] private Transform Player;
    [SerializeField] private float SmoothTime = 0.2f;
    [SerializeField] private CameraClamp Clamp;
    [SerializeField] private float MinRiseJump = 5f;
    [SerializeField] private float MinFollowHoldTime = 2f;

    [Header("Zoom In Out")]
    [SerializeField] private float MaxZoomIn = 3f;
    [SerializeField] private float MaxZoomOut = 10f;
    [SerializeField] private float ZoomLerpSpeed = 1f;
    [SerializeField] private float SpeedThreshold = 5f;

    private Vector3 _offset = new Vector3(0, 0, -10);
    private Vector3 _velocity = new Vector3(2, 2, 2);
    private float targetZoom;
    private Rigidbody2D _rb;
    private float _beforeY;
    private bool _followY;
    private bool _wasHopping;
    private float _hoppingEnterTime;

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
        _beforeY = Player.position.y;
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
        /*Vector3 targetPos = Player.position + _offset;
        return Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, SmoothTime);*/
        float targetX = Player.position.x + _offset.x;
        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref _velocity.x, SmoothTime);

        // isHopping은 TrianlgeController 에서만 체크
        bool isGround = Player.GetComponent<PlayerController>().IsGrounded;
        //bool isHopping = Player.GetComponent<TrianglePlayerController>().IsHopping;
        bool isJumping = Player.GetComponent<PlayerController>().IsJumping;

        /*if (isHopping && !_wasHopping)
        {
            _hoppingEnterTime = Time.time;
        }
        _wasHopping = isHopping;*/

        float newY = transform.position.y;

        if (isGround)
        {
            _beforeY = Player.position.y;
        }

        float changedY = Player.position.y - _beforeY;

        /*if (isHopping)
        {
            if (Time.time - _hoppingEnterTime > 2f)
                _followY = true;
            else
                _followY = false;
        }*/

        if (Mathf.Abs(changedY) >= MinRiseJump)
        {
            _followY = true;
        }
        else if (isJumping)
        {
            _followY = true;
        }

        if (_followY)
        {
            float targetY = Player.position.y + _offset.y;
            newY = Mathf.SmoothDamp(transform.position.y, targetY, ref _velocity.y, SmoothTime);
        }

        return new Vector3(newX, newY, transform.position.z);
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
}
