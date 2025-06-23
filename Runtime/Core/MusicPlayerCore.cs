using System;
using System.Collections.Generic;
using OpenMusicPlayer.Utilities;
using UnityEngine;

namespace OpenMusicPlayer.Core
{
    public enum PlaybackState
    {
        Stopped,   // Not playing anything
        Playing,   // Actively playing audio
        Paused     // Playback paused (can be resumed)
    }

    public class MusicPlayerCore : MonoBehaviour
    {
        [Header("Playlist")]
        public MusicPlaylistScriptableObject playlistAsset;
        public List<MusicTrack> playlist = new();

        [Header("Playback settings")]
        public bool loop = true; // Loops musicTrack
        public bool shuffle;
        [Range(0f, 1f)] public float volume = 0.8f;
        
        [Header("Advanced Settings")]
        [SerializeField] private MusicTrack defaultTrack;
        [SerializeField] private bool loopPlaylist = true;
        [SerializeField] private bool playOnStart;
        [SerializeField] private float volumeDelta;
        
        [Header("Sampling Settings")]
        [SerializeField] [Tooltip("Threshold in seconds")]  
        private float endOfSongThreshold = 0.1f;
        [SerializeField] [Tooltip("Interval in seconds")]  
        private float progressUpdateInterval = 0.1f;
        
        // Playback state tracking
        private PlaybackState _currentState = PlaybackState.Stopped;
        public PlaybackState CurrentState => _currentState;
        
        // Internal components
        private AudioSource _audioSource;
        private int _currentIndex;
        private readonly List<int> _playOrder = new();
        private float _progressUpdateTimer;
        
        // Events
        public event Action<bool> OnLoopChanged;
        public event Action<float> OnPlaybackProgress;
        public event Action<PlaybackState> OnPlaybackStateChanged;
        public event Action<MusicPlaylistScriptableObject> OnPlaylistLoaded;
        public event Action OnPlaylistUpdated;
        public event Action OnReachedPlaylistEnd;
        public event Action OnReachedPlaylistStart;
        public event Action<bool> OnShuffleChanged;
        public event Action<MusicTrack> OnSongChanged;
        public event Action<float> OnVolumeChanged;

        private MusicTrack CurrentTrack => 
            (playlist.Count == 0 || _currentIndex >= _playOrder.Count) ? 
                defaultTrack : 
                playlist[_playOrder[_currentIndex]];
        
        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = loop;
        }

        private void Start()
        {
            InitializePlaylist();
            SetVolume(volume);

            if (playlistAsset != null)
            {
                LoadPlaylist(playlistAsset);
            }
            
            // Set initial state based on playOnStart
            if (playlist.Count == 0 || !playOnStart)
            {
                SetState(PlaybackState.Stopped);
            }
        }

        private void Update()
        {
            HandlePlaybackProgress();
        }

        public void AddSong(MusicTrack musicTrack)
        {
            playlist.Add(musicTrack);
            GeneratePlayOrder();
            OnPlaylistUpdated?.Invoke();
            
            // Autoplay if we were stopped with no songs
            if (playlist.Count == 1 && _currentState == PlaybackState.Stopped)
            {
                Play();
            }
        }
        
        public void DecreaseVolume()
        {
            SetVolume(volume - volumeDelta);
        }

        public void IncreaseVolume()
        {
            SetVolume(volume + volumeDelta);
        }
        
        public void LoadPlaylist(MusicPlaylistScriptableObject newPlaylist)
        {
            // Clear existing playlist
            playlist.Clear();
    
            // Add new songs
            if (newPlaylist.songs is { Count: > 0 })
            {
                playlist.AddRange(newPlaylist.songs);
            }
    
            // Apply settings
            shuffle = newPlaylist.shuffleByDefault;
            loop = newPlaylist.loopByDefault;
            SetVolume(newPlaylist.defaultVolume);
    
            // Initialize
            GeneratePlayOrder();
            OnPlaylistUpdated?.Invoke();
    
            // Update state
            if (newPlaylist.songs.Count > 0)
            {
                PlayCurrent();
            }
            else
            {
                SetState(PlaybackState.Stopped);
            }
            
            OnPlaylistLoaded?.Invoke(newPlaylist);
        }
        
        public void NextTrack()
        {
            if (_playOrder.Count == 0) return;
    
            var nextIndex = _currentIndex + 1;
    
            // Reached end of playlist
            if (nextIndex >= _playOrder.Count)
            {
                if (!loopPlaylist)
                {
                    // Stop playback without changing index
                    SetState(PlaybackState.Stopped);
                    OnReachedPlaylistEnd?.Invoke();
                    return;
                }
                nextIndex = 0; // Loop to beginning
            }

            _currentIndex = nextIndex;
            PlayCurrent();
        }

        public void Pause()
        {
            if (_currentState != PlaybackState.Playing) return;
            
            _audioSource.Pause();
            SetState(PlaybackState.Paused);
        }

        public void Play()
        {
            if (playlist.Count == 0) return;
            
            if (_currentState == PlaybackState.Paused)
            {
                // Resume from pause
                _audioSource.Play();
                SetState(PlaybackState.Playing);
            }
            else
            {
                // Start fresh playback
                PlayCurrent();
            }
        }
        
        public void PreviousTrack()
        {
            if (_playOrder.Count == 0) return;
    
            var prevIndex = _currentIndex - 1;
    
            // Reached start of playlist
            if (prevIndex < 0)
            {
                if (!loopPlaylist)
                {
                    // Stop playback without changing index
                    SetState(PlaybackState.Stopped);
                    OnReachedPlaylistStart?.Invoke();
                    return;
                }
                prevIndex = _playOrder.Count - 1; // Loop to end
            }
            
            _currentIndex = prevIndex;
            PlayCurrent();
        }
        
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            UpdateVolume();
            OnVolumeChanged?.Invoke(volume);
        }

        public void Stop()
        {
            SetState(PlaybackState.Stopped);
        }

        public void TogglePlayPause()
        {
            switch (_currentState)
            {
                case PlaybackState.Playing:
                    Pause();
                    break;
                case PlaybackState.Paused:
                    Play();
                    break;
                case PlaybackState.Stopped:
                    Play();
                    break;
            }
        }

        public void ToggleLoop()
        {
            loop = !loop;
            _audioSource.loop = loop;
            OnLoopChanged?.Invoke(loop);
        }
        
        public void ToggleShuffle()
        {
            shuffle = !shuffle;
            GeneratePlayOrder();
            OnShuffleChanged?.Invoke(shuffle);
        }
        
        private void GeneratePlayOrder()
        {
            _playOrder.Clear();
            for (var i = 0; i < playlist.Count; i++) _playOrder.Add(i);
            if (shuffle) _playOrder.Shuffle();
        }
        
        private void HandlePlaybackProgress()
        {
            // Only track progress when playing
            if (_currentState != PlaybackState.Playing || !_audioSource.clip) return;
            
            _progressUpdateTimer -= Time.deltaTime;
            
            if (_progressUpdateTimer <= 0)
            {
                OnPlaybackProgress?.Invoke(_audioSource.time / _audioSource.clip.length);
                _progressUpdateTimer = progressUpdateInterval;
            }

            // Handle musicTrack completion
            if (!loop && _audioSource.time >= _audioSource.clip.length - endOfSongThreshold)
            {
                NextTrack();
            }
        }

        private void InitializePlaylist()
        {
            GeneratePlayOrder();
            if (playOnStart && playlist.Count > 0)
            {
                PlayCurrent();
            }
            else
            {
                // Set initial state
                SetState(PlaybackState.Stopped);
            }
        }
        
        private void PlayCurrent()
        {
            if (playlist.Count == 0 || _currentIndex >= _playOrder.Count)
            {
                SetState(PlaybackState.Stopped);
                return;
            }
            
            _audioSource.clip = CurrentTrack.clip;
            _audioSource.Play();
            
            OnSongChanged?.Invoke(CurrentTrack);
            SetState(PlaybackState.Playing);
        }

        private void SetState(PlaybackState newState)
        {
            if (_currentState == newState) return;
            
            _currentState = newState;
            
            switch (newState)
            {
                case PlaybackState.Stopped:
                    _audioSource.Stop();
                    _audioSource.time = 0;
                    OnSongChanged?.Invoke(defaultTrack);
                    break;
                
                case PlaybackState.Paused:
                    // Keep current musicTrack info
                    break;
                case PlaybackState.Playing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
            
            OnPlaybackStateChanged?.Invoke(newState);
        }
        
        private void UpdateVolume()
        {
            if (_audioSource != null) _audioSource.volume = volume;
        }
    }
}