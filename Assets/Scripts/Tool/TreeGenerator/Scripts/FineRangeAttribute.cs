using UnityEngine;

namespace TreeTool
{
    /// <summary>
    /// Put on a float field to draw it as a slider whose drag distance near 0 covers a much
    /// smaller value range than a plain [Range] slider - most Wind Response-style fields are
    /// only ever used in a tiny sliver (0-0.1) of a 0-1/0-2 range, so a linear slider wastes
    /// nearly all its drag distance on values nobody wants and makes the useful low end almost
    /// impossible to land on precisely.
    /// </summary>
    public class FineRangeAttribute : PropertyAttribute
    {
        public readonly float Min;
        public readonly float Max;
        public readonly float Power;

        /// <param name="power">How much drag distance is dedicated to low values - 1 = linear
        /// (same as [Range]), higher = more precision near Min at the cost of coarser precision
        /// near Max. 2-3 is a good default.</param>
        public FineRangeAttribute(float min, float max, float power = 2.5f)
        {
            Min = min;
            Max = max;
            Power = power;
        }
    }
}
