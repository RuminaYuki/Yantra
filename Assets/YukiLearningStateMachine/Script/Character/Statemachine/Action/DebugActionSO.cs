using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "DebugAction", 
    menuName = "YUKI Learning State Machine/Actions/DebugAction")]
public class DebugCheckStateActionSO : StateActionSO
{
    public DebugSetting DebugSetting;
    public override StateAction CreateAction(StateMachine stateMachine)
    {
        return new DebugAction(DebugSetting);
    }
}

public class DebugAction : StateAction
{
    private string _stateName;

    private bool _enableCheckUpdate;

    public DebugAction(DebugSetting debugSetting)
    {
        _stateName = debugSetting.StateName;

        _enableCheckUpdate = debugSetting.EnableCheckUpdate;
    }

    public override void OnStateEnter()
    {
        Debug.Log($"Enter: {_stateName}");
    }

    public override void OnUpdate()
    {
        if (!_enableCheckUpdate) return;
        Debug.Log($"Update: {_stateName}");
    }

    public override void OnStateExit()
    {
        Debug.Log($"Exit: {_stateName}");
    }
}

[System.Serializable]
public struct DebugSetting
{
    public string StateName;
    public bool EnableCheckUpdate;
}

