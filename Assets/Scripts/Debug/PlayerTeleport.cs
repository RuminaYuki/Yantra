using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class TeleportPoint
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public TeleportPoint(Transform transform)
    {
        Position = transform.position;
        Rotation = transform.rotation;
        Scale = transform.localScale;
    }
}


public class PlayerTeleport : MonoBehaviour
{
    [SerializeField] private List<TeleportPoint> _teleportPoints = new(5);

    private void Update()
    {
        switch (Input.inputString)
        {
            case "1":
                TeleportToPoint(0);
                break;
            case "2":
                TeleportToPoint(1);
                break;
            case "3":
                TeleportToPoint(2);
                break;
            case "4":
                TeleportToPoint(3);
                break;
            case "5":
                TeleportToPoint(4);
                break;
            case "6":
                TeleportToPoint(5);
                break; 
        }

        if (Input.GetKeyUp(KeyCode.Minus))
        {
            SetPosition();
        }
    }

    private void TeleportToPoint(int index)
    {
        if (index >= 0 && index < _teleportPoints.Count)
        {
            TeleportPoint targetPoint = _teleportPoints[index];
            transform.position = targetPoint.Position;
            transform.rotation = targetPoint.Rotation;
        }
        else
        {
            Debug.LogWarning($"Invalid teleport point index: {index}");
        }
    }

    private void SetPosition()
    {
        _teleportPoints.Add(new TeleportPoint(transform));
    }
}
