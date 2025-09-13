using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    // Hack ;Input ����
    private int currentPlayer = 0;
    private int selectPlayer = 0;
    private bool isSelectUIActive = false;  // UI�� ���� Ȱ��ȭ�Ǿ� �ִ��� ����

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

        inputActions.UI.SwitchHold.performed += OnSwithPlayerHold; // Ȧ��Ű 0.5�� �̻� ������ OnSwithPlayerHold ȣ��
        inputActions.UI.SwitchHold.canceled += OnSwitchPlayerCancled;
        inputActions.UI.SelectPlayer.performed += ChangeSelectPlayer;
        inputActions.UI.SelectPlayer.canceled += ChangeSelectPlayer;

        inputActions.UI.QuickSwitchRight.performed += QuickSwitchPlayerRight;
        inputActions.UI.QuickSwitchLeft.performed += QuickSwitchPlayerLeft;
    }

    private void OnDisable()
    {
        inputActions.UI.SwitchHold.performed -= OnSwithPlayerHold; // Ȧ��Ű 0.2�� �̻� ������ OnSwithPlayerHold ȣ��
        inputActions.UI.SwitchHold.canceled -= OnSwitchPlayerCancled;
        inputActions.UI.SelectPlayer.performed -= ChangeSelectPlayer;
        inputActions.UI.SelectPlayer.canceled -= ChangeSelectPlayer;

        inputActions.UI.QuickSwitchRight.performed -= QuickSwitchPlayerRight;
        inputActions.UI.QuickSwitchLeft.performed -= QuickSwitchPlayerLeft;

        inputActions.UI.Disable();
    }

    private void ChangeSelectPlayer(InputAction.CallbackContext context)
    {
        // ����â Ȱ��ȭ�� ���¿����� ������ ����
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

        print(selectPlayer + "������");
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

        isSelectUIActive = true;  // UI�� ���� Ȱ��ȭ�Ǿ� �ִ��� ����
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
            // ���� UI�� Ȱ��ȭ ���� �ʾ�����
            if (!isSelectUIActive)
            {
                // 0.2�� �̻� Ȧ�� Ű�� ������ ��, ���� UI Ȱ��ȭ
                AcitveSelectUI();
            }
        }
    }

    public void OnSwitchPlayerCancled(InputAction.CallbackContext context)
    {
        // ����â�� Ȱ��ȭ �� ���¿��ٸ�
        if (isSelectUIActive)
        {
            //���� UI��Ȱ��ȭ
            DeActiveSelectUI();
            // ĳ���� ����
            ActiveSelectPlayer(currentPlayer, selectPlayer);
        }
    }

    private void QuickSwitchPlayerRight(InputAction.CallbackContext context)
    {
        print("e Ű ����: ������ �÷��̾�� ����");

        // ���� �÷��̾� �ε����� 1 ������Ű��, �÷��̾� �� �̻��̸� 0���� ��ȯ
        selectPlayer = (currentPlayer + 1) % players.Count;

        ActiveSelectPlayer(currentPlayer, selectPlayer);
    }
    private void QuickSwitchPlayerLeft(InputAction.CallbackContext context)
    {
        print("q Ű ����: ���� �÷��̾�� ����");

        // ���� �÷��̾� �ε����� 1 ���ҽ�Ű��, 0 �̸��̸� ������ �ε����� ��ȯ
        selectPlayer = (currentPlayer - 1 + players.Count) % players.Count;

        ActiveSelectPlayer(currentPlayer, selectPlayer);
    }

    private void ActiveSelectPlayer(int oldPlayer, int newPlayer)
    {
        // ���� ĳ���ͷιٲٷ��� return
        if (oldPlayer == newPlayer) return;

        GameObject oldPlayerPrefab = players[oldPlayer];
        Transform lastPos = oldPlayerPrefab.transform;
        Vector2 lastVelocity = oldPlayerPrefab.GetComponent<Rigidbody2D>().linearVelocity;
        oldPlayerPrefab.SetActive(false);

        _currentPlayerPrefab = players[newPlayer];
        _currentPlayerPrefab.transform.position = lastPos.position;
        _currentPlayerPrefab.SetActive(true);
        _currentPlayerPrefab.GetComponent<IPlayerController>().OnEnableSetVelocity(lastVelocity.x, lastVelocity.y);

        currentPlayer = selectPlayer; // �ε��� ����ȭ
    }

    IEnumerator ScaleOverTime()
    {
        selectPlayerPanel.SetActive(true);
        selectPlayerPanel.transform.localScale = Vector3.zero;

        Vector3 initialScale = selectPlayerPanel.transform.localScale;
        float elapsedTime = 0f;

        // ��� �ð��� ������ ���� �ð����� ���� ������ �ݺ�
        while (elapsedTime < _selectPanelSpeed)
        {
            // �� �����Ӹ��� ���� �÷��̾��� ��ġ�� ����
            selectPlayerPanel.transform.position = _currentPlayerPrefab.transform.position;

            // Time.deltaTime�� ����Ͽ� ��� �ð� ��� (Time.timeScale�� ���� ����)
            elapsedTime += Time.deltaTime;

            // ������� 0.0���� 1.0 ���̷� ���
            float t = Mathf.Clamp01(elapsedTime / _selectPanelSpeed);

            // Lerp �Լ��� ũ�⸦ �ε巴�� ����
            selectPlayerPanel.transform.localScale = Vector3.Lerp(initialScale, _MaxScale, t);

            // ���� �����ӱ��� ���
            yield return null;
        }
        // �� �������ʹ� �г��� ��ġ�� �����ϰ� ũ�� �ִϸ��̼��� ����
        while (true)
        {
            selectPlayerPanel.transform.position = _currentPlayerPrefab.transform.position;
            yield return null;
        }
    }

}
