using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewConsumeStaminaAction",
    menuName = "YUKI Learning State Machine/StateMachine/Actions/Stamina/Consume Stamina")]
public class ConsumeStaminaActionSO : StateActionSO
{
    [Tooltip("จำนวน Stamina ที่จะลดลง (ต่อวินาที)")]
    [SerializeField] private float consumeRate = 10f;

    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new ConsumeStaminaAction(consumeRate);
    }
}

public class ConsumeStaminaAction : StateAction
{
    private readonly float consumeRate;
    private StaminaSystem staminaSystem;

    public ConsumeStaminaAction(float consumeRate)
    {
        this.consumeRate = consumeRate;
    }

    public override void Awake(StateMachine stateMachine)
    {
        // ให้ State Machine ไปควานหาตู้เซฟ Stamina บนตัวผู้เล่น
        staminaSystem = stateMachine.GetComponent<StaminaSystem>();

        if (staminaSystem == null)
            Debug.LogError("ConsumeStaminaAction ไม่พบ PlayerStaminaSystem บนตัวละคร!");
    }

    public override void OnStateEnter() { }
    public override void OnStateExit() { }

    public override void OnUpdate()
    {
        if (staminaSystem == null) return;

        // สั่งหัก Stamina แบบค่อยเป็นค่อยไปตามเฟรมเรต (Time.deltaTime)
        bool hasStamina = staminaSystem.TryConsumeStamina(consumeRate * Time.deltaTime);

        // (ในอนาคต ถ้า hasStamina == false เราสามารถสั่งบังคับให้หลุดจาก State วิ่งได้ที่นี่ครับ)
    }
}