using UnityEngine;

public class LightBeam : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO _beamEventChannel;
    [SerializeField] private GameObject _beamGameObject;

    private void Awake()
    {
        if (_beamEventChannel != null)
        {
            _beamEventChannel.Raised += HandleBeam;
        }
    }

    private void OnDestroy()
    {
        if (_beamEventChannel != null)
        {
            _beamEventChannel.Raised -= HandleBeam;
        }
    }

    private void HandleBeam(bool value)
    {
        //Debug.Log(_beamGameObject.name);
        if (_beamGameObject == null)
        {
            return;
        }

        _beamGameObject.SetActive(value);
    }
}
