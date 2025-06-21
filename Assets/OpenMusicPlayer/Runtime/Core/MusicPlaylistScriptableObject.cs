using System.Collections.Generic;
using UnityEngine;

namespace OpenMusicPlayer.Core
{
    [CreateAssetMenu(menuName = "OpenMusicPlayer/Music Playlist")]
    public class MusicPlaylistScriptableObject : ScriptableObject
    {
        public List<MusicTrack> songs = new();
        public string title = "My Playlist";
        public bool shuffleByDefault;
        public bool loopByDefault = true;
        public float defaultVolume = 0.8f;
    }
}