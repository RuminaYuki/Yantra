using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class YantVanish : MonoBehaviour
{
    private string _originalTag;
    private Coroutine _revertRoutine;
    private bool _isVanished;
    private bool _hasMovementInput;
    private YantraInputObserverSO _inputObserver;
    [SerializeField] private StateFlagsAccess _stateFlags;

    [SerializeField] private StatSO _statSO;
    [SerializeField] private FlagSO _flagSO;
    [SerializeField] private FlagSO _isCrouchFlag;

    public void Apply(string vanishTag, bool temporary, float duration, YantraInputObserverSO inputObserver)
    {
        if (_revertRoutine != null)
        {
            StopCoroutine(_revertRoutine);
            _revertRoutine = null;
        }

        if (_inputObserver != null)
        {
            _inputObserver.OnMoveChannel -= OnMoveInput;
            _inputObserver.OnCrouchChannel -= OnCrouchInput;
        }

        if (!_isVanished)
            _originalTag = gameObject.tag;

        // ค้นหา StateFlags หากยังไม่ได้เก็บไว้
        if (_stateFlags == null)
            _stateFlags = GetComponent<StateFlagsAccess>();

        gameObject.tag = vanishTag;
        _isVanished = true;
        _hasMovementInput = false;

        _inputObserver = inputObserver;
        if (_inputObserver != null)
        {
            _inputObserver.OnMoveChannel += OnMoveInput;
            _inputObserver.OnCrouchChannel += OnCrouchInput;
        }

        if (temporary)
            _revertRoutine = StartCoroutine(RevertAfter(duration));

        if (_statSO != null)
        {
            StatCountdown countdown = GetComponent<StatCountdown>();

            if (countdown != null)
            {
                countdown.SetStatCountdown(_statSO, duration, true);
            }
        }

        if (_flagSO != null)
        {
            FlagCountdown flagdown = GetComponent<FlagCountdown>();

            if (flagdown != null)
                flagdown.SetFlagCountdown(_flagSO, duration, true);
        }
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

            if (_statSO != null)
            {
                StatCountdown countdown = GetComponent<StatCountdown>();

                if (countdown != null)
                {
                    countdown.StopCountdown(_statSO);
                }
            }

            if (_flagSO != null)
            {
                FlagCountdown flagdown = GetComponent<FlagCountdown>();

                if (flagdown != null)
                    flagdown.StopCountdown(_flagSO);
            }
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

        _hasMovementInput = moveInput.sqrMagnitude > 1e-6f;
        if (!_hasMovementInput) return;

        // ยังคงย่อเดินได้ถ้ากำลัง crouch อยู่
        if (_stateFlags != null && _isCrouchFlag != null && _stateFlags.Get(_isCrouchFlag))
            return;

        Debug.Log("here");

        Revert();
    }

    private void OnCrouchInput(bool isCrouching)
    {
        if (_isVanished && !isCrouching && _hasMovementInput)
        {
            Revert();
        }
    }

    private void UnsubscribeObserver()
    {
        if (_inputObserver != null)
        {
            _inputObserver.OnMoveChannel -= OnMoveInput;
            _inputObserver.OnCrouchChannel -= OnCrouchInput;
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
