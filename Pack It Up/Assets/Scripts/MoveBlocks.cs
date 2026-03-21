using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveBlocks : MonoBehaviour
{
    // sound effect variables
    private AudioClip explosionSound;
    private AudioClip waterSound;

    // transform variables & script variables & game object variables
    public Transform parentTransform;
    private SpawnBlock spawnBlockScript;
    private GameObject gameOverObject;
    private Camera mainCamera;

    // text object variables
    private TextMeshProUGUI roundCounter;
    private TextMeshProUGUI linesCounter;
    private TextMeshProUGUI scoreCounter;

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

    // clear line animation variables
    private List<GameObject> clearedBlocks;
    private float squareSpeed = 10.0f;
    private Vector2 squareDestination = new Vector2(8.2f, -1.1f);

    // score variables
    public int gameScore;
    private int singlePlaceClears = 0;

    // cursed pieces variables
    private bool NegativeBlockCalled = false;
    private bool BoxBlockCalled = false;
    public GameObject numberDisplayPrefab;
    public Sprite[] numberDisplaySprites;
    public Sprite explosionSprite;
    public GameObject explosionObject;
    private bool placingBlock = false;

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
        // Initialize player input for various game objects
        playerInput = GetComponent<PlayerInput>();
        PauseManager.instance.playerInput = playerInput;

        // Initialize timers
        defaultFallTimer = Mathf.Pow((0.8f - ((gameRound - 1) * 0.007f)), gameRound - 1);
        fallTimer = defaultFallTimer;
        autoMoveTimer = defaultAutoMoveTimer;
        autoMoveCapTimer = defaultAutoMoveCapTimer;

        // Initialize spawn block script
        spawnBlockScript = FindFirstObjectByType<SpawnBlock>();

        // Initialize game over object
        gameOverObject = GameObject.Find("GameOver");

        // disable the game over game object
        gameOverObject.GetComponent<Renderer>().enabled = false;

        // update the game round display
        roundCounter = GameObject.Find("LevelCanvas/RoundCounter").GetComponent<TextMeshProUGUI>();
        roundCounter.text = "Round: " + gameRound;

        // update the game lines display
        linesCounter = GameObject.Find("LevelCanvas/LinesCounter").GetComponent<TextMeshProUGUI>();
        linesCounter.text = "Lines: " + lineClears;
        
        // update the game score display
        scoreCounter = GameObject.Find("LevelCanvas/ScoreCounter").GetComponent<TextMeshProUGUI>();
        scoreCounter.text = "Score: " + gameScore;

        // Initialize the camera variable
        mainCamera = Camera.main;

        //assign the sound effects
        explosionSound = Resources.Load<AudioClip>("Sounds/SFX/explosion");
        waterSound = Resources.Load<AudioClip>("Sounds/SFX/water");

        // test for a game over
        if (!ValidMove(0, -1))
        {
            EndGame();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // check if dialogue is active
        if (Dialogue.instance != null)
        {
            if (Dialogue.instance.DialogueActive) { return; }
        }

        // movement for the drag block
        if (gameObject.name == "DragBlock" && !PauseManager.instance.IsPaused)
        {
            DragBlockMove(Mathf.RoundToInt(parentTransform.position.x));
        }

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
            // disable rotation for the box block
            if (!(gameObject.name == "BoxBlock"))
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
        }
        
        // move down 1 unit every second
        if ((fallTimer -= Time.deltaTime) < 0)
        {
            // move down 1 unit every second
            if (!quickDrop)
                fallTimer = defaultFallTimer; // reset timer to 1 second
            if (ValidMove(0, -1) && !(BoxBlockCalled) && !(placingBlock))
            {
                parentTransform.Translate(Vector2.down);
            }
            // once it hits the bottom of the screen place the block
            else if (!placingBlock)
            {
                placingBlock = true;
                _ = HandleBlockPlacement();
            }
        }

        // quick drop the block
        if (playerInput.actions["Drop"].WasReleasedThisFrame()) {
            quickDrop = true;
            fallTimer = 0;
        }
    }

    private async Task HandleBlockPlacement()
    {
        // self destruct script on hitting the bottom of the screen & do update stuff
        AddToGrid();
        // function for doing special block actions
        await CursedBlocks();
        CheckForLines();
        spawnBlockScript.NewBlock(lineClears, gameRound, gameScore);
        Destroy(gameObject.GetComponent<PlayerInput>());
        Destroy(this);
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
        if (!(gameObject.name == "DragBlock"))
        {
            // move the block left, right, or down
            if (ValidMove((int)movementX, (int)movementY) && !(BoxBlockCalled))
            {
                parentTransform.position = new Vector2(parentTransform.position.x + movementX, parentTransform.position.y + movementY);
            }
        }
        else
        {
            // only allow downwards movement for the drag block
            if (ValidMove(0, (int)movementY))
            {
                parentTransform.position = new Vector2(parentTransform.position.x, parentTransform.position.y + movementY);
            }
        }
    }

    // move the block based on mouse position
    public void DragBlockMove(int previousX)
    {
        // get the current mouse position relative to the world
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 cameraDistance = new Vector3(screenPos.x, screenPos.y, 10f);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(cameraDistance);

        //check the difference between the current mouse position and the previous x position
        int mouseXMovement = Mathf.RoundToInt(worldPosition.x) - previousX;

        // update the block position if it is valid
        if (ValidMove(mouseXMovement, 0))
        {
            parentTransform.position = new Vector2(Mathf.RoundToInt(worldPosition.x), parentTransform.position.y);
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

                // animate the pieces leaving the board
                AnimateLine();
                
                // move the rows down
                RowDown(i);
            }
        }

        // increase the score by a certain amount based on the number of line clears in one placement
        if (singlePlaceClears == 1)
        {
            gameScore += 40 * gameRound;
        }
        else if (singlePlaceClears == 2)
        {
            gameScore += 100 * gameRound;
        }
        else if (singlePlaceClears == 3)
        {
            gameScore += 300 * gameRound;
        }
        else if (singlePlaceClears >= 4)
        {
            gameScore += 1200 * gameRound;
        }

        // update the single placement variable back to 0 for the next time it's used for checks
        singlePlaceClears = 0;

        // update the game round according to line clears
        gameRound = (lineClears / 10) + 1;
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
        // variable for testing for the x position of the half block;
        int halfBlockX;
        bool halfBlockExists = false;

        // search for the half block
        for (halfBlockX = 0; halfBlockX < width; halfBlockX++)
        {
            if (grid[halfBlockX, i].gameObject.name == "HalfSquare")
            {
                halfBlockExists = true;
                break;
            }
        }

        // set the normal values
        int testWidth = width;
        int startX = 0;

        if (halfBlockExists)
        {
            // if the half block is on the left side only clear the left side
            if (halfBlockX < 5)
            {
                testWidth = 5;
            }
            // otherwise clear the right side
            else
            {
                startX = 5;
            }
        }

        for (int j = startX; j < testWidth; j++)
        {
            // testing for a square with a number on it
            if (grid[j, i].gameObject.transform.childCount > 0)
            {
                if (grid[j, i].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name[0] == '2')
                {
                    // destroy the number overlay if it is at 2
                    Destroy(grid[j, i].gameObject.transform.GetChild(0).gameObject);
                }
                // if the overlay is higher than two
                else
                {
                    // get the value of the number display
                    int numberDisplayValue = (int)Char.GetNumericValue(grid[j, i].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name[0]);

                    // decrement the display value
                    numberDisplayValue--;

                    // update the display's sprite
                    grid[j, i].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = numberDisplaySprites[numberDisplayValue-2];
                }
            }
            // if it is a normal square do the normal line deletion
            else
            {
                // clone the cleared block
                GameObject clearedBlock = Instantiate(grid[j, i].gameObject, grid[j, i].gameObject.transform.position, Quaternion.identity);

                // shrink the cleared block
                clearedBlock.transform.localScale = new Vector3(0.5f, 0.5f, 1);

                // add the cleared block to the list of cleared blocks
                clearedBlocks.Add(grid[j, i].gameObject);

                // delete the game objects on the line
                Destroy(grid[j, i].gameObject);

                // update the grid
                grid[j, i] = null;
            }
        }
        
        // update the line clears variable
        lineClears++;

        // update the variable keeping track of the line clears made by one block
        singlePlaceClears++;
    }


    // function for animating the blocks leaving the board
    private async void AnimateLine()
    {
        for (int i = 0; i < clearedBlocks.Count; i++)
        {
            // check if the distance to the destination of the bottom box is really close or not
            while (Vector3.Distance(clearedBlocks[i].transform.position, squareDestination) > 0.01f)
            {
                // move the individual square
                clearedBlocks[i].transform.position = Vector3.MoveTowards(clearedBlocks[i].transform.position, squareDestination, squareSpeed * Time.deltaTime);

                // wait until the square is in place
                await Task.Yield();
            }

            // stop the square
            clearedBlocks[i].transform.position = squareDestination;

            // remove the square from the list of cleared blocks
            clearedBlocks.RemoveAt(i);
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

                    // check if the square actually moves down
                    if (newY != y)
                    {
                        // update the grid
                        grid[j, newY] = grid[j, y];
                        grid[j, y] = null;

                        //move the square down
                        grid[j, newY].transform.position = new Vector3(j, newY, 0);
                    }
                }
            }
        }
    }

    // add the player's block to the grid
    public void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            if (children.name != "BoxFlap")
            {
                // round the x and y positions
                int roundedX = Mathf.RoundToInt(children.position.x);
                int roundedY = Mathf.RoundToInt(children.position.y);

                //add the block to the grid
                grid[roundedX, roundedY] = children;
            }
        }
    }

    // check if the player's movements were valid
    public bool ValidMove(int xUpdate, int yUpdate = 0) {
        // functionality for the Negative Block
        if (gameObject.name == "JNegativeBlock")
        {
            int overlapCount = 0;
            foreach (Transform children in transform)
            {
                // round the x and y positions
                int roundedX = Mathf.RoundToInt(children.position.x);
                int roundedY = Mathf.RoundToInt(children.position.y);

                // create the updated x and y positions
                int updatedX = roundedX + xUpdate;
                int updatedY = roundedY + yUpdate;

                // check if the block is on any side of the box
                if (updatedX < 0 || updatedX >= width || updatedY >= height)
                {
                    // if it is don't allow movement
                    return false;
                }

                // activate the negative block if it is at the bottom of the screen
                if (updatedY < 0)
                {
                    NegativeBlockDestruction();
                }

                // check if all blocks are touching another piece
                if (grid[roundedX, roundedY] != null)
                {
                    // increment the overlapping negative blocks count if the negative square is overlapping another piece
                    overlapCount++;
                }
            }

            // if all of the pieces of the negative block overlap with a normal piece
            if (overlapCount == gameObject.transform.childCount)
            {
                // remove the pieces the negative piece overlaps with
                NegativeBlockDestruction();
            }
        }
        // functionality for the Box Block
        else if (gameObject.name == "BoxBlock")
        {
            foreach (Transform children in transform)
            {
                if (children.name != "BoxFlap")
                {
                    // round the x and y positions
                    int roundedX = Mathf.RoundToInt(children.position.x);
                    int roundedY = Mathf.RoundToInt(children.position.y);

                    // create the updated x and y positions
                    int updatedX = roundedX + xUpdate;
                    int updatedY = roundedY + yUpdate;

                    // check if the block is on any side of the box
                    if (updatedX < 0 || updatedX >= width || updatedY >= height)
                    {
                        // if it is don't allow movement
                        return false;
                    }

                    // activate the box block if it is at the bottom of the screen
                    if (updatedY < 0)
                    {
                        return BoxBlockDestruction();
                    }

                    // check for an overlap square
                    if (children.name == "OverlapSquare")
                    {
                        // check if the overlap blocks are touching another piece
                        if (grid[roundedX, roundedY] != null)
                        {
                            // activate the box block if the box square is overlapping another piece
                            return BoxBlockDestruction();
                        }
                    }
                    else
                    {
                        // make the box only do this check if it is moved && check if the square is overlapping another piece
                        if (xUpdate != 0 && grid[roundedX, roundedY] != null)
                        {
                            // activate the box block if the box is overlapping another piece
                            return BoxBlockDestruction();
                        }
                    }
                }
            }
        }
        else
        {
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
        }

        // allow movement if no collison or getting out of bounds
        return true;
    }

    // check if the player's rotation was valid
    public bool ValidRotation()
    {
        // functionality for the Negative Block
        if (gameObject.name == "JNegativeBlock")
        {
            int overlapCount = 0;
            foreach (Transform children in transform)
            {
                // round the x and y positions
                int roundedX = Mathf.RoundToInt(children.position.x);
                int roundedY = Mathf.RoundToInt(children.position.y);

                // check if the block is on any side of the box
                if (roundedX < 0 || roundedX >= width || roundedY >= height)
                {
                    // if it is don't allow movement
                    return false;
                }

                // activate the negative block if it is at the bottom of the screen
                if (roundedY < 0)
                {
                    NegativeBlockDestruction();
                }

                // check if all blocks are touching another piece
                if (grid[roundedX, roundedY] != null)
                {
                    // increment the overlapping negative blocks count if the negative square is overlapping another piece
                    overlapCount++;
                }
            }

            // if all of the pieces of the negative block overlap with a normal piece
            if (overlapCount == gameObject.transform.childCount)
            {
                // remove the pieces the negative piece overlaps with
                NegativeBlockDestruction();
            }
        }
        else
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
        }

        // if it isn't going to move outside allow movement
        return true;
    }

    // check the type of block & do its ability
    private async Task CursedBlocks()
    {
        // functionality for the bomb block
        if (gameObject.name == "BombBlock")
        {
            // round the current x and y positions
            int roundedX = Mathf.RoundToInt(parentTransform.position.x);
            int roundedY = Mathf.RoundToInt(parentTransform.position.y);

            // play the explosion animation for the bomb
            ExplosionAnimation(grid[roundedX, roundedY].gameObject.transform.GetComponent<SpriteRenderer>());

            // wait 100 milliseconds
            await Task.Delay(100);

            // hide the bomb
            grid[roundedX, roundedY].gameObject.transform.GetComponent<SpriteRenderer>().enabled = false;

            // check each y position around the bomb
            for (int y = 1; y >= -1; y--)
            {
                // check each x position around the bomb
                for (int x = -1; x <= 1; x++)
                {
                    // make the x and y to check
                    int checkX = roundedX + x;
                    int checkY = roundedY + y;

                    // if the square is trying to delete out of bounds
                    if (!(checkX < 0 || checkX >= width || checkY < 0 || checkY >= height))
                    {
                        // check if the square is empty & if trying to delete the current game object
                        if ((grid[checkX, checkY] != null) && !(x == 0 && y == 0))
                        {
                            // testing for a square with a number on it
                            if (grid[checkX, checkY].gameObject.transform.childCount > 0)
                            {
                                // play the explosion animation
                                GameObject TempExplosion = ExplosionAnimation(null, checkX, checkY, true);

                                if (grid[checkX, checkY].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name[0] == '2')
                                {
                                    // destroy the number overlay if it is at 2
                                    Destroy(grid[checkX, checkY].gameObject.transform.GetChild(0).gameObject);
                                }
                                // if the overlay is higher than two
                                else
                                {
                                    // get the value of the number display
                                    int numberDisplayValue = (int)Char.GetNumericValue(grid[checkX, checkY].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name[0]);

                                    // decrement the display value
                                    numberDisplayValue--;

                                    // update the display's sprite
                                    grid[checkX, checkY].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = numberDisplaySprites[numberDisplayValue - 2];
                                }

                                // wait 100 milliseconds
                                await Task.Delay(100);

                                // destroy the explosion game object
                                Destroy(TempExplosion);
                            }
                            // if it is a normal square do the normal bomb deletion
                            else
                            {
                                // play the explosion animation
                                ExplosionAnimation(grid[checkX, checkY].gameObject.transform.GetComponent<SpriteRenderer>());

                                // wait 100 milliseconds
                                await Task.Delay(100);

                                // destroy the game object around the bomb
                                Destroy(grid[checkX, checkY].gameObject);

                                // update the grid
                                grid[checkX, checkY] = null;
                            }

                            // give the player 10 points (scaling with each game round) for each square cleared
                            gameScore += 10 * gameRound;
                        }
                    }
                }
            }

            // update the grid
            grid[roundedX, roundedY] = null;

            // destroy the bomb block
            Destroy(transform.parent.gameObject);
        }

        // functionality for the water block
        if (gameObject.name == "JWaterBlock")
        {
            // look at the y's from the bottom line upwards
            for (int y = 0; y < height; y++)
            {
                // loop through each x
                for (int j = 0; j < width; j++)
                {
                    // if tile is filled with a square
                    if (grid[j, y] != null && grid[j, y].transform.parent.name == "JWaterBlock")
                    {
                        // find the lowest the square can move down
                        int downY = y - 1;
                        while (downY >= 0 && grid[j, downY] == null)
                        {
                            downY--;
                        }

                        // store the movement downward in a variable
                        int newY = downY + 1;

                        // check if the square actually moves down
                        if (newY != y)
                        {
                            // add a delay for until the next water falls down
                            await Task.Delay(300);

                            // update the grid
                            grid[j, newY] = grid[j, y];
                            grid[j, y] = null;

                            //move the square down
                            grid[j, newY].transform.position = new Vector3(j, newY, 0);

                            // play the water sound effect
                            SFXManager.instance.PlaySFXClip(waterSound, parentTransform, 1f);
                        }
                    }
                }
            }
        }

        // functionality for the gravel block
        if (gameObject.name == "GravelBlock")
        {
            // create variables for the maximum x and y of the gravel block
            int minX = Mathf.RoundToInt(transform.GetChild(0).position.x);
            int minY = Mathf.RoundToInt(transform.GetChild(0).position.y);
            int maxX = 0;
            int maxY = 0;

            // look for the maximum and minimum x and y of the block
            foreach (Transform children in transform)
            {
                if (Mathf.RoundToInt(children.position.x) < minX)
                {
                    minX = Mathf.RoundToInt(children.position.x);
                }
                if (Mathf.RoundToInt(children.position.y) < minY)
                {
                    minY = Mathf.RoundToInt(children.position.y);
                }
                if (Mathf.RoundToInt(children.position.x) > maxX)
                {
                    maxX = Mathf.RoundToInt(children.position.x);
                }
                if (Mathf.RoundToInt(children.position.y) > maxY)
                {
                    maxY = Mathf.RoundToInt(children.position.y);
                }
            }

            // update the maximum and minimum x and y for modifying the squares around the gravel block
            if (minX - 1 >= 0)
            {
                minX--;
            }
            if (minY - 1 >= 0)
            {
                minY--;
            }
            if (maxX + 1 < width)
            {
                maxX++;
            }
            if (maxY + 1 < height)
            {
                maxY++;
            }

            // look at the gravel's bottom y upwards
            for (int y = minY; y <= maxY; y++)
            {
                // loop through each x that's part of and around the gravel
                for (int j = minX; j <= maxX; j++)
                {
                    // if tile is filled with a square
                    if (grid[j, y] != null)
                    {
                        // find the lowest the square can move down
                        int downY = y - 1;
                        while (downY >= 0 && grid[j, downY] == null)
                        {
                            downY--;
                        }

                        // store the movement downward in a variable
                        int newY = downY + 1;

                        // check if the square actually moves down
                        if (newY != y)
                        {
                            // update the grid
                            grid[j, newY] = grid[j, y];
                            grid[j, y] = null;

                            //move the square down
                            grid[j, newY].transform.position = new Vector3(j, newY, 0);
                        }
                    }
                }
            }
        }
    }

    // function for destorying blocks that the negative block overlaps with
    private void NegativeBlockDestruction()
    {
        // check if this function has been called
        if (NegativeBlockCalled == false)
        {   
            // stop this from spawning multiple blocks
            NegativeBlockCalled = true;
            
            // loop through each child to destroy all of the correct blocks
            foreach (Transform children in transform)
            {
                // round the current child's x and y positions
                int roundedX = Mathf.RoundToInt(children.transform.position.x);
                int roundedY = Mathf.RoundToInt(children.transform.position.y);

                // check if there is actually a piece there
                if (grid[roundedX, roundedY] != null)
                {
                    // delete the game objects overlapping with a piece
                    Destroy(grid[roundedX, roundedY].gameObject);

                    // update the grid
                    grid[roundedX, roundedY] = null;
                }
            }

            // spawn a new piece
            spawnBlockScript.NewBlock(lineClears, gameRound, gameScore);

            // destroy the negative block
            Destroy(transform.parent.gameObject);
        }
    }

    // function for playing the explosion animation
    private GameObject ExplosionAnimation(SpriteRenderer blockSpriteRenderer, int checkX = 0, int checkY = 0, bool numberedBlock = false)
    {
        if (numberedBlock)
        {
            // update the block texture to be the explosion
            GameObject TempExplosion = Instantiate(explosionObject, grid[checkX, checkY].gameObject.transform.position, Quaternion.identity);

            // play the explosion sound effect
            SFXManager.instance.PlaySFXClip(explosionSound, parentTransform, 1f);

            // return the explosion object
            return TempExplosion;
        }
        else
        {
            // update the block texture to be the explosion
            blockSpriteRenderer.sprite = explosionSprite;

            // play the explosion sound effect
            SFXManager.instance.PlaySFXClip(explosionSound, parentTransform, 1f);
        }

        // return nothing
        return null;
    }

    // function for incrementing blocks that the box block overlaps with
    private bool BoxBlockDestruction()
    {
        // check if this function has been called
        if (BoxBlockCalled == false)
        {
            // stop this from spawning multiple blocks
            BoxBlockCalled = true;

            // loop through each child to destroy all of the correct blocks
            foreach (Transform children in transform)
            {
                if (children.name != "BoxFlap")
                {
                    // round the current child's x and y positions
                    int roundedX = Mathf.RoundToInt(children.transform.position.x);
                    int roundedY = Mathf.RoundToInt(children.transform.position.y);

                    // check if there is actually a piece there
                    if (grid[roundedX, roundedY] != null)
                    {
                        // create the value for the number display
                        int numberDisplayValue = 2;

                        // testing for a square with a number on it
                        if (grid[roundedX, roundedY].gameObject.transform.childCount > 0)
                        {
                            // get the value of the number display
                            numberDisplayValue = (int)Char.GetNumericValue(grid[roundedX, roundedY].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name[0]);

                            // check if the number display value is at its maximum
                            if (numberDisplayValue != 5)
                            {
                                // increment the display value
                                numberDisplayValue++;
                            }
                        }
                        // delete the game objects overlapping with a piece
                        Destroy(grid[roundedX, roundedY].gameObject);

                        // update the grid
                        grid[roundedX, roundedY] = null;

                        // create the number display object
                        GameObject numberDisplayBox = Instantiate(numberDisplayPrefab, new Vector2(roundedX, roundedY), Quaternion.identity);

                        // assign the number display object's number
                        numberDisplayBox.GetComponent<SpriteRenderer>().sprite = numberDisplaySprites[numberDisplayValue - 2];

                        // assign the number display's parent
                        numberDisplayBox.transform.parent = children.transform;


                    }
                }
                else
                {
                    // delete the box flaps
                    Destroy(children.gameObject);
                }
            }
        }

        // stop movement
        return false;
    }

    // end the game upon loss
    private void EndGame()
    {
        // show the game over object
        gameOverObject.GetComponent<Renderer>().enabled = true;

        // hide the text
        roundCounter.enabled = false;
        linesCounter.enabled = false;

        // show the restart button & home button
        buttonUI.instance.ToggleButton(0);
        buttonUI.instance.ToggleButton(1);

        // delete this script to disable movement
        Destroy(this);
    }
}
