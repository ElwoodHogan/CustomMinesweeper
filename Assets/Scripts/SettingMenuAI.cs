using UnityEngine;
using UnityEngine.UI;
using static FrontMan;
using UnityEngine.Audio;

public class SettingMenuAI : MonoBehaviour
{
    [SerializeField] Slider MoveSpeed;
    [SerializeField] Slider PanSpeed;
    [SerializeField] Slider ScrollSpeed;
    [SerializeField] Toggle CameraClamp;
    [SerializeField] GameObject returnButton;
    public static SettingMenuAI SM;

    [SerializeField] AudioMixer MusicMixer;
    [SerializeField] AudioMixer SoundMixer;
    private void Awake()
    {
        SM = this;
    }

    private void Update()
    {
        returnButton.SetActive(FM.playing);
    }
    public float GetMS()
    {
        return 8 * (Mathf.Pow(MoveSpeed.value, 2)) + 1;
    }
    public float GetPS()
    {
        return 8 * (Mathf.Pow(PanSpeed.value, 2)) + 1;
    }

    public float GetSS()
    {
        return 8 * (Mathf.Pow(ScrollSpeed.value, 2)) + 1;
    }

    public bool GetCC()
    {
        return CameraClamp.isOn;
    }

    public void SetMusicLevel(float slidervalue)
    {
        MusicMixer.SetFloat("Music volume", Mathf.Log10(slidervalue) * 20);
    }
    public void SetSoundLevel(float slidervalue)
    {
        SoundMixer.SetFloat("Sound volume", Mathf.Log10(slidervalue) * 20);
    }

}
