using UnityEngine;
using UnityEngine.InputSystem;
public class KirbyInput : MonoBehaviour
{
    #region References
    private InputSystem_Actions _inputs;
    private KirbyController _playerMove;
    private KirbyJump _playerJump;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _inputs = new InputSystem_Actions();
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
        _inputs.Kirby.Enable(); // 정의한 모든 액션맵 활성화
        _inputs.Kirby.Move.performed += OnMoveInput;
        _inputs.Kirby.Move.canceled += OnMoveInput;
        _inputs.Kirby.Jump.performed += OnJumpClicked;
    }

    private void DisableInput()
    {
        if (_inputs == null)
        {
            return;
        }
        _inputs.Kirby.Disable(); // 모든 액션맵 비활성화
        _inputs.Kirby.Move.performed -= OnMoveInput;
        _inputs.Kirby.Move.canceled -= OnMoveInput;
        _inputs.Kirby.Jump.performed -= OnJumpClicked;
    }

    #endregion

    void OnJumpClicked(InputAction.CallbackContext context)
    {
        _playerJump.OnJumpClicked();
    }

    void OnMoveInput(InputAction.CallbackContext context)
    {
        _playerMove.OnMoveInput(context.ReadValue<Vector2>());
    }
}
