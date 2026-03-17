using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private bool menuOpen = false;
    private GameObject pauseMenu;
    private SoundMixerManager soundMixer;
    private Slider masterVolume;
    private Slider SFXVolume;
    private Slider musicVolume;
    private Button backButton;

    private void Start()
    {
        pauseMenu = GameObject.Find("PauseMenuCanvas");
        pauseMenu.SetActive(false);
        soundMixer = GameObject.Find("SoundMixerManager").GetComponent<SoundMixerManager>();
        masterVolume = pauseMenu.transform.Find("Master Volume").gameObject.GetComponent<Slider>();
        SFXVolume = pauseMenu.transform.Find("SFX Volume").gameObject.GetComponent<Slider>();
        musicVolume = pauseMenu.transform.Find("Music Volume").gameObject.GetComponent<Slider>();
        backButton = pauseMenu.transform.Find("BackButton").gameObject.GetComponent<Button>();
        masterVolume.onValueChanged.AddListener(
            delegate { soundMixer.SetMasterVolume(masterVolume.value); }
        );
        SFXVolume.onValueChanged.AddListener(
            delegate { soundMixer.SetSFXVolume(SFXVolume.value); }
        );
        musicVolume.onValueChanged.AddListener(
            delegate { soundMixer.SetMusicVolume(musicVolume.value); }
        );
        backButton.onClick.AddListener(
            delegate { PauseUnpause(); }
        );
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            PauseUnpause();
        }
    }

    public void PauseUnpause()
    {
        if (!PauseManager.instance.IsPaused)
        {
            Pause();
        }
        else
        {
            Unpause();
        }
    }

    // open or close the pause menu
    public void UpdateSoundMenu()
    {
        menuOpen = !menuOpen;
        pauseMenu.SetActive(menuOpen);
    }

    // pause function
    public void Pause()
    {
        PauseManager.instance.PauseGame();
        UpdateSoundMenu();
    }

    // unpause function
    public void Unpause()
    {
        PauseManager.instance.UnpauseGame();
        UpdateSoundMenu();
    }
}
