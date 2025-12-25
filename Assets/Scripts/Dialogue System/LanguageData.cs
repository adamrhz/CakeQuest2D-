using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public enum Language
{
    Français,
    English
}
public class LanguageDataHolder
{

    public List<JsonData> Data = new List<JsonData>();
    private LanguageData languageData;

    public LanguageDataHolder(LanguageData languageData)
    {
        this.languageData = languageData;
    }

    public void SaveDataList(string path)
    {
        Data = languageData.translationData.Values.ToList();
        if (Data != null)
        {
            string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(path + ".json", jsonData);
            Debug.Log($"Data list saved to {path}");
        }
        else
        {
            Debug.LogError("Singleton or Data list is null, cannot save data.");
        }
    }
}
[Serializable]
public class LanguageData
{
    public static string INDICATION = "Indications";
    public static string CONTROLS = "ControlScheme";
    public static string MENUS = "MenuTranslations";

    public static UnityEvent OnLanguageLoaded = new UnityEvent();

    public List<JsonData> Data = new List<JsonData>();
    public Dictionary<string, JsonData> translationData;
    public Dictionary<string, string> GlobalColors;
    public List<GlobalColor> globalColors = new List<GlobalColor>();
    public static Language language = Language.Français;
    public Language localLanguage = Language.Français;
    public static Language defaultLanguage = Language.Français;
    private static LanguageData _singleton;
    public static LanguageData Singleton
    {
        get
        {
            if (_singleton == null)
            {
                return new LanguageData();
            }
            return _singleton;
        }
        private set
        {
            if (value != null)
            {
                _singleton = value;
                _singleton.SetGlobalDictionary();
            }
        }
    }


    public LanguageData()
    {

    }
    public static bool Loaded()
    {
        if (_singleton != null)
        {
            return _singleton.localLanguage == language;
        }
        return _singleton != null;
    }


    public void SetGlobalDictionary()
    {
        if (globalColors != null && globalColors.Count > 0)
        {
            if (GlobalColors == null)
            {
                GlobalColors = new Dictionary<string, string>();
            }

            foreach (var color in globalColors)
            {
                if (!GlobalColors.ContainsKey(color.key))
                {

                    GlobalColors.Add(color.key, color.value);
                }
            }

            // Optionally, you can log or debug the globalColors list
            // Debug.Log($"Global Colors converted: {JsonConvert.SerializeObject(GlobalColors)}");
        }
        else
        {
            if (GlobalColors == null)
            {
                GlobalColors = new Dictionary<string, string>();
            }
        }

        if (Data != null && Data.Count > 0)
        {
            if (translationData == null)
            {
                translationData = new Dictionary<string, JsonData>();
            }

            foreach (var translation in Data)
            {
                if (!translationData.ContainsKey(translation.dataId))
                {
                    translationData.Add(translation.dataId, translation);
                }
            }

            // Optionally, you can log or debug the globalColors list
            // Debug.Log($"Global Colors converted: {JsonConvert.SerializeObject(GlobalColors)}");
        }
        else
        {
            if (translationData == null)
            {
                translationData = new Dictionary<string, JsonData>();
            }
        }


    }

    public static Language GetLanguage()
    {
        return language;
    }

    public static bool SetLanguage(Language value)
    {
        if (language != value)
        {
            language = value;
            return true;
        }
        return false;
    }
    public bool SetLocalLanguage(Language value)
    {
        if (language != value)
        {
            language = value;
            return true;
        }
        return false;
    }
    public static string GetLanguageSuffix()
    {
        return $"{language.ToString().ToLower()}";
    }

    public void SaveDataList(string path)
    {
        LanguageDataHolder holder = new LanguageDataHolder(this);
        holder.SaveDataList(path);
    }
    public static LanguageData LoadGameData()
    {
        string languageSuffix = GetLanguageSuffix();



        TextAsset jsonFile = Resources.Load<TextAsset>($"translation/{languageSuffix}");


        if (jsonFile == null)
        {
            Debug.LogError($"Loaded asset is not a TextAsset for {languageSuffix}");
        }
        LanguageData combinedData = new LanguageData();
        LanguageData data = JsonConvert.DeserializeObject<LanguageData>(jsonFile.text);
        if (data != null)
        {
            if (data.Data != null)
            {
                combinedData.Data.AddRange(data.Data);
            }
            if (data.globalColors != null)
            {
                combinedData.globalColors.AddRange(data.globalColors);
            }
        }


        OnLanguageLoaded?.Invoke();
        return combinedData;
    }
    public static LanguageData LoadLocalData(string path, JsonDataType filter = JsonDataType.None)
    {


        LanguageData combinedData = new LanguageData();
        try
        {
            string jsonFile = System.IO.File.ReadAllText(path);


            if (jsonFile == null)
            {
                Debug.LogError($"Loaded asset is not a TextAsset for {path}");
            }
            LanguageData data = JsonConvert.DeserializeObject<LanguageData>(jsonFile);
            if (data != null)
            {

                if (filter != JsonDataType.None)
                {

                    List<JsonData> toRemove = new List<JsonData>();

                    foreach (JsonData j in data.Data)
                    {

                        if (j.ContainsKey("line"))
                        {

                            toRemove.Add(j);

                        }

                    }
                    foreach (JsonData j in toRemove)
                    {

                        data.Data.Remove(j);

                    }

                }



                if (data.Data != null)
                {
                    combinedData.Data.AddRange(data.Data);
                }
                if (data.globalColors != null)
                {
                    combinedData.globalColors.AddRange(data.globalColors);
                }
            }
        }
        catch (Exception e)
        {

            Debug.LogError($"No File at {path} : {e}");
        }




        return combinedData;
    }

    public static IEnumerator LoadAllJsonAsync(Action onComplete = null)
    {
        string languageSuffix = GetLanguageSuffix();
        string languageFolder = $"translation/{languageSuffix}";
        string colorFolder = $"translation/Global_Colors";


        TextAsset[] files = Resources.LoadAll<TextAsset>(languageFolder);


        if (files == null || files.Length == 0)
        {
            Debug.LogError($"No files found in Resources/{languageFolder}");
            onComplete?.Invoke();
        }
        LanguageData combinedData = new LanguageData();
        foreach (TextAsset file in files)
        {
            if (file is TextAsset textAsset)
            {
                try
                {

                    LanguageData data = JsonConvert.DeserializeObject<LanguageData>(file.text);
                    if (data != null)
                    {
                        if (data.Data != null)
                        {
                            combinedData.Data.AddRange(data.Data);
                        }
                        if (data.globalColors != null)
                        {
                            combinedData.globalColors.AddRange(data.globalColors);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing file {textAsset.name}: {e.Message}");
                    Debug.LogError($"{file.text}");
                }
            }
            yield return null;
        }

        ResourceRequest request = Resources.LoadAsync<TextAsset>(colorFolder);

        while (!request.isDone)
        {
            yield return null;
        }

        TextAsset Colorfile = ((TextAsset)request.asset);
        if (Colorfile is TextAsset colorFileAsset)
        {
            try
            {

                LanguageData data = JsonConvert.DeserializeObject<LanguageData>(colorFileAsset.text);
                if (data != null)
                {
                    if (data.globalColors != null)
                    {
                        combinedData.globalColors.AddRange(data.globalColors);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing file {colorFileAsset.name}: {e.Message}");
            }
        }

        combinedData.SetGlobalDictionary();
        Singleton = combinedData;
        OnLanguageLoaded?.Invoke();
        onComplete?.Invoke();
        Singleton.localLanguage = language;
    }

    public static IEnumerator LoadJsonAsync(Action onComplete = null)
    {
        string languageSuffix = GetLanguageSuffix();

        ResourceRequest request = Resources.LoadAsync<TextAsset>($"translation/{languageSuffix}");
        yield return request;

        if (request.asset == null)
        {
            Debug.LogError($"Failed to load translation file for {languageSuffix}");
            onComplete?.Invoke();
            yield break;
        }

        TextAsset jsonFile = request.asset as TextAsset;
        if (jsonFile == null)
        {
            Debug.LogError($"Loaded asset is not a TextAsset for {languageSuffix}");
            onComplete?.Invoke();
            yield break;
        }
        LanguageData combinedData = new LanguageData();

        LanguageData data = JsonConvert.DeserializeObject<LanguageData>(jsonFile.text);
        if (data != null)
        {
            if (data.Data != null)
            {
                combinedData.Data.AddRange(data.Data);
            }
            if (data.globalColors != null)
            {
                combinedData.globalColors.AddRange(data.globalColors);
            }
        }


        Singleton = combinedData;
        Singleton.localLanguage = language;
        onComplete?.Invoke();
        OnLanguageLoaded?.Invoke();
    }

    public JsonData GetLocalDataById(string id)
    {

        if (translationData.TryGetValue(id, out JsonData value))
        {
            return value;
        }
        Debug.LogWarning($"Key '{id}' not found in Translation File data.");
        return new JsonData(id);
    }


    public static JsonData GetDataById(string id)
    {
        if (Singleton == null)
        {
            return new JsonData(id);
        }
        if (Singleton.translationData == null)
        {
            return new JsonData(id);
        }
        if (Singleton.translationData.TryGetValue(id, out JsonData value))
        {
            return value;
        }
        Debug.LogWarning($"Key '{id}' not found in Translation File data.");
        return new JsonData(id);
    }
}

[Serializable]
public class GlobalColor
{
    public string key;
    public string value;
}

public enum JsonDataType
{
    None = -1,
    Misc = 0,
    Line,
    Character,
    Item,
    Skill,
    Quest

}

[Serializable]
public class JsonData
{
    public string dataId;
    public Dictionary<string, string> data = new();
    //public string jsonData;

    public JsonData()
    {
        data = new Dictionary<string, string>();
    }
    public JsonData(string id, Dictionary<string, string> dict)
    {
        dataId = id;
        data = dict ?? new Dictionary<string, string>();
    }
    public JsonData(string id)
    {
        this.dataId = id;
        data = new Dictionary<string, string>();
    }

    public JsonData(string id, JsonDataType t, string data)
    {
        this.dataId = id;
        //this.jsonData = data;
    }
    public bool ContainsKey(string key)
    {
        return data != null && data.ContainsKey(key);
        //if (string.IsNullOrEmpty(jsonData))
        //{
        //    Debug.LogWarning("jsonData is null or empty.");
        //}
        //else
        //{


        //    try
        //    {
        //        // Deserialize the jsonData string into a SerializableDictionary
        //        Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

        //        if (values.TryGetValue(key, out string value))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Error parsing JSON data: {e.Message}");

        //    }
        //}

        //return false;
    }
    public string GetValueByKey(string key, bool nullable = true)
    {

        string returnValue = "";
        if (!nullable)
        {
            returnValue = $"{dataId}.{key}";

        }

        if (data != null && data.TryGetValue(key, out string value))
        {
            // Replace <sprite name=something> with <sprite name=something_else>
            string pattern = @"<sprite name=([\w\d_]+)>";
            string replacedText = Regex.Replace(value, pattern, match =>
            {
                string originalName = match.Groups[1].Value;
                string newName = $"{originalName}_{InputManager.controlSettings}"; // Example replacement
                return $"<sprite name={newName}>";
            });

            // Replace {colorName} placeholders with corresponding values from GlobalColors
            pattern = @"\{color_([\w\d_]+)\}";
            replacedText = Regex.Replace(replacedText, pattern, match =>
            {
                string placeholder = match.Groups[1].Value;
                if (LanguageData.Singleton.GlobalColors.TryGetValue(placeholder, out string colorValue))
                {
                    return colorValue;
                }
                return match.Value;
            });
            returnValue = replacedText;
        }
        else
        {
            Debug.LogWarning($"Key '{key}' not found in '{dataId}'");
            if (key == "line")
                returnValue = $"No Line Found with Key '{dataId} | {key}'";
        }
        return returnValue;
    }




}
