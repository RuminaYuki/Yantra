using UnityEngine;

[CreateAssetMenu(
    fileName = "NewStat",
    menuName = "Game/Stats/Stat"
)]
public class StatSO : ScriptableObject
{
    [SerializeField] private string displayName;

    public string DisplayName =>
        string.IsNullOrWhiteSpace(displayName)
            ? name
            : displayName;
}