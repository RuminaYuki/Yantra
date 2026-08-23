using UnityEngine;
using System.Collections.Generic;

public class LevelAudioManager : MonoBehaviour
{
    public static LevelAudioManager Instance { get; private set; }

    // ตัวแปรสำหรับล็อคกล่องเสียงตอนคัตซีนทำงาน
    public static bool IsCutsceneActive = false;

    [Header("เพลงประกอบฉาก (BGM)")]
    [SerializeField] private SoundID sceneBGM;

    [Header("Debug")]
    [Tooltip("เปิดเพื่อดูว่าใครเป็นคนสั่งหรี่เสียง ambient — ปิดเมื่อหาเจอแล้ว")]
    [SerializeField] private bool logMuffleCalls = false;

    [Header("เสียงบรรยากาศภายนอก (Outside Ambient)")]
    [SerializeField] private SoundID[] outsideAmbientSounds;

    // [CHANGED] เก็บ "ใบเสร็จ" แทนตัวลำโพงจริง
    private List<SFXHandle> activeOutsideAmbients = new List<SFXHandle>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        // ไม่งั้นฉากใหม่จะเห็น Instance ค้างเป็นตัวเก่าที่ถูก Destroy ไปแล้ว
        if (Instance == this) Instance = null;
    }

    // ==========================================
    // เชื่อมต่อกับระบบ Cutscene
    // ==========================================
    private void OnEnable()
    {
        CutsceneController.OnGlobalCutsceneStateChanged += HandleCutsceneState;
    }

    private void OnDisable()
    {
        CutsceneController.OnGlobalCutsceneStateChanged -= HandleCutsceneState;
    }

    private void HandleCutsceneState(bool isPlaying)
    {
        IsCutsceneActive = isPlaying;
    }

    private void Start()
    {
        if (SoundManager.Instance == null) return;

        if (sceneBGM != null)
            SoundManager.Instance.PlayBGM(sceneBGM);

        if (outsideAmbientSounds != null)
        {
            foreach (var ambient in outsideAmbientSounds)
            {
                if (ambient == null) continue;

                SFXHandle handle = SoundManager.Instance.PlayLoopSFXForever(ambient, transform.position);

                if (handle.IsValid) activeOutsideAmbients.Add(handle);
            }
        }
    }

    public void MuffleOutsideAmbients(float targetMultiplier, float fadeTime)
    {
#if UNITY_EDITOR
        if (logMuffleCalls)
        {
            // StackTrace จะบอกว่าใครเรียกฟังก์ชันนี้ ไล่ดูบรรทัดที่ 2-3 จากบนจะเจอตัวการ
            Debug.Log($"[Muffle] หรี่เป็น {targetMultiplier} ใน {fadeTime} วิ | เฟรม {Time.frameCount}\n"
                + System.Environment.StackTrace);
        }
#endif

        // ถ้าเดินหน้าแล้ว RemoveAt กลางทาง index จะเลื่อน → ข้ามสมาชิกบางตัว
        for (int i = activeOutsideAmbients.Count - 1; i >= 0; i--)
        {
            SFXHandle handle = activeOutsideAmbients[i];

            if (!handle.IsValid)
            {
                // ลำโพงตัวนี้ไปทำงานอื่นแล้ว ทิ้งใบเสร็จไป จะได้ไม่ต้องเช็คซ้ำทุกรอบ
                activeOutsideAmbients.RemoveAt(i);
                continue;
            }

            handle.FadeToVolumeMultiplier(targetMultiplier, fadeTime);
        }
    }
}