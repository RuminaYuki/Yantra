using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
public class PairedAnimationActor : MonoBehaviour, IPairedAnimationActor
{
    [System.Serializable]
    private class PairedAnimation
    {
        [FormerlySerializedAs("type")]
        public PairedAnimationId id;
        public string stateName;
    }

    [SerializeField] private PairedAnimation[] _animations;

    private Animator _animator;
    private IMovementLock _movementLock;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movementLock = GetComponent<IMovementLock>();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void LockMovement()
    {
        _movementLock?.LockMovement(this);
    }

    public void UnlockMovement()
    {
        _movementLock?.UnlockMovement(this);
    }

    public bool CanPlay(PairedAnimationId animationId)
    {
        return TryGetStateName(animationId, out _);
    }

    public void PlayAnimation(PairedAnimationId animationId)
    {
        if (!TryGetStateName(animationId, out string stateName))
        {
            Debug.LogWarning(
                $"{name} does not support this paired animation.",
                this);
            return;
        }

        _animator.CrossFadeInFixedTime(stateName, 0.1f);
    }

    public bool IsInAnimation(PairedAnimationId animationId)
    {
        if (_animator == null || _animator.IsInTransition(0))
            return false;

        if (!TryGetStateName(animationId, out string stateName))
            return false;

        AnimatorStateInfo stateInfo =
            _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.IsName(stateName);
    }

    public bool IsAnimationFinished(PairedAnimationId animationId)
    {
        if (_animator == null)
            return false;

        if (!TryGetStateName(animationId, out string stateName))
            return false;

        AnimatorStateInfo stateInfo =
            _animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(stateName))
            return true;

        return stateInfo.normalizedTime >= 0.95f;
    }

    private bool TryGetStateName(
        PairedAnimationId animationId,
        out string stateName)
    {
        if (animationId == null || _animations == null)
        {
            stateName = null;
            return false;
        }

        foreach (PairedAnimation animation in _animations)
        {
            if (animation == null)
                continue;

            if (animation.id != animationId)
                continue;

            if (string.IsNullOrWhiteSpace(animation.stateName))
                continue;

            stateName = animation.stateName;
            return true;
        }

        stateName = null;
        return false;
    }
}
