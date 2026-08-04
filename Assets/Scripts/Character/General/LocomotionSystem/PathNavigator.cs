using UnityEngine;

public class PathNavigator : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _repathInterval = 0.25f;

    private IPathfinder _pathfinder;
    private float _timer;

    public Vector3 Direction { get; private set; }
    public Transform Target {get => _target; set => _target = value;}

    private void Awake()
    {
        _pathfinder = new UnityNavMeshPathfinder();
    }

    private void Update()
    {
        if (_target == null)
        {
            Direction = Vector3.zero;
            return;
        }

        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            _timer = _repathInterval;

            _pathfinder.CalculatePath(
                transform.position,
                _target.position
            );
        }

        Direction = _pathfinder.GetDirection(transform.position);
    }
}