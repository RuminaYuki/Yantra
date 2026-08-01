using UnityEngine;

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
    [SerializeField]
    private string _attackerAnimation = "PairedAttack";

    [SerializeField]
    private string _victimAnimation = "PairedHurt";

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
        _isPlayed = true;
    }

    private void TryAttack()
    {
        _pairedManager.TryStart(
            attacker: _attacker,
            victim: _victim,
            attackerSnapPoint: _attackerSnapPoint,
            victimSnapPoint: _victimSnapPoint,
            attackerAnimation: _attackerAnimation,
            victimAnimation: _victimAnimation
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
