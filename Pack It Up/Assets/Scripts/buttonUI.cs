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
        ToggleButton(buttons[0]);
    }

    // function for adding the behavior of restarting the game
    public void RestartButton() {
        SceneManager.LoadScene(mainGame);
    }

    // function for disabling a button
    public void ToggleButton(Button button)
    {
        // make the button invisible/visible and disable/enable interaction
        button.gameObject.SetActive(!button.gameObject.activeInHierarchy);
        button.interactable = !button.interactable;
    }
}
