using System.Drawing;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class MovePieces : MonoBehaviour
{
    // game object and script variables
    public GameObject parentObject;
    private BoardScript boardScript;
    private GameObject gameBoard;
    
    // position variables
    public float defaultXPos;
    public float defaultYPos;
    private int gameBoardX, gameBoardY = 0;
    
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
        boardScript = gameBoard.GetComponent<BoardScript>();

        // Initialize timers
        fallTimer = defaultFallTimer;
        autoMoveTimer = defaultAutoMoveTimer;
        autoMoveCapTimer = defaultAutoMoveCapTimer;

        // Initialize postion on game board
        parentObject.transform.position = new Vector2(defaultXPos, defaultYPos);

        // put the initial left L piece game board
        boardScript.UpdateBlock(gameBoardX, gameBoardY, 1);

        //print the board
        boardScript.PrintBoard();
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
            // test if the next player postion is not off of the board
            if (!boardScript.TestOutside(gameBoardX, gameBoardY, 0, 1)) {
                // move down visually 1 unit every second
                fallTimer = defaultFallTimer; // reset timer to 1 second
                parentObject.transform.Translate(Vector2.down);

                // clear previous block position on the game board
                boardScript.UpdateBlock(gameBoardX, gameBoardY, 0);

                // update position on game board
                gameBoardY++;
                boardScript.UpdateBlock(gameBoardX, gameBoardY, 1);

                //print the board
                boardScript.PrintBoard();
            }
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
        // test if the next player postion is not off of the board
        if (!boardScript.TestOutside(gameBoardX, gameBoardY, (int)moveInput.x, (int)moveInput.y))
        {
            // find the next player position
            Vector2 nextPos = new Vector2(parentObject.transform.position.x + moveInput.x, parentObject.transform.position.y + moveInput.y);

            // update the player's visual position
            parentObject.transform.position = nextPos;

            // clear previous block position on the game board
            boardScript.UpdateBlock(gameBoardX, gameBoardY, 0);

            // update player's position on actual game board
            gameBoardX += (int)moveInput.x;
            gameBoardY += (int)moveInput.y;
            boardScript.UpdateBlock(gameBoardX, gameBoardY, 1);

            //rotation detection
        }
    }
}
