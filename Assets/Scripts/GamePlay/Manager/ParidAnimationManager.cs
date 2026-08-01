using System.Collections;
using UnityEngine;

public class PairedAnimationManager : MonoBehaviour
{
    private bool _isPlaying;

    public bool TryStart(
        IPairedAnimationActor attacker,
        IPairedAnimationActor victim,
        Transform attackerSnapPoint,
        Transform victimSnapPoint,
        string attackerAnimation,
        string victimAnimation)
    {
        if (_isPlaying)
            return false;

        if (attacker == null || victim == null)
            return false;

        if (attackerSnapPoint == null ||
            victimSnapPoint == null)
        {
            Debug.LogWarning("Paired Animation: ไม่มี Snap Point");
            return false;
        }

        StartCoroutine(PlayRoutine(
            attacker,
            victim,
            attackerSnapPoint,
            victimSnapPoint,
            attackerAnimation,
            victimAnimation
        ));

        return true;
    }

    private IEnumerator PlayRoutine(
        IPairedAnimationActor attacker,
        IPairedAnimationActor victim,
        Transform attackerSnapPoint,
        Transform victimSnapPoint,
        string attackerAnimation,
        string victimAnimation)
    {
        _isPlaying = true;

        attacker.LockMovement();
        victim.LockMovement();

        SnapActor(attacker, attackerSnapPoint);
        SnapActor(victim, victimSnapPoint);

        // เรียกในเฟรมเดียวกัน
        attacker.PlayAnimation(attackerAnimation);
        victim.PlayAnimation(victimAnimation);

        // ใช้ทดสอบไปก่อน
        yield return new WaitForSeconds(1.5f);

        attacker.UnlockMovement();
        victim.UnlockMovement();

        _isPlaying = false;
    }

    private void SnapActor(
        IPairedAnimationActor actor,
        Transform snapPoint)
    {
        Transform actorTransform = actor.GetTransform();

        Vector3 position = snapPoint.position;

        // Snap เฉพาะพื้นราบ
        position.y = actorTransform.position.y;

        Quaternion rotation = Quaternion.Euler(
            0f,
            snapPoint.eulerAngles.y,
            0f
        );

        actorTransform.SetPositionAndRotation(
            position,
            rotation
        );
    }
}