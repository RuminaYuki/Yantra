using UnityEngine;

public class PlayerYantAnimationEvent : MonoBehaviour
{
    [SerializeField] private YantEffectController _currentController;

    public void SetCurrentController(YantEffectController controller)
    {
        _currentController = controller;
    }

    public void TriggerYantEffect(int boolInt = 0)
    {
        bool value = false;
        if (boolInt == 1) value = true;

        if (_currentController != null)
        {
            //Debug.Log(_currentController);
            _currentController.TriggerAnimationTiming(value);
            //_currentController = null;
        }
    }
}