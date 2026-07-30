using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Yuki.Learning.StateMachine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class Provider : MonoBehaviour
{
    [SerializeField] private List<ScriptableObject> Anchors = new List<ScriptableObject>();
    [SerializeField] private List<IRuntimeAnchorBase> runtimeAnchorBases = new List<IRuntimeAnchorBase>();

    private void Awake()
    {
        Provide();
    }

    private void Provide()
    {
        for (int i = Anchors.Count - 1; i >= 0; i--)
        {
            if (Anchors[i] is not IRuntimeAnchorBase anchor)
            {
                Anchors.RemoveAt(i);
                continue;
            }

            anchor.Provide(gameObject);
            runtimeAnchorBases.Add(anchor);
        }
    }

    private void OnDisable()
    {
        for (int i = runtimeAnchorBases.Count - 1; i >= 0; i--)
        {
            runtimeAnchorBases[i].Unset();
        }
    }
}