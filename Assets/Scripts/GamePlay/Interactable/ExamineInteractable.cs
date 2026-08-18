using System.Collections;
using UnityEngine;

public class ExamineInteractable : InteractableBase
{
    [Header("Read Interactable")]
    [SerializeField] private Transform cameraPoint;

    [Header("Transition Settings")]
    [SerializeField] private bool transitoinSmooth = true;
    [Tooltip("How long the camera takes to reach the target pose (seconds).")]
    [SerializeField] private float transitionDuration = 0.6f;

    private PlayerCameraController _cameraController;
    private Transform _cameraTransform;
    private ILocomotionLock _locomotionLock;
    private Vector3 _savedPosition;
    private Quaternion _savedRotation;
    private Coroutine _transitionRoutine;
    private bool _isReading;

    public bool IsReading => _isReading;

    public override bool Interact(GameObject rootplayer)
    {
        if(!base.Interact(rootplayer)) return false;

        if (cameraPoint == null)
        {
            Debug.LogWarning($"{nameof(ExamineInteractable)}: cameraPoint is not assigned on {gameObject.name}.");
            return false;
        }

        if (_isReading)
            return false;

        if (cameraPoint == null)
        {
            Debug.LogWarning($"{nameof(ExamineInteractable)}: cameraPoint is not assigned on {gameObject.name}.");
            return false;
        }

        if (!ResolveCameraController(rootplayer))
            return false;

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
        return true;
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
        _cameraController = rootplayer.GetComponentInChildren<PlayerCameraController>();

        if (_cameraController == null)
        {
            Camera camera = rootplayer.GetComponentInChildren<Camera>();
            if (camera != null)
                _cameraController = camera.GetComponent<PlayerCameraController>();
        }

        if (_cameraController == null && Camera.main != null)
            _cameraController = Camera.main.GetComponent<PlayerCameraController>();

        if (_cameraController == null)
        {
            Debug.LogWarning($"{nameof(ExamineInteractable)}: PlayerCameraController not found on player.");
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
        yield return TransitionPose(
            _cameraTransform.position,
            _cameraTransform.rotation,
            cameraPoint.position,
            cameraPoint.rotation);

        SnapToCameraPoint();
        _transitionRoutine = null;
    }

    private IEnumerator TransitionFromReadPose()
    {
        yield return TransitionPose(
            _cameraTransform.position,
            _cameraTransform.rotation,
            _savedPosition,
            _savedRotation);

        FinishReading();
    }

    private IEnumerator TransitionPose(
        Vector3 startPosition,
        Quaternion startRotation,
        Vector3 endPosition,
        Quaternion endRotation)
    {
        float duration = Mathf.Max(0.01f, transitionDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            _cameraTransform.position = Vector3.Lerp(startPosition, endPosition, t);
            _cameraTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        _cameraTransform.SetPositionAndRotation(endPosition, endRotation);
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

    public override bool CancelInteraction(GameObject rootplayer)
    {
        EndReading();
        return base.CancelInteraction(rootplayer);
    }
}
