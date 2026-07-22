using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TreeTool.EditorTools
{
    /// <summary>
    /// Draws each Branch Levels list element, skipping "windResponse" - that field is shown
    /// consolidated in the inspector's Wind Response section instead (see
    /// ProceduralTreeEditor.DrawWindResponseSection), so it doesn't need to also show here.
    /// Everything else (including "name", which Unity already used as this element's foldout
    /// label by default) renders exactly as it did before.
    /// </summary>
    [CustomPropertyDrawer(typeof(BranchLevelSettings))]
    public class BranchLevelSettingsDrawer : PropertyDrawer
    {
        const string HiddenField = "windResponse";

        static IEnumerable<SerializedProperty> VisibleChildren(SerializedProperty property)
        {
            SerializedProperty it = property.Copy();
            SerializedProperty end = it.GetEndProperty();
            it.NextVisible(true);
            while (!SerializedProperty.EqualContents(it, end))
            {
                if (it.name != HiddenField)
                    yield return it.Copy();
                if (!it.NextVisible(false))
                    break;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float h = EditorGUIUtility.singleLineHeight;
            foreach (var child in VisibleChildren(property))
                h += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty nameProp = property.FindPropertyRelative("name");
            GUIContent foldLabel = nameProp != null && !string.IsNullOrEmpty(nameProp.stringValue)
                ? new GUIContent(nameProp.stringValue)
                : label;

            var foldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, foldLabel, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                foreach (var child in VisibleChildren(property))
                {
                    float h = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), child, true);
                    y += h + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
