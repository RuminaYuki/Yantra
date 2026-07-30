using UnityEngine;

/// <summary>
/// โซน trigger สำหรับสั่งเล่นเสียงพากย์เมื่อผู้เล่นเดินเข้ามา (เช่น เข้าหมู่บ้าน/เจอถ้ำ ฯลฯ)
/// วางบน GameObject ที่มี Collider (IsTrigger) ครอบพื้นที่ แล้วเลือก VoiceLine ที่ต้องการ
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerVoiceTrigger : MonoBehaviour
{
    [Tooltip("ชื่อเหตุการณ์เสียงพากย์ ดูค่าได้จาก PlayerVoice.*")]
    [SerializeField] private string _voiceLine = PlayerVoice.EnterVillage;

    [Tooltip("Tag ของผู้เล่น")]
    [SerializeField] private string _playerTag = "Player";

    [Tooltip("เล่นครั้งเดียวแล้วเลิกทำงาน (เหมาะกับบทพูดตามเนื้อเรื่อง)")]
    [SerializeField] private bool _triggerOnce = true;

    private bool _fired;

    private void Reset()
    {
        // ตั้ง Collider เป็น trigger อัตโนมัติตอนเพิ่ม component
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggerOnce && _fired) return;
        if (!other.CompareTag(_playerTag)) return;

        _fired = true;
        PlayerVoice.Publish(_voiceLine);

        if (_triggerOnce)
            gameObject.SetActive(false);
    }
}
