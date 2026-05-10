using Godot;

public partial class DynamicAudioManager : Node
{
    // Assign your 4 AudioStreamPlayers in the Godot Inspector
    [Export] public AudioStreamPlayer[] musicStems;
    
    private int currentTrackIndex = 0;
    private Tween fadeTween;

    public override void _Ready()
    {
        if (musicStems == null || musicStems.Length == 0) return;

        // Start all tracks perfectly synced
        for (int i = 0; i < musicStems.Length; i++)
        {
            // Set all tracks to -80dB (muted) except the first one (0dB)
            musicStems[i].VolumeDb = (i == currentTrackIndex) ? 0f : -80f;
            musicStems[i].Play();
        }
    }

    // Call this method when the player enters a new area
    public void SwitchToTrack(int trackIndex, float fadeDuration = 2.0f)
    {
        // Don't do anything if the track is already playing or index is invalid
        if (trackIndex == currentTrackIndex || trackIndex < 0 || trackIndex >= musicStems.Length) 
            return;

        // If a crossfade is already happening, stop it so we don't get audio glitches
        fadeTween?.Kill();
        
        // Create a new tween and set it to parallel so all volumes change at the exact same time
        fadeTween = CreateTween().SetParallel(true);

        for (int i = 0; i < musicStems.Length; i++)
        {
            // The target track fades up to 0dB, all others fade out to -80dB
            float targetVolume = (i == trackIndex) ? 0f : -80f;
            
            // "volume_db" is the string path for the VolumeDb property
            fadeTween.TweenProperty(musicStems[i], "volume_db", targetVolume, fadeDuration)
                     // Using Sine or Quad easing makes the audio crossfade sound much more natural
                     .SetTrans(Tween.TransitionType.Sine)
                     .SetEase(Tween.EaseType.InOut);
        }

        currentTrackIndex = trackIndex;
    }
}