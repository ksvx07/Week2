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
        EnableInput(); // Input System Ȱ��ȭ
    }

    private void OnDisable()
    {
        DisableInput(); // Input System ��Ȱ��ȭ
    }
    #endregion

    #region Input Event Handler
    private void EnableInput()
    {
        if (_inputs == null)
        {
            return;
        }
        _inputs.Kirby.Enable(); // ������ ��� �׼Ǹ� Ȱ��ȭ
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
        _inputs.Kirby.Disable(); // ��� �׼Ǹ� ��Ȱ��ȭ
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
