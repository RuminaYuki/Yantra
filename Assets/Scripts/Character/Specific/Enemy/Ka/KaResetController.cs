using UnityEngine;

[RequireComponent(typeof(StateMachineController))]
[RequireComponent(typeof(CharacterTeleporter))]
public class KaResetController : MonoBehaviour
{
    [Header("Reset Setup")]
    [SerializeField] private Transform _defaultResetPoint;

    private StateMachineController _stateMachineController;
    private CharacterTeleporter _teleporter;
    private Health _health;
    private StateFlags _stateFlags;
    private FlagCountdown _flagCountdown;
    private PathNavigator _pathNavigator;
    private WaypointPath _waypointPath;
    private AttackTokenUser _attackTokenUser;
    private PairedAnimationActor _pairedAnimationActor;
    private Animator _animator;

    private void Awake()
    {
        _stateMachineController = GetComponent<StateMachineController>();
        _teleporter = GetComponent<CharacterTeleporter>();
        _health = GetComponent<Health>();
        _stateFlags = GetComponent<StateFlags>();
        _flagCountdown = GetComponent<FlagCountdown>();
        _pathNavigator = GetComponent<PathNavigator>();
        _waypointPath = GetComponent<WaypointPath>();
        _attackTokenUser = GetComponent<AttackTokenUser>();
        _pairedAnimationActor = GetComponent<PairedAnimationActor>();
        _animator = GetComponent<Animator>();
    }

    public void ResetEnemy()
    {
        ResetEnemy(_defaultResetPoint);
    }

    public void ResetEnemy(Transform resetPoint)
    {
        if (resetPoint == null)
        {
            Debug.LogWarning("Ka reset point is not assigned.", this);
            return;
        }

        ResetCombat();
        ResetTimers();
        ResetNavigation();
        ResetTransform(resetPoint);
        ResetHealthAndFlags();
        ResetAnimator();
        ResetStateMachine();
    }

    private void ResetCombat()
    {
        _attackTokenUser?.Release();

        if (_pairedAnimationActor == null)
            return;

        _pairedAnimationActor.UnlockMovement();
        _pairedAnimationActor.SetRootMotionEnabled(true);
    }

    private void ResetTimers()
    {
        _flagCountdown?.ResetAllCountdown();
    }

    private void ResetNavigation()
    {
        _pathNavigator?.ClearTarget();
        _waypointPath?.ResetToFirstPoint();
    }

    private void ResetTransform(Transform resetPoint)
    {
        if (_teleporter == null)
        {
            Debug.LogWarning(
                "KaResetController requires CharacterTeleporter.",
                this);

            return;
        }

        Vector3 facePosition = resetPoint.position + resetPoint.forward;
        _teleporter.Teleport(resetPoint.position);
    }

    private void ResetHealthAndFlags()
    {
        _health?.RestoreFullHealth();
        _stateFlags?.ResetToInitialValues();
    }

    private void ResetAnimator()
    {
        if (_animator == null)
            return;

        _animator.Rebind();
        _animator.Update(0f);
    }

    private void ResetStateMachine()
    {
        if (_stateMachineController == null)
        {
            Debug.LogWarning(
                "KaResetController requires StateMachineController.",
                this);

            return;
        }

        _stateMachineController.RestartTable(0);
    }
}