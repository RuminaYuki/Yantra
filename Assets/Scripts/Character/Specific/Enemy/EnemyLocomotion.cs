using UnityEngine;
using System.Collections;
[RequireComponent(typeof(CharacterController),typeof(Animator))]
public class EnemyLocomotion : BaseLocomotion
{
    [Header("Locomotion Anim Parameter")]
    [SerializeField] private string _nameParameterMoveZ;

    PathNavigator _pathNavigator;

    protected override void Awake()
    {
        base.Awake();
        MoveAnimator.SetParameter(_nameParameterMoveZ);
        _pathNavigator = GetComponent<PathNavigator>();
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

        Debug.Log($"Path direction: {_pathNavigator.Direction}");
        Rotation.Rotate(_pathNavigator.Direction);
    }
}
