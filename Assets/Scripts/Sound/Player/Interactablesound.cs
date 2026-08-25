using UnityEngine;

/// <summary>
/// เล่นเสียงตอนผู้เล่นกดใช้งานวัตถุ (กระดาษยันต์ กระดาษอ่าน ฯลฯ)
///
/// แปะบน GameObject ตัวเดียวกับที่มี InteractableBase
///
/// ไม่แตะสคริปต์ระบบ interact เลยสักบรรทัด — ใช้ event ที่ InteractableBase
/// เปิดไว้ให้อยู่แล้ว (OnInteract / OnEndInteract) เหมือนที่ PickupInteractable
/// กับ PaperDataInteractable ทำ
/// </summary>
public class InteractableSound : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("เว้นว่างได้ จะหาจาก GameObject ตัวเองอัตโนมัติ")]
    [SerializeField] private InteractableBase _interactable;

    [Header("Sounds")]
    [Tooltip("เสียงตอนกดใช้งาน เช่นเสียงหยิบกระดาษ")]
    [SerializeField] private SoundID _interactSound;

    [Tooltip("เสียงตอนเลิกใช้งาน เช่นเสียงวางกระดาษลง — เว้นว่างได้ถ้าไม่ต้องการ")]
    [SerializeField] private SoundID _endInteractSound;

    private void Awake()
    {
        if (_interactable == null)
            _interactable = GetComponent<InteractableBase>();

        if (_interactable == null)
        {
            Debug.LogWarning(
                $"{nameof(InteractableSound)}: หา InteractableBase ไม่เจอบน {gameObject.name}",
                this);
        }
    }

    private void OnEnable()
    {
        if (_interactable == null) return;

        _interactable.OnInteract += HandleInteract;
        _interactable.OnEndInteract += HandleEndInteract;
    }

    private void OnDisable()
    {
        // ต้อง unsubscribe เสมอ เพื่อป้องกัน Memory Leak
        // สำคัญเป็นพิเศษกับวัตถุที่ถูกเก็บ เช่นยันต์ที่ SetActive(false) หลังหยิบ
        if (_interactable == null) return;

        _interactable.OnInteract -= HandleInteract;
        _interactable.OnEndInteract -= HandleEndInteract;
    }

    private void HandleInteract(GameObject rootPlayer)
    {
        PlaySound(_interactSound);
    }

    private void HandleEndInteract(GameObject rootPlayer)
    {
        PlaySound(_endInteractSound);
    }

    private void PlaySound(SoundID id)
    {
        if (id == null || SoundManager.Instance == null) return;
        SoundManager.Instance.PlayEventSFX(id);
    }
}