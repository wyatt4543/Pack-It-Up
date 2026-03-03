using UnityEngine;
using UnityEngine.InputSystem;

public class MoveBlocks : MonoBehaviour
{
    // transform variables
    public Transform parentTransform;

    //gameboard variables
    public static int width = 10;
    public static int height = 20;

    // movement variables
    private float movementX;
    private float movementY;

    // rotation variables
    public Vector2 rotationPoint;

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
        // Initialize player input
        playerInput = GetComponent<PlayerInput>();

        // Initialize timers
        fallTimer = defaultFallTimer;
        autoMoveTimer = defaultAutoMoveTimer;
        autoMoveCapTimer = defaultAutoMoveCapTimer;
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
            if ((autoMoveTimer -= Time.deltaTime) < 0) 
            {
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
        if (playerInput.actions["Rotate"].WasPressedThisFrame())
        {
            // if up key pressed
            if (rotateInput == 1) 
            {
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
                if (!ValidRotation())
                {
                    transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
                }
            }
            // if z key pressed
            else
            {
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
                if (!ValidRotation())
                {
                    transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
                }
            }
        }
        
        // move down 1 unit every second
        if ((fallTimer -= Time.deltaTime) < 0)
        {
            // move down 1 unit every second
            fallTimer = defaultFallTimer; // reset timer to 1 second
            if (ValidMove(0, -1))
            {
                parentTransform.Translate(Vector2.down);
            }
        }

        // drop the block
        //if (playerInput.actions["Drop"].WasPressedThisFrame()) {
            //parentObject.transform.position = new Vector2(parentObject.transform.position.x, parentObject.transform.position.y - boardScript.DropBlock(gameBoardX,gameBoardY));
        //    Destroy(this);
        //}

        // self destruct on hitting the bottom of the screen
        //if (boardScript.TestOutsideBottomBlock(gameBoardY, 1)) {
        //    Destroy(this);
        //}
    }

    // get player inputs
    public void PlayerMovement(InputAction.CallbackContext context)
    {
        if (context.action.name == "Move")
        {
            moveInput = context.ReadValue<Vector2>();
            movementX = Mathf.Ceil(moveInput.x);
            movementY = Mathf.Ceil(moveInput.y);
        }
        else if (context.action.name == "Rotate")
        { 
            rotateInput = context.ReadValue<float>();
        }
    }

    // move the block based on player inputs
    public void Move()
    {
        // move the block left, right, or down
        if (ValidMove((int)movementX, (int)movementY))
        {
            parentTransform.position = new Vector2(parentTransform.position.x + movementX, parentTransform.position.y + movementY);
        }
    }

    // check if the player's movements were valid
    public bool ValidMove(int xUpdate, int yUpdate = 0) {
        foreach (Transform children in transform)
        {
            // round the x and y positions
            int roundedX = Mathf.RoundToInt(children.position.x);
            int roundedY = Mathf.RoundToInt(children.position.y);

            // create the updated x and y positions
            int updatedX = roundedX + xUpdate;
            int updatedY = roundedY + yUpdate;

            // check if the block is on any side of the box
            if (updatedX < 0 || updatedX >= width || updatedY < 0 || updatedY >= height)
            {
                // if it is don't allow movement
                return false;
            }
        }

        // if it isn't going to move outside allow movement
        return true;
    }

    // check if the player's rotation was valid
    public bool ValidRotation()
    {
        foreach (Transform children in transform)
        {
            // round the x and y positions
            int roundedX = Mathf.RoundToInt(children.position.x);
            int roundedY = Mathf.RoundToInt(children.position.y);

            // check if the block is on any side of the box
            if (roundedX < 0 || roundedX >= width || roundedY < 0 || roundedY >= height)
            {
                // if it is don't allow movement
                return false;
            }
        }

        // if it isn't going to move outside allow movement
        return true;
    }
}
