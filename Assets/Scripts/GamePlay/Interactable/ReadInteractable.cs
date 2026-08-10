using System.Collections;
using UnityEngine;

public class ReadInteractable : InteractableBase
{
    [Header("Read Interactable")]
    [SerializeField] private Transform cameraPoint;

    [Header("Transition Settings")]
    [SerializeField] private bool transitoinSmooth = true;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private float arrivalThreshold = 0.05f;

    private FatalFrameCameraController _cameraController;
    private Transform _cameraTransform;
    private ILocomotionLock _locomotionLock;
    private Vector3 _savedPosition;
    private Quaternion _savedRotation;
    private Coroutine _transitionRoutine;
    private bool _isReading;

    public bool IsReading => _isReading;

    public override void Interact(GameObject rootplayer)
    {
        base.Interact(rootplayer);

        if (cameraPoint == null)
        {
            Debug.LogWarning($"{nameof(ReadInteractable)}: cameraPoint is not assigned on {gameObject.name}.");
            return;
        }

        if (_isReading)
            return;

        if (cameraPoint == null)
        {
            Debug.LogWarning($"{nameof(ReadInteractable)}: cameraPoint is not assigned on {gameObject.name}.");
            return;
        }

        if (!ResolveCameraController(rootplayer))
            return;

        _savedPosition = _cameraTransform.position;
        _savedRotation = _cameraTransform.rotation;

        _locomotionLock = rootplayer.GetComponentInChildren<ILocomotionLock>();
        if (_locomotionLock != null)
            _locomotionLock.LockLocomotion(this);

        _cameraController.IsCutsceneMode = true;
        _isReading = true;

        if (transitoinSmooth)
            _transitionRoutine = StartCoroutine(TransitionToReadPose());
        else
            SnapToCameraPoint();
    }

    /// เรียกเมื่อออกจากโหมดอ่าน (จาก UI หรือ input อื่น)
    public void EndReading()
    {
        if (!_isReading)
            return;

        StopTransitionRoutine();

        if (transitoinSmooth)
            _transitionRoutine = StartCoroutine(TransitionFromReadPose());
        else
            FinishReading();
    }

    private bool ResolveCameraController(GameObject rootplayer)
    {
        _cameraController = rootplayer.GetComponentInChildren<FatalFrameCameraController>();

        if (_cameraController == null)
        {
            Camera camera = rootplayer.GetComponentInChildren<Camera>();
            if (camera != null)
                _cameraController = camera.GetComponent<FatalFrameCameraController>();
        }

        if (_cameraController == null && Camera.main != null)
            _cameraController = Camera.main.GetComponent<FatalFrameCameraController>();

        if (_cameraController == null)
        {
            Debug.LogWarning($"{nameof(ReadInteractable)}: FatalFrameCameraController not found on player.");
            return false;
        }

        _cameraTransform = _cameraController.transform;
        return true;
    }

    private void SnapToCameraPoint()
    {
        _cameraTransform.SetPositionAndRotation(cameraPoint.position, cameraPoint.rotation);
    }

    private IEnumerator TransitionToReadPose()
    {
        while (!HasReached(cameraPoint.position, cameraPoint.rotation))
        {
            float step = Time.deltaTime * transitionSpeed;
            _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, cameraPoint.position, step);
            _cameraTransform.rotation = Quaternion.Slerp(_cameraTransform.rotation, cameraPoint.rotation, step);
            yield return null;
        }

        SnapToCameraPoint();
        _transitionRoutine = null;
    }

    private IEnumerator TransitionFromReadPose()
    {
        while (!HasReached(_savedPosition, _savedRotation))
        {
            float step = Time.deltaTime * transitionSpeed;
            _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, _savedPosition, step);
            _cameraTransform.rotation = Quaternion.Slerp(_cameraTransform.rotation, _savedRotation, step);
            yield return null;
        }

        FinishReading();
    }

    private void FinishReading()
    {
        _cameraTransform.SetPositionAndRotation(_savedPosition, _savedRotation);
        _cameraController.IsCutsceneMode = false;

        if (_locomotionLock != null)
        {
            _locomotionLock.UnlockLocomotion(this);
            _locomotionLock = null;
        }

        _isReading = false;
        _transitionRoutine = null;
    }

    private bool HasReached(Vector3 targetPosition, Quaternion targetRotation)
    {
        return Vector3.Distance(_cameraTransform.position, targetPosition) < arrivalThreshold
            && Quaternion.Angle(_cameraTransform.rotation, targetRotation) < 1f;
    }

    private void StopTransitionRoutine()
    {
        if (_transitionRoutine != null)
        {
            StopCoroutine(_transitionRoutine);
            _transitionRoutine = null;
        }
    }

    private void OnDisable()
    {
        if (!_isReading)
            return;

        StopTransitionRoutine();

        if (_cameraController != null)
            _cameraController.IsCutsceneMode = false;

        if (_locomotionLock != null)
        {
            _locomotionLock.UnlockLocomotion(this);
            _locomotionLock = null;
        }

        _isReading = false;
    }
}
