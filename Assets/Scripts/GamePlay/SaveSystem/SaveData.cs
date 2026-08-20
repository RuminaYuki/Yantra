using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string sceneName;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public int yantCount;
    public float currentHp;
    public float currentStamina;
}
