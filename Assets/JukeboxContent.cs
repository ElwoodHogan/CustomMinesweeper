using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Jukebox;
using UnityEngine.UI;
using TMPro;

public class JukeboxContent : MonoBehaviour
{
    public TextMeshProUGUI songName;
    public int songIndex = 0;

    public Image barImage;

    public Color NotPlayingColor;
    public Color PlayingColor;
    public void Play()
    {
        JB.SwitchTrack(songIndex);
        JB.currentTrackIndex = songIndex;
        ChangeColors();
    }

    public void ChangeColors()
    {
        foreach (Transform contentItem in transform.parent)
        {
            contentItem.GetComponent<JukeboxContent>().barImage.color = NotPlayingColor;
        }
        barImage.color = PlayingColor;
    }

    public void Toggle(bool toggled)
    {
        JB.songList[songIndex].toggled = toggled;
    }
}
