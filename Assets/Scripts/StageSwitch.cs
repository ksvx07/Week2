using UnityEngine;

public enum SwitchDirection
{
    LeftToRight,
    RightToLeft,
    TopToBottom,
    BottomToTop
}

public class StageSwitch : MonoBehaviour
{
    [SerializeField] private CameraClamp Clamp;
    [SerializeField] private int BeforeId;
    [SerializeField] private int AfterId;
    [SerializeField] private SwitchDirection direction;

    private Transform Player => PlayerManager.Instance._currentPlayerPrefab.transform;

    void Update()
    {
        if (Player == null) return;

        if (BeforeId == 0 || AfterId == 0) return;

        switch (direction)
        {
            case SwitchDirection.LeftToRight:
                if (Player.position.x > transform.position.x)
                    Clamp.SetMapBounds(AfterId);
                else if(Player.position.x < transform.position.x)
                    Clamp.SetMapBounds(BeforeId);
                break;
            case SwitchDirection.RightToLeft:
                if (Player.position.x < transform.position.x)
                    Clamp.SetMapBounds(AfterId);
                else if (Player.position.x > transform.position.x)
                    Clamp.SetMapBounds(BeforeId);
                break;
            case SwitchDirection.TopToBottom:
                if (Player.position.y < transform.position.y)
                    Clamp.SetMapBounds(AfterId);
                else if (Player.position.y > transform.position.y)
                    Clamp.SetMapBounds(BeforeId);
                break;
            case SwitchDirection.BottomToTop:
                if (Player.position.y > transform.position.y)
                    Clamp.SetMapBounds(AfterId);
                else if (Player.position.y < transform.position.y)
                    Clamp.SetMapBounds(BeforeId);
                break;
        }
    }
}
