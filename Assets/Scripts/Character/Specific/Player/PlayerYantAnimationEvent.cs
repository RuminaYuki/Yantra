using UnityEngine;

public class PlayerYantAnimationEvent : MonoBehaviour
{
    [SerializeField] private YantEffectController _currentController;

    public void SetCurrentController(YantEffectController controller)
    {
        _currentController = controller;
    }

    public void TriggerYantEffect()
    {
        if (_currentController != null)
        {
            _currentController.TriggerAnimationTiming();
            _currentController = null;
        }
    }
}