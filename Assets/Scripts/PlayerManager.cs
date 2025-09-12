using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> players;
    [SerializeField] private CameraController camControlelr;
    [SerializeField] private GameObject selectPlayerPanel;

    private List<StageSwitch> StageSwitch;
    private GameObject _currentPlayer;
    private PlayerInput inputActions;
    private bool _isHold = false;
    private Vector3 _scale = new Vector3(1.2f, 1.2f, 1.2f);
    private float _selectPanelSpeed = 4f;

    private void Awake()
    {
        inputActions = new PlayerInput();
        _currentPlayer = GameObject.FindWithTag("Player");
        StageSwitch = Object.FindObjectsByType<StageSwitch>(FindObjectsSortMode.None).ToList();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Hold.started += OnHold;
    }

    private void OnDisable()
    {
        inputActions.Player.Hold.started -= OnHold;
        inputActions.Player.Disable();
    }

    private void OnHold(InputAction.CallbackContext ctx)
    {
        _isHold = true;
    }

    private void Update()
    {
        // 키 눌렀을 때 변환
        if (_isHold)
        {
            Time.timeScale = 0.3f;

            selectPlayerPanel.SetActive(true);

            Vector3.Lerp(selectPlayerPanel.transform.localScale, _scale, _selectPanelSpeed * Time.deltaTime);

            // 키보드, 게임 패드 입력 들어와서 네비게이션으로 플레이어 선택되게 하기
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            RapidChangePlayer();
        }
    }

    private void RapidChangePlayer()
    {
        int currentIdx = players.IndexOf(_currentPlayer);

        if (currentIdx + 1 < players.Count)
        {
            var nextPlayer = players[currentIdx + 1];

            SetActivePlayer(_currentPlayer, nextPlayer);
        }
        else
        {
            var nextPlayer = players[0];

            SetActivePlayer(_currentPlayer, nextPlayer);
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
