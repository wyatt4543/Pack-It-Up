using UnityEngine;

public class SpawnBlock : MonoBehaviour
{
    public GameObject[] Blocks;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // spawn a new block at the beginning of the game
        NewBlock();
    }

    // function for creating new blocks
    public void NewBlock(int lineClears = 0, int gameRound = 1) {
        // create a random new block in the spawner postion
        GameObject newBlock = Instantiate(Blocks[Random.Range(0, Blocks.Length)], transform.position, Quaternion.identity);

        // get the moveBlocksScript
        MoveBlocks moveBlocksScript = newBlock.GetComponentsInChildren<MoveBlocks>()[0];

        // set the number of line clears
        moveBlocksScript.lineClears = lineClears;

        // set the game round
        if ((lineClears > 0) && (lineClears % 10 == 0))
        {
            gameRound++;
        }
        moveBlocksScript.gameRound = gameRound;
    }
}
