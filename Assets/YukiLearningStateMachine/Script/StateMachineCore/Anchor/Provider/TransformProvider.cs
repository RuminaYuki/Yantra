using UnityEngine;

public class TransformProvider : MonoBehaviour
{
    [SerializeField] private TransformAnchor _transformAnchor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _transformAnchor.Provide(transform);
    }

    private void OnDisable()
    {
        _transformAnchor.Unset();
    }
}
