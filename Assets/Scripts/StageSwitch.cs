using UnityEngine;

public class StageSwitch : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private CameraClamp Clamp;

    void Update()
    {
        if(Player.position.x > transform.position.x)
        {
            Clamp.SetMapBounds(gameObject.name);
        }
    }
}
