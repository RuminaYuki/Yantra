using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    public enum FootstepMode
    {
        DistanceBased,   // แนะนำ: นับระยะทาง ไม่ต้องใช้ Animation Event เลย
        AnimationEvent   // แบบเดิม: รอ event จากคลิป
    }

    [Header("คูปองเสียงต่างๆ ของนาวิน")]
    [SerializeField] private SoundID footstepWalkID;
    [SerializeField] private SoundID footstepRunID;   // ว่างไว้ได้ ถ้าไม่มีจะใช้เสียงเดินแทน
    [SerializeField] private SoundID flashlightToggleID;
    [SerializeField] private SoundID openNotebookID;

    [Header("โหมดเสียงฝีเท้า")]
    [SerializeField] private FootstepMode mode = FootstepMode.DistanceBased;

    [Header("ตั้งค่าโหมดนับระยะทาง")]
    [Tooltip("ระยะก้าวตอนเดิน (เมตร) — เดินครบระยะนี้ = 1 ก้าว")]
    [SerializeField] private float walkStrideLength = 1.6f;

    [Tooltip("ระยะก้าวตอนวิ่ง — ตอนวิ่งก้าวยาวขึ้น เลยใส่มากกว่าเดิน")]
    [SerializeField] private float runStrideLength = 2.4f;

    [Tooltip("เร็วเกินค่านี้ = ถือว่าวิ่ง (ใช้เลือกเสียงและระยะก้าว)")]
    [SerializeField] private float runSpeedThreshold = 3.5f;

    [Header("เงื่อนไขร่วม (ใช้ทั้ง 2 โหมด)")]
    [Tooltip("ช้ากว่านี้ถือว่ายืนอยู่กับที่ → ไม่มีเสียง")]
    [SerializeField] private float minMoveSpeed = 0.15f;

    [Tooltip("ระยะห่างขั้นต่ำระหว่างก้าว (วินาที) กัน event ยิงซ้อน")]
    [SerializeField] private float minStepInterval = 0.15f;

    [SerializeField] private bool requireGrounded = true;

    [Header("Debug")]
    [SerializeField] private bool logFootsteps = false;

    private CharacterController charController;
    private float lastStepTime = -999f;
    private Vector3 lastPosition;
    private float measuredSpeed;
    private float distanceAccumulated;

    public float CurrentSpeed => measuredSpeed;
    public bool IsRunning => measuredSpeed >= runSpeedThreshold;

    private void Awake()
    {
        charController = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        // วัดจากตำแหน่งจริง — ใช้ได้กับทุกระบบเคลื่อนที่ ไม่ต้องแก้โค้ดเดิม
        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f; // ตัดแกนตั้ง ไม่งั้นตอนตก/ขึ้นบันไดจะนับเป็นเดิน
        lastPosition = transform.position;

        float distanceThisFrame = delta.magnitude;

        if (Time.deltaTime > 0f)
            measuredSpeed = distanceThisFrame / Time.deltaTime;

        if (mode == FootstepMode.DistanceBased)
            UpdateDistanceFootstep(distanceThisFrame);
    }

    private void UpdateDistanceFootstep(float distanceThisFrame)
    {
        // ยืนอยู่กับที่ หรือลอยอยู่ → ไม่สะสมระยะ
        // ตรงนี้แหละที่ทำให้ "หยุดเดินแล้วเงียบทันที" ไม่มีเสียงหลุดตามมาอีก
        if (measuredSpeed < minMoveSpeed || !IsGrounded())
        {
            // เก็บระยะค้างไว้เกือบเต็ม พอออกเดินใหม่จะได้มีเสียงก้าวแรกเลย ไม่ต้องรอ
            distanceAccumulated = Mathf.Min(distanceAccumulated, walkStrideLength * 0.8f);
            return;
        }

        distanceAccumulated += distanceThisFrame;

        float stride = IsRunning ? runStrideLength : walkStrideLength;

        if (distanceAccumulated >= stride)
        {
            distanceAccumulated -= stride;
            TriggerFootstep();
        }
    }

    private bool IsGrounded()
    {
        if (!requireGrounded) return true;
        if (charController != null) return charController.isGrounded;

        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);
    }

    private void TriggerFootstep()
    {
        if (Time.time - lastStepTime < minStepInterval) return;
        lastStepTime = Time.time;

        SoundID idToPlay = (IsRunning && footstepRunID != null) ? footstepRunID : footstepWalkID;
        if (idToPlay == null) return;

        if (logFootsteps)
            Debug.Log($"👣 ก้าว! ({(IsRunning ? "วิ่ง" : "เดิน")}) speed={measuredSpeed:F2}");

        SoundManager.Instance.PlaySFX(idToPlay, transform.position);
    }

    // ==========================================
    // Animation Event — ยังใช้ได้ถ้าสลับโหมดกลับ
    // ==========================================
    public void PlayFootstepSound()
    {
        // ถ้าอยู่โหมดนับระยะ ให้เมิน event ที่หลงเหลือในคลิปไปเลย
        // จะได้ไม่ต้องไปนั่งลบ event ออกจากคลิปทีละตัว
        if (mode != FootstepMode.AnimationEvent) return;

        if (measuredSpeed < minMoveSpeed)
        {
            if (logFootsteps) Debug.Log("[Footstep] ปัดตก — หยุดเดินแล้ว");
            return;
        }

        if (!IsGrounded())
        {
            if (logFootsteps) Debug.Log("[Footstep] ปัดตก — ลอยอยู่");
            return;
        }

        TriggerFootstep();
    }

    // ==========================================
    // เสียงของใช้ติดตัว
    // ==========================================
    public void PlayFlashlightSound()
    {
        if (flashlightToggleID != null)
            SoundManager.Instance.PlaySFX(flashlightToggleID, transform.position);
    }

    public void PlayNotebookSound()
    {
        if (openNotebookID != null)
            SoundManager.Instance.PlaySFX(openNotebookID, transform.position);
    }
}