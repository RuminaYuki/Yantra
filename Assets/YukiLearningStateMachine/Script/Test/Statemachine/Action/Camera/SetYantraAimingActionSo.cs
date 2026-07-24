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
    private FatalFrameCameraController cameraController;

    public SetYantraAimingAction(bool isAiming)
    {
        this.isAiming = isAiming;
    }

    public override void Awake(StateMachine stateMachine)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            cameraController = mainCamera.GetComponent<FatalFrameCameraController>();

        if (cameraController == null)
            cameraController = Object.FindFirstObjectByType<FatalFrameCameraController>();

        if (cameraController == null)
            Debug.LogError("SetYantraAimingAction cannot find FatalFrameCameraController.");
    }

    public override void OnStateEnter()
    {
        if (cameraController == null) return;
        cameraController.IsYantraAiming = isAiming;
    }
    public override void OnUpdate(){}
}