using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class UmbraWeek15Build
{
    private const string OutputDirectory = "Builds/Semana15";
    private const string ExecutablePath = OutputDirectory + "/UMBRA_Hito3.exe";

    [MenuItem("Tools/UMBRA/Semana 15/Build Hito 3")]
    public static void BuildDeliverable()
    {
        UmbraPrototypeBuilder.RebuildAndValidate();
        UmbraWeek15Setup.ConfigureAllScenes();
        UmbraTechnicalReport.Generate();

        Directory.CreateDirectory(OutputDirectory);
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length != 5 || scenes.Any(path => !path.Contains("/Chapter_")))
        {
            throw new InvalidOperationException(
                "Hito 3 requiere exactamente cinco escenas Chapter_ activas.");
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
        Debug.Log("UMBRA HITO 3 BUILD: " + summary.result +
            " | size=" + summary.totalSize +
            " | warnings=" + summary.totalWarnings +
            " | errors=" + summary.totalErrors);

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException("Hito 3 build failed: " + summary.result);
        }

        File.WriteAllText(
            Path.Combine(OutputDirectory, "LEEME_SEMANA15.txt"),
            "UMBRA Hito 3 - Semana 15\n" +
            "Ejecute UMBRA_Hito3.exe.\n" +
            "Demuestre patrulla, vigilancia, persecucion, busqueda y retorno.\n" +
            "Muestre seguimiento, adelanto, limites, zona de amenaza, zoom y trauma de camara.\n");
        Debug.Log("UMBRA HITO 3 READY: " + ExecutablePath);
    }
}
