using System.Collections;
using UnityEngine;

public class YantVanish : MonoBehaviour
{
    private string _originalTag;
    private Coroutine _revertRoutine;
    private bool _isVanished;
    private YantraInputObserverSO _inputObserver;

    public void Apply(string vanishTag, bool temporary, float duration, YantraInputObserverSO inputObserver)
    {
        if (_revertRoutine != null)
        {
            StopCoroutine(_revertRoutine);
            _revertRoutine = null;
        }

        if (_inputObserver != null)
            _inputObserver.OnMoveChannel -= OnMoveInput;

        if (!_isVanished)
            _originalTag = gameObject.tag;

        gameObject.tag = vanishTag;
        _isVanished = true;

        _inputObserver = inputObserver;
        if (_inputObserver != null)
            _inputObserver.OnMoveChannel += OnMoveInput;

        if (temporary)
            _revertRoutine = StartCoroutine(RevertAfter(duration));
    }

    public void Revert()
    {
        if (_revertRoutine != null)
        {
            StopCoroutine(_revertRoutine);
            _revertRoutine = null;
        }

        if (_isVanished)
        {
            gameObject.tag = _originalTag;
            _isVanished = false;
        }

        UnsubscribeObserver();
    }

    private void OnDisable()
    {
        UnsubscribeObserver();
    }

    private void OnMoveInput(Vector3 moveInput)
    {
        if (!_isVanished) return;
        if (moveInput.sqrMagnitude < 1e-6f) return;

        Revert();
    }

    private void UnsubscribeObserver()
    {
        if (_inputObserver != null)
        {
            _inputObserver.OnMoveChannel -= OnMoveInput;
            _inputObserver = null;
        }
    }

    private IEnumerator RevertAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        gameObject.tag = _originalTag;
        _isVanished = false;
        _revertRoutine = null;
        UnsubscribeObserver();
    }
}
