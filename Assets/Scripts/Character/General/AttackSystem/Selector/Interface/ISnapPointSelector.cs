using UnityEngine;

public interface ISnapPointSelector
{
    PairedSnapPoint Select(Vector3 attackerPosition);
}