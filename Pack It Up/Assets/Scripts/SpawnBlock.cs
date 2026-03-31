using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnBlock : MonoBehaviour
{
    public static SpawnBlock instance;
    public GameObject[] Blocks;
    public Sprite[] numberDisplaySprites;
    public Sprite explosionSprite;
    public GameObject explosionObject;
    [SerializeField] private GameObject package;
    public GameObject blockChute;
    private int nextBlockType;
    private int blockCount = 0;

    private void Awake()
    {
        transform.position = new Vector2(0, 18);

        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // assign the first block type
        nextBlockType = Random.Range(0, Blocks.Length);

        // spawn a new block at the beginning of the game
        NewBlock();
    }

    // function for creating new blocks
    public async void NewBlock(bool fromHandleBlockPlacement = false, int blockType = -1) {
        // make sure handle block placement does not place a second block
        if (MoveBlocks.instance.currentBlock != null)
        {
            if (MoveBlocks.instance.currentBlock.gameObject.name == "JNegativeBlock")
            {
                if (!fromHandleBlockPlacement)
                {
                    // destroy the negative block
                    Destroy(MoveBlocks.instance.currentBlock.transform.parent.gameObject);
                }
                else
                {
                    return;
                }
            }
        }

        GameObject nextBlock = null;
        
        // increase the block count
        blockCount++;

        // if the first block is placed on level 1 update the dialog
        if (SceneManager.GetActiveScene().name == "Level 1" && blockCount == 2)
        {
            // activate the dialog game object
            Dialogue.instance.gameObject.SetActive(true);

            // activate the order manager game object
            OrderManager.instance.gameObject.SetActive(true);

            // add new lines
            Dialogue.instance.dialogLines = new string[] {
                "Worker: Now that you've placed your first crate, let's do your first order.",
                "Worker: On the right is an order.",
                "Worker: To fulfill that order, you need to make a complete row of crates to send it out of the box.",
                "Worker: The order number goes down by one only once you get the full group of required crates delivered."
            };

            // restart the dialog
            Dialogue.instance.RestartDialogue();

            // enable the order manager
            OrderManager.instance.EnableOrders();
        }
        
        // assign the default block type value
        if (blockType == -1)
        {
            // delete the previous next block
            if (nextBlock)
            {
                Destroy(nextBlock);
            }

            // assign the current block type
            blockType = nextBlockType;

            // get the next block type
            nextBlockType = Random.Range(0, Blocks.Length);
        }

        // create the next block
        nextBlock = Instantiate(Blocks[nextBlockType], Vector2.zero, Quaternion.identity);

        // delete the next block's scripts
        MonoBehaviour[] scripts = nextBlock.transform.GetChild(0).GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            Destroy(script);
        }

        // shrink the next block
        nextBlock.transform.localScale = new Vector3(0.25f, 0.25f, 1);

        // create the package for the next block
        GameObject nextPackage = Instantiate(package, new Vector2(8.5f, 22.5f), Quaternion.identity);

        // grow the next package
        nextPackage.transform.localScale = new Vector3(1.5f, 1.5f, 1);

        // make the next package the parent of the next block
        nextBlock.transform.parent = nextPackage.transform;

        // move the next block to the package
        nextBlock.transform.localPosition = new Vector2(-0.248f, 0);

        // create a random new block in the spawner postion
        GameObject newBlock = Instantiate(Blocks[blockType], transform.position, Quaternion.identity);

        // set the current block
        GameObject currentSquare = newBlock.transform.GetChild(0).gameObject;
        MoveBlocks currentSquareScript = currentSquare.GetComponent<MoveBlocks>();
        MoveBlocks.instance.currentBlock = currentSquare;

        // set the MoveBlocks variables
        MoveBlocks.instance.parentTransform = currentSquareScript.parentTransform;
        MoveBlocks.instance.rotationPoint = currentSquareScript.rotationPoint;
        Destroy(currentSquareScript);

        // reset variables
        await ResetVariables();

        // check for the copied block type & assign it a random value
        if (blockType == 11)
        {
            newBlock.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = numberDisplaySprites[Random.Range(0, numberDisplaySprites.Length)];
        }

        // assign the move blocks script of for the order manager
        if (OrderManager.instance != null)
        {
            OrderManager.instance.moveBlockScript = MoveBlocks.instance;
        }

        // set the number display sprites
        MoveBlocks.instance.numberDisplaySprites = numberDisplaySprites;

        // set the explosion sprite
        MoveBlocks.instance.explosionSprite = explosionSprite;

        // set the explosion object
        MoveBlocks.instance.explosionObject = explosionObject;

        // set the package game object
        MoveBlocks.instance.package = package;
    }

    public async Task ResetVariables()
    {
        // cursed pieces variables
        MoveBlocks.instance.isClearing = false;
        MoveBlocks.instance.NegativeBlockCalled = false;
        MoveBlocks.instance.BoxBlockCalled = false;
        MoveBlocks.instance.placingBlock = false;

        // timer variables
        MoveBlocks.instance.quickDrop = false;

        // Initialize timers
        MoveBlocks.instance.defaultFallTimer = Mathf.Pow((0.8f - ((MoveBlocks.instance.gameRound - 1) * 0.007f)), MoveBlocks.instance.gameRound - 1);
        MoveBlocks.instance.fallTimer = MoveBlocks.instance.defaultFallTimer;
        MoveBlocks.instance.autoMoveTimer = MoveBlocks.instance.defaultAutoMoveTimer;
        MoveBlocks.instance.autoMoveCapTimer = MoveBlocks.instance.defaultAutoMoveCapTimer;

        // update the game round display
        MoveBlocks.instance.roundCounter.text = "Round: " + MoveBlocks.instance.gameRound;

        // update the game lines display
        MoveBlocks.instance.linesCounter.text = "Lines: " + MoveBlocks.instance.lineClears;

        // update the game score display
        MoveBlocks.instance.scoreCounter.text = "Score: " + MoveBlocks.instance.gameScore;

        // test for a game over
        if (!MoveBlocks.instance.ValidMove(0, -1) && !PauseManager.instance.isGameOver)
        {
            await MoveBlocks.instance.EndGame();
            return;
        }

        // wait until everything has reset
        await Task.Yield();
    }
}
