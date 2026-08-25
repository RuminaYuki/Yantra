using UnityEngine;

public class LightBeam : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO _beamEventChannel;
    [SerializeField] private GameObject _beamGameObject;

    private void Awake()
    {
        _beamEventChannel.Raised += HandleBeam;
    }

    private void HandleBeam(bool value)
    {
        //Debug.Log(_beamGameObject.name);
        _beamGameObject.SetActive(value);
    }
}
