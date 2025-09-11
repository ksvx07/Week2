using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform Player;
    [SerializeField] private float SmoothTime = 0.2f;
    [SerializeField] private CameraClamp Clamp;

    [Header("Zoom In Out")]
    [SerializeField] private float MaxZoomIn = 3f;
    [SerializeField] private float MaxZoomOut = 10f;
    [SerializeField] private float ZoomLerpSpeed = 2f;
    [SerializeField] private float TestSpeed;

    private Vector3 _offset = new Vector3(0, 0, -10);
    private Vector3 _velocity = new Vector3(2, 2, 2);

    private static CameraController instance = null;

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
    }

    private void LateUpdate()
    {
        var desiredPos = HandleFollow();

        transform.position = Clamp.HandleClamp(desiredPos);

        HandleZoomInOut();
    }

    private Vector3 HandleFollow()
    {
        Vector3 targetPos = Player.position + _offset;
        return Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, SmoothTime);
    }

    private void HandleCameraShaking()
    {

    }

    private void HandleZoomInOut()
    {
        var playerVelocity = Player.GetComponent<Rigidbody2D>().linearVelocity;

        // 원의 경우 속도 ~ 이상일 때 줌 아웃 효과
        float speed = playerVelocity.magnitude;
        float targetZoom = Mathf.Lerp(MaxZoomIn, MaxZoomOut, speed / 10f);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * ZoomLerpSpeed);
    }
}
