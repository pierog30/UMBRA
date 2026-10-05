using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class UmbraWeek8Build
{
    private const string OutputDirectory = "Builds/Semana08";
    private const string ExecutablePath = OutputDirectory + "/UMBRA_Semana08.exe";

    [MenuItem("Tools/UMBRA/Build Semana 8 Deliverable")]
    public static void BuildDeliverable()
    {
        UmbraPrototypeBuilder.RebuildAndValidate();
        UmbraTechnicalReport.Generate();
        if (!Application.isBatchMode)
        {
            UmbraPrototypeBuilder.CaptureAllPreviews();
        }

        Directory.CreateDirectory(OutputDirectory);
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = ExecutablePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log("UMBRA WEEK 8 BUILD: " + summary.result +
            " | size=" + summary.totalSize +
            " | warnings=" + summary.totalWarnings +
            " | errors=" + summary.totalErrors);

        if (summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception("Semana 8 build failed: " + summary.result);
        }

        Debug.Log("UMBRA WEEK 8 DELIVERABLE READY: " + ExecutablePath);
    }
}
