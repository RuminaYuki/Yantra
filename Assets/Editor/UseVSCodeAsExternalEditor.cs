using UnityEditor;

[InitializeOnLoad]
internal static class UseVisualStudioAsExternalEditor
{
    private const string VSPath = @"C:\Program Files\Microsoft Visual Studio\18\Enterprise\Common7\IDE\devenv.exe";

    static UseVisualStudioAsExternalEditor()
    {
        Apply();
    }

    [MenuItem("Tools/Setup/Use Visual Studio As Script Editor")]
    private static void ApplyFromMenu()
    {
        Apply();
        EditorUtility.DisplayDialog("Script Editor", "Unity will now open scripts in Visual Studio.", "OK");
    }

    private static void Apply()
    {
        if (System.IO.File.Exists(VSPath))
            EditorPrefs.SetString("kScriptsDefaultApp", VSPath);
    }
}
