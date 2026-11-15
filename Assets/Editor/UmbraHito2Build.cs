using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class UmbraHito2Build
{
    private const string OutputDirectory = "Builds/Hito2";
    private const string ExecutablePath = OutputDirectory + "/UMBRA_Hito2.exe";

    [MenuItem("Tools/UMBRA/Build Hito 2")]
    public static void BuildDeliverable()
    {
        UmbraPrototypeBuilder.RebuildAndValidate();
        UmbraTechnicalReport.Generate();

        Directory.CreateDirectory(OutputDirectory);
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length != 5 || scenes.Any(path => !path.Contains("/Chapter_")))
        {
            throw new InvalidOperationException(
                "Hito 2 requiere exactamente cinco escenas Chapter_ activas en Build Settings.");
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = ExecutablePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log("UMBRA HITO 2 BUILD: " + summary.result +
            " | size=" + summary.totalSize +
            " | warnings=" + summary.totalWarnings +
            " | errors=" + summary.totalErrors);

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException("Hito 2 build failed: " + summary.result);
        }

        Debug.Log("UMBRA HITO 2 READY: " + ExecutablePath);
    }
}
