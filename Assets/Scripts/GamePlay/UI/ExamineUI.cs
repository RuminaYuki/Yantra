using TMPro;
using UnityEngine;
using static UnityEditor.U2D.ScriptablePacker;

public class ExamineUI : MonoBehaviour
{
    public static ExamineUI Instance { get; private set; }

    [SerializeField] private GameObject examineCanvas;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            this.enabled = false;
            Debug.LogWarning("ExamineUI already exists.");
        }

        Close();
    }

    public void Open(PaperDataSO data)
    {
        examineCanvas.SetActive(true);

        // เอาข้อมูลไปใส่ UI
        titleText.text = data.Line.title;
        contentText.text = data.Line.content;
    }

    public void Close()
    {
        examineCanvas.SetActive(false);
    }
}
