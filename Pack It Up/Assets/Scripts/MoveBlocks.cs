using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveBlocks : MonoBehaviour
{
    // transform variables & script variables
    public Transform parentTransform;
    private SpawnBlock spawnBlockScript;

    // text object variables
    private TextMeshProUGUI roundCounter;
    private TextMeshProUGUI linesCounter;

    // gameboard variables
    public static int width = 10;
    public static int height = 20;
    private static Transform[,] grid = new Transform[width, height]; 

    // movement variables
    private float movementX;
    private float movementY;

    // rotation variables
    public Vector2 rotationPoint;

    // fall variables
    // calculation for the speed per round: (0.8-((level-1)*0.007))^(level-1)
    public int lineClears;
    public int gameRound;
    private float defaultFallTimer;
    private float fallTimer;

    // timer variables
    private float defaultAutoMoveTimer = 0.1f;
    private float defaultAutoMoveCapTimer = 1.0f / 60.0f;
    private float autoMoveTimer;
    private float autoMoveCapTimer;
    private bool quickDrop = false;

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
        defaultFallTimer = Mathf.Pow((0.8f - ((gameRound - 1) * 0.007f)), gameRound - 1);
        print("defaultFallTimer: " + defaultFallTimer);
        fallTimer = defaultFallTimer;
        autoMoveTimer = defaultAutoMoveTimer;
        autoMoveCapTimer = defaultAutoMoveCapTimer;

        // Initialize spawn block script
        spawnBlockScript = FindFirstObjectByType<SpawnBlock>();

        // update the game round display
        roundCounter = GameObject.Find("Canvas/RoundCounter").GetComponent<TextMeshProUGUI>();
        roundCounter.text = "Round: " + gameRound;

        // update the game lines display
        roundCounter = GameObject.Find("Canvas/LinesCounter").GetComponent<TextMeshProUGUI>();
        roundCounter.text = "Lines: " + lineClears;
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
            if (!quickDrop)
                fallTimer = defaultFallTimer; // reset timer to 1 second
            if (ValidMove(0, -1))
            {
                parentTransform.Translate(Vector2.down);
            }
            // once it hits the bottom of the screen place the block
            else
            {
                // self destruct script on hitting the bottom of the screen & do update stuff
                AddToGrid();
                CheckForLines();
                spawnBlockScript.NewBlock(lineClears, gameRound);
                Destroy(this);
            }
        }

        // quick drop the block
        if (playerInput.actions["Drop"].WasReleasedThisFrame()) {
            quickDrop = true;
            fallTimer = 0;
        }
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

    // function for doing line clears
    public void CheckForLines()
    {
        for (int i = height - 1; i >= 0; i--)
        {
            // if line clear
            if (HasLine(i))
            {
                // delete the line with the line clear
                DeleteLine(i);
                
                // move the rows down
                RowDown(i);
            }
        }
    }

    // function for checking for a line clear
    public bool HasLine(int i)
    {
        // search each line for a clear & return true or false for a clear
        for (int j = 0; j < width; j++)
        {
            if (grid[j, i] == null)
                return false;
        }

        return true;
    }

    // function for deleting lines
    public void DeleteLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            // delete the game objects on the line
            Destroy(grid[j, i].gameObject);

            // update the grid
            grid[j, i] = null;
        }
        // update the line clears variable
        lineClears++;

        // update the game round if lineClears is a multiple of 10
        if ((lineClears > 0) && (lineClears % 10 == 0))
        {
            gameRound++;
        }
    }
    
    // function for moving the blocks down
    public void RowDown(int i)
    {
        // look at the y's from current line upwards
        for (int y = i; y < height; y++)
        {
            // loop through each x
            for (int j = 0; j < width; j++)
            {
                // if tile is filled with a square
                if (grid[j,y] != null)
                {
                    // find the lowest the square can move down
                    int downY = y - 1;
                    while (downY >= 0 && grid[j, downY] == null)
                    {
                        downY--;
                    }

                    // store the movement downward in a variable
                    int newY = downY + 1;
                    
                    // update the grid
                    grid[j, newY] = grid[j, y];
                    grid[j, y] = null;

                    //move the square down
                    grid[j, newY].transform.position = new Vector3(j, newY, 0);
                }
            }
        }
    }

    // check if the player's movements were valid
    public void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            // round the x and y positions
            int roundedX = Mathf.RoundToInt(children.position.x);
            int roundedY = Mathf.RoundToInt(children.position.y);

            //add the block to the grid
            grid[roundedX, roundedY] = children;
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

            // check if the block is touching any other piece
            if (grid[updatedX, updatedY] != null)
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

            // check if the block is touching any other piece
            if (grid[roundedX, roundedY] != null)
            {
                // if it is don't allow movement
                return false;
            }
        }

        // if it isn't going to move outside allow movement
        return true;
    }
}
