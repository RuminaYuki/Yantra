using System.Reflection;
using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "NewCheckObjectIsActiveCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Check Object Is Active")]
public class CheckObjectIsActiveConditionSO : StateConditionSO
{
    [SerializeField] private GameObjectAnchor objectAnchor;

    public override Condition CreateCondition()
    {
        return new CheckObjectIsActiveCondition(objectAnchor);
    }
}

public class CheckObjectIsActiveCondition : Condition
{
    GameObjectAnchor objectAnchor = null;

    public CheckObjectIsActiveCondition(GameObjectAnchor objectAnchor)
    {
        this.objectAnchor = objectAnchor;
    }

    protected override bool Statement()
    {
        if(objectAnchor == null) return false;
        if (objectAnchor.Value == null) return false;

        bool value = objectAnchor.Value.activeSelf; 
        Debug.Log(value);
        return value;
    }
}
