using UnityEngine;

public class CheckSize : MonoBehaviour
{
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();

        Debug.Log($"Size = {renderer.bounds.size}");
        Debug.Log($"Width = {renderer.bounds.size.x} m");
        Debug.Log($"Height = {renderer.bounds.size.y} m");
        Debug.Log($"Depth = {renderer.bounds.size.z} m");
    }
}