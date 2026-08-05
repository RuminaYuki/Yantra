using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class BookTab : MonoBehaviour
{
    private BookGroup bookGroup;

    [Tooltip("GameObject ที่จะถูกปิดใช้งานเมื่อ Tab นี้ถูกคลิก")]
    [SerializeField] List<GameObject> gameObjOnDisable = new List<GameObject>();

    //เพื่อให้สามารถ subscribe event ได้จากภายนอก ทำสิ่งอื่นๆได้
    public Action<bool> OnTabClicked;
 
    public void OnClick()
    {
        if(bookGroup != null)
        {
            bookGroup.ToggleEnable(this);
        }

    }

    public void SetActiveGameObjects(bool isActive)
    {
        foreach (var obj in gameObjOnDisable)
        {
            obj.SetActive(isActive);
        }
        OnTabClicked?.Invoke(isActive);
    }


    //API
    public void SetBookGroup(BookGroup bookGroup)
    {
        this.bookGroup = bookGroup;
    }
}
