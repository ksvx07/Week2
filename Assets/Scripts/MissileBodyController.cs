using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissileBodyController : MonoBehaviour
{
    [SerializeField] private GameObject missile;
    [SerializeField] private Transform missileBody;
    [SerializeField] private float shootingCooldown;

    private Transform _player;
    private float _shootingCurrentTime;
    public List<GameObject> _activeMissiles = new();
    public Queue<GameObject> _missilePool = new();
    
    void Start()
    {
        _shootingCurrentTime = Time.time;
        _player = GameObject.FindWithTag("Player").transform;
        _activeMissiles.Add(missile);
    }

    void Update()
    {
        var distance = Vector3.Distance(transform.position, _player.position);
        Vector3 directionToTarget = _player.position - transform.position;

        if(Mathf.Abs(distance) < 5f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            transform.rotation = targetRotation * Quaternion.Euler(0, 90, 0);

            if(Time.time - _shootingCurrentTime > shootingCooldown)
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
            bullet.transform.position = missileBody.position;   // 위치 초기화
            bullet.transform.rotation = Quaternion.identity;    // 회전 초기화
            if(transform.position.x < _player.position.x)
            {
                bullet.transform.localScale = new Vector3(-1, 1, 1);
            }

        }
        else
        {
            bullet = Instantiate(missile, missileBody.position, Quaternion.identity);
        }

        bullet.GetComponent<FollowMissile>().Init(this);
        bullet.transform.SetParent(null);

        _shootingCurrentTime = Time.time;
        bullet.SetActive(true);
        _activeMissiles.Add(bullet);
    }
}
