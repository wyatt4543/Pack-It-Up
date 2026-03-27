using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SoundMenuManager : MonoBehaviour
{
    private GameObject soundSettingsMenu;
    private SoundMixerManager soundMixer;
    private Slider masterVolume;
    private Slider SFXVolume;
    private Slider musicVolume;

    private const string MasterVolumeKey = "MasterVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string MusicVolumeKey = "MusicVolume";

    private void Start()
    {
        soundSettingsMenu = GameObject.Find("SettingsCanvas/SettingsStuff/SoundSettings");
        soundMixer = GameObject.Find("SoundMixerManager").GetComponent<SoundMixerManager>();
        masterVolume = soundSettingsMenu.transform.Find("Master Volume").gameObject.GetComponent<Slider>();
        SFXVolume = soundSettingsMenu.transform.Find("SFX Volume").gameObject.GetComponent<Slider>();
        musicVolume = soundSettingsMenu.transform.Find("Music Volume").gameObject.GetComponent<Slider>();

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
