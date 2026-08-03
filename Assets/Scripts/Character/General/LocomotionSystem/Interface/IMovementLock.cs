using UnityEngine;

public interface IMovementLock
{
    bool IsMovementLocked { get; }
    bool ShouldResetMoveAnimation { get; }

    void LockMovement(object owner, bool resetMoveAnimation = true);
    void UnlockMovement(object owner);
}
