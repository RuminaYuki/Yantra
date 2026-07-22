using UnityEditor;
using UnityEngine;

namespace TreeTool.EditorTools
{
    /// <summary>Draws [ToolRange] fields as a normal slider, or as a plain typed number field
    /// (any value allowed) when the tree's Manual Entry toggle is on - see ManualEntry.cs.</summary>
    [CustomPropertyDrawer(typeof(ToolRangeAttribute))]
    public class ToolRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var range = (ToolRangeAttribute)attribute;
            bool manual = ManualEntry.IsOn(property);

            label = EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = manual
                    ? EditorGUI.FloatField(position, label, property.floatValue)
                    : EditorGUI.Slider(position, label, property.floatValue, range.min, range.max);
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = manual
                    ? EditorGUI.IntField(position, label, property.intValue)
                    : EditorGUI.IntSlider(position, label, property.intValue, (int)range.min, (int)range.max);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            EditorGUI.EndProperty();
        }
    }
}
