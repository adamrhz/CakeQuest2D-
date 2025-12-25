using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(MealEffect), true)]
public class MealEffectDrawer : PropertyDrawer
{
    private Type[] _types;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_types == null)
        {
            _types = TypeCache.GetTypesDerivedFrom<MealEffect>()
                .Where(t => !t.IsAbstract)
                .ToArray();
        }

        EditorGUI.BeginProperty(position, label, property);

        Rect header = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect body = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2,
                               position.width, position.height - EditorGUIUtility.singleLineHeight);

        DrawTypeDropdown(header, property);

        if (property.managedReferenceValue != null)
            EditorGUI.PropertyField(body, property, GUIContent.none, true);

        EditorGUI.EndProperty();
    }

    void DrawTypeDropdown(Rect rect, SerializedProperty property)
    {
        string current = property.managedReferenceValue?.GetType().Name ?? "None";

        if (EditorGUI.DropdownButton(rect, new GUIContent(current), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            foreach (var type in _types)
            {
                menu.AddItem(new GUIContent(type.Name),
                    property.managedReferenceValue?.GetType() == type,
                    () =>
                    {
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    });
            }

            menu.ShowAsContext();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceValue == null)
            return EditorGUIUtility.singleLineHeight;

        return EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight + 4;
    }
}
