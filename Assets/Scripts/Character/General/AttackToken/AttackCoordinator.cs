using UnityEngine;
public class AttackCoordinator : MonoBehaviour
{
    private GameObject currentAttacker;

    public bool TryClaim(GameObject requester)
    {
        if (requester == null)
            return false;

        if (currentAttacker == null)
            currentAttacker = requester;

        return currentAttacker == requester;
    }

    public bool IsOwner(GameObject requester)
    {
        return requester != null &&
               currentAttacker == requester;
    }

    public void Release(GameObject requester)
    {
        if (requester != null &&
            currentAttacker == requester)
        {
            currentAttacker = null;
        }
    }
}