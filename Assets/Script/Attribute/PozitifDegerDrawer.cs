using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PositiveValueAttribute))]
public class PozitifDegerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.intValue = Mathf.Max(0, property.intValue);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Bu özellik sadece sayısal değişkenlerde kullanılabilir!");
        }
    }
}