using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class buttonUI : MonoBehaviour
{
    public static buttonUI instance;

    // set the string to the main game scene name
    [SerializeField] private string mainGame = "MainGame";
    [SerializeField] private string mainMenu = "MainMenu";
    [SerializeField] private Animator titleDoorAnimator;
    private bool triggerTitleDoorOpen = false;
    private string nextLevel;
    
    // collect a list of all of the buttons, images, and canvases
    public Button[] buttons;
    public Canvas[] canvases;
    public Image[] images;

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
            // hide the currently unused canvases
            ToggleCanvas(1);
            ToggleCanvas(2);
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
    public async void PlayButton()
    {
        // toggle the door animation
        ToggleTitleDoorAnimation();

        // instantly hide the title
        ToggleImage(0);

        // wait for the animation to complete
        await Task.Delay(350);

        // open the level selection menu
        ToggleCanvas(0);
        ToggleCanvas(1);
    }

    // function for adding the behavior of selecting a level
    public void SelectLevel(string level)
    {
        if (!(level == "0"))
        {
            // load the selected level
            SceneManager.LoadScene("Level " + level);
        }
        else
        {
            // load the arcade mode
            SceneManager.LoadScene(mainGame);
        }
    }

    // function for opening the settings menu
    public void SettingsButton()
    {
        // hide the other two buttons
        ToggleButton(0);
        ToggleButton(2);

        // open the settings menu 
    }

    // function for the button to show the credits
    public async void CreditsButton()
    {
        // toggle the door animation
        ToggleTitleDoorAnimation();

        // instantly hide the title
        ToggleImage(0);

        // wait for the animation to complete
        await Task.Delay(350);

        // open the credits menu
        ToggleCanvas(0);
        ToggleCanvas(2);
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
        // unpause the game
        PauseManager.instance.UnpauseGame();

        // update the nextLevel string
        nextLevel = "Level " + (SceneManager.GetActiveScene().name[^1] - '0' + 1);

        if (SceneUtility.GetBuildIndexByScenePath(nextLevel) != -1)
        {
            // load the next level
            SceneManager.LoadScene(nextLevel);
        }
        else
        {
            // load arcade mode
            SceneManager.LoadScene(mainGame);
        }
    }

    // function for disabling/enabling a button
    public void ToggleButton(int buttonIndex)
    {
        // make the button invisible/visible and disable/enable interaction
        buttons[buttonIndex].gameObject.SetActive(!buttons[buttonIndex].gameObject.activeInHierarchy);
        buttons[buttonIndex].interactable = !buttons[buttonIndex].interactable;
    }

    // function for disabling/enabling a canvas
    public void ToggleCanvas(int canvasIndex)
    {
        canvases[canvasIndex].enabled = !canvases[canvasIndex].enabled;
    }

    // function for toggling the title screen door open animation
    public void ToggleTitleDoorAnimation()
    {
        triggerTitleDoorOpen = !triggerTitleDoorOpen;
        titleDoorAnimator.SetBool("triggerTitleDoorOpen", triggerTitleDoorOpen);
    }

    // function for toggling an image
    public void ToggleImage(int imageIndex)
    {
        images[imageIndex].enabled = !images[imageIndex].enabled;
    }
}
