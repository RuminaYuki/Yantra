using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewSetYantraAimingAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/FatalFrameCamera/Set Yantra Aiming")]
public class SetYantraAimingActionSO : StateActionSO
{
    [SerializeField] private bool isAiming;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new SetYantraAimingAction(isAiming);
    }
}

public class SetYantraAimingAction : StateAction
{
    private readonly bool isAiming;
    private PlayerCameraController cameraController;

    public SetYantraAimingAction(bool isAiming)
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
            Debug.LogError("SetYantraAimingAction cannot find PlayerCameraController.");
    }

    public override void OnStateEnter()
    {
        if (cameraController == null) return;
        cameraController.IsYantraAiming = isAiming; // เรียกผ่าน Property
    }

    public override void OnStateExit()
    {
        if (cameraController == null) return;
        cameraController.IsYantraAiming = false; // เรียกผ่าน Property
    }

    public override void OnUpdate(){}
}
