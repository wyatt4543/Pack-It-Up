using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class buttonUI : MonoBehaviour
{
    // set the string to the main game scene name
    [SerializeField] private string mainGame = "MainGame";
    public Button[] buttons;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // if the scene is the main game
        if (SceneManager.GetActiveScene().name == mainGame)
        {
            // hide the restart button
            ToggleButton(0);
        }
    }

    // function for adding the behavior of restarting the game
    public void RestartButton() {
        // load the main game on restart
        SceneManager.LoadScene(mainGame);
    }

    // function for disabling a button
    public void ToggleButton(int buttonIndex)
    {
        // make the button invisible/visible and disable/enable interaction
        buttons[buttonIndex].gameObject.SetActive(!buttons[buttonIndex].gameObject.activeInHierarchy);
        buttons[buttonIndex].interactable = !buttons[buttonIndex].interactable;
    }
}
