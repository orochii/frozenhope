using System;
using System.Diagnostics;
using Godot;
using Newtonsoft.Json;

[Serializable]
public class SettingsData
{
    public int VolumeMaster { get; set; } = 8;
    public int VolumeBGM { get; set; } = 8;
    public int VolumeAMB { get; set; } = 8;
    public int VolumeSFX { get; set; } = 8;
    public int VolumeVoice { get; set; } = 8;
    public int LowHealthAlert { get; set; } = 1;
    //
    public int ScreenResolution { get; set; } = 0;
    public int ScreenMode { get; set; } = 0;
    public int ScreenScale { get; set; } = 1;
    public string Language { get; set; } = "en";
    public bool Subtitles { get; set; } = true;
    public int ControlScheme { get; set; } = 0;
    public int AimMode { get; set; } = 0;
    // bindings?
}

public class GameSettings
{
    public object GetValue(string property)
    {
        var t = data.GetType();
        var p = t.GetProperty(property);
        return p.GetGetMethod().Invoke(data,null);
    }
    public void SetValue(string property, object val)
    {
        var t = data.GetType();
        var p = t.GetProperty(property);
        p.GetSetMethod().Invoke(data, new object[] { val });
        Refresh();
    }
    public Type GetValueType(string property)
    {
        var t = data.GetType();
        var p = t.GetProperty(property);
        if (p == null)
        {
            GD.Print("[GetValueType] Missing property ",property);
        }
        return p?.PropertyType;
    }
    private const string Filename = "user://config.dat";
    private static Vector2I[] SCREEN_SIZES = new Vector2I[] {
        new Vector2I(320,240),
        new Vector2I(640,480),
    };
    SettingsData data;
    public GameSettings()
    {
        if (FileExists()) Load();
        else data = new SettingsData();
        //
        Refresh();
    }
    public void Refresh()
    {
        RefreshVolume();
        RefreshScale();
        RefreshLanguage();
    }
    public int GetScreenWidth()
    {
        return SCREEN_SIZES[data.ScreenResolution].X;
    }
    public int GetScreenHeight()
    {
        return SCREEN_SIZES[data.ScreenResolution].Y;
    }
    public bool FileExists()
    {
        return FileAccess.FileExists(Filename);
    }
    public void Save()
    {
        var saveFile = FileAccess.Open(Filename, FileAccess.ModeFlags.Write);
        string json = JsonConvert.SerializeObject(data);
        saveFile.StoreString(json);
        saveFile.Close();
    }
    public void Load()
    {
        var saveFile = FileAccess.Open(Filename, FileAccess.ModeFlags.Read);
        string json = saveFile.GetAsText();
        data = JsonConvert.DeserializeObject<SettingsData>(json);
        saveFile.Close();
    }
    private void RefreshVolume()
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), ConvertUtils.PercToDb(data.VolumeMaster * 10));
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), ConvertUtils.PercToDb(data.VolumeBGM * 10));
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), ConvertUtils.PercToDb(data.VolumeSFX * 10));
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Ambient"), ConvertUtils.PercToDb(data.VolumeAMB * 10));
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Voices"), ConvertUtils.PercToDb(data.VolumeVoice * 10));
    }
    private void RefreshScale()
    {
        var size = SCREEN_SIZES[data.ScreenResolution];
        Resize(size.X, size.Y, data.ScreenScale + 1);
        SetMode(data.ScreenMode);
    }
    private void RefreshLanguage()
    {
        //var langs = TranslationServer.GetLoadedLocales();
        TranslationServer.SetLocale(data.Language);
    }
    private void Resize(int width, int height, int scale)
    {
        Vector2I screenSize = new Vector2I(width, height);
        // Window size
        var window = Main.Instance.GetWindow();
        window.ContentScaleSize = screenSize;
        window.Size = screenSize * scale;
        //
        CenterWindow();
        // Viewport size
        //Main.Instance.viewport.Size2DOverride = screenSize;
        // UI: set parent and canvas scale
        //if necessary
    }
    private void CenterWindow()
    {
        var screenPosition = DisplayServer.ScreenGetPosition();
        var screenSize = DisplayServer.ScreenGetSize();
        var windowSize = DisplayServer.WindowGetSize();
        DisplayServer.WindowSetPosition(screenPosition + ((screenSize - windowSize) / 2));
    }
    private void SetMode(int mode)
    {
        switch (mode)
        {
            case 0: //windowed
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                Main.Instance.GetWindow().Borderless = false;
                break;
            case 1: //borderless window
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                Main.Instance.GetWindow().Borderless = true;
                break;
            case 2: //fullscreen
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                Main.Instance.GetWindow().Borderless = false;
                break;
        }
    }
}
