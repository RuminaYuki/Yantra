using UnityEngine;

public class SealYant : MonoBehaviour, IYantEffect
{
    [Header("Aim")]
    [SerializeField] private Transform _aimCamera;
    [SerializeField] private float _maxAimDistance = 100f;
    [SerializeField] private LayerMask _aimMask = ~0;

    private void OnValidate()
    {
        if (_aimCamera == null) _aimCamera = Camera.main?.transform;
    }

    public bool Initialize(GameObject playerRoot, YantraStatsController stats)
    {
        RaycastHit hit;
        GetAimDirection(out hit);
        if (hit.collider != null) //????????? script ????????? object ?????????
        {

        }
        return true;
    }
    private Vector3 GetAimDirection(out RaycastHit hit)
    {
        Ray ray = new Ray(_aimCamera.position, _aimCamera.forward);
        if (Physics.Raycast(ray, out hit, _maxAimDistance, _aimMask, QueryTriggerInteraction.Collide))
        {
            return (hit.point - this.transform.position).normalized;
        }
        else
        {
            hit = default(RaycastHit);
            return _aimCamera.forward;
        }
    }
}
