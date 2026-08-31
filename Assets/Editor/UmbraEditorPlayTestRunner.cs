using UnityEditor;
using UnityEditor.SceneManagement;

public static class UmbraEditorPlayTestRunner
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Level_01_Forest.unity", OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }
}
