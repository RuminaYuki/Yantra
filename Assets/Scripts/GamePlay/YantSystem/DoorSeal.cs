using System.Collections;
using UnityEngine;

public class DoorSeal : MonoBehaviour
{
    public bool _isSeal { get; private set; }
    [SerializeField] private float _sealDuration = 5f;

    private Coroutine coroutine;

    public void SetSeal()
    {
        _isSeal = true;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(SealEffectDuration());     
    }

    IEnumerator SealEffectDuration()
    {
        yield return new WaitForSeconds(_sealDuration);
        _isSeal = false;
    }
}
