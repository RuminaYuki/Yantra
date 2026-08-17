using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine returnRoutine;
    [HideInInspector] public string myPoolTag; // ตัวแปรสำหรับจำว่าตัวเองมาจากแท็กไหน

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // ตัวช่วยกลาง: เซ็ตค่าลำโพงจากคูปอง แล้วคืนความยาวเสียงจริง (คิด pitch แล้ว)
    private float Setup(SoundData data, bool loop)
    {
        AudioClip clipToPlay = data.GetClip();

        if (clipToPlay == null)
        {
            Debug.LogWarning("[SFXPlayer] SoundData ไม่มี AudioClip");
            ReturnHome();
            return 0f;
        }

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        float pitch = data.GetPitch();

        // นำค่าจากคูปอง (Inspector) มาเซ็ตใส่ลำโพง
        audioSource.clip = clipToPlay;
        audioSource.volume = data.volume;
        audioSource.pitch = pitch;
        audioSource.spatialBlend = data.spatialBlend;
        audioSource.loop = loop;

        if (data.mixerGroup != null)
            audioSource.outputAudioMixerGroup = data.mixerGroup;

        audioSource.Play();

        // [FIX] ของเดิมใช้ clip.length ตรงๆ แต่ถ้าสุ่ม pitch ได้ 0.9
        // เสียงจะยาวขึ้นเป็น length / 0.9 แล้ว coroutine ปิดลำโพงก่อนเสียงจบ → เสียงแหว่ง
        // (เสียงฝีเท้าที่เปิด useRandomPitch จะโดนบ่อยมาก)
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

    /// <summary>[ADD] วนไม่มีกำหนด ต้องเรียก Stop() เองเท่านั้น (เช่น เสียงหายใจตอน Tani ไล่)</summary>
    public void PlayLoopForever(SoundData data)
    {
        Setup(data, true);
        // ไม่ตั้ง coroutine — รอคำสั่ง Stop() อย่างเดียว
    }

    /// <summary>[ADD] สั่งหยุดกลางคันแล้วคืนลำโพงเข้าโกดังทันที</summary>
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

    /// <summary>[ADD] ค่อยๆ หรี่เสียงลงแล้วค่อยคืนโกดัง (เหมาะกับเสียงบรรยากาศ)</summary>
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
        yield return new WaitForSeconds(duration);
        returnRoutine = null;
        ReturnHome();
    }

    // แทนที่จะปิดตัวเองดื้อๆ ให้ส่งตัวเองไปให้โกดังประเมินว่าจะ "เก็บ" หรือ "เผาทิ้ง"
    private void ReturnHome()
    {
        if (!gameObject.activeInHierarchy) return;

        if (ObjectPooler.Instance != null && !string.IsNullOrEmpty(myPoolTag))
        {
            ObjectPooler.Instance.ReturnToPool(myPoolTag, gameObject);
        }
        else
        {
            // ถ้าหาโกดังไม่เจอจริงๆ ค่อยลบทิ้ง (กรณีนี้ไม่ควรเกิดแล้วหลังแก้ SoundManager)
            Destroy(gameObject);
        }
    }

    // กันเคสถูกปิดจากที่อื่น เช่น เปลี่ยนซีน หรือโดน pool สั่งปิด
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
            audioSource.clip = null; // ปล่อย reference ไม่ให้ค้าง
        }
    }
}