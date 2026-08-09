using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
public abstract class BaseLocomotion : MonoBehaviour,ILocomotionLock,IRootMotionControl
{
    [Header("Locomotion")]
    [SerializeField] private float _dampTime = 0.1f;
    [SerializeField] private float _multiply = 1f;

    [Header("Rotation")]
    [SerializeField] private float _rotateSpeed = 1f;

    [Header("Gravity")]
    [FormerlySerializedAs("gravityMultiplier")]
    [SerializeField] private float _gravityMultiplier = 1f;

    protected CharacterController CharacterController { get; private set; }
    protected Animator Animator {get; private set;}

    protected LocomotionAnim LocomotionAnim { get; private set; }
    protected RotationTransform Rotation { get; private set; }
    protected GravityCharacterCon Gravity { get; private set; }

    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        CharacterController = GetComponent<CharacterController>();

        LocomotionAnim = new LocomotionAnim(Animator,_dampTime,_multiply);
        Rotation = new RotationTransform(transform,_rotateSpeed);

        Gravity = new GravityCharacterCon(CharacterController,_gravityMultiplier);
    }
    protected virtual void OnAnimatorMove()
    {
        Vector3 rootMotionPosition = _rootMotionEnabled
            ? Animator.deltaPosition
            : Vector3.zero;

        CharacterController.Move(
            rootMotionPosition + Gravity.Gravity());

        if (_rootMotionEnabled && IsMovementLocked)
        {
            transform.rotation *= Animator.deltaRotation;
        }  
    }

    // IMovementLock implementation
    private readonly HashSet<object> _movementLockOwners = new();
    private readonly HashSet<object> _moveAnimationResetOwners = new();

    public bool IsMovementLocked => _movementLockOwners.Count > 0;
    public bool ShouldResetMoveAnimation => _moveAnimationResetOwners.Count > 0;

    public void LockLocomotion(object owner,bool resetMoveAnimation = true)
    {
        if (owner == null)
            return;

        _movementLockOwners.Add(owner);

        if (resetMoveAnimation)
            _moveAnimationResetOwners.Add(owner);
    }

    public void UnlockLocomotion(object owner)
    {
        if (owner == null)
            return;

        _movementLockOwners.Remove(owner);
        _moveAnimationResetOwners.Remove(owner);
    }
    
    // IRootMotionControl implementation
    private bool _rootMotionEnabled = true;
    public void SetRootMotionEnabled(bool enabled)
    {
        _rootMotionEnabled = enabled;
    }

    public float GetMoveMultiply() => LocomotionAnim.Multiply;
    public void SetMoveMultiply(float multiply) => LocomotionAnim.Multiply = multiply;
    public float GetRotateSmoothSpeed() => Rotation.Speed;
    public void SetRotateSmoothSpeed(float value) => Rotation.Speed = value;
    public void StopMovement(bool enableMove = true) => LocomotionAnim.SetMovementEnabled(!enableMove);
}
