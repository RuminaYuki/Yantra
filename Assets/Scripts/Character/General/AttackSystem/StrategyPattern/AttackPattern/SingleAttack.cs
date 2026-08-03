using UnityEngine;

public class SingleAttack : BasePairedAttack
{
    [SerializeField] private PairedSnapData _snapData;

    protected override PairedSnapData SelectSnapData(
        PairedAnimationActor attacker,
        PairedAnimationActor victim)
    {
        return _snapData;
    }
}
