using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class AmbientZoneTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string[] allowedTags = { "Player", "Dummy" };

    [Header("Transition Settings")]
    [Tooltip("ความเร็วในการ Fade เสียงเข้า/ออก (วินาที)")]
    [SerializeField] private float fadeDuration = 2.0f;

    [Header("Outside Ambient Settings")]
    [Tooltip("ความดังปกติ ตอนที่ผู้เล่น 'เดินออกจากบ้าน'")]
    [Range(0f, 1f)]
    [SerializeField] private float normalOutsideVolume = 1.0f;

    [Tooltip("ความดังตอนที่ผู้เล่น 'เดินเข้ามาในโซนนี้' (เช่น เข้าบ้านเหลือ 0.25)")]
    [Range(0f, 1f)]
    [SerializeField] private float insideVolumeTarget = 0.25f;

    [Header("Indoor Ambient (Optional)")]
    [SerializeField] private SoundID indoorAmbientSound;

    // เริ่มต้นเป็น None เพราะ struct ใส่ null ไม่ได้
    private SFXHandle currentIndoorAmbient = SFXHandle.None;

    private Dictionary<Collider, int> overlapCount = new Dictionary<Collider, int>();

    private bool IsOccupied => overlapCount.Count > 0;

    private bool IsAllowed(string tagToCheck)
    {
        foreach (string t in allowedTags)
        {
            if (tagToCheck == t) return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsAllowed(other.tag)) return;

        bool wasEmpty = !IsOccupied;

        // นับก่อนเสมอ แม้อยู่ระหว่างคัตซีน
        // ถ้าข้ามการนับตอนคัตซีน ตัวเลขจะเพี้ยนถาวรหลังคัตซีนจบ
        if (overlapCount.TryGetValue(other, out int count))
            overlapCount[other] = count + 1;
        else
            overlapCount[other] = 1;

        if (LevelAudioManager.IsCutsceneActive) return;
        if (!wasEmpty) return;

        if (LevelAudioManager.Instance != null)
            LevelAudioManager.Instance.MuffleOutsideAmbients(insideVolumeTarget, fadeDuration);

        if (indoorAmbientSound != null && SoundManager.Instance != null)
        {
            currentIndoorAmbient = SoundManager.Instance.PlayLoopSFXForever(indoorAmbientSound, transform.position);

            if (currentIndoorAmbient.IsValid)
            {
                currentIndoorAmbient.SetVolumeMultiplier(0f);
                currentIndoorAmbient.FadeToVolumeMultiplier(1f, fadeDuration);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsAllowed(other.tag)) return;

        if (overlapCount.TryGetValue(other, out int count))
        {
            if (count <= 1) overlapCount.Remove(other);
            else overlapCount[other] = count - 1;
        }

        if (LevelAudioManager.IsCutsceneActive) return;

        // ยังอยู่ในกล่องใบอื่นของโซนนี้ ยังไม่ถือว่าออก
        if (IsOccupied) return;

        if (LevelAudioManager.Instance != null)
            LevelAudioManager.Instance.MuffleOutsideAmbients(normalOutsideVolume, fadeDuration);

        // [CHANGED] ถ้าใบเสร็จหมดอายุแล้ว บรรทัดนี้จะเมินเงียบๆ ไม่ไปโดนเสียงคนอื่น
        currentIndoorAmbient.FadeOutAndStop(fadeDuration);
        currentIndoorAmbient = SFXHandle.None;
    }

    private void OnDisable()
    {
        // PlayLoopSFXForever ไม่คืนลำโพงเข้าพูลเอง ถ้าไม่สั่งหยุด = เสียโควต้าพูลถาวร
        currentIndoorAmbient.Stop();
        currentIndoorAmbient = SFXHandle.None;
        overlapCount.Clear();
    }
}