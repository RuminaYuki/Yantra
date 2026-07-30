using UnityEngine;
using Yuki.Learning.StateMachine;
[CreateAssetMenu(
    fileName = "GameObjectAnchor",
    menuName = "YUKI Learning State Machine/Anchor/GameObjectAnchor")]
public class GameObjectAnchor : RuntimeAnchorBase<GameObject>, IRuntimeAnchorBase
{
    public void Provide(GameObject gameObject)
    {
        base.Provide(gameObject);
    }

    public void Unset()
    {
        base.Unset();
    }
}
