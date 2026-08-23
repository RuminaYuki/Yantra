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
    private AudioSource _voiceSource;

    [Header("Voice / Dialogue")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioMixerGroup voiceMixerGroup;

    [Header("Ducking")]
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

    public bool IsVoicePlaying => voiceSource != null && voiceSource.isPlaying;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        if (bgmSource == null)
            Debug.LogWarning("[SoundManager] BGM source is missing!");

        if (voiceSource == null)
            Debug.LogWarning("[SoundManager] Voice source is missing!");
        else
            voiceSource.spatialBlend = 0f;

        BuildSoundLookup();
    }

    private void BuildSoundLookup()
    {
        soundsById = new Dictionary<SoundID, SoundData>();

        if (soundTable == null || soundTable.soundLibraries == null) return;

        foreach (SoundLibrary library in soundTable.soundLibraries)
        {
            if (library == null || library.sounds == null) continue;

            foreach (SoundData sound in library.sounds)
            {
                if (sound == null || sound.id == null) continue;
                soundsById[sound.id] = sound;
            }
        }
    }

    private bool TryGetData(SoundID id, out SoundData data)
    {
        data = null;
        if (id == null || soundsById == null) return false;
        return soundsById.TryGetValue(id, out data);
    }

    // ==========================================
    // SFX (One-shot) — ไม่ต้องใช้ใบเสร็จ เพราะยิงแล้วจบ ไม่มีใครสั่งงานมันทีหลัง
    // ==========================================
    public float PlaySFX(SoundID id, Vector3 position)
    {
        SFXPlayer player = SpawnPlayer(id, position, out SoundData data);
        if (player == null) return 0f;
        return player.Play(data);
    }

    public float PlaySFXAttached(SoundID id, Transform target)
    {
        if (target == null) return 0f;

        SFXPlayer player = SpawnPlayer(id, target.position, out SoundData data);
        if (player == null) return 0f;

        player.FollowTarget(target);
        return player.Play(data);
    }

    // ==========================================
    // SFX (Loop) — [CHANGED] คืน SFXHandle แทน SFXPlayer
    // ==========================================
    // เสียงลูปคือเสียงที่ "มีคนถือไว้สั่งงานทีหลัง" (หรี่, fade, หยุด)
    // เลยต้องออกใบเสร็จให้ ห้ามยื่นตัวลำโพงจริงให้ใครถือเด็ดขาด
    //
    // ⚠️ สังเกตลำดับให้ดี: ต้อง Play ก่อน แล้วค่อยอ่าน Version
    //    เพราะ Setup() ข้างใน Play จะ +1 ให้ version
    //    ถ้าอ่าน Version ก่อน จะได้เลขเก่า → ใบเสร็จหมดอายุตั้งแต่วินาทีแรก
    public SFXHandle PlayLoopSFX(SoundID id, Vector3 position, float duration)
    {
        SFXPlayer player = SpawnPlayer(id, position, out SoundData data);
        if (player == null) return SFXHandle.None;

        player.PlayLoop(data, duration);
        return new SFXHandle(player, player.Version);
    }

    public SFXHandle PlayLoopSFXForever(SoundID id, Vector3 position)
    {
        SFXPlayer player = SpawnPlayer(id, position, out SoundData data);
        if (player == null) return SFXHandle.None;

        player.PlayLoopForever(data);
        return new SFXHandle(player, player.Version);
    }

    private SFXPlayer SpawnPlayer(SoundID id, Vector3 position, out SoundData data)
    {
        if (!TryGetData(id, out data)) return null;

        if (ObjectPooler.Instance == null) return null;

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
    // Voice & Ducking
    // ==========================================
    public float PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (clip == null || voiceSource == null) return 0f;

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.volume = Mathf.Clamp01(volume);
        voiceSource.pitch = 1f;
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f;

        if (voiceMixerGroup != null)
            voiceSource.outputAudioMixerGroup = voiceMixerGroup;

        voiceSource.Play();

        if (duckBgmDuringVoice)
            StartDuck(true);

        return clip.length;
    }

    public void StopVoice()
    {
        if (voiceSource != null) voiceSource.Stop();
        if (duckBgmDuringVoice) StartDuck(false);
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

        if (ducked)
        {
            while (IsVoicePlaying) yield return null;
            duckRoutine = StartCoroutine(DuckRoutine(false));
            yield break;
        }
        duckRoutine = null;
    }

    // ==========================================
    // BGM
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
        bgmSource.volume = IsVoicePlaying && duckBgmDuringVoice ? bgmBaseVolume * duckedBgmMultiplier : bgmBaseVolume;
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
    // Volume Control
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