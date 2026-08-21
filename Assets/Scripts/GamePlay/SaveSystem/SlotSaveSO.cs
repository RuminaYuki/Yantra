using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Slot Save", menuName = "SaveSystem/SlotSO")]
public class SlotSaveSO : ScriptableObject
{
    private List<BaseSave> ListSaveSo = new();
}
