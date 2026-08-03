using UnityEngine;
using System.Collections;
[RequireComponent(typeof(CharacterController),typeof(Animator))]
public class EnemyLocomotion : BaseLocomotion
{
    private Vector3 _directionMove;

    [Header("Locomotion Anim Parameter")]
    [SerializeField] private string _nameParameterMoveZ;

    protected override void Awake()
    {
        base.Awake();
        MoveAnimator.SetParameter(_nameParameterMoveZ);
    }
    private void Update()
    {
        if (IsMovementLocked)
        {
            if (ShouldResetMoveAnimation)
                MoveAnimator.SetMove(0f);

            return;
        }
        MoveAnimator.SetMove(1f);
    }
    private void FixedUpdate()
    {
        if (IsMovementLocked)
        {
            Rotation.Rotate(Vector3.zero);
            return;
        }
        Rotation.Rotate(transform.forward);
    }
}
