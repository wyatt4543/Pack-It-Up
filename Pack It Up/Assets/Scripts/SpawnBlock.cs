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
    public void NewBlock() {
        // create a random new block in the spawner postion
        Instantiate(Blocks[Random.Range(0, Blocks.Length)], transform.position, Quaternion.identity);
    }
}
