using UnityEngine;

public class GhostAnimationController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private static readonly int HashIdle    = Animator.StringToHash("IsIdle");
    private static readonly int HashPatrol  = Animator.StringToHash("IsPatrolling");
    private static readonly int HashChase   = Animator.StringToHash("IsChasing");
    private static readonly int HashAttack  = Animator.StringToHash("IsAttacking");
    private static readonly int HashSearch  = Animator.StringToHash("IsSearching");
    private static readonly int HashExecute = Animator.StringToHash("IsExecuting");
    private static readonly int HashHit     = Animator.StringToHash("IsHit");

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    private void OnValidate()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    public void Play(GhostState state)
    {
        if (_animator == null) return;

        SetAllFalse();

        int hash = state switch
        {
            GhostState.Idle    => HashIdle,
            GhostState.Patrol  => HashPatrol,
            GhostState.Chase   => HashChase,
            GhostState.Attack  => HashAttack,
            GhostState.Search  => HashSearch,
            GhostState.Execute => HashExecute,
            GhostState.Hit     => HashHit,
            _ => -1
        };

        if (hash != -1)
            _animator.SetBool(hash, true);
    }

    private void SetAllFalse()
    {
        _animator.SetBool(HashIdle,    false);
        _animator.SetBool(HashPatrol,  false);
        _animator.SetBool(HashChase,   false);
        _animator.SetBool(HashAttack,  false);
        _animator.SetBool(HashSearch,  false);
        _animator.SetBool(HashExecute, false);
        _animator.SetBool(HashHit,     false);
    }
}
