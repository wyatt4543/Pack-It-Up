using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    private PlayerInput playerInput;
    private GameObject Spawner;
    public bool isGameOver = false;
    public bool levelComplete = false;

    public bool IsPaused { get; private set; }

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void Start()
    {
        // find the spawner game object
        Spawner = GameObject.Find("Spawner");

        // Initialize player input
        playerInput = Spawner.GetComponent<PlayerInput>();
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

        // wait until the end of the frame before switching maps
        StartCoroutine(DelayedMapSwitch());
    }

    private IEnumerator DelayedMapSwitch()
    {
        // wait for the mouse to not be pressed down
        yield return new WaitUntil(() => !Mouse.current.leftButton.isPressed);

        // add an extra buffer
        yield return new WaitForEndOfFrame();

        playerInput.SwitchCurrentActionMap("Player");

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
