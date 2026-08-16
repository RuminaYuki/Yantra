using UnityEngine;

public class NormalInteractable : InteractableBase
{
    public override bool Interact(GameObject rootplayer)
    {
        if (!base.Interact(rootplayer)) return false;

        CancelInteraction(rootplayer);

        return true;
    }

    public override bool CancelInteraction(GameObject rootplayer)
    {
        return base.CancelInteraction(rootplayer);
    }
}
