using UnityEngine;

namespace TreeTool
{
    /// <summary>
    /// Put on a FloatRange / IntRange field to draw it as a draggable min-max slider.
    /// </summary>
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public readonly float Min;
        public readonly float Max;

        public MinMaxRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
