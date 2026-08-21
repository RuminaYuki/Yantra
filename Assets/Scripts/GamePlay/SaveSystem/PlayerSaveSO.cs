using UnityEngine;

[CreateAssetMenu(
    fileName = "New Player Save",
    menuName = "SaveSystem/SaveSO/PlayerSaveSO")]
public class PlayerSave : BaseSave
{
    public bool HasData;

    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public float Health;
}
