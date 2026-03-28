using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    public PlayerInput playerInput;
    public bool isGameOver = false;
    public bool levelComplete = false;

    public bool IsPaused {  get; private set; }

    public void Awake()
    {
        // unpause the game
        PauseManager.instance.UnpauseGame();

        if (instance == null)
        {
            instance = this;
        }
    }

    public void PauseGame()
    {
        // pause time
        IsPaused = true;
        Time.timeScale = 0f;

        // switch the action map
        playerInput.SwitchCurrentActionMap("UI");
    }

    public void UnpauseGame()
    {
        if (!levelComplete)
        {
            // unpause time
            IsPaused = false;
            Time.timeScale = 1f;

            // switch the action map
            playerInput.SwitchCurrentActionMap("Player");
        }
    }
}
