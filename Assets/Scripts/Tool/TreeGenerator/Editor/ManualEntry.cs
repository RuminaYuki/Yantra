using UnityEditor;

namespace TreeTool.EditorTools
{
    /// <summary>
    /// Shared lookup so every custom slider drawer (ToolRange / MinMaxRange / FineRange) can
    /// check the tree's global "Manual Entry" toggle without re-deriving the property path.
    /// property.serializedObject always refers to the root ProceduralTree instance regardless
    /// of how deeply nested the field being drawn is (inside a branch level, a root type, ...),
    /// so one fixed relative path ("settings.manualNumberEntry") reaches the flag from anywhere.
    /// </summary>
    static class ManualEntry
    {
        public static bool IsOn(SerializedProperty property)
        {
            SerializedProperty flag = property.serializedObject.FindProperty("settings.manualNumberEntry");
            return flag != null && flag.boolValue;
        }
    }
}
