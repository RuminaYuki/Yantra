using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private SoundTable soundTable;

    [Header("SFX")]
    [SerializeField] private string audioPoolTag = "SFXsource";

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;

    [Header("Voice System")]
    [Tooltip("ลำโพงเฉพาะสำหรับเสียงพากย์ (2D)")]
    private AudioSource _voiceSource;

    [Header("🎙️ Voice / Dialogue")]
    [Tooltip("AudioSource เฉพาะเสียงพากย์ (แยกจาก BGM เพื่อให้หยุด/ข้ามได้อิสระ)")]
    [SerializeField] private AudioSource voiceSource;

    [Tooltip("ช่องเสียงพากย์ใน Mixer — ผู้เล่นจะได้ปรับดังเบาแยกได้")]
    [SerializeField] private AudioMixerGroup voiceMixerGroup;

    [Header("🔉 Ducking (หรี่เพลงตอนมีคนพูด)")]
    [SerializeField] private bool duckBgmDuringVoice = true;
    [Range(0f, 1f)][SerializeField] private float duckedBgmMultiplier = 0.35f;
    [SerializeField] private float duckFadeTime = 0.25f;

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Mixer Parameter Names")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [SerializeField] private string bgmParam = "BGMVolume";
    [SerializeField] private string voiceParam = "VoiceVolume";

    private SoundID currentBgmId;
    private Dictionary<SoundID, SoundData> soundsById;

    private float bgmBaseVolume = 1f;
    private Coroutine duckRoutine;

    protected override bool UseDontDestroyOnLoad => false;

    /// <summary>ตอนนี้มีเสียงพากย์เล่นอยู่มั้ย</summary>
    public bool IsVoicePlaying => voiceSource != null && voiceSource.isPlaying;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        if (bgmSource == null)
            Debug.LogWarning("[SoundManager] BGM source is missing!");

        if (voiceSource == null)
            Debug.LogWarning("[SoundManager] Voice source is missing! (เสียงพากย์จะเล่นไม่ได้)");
        else
            voiceSource.spatialBlend = 0f; // เสียงพากย์เป็น 2D เสมอ ดังเท่ากันทุกมุมกล้อง

        BuildSoundLookup();
    }

    private void BuildSoundLookup()
    {
        soundsById = new Dictionary<SoundID, SoundData>();

        if (soundTable == null || soundTable.soundLibraries == null)
        {
            Debug.LogWarning("[SoundManager] SoundTable ว่างเปล่า!");
            return;
        }

        foreach (SoundLibrary library in soundTable.soundLibraries)
        {
            if (library == null || library.sounds == null) continue;

            foreach (SoundData sound in library.sounds)
            {
                if (sound == null || sound.id == null) continue;

                if (soundsById.ContainsKey(sound.id))
                    Debug.LogWarning($"[SoundManager] SoundID '{sound.id.name}' ซ้ำ! ตัวใน '{library.name}' ทับของเดิม");

                soundsById[sound.id] = sound;
            }
        }
    }

    private bool TryGetData(SoundID id, out SoundData data)
    {
        data = null;

        if (id == null)
        {
            Debug.LogWarning("[SoundManager] เรียกเสียงด้วย SoundID ที่เป็น null");
            return false;
        }

        if (soundsById == null) return false;

        if (!soundsById.TryGetValue(id, out data))
        {
            Debug.LogWarning($"[SoundManager] ไม่พบ SoundID '{id.name}' ใน SoundTable!");
            return false;
        }

        return true;
    }

    // ==========================================
    // 🔊 SFX
    // ==========================================
    public float PlaySFX(SoundID id, Vector3 position)
    {
        SFXPlayer player = SpawnPlayer(id, position, out SoundData data);
        if (player == null) return 0f;
        return player.Play(data);
    }

    public SFXPlayer PlayLoopSFX(SoundID id, Vector3 position, float duration)
    {
        SFXPlayer player = SpawnPlayer(id, position, out SoundData data);
        if (player == null) return null;

        player.PlayLoop(data, duration);
        return player;
    }

    public SFXPlayer PlayLoopSFXForever(SoundID id, Vector3 position)
    {
        SFXPlayer player = SpawnPlayer(id, position, out SoundData data);
        if (player == null) return null;

        player.PlayLoopForever(data);
        return player;
    }

    private SFXPlayer SpawnPlayer(SoundID id, Vector3 position, out SoundData data)
    {
        if (!TryGetData(id, out data)) return null;

        if (ObjectPooler.Instance == null)
        {
            Debug.LogWarning("[SoundManager] ไม่พบ ObjectPooler ในซีน");
            return null;
        }

        SFXPlayer result = null;

        GameObject audioObject = ObjectPooler.Instance.SpawnFromPool(
            audioPoolTag, position, Quaternion.identity,
            (obj) =>
            {
                if (obj.TryGetComponent(out SFXPlayer p))
                {
                    p.myPoolTag = audioPoolTag;
                    result = p;
                }
            }
        );

        if (audioObject == null) return null;

        if (result == null)
            Debug.LogWarning($"[SoundManager] Prefab ในพูล '{audioPoolTag}' ไม่มี SFXPlayer");

        return result;
    }

    public void PlayEventSFX(SoundID id) => PlaySFX(id, GetListenerPosition());

    private Vector3 GetListenerPosition()
    {
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener != null) return listener.transform.position;
        if (Camera.main != null) return Camera.main.transform.position;
        return Vector3.zero;
    }

    // ==========================================
    // 🎙️ Voice / Dialogue
    // ==========================================

    /// <summary>
    /// เล่นเสียงพากย์ 1 ประโยค — ระบบซับไตเติ้ลเรียกอันนี้แทน PlayOneShot
    /// </summary>
    /// <returns>ความยาวเสียง (วินาที) เอาไปตั้งเวลาโชว์ซับได้เลย</returns>
    public float PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (clip == null || voiceSource == null) return 0f;

        // ประโยคใหม่มาแทรก = ตัดประโยคเดิมทิ้งทันที (บทพูดเล่นทีละประโยค)
        voiceSource.Stop();

        voiceSource.clip = clip;
        voiceSource.volume = Mathf.Clamp01(volume);
        voiceSource.pitch = 1f;      // เสียงพากย์ห้ามสุ่ม pitch เด็ดขาด
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f;

        if (voiceMixerGroup != null)
            voiceSource.outputAudioMixerGroup = voiceMixerGroup;

        voiceSource.Play();

        if (duckBgmDuringVoice)
            StartDuck(true);

        return clip.length;
    }

    /// <summary>หยุดเสียงพากย์กลางคัน (เช่น ผู้เล่นกดข้ามบทพูด)</summary>
    public void StopVoice()
    {
        if (voiceSource != null)
            voiceSource.Stop();

        if (duckBgmDuringVoice)
            StartDuck(false);
    }

    private void StartDuck(bool ducked)
    {
        if (bgmSource == null) return;

        if (duckRoutine != null) StopCoroutine(duckRoutine);
        duckRoutine = StartCoroutine(DuckRoutine(ducked));
    }

    private IEnumerator DuckRoutine(bool ducked)
    {
        float target = ducked ? bgmBaseVolume * duckedBgmMultiplier : bgmBaseVolume;
        float start = bgmSource.volume;
        float t = 0f;

        while (t < duckFadeTime)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(start, target, t / duckFadeTime);
            yield return null;
        }

        bgmSource.volume = target;

        // ถ้ากำลังหรี่อยู่ ให้เฝ้ารอจนเสียงพากย์จบแล้วค่อยดันเพลงกลับเอง
        if (ducked)
        {
            while (IsVoicePlaying) yield return null;
            duckRoutine = StartCoroutine(DuckRoutine(false));
            yield break;
        }

        duckRoutine = null;
    }

    // ==========================================
    // 🎶 BGM
    // ==========================================
    public void PlayBGM(SoundID id)
    {
        if (bgmSource == null) return;
        if (!TryGetData(id, out SoundData data)) return;

        AudioClip clipToPlay = data.GetClip();
        if (clipToPlay == null) return;

        if (currentBgmId == id && bgmSource.isPlaying) return;

        currentBgmId = id;
        bgmBaseVolume = data.volume;

        bgmSource.clip = clipToPlay;
        bgmSource.volume = IsVoicePlaying && duckBgmDuringVoice
            ? bgmBaseVolume * duckedBgmMultiplier
            : bgmBaseVolume;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;

        if (data.mixerGroup != null)
            bgmSource.outputAudioMixerGroup = data.mixerGroup;

        bgmSource.Play();
    }

    public void StopBGM()
    {
        currentBgmId = null;
        if (bgmSource != null) bgmSource.Stop();
    }

    // ==========================================
    // 🎛️ Volume (รับค่า 0–1 จาก Slider แล้วแปลงเป็น dB ให้เอง)
    // ==========================================
    public void SetMasterVolume(float level01) => SetMixerVolume(masterParam, level01);
    public void SetSoundFXVolume(float level01) => SetMixerVolume(sfxParam, level01);
    public void SetMusicVolume(float level01) => SetMixerVolume(bgmParam, level01);
    public void SetVoiceVolume(float level01) => SetMixerVolume(voiceParam, level01);

    private void SetMixerVolume(string param, float level01)
    {
        if (audioMixer == null || string.IsNullOrEmpty(param)) return;

        float db = level01 <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(level01)) * 20f;
        audioMixer.SetFloat(param, db);
    }
}