using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewHasStaminaCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Stamina/Has Stamina")]
public class HasStaminaConditionSO : StateConditionSO
{
    public override Condition CreateCondition()
    {
        return new HasStaminaCondition();
    }
}

public class HasStaminaCondition : Condition
{
    private StaminaSystem staminaSystem;

    public override void Awake(StateMachine stateMachine)
    {
        // ควานหาตู้เซฟ Stamina บนตัวผู้เล่น
        staminaSystem = stateMachine.GetComponent<StaminaSystem>();

        if (staminaSystem == null)
            Debug.LogError("HasStaminaCondition ไม่พบ PlayerStaminaSystem!");
    }

    protected override bool Statement()
    {
        if (staminaSystem == null) return false;

        // คืนค่า true ถ้ายังมีพลังงานเหลือ (แม้แต่นิดเดียวก็วิ่งต่อได้)
        // ถ้าเป็น false แปลว่าพลังหมดเกลี้ยงแล้ว
        return staminaSystem.HasEnoughStamina(0.1f);
    }
}