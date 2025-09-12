using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> Players;
    [SerializeField] private CameraController camControlelr;
    private List<StageSwitch> StageSwitch;
    private GameObject _currentPlayer;

    private void Awake()
    {
        _currentPlayer = GameObject.FindWithTag("Player");
        StageSwitch = Object.FindObjectsByType<StageSwitch>(FindObjectsSortMode.None).ToList();
    }

    private void Update()
    {
        // 키 눌렀을 때 변환 (임시)
        if (Input.GetKeyDown(KeyCode.W))
        {
            int currentIdx = Players.IndexOf(_currentPlayer);

            if(currentIdx + 1  < Players.Count)
            {
                var nextPlayer = Players[currentIdx + 1];

                SetActivePlayer(_currentPlayer, nextPlayer);
            }
            else
            {
                var nextPlayer = Players[0];

                SetActivePlayer(_currentPlayer, nextPlayer);
            }
        }
    }

    private void SetActivePlayer(GameObject _lastPlayer, GameObject _nowPlayer)
    {
        var lastPosition = _lastPlayer.transform.position;
        var lastVelocity = _lastPlayer.GetComponent<Rigidbody2D>().linearVelocity;
        Debug.Log(lastVelocity);
        _lastPlayer.SetActive(false);

        _currentPlayer = _nowPlayer;
        _currentPlayer.transform.position = lastPosition;
        _currentPlayer.SetActive(true);
        _currentPlayer.GetComponent<IPlayerController>().OnEnableSetVelocity(lastVelocity.x, lastVelocity.y);
        Debug.Log(_currentPlayer.GetComponent<Rigidbody2D>().linearVelocity);

        camControlelr.SetPlayer(_currentPlayer.transform);
        foreach (var stageSwitch in StageSwitch)
        {
            stageSwitch.SetPlayer(_currentPlayer.transform);
        }
    }
}
