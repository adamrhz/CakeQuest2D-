#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class DialogueJsonMigrator
{
    private const string ROOT = "Assets/Resources/translation";

    [MenuItem("Tools/Migrate/Dialogue JSON -> Dictionary")]
    public static void MigrateDialogueJson()
    {
        string fullPath = Path.Combine(Application.dataPath, "Resources/translation");
        Debug.Log(fullPath);
        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"Directory not found: {fullPath}");
            return;
        }

        string[] files = Directory.GetFiles(fullPath, "*.json", SearchOption.AllDirectories);
        int migratedCount = 0;

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);

            // Quick filter

            try
            {
                OldLanguageData oldData = JsonConvert.DeserializeObject<OldLanguageData>(json);
                if (oldData?.Data == null) continue;

                bool changed = false;
                NewLanguageData newData = new NewLanguageData();

                foreach (var oldEntry in oldData.Data)
                {
                    var newEntry = new NewJsonData();
                    newEntry.dataId = oldEntry.dataId;
                    newEntry.data = JsonConvert.DeserializeObject<Dictionary<string, string>>(oldEntry.jsonData);
                    newData.Data.Add(newEntry);
                    changed = true;
                }

                if (!changed) continue;

                string newJson = JsonConvert.SerializeObject(newData, Formatting.Indented);
                File.WriteAllText(file, newJson);

                Debug.Log($"Migrated: {file}");
                migratedCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed migrating {file}: {e.Message}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Migration complete. Files migrated: {migratedCount}");
    }

    // --- Old format ---
    private class OldLanguageData
    {
        public List<OldJsonData> Data;
    }

    private class OldJsonData
    {
        public string dataId;
        public string jsonData;
    }

    // --- New format ---
    private class NewLanguageData
    {
        public List<NewJsonData> Data = new List<NewJsonData>();
    }

    private class NewJsonData
    {
        public string dataId;
        public Dictionary<string, string> data;
    }
}
#endif
