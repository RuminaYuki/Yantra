using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private FatalFrameCameraController cameraController;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private float maxYawAngle = 60f;
    [SerializeField] private float maxPitchUp = 60f;
    [SerializeField] private float maxPitchDown = 40f;
    [SerializeField] private float smoothSpeed = 10f;

    private Animator animator;
    private float currentYaw;
    private float currentPitch;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (playerRoot == null) playerRoot = transform;
    }

    private void Update()
    {
        if (animator == null || cameraController == null) return;

        Vector2 cameraRotation = cameraController.CameraRotation;

        float relativeYaw = Mathf.DeltaAngle(playerRoot.eulerAngles.y, cameraRotation.x);
        float targetYaw = Mathf.Clamp(relativeYaw / maxYawAngle, -1f, 1f) * 2f;

        float pitchLimit = cameraRotation.y >= 0f ? maxPitchUp : maxPitchDown;
        float targetPitch = Mathf.Clamp(cameraRotation.y / pitchLimit, -1f, 1f) * 2f;

        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * smoothSpeed);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * smoothSpeed);

        animator.SetFloat("MouseX", currentYaw);
        animator.SetFloat("MouseY", -currentPitch);
    }
}
