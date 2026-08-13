using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public struct PaperLine
{
    [Tooltip("What is the point of having it?")]
    public Sprite icon;

    [Tooltip("Name of paper")]
    public string title;

    [Tooltip("\"Content\"—there is nothing more to it than that.")]
    [TextArea]
    public string content;
} 

[CreateAssetMenu(
    fileName = "New Paper Data",
    menuName = "UI/Paper/Paper Data")]
public class PaperDataSO : ScriptableObject
{
    [Tooltip("The sequence of all content on this sheet (read from top to bottom)")]
    public PaperLine Line;
}
