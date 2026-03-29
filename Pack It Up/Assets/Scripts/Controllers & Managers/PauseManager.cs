using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    public PlayerInput playerInput;
    public bool isGameOver = false;
    public bool levelComplete = false;

    public bool IsPaused {  get; private set; }
    public bool GotChanged { get; private set; }

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PauseGame()
    {
        // tell other functions that the value of isPaused has changed
        GotChanged = true;

        // pause time
        IsPaused = true;
        Time.timeScale = 0f;

        // switch the action map
        playerInput.SwitchCurrentActionMap("UI");

        GotChanged = false;
    }

    public void UnpauseGame()
    {
        // tell other functions that the value of isPaused has changed
        GotChanged = true;

        // unpause time
        IsPaused = false;
        Time.timeScale = 1f;

        // switch the action map
        playerInput.SwitchCurrentActionMap("Player");

        GotChanged = false;
    }
}
