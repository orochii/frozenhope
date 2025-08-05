using System;
using System.Collections.Generic;
using Godot;

public class AudioManager {
    const int MUSIC_CHANNELS = 2;
    const float MIN_DECIBELS = -80f;
    const float CHANGE_FADE_DUR = 1.2f;
    const string BUS_SFX = "SFX";
    const string BUS_BGM = "BGM";
    static AudioManager instance;
    private Node root;
    private AudioLibrary library;
    private AudioStreamPlayer[] musicInstances;
    private int currentMusicInstance;
    private AudioStreamPlayer jinglePlayer;
    private Dictionary<StringName,AudioStreamPlayer> audioInstances;
    private AudioManager() {
        root = new Node();
        audioInstances = new Dictionary<StringName, AudioStreamPlayer>();
        Main.Instance.AddChild(root);
        // Add two instances for music (fades).
        musicInstances = new AudioStreamPlayer[MUSIC_CHANNELS];
        for (int i = 0; i < MUSIC_CHANNELS; i++) {
            var m = new AudioStreamPlayer();
            m.Name = string.Format("Music{0}",i);
            m.Bus = BUS_BGM;
            m.ProcessMode = Node.ProcessModeEnum.Always;
            musicInstances[i] = m;
            root.AddChild(m);
        }
        // Add one instance for jingles.
        jinglePlayer = new AudioStreamPlayer();
        jinglePlayer.Name = "Jingle";
        jinglePlayer.Bus = BUS_BGM;
        jinglePlayer.ProcessMode = Node.ProcessModeEnum.Always;
        jinglePlayer.Finished += OnJingleEnds;
        root.AddChild(jinglePlayer);
        // Add one instance per system sound.
        library = OZResourceLoader.Load<AudioLibrary>("res://Data/audio_library.tres");
        if (library != null) {
            foreach(var entry in library.entries) {
                // Spawn and configure.
                var s = new AudioStreamPlayer();
                s.Name = entry.id;
                root.AddChild(s);
                s.Stream = entry.stream;
                s.VolumeDb = entry.volume;
                s.PitchScale = entry.pitch;
                s.ProcessMode = Node.ProcessModeEnum.Always;
                s.Bus = BUS_SFX;
                // Register to dictionary.
                audioInstances.Add(entry.id, s);
            }
        }
    }
    private void OnJingleEnds() {
        ResumeMusic();
    }
    public static void Init() {
        instance = new AudioManager();
    }
    public static void PlaySystemSound(StringName id) {
        if (instance == null) return;
        if (instance.audioInstances.TryGetValue(id, out var snd)) {
            snd.Play();
        }
    }
    public static void PlaySound2D(Vector2 position, AudioStream stream, float volume=0,float pitch=1) {
        if (Main.Instance == null) return;
        if (stream == null) return;
        var snd = new AudioStreamPlayer2D();
        Main.Instance.AddChild(snd);
        snd.Bus = BUS_SFX;
        snd.Stream = stream;
        snd.VolumeDb = volume;
        snd.PitchScale = pitch;
        snd.GlobalPosition = position;
        snd.Play();
        snd.Finished += snd.QueueFree;
    }
    Tween musicFade;
    public static void PlaySound2D(Vector2 position, AudioEntry entry) {
        if (entry == null) return;
        PlaySound2D(position, entry.stream, entry.volume, entry.pitch);
    }
    public static void PlayMusic(AudioStream music, float volume, float pitch)
    {
        if (instance == null) return;
        if (music == instance.CurrentMusicChannel.Stream) {
            instance.CurrentMusicChannel.VolumeDb = volume;
            instance.CurrentMusicChannel.PitchScale = pitch;
            return;
        }
        // Fade current song.
        FadeOutMusic(CHANGE_FADE_DUR);
        // Change channel
        instance.currentMusicInstance = (instance.currentMusicInstance + 1) % MUSIC_CHANNELS;
        // Play new song
        instance.CurrentMusicChannel.Stream = music;
        instance.CurrentMusicChannel.VolumeDb = volume;
        instance.CurrentMusicChannel.PitchScale = pitch;
        instance.CurrentMusicChannel.Play();
        instance.CurrentMusicChannel.StreamPaused = false;
    }
    public static void PlayMusic(AudioEntry entry) {
        if (entry == null) {
            StopMusic();
            return;
        }
        PlayMusic(entry.stream, entry.volume, entry.pitch);
    }
    public static void FadeOutMusic(float duration) {
        if (instance == null) return;
        if (instance.musicFade != null) instance.musicFade.Kill();
        instance.musicFade = instance.root.CreateTween();
        instance.musicFade.TweenProperty(instance.CurrentMusicChannel, "volume_db", MIN_DECIBELS, duration);
        instance.musicFade.TweenProperty(instance.CurrentMusicChannel, "playing", false, 0.1);
        instance.musicFade.Play();
    }
    public static void FadeInMusic(float duration, float volume) {
        if (instance == null) return;
        if (instance.musicFade != null) instance.musicFade.Kill();
        instance.musicFade = instance.root.CreateTween();
        instance.CurrentMusicChannel.VolumeDb = MIN_DECIBELS;
        instance.CurrentMusicChannel.Play();
        instance.musicFade.TweenProperty(instance.CurrentMusicChannel, "volume_db", volume, duration);
        instance.musicFade.Play();
    }
    public static void StopMusic() {
        if (instance == null) return;
        instance.CurrentMusicChannel.Playing = false;
    }
    public static void PauseMusic() {
        if (instance == null) return;
        instance.CurrentMusicChannel.StreamPaused = true;
    }
    public static void ResumeMusic() {
        if (instance == null) return;
        instance.CurrentMusicChannel.StreamPaused = false;
    }
    public static void PlayJingle(AudioStream music, float volume, float pitch) {
        if (instance == null) return;
        PauseMusic();
        instance.jinglePlayer.Stream = music;
        instance.jinglePlayer.VolumeDb = volume;
        instance.jinglePlayer.PitchScale = pitch;
        instance.jinglePlayer.Play();
    }
    public static void PlayJingle(AudioEntry entry) {
        if (entry == null) return;
        PlayJingle(entry.stream, entry.volume, entry.pitch);
    }
    public AudioStreamPlayer CurrentMusicChannel => musicInstances[currentMusicInstance];
}