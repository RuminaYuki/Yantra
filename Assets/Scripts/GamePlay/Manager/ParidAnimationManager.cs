using System.Collections;
using UnityEngine;
using System;

public class PairedAnimationManager : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float _warpDuration = 0.15f;
    public event Action<IPairedAnimationActor,IPairedAnimationActor,PairedAnimationId> PairedAnimationFinished;
    private bool _isPlaying;

    public bool TryStart(
        IPairedAnimationActor attacker,
        IPairedAnimationActor victim,
        Transform attackerSnapPoint,
        Transform victimSnapPoint,
        PairedAnimationId animationId)
    {
        if (attackerSnapPoint == null || victimSnapPoint == null)
        {
            Debug.LogWarning("Paired Animation: Snap Point is missing.");
            return false;
        }

        Pose attackerPose = new Pose(
            attackerSnapPoint.position,
            attackerSnapPoint.rotation);

        Pose victimPose = new Pose(
            victimSnapPoint.position,
            victimSnapPoint.rotation);

        return TryStart(
            attacker,
            victim,
            attackerPose,
            victimPose,
            animationId);
    }

    public bool TryStart(
        IPairedAnimationActor attacker,
        IPairedAnimationActor victim,
        Pose attackerSnapPose,
        Pose victimSnapPose,
        PairedAnimationId animationId)
    {
        if (_isPlaying || attacker == null || victim == null)
            return false;

        if (animationId == null)
        {
            Debug.LogWarning("Paired Animation ID is missing.");
            return false;
        }

        if (!attacker.CanPlay(animationId) ||
            !victim.CanPlay(animationId))
        {
            Debug.LogWarning(
                "Actor does not support this paired animation.");
            return false;
        }

        StartCoroutine(PlayRoutine(
            attacker,
            victim,
            attackerSnapPose,
            victimSnapPose,
            animationId));

        return true;
    }

    private IEnumerator PlayRoutine(
        IPairedAnimationActor attacker,
        IPairedAnimationActor victim,
        Pose attackerSnapPose,
        Pose victimSnapPose,
        PairedAnimationId animationId)
    {
        _isPlaying = true;

        attacker.LockMovement();
        victim.LockMovement();

        yield return WarpActors(
            attacker,
            victim,
            attackerSnapPose,
            victimSnapPose);

        attacker.PlayAnimation(animationId);
        victim.PlayAnimation(animationId);

        yield return new WaitUntil(() =>
            attacker.IsInAnimation(animationId) &&
            victim.IsInAnimation(animationId));

        yield return new WaitUntil(() =>
            attacker.IsAnimationFinished(animationId) &&
            victim.IsAnimationFinished(animationId));
        
        PairedAnimationFinished?.Invoke(
        attacker,
        victim,
        animationId);

        attacker.ExitAnimation(animationId);
        victim.ExitAnimation(animationId);

        attacker.UnlockMovement();
        victim.UnlockMovement();

        _isPlaying = false;
    }

    private IEnumerator WarpActors(
        IPairedAnimationActor attacker,
        IPairedAnimationActor victim,
        Pose attackerSnapPose,
        Pose victimSnapPose)
    {
        Transform attackerTransform = attacker.GetTransform();
        Transform victimTransform = victim.GetTransform();

        Vector3 attackerStartPosition = attackerTransform.position;
        Vector3 victimStartPosition = victimTransform.position;

        Quaternion attackerStartRotation = attackerTransform.rotation;
        Quaternion victimStartRotation = victimTransform.rotation;

        Vector3 attackerTargetPosition = attackerSnapPose.position;
        Vector3 victimTargetPosition = victimSnapPose.position;

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
                    attackerSnapPose.rotation,
                    t));

            victimTransform.SetPositionAndRotation(
                Vector3.Lerp(
                    victimStartPosition,
                    victimTargetPosition,
                    t),
                Quaternion.Slerp(
                    victimStartRotation,
                    victimSnapPose.rotation,
                    t));

            yield return null;
        }

        attackerTransform.SetPositionAndRotation(
            attackerTargetPosition,
            attackerSnapPose.rotation);

        victimTransform.SetPositionAndRotation(
            victimTargetPosition,
            victimSnapPose.rotation);
    }
}
