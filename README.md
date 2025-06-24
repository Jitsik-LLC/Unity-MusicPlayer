# OpenMusicPlayer - Friendly Music Player 🎵 for Unity
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity UPM](https://img.shields.io/badge/Unity-UPM-blue)](https://docs.unity3d.com/Manual/upm-ui.html) <!-- TODO: UPDATE UPM -->
## Overview
OpenMusicPlayer is a flexible, event-driven music player system for Unity that provides robust playlist management, playback control, and seamless UI integration. Designed for both simple implementations and complex audio systems, it's perfect for games, simulations, and VR experiences.
## Features
* 🎵 Dynamic playlist management with ScriptableObjects
* 🔄 Multiple playback modes (loop, shuffle, sequential)
* 🎚️ Volume control with smooth transitions
* 🎛️ UI integration with visual feedback
* 🚀 Event-driven architecture for easy extensibility
* 📦 Plug-and-play prefab system
* 🎮 VR-ready design

## Installation
Add to your Unity project via UPM:
```shell
openupm add https://github.com/yourusername/OpenMusicPlayer.git
```
## Quick Start
1. Create a Playlist
   - Right-click in Project window → Create → Audio → Music Playlist
   - Add songs and configure settings
2. Add to Scene
   - Drag MusicPlayerCore.prefab into your scene.
   - Assign your playlist in the Inspector.
3. Add UI
   - Use the included MusicPlayerUI.prefab or create your own.
   - Connect UI elements to the MusicPlayerCore events
4. Control Playback
    ```csharp
    // In your game code
    MusicPlayerCore player = FindObjectOfType<MusicPlayerCore>();
    player.Play();
    player.NextTrack();
    player.SetVolume(0.7f);
    ```

## Samples 
Import samples from the Package Manager details on Open Music Player. 
1. If not found in project, you will be asked to Import `TextMeshPro` Essentials because we are using the base font.
2. Reload any current Sample scene if already loaded.

## Best Practices
1. **Use ScriptableObjects** for playlist management - makes it easy to swap playlists.
2. **Subscribe to events** rather than polling for state changes.
3. **Use the default song** to show "no music" state in UI.
4. **Adjust progressUpdateInterval** based on performance needs.
5. **Use volumeDelta** for consistent volume steps.

## Advanced Usage
### Custom UI Implementation
```csharp
public class CustomPlayerUI : MonoBehaviour
{
    [SerializeField] private Image playIcon;
    [SerializeField] private Slider progressBar;
    
    private MusicPlayerCore player;
    
    void Start()
    {
        player = FindObjectOfType<MusicPlayerCore>();
        player.OnPlaybackStateChanged += state => {
            playIcon.color = state == PlaybackState.Playing ? Color.green : Color.white;
        };
        
        player.OnPlaybackProgress += progress => {
            progressBar.value = progress;
        };
    }
}
```
### Playlist Management
```csharp
// Create playlist at runtime
MusicPlaylistScriptableObject runtimePlaylist = ScriptableObject.CreateInstance<MusicPlaylistScriptableObject>();
runtimePlaylist.songs = new List<Song>(customSongs);
player.LoadPlaylist(runtimePlaylist);

// Add songs dynamically
player.AddSong(new Song {
    title = "New Song",
    artist = "Artist",
    clip = newAudioClip
});
```
### VR Integration Example
```csharp
public class VRMusicController : MonoBehaviour
{
    [SerializeField] private XRController controller;
    [SerializeField] private MusicPlayerCore player;
    
    void Update()
    {
        if(controller.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed) && pressed)
        {
            player.NextTrack();
        }
    }
}
```
## Troubleshooting

| Issue                 | Solution                                           |
| --------------------- | -------------------------------------------------- |
| No sound              | Check AudioClip assignment in playlist             |
| UI not updating       | Verify event subscriptions                         |
| Playback stuttering   | Increase `endOfSongThreshold`                      |
| Progress bar jitter   | Add `SliderInteractController` component           |
| Shuffle not working   | Call `GeneratePlayOrder()` after playlist changes |

## Contribution
Contributions are welcome! Please follow these guidelines:
1. Fork the repository.
2. Create a feature branch.
3. Submit a pull request.
4. Include tests for new functionality.

## License
MIT License - Free for personal and commercial use.
