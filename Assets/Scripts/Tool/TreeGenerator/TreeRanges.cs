using System;
using UnityEngine;

namespace TreeTool
{
    /// <summary>Random range [min, max] for floats, drawn as a min-max slider.</summary>
    [Serializable]
    public struct FloatRange
    {
        public float min;
        public float max;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Lerp(float t) => Mathf.Lerp(min, max, t);
        public float Random(System.Random rand) => Lerp((float)rand.NextDouble());

        public void Sort()
        {
            if (min > max) (min, max) = (max, min);
        }
    }

    /// <summary>Random range [min, max] for ints (inclusive), drawn as a min-max slider.</summary>
    [Serializable]
    public struct IntRange
    {
        public int min;
        public int max;

        public IntRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int Random(System.Random rand) => rand.Next(min, max + 1);

        public void Sort()
        {
            if (min > max) (min, max) = (max, min);
        }
    }
}
