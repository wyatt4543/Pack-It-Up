using UnityEngine;

public class SpawnBlock : MonoBehaviour
{
    public GameObject[] Blocks;
    public Sprite[] numberDisplaySprites;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // spawn a new block at the beginning of the game
        NewBlock(0,1,0,11);
    }

    // function for creating new blocks
    public void NewBlock(int lineClears = 0, int gameRound = 1, int gameScore = 0, int blockType = -1) {
        // assign the default block type value
        if (blockType == -1)
        {
            blockType = Random.Range(0, Blocks.Length);
        }

        // check for the copied block type & assign it a random value
        if (blockType == 11)
        {

        }

        // create a random new block in the spawner postion
        GameObject newBlock = Instantiate(Blocks[blockType], transform.position, Quaternion.identity);

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
    }
}
