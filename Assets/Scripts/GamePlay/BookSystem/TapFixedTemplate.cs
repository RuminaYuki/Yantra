using UnityEngine;

public class TapFixedTemplate : MonoBehaviour
{
    public string fixedTemplateName;

    [SerializeField] private BookTab bookTab;
    [SerializeField] private YantraGridShapeMatcher yantraGridShapeMatcher;

    private void Start()
    {
        if (bookTab == null) bookTab = GetComponent<BookTab>();
    }

    private void OnEnable()
    {
        if (bookTab == null) return;
        bookTab.OnTabClicked += OnTabClicked;
    }

    private void OnDisable()
    {
        if (bookTab == null) return;
        bookTab.OnTabClicked -= OnTabClicked;
    }

    private void OnTabClicked(bool value)
    {
        if (!value) return;
        //Debug.Log($"TapFixedTemplate: OnTabClicked - {value} {gameObject.name}");
        yantraGridShapeMatcher.SetFixedTemplate(fixedTemplateName);
    }
}
