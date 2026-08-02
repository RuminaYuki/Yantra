using UnityEngine;

public class AttackSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PairedAnimationManager _pairedManager;
    [SerializeField] private PairedAnimationActor _attacker;
    [SerializeField] private PairedAnimationActor _target;

    [Header("Condition")]
    [SerializeField] private float _activationDistance = 0.5f;

    private ISnapPointSelector _snapPointSelector;

    private void Awake()
    {
        _snapPointSelector = GetComponent<ISnapPointSelector>();

        if (_snapPointSelector == null)
        {
            Debug.LogError(
                "Snap Point Selector must implement ISnapPointSelector.",
                this);
        }
    }
    
    //============================TEST===============================
    private bool _isPlayed;

    private void Update()
    {
        if (_isPlayed)
            return;

        if (!AttackDistance())
            return;

        TryAttack();
        _isPlayed = true;
    }
    //================================================================

    public void TryAttack()
    {
        if (_pairedManager == null || _attacker == null ||
            _target == null || _snapPointSelector == null)
        {
            Debug.LogWarning("Attack setup is missing.", this);
            return;
        }

        PairedSnapPoint snapPoint = 
        _snapPointSelector.Select(_attacker.transform.position);

        if (snapPoint == null)
            return;

        _pairedManager.TryStart(
            attacker: _attacker,
            victim: _target,
            attackerSnapPoint: snapPoint.AttackerPoint,
            victimSnapPoint: snapPoint.VictimPoint,
            animationId: snapPoint.AnimationId);
    }

    public bool AttackDistance()
    {
        if (_attacker == null || _target == null)
            return false;

        Vector3 offset =
            _target.transform.position -
            _attacker.transform.position;

        offset.y = 0f;

        return offset.sqrMagnitude <=
               _activationDistance * _activationDistance;
    }
}