using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    public PlayerInput playerInput;

    public bool IsPaused {  get; private set; }

    public void Awake()
    {
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
        // unpause time
        IsPaused = false;
        Time.timeScale = 1f;

        // switch the action map
        playerInput.SwitchCurrentActionMap("Player");
    }
}
