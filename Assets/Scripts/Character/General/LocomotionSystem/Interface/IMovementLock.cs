using UnityEngine;

public interface IMovementLock
{
    bool IsMovementLocked { get; }

    void LockMovement(object owner);
    void UnlockMovement(object owner);
}
