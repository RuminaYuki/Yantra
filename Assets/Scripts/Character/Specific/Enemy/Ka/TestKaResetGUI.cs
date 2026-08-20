using UnityEngine;

public class TestKaResetGUI : MonoBehaviour
{
    [SerializeField] private KaResetController _kaResetController;
    [SerializeField] private Rect _buttonRect = new Rect(20f, 20f, 180f, 50f);

    private void OnGUI()
    {
        if (!GUI.Button(_buttonRect, "Reset Ka Enemy"))
            return;

        if (_kaResetController == null)
        {
            Debug.LogWarning("TestKaResetGUI: KaResetController is not assigned.", this);
            return;
        }

        _kaResetController.ResetEnemy();
    }
}