using OpenMusicPlayer.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OpenMusicPlayer.Samples.HUD
{
    public class MusicPlayerUI :MonoBehaviour
    {
        [Header("MusicTrack")] 
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI artistText;
        [SerializeField] private Image coverImage;
        
        [Header("Playlist")]
        [SerializeField]  private TextMeshProUGUI playlistTitleText;
        
        [Header("Playback")]
        [SerializeField] private Slider progressSlider;
        
        [Header("Controls")]
        [SerializeField] private Button decreaseVolumeButton;
        [SerializeField] private Button increaseVolumeButton;
        [SerializeField] private Button loopButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button playPauseButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button shuffleButton;
        [SerializeField] private Slider volumeSlider;
        
        [Header("Defaults ")]
        [SerializeField] private Sprite defaultCover;
        [SerializeField] private Color activeStateColor = Color.green;
        [SerializeField] private Color inactiveStateColor = Color.white;
        [HideInInspector] public char pauseIconCode = ('\uf04c');
        [HideInInspector] public char playIconCode = ('\uf04b');

        private MusicPlayerCore _player;
        private SliderInteractController _progressInteractController;
        private TextMeshProUGUI _loopButtonTextIcon;
        private TextMeshProUGUI _playPauseIcon;
        private TextMeshProUGUI _shuffleButtonTextIcon;
        
        private void Awake()
        {
            _player = FindObjectOfType<MusicPlayerCore>();
            
            // Add Custom Slider Interactive Controller
            _progressInteractController = progressSlider.gameObject.AddComponent<SliderInteractController>();
            // Get button text child for control indicators
            _loopButtonTextIcon = loopButton.GetComponentInChildren<TextMeshProUGUI>();
            _playPauseIcon = playPauseButton.GetComponentInChildren<TextMeshProUGUI>();
            _shuffleButtonTextIcon = shuffleButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents(); 
        }

        private void InitializeUI()
        {
            // Add listeners to buttons.
            decreaseVolumeButton.onClick.AddListener(OnDecreaseVolume);
            increaseVolumeButton.onClick.AddListener(OnIncreaseVolume);
            loopButton.onClick.AddListener(OnLoopToggle);
            nextButton.onClick.AddListener(OnNext);
            playPauseButton.onClick.AddListener(OnPlayPause);
            previousButton.onClick.AddListener(OnPrevious);
            shuffleButton.onClick.AddListener(OnShuffleToggle);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            
            // Initialize control indicators.
            UpdateLoopVisual(_player.loop);
            UpdateShuffleVisual(_player.shuffle);
            volumeSlider.value = _player.volume;
        }
       
       private void SubscribeToEvents()
       {
           if (_player == null) return;

           _player.OnLoopChanged += UpdateLoopVisual;
           _player.OnPlaybackProgress += HandlePlaybackProgress;
           _player.OnPlaybackStateChanged += HandlePlaybackStateChanged;
           _player.OnPlaylistLoaded += HandlePlaylistLoaded;
           _player.OnPlaylistUpdated += HandlePlaylistUpdated;
           _player.OnReachedPlaylistEnd += HandleReachedPlaylistEnd;
           _player.OnReachedPlaylistStart += HandleReachedPlaylistStart;
           _player.OnShuffleChanged += UpdateShuffleVisual;
           _player.OnSongChanged += HandleSongChanged;
           _player.OnVolumeChanged += HandleVolumeChanged;
       }

       private void UnsubscribeFromEvents()
       {
           if (_player == null) return;

           _player.OnLoopChanged -= UpdateLoopVisual;
           _player.OnPlaybackProgress -= HandlePlaybackProgress;
           _player.OnPlaybackStateChanged -= HandlePlaybackStateChanged;
           _player.OnPlaylistLoaded -= HandlePlaylistLoaded;
           _player.OnPlaylistUpdated -= HandlePlaylistUpdated;
           _player.OnReachedPlaylistEnd -= HandleReachedPlaylistEnd;
           _player.OnReachedPlaylistStart -= HandleReachedPlaylistStart;
           _player.OnShuffleChanged -= UpdateShuffleVisual;
           _player.OnSongChanged -= HandleSongChanged;
           _player.OnVolumeChanged -= HandleVolumeChanged;
       }
       
       private void HandleSongChanged(MusicTrack musicTrack)
       {
           titleText.text = musicTrack.title;
           artistText.text = musicTrack.artist;
           coverImage.sprite = musicTrack.coverArt ? musicTrack.coverArt : defaultCover;
           progressSlider.value = 0;
       }

       private void HandlePlaybackStateChanged(PlaybackState newState)
       {
           var isPlaying = newState == PlaybackState.Playing;
           UpdatePlayButtonVisual(isPlaying);
       }

       private void HandlePlaybackProgress(float progress)
       {
           // Only update if not actively dragging the slider
           if (!_progressInteractController.IsDragging)
           {
               progressSlider.value = progress;
           }
       }
       private void HandlePlaylistLoaded(MusicPlaylistScriptableObject newPlaylist)
       {
           playlistTitleText.text = newPlaylist.title;
           UpdateShuffleVisual(_player.shuffle);
           UpdateLoopVisual(_player.loop);
       }

       private void HandlePlaylistUpdated()
       {
           // Update controls when playlist changes
           UpdateShuffleVisual(_player.shuffle);
           UpdateLoopVisual(_player.loop);
       }
       
       private void HandleReachedPlaylistStart()
       {
           Debug.Log($"Reached playlist start out of {_player.playlist.Count}");
       }

       private void HandleReachedPlaylistEnd()
       {
           Debug.Log($"Reached playlist end at {_player.playlist.Count}");
       }
       
       private void HandleVolumeChanged(float volume)
       {
           // Prevent recursive updates
           if (Mathf.Abs(volumeSlider.value - volume) > 0.01f)
           {
               volumeSlider.value = volume;
           }
       }
       
       private void UpdateLoopVisual(bool isActive)
       {
           _loopButtonTextIcon.color = isActive ? activeStateColor : inactiveStateColor;
       }

       private void UpdatePlayButtonVisual(bool isPlaying)
       {
           if (!_player) return;
           _playPauseIcon.text = (isPlaying ? pauseIconCode : playIconCode).ToString();
       }

       private void UpdateShuffleVisual(bool isActive)
       {
           _shuffleButtonTextIcon.color = isActive ? activeStateColor : inactiveStateColor;
       }
       
       // UI Event Handlers
        public void OnDecreaseVolume() => _player.DecreaseVolume();
        public void OnIncreaseVolume() => _player.IncreaseVolume();
        public void OnLoopToggle() => _player.ToggleLoop();
        public void OnNext() => _player.NextTrack();
        public void OnPlayPause() => _player.TogglePlayPause();
        public void OnPrevious() => _player.PreviousTrack();
        public void OnShuffleToggle() => _player.ToggleShuffle();
        private void OnVolumeChanged(float value) => _player.SetVolume(value);
    }
}
