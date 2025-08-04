using Godot;

[GlobalClass]
public partial class AudioEntry : Resource {
    [Export] public StringName id;
    [Export] public AudioStream stream;
    [Export] public float volume = 0;
    [Export] public float pitch = 1;
}