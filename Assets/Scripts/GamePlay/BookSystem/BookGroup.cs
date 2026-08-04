using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BookGroup : MonoBehaviour
{
    [SerializeField] private List<BookTabData> bookTabs = new();

    private void Awake()
    {
        foreach (var bookTab in bookTabs)
        {
            bookTab.bookTab.SetBookGroup(this);
            bookTab.bookTab.SetActiveGameObjects(bookTab == bookTabs[0]);
        }
    }

    public void ToggleEnable(BookTab bookTab)
    {
        if (bookTabs.Exists(x => x.bookTab == bookTab))
        {
            foreach (var _bookTab in bookTabs)
            {
                if (_bookTab.setInObjectActive)
                {
                    _bookTab.bookTab.gameObject.SetActive(_bookTab.bookTab == bookTab);
                    continue;
                }
                _bookTab.bookTab.SetActiveGameObjects(_bookTab.bookTab == bookTab);
            }
        }

    }
}

[Serializable]
public class BookTabData
{
    public BookTab bookTab;
    [Tooltip("GameObject ที่จะถูกปิดใช้งานเมื่อ Tab นี้ถูกคลิก")]
    public bool setInObjectActive;
}
