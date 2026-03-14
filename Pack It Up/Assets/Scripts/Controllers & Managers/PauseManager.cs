using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    
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
        IsPaused = true;
        Time.timeScale = 0f;
    }
    
    public void UnpauseGame()
    {
        IsPaused = true;
        Time.timeScale = 1f;
    }
}
