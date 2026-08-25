using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(Personality))]
[CustomPropertyDrawer(typeof(WayOfSpeaking))]
public class EnumFlagsPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
    }
}
