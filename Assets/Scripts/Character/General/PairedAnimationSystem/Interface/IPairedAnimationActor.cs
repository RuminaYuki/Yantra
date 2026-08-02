using UnityEngine;

public interface IPairedAnimationActor
{
    Transform GetTransform();

    void LockMovement();

    void PlayAnimation(string animationName);

    void UnlockMovement();

    bool IsInAnimation(string animationName);
    bool IsAnimationFinished(string animationName);
}
