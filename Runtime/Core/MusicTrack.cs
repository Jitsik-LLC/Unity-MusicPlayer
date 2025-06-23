using UnityEngine;

namespace OpenMusicPlayer.Core
{
    [System.Serializable]
    public class MusicTrack
    {
        public string title = "Untitled";
        public string artist = "Unknown Artist";
        public AudioClip clip;
        public Sprite coverArt;
        public string url;
        public string license = "PD"; // CC0 1.0 Universal, etc.
    }
}