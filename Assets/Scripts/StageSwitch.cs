using UnityEngine;

public class StageSwitch : MonoBehaviour
{
    [SerializeField] private CameraClamp Clamp;

    private Transform Player => PlayerManager.Instance._currentPlayerPrefab.transform;

    void Update()
    {
        if (Player == null) return;

        if(Player.position.x > transform.position.x)
        {
            Clamp.SetMapBounds(gameObject.name);
        }
    }
}
