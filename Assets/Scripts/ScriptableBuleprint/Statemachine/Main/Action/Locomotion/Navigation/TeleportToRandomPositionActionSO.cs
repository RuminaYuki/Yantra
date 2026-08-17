using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "TeleportToRandomPositionAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Locomotion/Navigation/Teleport To Random Position")]
public class TeleportToRandomPositionActionSO : StateActionSO
{
    [SerializeField] private TransformAnchor _originAnchor;
    [SerializeField, Min(0.01f)] private float _radius = 8f;
    [SerializeField, Min(0.01f)] private float _minDistance = 4f;
    [SerializeField, Min(1)] private int _maxAttempts = 3;
    [SerializeField] private bool _faceOrigin = true;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new TeleportToRandomPositionAction(
            _originAnchor,
            _radius,
            _minDistance,
            _maxAttempts,
            _faceOrigin);
    }
}

public class TeleportToRandomPositionAction : StateAction
{
    private readonly TransformAnchor _originAnchor;
    private readonly float _radius;
    private readonly float _minDistance;
    private readonly int _maxAttempts;
    private readonly bool _faceOrigin;

    private CharacterTeleporter _teleporter;
    private RandomWalkPoint _randomWalkPoint;
    private GameObject _owner;

    public TeleportToRandomPositionAction(
        TransformAnchor originAnchor,
        float radius,
        float minDistance,
        int maxAttempts,
        bool faceOrigin)
    {
        _originAnchor = originAnchor;
        _radius = Mathf.Max(0.01f, radius);
        _minDistance = Mathf.Clamp(minDistance, 0.01f, _radius);
        _maxAttempts = Mathf.Max(1, maxAttempts);
        _faceOrigin = faceOrigin;
    }

    public override void Awake(StateMachine stateMachine)
    {
        _owner = stateMachine.Owner;
        _teleporter = stateMachine.GetComponent<CharacterTeleporter>();
        _randomWalkPoint = stateMachine.GetComponent<RandomWalkPoint>();

        if (_teleporter == null)
            Debug.LogError("TeleportToRandomPositionAction requires CharacterTeleporter.", _owner);

        if (_randomWalkPoint == null)
            Debug.LogError("TeleportToRandomPositionAction requires RandomWalkPoint.", _owner);
    }

    public override void OnStateEnter()
    {
        if (_teleporter == null || _randomWalkPoint == null)
            return;

        if (_originAnchor == null || !_originAnchor.IsSet)
        {
            Debug.LogWarning("TeleportToRandomPositionAction origin anchor is not set.", _owner);
            return;
        }

        Vector3 origin = _originAnchor.Value.position;

        for (int i = 0; i < _maxAttempts; i++)
        {
            if (!_randomWalkPoint.TryGetRandomPoint(
                    origin,
                    _radius,
                    _minDistance,
                    out Vector3 position))
                continue;

            if (_faceOrigin)
                _teleporter.TeleportAndFace(position, origin);
            else
                _teleporter.Teleport(position);

            return;
        }

        Debug.LogWarning(
            $"TeleportToRandomPositionAction could not find a valid position after {_maxAttempts} attempts.",
            _owner);
    }

    public override void OnUpdate() { }
}
