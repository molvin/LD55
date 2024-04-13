using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(RuneRef))]
public class CardRefDrawer : PropertyDrawer
{
    public static string[] Names = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (Names == null)
        {
            var temp = Runes.GetAllRunes().Select(rune => rune.Name).ToList();
            temp.Insert(0, "None");
            Names = temp.ToArray();
        }

        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty indexProperty = property.FindPropertyRelative("Index");

        indexProperty.intValue = EditorGUI.Popup(position, indexProperty.intValue, Names);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }
}
