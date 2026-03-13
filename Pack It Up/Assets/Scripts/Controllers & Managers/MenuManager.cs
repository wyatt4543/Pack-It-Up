using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private bool menuOpen = false;
    private GameObject soundMenu;
    private SoundMixerManager soundMixer;
    private Slider masterVolume;
    private Slider SFXVolume;
    private Slider musicVolume;
    private Button backButton;

    private void Start()
    {
        soundMenu = GameObject.Find("SoundMenuCanvas");
        soundMenu.SetActive(false);
        soundMixer = GameObject.Find("SoundMixerManager").GetComponent<SoundMixerManager>();
        masterVolume = soundMenu.transform.Find("Master Volume").gameObject.GetComponent<Slider>();
        SFXVolume = soundMenu.transform.Find("SFX Volume").gameObject.GetComponent<Slider>();
        musicVolume = soundMenu.transform.Find("Music Volume").gameObject.GetComponent<Slider>();
        backButton = soundMenu.transform.Find("BackButton").gameObject.GetComponent<Button>();
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
            delegate { UpdateSoundMenu(); }
        );
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UpdateSoundMenu();
        }
    }

    public void UpdateSoundMenu()
    {
        menuOpen = !menuOpen;
        soundMenu.SetActive(menuOpen);
    }

    // pause function
    public void Pause()
    {

    }

    // unpause function
    public void UnPause()
    {

    }
}
