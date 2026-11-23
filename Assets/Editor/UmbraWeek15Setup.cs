using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class UmbraWeek15Setup
{
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Chapter_01_El_Fondo.unity",
        "Assets/Scenes/Chapter_02_Los_Abandonados.unity",
        "Assets/Scenes/Chapter_03_La_Carne.unity",
        "Assets/Scenes/Chapter_04_El_Peso.unity",
        "Assets/Scenes/Chapter_05_El_Umbral.unity"
    };

    [MenuItem("Tools/UMBRA/Semana 15/Configurar Hito 3")]
    public static void ConfigureAllScenes()
    {
        foreach (string scenePath in ScenePaths)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                throw new InvalidOperationException("No existe la escena: " + scenePath);
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ConfigureCurrentScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("UMBRA SEMANA 15 CONFIGURADA: " + scenePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("UMBRA SEMANA 15 READY: cinco capitulos con IA y zonas de camara");
    }

    private static void ConfigureCurrentScene()
    {
        CameraFollow2D cameraFollow = UnityEngine.Object.FindFirstObjectByType<CameraFollow2D>();
        PlayerController2D player = UnityEngine.Object.FindFirstObjectByType<PlayerController2D>();
        EnemyAI2D[] enemies = UnityEngine.Object.FindObjectsByType<EnemyAI2D>(FindObjectsSortMode.None);

        if (cameraFollow == null || player == null)
        {
            throw new InvalidOperationException("La escena requiere CameraFollow2D y PlayerController2D.");
        }

        cameraFollow.lookAheadDistance = 1.25f;
        cameraFollow.lookAheadSmooth = 5f;
        cameraFollow.defaultTransitionSpeed = 3.5f;
        EditorUtility.SetDirty(cameraFollow);

        foreach (EnemyAI2D enemy in enemies)
        {
            enemy.verticalTolerance = 2.4f;
            enemy.lostSightGrace = 0.35f;
            enemy.wallCheckDistance = 0.55f;
            enemy.visionAngle = enemy.kind == EnemyAI2D.EnemyKind.GrotesqueDemon ? 120f : 180f;
            EditorUtility.SetDirty(enemy);
        }

        GameObject previousRoot = GameObject.Find("Semana 15 - Camera Zones");
        if (previousRoot != null)
        {
            UnityEngine.Object.DestroyImmediate(previousRoot);
        }

        GameObject root = new GameObject("Semana 15 - Camera Zones");
        Vector3 playerPosition = player.transform.position;
        Vector3 focusPosition = enemies.Length > 0
            ? enemies.OrderBy(enemy => Mathf.Abs(enemy.transform.position.x - playerPosition.x)).Last().transform.position
            : playerPosition + Vector3.right * 9f;

        float horizontalDirection = focusPosition.x >= playerPosition.x ? 1f : -1f;
        CreateZone(
            root.transform,
            "Zona de amenaza",
            focusPosition,
            new Vector2(9f, 6.5f),
            new Vector3(horizontalDirection * 1.35f, 1.35f, -10f),
            4.65f,
            3.2f);

        EditorUtility.SetDirty(root);
    }

    private static void CreateZone(
        Transform parent,
        string zoneName,
        Vector3 center,
        Vector2 size,
        Vector3 cameraOffset,
        float orthographicSize,
        float transitionSpeed)
    {
        GameObject zoneObject = new GameObject("Camera Zone - " + zoneName);
        zoneObject.transform.SetParent(parent);
        zoneObject.transform.position = new Vector3(center.x, center.y, 0f);

        BoxCollider2D trigger = zoneObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = size;

        CameraZone2D zone = zoneObject.AddComponent<CameraZone2D>();
        zone.zoneName = zoneName;
        zone.overrideOffset = true;
        zone.cameraOffset = cameraOffset;
        zone.overrideZoom = true;
        zone.orthographicSize = orthographicSize;
        zone.transitionSpeed = transitionSpeed;
        zone.overrideBounds = false;
    }
}
