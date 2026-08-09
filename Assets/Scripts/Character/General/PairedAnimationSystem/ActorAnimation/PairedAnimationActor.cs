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
        public string exitStateName;
    }

    [SerializeField] private PairedAnimation[] _animations;

    private Animator _animator;
    private ILocomotionLock _movementLock;
    private IRootMotionControl _rootMotionControl;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movementLock = GetComponent<ILocomotionLock>();
        _rootMotionControl = GetComponent<IRootMotionControl>();
    }
    public void SetRootMotionEnabled(bool enabled)
    {
        _rootMotionControl?.SetRootMotionEnabled(enabled);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void LockMovement()
    {
        _movementLock?.LockLocomotion(
            this,
            resetMoveAnimation: false);
    }

    public void UnlockMovement()
    {
        _movementLock?.UnlockLocomotion(this);
    }

    public bool CanPlay(PairedAnimationId animationId)
    {
        return TryGetAnimation(animationId, out _);
    }

    public void PlayAnimation(PairedAnimationId animationId)
    {
        if (!TryGetAnimation(
            animationId,
            out PairedAnimation animation))
        {
            Debug.LogWarning(
                $"{name} does not support this paired animation.",
                this);
            return;
        }

        _animator.CrossFadeInFixedTime(
            animation.stateName,
            0.25f);
    }

    public void ExitAnimation(PairedAnimationId animationId)
    {
        if (!TryGetAnimation(
            animationId,
            out PairedAnimation animation))
            return;

        if (string.IsNullOrWhiteSpace(animation.exitStateName))
        {
            Debug.LogWarning(
                $"{name} has no exit state for this paired animation.",
                this);
            return;
        }

        _animator.CrossFadeInFixedTime(
            animation.exitStateName,
            0.25f);
    }

    public bool IsInAnimation(PairedAnimationId animationId)
    {
        if (_animator == null || _animator.IsInTransition(0))
            return false;

        if (!TryGetAnimation(
            animationId,
            out PairedAnimation animation))
            return false;

        AnimatorStateInfo stateInfo =
            _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.IsName(animation.stateName);
    }

    public bool IsAnimationFinished(PairedAnimationId animationId)
    {
        if (_animator == null)
            return false;

        if (!TryGetAnimation(
            animationId,
            out PairedAnimation animation))
            return false;

        AnimatorStateInfo stateInfo =
            _animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(animation.stateName))
            return true;

        return stateInfo.normalizedTime >= 0.95f;
    }

    private bool TryGetAnimation(
        PairedAnimationId animationId,
        out PairedAnimation result)
    {
        if (animationId == null || _animations == null)
        {
            result = null;
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

            result = animation;
            return true;
        }

        result = null;
        return false;
    }
}
