using UnityEngine;

public interface IPairedAnimationActor
{
    Transform GetTransform();

    void LockMovement();
    void PlayAnimation(PairedAnimationId animationId);
    void ExitAnimation(PairedAnimationId animationId);
    void UnlockMovement();

    bool IsInAnimation(PairedAnimationId animationId);
    bool IsAnimationFinished(PairedAnimationId animationId);

    bool CanPlay(PairedAnimationId animationId);
}
