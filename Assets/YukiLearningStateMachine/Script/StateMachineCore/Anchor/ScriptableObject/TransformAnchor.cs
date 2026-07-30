using UnityEngine;
using Yuki.Learning.StateMachine;

[CreateAssetMenu(
    fileName = "TransformAnchor",
    menuName = "YUKI Learning State Machine/Anchor/TransformAnchor")]
public class TransformAnchor : RuntimeAnchorBase<Transform>, IRuntimeAnchorBase
{
    public void Provide(GameObject gameObject)
    {
        Transform value = gameObject.transform.GetComponent<Transform>();

        base.Provide(value);
    }

    public void Unset()
    {
        base.Unset();
    }
}
