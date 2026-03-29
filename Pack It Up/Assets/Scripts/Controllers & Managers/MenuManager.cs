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
    private GameObject pauseButton;

    private const string MasterVolumeKey = "MasterVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string MusicVolumeKey = "MusicVolume";

    private void Start()
    {
        pauseMenu = GameObject.Find("PauseMenuCanvas");
        pauseMenu.SetActive(false);
        soundMixer = GameObject.Find("SoundMixerManager").GetComponent<SoundMixerManager>();
        masterVolume = pauseMenu.transform.Find("Master Volume").gameObject.GetComponent<Slider>();
        SFXVolume = pauseMenu.transform.Find("SFX Volume").gameObject.GetComponent<Slider>();
        musicVolume = pauseMenu.transform.Find("Music Volume").gameObject.GetComponent<Slider>();
        pauseButton = GameObject.Find("LevelCanvas/PauseButton");
        
        // get the saved sound volume values
        masterVolume.value = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        SFXVolume.value = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        musicVolume.value = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);

        // update the mixer to the saved values
        soundMixer.SetMasterVolume(masterVolume.value);
        soundMixer.SetSFXVolume(SFXVolume.value);
        soundMixer.SetMusicVolume(musicVolume.value);

        // create listeners for when the values change to save the new volume values
        masterVolume.onValueChanged.AddListener(
            delegate { soundMixer.SetMasterVolume(OnMasterVolumeChanged(masterVolume.value)); }
        );
        SFXVolume.onValueChanged.AddListener(
            delegate { soundMixer.SetSFXVolume(OnSFXVolumeChanged(SFXVolume.value)); }
        );
        musicVolume.onValueChanged.AddListener(
            delegate { soundMixer.SetMusicVolume(OnMusicVolumeChanged(musicVolume.value)); }
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
        if (!PauseManager.instance.isGameOver && !PauseManager.instance.levelComplete)
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
        pauseButton.SetActive(false);
        PauseManager.instance.PauseGame();
        UpdateSoundMenu();
    }

    // unpause function
    public void Unpause()
    {
        pauseButton.SetActive(true);
        PauseManager.instance.UnpauseGame();
        UpdateSoundMenu();
    }

    // functions to save the volume changes
    public float OnMasterVolumeChanged(float value)
    {
        soundMixer.SetMasterVolume(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();
        return value;
    }

    public float OnSFXVolumeChanged(float value)
    {
        soundMixer.SetSFXVolume(value);
        PlayerPrefs.SetFloat(SFXVolumeKey, value);
        PlayerPrefs.Save();
        return value;
    }

    public float OnMusicVolumeChanged(float value)
    {
        soundMixer.SetMusicVolume(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
        return value;
    }
}
