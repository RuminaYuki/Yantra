using UnityEditor;
using UnityEngine;

namespace TreeTool.EditorTools
{
    /// <summary>Draws FloatRange / IntRange fields tagged with [MinMaxRange] as a draggable slider.</summary>
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        const float FieldWidth = 46f;
        const float Pad = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");
            if (minProp == null || maxProp == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var range = (MinMaxRangeAttribute)attribute;
            bool isInt = minProp.propertyType == SerializedPropertyType.Integer;

            label = EditorGUI.BeginProperty(position, label, property);
            Rect content = EditorGUI.PrefixLabel(position, label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var minRect = new Rect(content.x, content.y, FieldWidth, content.height);
            var sliderRect = new Rect(content.x + FieldWidth + Pad, content.y,
                                      content.width - (FieldWidth + Pad) * 2f, content.height);
            var maxRect = new Rect(content.xMax - FieldWidth, content.y, FieldWidth, content.height);

            float minV = isInt ? minProp.intValue : minProp.floatValue;
            float maxV = isInt ? maxProp.intValue : maxProp.floatValue;

            EditorGUI.BeginChangeCheck();
            minV = isInt
                ? EditorGUI.IntField(minRect, Mathf.RoundToInt(minV))
                : EditorGUI.FloatField(minRect, minV);
            EditorGUI.MinMaxSlider(sliderRect, ref minV, ref maxV, range.Min, range.Max);
            maxV = isInt
                ? EditorGUI.IntField(maxRect, Mathf.RoundToInt(maxV))
                : EditorGUI.FloatField(maxRect, maxV);
            if (EditorGUI.EndChangeCheck())
            {
                minV = Mathf.Clamp(minV, range.Min, range.Max);
                maxV = Mathf.Clamp(maxV, minV, range.Max);
                if (isInt)
                {
                    minProp.intValue = Mathf.RoundToInt(minV);
                    maxProp.intValue = Mathf.RoundToInt(maxV);
                }
                else
                {
                    minProp.floatValue = minV;
                    maxProp.floatValue = maxV;
                }
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
