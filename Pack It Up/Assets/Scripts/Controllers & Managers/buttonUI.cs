using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class buttonUI : MonoBehaviour
{
    public static buttonUI instance;

    // set the string to the main game scene name
    [SerializeField] private string mainGame = "MainGame";
    [SerializeField] private string mainMenu = "MainMenu";
    
    // collect a list of all of the buttons & canvases
    public Button[] buttons;
    public Canvas[] canvases;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // if scene is the main menu
        if (SceneManager.GetActiveScene().name == mainMenu)
        {
            // hide the level select canvas
            canvases[1].enabled = false;
        }

        // if the scene is the main game
        if (SceneManager.GetActiveScene().name == mainGame || SceneManager.GetActiveScene().name.Contains("Level"))
        {
            // hide the restart button & home button
            ToggleButton(0);
            ToggleButton(1);
        }

        // if scene is a level
        if (SceneManager.GetActiveScene().name.Contains("Level"))
        {
            // hide the continue button
            ToggleButton(2);
        }
    }

    // function for adding the behavior of starting the game
    public void PlayButton()
    {
        // open the level selection menu
        canvases[0].enabled = false;
        canvases[1].enabled = true;
    }

    // function for adding the behavior of selecting a level
    public void SelectLevel(string level)
    {
        // load the selected level
        SceneManager.LoadScene("Level " + level);
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

    // function for adding the behavior of heading back to the main menu
    public void HomeButton()
    {
        SceneManager.LoadScene(mainMenu);
    }

    // function for adding the behavior of restarting the game
    public void RestartButton() {
        // load the current scene on restart
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // function for continuing to the next level
    public void ContinueButton() {
        // load the next level
        PauseManager.instance.UnpauseGame();
        SceneManager.LoadScene("Level " + (SceneManager.GetActiveScene().name[^1] - '0' + 1));
    }

    // function for disabling a button
    public void ToggleButton(int buttonIndex)
    {
        // make the button invisible/visible and disable/enable interaction
        buttons[buttonIndex].gameObject.SetActive(!buttons[buttonIndex].gameObject.activeInHierarchy);
        buttons[buttonIndex].interactable = !buttons[buttonIndex].interactable;
    }
}
