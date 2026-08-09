using UnityEngine;
using System.Collections;
[RequireComponent(typeof(CharacterController),typeof(Animator),typeof(PathNavigator))]
public class EnemyLocomotion : BaseLocomotion
{
    [Header("Locomotion Anim Parameter")]
    [SerializeField] private string _nameParameterMoveZ;
    [SerializeField] private string _nameTurnAngle;
    [SerializeField] private string _nameStartTurn;
    [SerializeField] private float _angleTurn = 45;

    PathNavigator _pathNavigator;
    private bool _wasMoving;

    protected override void Awake()
    {
        base.Awake();
        LocomotionAnim.SetMoveParameter(_nameParameterMoveZ);
        LocomotionAnim.SetTurnParameter(_nameTurnAngle,_nameStartTurn);
        _pathNavigator = GetComponent<PathNavigator>();
    }
    private void Update()
    {
        if (IsMovementLocked)
        {
            _wasMoving = false;

            if (ShouldResetMoveAnimation)
                LocomotionAnim.SetMove(0f);

            return;
        }

        bool isMoving = _pathNavigator.Direction.sqrMagnitude > 0.01f;
        TurnToTarget(_pathNavigator.Target);
        LocomotionAnim.SetMove(isMoving ? 1f : 0f);
    }
    private void FixedUpdate()
    {
        if (IsMovementLocked)
        {
            Rotation.Rotate(Vector3.zero);
            return;
        }
        Rotation.Rotate(_pathNavigator.Direction);
    }
    private void TurnToTarget(Transform target)
    {
        bool isMoving = _pathNavigator.Direction.sqrMagnitude > 0.01f;
        bool justStartedMoving = isMoving && !_wasMoving;

        if (target != null && justStartedMoving)
        {
            float angle = GetSignedAngleFromEnemy(transform,target);

            if (Mathf.Abs(angle) >= _angleTurn)
            {
                LocomotionAnim.SetTurn(angle);
            }
        }

        _wasMoving = isMoving;
    }
    private float GetSignedAngleFromEnemy(Transform enemy, Transform target)
    {
        Vector3 enemyForward =
            Vector3.ProjectOnPlane(enemy.forward, Vector3.up).normalized;

        Vector3 directionToTarget =
            Vector3.ProjectOnPlane(target.position - enemy.position,Vector3.up
            ).normalized;

        return Vector3.SignedAngle(enemyForward,directionToTarget,Vector3.up);
    }
}
