using UnityEngine;
using UnityEngine.AI;

public class TaniStateMachine : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GhostAnimationController _animationController;
    [SerializeField] private GhostAudioManager _audioManager;
    [SerializeField] private Transform player;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float waypointReachedDistance = 0.35f;
    [SerializeField] private bool enablePatrolTurnAtWaypoint = true;
    [SerializeField] private float patrolTurnDuration = 1.2f;
    [SerializeField] private float patrolLookHoldDuration = 0.8f;
    [SerializeField] private float patrolLookBackAngle = 180f;

    [Header("Chase")]
    [SerializeField] private bool enableSight = true;
    [SerializeField] private float sightRange = 10f;
    [SerializeField, Range(1f, 360f)] private float sightAngle = 90f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float chaseTargetNavMeshSampleRadius = 3f;
    [SerializeField] private float chaseTargetMaxHorizontalNavMeshDistance = 0.35f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackWidth = 3f;
    [SerializeField] private float attackDepth = 3f;
    [SerializeField] private float attackRightOffset;
    [SerializeField] private float attackForwardOffset;
    [SerializeField] private float attackHeightOffset;
    [SerializeField] private float attackDuration = 1.2f;
    [SerializeField] private bool stopAgentDuringAttack = true;
    [SerializeField] private float attackDamage = 25f;

    [Header("Search")]
    [SerializeField] private float searchSpeed = 3f;
    [SerializeField] private float searchDuration = 1.5f;
    [SerializeField] private float searchSightRangeBonus = 4f;
    [SerializeField, Range(0f, 360f)] private float searchSightAngleBonus = 60f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private float waypointGizmoRadius = 0.4f;
    [SerializeField, Range(3, 32)] private int sightConeSegments = 12;
    [SerializeField, Range(8, 64)] private int attackAreaSegments = 24;

    private TaniState currentState;
    private Idle idleState;
    private Patrol patrolState;
    private Chase chaseState;
    private Attack attackState;
    private Search searchState;
    private Execute executeState;
    private bool warnedMissingAgent;
    private bool warnedMissingPlayer;
    private Vector3 lastSeenPlayerPosition;
    private bool hasLastSeenPlayerPosition;
    private YantraStatsController _playerStats;

    public NavMeshAgent Agent => agent;
    public Transform Player => player;
    public Transform[] PatrolWaypoints => patrolWaypoints;
    public float PatrolSpeed => patrolSpeed;
    public float WaypointReachedDistance => waypointReachedDistance;
    public bool EnablePatrolTurnAtWaypoint => enablePatrolTurnAtWaypoint;
    public float PatrolTurnDuration => patrolTurnDuration;
    public float PatrolLookHoldDuration => patrolLookHoldDuration;
    public float PatrolLookBackAngle => patrolLookBackAngle;
    public bool ShowDebugGizmos => showDebugGizmos;
    public float WaypointGizmoRadius => waypointGizmoRadius;
    public bool EnableSight => enableSight;
    public float SightRange => sightRange;
    public float SightAngle => sightAngle;
    public float ChaseSpeed => chaseSpeed;
    public float AttackRange => attackRange;
    public float AttackWidth => attackWidth;
    public float AttackDepth => attackDepth;
    public float AttackRightOffset => attackRightOffset;
    public float AttackForwardOffset => attackForwardOffset;
    public float AttackHeightOffset => attackHeightOffset;
    public float AttackDuration => attackDuration;
    public bool StopAgentDuringAttack => stopAgentDuringAttack;
    public float AttackDamage => attackDamage;
    public float SearchSpeed => searchSpeed;
    public float SearchDuration => searchDuration;
    public float SearchSightRangeBonus => searchSightRangeBonus;
    public float SearchSightAngleBonus => searchSightAngleBonus;
    public Vector3 LastSeenPlayerPosition => lastSeenPlayerPosition;
    public bool HasLastSeenPlayerPosition => hasLastSeenPlayerPosition;
    public Idle IdleState => idleState;
    public Patrol PatrolState => patrolState;
    public Chase ChaseState => chaseState;
    public Attack AttackState => attackState;
    public Search SearchState => searchState;
    public Execute ExecuteState => executeState;

    private void OnValidate()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (_animationController == null) _animationController = GetComponent<GhostAnimationController>();
        if (_audioManager == null) _audioManager = GetComponent<GhostAudioManager>();
    }

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (_animationController == null) _animationController = GetComponent<GhostAnimationController>();
        if (_audioManager == null) _audioManager = GetComponent<GhostAudioManager>();

        TryFindPlayer();
        idleState    = new Idle(this);
        patrolState  = new Patrol(this);
        chaseState   = new Chase(this);
        attackState  = new Attack(this);
        searchState  = new Search(this);
        executeState = new Execute(this);
    }

    private void Start()
    {
        ChangeState(patrolState);
    }

    private void Update()
    {
        bool hasPlayer = EnsurePlayerTarget() || TryFindPlayer();
        if (!hasPlayer && currentState != patrolState)
        {
            ClearPlayerTarget();
            ChangeState(patrolState);
        }

        currentState?.Tick();
    }

    public void ChangeState(TaniState nextState)
    {
        if (nextState == null)
        {
            Debug.LogWarning($"{nameof(TaniStateMachine)} on {name} tried to change to a missing state.");
            return;
        }

        currentState?.Exit();
        currentState = nextState;
        currentState.Enter();
    }

    public bool TrySetDestination(Vector3 destination)
    {
        if (!CanUseAgent()) return false;
        return agent.SetDestination(destination);
    }

    public bool TrySetReachableDestination(Vector3 destination)
    {
        if (!CanUseAgent()) return false;
        if (!TryGetReachableNavMeshPosition(destination, out Vector3 navMeshPosition)) return false;

        return agent.SetDestination(navMeshPosition);
    }

    public bool HasReachedDestination()
    {
        if (!CanUseAgent()) return false;
        if (agent.pathPending) return false;
        return agent.remainingDistance <= waypointReachedDistance;
    }

    public void StopCurrentAudio()
    {
        _audioManager?.StopCurrentStateAudio();
    }

    public void PlayIdleAnimation()
    {
        _animationController.Play(GhostState.Idle);
        _audioManager?.PlayAudioForState(GhostState.Idle);
    }

    public void PlayPatrolAnimation()
    {
        _animationController.Play(GhostState.Patrol);
        _audioManager?.PlayAudioForState(GhostState.Patrol);
    }

    public void PlayChaseAnimation()
    {
        _animationController.Play(GhostState.Chase);
        _audioManager?.PlayAudioForState(GhostState.Chase);
    }

    public void PlayAttackAnimation()
    {
        _animationController.Play(GhostState.Attack);
        _audioManager?.PlayAudioForState(GhostState.Attack);
    }

    public void PlaySearchAnimation()
    {
        _animationController.Play(GhostState.Search);
        _audioManager?.PlayAudioForState(GhostState.Search);
    }

    public void PlayExecuteAnimation()
    {
        _animationController.Play(GhostState.Execute);
        _audioManager?.PlayAudioForState(GhostState.Execute);
    }

    public void PlayHitAnimation()
    {
        _animationController.Play(GhostState.Hit);
        _audioManager?.PlayAudioForState(GhostState.Hit);
    }

    public bool CanSeePlayer()
    {
        return CanSeePlayer(0f, 0f);
    }

    public bool CanSeePlayer(float rangeBonus, float angleBonus)
    {
        if (!enableSight || !EnsurePlayerTarget()) return false;

        float checkRange = Mathf.Max(0f, sightRange + rangeBonus);
        float checkAngle = Mathf.Clamp(sightAngle + angleBonus, 1f, 360f);
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > checkRange * checkRange) return false;
        if (toPlayer.sqrMagnitude <= Mathf.Epsilon) return true;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= Mathf.Epsilon) return false;

        float angleToPlayer = Vector3.Angle(forward.normalized, toPlayer.normalized);
        return angleToPlayer <= checkAngle * 0.5f;
    }

    public void RecordLastSeenPlayerPosition()
    {
        if (!EnsurePlayerTarget()) return;
        lastSeenPlayerPosition = player.position;
        hasLastSeenPlayerPosition = true;
    }

    public void ChangeToUnreachablePlayerSearch()
    {
        hasLastSeenPlayerPosition = false;
        StopAgent();
        ChangeState(searchState);
    }

    public void ForceAttackFromHitbox(Transform target)
    {
        if (!SetPlayerTarget(target)) return;

        FacePlayer();
        RecordLastSeenPlayerPosition();
        ChangeState(attackState);
    }

    public void FacePlayer()
    {
        if (!EnsurePlayerTarget()) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon) return;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    public bool IsPlayerInAttackRange()
    {
        if (!EnsurePlayerTarget()) return false;

        Vector3 localPlayerPosition = transform.InverseTransformPoint(player.position);
        Vector3 attackCenter = GetAttackAreaLocalCenter();
        float halfWidth = GetAttackAreaHalfWidth();
        float halfDepth = GetAttackAreaHalfDepth();
        float normalizedX = (localPlayerPosition.x - attackCenter.x) / halfWidth;
        float normalizedZ = (localPlayerPosition.z - attackCenter.z) / halfDepth;

        return normalizedX * normalizedX + normalizedZ * normalizedZ <= 1f;
    }

    public void SetAgentSpeed(float speed)
    {
        if (agent == null) return;
        agent.speed = speed;
    }

    public void StopAgent()
    {
        if (agent == null || !agent.enabled) return;
        if (agent.isOnNavMesh) agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void DrawSightCone(Color color)
    {
        DrawSightCone(color, 0f, 0f);
    }

    public void DrawSightCone(Color color, float rangeBonus, float angleBonus)
    {
        if (!enableSight) return;

        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= Mathf.Epsilon) forward = Vector3.forward;
        forward.Normalize();

        int segments = Mathf.Max(3, sightConeSegments);
        float drawRange = Mathf.Max(0f, sightRange + rangeBonus);
        float drawAngle = Mathf.Clamp(sightAngle + angleBonus, 1f, 360f);
        float halfAngle = drawAngle * 0.5f;
        Vector3 previousPoint = origin + Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward * drawRange;

        Gizmos.color = color;
        Gizmos.DrawLine(origin, previousPoint);

        for (int i = 1; i <= segments; i++)
        {
            float lerp = i / (float)segments;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, lerp);
            Vector3 nextPoint = origin + Quaternion.AngleAxis(angle, Vector3.up) * forward * drawRange;
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }

        Gizmos.DrawLine(origin, previousPoint);
    }

    public void DrawAttackArea(Color color)
    {
        int segments = Mathf.Max(8, attackAreaSegments);
        Vector3 center = transform.TransformPoint(GetAttackAreaLocalCenter());
        float halfWidth = GetAttackAreaHalfWidth();
        float halfDepth = GetAttackAreaHalfDepth();
        Vector3 previousPoint = center + transform.right * halfWidth;

        Gizmos.color = color;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;
            Vector3 nextPoint = center
                + transform.right * Mathf.Cos(angle) * halfWidth
                + transform.forward * Mathf.Sin(angle) * halfDepth;

            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }

        Gizmos.DrawLine(transform.position, center);
    }

    private Vector3 GetAttackAreaLocalCenter()
    {
        return new Vector3(attackRightOffset, attackHeightOffset, attackForwardOffset);
    }

    private float GetAttackAreaHalfWidth()
    {
        return attackWidth > 0f ? attackWidth * 0.5f : Mathf.Max(0.01f, attackRange);
    }

    private float GetAttackAreaHalfDepth()
    {
        return attackDepth > 0f ? attackDepth * 0.5f : Mathf.Max(0.01f, attackRange);
    }

    private bool CanUseAgent()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh) return true;

        if (!warnedMissingAgent)
        {
            warnedMissingAgent = true;
            Debug.LogWarning($"{nameof(TaniStateMachine)} on {name} needs an enabled NavMeshAgent placed on a baked NavMesh.");
        }

        return false;
    }

    private bool TryGetReachableNavMeshPosition(Vector3 destination, out Vector3 navMeshPosition)
    {
        navMeshPosition = destination;

        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, chaseTargetNavMeshSampleRadius, agent.areaMask))
        {
            return false;
        }

        Vector2 horizontalOffset = new Vector2(hit.position.x - destination.x, hit.position.z - destination.z);
        float maxHorizontalDistance = Mathf.Max(0f, chaseTargetMaxHorizontalNavMeshDistance);
        if (horizontalOffset.sqrMagnitude > maxHorizontalDistance * maxHorizontalDistance)
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        navMeshPosition = hit.position;
        return true;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        patrolState ??= new Patrol(this);
        chaseState ??= new Chase(this);
        attackState ??= new Attack(this);
        searchState ??= new Search(this);

        TaniState debugState = currentState ?? patrolState;
        debugState.DrawGizmos();
    }

    public void DealDamageToPlayer()
    {
        if (!EnsurePlayerTarget()) return;

        _playerStats?.TakeDamage(attackDamage);
    }

    private bool TryFindPlayer()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag(PlayerTag);
        foreach (GameObject playerObject in playerObjects)
        {
            if (SetPlayerTarget(playerObject.transform))
            {
                return true;
            }
        }

        if (!warnedMissingPlayer)
        {
            warnedMissingPlayer = true;
            Debug.LogWarning($"{nameof(TaniStateMachine)} on {name} needs a Player transform or a GameObject tagged Player.");
        }

        ClearPlayerTarget();
        return false;
    }

    private bool SetPlayerTarget(Transform target)
    {
        Transform taggedPlayer = FindTaggedPlayerTransform(target);
        if (taggedPlayer == null)
        {
            ClearPlayerTarget();
            return false;
        }

        player = taggedPlayer;
        _playerStats = taggedPlayer.GetComponent<YantraStatsController>()
                       ?? taggedPlayer.GetComponentInParent<YantraStatsController>()
                       ?? taggedPlayer.GetComponentInChildren<YantraStatsController>();
        return true;
    }

    private bool EnsurePlayerTarget()
    {
        return SetPlayerTarget(player);
    }

    private static Transform FindTaggedPlayerTransform(Transform target)
    {
        if (target == null) return null;

        Transform root = target.root;
        return root != null && root.CompareTag(PlayerTag) ? root : null;
    }

    private void ClearPlayerTarget()
    {
        player = null;
        _playerStats = null;
        hasLastSeenPlayerPosition = false;
    }
}
