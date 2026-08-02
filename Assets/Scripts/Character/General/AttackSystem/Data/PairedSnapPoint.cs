using UnityEngine;

[System.Serializable]
public class PairedSnapPoint
{
    [SerializeField] private PairedAnimationId _animationId;
    [SerializeField] private Transform _attackerPoint;
    [SerializeField] private Transform _victimPoint;

    public PairedAnimationId AnimationId => _animationId;
    public Transform AttackerPoint => _attackerPoint;
    public Transform VictimPoint => _victimPoint;

    public bool IsValid()
    {
        return _animationId != null &&
               _attackerPoint != null &&
               _victimPoint != null;
    }
}