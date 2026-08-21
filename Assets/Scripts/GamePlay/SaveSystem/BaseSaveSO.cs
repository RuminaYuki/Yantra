using UnityEngine;

public class BaseSave : ScriptableObject
{
    protected string ID = string.Empty;
    public virtual string GetID() { return ID; }
}
