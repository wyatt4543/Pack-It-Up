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
            // hide the restart button & home button
            ToggleButton(0);
            ToggleButton(1);
        }
    }

    // function for adding the behavior of starting the game
    public void PlayButton()
    {
        // load the main game on play
        SceneManager.LoadScene(mainGame);
    }

    // function for opening the settings menu
    public void SettingsButton()
    {
        // hide the other two buttons
        ToggleButton(0);
        ToggleButton(2);
        
        // open the settings menu 
    }

    // function for the button to quit the game
    public void QuitButton()
    {
        Application.Quit();
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
