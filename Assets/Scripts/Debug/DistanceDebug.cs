using UnityEngine;
using System.Collections.Generic;

public class DistanceDebug : MonoBehaviour
{
    [SerializeField] private List<DistanceConditionSO> _conditions = new();
    public bool IsShowGizmos = true;

    private void OnDrawGizmos()
    {
        if (!IsShowGizmos || _conditions == null)
            return;

        Gizmos.color = Color.yellow;

        foreach (DistanceConditionSO condition in _conditions)
        {
            if (condition == null)
                continue;

            Gizmos.DrawWireSphere(
                transform.position,
                condition.Distance);
        }
    }
}
