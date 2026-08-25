using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TeleportPointSO))]
public class TeleportPointSODrawer : PropertyDrawer
{
    private const float LineGap = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var point = property.objectReferenceValue as TeleportPointSO;
        float lineHeight = EditorGUIUtility.singleLineHeight;

        Rect foldoutRect = new Rect(position.x, position.y, 14f, lineHeight);
        Rect fieldRect = new Rect(position.x, position.y, position.width, lineHeight);

        if (point != null)
        {
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);
        }

        property.objectReferenceValue = EditorGUI.ObjectField(fieldRect, label, property.objectReferenceValue, typeof(TeleportPointSO), false); // new

        if (property.isExpanded && point != null)
        {
            EditorGUI.indentLevel++;
            float y = position.y + lineHeight + LineGap;

            Rect posRect = new Rect(position.x, y, position.width, lineHeight);
            Rect rotRect = new Rect(position.x, y + (lineHeight + LineGap), position.width, lineHeight);
            Rect scaleRect = new Rect(position.x, y + (lineHeight + LineGap) * 2, position.width, lineHeight);

            GUI.enabled = false;
            EditorGUI.Vector3Field(posRect, "Position", point.Position);
            EditorGUI.Vector3Field(rotRect, "Rotation (Euler)", point.Rotation.eulerAngles);
            EditorGUI.Vector3Field(scaleRect, "Scale", point.Scale);
            GUI.enabled = true;

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        var point = property.objectReferenceValue as TeleportPointSO;

        if (property.isExpanded && point != null)
        {
            return lineHeight + (lineHeight + LineGap) * 3;
        }

        return lineHeight;
    }
}