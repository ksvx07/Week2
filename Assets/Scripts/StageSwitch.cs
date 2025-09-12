using UnityEngine;

public class StageSwitch : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private CameraClamp Clamp;

    private void Awake()
    {
        Player = GameObject.FindWithTag("Player").transform;
    }
    void Update()
    {
        if (Player == null) return;

        if(Player.position.x > transform.position.x)
        {
            Clamp.SetMapBounds(gameObject.name);
        }
    }

    public void SetPlayer(Transform player)
    {
        Player = player;
    }
}
