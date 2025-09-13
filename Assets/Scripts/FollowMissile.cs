using Unity.VisualScripting;
using UnityEngine;

public class FollowMissile : MonoBehaviour
{
    [SerializeField] private float _shootingSpeed = 8f;
    [SerializeField] private float _rotateSpeed = 150f;
    [SerializeField] private float _updateInterval = 0.05f; // 연산 주기 (초)

    private MissileBodyController _missileController;
    private Transform _player => PlayerManager.Instance._currentPlayerPrefab.transform;
    private Rigidbody2D _rb;

    public void Init(MissileBodyController missileController)
    {
        _missileController = missileController;
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (_player == null) return;

/*        Vector2 toTarget = ((Vector2)_player.position - _rb.position).normalized;

        Vector2 forward = (Vector2)transform.up;

        float rotateAmount = Vector3.Cross(forward, toTarget).z;

        _rb.angularVelocity = rotateAmount * _rotateSpeed;

        _rb.linearVelocity = forward * _shootingSpeed;*/

        if (_player == null) return;

        // 목표 방향
        Vector2 toTarget = ((Vector2)_player.position - _rb.position).normalized;
        Vector2 forward = (Vector2)transform.up;

        // Cross.z 로 회전량 계산 (원본과 동일)
        float rotateAmount = Vector3.Cross(forward, toTarget).z;

        // angularVelocity : MoveRotation 방식
        float rotation = _rb.rotation + (rotateAmount * _rotateSpeed * Time.fixedDeltaTime);
        _rb.MoveRotation(rotation);

        // linearVelocity : velocity 방식
        _rb.linearVelocity = forward * _shootingSpeed;
    }

    private void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            if (_missileController != null)
            {
                _missileController._missilePool.Enqueue(gameObject);
                _missileController._activeMissiles.Remove(gameObject);
            }
        }
    }
}
