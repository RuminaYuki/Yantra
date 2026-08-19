using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class YantEffectSettings
{
    [Tooltip("ต้องมี IYantEffect MonoBehaviour ติดอยู่บน root ของ prefab")]
    public MonoBehaviour _yantReferences;
    [Tooltip("Effect type ที่จะเรียกใช้ \nStart : spawn แล้วเรียกใช้เลย \nOne Shot : กดใช้ได้ครั้งเดียว \nHold : ต้องกดค้างเพื่อใช้งาน")]
    public YantEffectType _effectType;
    public IYantEffect _yantEffect = null;
    [Tooltip("ระยะเวลา Effect , ถ้าอยู่เป็น effect สุดท้ายจะรอเวลานี้ก่อน destroy")]
    public float _effectDuration = 1f;
    [Tooltip("ถ้า Effect type เป็น hold จะมาอ้างอิงเวลากดค้างตรงนี้")]
    public float _holdDuration = 0f;
}

public class YantEffectController : MonoBehaviour
{
    [Tooltip("อายุการใช้งานก่อนโดน Destroy")]
    [SerializeField] private float _yantLifeTime = 1.0f;

    [SerializeField] private List<YantEffectSettings> _effectSettings = new List<YantEffectSettings>();

    [Header("Animation")]
    [SerializeField] private string AnimationName = "";
    [SerializeField] private string CancelHold = " ";
    [SerializeField] private int LayerIndex = 0;

    private GameObject _playerRoot;
    private Coroutine _destroyCoroutine;
    private readonly List<IYantAnimationTiming> _pendingAnimationTimings = new();

    private void Awake()
    {
        CheckEffectReferences();
    }

    private void Start()
    {
        TryInitializeStart();
        if (_yantLifeTime > 0f)
        {
            _destroyCoroutine = StartCoroutine(DestroyAfterDelay(_yantLifeTime));
        }
    }

    private void CheckEffectReferences()
    {
        for (int i = _effectSettings.Count - 1; i >= 0; i--)
        {
            var setting = _effectSettings[i];
            setting._yantEffect = setting._yantReferences as IYantEffect;

            if (setting._yantEffect == null)
            {
                _effectSettings.RemoveAt(i);
                continue;
            }

            _effectSettings[i] = setting;
        }
    }

    private void TryInitializeStart()
    {
        for (int i = _effectSettings.Count - 1; i >= 0; i--)
        {
            var setting = _effectSettings[i];
            if (!setting._effectType.HasFlag(YantEffectType.Start))
            {
                continue;
            }

            setting._yantEffect.Initialize(_playerRoot, false);
            RegisterAnimationTiming(setting._yantEffect);
            PlayAnimation(AnimationName);

            if (setting._effectType.HasFlag(YantEffectType.OneShot))
            {
                if (_effectSettings.Count == 1)
                {
                    DestroyGameObject(setting._effectDuration);
                }
                _effectSettings.RemoveAt(i);
            }
            else
            {
                _effectSettings[i] = setting;
            }
        }
    }

    public void TryInitialize(float holdTime, bool holdLMB = false)
    {
        for (int i = _effectSettings.Count - 1; i >= 0; i--)
        {
            var setting = _effectSettings[i];
            if (setting._effectType.HasFlag(YantEffectType.Hold))
            {
                if (setting._holdDuration > 0f && holdTime < setting._holdDuration)
                {
                    continue;
                }
            }

            bool initialized = setting._yantEffect.Initialize(_playerRoot, holdLMB);
            if (holdLMB)
            {
                RegisterAnimationTiming(setting._yantEffect);
                PlayAnimation(AnimationName);
            } else if (!string.IsNullOrWhiteSpace(CancelHold))
            {
                PlayAnimation(CancelHold);
            }

            if (setting._effectType.HasFlag(YantEffectType.OneShot) && initialized)
            {
                if (_effectSettings.Count == 1)
                {
                    DestroyGameObject(setting._effectDuration);
                }
                _effectSettings.RemoveAt(i);
            }
            else
            {
                _effectSettings[i] = setting;
            }
        }
    }

    public void SetDefaultValue(GameObject playerRoot)
    {
        _playerRoot = playerRoot;
    }

    public void TriggerAnimationTiming()
    {
        foreach (IYantAnimationTiming animationTiming in _pendingAnimationTimings)
        {
            animationTiming.TriggerAnimationTiming();
            Debug.Log(animationTiming);
        }

        _pendingAnimationTimings.Clear();
    }

    private void RegisterAnimationTiming(IYantEffect yantEffect)
    {
        if (yantEffect is IYantAnimationTiming animationTiming &&
            !_pendingAnimationTimings.Contains(animationTiming))
        {
            _pendingAnimationTimings.Add(animationTiming);
        }

        if (_playerRoot == null || yantEffect is not IYantAnimationTiming)
        {
            return;
        }

        Animator animator = _playerRoot.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            return;
        }

        PlayerYantAnimationEvent animationEvent =
            animator.GetComponent<PlayerYantAnimationEvent>();

        if (animationEvent == null)
        {
            animationEvent = animator.gameObject.AddComponent<PlayerYantAnimationEvent>();
        }

        animationEvent.SetCurrentController(this);
    }

    private void PlayAnimation(string animationName)
    {
        if (string.IsNullOrWhiteSpace(animationName) || _playerRoot == null)
        {
            return;
        }

        Animator animator = _playerRoot.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Cannot play yant animation because the player has no Animator.");
            return;
        }

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(LayerIndex);
        if (currentState.IsName(animationName))
        {
            return;
        }

        Debug.Log($"Player animation : {animationName}");
        animator.Play(animationName, LayerIndex);
    }

    private void DestroyGameObject(float delay)
    {
        if (_destroyCoroutine != null)
        {
            StopCoroutine(_destroyCoroutine);
            _destroyCoroutine = null;
        }

        _destroyCoroutine = StartCoroutine(DestroyAfterDelay(delay));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}

[System.Flags]
public enum YantEffectType
{
    None = 0,
    Start = 1 << 0,
    OneShot = 1 << 1,
    Hold = 1 << 2,
}