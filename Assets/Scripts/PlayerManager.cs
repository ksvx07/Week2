using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Progress;

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
    private Vector3 _originScale = Vector3.zero;
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
            
        inputActions.UI.QuckSwitch.performed += QuickSwitchPlayer;
    }

    private void OnDisable()
    {
        inputActions.UI.SwitchHold.performed -= OnSwithPlayerHold; // 홀드키 0.5초 이상 누르면 OnSwithPlayerHold 호출
        inputActions.UI.SwitchHold.canceled -= OnSwitchPlayerCancled;
        inputActions.UI.SelectPlayer.performed -= ChangeSelectPlayer;

        inputActions.UI.QuckSwitch.performed -= QuickSwitchPlayer;

        inputActions.UI.Disable();
    }

    private void ChangeSelectPlayer(InputAction.CallbackContext context)
    {
        print("선택창 활성화 여부 체크 중");
        // 선택창 활성화된 상태에서만 선택이 가능
        if (!isSelectUIActive) return;
        print("선택창 활성화 됨");

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
        pannelActive =StartCoroutine(ScalePanel(selectPlayerPanel.transform, _originScale, _MaxScale, _selectPanelSpeed));
        selectPlayerPanel.SetActive(true);

        isSelectUIActive = true;  // UI가 현재 활성화되어 있는지 여부
}

private void DeActiveSelectUI()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        selectPlayerPanel.SetActive(false);
        isSelectUIActive = false;
    }


    public void OnSwithPlayerHold(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // 1초 이상 Shift 키를 눌렀을 때, 선택 UI 활성화
            AcitveSelectUI();
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



    private void QuickSwitchPlayer(InputAction.CallbackContext context)
    {
        print("변경키 짧게 누름: 바로변경");

        if (currentPlayer + 1 < players.Count)
        {
            selectPlayer++;
        }
        else
        {
            selectPlayer = 0;
        }
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

    IEnumerator ScalePanel(Transform targetTransform, Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;

        // 경과 시간이 설정한 지속 시간보다 작을 때까지 루프를 실행합니다.
        while (elapsedTime < duration)
        {
            // t 값은 0부터 1까지 부드럽게 증가하는 비율입니다.
            // 이 값이 Lerp 함수의 마지막 인자로 사용됩니다.
            float t = elapsedTime / duration;

            // Vector3.Lerp를 사용하여 시작 크기에서 최종 크기로 보간합니다.
            targetTransform.localScale = Vector3.Lerp(startScale, endScale, t);

            // 경과 시간을 업데이트합니다. Time.deltaTime은 이전 프레임으로부터의 시간입니다.
            elapsedTime += Time.unscaledDeltaTime;

            // 다음 프레임까지 기다립니다.
            yield return null;
        }

        // 루프가 끝난 후, 최종 크기를 정확하게 설정하여 오차를 방지합니다.
        targetTransform.localScale = endScale;
    }
}
