using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
public abstract class BaseLocomotion : MonoBehaviour,IMovementLock
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
    protected Animator Animator { get; private set; }

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
        CharacterController.Move(Animator.deltaPosition + Gravity.Gravity());
        if (IsMovementLocked)
        {
            transform.rotation *= Animator.deltaRotation;
            
        }   
    }

    private readonly HashSet<object> _movementLockOwners = new();
    public bool IsMovementLocked =>_movementLockOwners.Count > 0;

    public void LockMovement(object owner)
    {
        if (owner == null)
            return;

        _movementLockOwners.Add(owner);
    }

    public void UnlockMovement(object owner)
    {
        if (owner == null)
            return;

        _movementLockOwners.Remove(owner);
    }
}
