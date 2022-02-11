using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Jukebox : MonoBehaviour
{
    public static Jukebox JB;
    public List<Song> songList;
    public int songAmount;
    public AudioSource AS;

    public int currentTrackIndex = 0;
    public float trackSwitchDelay = 2;

    public bool ShuffleMode = false;

    public Transform JukeboxContentParent;

    public JukeboxContent JukeboxContentItemPrefab;

    public void SetShuffleMode(bool state)
    {
        ShuffleMode = state;
    }

    private void Awake()
    {
        JB = this;
        AS = GetComponent<AudioSource>();
        timeTillNextSong = AS.clip.length + trackSwitchDelay;
    }

    public void PlayNextSong()
    {
        if (ShuffleMode)
        {
            if (!songList.Any(song => song.toggled)) return;  //if no songs are selected, return
            var rand = new System.Random();
            List<Song> toggledSonglist = songList.Where(song => song.toggled).ToList();
            print(toggledSonglist.Count);
            currentTrackIndex = songList.IndexOf(toggledSonglist.RandomPicker());
            AS.clip = songList[currentTrackIndex].audio;
            timeTillNextSong = AS.clip.length + trackSwitchDelay;
            AS.Play();
            JukeboxContentParent.GetChild(currentTrackIndex).GetComponent<JukeboxContent>().ChangeColors();
        }
        else
        {
            if (!songList.Any(song => song.toggled)) return;  //if no songs are selected, return

            currentTrackIndex = (currentTrackIndex + 1) % songList.Count;

            if (!songList[currentTrackIndex].toggled) PlayNextSong();
            else
            {
                AS.clip = songList[currentTrackIndex].audio;
                timeTillNextSong = AS.clip.length + trackSwitchDelay;
                AS.Play();
                JukeboxContentParent.GetChild(currentTrackIndex).GetComponent<JukeboxContent>().ChangeColors();
            }
        }
    }

    public void PlayPreviousSong()
    {
        currentTrackIndex = (currentTrackIndex - 1 + songList.Count) % songList.Count;
        AS.clip = songList[currentTrackIndex].audio;
        timeTillNextSong = AS.clip.length + trackSwitchDelay;
        AS.Play();
        JukeboxContentParent.GetChild(currentTrackIndex).GetComponent<JukeboxContent>().ChangeColors();
    }

    public void SwitchTrack(int trackIndex)
    {
        AS.clip = songList[trackIndex].audio;
        timeTillNextSong = AS.clip.length + trackSwitchDelay;
        AS.Play();
        JukeboxContentParent.GetChild(currentTrackIndex).GetComponent<JukeboxContent>().ChangeColors();
    }

    public float timeTillNextSong;
    private void Update()
    {
        if (!paused) timeTillNextSong -= Time.deltaTime;
        if (timeTillNextSong < 0) PlayNextSong();
    }

    bool paused = false;
    public void TogglePause()
    {
        paused = !paused;
        if (AS.isPlaying) AS.Pause();
        else AS.Play();
    }
    [Button]
    public void CreateAudioList()
    {
        songList = new List<Song>();
        for (int i = 0; i < songAmount; i++)
        {
            songList.Add(new Song());
        }
    }
    [Button]
    public void GenerateContent()
    {
        foreach (var song in songList)
        {
            JukeboxContent jc = Instantiate(JukeboxContentItemPrefab, JukeboxContentParent);
            jc.songName.text = song.title;
            jc.songIndex = songList.IndexOf(song);
        }
    }
}

[Serializable]
public class Song
{
    public string title = "insert name";
    public AudioClip audio;
    public bool toggled = true;
}
