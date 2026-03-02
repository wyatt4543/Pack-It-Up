using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class MovePieces : MonoBehaviour
{
    // game object and script variables
    public GameObject parentObject;
    private ClearScript clearScript;
    private GameObject gameBoard;
    public float defaultXPos;
    public float defaultYPos;
    private float edgeOfBoardX;
    private float edgeOfBoardY;
    
    // timer variables
    private float defaultFallTimer = 1.5f;
    private float defaultAutoMoveTimer = 0.1f;
    private float defaultAutoMoveCapTimer = 1.0f / 60.0f;
    private float fallTimer;
    private float autoMoveTimer;
    private float autoMoveCapTimer;

    // input variables
    private Vector2 moveInput;
    private float rotateInput;
    private PlayerInput playerInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize object variables
        playerInput = GetComponent<PlayerInput>();
        gameBoard = GameObject.Find("Background");
        clearScript = gameBoard.GetComponent<ClearScript>();

        // Initialize timers
        fallTimer = defaultFallTimer;
        autoMoveTimer = defaultAutoMoveTimer;
        autoMoveCapTimer = defaultAutoMoveCapTimer;

        parentObject.transform.position = new Vector2(defaultXPos, defaultYPos);

        edgeOfBoardX = Mathf.Abs(transform.position.x);
        edgeOfBoardY = Mathf.Abs(transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        // normal move left and right
        if (playerInput.actions["Move"].WasPressedThisFrame())
        {
            Move();
        }

        // test for auto move
        if (playerInput.actions["Move"].IsPressed() && !(playerInput.actions["Move"].WasPressedThisFrame()))
        {
            // test if enough time has passed for automove
            if ((autoMoveTimer -= Time.deltaTime) < 0) {
                // cap the automovement to 60 movements per second
                if ((autoMoveCapTimer -= Time.deltaTime) < 0)
                {
                    autoMoveCapTimer = defaultAutoMoveCapTimer; // reset timer to 1/60 of a second
                    Move();
                }
            }
        }

        // reset auto move detection
        if (playerInput.actions["Move"].WasReleasedThisFrame()) {
            autoMoveTimer = defaultAutoMoveTimer; // reset to 0.1 second
        }

        // rotate the block
        if (playerInput.actions["Rotate"].WasPressedThisFrame()) {
            if (rotateInput == 1) {
                transform.Rotate(0, 0, 90);
            } else {
                transform.Rotate(0, 0, -90);
            }
        }

        // move down 1 unit every second
        if ((fallTimer -= Time.deltaTime) < 0)
        {
            fallTimer = defaultFallTimer; // reset timer to 1 second
            parentObject.transform.Translate(Vector2.down);
        }

        // drop the block
        if (playerInput.actions["Drop"].WasPressedThisFrame()) {
            parentObject.transform.position = new Vector2(parentObject.transform.position.x, -10f);
            Destroy(this);
        }

        // self destruct on hitting the bottom of the screen
        if (parentObject.transform.position.y == -10f) {
            Destroy(this);
        }
    }

    // get player inputs
    public void PlayerMovement(InputAction.CallbackContext context)
    {
        if (context.action.name == "Move")
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.action.name == "Rotate")
        { 
            rotateInput = context.ReadValue<float>();
        }
    }

    // move the block based on player inputs
    public void Move()
    {
        // find the next player position
        Vector2 nextPos = new Vector2(parentObject.transform.position.x + moveInput.x, parentObject.transform.position.y + moveInput.y);

        // if the next player position is not off the board update the player position
        if (Mathf.Abs(nextPos.x) <= edgeOfBoardX)
        {
            parentObject.transform.position = nextPos;
        }
        else if (Mathf.Abs(nextPos.y) <= edgeOfBoardY && Mathf.Abs(transform.rotation.z) == 90) {
            parentObject.transform.position = nextPos;
        }
    }
}
