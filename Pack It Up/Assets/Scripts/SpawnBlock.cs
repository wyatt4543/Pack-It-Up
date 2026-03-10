using UnityEngine;

public class SpawnBlock : MonoBehaviour
{
    public GameObject[] Blocks;
    public Sprite[] numberDisplaySprites;
    public Sprite explosionSprite;
    public GameObject explosionObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // spawn a new block at the beginning of the game
        NewBlock();
    }

    // function for creating new blocks
    public void NewBlock(int lineClears = 0, int gameRound = 1, int gameScore = 0, int blockType = -1) {
        // assign the default block type value
        if (blockType == -1)
        {
            blockType = Random.Range(0, Blocks.Length);
        }

        // create a random new block in the spawner postion
        GameObject newBlock = Instantiate(Blocks[blockType], transform.position, Quaternion.identity);

        // check for the copied block type & assign it a random value
        if (blockType == 11)
        {
            newBlock.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = numberDisplaySprites[Random.Range(0, numberDisplaySprites.Length)];
        }

        // get the moveBlocksScript
        MoveBlocks moveBlocksScript = newBlock.GetComponentsInChildren<MoveBlocks>()[0];

        // set the number of line clears
        moveBlocksScript.lineClears = lineClears;

        // set the game round
        moveBlocksScript.gameRound = gameRound;

        // set the game score
        moveBlocksScript.gameScore = gameScore;

        // set the number display sprites
        moveBlocksScript.numberDisplaySprites = numberDisplaySprites;

        // set the explosion sprite
        moveBlocksScript.explosionSprite = explosionSprite;

        // set the explosion object
        moveBlocksScript.explosionObject = explosionObject;
    }
}
