using System.Collections.Generic;
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
    private GameObject nextBlock;
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
    public void NewBlock(int lineClears = 0, int gameRound = 1, int gameScore = 0, List<GameObject> currentPackages = null, int blockType = -1) {
        // assign the default value of currentPackages
        if (currentPackages == null)
        {
            currentPackages = new List<GameObject>();
        }
        
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

        // check for the copied block type & assign it a random value
        if (blockType == 11)
        {
            newBlock.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = numberDisplaySprites[Random.Range(0, numberDisplaySprites.Length)];
        }

        // get the moveBlocksScript
        MoveBlocks moveBlocksScript = newBlock.GetComponentsInChildren<MoveBlocks>()[0];

        // assign the move blocks script of for the order manager
        if (OrderManager.instance != null)
        {
            OrderManager.instance.moveBlockScript = moveBlocksScript;
        }

        // set the number of line clears
        moveBlocksScript.lineClears = lineClears;

        // set the game round
        moveBlocksScript.gameRound = gameRound;

        // set the game score
        moveBlocksScript.gameScore = gameScore;

        // set the current packages
        moveBlocksScript.currentPackages = currentPackages;

        // set the number display sprites
        moveBlocksScript.numberDisplaySprites = numberDisplaySprites;

        // set the explosion sprite
        moveBlocksScript.explosionSprite = explosionSprite;

        // set the explosion object
        moveBlocksScript.explosionObject = explosionObject;

        // set the package game object
        moveBlocksScript.package = package;
    }
}
