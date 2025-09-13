using UnityEngine;
using UnityEngine.InputSystem;
public class KirbyInput : MonoBehaviour
{
    #region References
    private PlayerInput _inputs;
    private KirbyController _playerMove;
    private KirbyJump _playerJump;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _inputs = new PlayerInput();
        _playerMove = GetComponent<KirbyController>();
        _playerJump = GetComponent<KirbyJump>();
    }
    private void OnEnable()
    {
        EnableInput(); // Input System 활성화
    }

    private void OnDisable()
    {
        DisableInput(); // Input System 비활성화
    }
    #endregion

    #region Input Event Handler
    private void EnableInput()
    {
        if (_inputs == null)
        {
            return;
        }
        _inputs.Player.Enable(); // 정의한 모든 액션맵 활성화
        _inputs.Player.Move.performed += OnMoveInput;
        _inputs.Player.Move.canceled += OnMoveInput;
        _inputs.Player.Jump.performed += OnJumpClicked;
        _inputs.Player.Dash.performed += OnTurboClicked;
    }

    private void DisableInput()
    {
        if (_inputs == null)
        {
            return;
        }
        _inputs.Player.Disable(); // 모든 액션맵 비활성화
        _inputs.Player.Move.performed -= OnMoveInput;
        _inputs.Player.Move.canceled -= OnMoveInput;
        _inputs.Player.Jump.performed -= OnJumpClicked;
        _inputs.Player.Dash.performed -= OnTurboClicked;
    }

    #endregion

    void OnJumpClicked(InputAction.CallbackContext context)
    {
        _playerJump.OnJumpClicked();
    }
    void OnTurboClicked(InputAction.CallbackContext context)
    {
        _playerMove.OnTurboModePressed();
    }
    void OnMoveInput(InputAction.CallbackContext context)
    {
        if (PlayerManager.Instance.IsHold) return;

        _playerMove.OnMoveInput(context.ReadValue<Vector2>());
    }
}
