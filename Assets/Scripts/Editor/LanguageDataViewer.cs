using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class LanguageDataViewer : EditorWindow
{
    private Vector2 scroll;
    private bool autoRefresh = true;

    [MenuItem("Tools/Localization/Language Data Viewer")]
    public static void Open()
    {
        GetWindow<LanguageDataViewer>("Language Data");
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            Repaint();
        }

        autoRefresh = GUILayout.Toggle(autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);
        GUILayout.EndHorizontal();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("LanguageData is only available in Play Mode.", MessageType.Info);
            return;
        }


        var instance = LanguageData.Singleton;

        if (instance == null)
        {
            EditorGUILayout.HelpBox("LanguageData.Singleton is null.", MessageType.Warning);
            return;
        }

        if (instance.Data == null || instance.Data.Count == 0)
        {
            EditorGUILayout.HelpBox("LanguageData.Singleton.Data is empty.", MessageType.Info);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var entry in instance.Data)
        {
            if (entry == null)
                continue;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("ID", entry.dataId, EditorStyles.boldLabel);

            if (entry.data == null || entry.data.Count == 0)
            {
                EditorGUILayout.LabelField("No values.");
            }
            else
            {
                foreach (KeyValuePair<string, string> kv in entry.data)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(kv.Key, GUILayout.Width(150));
                    EditorGUILayout.SelectableLabel(kv.Value, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void Update()
    {
        if (autoRefresh)
            Repaint();
    }
}
