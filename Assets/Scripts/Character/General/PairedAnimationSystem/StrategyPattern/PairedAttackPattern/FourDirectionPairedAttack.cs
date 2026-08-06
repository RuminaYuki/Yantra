using UnityEngine;

public class FourDirectionPairedAttack : BasePairedAttack
{
    [SerializeField] private PairedSnapData[] _snapData;

    protected override PairedSnapData SelectSnapData(
        PairedAnimationActor attacker,
        PairedAnimationActor victim)
    {
        if (_snapData == null || _snapData.Length == 0)
            return null;

        PairedSnapData closest = null;
        float closestDistance = float.MaxValue;

        foreach (PairedSnapData snapData in _snapData)
        {
            if (snapData == null || !snapData.IsValid())
                continue;

            Transform anchor = GetAnchor(
                snapData,
                attacker,
                victim);

            Vector3 snapPosition =
                anchor.TransformPoint(
                    snapData.AttackerPoint.localPosition);

            Vector3 offset =
                snapPosition - attacker.transform.position;

            offset.y = 0f;
            float distance = offset.sqrMagnitude;

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closest = snapData;
        }

        return closest;
    }
}
