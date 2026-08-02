using UnityEngine;

public class SingleSnapPointSelector : MonoBehaviour, ISnapPointSelector
{
    [SerializeField] private PairedSnapPoint _snapPoint;

    public PairedSnapPoint Select(Vector3 attackerPosition)
    {
        if (_snapPoint == null || !_snapPoint.IsValid())
        {
            Debug.LogWarning("Single Snap Point is invalid.", this);
            return null;
        }

        return _snapPoint;
    }
}