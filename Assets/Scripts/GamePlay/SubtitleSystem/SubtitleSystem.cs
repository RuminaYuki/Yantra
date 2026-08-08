using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SubtitleSystem : MonoBehaviour
{
    public static SubtitleSystem Instance { get; private set; }

    // หอกระจายข่าวให้ UI คอยฟัง (ไม่ต้องพึ่งพากันและกัน)
    public static event Action<bool> OnSubtitleToggle; // สั่งเปิด/ปิด
    public static event Action<string> OnSubtitleUpdate; // สั่งเปลี่ยนข้อความ

    private AudioSource _audioSource;
    private Coroutine _subtitleCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // ซ่อนซับไตเติ้ลตอนเริ่มเกม
        OnSubtitleToggle?.Invoke(false);
    }

    public void PlaySubtitle(SubtitleDataSO subtitleData)
    {
        if (subtitleData == null) return;

        if (_subtitleCoroutine != null) StopCoroutine(_subtitleCoroutine);
        _subtitleCoroutine = StartCoroutine(PlaySubtitleRoutine(subtitleData));
    }

    private IEnumerator PlaySubtitleRoutine(SubtitleDataSO subtitleData)
    {
        // ตะโกนบอก UI ให้ "เปิด" ซับไตเติ้ล
        OnSubtitleToggle?.Invoke(true);

        foreach (var line in subtitleData.Lines)
        {
            // 1. จัดฟอร์แมตข้อความและสี
            string displayText = line.DefaultText;
            if (line.Speaker != null)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(line.Speaker.NameColor);
                displayText = $"<color=#{hexColor}>[{line.Speaker.DefaultName}]</color> {line.DefaultText}";
            }

            // ตะโกนบอก UI ให้ "เปลี่ยนข้อความ"
            OnSubtitleUpdate?.Invoke(displayText);

            // 2. จัดการเสียงและเวลา
            float waitTime = line.FallbackDuration;
            if (line.VoiceClip != null)
            {
                _audioSource.clip = line.VoiceClip;
                _audioSource.Play();
                waitTime = line.VoiceClip.length;
            }

            yield return new WaitForSeconds(waitTime);
        }

        // ตะโกนบอก UI ให้ "ปิด" ซับไตเติ้ล
        OnSubtitleToggle?.Invoke(false);
        _subtitleCoroutine = null;
    }
}