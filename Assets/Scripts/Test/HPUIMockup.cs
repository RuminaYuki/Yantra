using UnityEngine;
using TMPro;

public class HPUIMockup : MonoBehaviour
{
    public Health health;
    public TextMeshProUGUI textMeshPro;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.text = "Hp:" + health.CurrentHP + "/10";
    }
}
