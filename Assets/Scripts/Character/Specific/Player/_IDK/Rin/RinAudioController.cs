using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kogetsu.Library.DesignPatternCore;

[System.Serializable]
public struct SoundAndTag
{
    public string Tag;
    public List<AudioClip> FootstepSounds;
    public List<AudioClip> RunSounds;
    public List<AudioClip> JumpLandSounds;
}

public class RinAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioSource _voiceSource;
    [SerializeField] private bool _interruptCurrentVoice = false;

    [Header("Footstep / SFX")]
    [SerializeField] private YantraStatsController _statsController;
    [SerializeField] private Transform _raycastPos;
    [SerializeField] private List<SoundAndTag> _soundAndTags = new();

    [Header("Damage & Stats SFX")]
    [SerializeField] private List<AudioClip> _damageSounds = new();
    [SerializeField, Range(0f, 1f)] private float _lowHpThreshold = 0.5f;
    [SerializeField] private List<AudioClip> _lowHp           = new();
    [SerializeField] private List<AudioClip> _staminaLow      = new();
    [SerializeField] private List<AudioClip> _staminaDepleted = new();
    [SerializeField] private List<AudioClip> _whileDrawing    = new();

    [Header("Voice Lines — สถานที่")]
    [SerializeField] private List<AudioClip> _enterVillage           = new();
    [SerializeField] private List<AudioClip> _findFatherClue         = new();
    [SerializeField] private List<AudioClip> _walkingToVillageCenter = new();
    [SerializeField] private List<AudioClip> _firstGhostEncounter    = new();
    [SerializeField] private List<AudioClip> _enterYantraHouse       = new();
    [SerializeField] private List<AudioClip> _findCave               = new();
    [SerializeField] private List<AudioClip> _findSculptureRoom      = new();
    [SerializeField] private List<AudioClip> _afterLeaveYantraHouse  = new();

    private Dictionary<string, SoundAndTag> _soundDictionary = new();
    private bool _lowHpAnnounced;

    private float _currentPlaySoundDelay;
    private bool _isMoving;
    private bool _isRunning;

    private void Awake()
    {
        foreach (var soundAndTag in _soundAndTags)
        {
            if (!_soundDictionary.ContainsKey(soundAndTag.Tag))
                _soundDictionary[soundAndTag.Tag] = soundAndTag;
            else
                Debug.LogWarning($"<color=#00FF88>[RinAudioController]</color> Tag '{soundAndTag.Tag}' ซ้ำ — ไม่สามารถเพิ่มเสียงได้");
        }
    }

    private void OnEnable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Subscribe<PlayerMovingEvent>(OnPlayerMoving);
            EventBus.Instance.Subscribe<PlayerRunningEvent>(OnPlayerRunning);
            EventBus.Instance.Subscribe<PlayerJumpingEvent>(OnPlayerJumping);
            EventBus.Instance.Subscribe<EventNameAndTag>(OnPlayerVoice);
            EventBus.Instance.Subscribe<PlayerTakeDamageEvent>(OnPlayerTakeDamage);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<PlayerMovingEvent>(OnPlayerMoving);
            EventBus.Instance.Unsubscribe<PlayerRunningEvent>(OnPlayerRunning);
            EventBus.Instance.Unsubscribe<PlayerJumpingEvent>(OnPlayerJumping);
            EventBus.Instance.Unsubscribe<EventNameAndTag>(OnPlayerVoice);
            EventBus.Instance.Unsubscribe<PlayerTakeDamageEvent>(OnPlayerTakeDamage);
        }
    }

    private void Update()
    {
        if (_currentPlaySoundDelay > 0f)
        {
            _currentPlaySoundDelay -= Time.deltaTime;
            return;
        }

        if (!_isMoving || !_statsController.IsGrounded || _raycastPos == null) return;

        if (Physics.Raycast(_raycastPos.position, Vector3.down, out RaycastHit hitInfo, 1f))
        {
            string tag = hitInfo.collider.tag;
            if (_soundDictionary.TryGetValue(tag, out SoundAndTag entry))
            {
                List<AudioClip> sounds = _isRunning && entry.RunSounds?.Count > 0
                    ? entry.RunSounds
                    : entry.FootstepSounds;

                if (sounds?.Count > 0)
                {
                    AudioClip clip = sounds[Random.Range(0, sounds.Count)];
                    _currentPlaySoundDelay = clip.length;
                    _audioSource.PlayOneShot(clip);
                }
            }
        }
    }

    private void OnPlayerMoving(PlayerMovingEvent data)
    {
        _isMoving = data.IsMoving;
        if (!_isMoving && _audioSource != null)
        {
            _audioSource.Stop();
            _currentPlaySoundDelay = 0f;
        }
    }
    private void OnPlayerRunning(PlayerRunningEvent data) => _isRunning = data.IsRunning;

    private void OnPlayerJumping(PlayerJumpingEvent data)
    {
        // เล่นเสียงตอนแตะพื้น (IsJumping กลับเป็น false)
        if (data.IsJumping || _raycastPos == null) return;
        if (Physics.Raycast(_raycastPos.position, Vector3.down, out RaycastHit hitInfo, 1f))
        {
            string tag = hitInfo.collider.tag;
            if (_soundDictionary.TryGetValue(tag, out SoundAndTag entry) && entry.JumpLandSounds?.Count > 0)
                _audioSource.PlayOneShot(entry.JumpLandSounds[Random.Range(0, entry.JumpLandSounds.Count)]);
        }
    }

    private void OnPlayerTakeDamage(PlayerTakeDamageEvent data)
    {
        if (_damageSounds == null || _damageSounds.Count == 0) return;
        AudioClip clip = _damageSounds[Random.Range(0, _damageSounds.Count)];
        _audioSource.PlayOneShot(clip);
        StartCoroutine(CheckLowHpAfterDamageSound(clip.length));
    }

    private IEnumerator CheckLowHpAfterDamageSound(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_statsController == null) yield break;
        float maxHp = _statsController.GetMaxHp();
        if (maxHp <= 0f) yield break;

        float percent = _statsController.GetCurrentHp() / maxHp;

        if (!_lowHpAnnounced && percent <= _lowHpThreshold)
        {
            _lowHpAnnounced = true;
            PlayVoice(_lowHp);
        }
        else if (_lowHpAnnounced && percent > _lowHpThreshold)
        {
            _lowHpAnnounced = false;
        }
    }

    private void OnPlayerVoice(EventNameAndTag data)
    {
        if (data.Tag != PlayerVoice.Tag) return;

        List<AudioClip> clips = data.Name switch
        {
            PlayerVoice.EnterVillage           => _enterVillage,
            PlayerVoice.FindFatherClue         => _findFatherClue,
            PlayerVoice.WalkingToVillageCenter => _walkingToVillageCenter,
            PlayerVoice.FirstGhostEncounter    => _firstGhostEncounter,
            PlayerVoice.EnterYantraHouse       => _enterYantraHouse,
            PlayerVoice.FindCave               => _findCave,
            PlayerVoice.FindSculptureRoom      => _findSculptureRoom,
            PlayerVoice.AfterLeaveYantraHouse  => _afterLeaveYantraHouse,
            PlayerVoice.WhileDrawing           => _whileDrawing,
            PlayerVoice.StaminaLow             => _staminaLow,
            PlayerVoice.StaminaDepleted        => _staminaDepleted,
            _                                  => null
        };

        PlayVoice(clips);
    }

    private void PlayVoice(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;

        AudioSource source = _voiceSource != null ? _voiceSource : _audioSource;
        if (!_interruptCurrentVoice && source.isPlaying) return;

        if (_interruptCurrentVoice) source.Stop();
        source.PlayOneShot(clips[Random.Range(0, clips.Count)]);
    }
}
