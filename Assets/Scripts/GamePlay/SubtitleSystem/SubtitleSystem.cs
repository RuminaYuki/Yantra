using System;
using System.Collections;
using UnityEngine;

public class SubtitleSystem : MonoBehaviour
{
    public static SubtitleSystem Instance { get; private set; }

    // หอกระจายข่าวให้ UI คอยฟัง (ไม่ต้องพึ่งพากันและกัน)
    public static event Action<bool> OnSubtitleToggle;    // สั่งเปิด/ปิด
    public static event Action<string> OnSubtitleUpdate;  // สั่งเปลี่ยนข้อความ

    [Header("⏱️ Timing")]
    [Tooltip("หน่วงเสียงพากย์ให้ตรงกับตอนที่ซับเฟดขึ้นมาพอดี — ใส่เท่ากับ Fade Duration ใน SubtitleUI")]
    [SerializeField] private float voiceDelayToMatchFade = 0.25f;

    [Tooltip("เวลาโชว์ขั้นต่ำต่อประโยค กันเคสลืมใส่ FallbackDuration แล้วซับกระพริบหาย")]
    [SerializeField] private float minLineDuration = 1.2f;

    [Tooltip("ถ้าไม่มีทั้งเสียงและ FallbackDuration จะคำนวณเวลาจากจำนวนตัวอักษร (ตัวอักษรต่อวินาที)")]
    [SerializeField] private float charactersPerSecond = 14f;

    [Tooltip("เว้นจังหวะหายใจระหว่างประโยค")]
    [SerializeField] private float gapBetweenLines = 0.15f;

    [Header("🎙️ Voice")]
    [Tooltip("ส่งเสียงพากย์ผ่าน SoundManager (ได้ mixer + ปรับ volume + หรี่เพลงอัตโนมัติ)")]
    [SerializeField] private bool routeVoiceThroughSoundManager = true;

    [Tooltip("AudioSource สำรอง ใช้เมื่อไม่มี SoundManager ในซีน (เช่นเทสซีนเดี่ยวๆ)")]
    [SerializeField] private AudioSource fallbackAudioSource;

    private Coroutine _subtitleCoroutine;

    /// <summary>ตอนนี้มีบทพูดเล่นอยู่มั้ย (เผื่อระบบอื่นอยากเช็คก่อนแทรก)</summary>
    public bool IsPlaying => _subtitleCoroutine != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (fallbackAudioSource == null)
            fallbackAudioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        // [FIX] ล้าง Instance ตอนถูกทำลาย ไม่งั้นเปลี่ยนซีนแล้วจะเหลือ reference ผีค้างอยู่
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        OnSubtitleToggle?.Invoke(false);
    }

    public void PlaySubtitle(SubtitleDataSO subtitleData)
    {
        if (subtitleData == null || subtitleData.Lines == null || subtitleData.Lines.Length == 0)
            return;

        StopSubtitle(); // [FIX] ตัดของเก่าให้เกลี้ยงก่อนเสมอ รวมถึงเสียงด้วย

        _subtitleCoroutine = StartCoroutine(PlaySubtitleRoutine(subtitleData));
    }

    /// <summary>หยุดบทพูดกลางคัน (ผู้เล่นกดข้าม, เข้าคัตซีน, ตาย ฯลฯ)</summary>
    public void StopSubtitle()
    {
        if (_subtitleCoroutine != null)
        {
            StopCoroutine(_subtitleCoroutine);
            _subtitleCoroutine = null;
        }

        // [FIX] ของเดิมหยุดแค่ coroutine แต่เสียงพากย์ประโยคเก่ายังเล่นค้าง
        // พอ trigger สองอันอยู่ใกล้กันเลยได้ยินเสียงทับกัน
        StopVoice();

        OnSubtitleToggle?.Invoke(false);
    }

    private IEnumerator PlaySubtitleRoutine(SubtitleDataSO subtitleData)
    {
        OnSubtitleToggle?.Invoke(true);

        // ล็อกให้เพลงหรี่ค้างตลอดบทสนทนา
        // ไม่งั้นเพลงจะเด้งขึ้นลงทุกช่องว่างระหว่างประโยค
        if (routeVoiceThroughSoundManager && SoundManager.Instance != null)
            SoundManager.Instance.SetVoiceDuckHold(true);

        foreach (var line in subtitleData.Lines)
        {
            // 1. จัดฟอร์แมตข้อความและสี
            OnSubtitleUpdate?.Invoke(BuildDisplayText(line));

            // 2. [FIX] รอให้ซับเฟดขึ้นมาก่อนค่อยปล่อยเสียง
            //    ไม่งั้นเสียงจะมาก่อนตัวหนังสือประมาณครึ่งวินาที (เพราะ UI ต้องเฟดออก+เฟดเข้า)
            if (voiceDelayToMatchFade > 0f)
                yield return new WaitForSecondsRealtime(voiceDelayToMatchFade);

            // 3. เล่นเสียงและคำนวณเวลาโชว์
            float waitTime = PlayLineVoice(line);
            waitTime = Mathf.Max(waitTime, GetFallbackDuration(line));

            // [FIX] ใช้ Realtime ให้ตรงกับ AudioSource ที่ไม่สนใจ timeScale
            //       ของเดิมพอกด pause กลางบทพูด ซับกับเสียงจะเลื่อนไม่ตรงกันถาวร
            yield return new WaitForSecondsRealtime(waitTime);

            if (gapBetweenLines > 0f)
                yield return new WaitForSecondsRealtime(gapBetweenLines);
        }

        if (routeVoiceThroughSoundManager && SoundManager.Instance != null)
            SoundManager.Instance.SetVoiceDuckHold(false);

        OnSubtitleToggle?.Invoke(false);
        _subtitleCoroutine = null;
    }

    private string BuildDisplayText(SubtitleLine line)
    {
        if (line.Speaker == null) return line.DefaultText;

        string hexColor = ColorUtility.ToHtmlStringRGB(line.Speaker.NameColor);
        return $"<color=#{hexColor}>[{line.Speaker.DefaultName}]</color> {line.DefaultText}";
    }

    private float PlayLineVoice(SubtitleLine line)
    {
        if (line.VoiceClip == null) return 0f;

        if (routeVoiceThroughSoundManager && SoundManager.Instance != null)
            return SoundManager.Instance.PlayVoice(line.VoiceClip);

        // โหมดสำรอง — ไม่ผ่าน mixer ผู้เล่นปรับ volume ไม่ได้ ใช้เฉพาะตอนเทส
        if (fallbackAudioSource != null)
        {
            fallbackAudioSource.Stop();
            fallbackAudioSource.clip = line.VoiceClip;
            fallbackAudioSource.Play();
        }

        return line.VoiceClip.length;
    }

    private void StopVoice()
    {
        if (routeVoiceThroughSoundManager && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopVoice();
            SoundManager.Instance.SetVoiceDuckHold(false);   // ปลดล็อก ไม่งั้นเพลงหรี่ค้างตลอดเกม
        }

        if (fallbackAudioSource != null)
            fallbackAudioSource.Stop();
    }

    // [FIX] กันเคส FallbackDuration = 0 แล้วซับกระพริบหายในเฟรมเดียว
    private float GetFallbackDuration(SubtitleLine line)
    {
        if (line.FallbackDuration > 0.01f)
            return line.FallbackDuration;

        // ไม่ได้ตั้งมา → เดาจากความยาวข้อความ พออ่านทัน
        int length = string.IsNullOrEmpty(line.DefaultText) ? 0 : line.DefaultText.Length;
        float estimated = charactersPerSecond > 0f ? length / charactersPerSecond : 0f;

        return Mathf.Max(estimated, minLineDuration);
    }
}