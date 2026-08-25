using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerTeleport : MonoBehaviour
{
    [SerializeField] private List<TeleportPointSO> _teleportPoints = new(5);

    [SerializeField] private VoidEventChannelSO _kaVoidEventChannel;
    [SerializeField] private VoidEventChannelSO _taniVoidEventChannel;

    [SerializeField] private IntEventChannelSO _taniEventChannel;

#if UNITY_EDITOR
    [Tooltip("Folder where new TeleportPointSO assets are saved when pressing I.")]
    [SerializeField] private string _saveFolder = "Assets/TeleportPoints";
#endif

    private void Update()
    {
        switch (Input.inputString)
        {
            case "1":
                TeleportToPoint(0);
                break;
            case "2":
                TeleportToPoint(1);
                break;
            case "3":
                TeleportToPoint(2);
                break;
            case "4":
                TeleportToPoint(3);
                _kaVoidEventChannel.Raise();
                _taniVoidEventChannel.Raise();
                break;
            case "5":
                TeleportToPoint(4);
                _kaVoidEventChannel.Raise();
                _taniVoidEventChannel.Raise();
                _taniEventChannel.Raise(1);
                break;
            case "6":
                TeleportToPoint(5);
                _kaVoidEventChannel.Raise();
                _taniVoidEventChannel.Raise();
                _taniEventChannel.Raise(2);
                break;
        }

        if (Input.GetKeyUp(KeyCode.I))
        {
            SetPosition();
        }
    }

    /// <param name="index">จุดที่ teleport</param>
    private void TeleportToPoint(int index)
    {
        if (index >= 0 && index < _teleportPoints.Count)
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(sceneIndex);

            TeleportPointSO targetPoint = _teleportPoints[index];
            transform.position = targetPoint.Position;
            transform.rotation = targetPoint.Rotation;
        }
        else
        {
            Debug.LogWarning($"Invalid teleport point index: {index}");
        }
    }

    private void SetPosition()
    {
#if UNITY_EDITOR
        var point = TeleportPointSO.CreateFromTransform(transform, _saveFolder);
#else
        var point = ScriptableObject.CreateInstance<TeleportPointSO>();
        point.Init(transform);
#endif
        _teleportPoints.Add(point);
    }
}
