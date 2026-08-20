using UnityEngine;

/// <summary>
/// MonoBehaviour บน yantra prefab แต่ล่ะอันต้อง implement interface นี้
/// YantCaster จะเรียก Initialize() ทันทีหลัง Instantiate
/// </summary>
public interface IYantEffect
{
    bool Initialize(GameObject playerRoot ,bool holdLMB = false);
}

public interface IYantAnimationTiming
{
    void TriggerAnimationTiming(bool value);
}
