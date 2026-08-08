using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewPlaySubtitleAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Dialog/Play Subtitle")]
public class PlaySubtitleActionSO : StateActionSO
{
    [Tooltip("ลากไฟล์ SubtitleDataSO (บทพูด) ที่ต้องการเล่นเมื่อเข้า State นี้มาใส่")]
    [SerializeField] private SubtitleDataSO subtitleData;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new PlaySubtitleAction(subtitleData);
    }
}

public class PlaySubtitleAction : StateAction
{
    private readonly SubtitleDataSO _subtitleData;

    public PlaySubtitleAction(SubtitleDataSO subtitleData)
    {
        _subtitleData = subtitleData;
    }

    public override void Awake(StateMachine stateMachine)
    {
        // ไม่ต้องหา Component อะไรบนตัวละคร เพราะเราจะเรียกใช้ผ่าน Singleton
    }

    public override void OnStateEnter()
    {
        // เมื่อ State นี้เริ่มทำงาน (เช่น บอสเข้าสเตทขู่) ให้ตะโกนสั่ง SubtitleSystem ทันที
        if (_subtitleData != null && SubtitleSystem.Instance != null)
        {
            SubtitleSystem.Instance.PlaySubtitle(_subtitleData);
        }
        else if (SubtitleSystem.Instance == null)
        {
            Debug.LogWarning("PlaySubtitleAction: ไม่พบ SubtitleManager ในฉาก กรุณาลาก Prefab มาวางด้วยครับ!");
        }
    }

    public override void OnStateExit() { }
    public override void OnUpdate() { }
}