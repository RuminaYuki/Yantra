using UnityEngine;
using Yuki.Learning.StateMachine;
using Yuki.Learning.StateMachine.ScriptableObjects;

[CreateAssetMenu(
    fileName = "ObstacleCollisionCondition",
    menuName = "YUKI Learning State Machine/StateMachine/Conditions/Obstacle Detector/Obstacle Collision")]
public class ObstacleCollisionConditionSO : StateConditionSO
{
    public override Condition CreateCondition()
    {
        return new ObstacleCollisionCondition();
    }
}

public class ObstacleCollisionCondition : Condition
{
    private ObstacleDetector _obstacleDetector;

    public override void Awake(StateMachine stateMachine)
    {
        _obstacleDetector = stateMachine.GetComponent<ObstacleDetector>();

        if (_obstacleDetector == null)
            Debug.LogError("ObstacleCollisionCondition requires ObstacleDetector.");
    }

    protected override bool Statement()
    {
        return _obstacleDetector != null &&
            _obstacleDetector.ConsumeCollision();
    }
}
