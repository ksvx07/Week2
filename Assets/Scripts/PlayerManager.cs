using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> players;
    [SerializeField] private CameraController camControlelr;
    [SerializeField] private GameObject selectPlayerPanel;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    private List<StageSwitch> StageSwitch;
    private GameObject _currentPlayer;
    private PlayerInput inputActions;

    private bool _isHold = false;
    private Vector2 _inputNavi = Vector2.zero;
    private Vector3 _originScale = Vector3.zero;
    private Vector3 _MaxScale = new Vector3(1.2f, 1.2f, 1.2f);
    private float _selectPanelSpeed = 10f;

    private Button _currentButton;

    private void Awake()
    {
        inputActions = new PlayerInput();
        _currentPlayer = GameObject.FindWithTag("Player");
        StageSwitch = FindObjectsByType<StageSwitch>(FindObjectsSortMode.None).ToList();

        upButton.onClick.AddListener(() => { SetActivePlayer(_currentPlayer, players.Find(x => x.name == upButton.name)); });
        downButton.onClick.AddListener(() => { SetActivePlayer(_currentPlayer, players.Find(x => x.name == downButton.name)); });
        leftButton.onClick.AddListener(() => { SetActivePlayer(_currentPlayer, players.Find(x => x.name == leftButton.name)); });
        rightButton.onClick.AddListener(() => { SetActivePlayer(_currentPlayer, players.Find(x => x.name == rightButton.name)); });
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.UI.Enable();

        inputActions.Player.Hold.started += OnHold;
        inputActions.Player.Hold.canceled += OnHold;
        inputActions.UI.Move.started += OnSelectPlayer;
        inputActions.UI.Move.canceled += OnSelectPlayer;
    }

    private void OnDisable()
    {
        inputActions.Player.Hold.started -= OnHold;
        inputActions.Player.Hold.canceled -= OnHold;
        inputActions.UI.Move.started -= OnSelectPlayer;
        inputActions.UI.Move.canceled -= OnSelectPlayer;

        inputActions.Player.Disable();
        inputActions.UI.Disable();
    }

    private void OnHold(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            _isHold = true;
        else if (ctx.canceled)
        {
            _isHold = false;

            // 홀드 상태 취소되면, 그 선택되어있던 버튼 클릭 invoke
            if(_currentButton  != null) _currentButton.onClick.Invoke();
        }
    }

    private void OnSelectPlayer(InputAction.CallbackContext ctx)
    {
        _inputNavi = ctx.ReadValue<Vector2>();
    }

    private void Highlight(Button button)
    {
        if (_currentButton == button) return;

        if (_currentButton != null) _currentButton.OnDeselect(null);

        _currentButton = button;
        _currentButton.Select();
    }

    private void Update()
    {
        // 키 눌렀을 때 변환
        if (_isHold)
        {
            Time.timeScale = 0.3f;

            if(!selectPlayerPanel.activeSelf) selectPlayerPanel.SetActive(true);
            selectPlayerPanel.transform.position = _currentPlayer.transform.position;
            selectPlayerPanel.transform.localScale = Vector3.Lerp(selectPlayerPanel.transform.localScale, _MaxScale, _selectPanelSpeed * Time.deltaTime);

            // 키보드, 게임 패드 입력 들어와서 네비게이션으로 플레이어 선택되게 하기
            if (_inputNavi == Vector2.zero) return;

            if(Mathf.Abs(_inputNavi.x) > Mathf.Abs(_inputNavi.y))
            {
                if (_inputNavi.x > 0) Highlight(rightButton);
                else Highlight(leftButton);
            }
            else
            {
                if(_inputNavi.y > 0) Highlight(upButton);
                else Highlight(downButton);
            }

        }
        else
        {
            Time.timeScale = 1f;

            if(selectPlayerPanel.activeSelf) selectPlayerPanel.SetActive(false);
            selectPlayerPanel.transform.localScale = Vector3.Lerp(selectPlayerPanel.transform.localScale, _originScale, _selectPanelSpeed * Time.deltaTime);

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
