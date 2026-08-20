using UnityEngine;

public class BookSoundListener : MonoBehaviour
{
    [Header("ใส่คูปองเสียงพลิกกระดาษ")]
    [SerializeField] private SoundID pageTurnSoundID;

    private BookTab[] allTabs;

    // ✨ ตัวแปรใหม่: เอาไว้ดักไม่ให้เล่นเสียงลั่นตอนเริ่มเกม และกันสคริปต์ Lead พัง
    private bool isReady = false;

    private void Awake()
    {
        // กวาดหา BookTab ทุกอัน
        allTabs = GetComponentsInChildren<BookTab>(true);
    }

    private void OnEnable()
    {
        foreach (var tab in allTabs)
        {
            if (tab != null)
                tab.OnTabClicked += PlayPageTurnSound;
        }
    }

    private void OnDisable()
    {
        foreach (var tab in allTabs)
        {
            if (tab != null)
                tab.OnTabClicked -= PlayPageTurnSound;
        }
    }

    private void Start()
    {
        // ฟังก์ชัน Start() จะทำงานหลังจาก Awake() ของ Lead จัดระเบียบเสร็จแล้วเสมอ
        // พอทุกอย่างปลอดภัย เราถึงจะอนุญาตให้สคริปต์นี้เริ่มส่งเสียงได้
        isReady = true;
    }

    private void PlayPageTurnSound(bool isActive)
    {
        // ถ้าเกมเพิ่งเริ่ม (isReady ยังเป็น false) ให้ข้ามการทำงานไปเลย ไม่ต้องเล่นเสียง
        if (!isReady) return;

        if (isActive && pageTurnSoundID != null)
        {
            SoundManager.Instance.PlaySFX(pageTurnSoundID, transform.position);
        }
    }
}