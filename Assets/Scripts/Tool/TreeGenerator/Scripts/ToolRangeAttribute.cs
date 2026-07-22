using UnityEngine;

namespace TreeTool
{
    /// <summary>
    /// Drop-in replacement for Unity's built-in [Range] that also respects the tree's global
    /// "Manual Entry" toggle (ProceduralTreeSettings.manualNumberEntry). Normally draws the same
    /// slider as [Range]; when Manual Entry is on, every [ToolRange] field across the whole
    /// inspector switches at once to a free-typed number field (any value, not clamped to
    /// min/max), so one checkbox turns every slider in the tool into direct number entry.
    /// </summary>
    public class ToolRangeAttribute : PropertyAttribute
    {
        public readonly float min;
        public readonly float max;

        public ToolRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
