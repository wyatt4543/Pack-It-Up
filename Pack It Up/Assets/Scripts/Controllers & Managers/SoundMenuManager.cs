using UnityEngine;
using UnityEngine.UI;

public class SoundMenuManager : MonoBehaviour
{
    private GameObject soundSettingsMenu;
    private SoundMixerManager soundMixer;
    private Slider masterVolume;
    private Slider SFXVolume;
    private Slider musicVolume;

    private void Start()
    {
        soundSettingsMenu = GameObject.Find("SettingsCanvas/SoundSettings");
        soundMixer = GameObject.Find("SoundMixerManager").GetComponent<SoundMixerManager>();
        masterVolume = soundSettingsMenu.transform.Find("Master Volume").gameObject.GetComponent<Slider>();
        SFXVolume = soundSettingsMenu.transform.Find("SFX Volume").gameObject.GetComponent<Slider>();
        musicVolume = soundSettingsMenu.transform.Find("Music Volume").gameObject.GetComponent<Slider>();
        masterVolume.onValueChanged.AddListener(
            delegate { soundMixer.SetMasterVolume(masterVolume.value); }
        );
        SFXVolume.onValueChanged.AddListener(
            delegate { soundMixer.SetSFXVolume(SFXVolume.value); }
        );
        musicVolume.onValueChanged.AddListener(
            delegate { soundMixer.SetMusicVolume(musicVolume.value); }
        );
    }
}
