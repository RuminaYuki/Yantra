using UnityEngine;
using UnityEngine.AI;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "SetRandomNavMeshPositionAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Path Navigator/Set Random NavMesh Position")]
public class SetRandomNavMeshPositionActionSO : StateActionSO
{
    [SerializeField] private TransformAnchor _targetAnchor;
    [SerializeField, Min(0f)] private float _minRange = 3f;
    [SerializeField, Min(0f)] private float _maxRange = 5f;
    [SerializeField, Min(0.01f)] private float _sampleRadius = 1f;
    [SerializeField, Min(1)] private int _maxAttempts = 10;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetRandomNavMeshPositionAction(
            _targetAnchor,
            _minRange,
            _maxRange,
            _sampleRadius,
            _maxAttempts);
    }
}

public class SetRandomNavMeshPositionAction : StateAction
{
    private readonly TransformAnchor _targetAnchor;
    private readonly float _minRange;
    private readonly float _maxRange;
    private readonly float _sampleRadius;
    private readonly int _maxAttempts;

    private readonly NavMeshPath _path = new();

    private PathNavigator _pathNavigator;
    private Transform _owner;
    private Transform _destination;

    public SetRandomNavMeshPositionAction(
        TransformAnchor targetAnchor,
        float minRange,
        float maxRange,
        float sampleRadius,
        int maxAttempts)
    {
        _targetAnchor = targetAnchor;
        _minRange = Mathf.Max(0f,minRange);
        _maxRange = Mathf.Max(_minRange,maxRange);
        _sampleRadius = Mathf.Max(0.01f,sampleRadius);
        _maxAttempts = Mathf.Max(1,maxAttempts);
    }

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner.transform;
        _pathNavigator = stateMachine.GetComponent<PathNavigator>();

        if (_pathNavigator == null)
            Debug.LogError("SetRandomNavMeshPositionAction requires PathNavigator.");
    }

    public override void OnStateEnter()
    {
        if (_pathNavigator == null)
            return;

        _pathNavigator.Target = null;

        if (!TryGetTarget(out Transform target))
            return;

        CreateDestination();

        for (int i = 0; i < _maxAttempts; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(_minRange,_maxRange);
            Vector3 candidate = target.position +
                new Vector3(randomDirection.x,0f,randomDirection.y) * randomDistance;

            if (!NavMesh.SamplePosition(candidate,out NavMeshHit hit,_sampleRadius,NavMesh.AllAreas))
                continue;

            Vector3 targetOffset = hit.position - target.position;
            targetOffset.y = 0f;
            float actualDistance = targetOffset.magnitude;

            if (actualDistance < _minRange || actualDistance > _maxRange)
                continue;

            if (!NavMesh.CalculatePath(_owner.position,hit.position,NavMesh.AllAreas,_path) ||
                _path.status != NavMeshPathStatus.PathComplete ||
                _path.corners.Length < 2)
            {
                continue;
            }

            _destination.position = hit.position;
            _pathNavigator.Target = _destination;
            return;
        }

        Debug.LogWarning(
            $"SetRandomNavMeshPositionAction could not find a reachable position after {_maxAttempts} attempts.",
            _owner);
    }

    public override void OnUpdate() { }

    public override void OnStateExit()
    {
        if (_pathNavigator != null && _pathNavigator.Target == _destination)
            _pathNavigator.Target = null;

        if (_destination != null)
            Object.Destroy(_destination.gameObject);

        _destination = null;
    }

    private void CreateDestination()
    {
        if (_destination != null)
            return;

        GameObject destinationObject = new("Random NavMesh Destination");
        destinationObject.hideFlags = HideFlags.HideInHierarchy;
        _destination = destinationObject.transform;
    }

    private bool TryGetTarget(out Transform target)
    {
        target = null;

        if (_targetAnchor == null || !_targetAnchor.IsSet)
            return false;

        target = _targetAnchor.Value;
        return target != null;
    }
}
