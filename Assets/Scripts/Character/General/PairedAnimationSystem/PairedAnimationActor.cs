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
}