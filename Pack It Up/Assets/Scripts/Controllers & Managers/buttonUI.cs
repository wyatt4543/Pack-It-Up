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
    [SerializeField] private GameObject[] settingsPages;
    private string packageObjectName = "Package";

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
        // set the unlocked levels
        int highestUnlockedLevel = PlayerPrefs.GetInt("highestUnlockedLevel", 1);

        // loop through the levels & disable the not unlocked levels
        foreach (Transform levelButton in canvases[1].transform)
        {
            // if the level is not unlocked
            if (levelButton.gameObject.name[^1] - '0' > highestUnlockedLevel && levelButton.gameObject.name.Contains("Level"))
            {
                // disable the button for that level
                levelButton.GetComponent<Button>().interactable = false;
            }
            // if the level is unlocked
            else
            {
                // make its button interactible
                levelButton.GetComponent<Button>().interactable = true;
            }
        }

        // if scene is the main menu
        if (SceneManager.GetActiveScene().name == mainMenu)
        {
            // hide the currently unused canvases
            ToggleCanvas(1);
            ToggleCanvas(2);
            ToggleCanvas(3);
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

        // check if the main menu is open
        if (canvases[0].enabled)
        {
            // instantly hide the title
            ToggleImage(0);

            // wait for the animation to complete
            await Task.Delay(350);

            // hide the main menu
            ToggleCanvas(0);

            // reenable the title because the main menu is hidden
            ToggleImage(0);

            // show the level selection menu
            ToggleCanvas(1);
        }
        else
        {
            // hide everything except the back button in the level selection menu
            ToggleNotBackButton(1);

            // wait for the animation to complete
            await Task.Delay(350);

            // hide the level selection menu
            ToggleCanvas(1);

            // reenable everything because the level selection menu is hidden
            ToggleNotBackButton(1);

            // show the open the main menu
            ToggleCanvas(0);
        }
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
    public async void SettingsButton()
    {
        // toggle the door animation
        ToggleTitleDoorAnimation();

        // check if the main menu is open
        if (canvases[0].enabled)
        {
            // instantly hide the title
            ToggleImage(0);

            // wait for the animation to complete
            await Task.Delay(350);

            // hide the main menu
            ToggleCanvas(0);

            // reenable the title because the main menu is hidden
            ToggleImage(0);

            // show the settings menu
            ToggleCanvas(3);
        }
        else
        {
            // hide everything except the back button in the settings menu
            ToggleNotBackButton(3);

            // wait for the animation to complete
            await Task.Delay(350);

            // hide the settings menu
            ToggleCanvas(3);

            // reenable everything because the settings menu is hidden
            ToggleNotBackButton(3);

            // show the open the main menu
            ToggleCanvas(0);
        }
    }

    // function for the button to show the credits
    public async void CreditsButton()
    {
        // toggle the door animation
        ToggleTitleDoorAnimation();

        // check if the main menu is open
        if (canvases[0].enabled)
        {
            // instantly hide the title
            ToggleImage(0);

            // wait for the animation to complete
            await Task.Delay(350);

            // hide the main menu
            ToggleCanvas(0);

            // reenable the title because the main menu is hidden
            ToggleImage(0);

            // show the credits menu
            ToggleCanvas(2);
        }
        else
        {
            // hide everything except the back button in the credits menu
            ToggleNotBackButton(2);

            // wait for the animation to complete
            await Task.Delay(350);

            // hide the credits menu
            ToggleCanvas(2);

            // reenable everything because the credits menu is hidden
            ToggleNotBackButton(2);

            // show the open the main menu
            ToggleCanvas(0);
        }
    }

    // function for the button to quit the game
    public void QuitButton()
    {
        Application.Quit();
    }

    // function for adding the behavior of heading back to the main menu
    public void HomeButton()
    {
        // destroy current packages in the level
        DestroyPackages();
        PauseManager.instance.UnpauseGame();
        SceneManager.LoadScene(mainMenu);
    }

    // function for adding the behavior of restarting the game
    public void RestartButton() {
        // destroy current packages in the level
        DestroyPackages();
        PauseManager.instance.UnpauseGame();
        // load the current scene on restart
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // function for continuing to the next level
    public void ContinueButton() {
        // destroy current packages in the level
        DestroyPackages();

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

    // function for toggling eveything except the back button on the title screen
    public void ToggleNotBackButton(int canvasIndex)
    {
        foreach (Transform children in canvases[canvasIndex].transform)
        {
            if (children.name != "BackButton" && children.name != "Overlay")
            {
                children.gameObject.SetActive(!children.gameObject.activeSelf);
            }
        }
    }

    public void ToggleSettings(int settingsIndex)
    {
        // hide the other settings that weren't toggled
        foreach (GameObject children in settingsPages)
        {
            if (children != settingsPages[settingsIndex])
            {
                children.SetActive(false);
            }
            else
            {
                children.SetActive(true);
            }
        }
    }

    public void DestroyPackages()
    {
        // Find all active GameObjects in the scene
        GameObject[] allGameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject package in allGameObjects)
        {
            // Check if the object's name contains the specified partialName
            if (package.name.Contains(packageObjectName))
            {
                Destroy(package);
            }
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
