using UnityEngine;

/// <summary>
/// MonoBehaviour บน yantra prefab แต่ล่ะอันต้อง implement interface นี้
/// YantCaster จะเรียก Initialize() ทันทีหลัง Instantiate
/// </summary>
public interface IYantEffect
{
    void Initialize(GameObject playerRoot, YantraStatsController stats);
}
