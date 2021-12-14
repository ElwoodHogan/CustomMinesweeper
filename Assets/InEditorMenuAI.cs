using UnityEngine;
using UnityEngine.UI;

public class InEditorMenuAI : MonoBehaviour
{
    [SerializeField] Text Height;
    [SerializeField] Text Width;
    [SerializeField] Text Mines;
    [SerializeField] Text ExitText;
    public int height;
    public int width;
    public int mines;
    public static InEditorMenuAI IEM;
    private void Awake()
    {
        IEM = this;
    }

}
