using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;


public static class GamePreference
{




    public static int SFXVolume
    {
        get => Get(nameof(SFXVolume), 50);
        set => Set(nameof(SFXVolume), value);
    }
    public static int MusicVolume
    {
        get => Get(nameof(MusicVolume), 50);
        set => Set(nameof(MusicVolume), value);
    }
    public static int VoiceVolume
    {
        get => Get(nameof(VoiceVolume), 50);
        set => Set(nameof(VoiceVolume), value);
    }
    public static int AmbianceVolume
    {
        get => Get(nameof(AmbianceVolume), 50);
        set => Set(nameof(AmbianceVolume), value);
    }
    public static bool FullScreen
    {
        get => Get(nameof(FullScreen), true);
        set => Set(nameof(FullScreen), value);
    }
    public static int Width
    {
        get => Get(nameof(Width), 1920);
        set => Set(nameof(Width), value);
    }
    public static int Height
    {
        get => Get(nameof(Height), 1080);
        set => Set(nameof(Height), value);
    }

    public static Language Language
    {
        get => Get(nameof(Language), Language.Français);
        set => Set(nameof(Language), value);
    }

    private static Dictionary<string, object> data = new Dictionary<string, object>();
    public static string filePath = "";

    public static void SetPath(string path) => filePath = path;

    public static void SaveToFile()
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            Dictionary<string, object> sanitizedData = new Dictionary<string, object>(data);
            System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(sanitizedData, Formatting.Indented));
        }
    }

    public static void LoadFromFile()
    {
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            string jsonString = System.IO.File.ReadAllText(filePath);
            {
                Dictionary<string, object> sanitizedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                data = sanitizedData
                ?? data;
            }
        }
    }


    private static void Set<T>(string key, T value)
    {
        data[key] = value;
        SaveToFile();
    }

    private static T Get<T>(string key, T defaultValue = default)
    {
        if (data.TryGetValue(key, out object value))
        {
            if (value is T tValue)
                return tValue;

            try { return (T)System.Convert.ChangeType(value, typeof(T)); }
            catch { }
        }
        return defaultValue;
    }

    public static void SetFieldByName(string nameOfField, object val)
    {
        if (data.TryGetValue(nameOfField, out object existing))
        {
            var existingType = existing?.GetType();
            var newType = val?.GetType();

            if (existingType == newType || (val != null && existingType?.IsAssignableFrom(newType) == true))
            {
                data[nameOfField] = val;
                SaveToFile();
            }
            else
            {
                try
                {
                    object converted = System.Convert.ChangeType(val, existingType);
                    data[nameOfField] = converted;
                    SaveToFile();
                }
                catch
                {
                    Debug.LogWarning($"[GamePreference] <color=yellow>Failed to assign value to '{nameOfField}'</color> : type mismatch ({newType} to {existingType})");
                }
            }
        }
        else
        {
            data[nameOfField] = val;
            SaveToFile();
        }
    }

}
