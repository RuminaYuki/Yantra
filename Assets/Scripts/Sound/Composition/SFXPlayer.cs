using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine returnRoutine;
    private Coroutine fadeRoutine; // คอยคุมการFade ขึ้นลง
    private float baseVolume = 1f; // จำความดังดั้งเดิมเอาไว้

    [HideInInspector] public string myPoolTag;

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private float Setup(SoundData data, bool loop)
    {
        AudioClip clipToPlay = data.GetClip();

        if (clipToPlay == null)
        {
            Debug.LogWarning("[SFXPlayer] SoundData missing AudioClip");
            ReturnHome();
            return 0f;
        }

        if (returnRoutine != null) { StopCoroutine(returnRoutine); returnRoutine = null; }
        if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }

        float pitch = data.GetPitch();

        // จำความดังดั้งเดิมไว้ จะได้หรี่และดันกลับมาถูก
        baseVolume = data.GetVolume();

        audioSource.clip = clipToPlay;
        audioSource.volume = baseVolume;
        audioSource.pitch = pitch;
        audioSource.spatialBlend = data.spatialBlend;
        audioSource.loop = loop;

        if (data.mixerGroup != null)
            audioSource.outputAudioMixerGroup = data.mixerGroup;

        audioSource.Play();

        return clipToPlay.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
    }

    public float Play(SoundData data)
    {
        float duration = Setup(data, false);
        if (duration <= 0f) return 0f;
        returnRoutine = StartCoroutine(ReturnAfterFinished(duration));
        return duration;
    }

    public float PlayLoop(SoundData data, float duration)
    {
        float clipLength = Setup(data, true);
        if (clipLength <= 0f) return 0f;
        returnRoutine = StartCoroutine(ReturnAfterFinished(duration));
        return duration;
    }

    public void PlayLoopForever(SoundData data)
    {
        Setup(data, true);
    }

    public void Stop()
    {
        if (returnRoutine != null) { StopCoroutine(returnRoutine); returnRoutine = null; }
        if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
        if (audioSource != null) audioSource.Stop();
        ReturnHome();
    }

    public void FadeOutAndStop(float fadeTime)
    {
        if (returnRoutine != null) { StopCoroutine(returnRoutine); returnRoutine = null; }
        if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
        fadeRoutine = StartCoroutine(FadeOutStopRoutine(fadeTime));
    }

    private IEnumerator FadeOutStopRoutine(float fadeTime)
    {
        float startVolume = audioSource.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = baseVolume;
        fadeRoutine = null;
        ReturnHome();
    }

    // ==========================================
    // ระบบเฟดเสียงแบบต่อเนื่อง (ไม่ปิดลำโพง แค่หรี่เสียง)
    // ==========================================

    /// <summary>บังคับหรี่เสียงทันที (เช่น เริ่มต้นที่ 0 เพื่อเตรียม Fade In)</summary>
    public void SetVolumeMultiplier(float multiplier)
    {
        if (audioSource != null)
            audioSource.volume = baseVolume * Mathf.Clamp01(multiplier);
    }

    /// <summary>ค่อยๆ หรี่หรือเร่งเสียงตามตัวคูณ (1.0 = ดังปกติ, 0.3 = แว่วๆ)</summary>
    public void FadeToVolumeMultiplier(float targetMultiplier, float fadeTime)
    {
        if (!gameObject.activeInHierarchy) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeVolumeRoutine(targetMultiplier, fadeTime));
    }

    private IEnumerator FadeVolumeRoutine(float targetMultiplier, float fadeTime)
    {
        float startVol = audioSource.volume;
        float targetVol = baseVolume * Mathf.Clamp01(targetMultiplier);
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVol, targetVol, t / fadeTime);
            yield return null;
        }

        audioSource.volume = targetVol;
        fadeRoutine = null;
    }

    // ==========================================

    private IEnumerator ReturnAfterFinished(float duration)
    {
        yield return new WaitForSeconds(duration + 0.15f);
        returnRoutine = null;
        ReturnHome();
    }

    private void ReturnHome()
    {
        if (!gameObject.activeInHierarchy) return;
        if (ObjectPooler.Instance != null && !string.IsNullOrEmpty(myPoolTag))
            ObjectPooler.Instance.ReturnToPool(myPoolTag, gameObject);
        else
            Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (returnRoutine != null) { StopCoroutine(returnRoutine); returnRoutine = null; }
        if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
        if (audioSource != null) { audioSource.Stop(); audioSource.clip = null; }
    }
}