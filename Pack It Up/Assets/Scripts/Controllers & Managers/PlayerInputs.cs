using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public static PlayerInputs instance;
    public MoveBlocks currentMoveBlockScript;
    private PlayerInput playerInput;
    private Vector2 moveInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (instance == null)
        {
            instance = this;
        }
    }

    // get player inputs
    public void PlayerMovement(InputAction.CallbackContext context)
    {
        if (context.action.name == "Move")
        {
            moveInput = context.ReadValue<Vector2>();
            currentMoveBlockScript.movementX = Mathf.Ceil(moveInput.x);
            currentMoveBlockScript.movementY = Mathf.Ceil(moveInput.y);
        }
        else if (context.action.name == "Rotate")
        {
            currentMoveBlockScript.rotateInput = context.ReadValue<float>();
        }
        else if (context.action.name == "RotateDragBlock")
        {
            currentMoveBlockScript.rotateDragBlockInput = context.ReadValue<float>();
        }
    }
}
