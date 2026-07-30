using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
public struct ShapeMatchJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> DrawnPoints;
    [ReadOnly] public NativeArray<float2> AllTemplatePoints;
    public int SampleCount;
    public float SearchRange;
    public float SearchThreshold;

    [WriteOnly] public NativeArray<float> Results;

    private const float _goldenRatio = 0.6180339887f;

    // Phase 1: คืนค่า raw distance (ยิ่งน้อยยิ่งเหมือน) เพื่อใช้ ranking เท่านั้น
    public void Execute(int index)
    {
        int startIndex = index * SampleCount;

        float a = -SearchRange;
        float b = SearchRange;

        float x1 = _goldenRatio * a + (1f - _goldenRatio) * b;
        float f1 = DistanceAtAngle(x1, startIndex);
        float x2 = (1f - _goldenRatio) * a + _goldenRatio * b;
        float f2 = DistanceAtAngle(x2, startIndex);

        while (math.abs(b - a) > SearchThreshold)
        {
            if (f1 < f2)
            {
                b = x2;
                x2 = x1; f2 = f1; // ← fix: f2=f1 (ไม่ใช่ f1=f2)
                x1 = _goldenRatio * a + (1f - _goldenRatio) * b;
                f1 = DistanceAtAngle(x1, startIndex);
            }
            else
            {
                a = x1;
                x1 = x2; f1 = f2; // ← fix: f1=f2 (ไม่ใช่ f2=f1)
                x2 = (1f - _goldenRatio) * a + _goldenRatio * b;
                f2 = DistanceAtAngle(x2, startIndex);
            }
        }

        Results[index] = math.min(f1, f2); // raw distance
    }

    private float DistanceAtAngle(float angleRad, int templateStartIndex)
    {
        float cos = math.cos(angleRad);
        float sin = math.sin(angleRad);
        float sum = 0f;

        for (int i = 0; i < SampleCount; i++)
        {
            float2 p = DrawnPoints[i];
            float2 rotated = new float2(
                p.x * cos - p.y * sin,
                p.x * sin + p.y * cos
            );
            sum += math.distance(rotated, AllTemplatePoints[templateStartIndex + i]);
        }

        return sum / SampleCount;
    }
}