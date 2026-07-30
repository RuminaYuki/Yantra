using UnityEngine;
using Yuki.Learning.StateMachine;
[CreateAssetMenu(
    fileName = "AnimatorAnchor",
    menuName = "YUKI Learning State Machine/Anchor/AnimatorAnchor")]

public class AnimatorAnchor : RuntimeAnchorBase<Animator> , IRuntimeAnchorBase
{
    public void Provide(GameObject player)
    {
        Animator value = player.GetComponent<Animator>();

        base.Provide(value);
    }

    public void Unset()
    {
        base.Unset();
    }
}

public interface IRuntimeAnchorBase
{
    void Provide(GameObject player);
    void Unset();
}
