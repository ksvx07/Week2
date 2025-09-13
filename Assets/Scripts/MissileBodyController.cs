using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissileBodyController : MonoBehaviour
{
    [SerializeField] private GameObject missile;
    [SerializeField] private Transform missileBody;
    [SerializeField] private float shootingCooldown;

    private Transform _player => PlayerManager.Instance._currentPlayerPrefab.transform;
    private float _shootingCurrentTime;
    private int _maxEnemy = 5;
    public List<GameObject> _activeMissiles = new();
    public Queue<GameObject> _missilePool = new();
    
    void Start()
    {
        _shootingCurrentTime = Time.time;
        _activeMissiles.Add(missile);
    }

    void Update()
    {
        if (_activeMissiles.Count > _maxEnemy) return;

        var distance = Vector3.Distance(transform.position, _player.position);
        Vector3 directionToTarget = _player.position - transform.position;

        if(Mathf.Abs(distance) < 5f )
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            transform.rotation = targetRotation * Quaternion.Euler(0, 90, 0);

            if(Time.time - _shootingCurrentTime > shootingCooldown )
            {
                ShootingBullet();
            }
        }
    }

    private void ShootingBullet()
    {
        GameObject bullet;

        if(_missilePool.Count > 0)
        {
            bullet = _missilePool.Dequeue();

            bullet.transform.SetParent(missileBody.transform);

            bullet.transform.localPosition = new Vector3(-0.83f, 0f, 0f);   // 위치 초기화
            bullet.transform.localRotation = Quaternion.Euler(0, 0, 90);    // 회전 초기화
        }
        else
        {
            bullet = Instantiate(missile, missileBody.transform);
        }

        bullet.GetComponent<FollowMissile>().Init(this);
        bullet.transform.SetParent(null);

        _shootingCurrentTime = Time.time;
        bullet.SetActive(true);
        _activeMissiles.Add(bullet);
    }
}
