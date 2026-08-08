using UnityEngine;

public abstract class BasePairedAttack : MonoBehaviour, IPairedAttackStrategy
{
    public bool TryAttack(
        PairedAnimationManager manager,
        PairedAnimationActor attacker,
        PairedAnimationActor victim)
    {
        if (manager == null || attacker == null || victim == null)
            return false;

        PairedSnapData snapData = SelectSnapData(attacker, victim);

        if (snapData == null || !snapData.IsValid())
        {
            Debug.LogWarning("Cannot find valid Paired Snap Data.", this);
            return false;
        }

        Transform anchor = GetAnchor(snapData, attacker, victim);

        Pose attackerPose = CreatePose(anchor, snapData.AttackerPoint);
        Pose victimPose = CreatePose(anchor, snapData.VictimPoint);

        return manager.TryStart(
            attacker,
            victim,
            attackerPose,
            victimPose,
            snapData.AnimationId);
    }

    protected abstract PairedSnapData SelectSnapData(
        PairedAnimationActor attacker,
        PairedAnimationActor victim);

    protected Transform GetAnchor(
        PairedSnapData snapData,
        PairedAnimationActor attacker,
        PairedAnimationActor victim)
    {
        return snapData.Anchor == PairedSnapAnchor.Attacker
            ? attacker.transform
            : victim.transform;
    }

    private Pose CreatePose(Transform anchor, Transform snapPoint)
    {
        return new Pose(
            anchor.TransformPoint(snapPoint.localPosition),
            anchor.rotation * snapPoint.localRotation);
    }
}
