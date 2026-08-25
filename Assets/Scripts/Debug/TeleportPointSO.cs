using UnityEngine;

[CreateAssetMenu(fileName = "TeleportPoint", menuName = "Teleport/Teleport Point", order = 0)]
public class TeleportPointSO : ScriptableObject
{
    [field: SerializeField] public Vector3 Position { get; private set; }
    [field: SerializeField] public Quaternion Rotation { get; private set; }
    [field: SerializeField] public Vector3 Scale { get; private set; }

    public void Init(Transform transform)
    {
        Position = transform.position;
        Rotation = transform.rotation;
        Scale = transform.localScale;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Creates a new TeleportPointSO instance from a Transform and saves it
    /// as a persistent asset so the reference survives play mode / domain reload.
    /// </summary>
    public static TeleportPointSO CreateFromTransform(Transform transform, string savePath)
    {
        var point = CreateInstance<TeleportPointSO>();
        point.Init(transform);

        if (!UnityEditor.AssetDatabase.IsValidFolder(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
            UnityEditor.AssetDatabase.Refresh();
        }

        string fileName = $"TeleportPoint_{System.DateTime.Now:yyyyMMdd_HHmmss}.asset";
        string assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"{savePath}/{fileName}");

        UnityEditor.AssetDatabase.CreateAsset(point, assetPath);
        UnityEditor.AssetDatabase.SaveAssets();

        return point;
    }
#endif
}
