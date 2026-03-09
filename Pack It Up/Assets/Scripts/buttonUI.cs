using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonUI : MonoBehaviour
{
    // set the string to the main game scene name
    [SerializeField] private string mainGame = "MainGame";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // function for adding the behavior of restarting the game
    public void RestartButton() {
        SceneManager.LoadScene(mainGame);
    }
}
