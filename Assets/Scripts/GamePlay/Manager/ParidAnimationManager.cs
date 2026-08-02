using System.Collections;
using UnityEngine;

public class PairedAnimationManager : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float _warpDuration = 0.15f;
    private bool _isPlaying;

    public bool TryStart(
        IPairedAnimationActor attacker,
        IPairedAnimationActor victim,
        Transform attackerSnapPoint,
        Transform victimSnapPoint,
        PairedAnimationId animationId)
    {
        if (_isPlaying || attacker == null || victim == null)
            return false;

        if (animationId == null)
        {
            Debug.LogWarning("Paired Animation: Animation type is missing.");
            return false;
        }

        if (!attacker.CanPlay(animationId) ||
            !victim.CanPlay(animationId))
        {
            Debug.LogWarning("Paired Animation: Actor does not support this animation.");
            return false;
        }

        if (attackerSnapPoint == null || victimSnapPoint == null)
        {
            Debug.LogWarning("Paired Animation: Snap Point is missing.");
            return false;
        }

        StartCoroutine(PlayRoutine(
            attacker,
            victim,
            attackerSnapPoint,
            victimSnapPoint,
            animationId));

        return true;
    }

    private IEnumerator PlayRoutine(
        IPairedAnimationActor attacker,
        IPairedAnimationActor victim,
        Transform attackerSnapPoint,
        Transform victimSnapPoint,
        PairedAnimationId animationId)
    {
        _isPlaying = true;

        attacker.LockMovement();
        victim.LockMovement();

        yield return WarpActors(
            attacker,
            victim,
            attackerSnapPoint,
            victimSnapPoint);

        attacker.PlayAnimation(animationId);
        victim.PlayAnimation(animationId);

        yield return new WaitUntil(() =>
            attacker.IsInAnimation(animationId) &&
            victim.IsInAnimation(animationId));

        yield return new WaitUntil(() =>
            attacker.IsAnimationFinished(animationId) &&
            victim.IsAnimationFinished(animationId));

        attacker.UnlockMovement();
        victim.UnlockMovement();

        _isPlaying = false;
    }

    private IEnumerator WarpActors(
        IPairedAnimationActor attacker,
        IPairedAnimationActor victim,
        Transform attackerSnapPoint,
        Transform victimSnapPoint)
    {
        Transform attackerTransform = attacker.GetTransform();
        Transform victimTransform = victim.GetTransform();

        Vector3 attackerStartPosition = attackerTransform.position;
        Vector3 victimStartPosition = victimTransform.position;

        Quaternion attackerStartRotation = attackerTransform.rotation;
        Quaternion victimStartRotation = victimTransform.rotation;

        Vector3 attackerTargetPosition = attackerSnapPoint.position;
        Vector3 victimTargetPosition = victimSnapPoint.position;

        Quaternion attackerTargetRotation = attackerSnapPoint.rotation;
        Quaternion victimTargetRotation = victimSnapPoint.rotation;

        attackerTargetPosition.y = attackerStartPosition.y;
        victimTargetPosition.y = victimStartPosition.y;

        float elapsedTime = 0f;

        while (elapsedTime < _warpDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsedTime / _warpDuration));

            attackerTransform.SetPositionAndRotation(
                Vector3.Lerp(
                    attackerStartPosition,
                    attackerTargetPosition,
                    t),
                Quaternion.Slerp(
                    attackerStartRotation,
                    attackerTargetRotation,
                    t));

            victimTransform.SetPositionAndRotation(
                Vector3.Lerp(
                    victimStartPosition,
                    victimTargetPosition,
                    t),
                Quaternion.Slerp(
                    victimStartRotation,
                    victimTargetRotation,
                    t));

            yield return null;
        }

        attackerTransform.SetPositionAndRotation(
            attackerTargetPosition,
            attackerTargetRotation);

        victimTransform.SetPositionAndRotation(
            victimTargetPosition,
            victimTargetRotation);
    }
}
