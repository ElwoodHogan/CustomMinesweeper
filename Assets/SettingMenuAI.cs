using UnityEngine;
using UnityEngine.UI;
using static FrontMan;

public class SettingMenuAI : MonoBehaviour
{
    [SerializeField] Slider MoveSpeed;
    [SerializeField] Slider PanSpeed;
    [SerializeField] Slider ScrollSpeed;
    [SerializeField] Toggle CameraClamp;
    [SerializeField] GameObject returnButton;
    public static SettingMenuAI SM;
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

}
