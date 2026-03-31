using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MoveBlocks : MonoBehaviour
{
    // make this script static
    public static MoveBlocks instance;

    // sound effect variables
    private AudioClip explosionSound;
    private AudioClip waterSound;
    private AudioClip gravelSound;
    private AudioClip placeSound;
    private AudioClip lineClearSound;
    private AudioClip gameOverSound;
    private AudioClip fanfareSound;

    // transform variables & script variables & game object variables
    public Transform parentTransform;
    private SpawnBlock spawnBlockScript;
    private GameObject gameOverObject;
    private Camera mainCamera;

    // text object variables
    public TextMeshProUGUI roundCounter;
    public TextMeshProUGUI linesCounter;
    public TextMeshProUGUI scoreCounter;
    private TextMeshProUGUI newHighscoreText;

    // gameboard variables
    public static int width = 10;
    public static int height = 20;
    private static Transform[,] grid = new Transform[width, height];

    // movement variables
    private float movementX;
    private float movementY;
    public bool isClearing = false;

    // rotation variables
    public Vector2 rotationPoint;

    // fall variables
    // calculation for the speed per round: (0.8-((level-1)*0.007))^(level-1)
    public int lineClears;
    public int gameRound;
    public float defaultFallTimer;
    public float fallTimer;

    // clear line animation variables
    private GameObject clearedBlock;
    private float squareSpeed = 10.0f;
    private Vector3 squareDestination = new Vector3(8.2f, -1.1f, 0f);

    // variables for completed packages
    public GameObject package;
    private float packageSpeed = 1.0f;
    private Vector2 packageStartPostion = new Vector2(8.2f, -3f);
    private float landedY = -5.125f;
    private Vector2 packageConveyorEnd = new Vector2(-19.468f, -5.125f);
    private float packageExplosionSpeed = 50.0f;
    private Vector2 packageTruck = new Vector2(-24.17f, -4.843f);
    public List<GameObject> currentPackages = new List<GameObject>();

    // score variables
    public int gameScore;
    private int singlePlaceClears = 0;

    // cursed pieces variables
    public bool NegativeBlockCalled = false;
    public bool BoxBlockCalled = false;
    public GameObject numberDisplayPrefab;
    public Sprite[] numberDisplaySprites;
    public Sprite explosionSprite;
    public GameObject explosionObject;
    public bool placingBlock = false;

    // timer variables
    public float defaultAutoMoveTimer = 0.1f;
    public float defaultAutoMoveCapTimer = 1.0f / 60.0f;
    public float autoMoveTimer;
    public float autoMoveCapTimer;
    public bool quickDrop = false;

    // input variables
    public GameObject currentBlock;
    private Vector2 moveInput;
    private float rotateInput;
    private float rotateDragBlockInput;
    public PlayerInput playerInput;

    // make a variable for stopping async functions from running
    private CancellationTokenSource _cts = new CancellationTokenSource();

    // when the game object is destroyed
    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    // set this script to the moveblocks instance
    private void Awake()
    {
        // find the spawner game object
        if (gameObject.name == "Spawner")
        {
            if (instance == null)
            {
                instance = this;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        // only if scene is arcade mode
        if (SceneManager.GetActiveScene().name == "MainGame")
        {
            // hide the new highscore text
            newHighscoreText = GameObject.Find("LevelCanvas/NewHighscoreText").GetComponent<TextMeshProUGUI>();
            newHighscoreText.enabled = false;
        }

        // Initialize the camera variable
        mainCamera = Camera.main;

        //assign the sound effects
        explosionSound = Resources.Load<AudioClip>("Sounds/SFX/explosion");
        waterSound = Resources.Load<AudioClip>("Sounds/SFX/water");
        gravelSound = Resources.Load<AudioClip>("Sounds/SFX/gravel");
        placeSound = Resources.Load<AudioClip>("Sounds/SFX/place_block");
        lineClearSound = Resources.Load<AudioClip>("Sounds/SFX/line_clear");
        gameOverSound = Resources.Load<AudioClip>("Sounds/SFX/game_over");
        fanfareSound = Resources.Load<AudioClip>("Sounds/SFX/fanfare");
    }

    // Update is called once per frame
    void Update()
    {
        // check if dialogue is active
        if (Dialogue.instance != null)
        {
            if (Dialogue.instance.DialogueActive) { return; }
        }

        // wait until the instance is made and that all the variables are assigned
        if (instance == null || instance.currentBlock == null || instance.parentTransform == null || instance.rotationPoint == null || isClearing) { return; }

        // movement for the drag block
        if (currentBlock.gameObject.name == "DragBlock" && !PauseManager.instance.IsPaused)
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
            // disable normal rotation for the box block & drag block
            if (!(currentBlock.gameObject.name == "BoxBlock") && !(currentBlock.gameObject.name == "DragBlock"))
            {
                Rotate(rotateInput);
            }
        }

        // rotate the drag block
        if (playerInput.actions["RotateDragBlock"].WasPressedThisFrame())
        {
            // only do rotation for the drag block
            if (currentBlock.gameObject.name == "DragBlock")
            {
                Rotate(rotateDragBlockInput);
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
            else if (!placingBlock && !isClearing && !NegativeBlockCalled)
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
        // if the block has already been destroyed do not continue
        if (currentBlock == null || currentBlock.gameObject.name != "JNegativeBlock") return;

        // immediately lock input
        isClearing = true;

        // play the place block sound effect
        SFXManager.instance.PlayPitchedSFXClip(placeSound, transform, 1f);

        // add the block to the grid
        AddToGrid();

        try
        {
            // function for doing special block actions
            await CursedBlocks(_cts.Token);

            // funtion for checking for line clears
            await CheckForLines(_cts.Token);

            // state that the line isn't clearing
            isClearing = false;

            if (_cts.Token.IsCancellationRequested) return;

            // do not create a new block if the scene is changing
            if (this == null || !gameObject.activeInHierarchy) return;
            spawnBlockScript.NewBlock();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Stopped async functions.");
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
        else if (context.action.name == "RotateDragBlock")
        {
            rotateDragBlockInput = context.ReadValue<float>();
        }
    }

    // move the block based on player inputs
    public void Move()
    {
        if (!(currentBlock.gameObject.name == "DragBlock"))
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

    private void Rotate(float currentRotationInput)
    {
        // if up key pressed
        if (currentRotationInput == 1)
        {
            currentBlock.transform.RotateAround(currentBlock.transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
            if (!ValidRotation())
            {
                currentBlock.transform.RotateAround(currentBlock.transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
            }
        }
        // if z key pressed
        else
        {
            currentBlock.transform.RotateAround(currentBlock.transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
            if (!ValidRotation())
            {
                currentBlock.transform.RotateAround(currentBlock.transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
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
        if (ValidMove(mouseXMovement - 1, 0))
        {
            parentTransform.position = new Vector2(Mathf.RoundToInt(worldPosition.x) - 1, parentTransform.position.y);
        }
    }

    // function for doing line clears
    public async Task CheckForLines(CancellationToken token)
    {
        for (int i = height - 1; i >= 0; i--)
        {
            if (token.IsCancellationRequested) return;

            // if line clear
            if (HasLine(i))
            {
                // delete the line with the line clear
                List<GameObject> blocksCleared = await DeleteLine(i, token);

                // animate the pieces leaving the board
                await AnimateLine(blocksCleared, token);

                // move the rows down
                await RowDown(i, token);

                i++;
            }
        }

        // check if the line clears are greater than 0
        if (singlePlaceClears > 0)
        {
            // create a package if there was at least one line clear
            CreatePackage();
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
    public async Task<List<GameObject>> DeleteLine(int i, CancellationToken token)
    {
        // create the list of clearedBlocks
        List<GameObject> clearedBlocks = new List<GameObject>();

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
            // do not check an empty grid space
            if (grid[j, i] == null) continue;

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
                clearedBlock = Instantiate(grid[j, i].gameObject, grid[j, i].gameObject.transform.position, Quaternion.identity);

                // move the cleared block's rendering to the highest sorting layer
                clearedBlock.GetComponent<SpriteRenderer>().sortingOrder = 10;

                // add the cleared block to the list of cleared blocks
                clearedBlocks.Add(clearedBlock);

                // delete the game objects on the line
                Destroy(grid[j, i].gameObject);

                // update the grid
                grid[j, i] = null;
            }

            // wait for the delete line function to complete
            await Task.Yield();
            token.ThrowIfCancellationRequested();
        }
        
        // update the line clears variable
        lineClears++;

        // update the variable keeping track of the line clears made by one block
        singlePlaceClears++;

        // returned the clearedBlocks
        return clearedBlocks;
    }


    // function for animating the blocks leaving the board
    private async Task AnimateLine(List<GameObject> clearedBlocks, CancellationToken token)
    {
        // set the blocks that have reached the end to 0
        bool clearedRows = false;

        // play the clear line sound effect
        AudioSource lineClearAudioSource = SFXManager.instance.PlayLoopedSFXClip(lineClearSound, transform, 1f);

        while (!clearedRows)
        {
            // reset the total of blocks reached
            clearedRows = true;

            foreach (GameObject currentSquare in clearedBlocks)
            {
                // skip if there are no longer any current squares
                if (currentSquare == null) { continue; }

                // move the square towards the end
                currentSquare.transform.position = Vector3.MoveTowards(currentSquare.transform.position, squareDestination, squareSpeed * Time.deltaTime);

                if (Vector2.Distance(currentSquare.transform.position, squareDestination) > 0.1f)
                {
                    clearedRows = false;
                }
            }

            // wait until all of the squares are cleared
            await Task.Yield();
            token.ThrowIfCancellationRequested();
        }

        // destroy all of the squares
        foreach (var square in clearedBlocks)
        {
            if (square != null) Destroy(square);
        }

        // clear the list
        clearedBlocks.Clear();

        // stop the clear line sound effect by deleting it's game object
        Destroy(lineClearAudioSource.gameObject);
    }


    // function for moving the blocks down
    public async Task RowDown(int i, CancellationToken token)
    {
        // look at the y's from the line above current y upwards
        for (int y = i + 1; y < height; y++)
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
            // wait for the row down function to complete
            await Task.Yield();
            token.ThrowIfCancellationRequested();
        }
    }

    // add the player's block to the grid
    public void AddToGrid()
    {
        foreach (Transform children in currentBlock.transform)
        {
            if (children.name != "BoxFlap")
            {
                // round the x and y positions
                int roundedX = Mathf.RoundToInt(children.position.x);
                int roundedY = Mathf.RoundToInt(children.position.y);

                // make sure the value doesn't get rounded to -1
                roundedY = Mathf.Max(roundedY, 0);

                //add the block to the grid
                grid[roundedX, roundedY] = children;
            }
        }
    }

    // check if the player's movements were valid
    public bool ValidMove(int xUpdate, int yUpdate = 0) {
        // functionality for the Negative Block
        if (currentBlock.gameObject.name == "JNegativeBlock")
        {
            int overlapCount = 0;
            foreach (Transform children in currentBlock.transform)
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
                    return false;
                }

                // check if all blocks are touching another piece
                if (grid[roundedX, roundedY] != null)
                {
                    // increment the overlapping negative blocks count if the negative square is overlapping another piece
                    overlapCount++;
                }
            }

            // if all of the pieces of the negative block overlap with a normal piece
            if (overlapCount == currentBlock.gameObject.transform.childCount)
            {
                // remove the pieces the negative piece overlaps with
                NegativeBlockDestruction();
                return false;
            }
        }
        // functionality for the Box Block
        else if (currentBlock.gameObject.name == "BoxBlock")
        {
            foreach (Transform children in currentBlock.transform)
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
            foreach (Transform children in currentBlock.transform)
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
        if (currentBlock.gameObject.name == "JNegativeBlock")
        {
            int overlapCount = 0;
            foreach (Transform children in currentBlock.transform)
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
                    return false;
                }

                // check if all blocks are touching another piece
                if (grid[roundedX, roundedY] != null)
                {
                    // increment the overlapping negative blocks count if the negative square is overlapping another piece
                    overlapCount++;
                }
            }

            // if all of the pieces of the negative block overlap with a normal piece
            if (overlapCount == currentBlock.gameObject.transform.childCount)
            {
                // remove the pieces the negative piece overlaps with
                NegativeBlockDestruction();
                return false;
            }
        }
        else
        { 
            foreach (Transform children in currentBlock.transform)
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

    // function for creating packages from completed lines
    private async void CreatePackage()
    {
        try
        {
            // create a new package
            GameObject newPackage = Instantiate(package, packageStartPostion, Quaternion.identity);

            // add the new package to the list of packages
            currentPackages.Add(newPackage);

            // assign its rigid body
            Rigidbody2D packageRigidBody = newPackage.GetComponent<Rigidbody2D>();

            // wait until the package is on the conveyor belt
            while (newPackage != null && Vector3.Distance(newPackage.transform.position, new Vector2(newPackage.transform.position.x, landedY)) > 0.1f)
            {
                await Task.Yield();
            }

            // destroy the rigid body once the package is close and assign its y to the landed y
            Destroy(packageRigidBody);
            newPackage.transform.position = new Vector2(newPackage.transform.position.x, landedY);

            // wait until the package is really close to the destination
            while (newPackage != null && Vector3.Distance(newPackage.transform.position, packageConveyorEnd) > 0.01f)
            {
                // move the package to the end of the conveyor belt
                newPackage.transform.position = Vector3.MoveTowards(newPackage.transform.position, packageConveyorEnd, packageSpeed * Time.deltaTime);

                // wait until the package is at the to the end of the conveyor belt
                await Task.Yield();
            }

            // stop the package at the end of the conveyor belt
            newPackage.transform.position = packageConveyorEnd;

            // explode the package
            GameObject TempExplosion = Instantiate(explosionObject, newPackage.transform.position, Quaternion.identity);

            // play the explosion sound effect
            SFXManager.instance.PlaySFXClip(explosionSound, newPackage.transform, 1f);

            // wait 100 milliseconds
            await Task.Delay(100);

            // destroy the explosion game object
            Destroy(TempExplosion);

            // make the package fly towards the truck
            while (newPackage != null && Vector3.Distance(newPackage.transform.position, packageTruck) > 0.01f)
            {
                // move the package to the end of the conveyor belt
                newPackage.transform.position = Vector3.MoveTowards(newPackage.transform.position, packageTruck, packageExplosionSpeed * Time.deltaTime);

                // wait until the package is at the to the end of the conveyor belt
                await Task.Yield();
            }

            // remove the package from the list of current packages
            currentPackages.Remove(newPackage);

            // destroy the package
            Destroy(newPackage);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    // check the type of block & do its ability
    private async Task CursedBlocks(CancellationToken token)
    {
        // ignore this function if the current block is null
        if (currentBlock == null) return;

        // functionality for the bomb block
        if (currentBlock.gameObject.name == "BombBlock")
        {
            // round the current x and y positions
            int roundedX = Mathf.RoundToInt(parentTransform.position.x);
            int roundedY = Mathf.RoundToInt(parentTransform.position.y);

            // play the explosion animation for the bomb
            ExplosionAnimation(grid[roundedX, roundedY].gameObject.transform.GetComponent<SpriteRenderer>());

            // wait 100 milliseconds
            await Task.Delay(100, token);

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
                                await Task.Delay(100, token);

                                // destroy the explosion game object
                                Destroy(TempExplosion);
                            }
                            // if it is a normal square do the normal bomb deletion
                            else
                            {
                                // play the explosion animation
                                ExplosionAnimation(grid[checkX, checkY].gameObject.transform.GetComponent<SpriteRenderer>());

                                // wait 100 milliseconds
                                await Task.Delay(100, token);

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
            Destroy(currentBlock.transform.parent.gameObject);
        }

        // functionality for the water block
        if (currentBlock.gameObject.name == "JWaterBlock")
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
                            await Task.Delay(300, token);

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
        if (currentBlock.gameObject.name == "GravelBlock")
        {
            // create variables for the maximum x and y of the gravel block
            int minX = Mathf.RoundToInt(currentBlock.transform.GetChild(0).position.x);
            int minY = Mathf.RoundToInt(currentBlock.transform.GetChild(0).position.y);
            int maxX = 0;
            int maxY = 0;

            // look for the maximum and minimum x and y of the block
            foreach (Transform children in currentBlock.transform)
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
                            // add a delay for until the next gravel column falls down
                            await Task.Delay(100, token);

                            // update the grid
                            grid[j, newY] = grid[j, y];
                            grid[j, y] = null;

                            //move the square down
                            grid[j, newY].transform.position = new Vector3(j, newY, 0);

                            // play the gravel sound effect
                            SFXManager.instance.PlaySFXClip(gravelSound, parentTransform, 1f);
                        }
                    }
                }
            }
        }
    }

    // function for destroying blocks that the negative block overlaps with
    private void NegativeBlockDestruction()
    {
        // check if this function has been called
        if (NegativeBlockCalled == false && placingBlock == false)
        {
            //stop the placement function from being called again
            placingBlock = true;
            isClearing = true;

            // stop this from spawning multiple blocks
            NegativeBlockCalled = true;

            // loop through each child to destroy all of the correct blocks
            foreach (Transform children in currentBlock.transform)
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

            print("finished");

            // destroy the negative block
            Destroy(currentBlock.transform.parent.gameObject);

            // spawn a new block
            spawnBlockScript.NewBlock();
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
            foreach (Transform children in currentBlock.transform)
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
    public async Task EndGame()
    {
        // set game over to true for the pause manager
        PauseManager.instance.isGameOver = true;

        // disable the music
        GameObject.Find("MusicManager").SetActive(false);

        // play the game over sound
        SFXManager.instance.PlaySFXClip(gameOverSound, transform, 1f);

        // pause the game
        PauseManager.instance.PauseGame();

        // wait for the game over sound to complete
        await Task.Delay(391);

        // unpause the game
        PauseManager.instance.UnpauseGame();

        // show the game over object
        gameOverObject.GetComponent<Renderer>().enabled = true;

        // make the score counter say final score
        scoreCounter.text = "Final Score: " + gameScore;

        // hide the text
        roundCounter.enabled = false;
        linesCounter.enabled = false;

        // show the restart button & home button
        buttonUI.instance.ToggleButton(0);
        buttonUI.instance.ToggleButton(1);

        // only if the scene is arcade mode
        if (SceneManager.GetActiveScene().name == "MainGame")
        {
            // make the score a child of the input field
            scoreCounter.GetComponent<RectTransform>().SetParent(Leaderboard.instance.inputFieldPanel.transform, false);

            // update the score position upon loss
            scoreCounter.GetComponent<RectTransform>().anchoredPosition = new Vector2(-360, 0);

            // if the highscore is a new highscore
            if (Leaderboard.instance.IsHighScore(gameScore))
            {
                // show the new highscore text
                newHighscoreText.enabled = true;

                // play the fanfare sound
                SFXManager.instance.PlaySFXClip(fanfareSound, transform, 1f);

                // pause the game
                PauseManager.instance.PauseGame();

                // wait for the fanfare sound to complete
                await Task.Delay(1735);

                // unpause the game
                PauseManager.instance.UnpauseGame();
            }

            // hide the new highscore text
            newHighscoreText.enabled = false;

            // show the leaderboard object
            Leaderboard.instance.gameObject.SetActive(true);

            // set the new score if possible
            Leaderboard.instance.SetCurrentScore(gameScore);
        }
        else
        {
            // hide the orders
            OrderManager.instance.ordersList.enabled = false;
            OrderManager.instance.ordersHolder.gameObject.SetActive(false);
            OrderManager.instance.currentOrderText.enabled = false;
            OrderManager.instance.totalOrdersText.enabled = false;

            // update the final score text position
            scoreCounter.rectTransform.anchoredPosition = new Vector2(0, 260);

            // update the text box width
            scoreCounter.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1200);

            // update the final score text size
            scoreCounter.fontSize = 72;

            // update the final score text alignment
            scoreCounter.alignment = TextAlignmentOptions.Center;
        }

        // wait until everything has loaded
        await Task.Yield();

        // pause the game
        PauseManager.instance.PauseGame();

        // delete this script to disable movement
        Destroy(this);
    }
}
