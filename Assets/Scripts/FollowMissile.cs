using UnityEngine;

public class FollowMissile : MonoBehaviour
{
    [SerializeField] private float _shootingSpeed = 8f;
    [SerializeField] private float _rotateSpeed = 200f;

    private MissileBodyController _missileController;
    private Transform _player;
    private Rigidbody2D _rb;

    void Awake()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        _rb = GetComponent<Rigidbody2D>();

        _rb.gravityScale = 0f;
        _rb.linearDamping = 0f;
        _rb.angularDamping = 0f;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Init(MissileBodyController missileController) => _missileController = missileController;

    void FixedUpdate()
    {
        if (_player == null) return;

        Vector2 toTarget = ((Vector2)_player.position - _rb.position).normalized;

        Vector2 forward = (Vector2)transform.right;

        float rotateAmount = Vector3.Cross(forward, toTarget).z;

        _rb.angularVelocity = -rotateAmount * _rotateSpeed;

        _rb.linearVelocity = - forward * _shootingSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            if (_missileController != null)
            {
                _missileController._missilePool.Enqueue(gameObject);
                _missileController._activeMissiles.Remove(gameObject);
            }
            Debug.Log("미사일 제거!");
        }
    }
}
