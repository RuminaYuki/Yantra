using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GhostState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Execute,
    Search,
    Hit
}

public enum GhostAudioPlayMode
{
    Loop,
    OneShot
}

[System.Serializable]
public class GhostAudioEntry
{
    public AudioSource audioSource;
    public GhostAudioPlayMode playMode = GhostAudioPlayMode.Loop;

    [Space]
    public bool enableFade = false;
    [Range(0f, 100f)] public float fadePercent = 10f;
    [Range(0f, 1f)] public float fadeStartVolume = 0f;

    [Space]
    public List<AudioClip> clips = new();
}

public class GhostAudioManager : MonoBehaviour
{
    [Header("Footstep Channel")]
    [SerializeField] private AudioSource _footstepSource;
    [SerializeField] private AudioClip _walkFootstepClip;
    [SerializeField] private AudioClip _runFootstepClip;

    [Header("State Audio")]
    [SerializeField] private GhostAudioEntry _idleAudio = new();
    [SerializeField] private GhostAudioEntry _patrolAudio = new();
    [SerializeField] private GhostAudioEntry _chaseAudio = new();
    [SerializeField] private GhostAudioEntry _attackAudio = new();
    [SerializeField] private GhostAudioEntry _executeAudio = new();
    [SerializeField] private GhostAudioEntry _searchAudio = new();
    [SerializeField] private GhostAudioEntry _hitAudio = new();

    private Dictionary<GhostState, GhostAudioEntry> _entries;
    private GhostAudioEntry _currentLoopEntry;
    private GhostAudioEntry _currentOneShotEntry;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        BuildEntries();
    }

    private void BuildEntries()
    {
        _entries = new()
        {
            { GhostState.Idle,    _idleAudio },
            { GhostState.Patrol,  _patrolAudio },
            { GhostState.Chase,   _chaseAudio },
            { GhostState.Attack,  _attackAudio },
            { GhostState.Execute, _executeAudio },
            { GhostState.Search,  _searchAudio },
            { GhostState.Hit,     _hitAudio },
        };
    }

    public void PlayAudioForState(GhostState state)
    {
        if (_entries == null) BuildEntries();
        if (!_entries.TryGetValue(state, out var entry)) return;
        if (entry.audioSource == null || entry.clips == null || entry.clips.Count == 0) return;

        AudioClip clip = entry.clips[Random.Range(0, entry.clips.Count)];

        if (entry.playMode == GhostAudioPlayMode.Loop)
        {
            if (entry.audioSource.clip == clip && entry.audioSource.isPlaying) return;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            float targetVolume = entry.audioSource.volume;
            entry.audioSource.clip = clip;
            entry.audioSource.loop = true;
            entry.audioSource.Play();
            _currentLoopEntry = entry;

            if (entry.enableFade && entry.fadePercent > 0f)
            {
                float duration = clip.length * (entry.fadePercent / 100f);
                _fadeCoroutine = StartCoroutine(FadeVolume(entry.audioSource, entry.fadeStartVolume, duration, targetVolume));
            }
        }
        else
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            _currentOneShotEntry = entry;
            entry.audioSource.PlayOneShot(clip);

            if (entry.enableFade && entry.fadePercent > 0f)
            {
                float targetVolume = entry.audioSource.volume;
                float duration = clip.length * (entry.fadePercent / 100f);
                _fadeCoroutine = StartCoroutine(FadeVolume(entry.audioSource, entry.fadeStartVolume, duration, targetVolume));
            }
        }
    }

    public void StopCurrentStateAudio()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        if (_entries == null) BuildEntries();

        foreach (var entry in _entries.Values)
        {
            if (entry.audioSource == null) continue;
            entry.audioSource.Stop();
            entry.audioSource.clip = null;
            entry.audioSource.loop = false;
        }

        _currentLoopEntry = null;
        _currentOneShotEntry = null;
    }

    public void PlayFootstep(bool isRunning)
    {
        if (_footstepSource == null) return;
        AudioClip clip = isRunning ? _runFootstepClip : _walkFootstepClip;
        if (clip == null) return;
        _footstepSource.PlayOneShot(clip);
    }

    public void StopFootstep()
    {
        if (_footstepSource == null) return;
        _footstepSource.Stop();
    }

    private IEnumerator FadeVolume(AudioSource source, float startVolume, float fadeDuration, float targetVolume)
    {
        source.volume = startVolume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        source.volume = targetVolume;
        _fadeCoroutine = null;
    }
}
