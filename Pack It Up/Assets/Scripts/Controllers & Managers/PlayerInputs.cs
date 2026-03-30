using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public static PlayerInputs instance;
    public MoveBlocks currentMoveBlockScript;
    public PlayerInput playerInput;

    // input variables
    private Vector2 moveInput;

    // timer variables
    private float defaultFallTimer;
    private float fallTimer;
    private float defaultAutoMoveTimer = 0.1f;
    private float defaultAutoMoveCapTimer = 1.0f / 60.0f;
    private float autoMoveTimer;
    private float autoMoveCapTimer;
    private bool quickDrop = false;

    private void Awake()
    {
        // Initialize timers
        defaultFallTimer = Mathf.Pow((0.8f - ((currentMoveBlockScript.gameRound - 1) * 0.007f)), currentMoveBlockScript.gameRound - 1);
        fallTimer = defaultFallTimer;
        autoMoveTimer = defaultAutoMoveTimer;
        autoMoveCapTimer = defaultAutoMoveCapTimer;

        playerInput = GetComponent<PlayerInput>();

        if (instance == null)
        {
            instance = this;
        }
    }

    private void Update()
    {
        // normal move left and right
        if (playerInput.actions["Move"].WasPressedThisFrame())
        {
            currentMoveBlockScript.Move();
        }

        // test for auto move
        if (playerInput.actions["Move"].IsPressed() && !(playerInput.actions["Move"].WasPressedThisFrame()))
        {
            // test if enough time has passed for automove
            if ((autoMoveTimer -= Time.deltaTime) < 0)
            {
                // cap the automovement to 60 movements per second
                if ((autoMoveCapTimer -= Time.deltaTime) < 0)
                {
                    autoMoveCapTimer = defaultAutoMoveCapTimer; // reset timer to 1/60 of a second
                    currentMoveBlockScript.Move();
                }
            }
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
