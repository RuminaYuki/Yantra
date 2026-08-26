using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PlayerTeleport : MonoBehaviour
{
    [SerializeField] private List<TeleportPointSO> _teleportPoints = new(5);

    private static bool _hasPendingTeleport;
    private static Vector3 _pendingPosition;
    private static Quaternion _pendingRotation;

    [SerializeField] private VoidEventChannelSO _kaVoidEventChannel;
    private static bool _hasKaVoidEventChannel;
    [SerializeField] private VoidEventChannelSO _taniVoidEventChannel;
    private static bool _hasTaniVoidEventChannel;

    [SerializeField] private IntEventChannelSO _taniEventChannel;
    private static bool _hasTaniEventChannel;
    private static int _intTaniEvent;

#if UNITY_EDITOR
    [Tooltip("Folder where new TeleportPointSO assets are saved when pressing I.")]
    [SerializeField] private string _saveFolder = "Assets/TeleportPoints";
    [SerializeField] Transform _playerControllerObj;
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
                break;
            case "5":
                TeleportToPoint(4);
                break;
            case "6":
                TeleportToPoint(5);
                break;
        }

        if (Input.GetKeyUp(KeyCode.I))
        {
            SetPosition();
        }
    }

    private void Awake()
    {
        if (_hasPendingTeleport)
        {
            transform.position = _pendingPosition;
            transform.rotation = _pendingRotation;
            _hasPendingTeleport = false;
        }
    }

    private void Start()
    {
        if (_hasKaVoidEventChannel)
        {
            Debug.Log("here");
            _kaVoidEventChannel.Raise();
            _hasKaVoidEventChannel = false;
        }

        if (_hasTaniVoidEventChannel)
        {
            _taniVoidEventChannel.Raise();
            _hasTaniVoidEventChannel = false;
        }

        if (_hasTaniEventChannel)
        {
            _taniEventChannel.Raise(_intTaniEvent);
            _hasTaniEventChannel = false;
        }
    }

    /// <param name="index">จุดที่ teleport</param>
    private void TeleportToPoint(int index)
    {
        if (index >= 0 && index < _teleportPoints.Count)
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            TeleportPointSO targetPoint = _teleportPoints[index];
            _pendingPosition = targetPoint.Position;
            _pendingRotation = targetPoint.Rotation;
            _hasPendingTeleport = true;

            if (index >= 3)
            {
                _hasKaVoidEventChannel = true;
                _hasTaniVoidEventChannel = true;
            }

            if (index == 4 || index == 5)
            {
                _intTaniEvent = index - 3;
                _hasTaniEventChannel = true;
            }

            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogWarning($"Invalid teleport point index: {index}");
        }
    }

    private void SetPosition()
    {
#if UNITY_EDITOR
        var point = TeleportPointSO.CreateFromTransform(_playerControllerObj.transform, _saveFolder);
#else
        var point = ScriptableObject.CreateInstance<TeleportPointSO>();
        point.Init(transform);
#endif
        _teleportPoints.Add(point);
    }
}
