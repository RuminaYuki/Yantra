using UnityEngine;

[CreateAssetMenu(fileName = "SaveSO", menuName = "Scriptable Objects/SaveSO")]
public class SaveSO : ScriptableObject
{
    public int SceneIndex;
    public Vector3 PlayerPosition;
    public Vector3 PlayerRotation;
    public int PlayerHealth;
    public int YantCount;
}
