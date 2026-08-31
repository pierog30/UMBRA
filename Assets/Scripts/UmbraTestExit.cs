using UnityEngine;

public static class UmbraTestExit
{
    public static void Quit(int exitCode)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(exitCode);
#else
        Application.Quit(exitCode);
#endif
    }
}
