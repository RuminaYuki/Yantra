using UnityEditor;
using UnityEngine;

namespace TreeTool.EditorTools
{
    /// <summary>Draws float fields tagged with [FineRange] as a power-curved slider - dragging
    /// near the left edge covers a much smaller value span than dragging near the right edge,
    /// so tiny "barely moves" values (which is most of what Wind Response fields actually use)
    /// are easy to land on precisely instead of being crammed into a couple of pixels.</summary>
    [CustomPropertyDrawer(typeof(FineRangeAttribute))]
    public class FineRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Float)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var range = (FineRangeAttribute)attribute;
            float value = property.floatValue;

            // slider position t in [0,1] <-> real value: value = Min + (Max-Min) * t^power,
            // so t = ((value-Min)/(Max-Min)) ^ (1/power)
            float normalized = Mathf.Clamp01((value - range.Min) / (range.Max - range.Min));
            float t = Mathf.Pow(normalized, 1f / range.Power);

            label = EditorGUI.BeginProperty(position, label, property);
            Rect content = EditorGUI.PrefixLabel(position, label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            const float fieldWidth = 46f;
            const float pad = 4f;
            var sliderRect = new Rect(content.x, content.y, content.width - fieldWidth - pad, content.height);
            var fieldRect = new Rect(content.xMax - fieldWidth, content.y, fieldWidth, content.height);

            EditorGUI.BeginChangeCheck();
            t = GUI.HorizontalSlider(sliderRect, t, 0f, 1f);
            float slidValue = range.Min + (range.Max - range.Min) * Mathf.Pow(t, range.Power);
            if (EditorGUI.EndChangeCheck())
                value = slidValue;

            EditorGUI.BeginChangeCheck();
            value = EditorGUI.FloatField(fieldRect, value);
            if (EditorGUI.EndChangeCheck())
                value = Mathf.Clamp(value, range.Min, range.Max);

            property.floatValue = value;

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
