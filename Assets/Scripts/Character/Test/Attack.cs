using UnityEngine;
using UnityEngine.Serialization;

public class Attack : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PairedAnimationManager _pairedManager;

    [SerializeField]
    private PairedAnimationActor _attacker;

    [SerializeField]
    private PairedAnimationActor _victim;

    [Header("Snap Points")]
    [SerializeField]
    private Transform _attackerSnapPoint;

    [SerializeField]
    private Transform _victimSnapPoint;

    [Header("Animations")]
    [FormerlySerializedAs("_animationType")]
    [SerializeField] private PairedAnimationId _animationId;

    [Header("Condition")]
    [SerializeField]
    private float _activationDistance = 0.5f;

    private bool _isPlayed;

    private void Update()
    {
        if (_isPlayed)
        return;

        if (!CheckDistance())
            return;

        TryAttack();
    }

    private void TryAttack()
    {
        if (_pairedManager == null)
        {
            Debug.LogWarning("PairedAnimationManager is missing.", this);
            return;
        }

        _isPlayed = _pairedManager.TryStart(
            attacker: _attacker,
            victim: _victim,
            attackerSnapPoint: _attackerSnapPoint,
            victimSnapPoint: _victimSnapPoint,
            animationId: _animationId
        );
    }

    private bool CheckDistance()
    {
        if (_attacker == null || _victim == null)
            return false;

        Vector3 offset =
            _victim.transform.position -
            _attacker.transform.position;

        offset.y = 0f;

        return offset.sqrMagnitude <=
               _activationDistance * _activationDistance;
    }
}
