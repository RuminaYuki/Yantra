using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PairedAnimationActor : MonoBehaviour,IPairedAnimationActor
{
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

    public void PlayAnimation(string animationName)
    {
        _animator.CrossFadeInFixedTime(animationName,0.1f);
    }

    public void UnlockMovement()
    {
        _movementLock?.UnlockMovement(this);
    }
    public bool IsInAnimation(string stateName)
    {
        if (_animator == null || _animator.IsInTransition(0))
            return false;

        AnimatorStateInfo stateInfo =
            _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.IsName(stateName);
    }

    public bool IsAnimationFinished(string stateName)
    {
        if (_animator == null)
        return false;

        AnimatorStateInfo stateInfo =
            _animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(stateName))
            return true;

        return stateInfo.normalizedTime >= 0.95f;
    }
}