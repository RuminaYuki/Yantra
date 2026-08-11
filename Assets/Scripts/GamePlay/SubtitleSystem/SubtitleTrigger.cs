using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SubtitleTrigger : MonoBehaviour
{
    [Header("Subtitle Settings")]
    [Tooltip("ลากไฟล์บทพูด (SubtitleDataSO) ที่อยากให้เล่นตอนเดินผ่านมาใส่ช่องนี้")]
    [SerializeField] private SubtitleDataSO _subtitleData;

    [Tooltip("ถ้าติ๊กถูก ระบบจะเล่นแค่ครั้งเดียวแล้วปิดตัวเองทิ้ง (กันผู้เล่นเดินถอยหลังกลับมาเหยียบซ้ำ)")]
    [SerializeField] private bool _playOnce = true;

    private void OnTriggerEnter(Collider other)
    {
        // เช็คว่าคนที่เดินมาชนป้ายนี้ คือ "Player" ใช่ไหม?
        if (other.CompareTag("Player"))
        {
            // ตะโกนสั่งให้ระบบซับไตเติ้ลทำงาน!
            if (_subtitleData != null && SubtitleSystem.Instance != null)
            {
                SubtitleSystem.Instance.PlaySubtitle(_subtitleData);
            }
            else
            {
                Debug.LogWarning("SubtitleTrigger: ลืมใส่ไฟล์บทพูด หรือลืมวาง SubtitleManager ในฉากครับ");
            }

            // ถ้าตั้งค่าให้เล่นรอบเดียว ก็ปิดกล่องล่องหนนี้ทิ้งไปเลย
            if (_playOnce)
            {
                gameObject.SetActive(false);
            }
        }
    }
}