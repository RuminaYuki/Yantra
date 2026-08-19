using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine returnRoutine;
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

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        float pitch = data.GetPitch();

        audioSource.clip = clipToPlay;

        // ดึงค่า Volume ที่ผ่านการคำนวณน้ำหนักเท้ามาแล้ว
        audioSource.volume = data.GetVolume();

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
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        if (audioSource != null)
            audioSource.Stop();

        ReturnHome();
    }

    public void FadeOutAndStop(float fadeTime)
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        returnRoutine = StartCoroutine(FadeRoutine(fadeTime));
    }

    private IEnumerator FadeRoutine(float fadeTime)
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
        audioSource.volume = startVolume;
        returnRoutine = null;
        ReturnHome();
    }

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
        {
            ObjectPooler.Instance.ReturnToPool(myPoolTag, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}