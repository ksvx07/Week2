using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    // Hack ;Input 변경
    private int currentPlayer = 0;
    private int selectPlayer = 0;
    private bool isSelectUIActive = false;  // UI가 현재 활성화되어 있는지 여부

    [SerializeField] private List<GameObject> players;


    [SerializeField] private CameraController camControlelr;
    [SerializeField] private GameObject selectPlayerPanel;

    public GameObject _currentPlayerPrefab { get; private set; }
    private PlayerInput inputActions;

    public bool IsHold { get; private set; }
    private Vector3 _MaxScale = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private float _selectPanelSpeed = 60f;
    private Coroutine pannelActive;

    public static PlayerManager Instance;

    private void Awake()
    {
        if (null == Instance)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        inputActions = new PlayerInput();

        currentPlayer = 0;
        currentPlayer = selectPlayer;
        _currentPlayerPrefab = players[currentPlayer];
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();

        inputActions.UI.SwitchHold.performed += OnSwithPlayerHold; // 홀드키 0.5초 이상 누르면 OnSwithPlayerHold 호출
        inputActions.UI.SwitchHold.canceled += OnSwitchPlayerCancled;
        inputActions.UI.SelectPlayer.performed += ChangeSelectPlayer;
        inputActions.UI.SelectPlayer.canceled += ChangeSelectPlayer;

        inputActions.UI.QuickSwitchRight.performed += QuickSwitchPlayerRight;
        inputActions.UI.QuickSwitchLeft.performed += QuickSwitchPlayerLeft;
    }

    private void OnDisable()
    {
        inputActions.UI.SwitchHold.performed -= OnSwithPlayerHold; // 홀드키 0.2초 이상 누르면 OnSwithPlayerHold 호출
        inputActions.UI.SwitchHold.canceled -= OnSwitchPlayerCancled;
        inputActions.UI.SelectPlayer.performed -= ChangeSelectPlayer;
        inputActions.UI.SelectPlayer.canceled -= ChangeSelectPlayer;

        inputActions.UI.QuickSwitchRight.performed -= QuickSwitchPlayerRight;
        inputActions.UI.QuickSwitchLeft.performed -= QuickSwitchPlayerLeft;

        inputActions.UI.Disable();
    }

    private void ChangeSelectPlayer(InputAction.CallbackContext context)
    {
        // 선택창 활성화된 상태에서만 선택이 가능
        if (!isSelectUIActive) return;

        if(context.performed) IsHold = true;
        if (context.canceled) IsHold = false;

        Vector2 inputVector = context.ReadValue<Vector2>();
        if (inputVector == Vector2.up)         // W (Up)
        {
            selectPlayer = 0;
        }
        else if (inputVector == Vector2.right) // D (Right) // A (Left)
        {
            selectPlayer = 1;
        }
        else if (inputVector == Vector2.down)  // S (Down)
        {
            selectPlayer = 2;
        }
        else if (inputVector == Vector2.left)// A (Left)
        {
            selectPlayer = 3;
        }

        print(selectPlayer + "선택함");
    }

    private void AcitveSelectUI()
    {
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_currentPlayerPrefab.transform.position);
        selectPlayerPanel.GetComponent<RectTransform>().position = screenPosition;

        if (pannelActive != null)
        {
            StopCoroutine(pannelActive);
        }
        pannelActive = StartCoroutine(ScaleOverTime());
        selectPlayerPanel.SetActive(true);

        isSelectUIActive = true;  // UI가 현재 활성화되어 있는지 여부
    }

    private void DeActiveSelectUI()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (pannelActive != null)
        {
            StopCoroutine(pannelActive);
        }
        selectPlayerPanel.SetActive(false);
        isSelectUIActive = false;
    }


    public void OnSwithPlayerHold(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // 선택 UI가 활성화 되지 않았으면
            if (!isSelectUIActive)
            {
                // 0.2초 이상 홀드 키를 눌렀을 때, 선택 UI 활성화
                AcitveSelectUI();
            }
        }
    }

    public void OnSwitchPlayerCancled(InputAction.CallbackContext context)
    {
        // 선택창이 활성화 된 상태였다면
        if (isSelectUIActive)
        {
            //선택 UI비활성화
            DeActiveSelectUI();
            // 캐릭터 변경
            ActiveSelectPlayer(currentPlayer, selectPlayer);
        }
    }

    private void QuickSwitchPlayerRight(InputAction.CallbackContext context)
    {
        print("e 키 누름: 오른쪽 플레이어로 변경");

        // 현재 플레이어 인덱스를 1 증가시키고, 플레이어 수 이상이면 0으로 순환
        selectPlayer = (currentPlayer + 1) % players.Count;

        ActiveSelectPlayer(currentPlayer, selectPlayer);
    }
    private void QuickSwitchPlayerLeft(InputAction.CallbackContext context)
    {
        print("q 키 누름: 왼쪽 플레이어로 변경");

        // 현재 플레이어 인덱스를 1 감소시키고, 0 미만이면 마지막 인덱스로 순환
        selectPlayer = (currentPlayer - 1 + players.Count) % players.Count;

        ActiveSelectPlayer(currentPlayer, selectPlayer);
    }

    private void ActiveSelectPlayer(int oldPlayer, int newPlayer)
    {
        // 같은 캐릭터로바꾸려면 return
        if (oldPlayer == newPlayer) return;

        GameObject oldPlayerPrefab = players[oldPlayer];
        Transform lastPos = oldPlayerPrefab.transform;
        Vector2 lastVelocity = oldPlayerPrefab.GetComponent<Rigidbody2D>().linearVelocity;
        oldPlayerPrefab.SetActive(false);

        _currentPlayerPrefab = players[newPlayer];
        _currentPlayerPrefab.transform.position = lastPos.position;
        _currentPlayerPrefab.SetActive(true);
        _currentPlayerPrefab.GetComponent<IPlayerController>().OnEnableSetVelocity(lastVelocity.x, lastVelocity.y);

        currentPlayer = selectPlayer; // 인덱스 동기화
    }

    IEnumerator ScaleOverTime()
    {
        selectPlayerPanel.SetActive(true);
        selectPlayerPanel.transform.localScale = Vector3.zero;

        Vector3 initialScale = selectPlayerPanel.transform.localScale;
        float elapsedTime = 0f;

        // 경과 시간이 설정된 지속 시간보다 작을 때까지 반복
        while (elapsedTime < _selectPanelSpeed)
        {
            // 매 프레임마다 현재 플레이어의 위치를 추적
            selectPlayerPanel.transform.position = _currentPlayerPrefab.transform.position;

            // Time.deltaTime을 사용하여 경과 시간 계산 (Time.timeScale에 영향 받음)
            elapsedTime += Time.deltaTime;

            // 진행률을 0.0에서 1.0 사이로 계산
            float t = Mathf.Clamp01(elapsedTime / _selectPanelSpeed);

            // Lerp 함수로 크기를 부드럽게 보간
            selectPlayerPanel.transform.localScale = Vector3.Lerp(initialScale, _MaxScale, t);

            // 다음 프레임까지 대기
            yield return null;
        }
        // 이 시점부터는 패널의 위치만 추적하고 크기 애니메이션은 종료
        while (true)
        {
            selectPlayerPanel.transform.position = _currentPlayerPrefab.transform.position;
            yield return null;
        }
    }

}
