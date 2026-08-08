using UnityEngine;

/// <summary>
/// ติดบน Vanish Yantra prefab — เพิ่ม YantVanish ให้ผู้เล่น (ถ้ายังไม่มี) แล้วสั่งให้หายตัว
/// </summary>
public class VanishYant : MonoBehaviour, IYantEffect
{
    [Header("Vanish Settings")]
    [SerializeField] private string _vanishTag = "Untagged";
    [SerializeField] private bool _temporary = true;
    [SerializeField] private float _duration = 5f;
    [SerializeField] private bool _cancelOnMove = true;
    [SerializeField] private YantraInputObserverSO _inputObserver;

    [Header("Flag")]
    [SerializeField] private FlagSO _vanishFlag;

    public bool Initialize(GameObject playerRoot)
    {
        if (playerRoot == null)
        {
            Debug.LogWarning("<color=#AA88FF>[VanishYant]</color> ไม่พบ playerRoot — หายตัวไม่ได้");
            Destroy(gameObject);
            return false;
        }

        if (!playerRoot.TryGetComponent(out YantVanish vanish))
            vanish = playerRoot.AddComponent<YantVanish>();

        YantraInputObserverSO observer = _cancelOnMove ? _inputObserver : null;
        vanish.Apply(_vanishTag, _temporary, _duration, observer, _vanishFlag);

        if (playerRoot.TryGetComponent(out StateFlags stateFlags) && _vanishFlag != null)
        {
            stateFlags.Set(_vanishFlag, true);
        }

        Debug.Log($"<color=#AA88FF>[VanishYant]</color> ผู้เล่นหายตัว (tag={_vanishTag}, {_duration}s)");

        return true;
    }
}
