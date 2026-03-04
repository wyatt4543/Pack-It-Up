using UnityEngine;

public class GameOver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(false);
    }

    // run this function when the player loses
    public void EndGame()
    {
        gameObject.SetActive(true);
    }
}
