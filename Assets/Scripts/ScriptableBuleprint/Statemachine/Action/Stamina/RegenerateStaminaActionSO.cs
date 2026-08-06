using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewRegenerateStaminaAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Stamina/Regenerate Stamina")]
public class RegenerateStaminaActionSO : StateActionSO
{
    [Tooltip("จำนวน Stamina ที่จะฟื้นฟู (ต่อวินาที)")]
    [SerializeField] private float regenRate = 15f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new RegenerateStaminaAction(regenRate);
    }
}

public class RegenerateStaminaAction : StateAction
{
    private readonly float regenRate;
    private StaminaSystem staminaSystem;

    public RegenerateStaminaAction(float regenRate)
    {
        this.regenRate = regenRate;
    }

    public override void Awake(StateMachine stateMachine)
    {
        staminaSystem = stateMachine.GetComponent<StaminaSystem>();

        if (staminaSystem == null)
            Debug.LogError("RegenerateStaminaAction ไม่พบ PlayerStaminaSystem บนตัวละคร!");
    }

    public override void OnStateEnter() { }
    public override void OnStateExit() { }

    public override void OnUpdate()
    {
        if (staminaSystem == null) return;

        // สั่งเพิ่ม Stamina แบบค่อยเป็นค่อยไป
        staminaSystem.RegenerateStamina(regenRate * Time.deltaTime);
    }
}