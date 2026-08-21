using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmbientZoneTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("ความเร็วในการ Fade เสียงเข้า/ออก (วินาที)")]
    [SerializeField] private float fadeDuration = 2.0f;

    [Header("Outside Ambient Settings")]
    [Tooltip("ตอนเข้าโซนนี้ จะให้เสียงภายนอกดังแค่ไหน? (1 = ปกติ, 0.6 = ชานบ้าน, 0.25 = ในบ้าน, 0 = ดับสนิท)")]
    [Range(0f, 1f)]
    [SerializeField] private float outsideVolumeTarget = 0.25f;

    [Header("Indoor Ambient (Optional)")]
    [Tooltip("คูปองเสียงบรรยากาศในโซนนี้ (ถ้าไม่มีให้ปล่อยว่างไว้)")]
    [SerializeField] private SoundID indoorAmbientSound;

    private SFXPlayer currentIndoorAmbient;

    // [ADD] ตัวนับว่าผู้เล่นกำลังเหยียบกล่องอยู่กี่ใบ
    private int playerInTriggersCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggersCount++;

            // ถ้าเหยียบกล่องใบแรก (เพิ่งเข้ามาในอาคาร) ค่อยรันคำสั่งเฟดเสียง
            if (playerInTriggersCount == 1)
            {
                if (LevelAudioManager.Instance != null)
                {
                    LevelAudioManager.Instance.MuffleOutsideAmbients(outsideVolumeTarget, fadeDuration);
                }

                if (indoorAmbientSound != null && SoundManager.Instance != null)
                {
                    currentIndoorAmbient = SoundManager.Instance.PlayLoopSFXForever(indoorAmbientSound, transform.position);

                    if (currentIndoorAmbient != null)
                    {
                        currentIndoorAmbient.SetVolumeMultiplier(0f);
                        currentIndoorAmbient.FadeToVolumeMultiplier(1f, fadeDuration);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggersCount--;

            // ถ้าตัวนับเหลือ 0 (หรือติดลบเผื่อบั๊ก) แปลว่าเดินออกจากกล่อง "ทุกใบ" แล้วจริงๆ ค่อยปิดเสียง
            if (playerInTriggersCount <= 0)
            {
                playerInTriggersCount = 0; // เซ็ตกลับเป็น 0 กันเหนียว

                if (LevelAudioManager.Instance != null)
                {
                    LevelAudioManager.Instance.MuffleOutsideAmbients(1.0f, fadeDuration);
                }

                if (currentIndoorAmbient != null)
                {
                    currentIndoorAmbient.FadeOutAndStop(fadeDuration);
                    currentIndoorAmbient = null;
                }
            }
        }
    }
}