using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewSetGunAimingAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/FatalFrameCamera/Set Gun Aiming")]
public class SetGunAimingActionSO : StateActionSO
{
    [SerializeField] private bool isAiming;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetGunAimingAction(isAiming);
    }
}

public class SetGunAimingAction : StateAction
{
    private readonly bool isAiming;
    private PlayerCameraController cameraController;

    public SetGunAimingAction(bool isAiming)
    {
        this.isAiming = isAiming;
    }

    public override void Awake(StateMachine stateMachine)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            cameraController = mainCamera.GetComponent<PlayerCameraController>();

        if (cameraController == null)
            cameraController = Object.FindFirstObjectByType<PlayerCameraController>();

        if (cameraController == null)
            Debug.LogError("SetGunAimingAction cannot find PlayerCameraController.");
    }

    public override void OnStateEnter()
    {
        if (cameraController == null) return;
        cameraController.IsGunAiming = isAiming; // เรียกผ่าน Property
    }

    public override void OnStateExit()
    {
        if (cameraController == null) return;
        cameraController.IsGunAiming = false; // เรียกผ่าน Property
    }

    public override void OnUpdate(){}
}
