using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class UmbraPrototypeBuilder
{
    private const string MarkerPath = "Assets/UMBRA_SETUP_DONE.txt";
    private const string SetupVersion = "UMBRA week 8 mechanics and parallax v3";
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Chapter_01_El_Fondo.unity",
        "Assets/Scenes/Chapter_02_Los_Abandonados.unity",
        "Assets/Scenes/Chapter_03_La_Carne.unity",
        "Assets/Scenes/Chapter_04_El_Peso.unity",
        "Assets/Scenes/Chapter_05_El_Umbral.unity"
    };
    private static Sprite[] creatureFrames;
    private static Sprite[] hospitalFrames;

    [InitializeOnLoadMethod]
    private static void AutoBuildOnce()
    {
        EditorApplication.delayCall += () =>
        {
            if (File.Exists(MarkerPath) && File.ReadAllText(MarkerPath).Trim() == SetupVersion)
            {
                // Preserve the chapter the designer had open instead of forcing chapter 1
                // every time scripts reload or the editor starts.
                return;
            }

            RebuildValidateAndCapture();
        };
    }

    [MenuItem("Tools/UMBRA/Rebuild Complete Game")]
    public static void BuildScene()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Art");
        EnsureLayer("Ground", 6);
        ConfigureWindowsPlayer();

        Sprite black = CreateColorSprite("black_square", new Color(0.02f, 0.02f, 0.025f, 1f));
        Sprite gray = CreateColorSprite("gray_square", new Color(0.45f, 0.45f, 0.48f, 1f));
        Sprite[] backgrounds =
        {
            LoadSpriteAsset("Assets/Art/Backgrounds/foggy_forest.png", 100f),
            LoadSpriteAsset("Assets/Art/Backgrounds/foggy_ruins.png", 100f),
            LoadSpriteAsset("Assets/Art/Backgrounds/abandoned_factory.png", 100f),
            LoadSpriteAsset("Assets/Art/Backgrounds/deep_caverns.png", 100f),
            LoadSpriteAsset("Assets/Art/Backgrounds/final_escape.png", 100f)
        };
        Sprite terrain = CreateTerrainSprite();
        Sprite[] characterFrames = CreateSheetFrames(
            "Assets/Art/Character/umbra_character_sheet.png",
            "Assets/Art/Character/Frames",
            "character",
            4,
            3,
            1.7f);
        Sprite[] props = CreateSheetFrames(
            "Assets/Art/Props/umbra_props_sheet.png",
            "Assets/Art/Props/Frames",
            "prop",
            4,
            3,
            2f);
        creatureFrames = CreateSheetFrames(
            "Assets/Art/Creatures/umbral_creatures_sheet.png",
            "Assets/Art/Creatures/Frames",
            "creature",
            2,
            2,
            2.35f);
        hospitalFrames = CreateSheetFrames(
            "Assets/Art/Hospital/umbral_hospital_clues_sheet.png",
            "Assets/Art/Hospital/Frames",
            "hospital_clue",
            3,
            2,
            2f);
        PhysicsMaterial2D noFriction = CreateNoFrictionMaterial();

        for (int level = 1; level <= ScenePaths.Length; level++)
        {
            BuildLevel(level, backgrounds[level - 1], terrain, black, gray, characterFrames, props, noFriction);
        }

        var buildScenes = new EditorBuildSettingsScene[ScenePaths.Length];
        for (int i = 0; i < ScenePaths.Length; i++)
        {
            buildScenes[i] = new EditorBuildSettingsScene(ScenePaths[i], true);
        }

        EditorBuildSettings.scenes = buildScenes;
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePaths[0]);
        Debug.Log("UMBRA: five connected chapters created.");
    }

    private static void ConfigureWindowsPlayer()
    {
        PlayerSettings.productName = "UMBRA";
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.SetGraphicsAPIs(
            BuildTarget.StandaloneWindows64,
            new[] { GraphicsDeviceType.Direct3D11 });
    }

    private static void BuildLevel(
        int level,
        Sprite forest,
        Sprite terrain,
        Sprite black,
        Sprite gray,
        Sprite[] characterFrames,
        Sprite[] props,
        PhysicsMaterial2D noFriction)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("Audio Ambiente").AddComponent<UmbraAudio>();

        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.gameObject.AddComponent<AudioListener>();
        camera.orthographic = true;
        camera.orthographicSize = 4.7f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.018f, 0.018f, 0.022f);

        Color[] chapterTints =
        {
            new Color(0.28f, 0.3f, 0.34f), new Color(0.42f, 0.45f, 0.5f),
            new Color(0.55f, 0.18f, 0.18f), new Color(0.34f, 0.3f, 0.32f),
            new Color(0.72f, 0.72f, 0.68f)
        };
        Color levelTint = chapterTints[level - 1];
        for (int i = 0; i < 3; i++)
        {
            GameObject background = CreateSpriteObject(
                "Infernal Background " + (i + 1),
                forest,
                new Vector2(-7f + (i * 19f), -0.1f),
                new Vector2(1.15f, 1.15f));
            SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = -100;
            renderer.color = levelTint;
            ParallaxLayer2D parallax = background.AddComponent<ParallaxLayer2D>();
            parallax.horizontalFactor = 0.12f;
            parallax.verticalFactor = 0.04f;
        }

        GameObject player = CreatePlayer(characterFrames, noFriction);
        if (level == 1)
        {
            player.AddComponent<AwakeningSequence2D>();
        }
        CameraFollow2D follow = camera.gameObject.AddComponent<CameraFollow2D>();
        follow.target = player.transform;
        follow.smoothSpeed = 9f;
        follow.minBounds = new Vector2(-7f, -1f);
        follow.maxBounds = new Vector2(level == 5 ? 22f : 19f, 3.5f);
        follow.SnapToTarget();

        AddAtmosphere(gray, black, props, level);
        AddHospitalClues(props, level);

        switch (level)
        {
            case 1:
                BuildForestLevel(terrain, props);
                break;
            case 2:
                BuildRuinsLevel(terrain, props);
                break;
            case 3:
                BuildFactoryLevel(terrain, props);
                break;
            case 4:
                BuildCavernsLevel(terrain, props);
                break;
            default:
                BuildEscapeLevel(terrain, props);
                break;
        }

        GameObject killPlane = CreateTriggerObject("Bottomless Fog", black, new Vector2(8f, -7f), new Vector2(38f, 1f));
        killPlane.GetComponent<SpriteRenderer>().enabled = false;
        killPlane.AddComponent<DeathTrap>();

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePaths[level - 1]);
    }

    private static GameObject CreatePlayer(Sprite[] frames, PhysicsMaterial2D noFriction)
    {
        GameObject player = CreateSpriteObject("Player", frames[0], new Vector2(-7f, -1.45f), Vector2.one);
        BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();
        playerCollider.size = new Vector2(0.52f, 1.35f);
        playerCollider.edgeRadius = 0.04f;
        playerCollider.sharedMaterial = noFriction;

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.gravityScale = 3f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PlayerController2D controller = player.AddComponent<PlayerController2D>();
        player.AddComponent<ShadowDash2D>();
        player.AddComponent<PlayerRespawn>();

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.74f, 0f);
        controller.groundCheck = groundCheck.transform;
        controller.groundLayer = LayerMask.GetMask("Ground");

        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
        playerRenderer.sortingOrder = 20;
        PlayerSpriteAnimator animator = player.AddComponent<PlayerSpriteAnimator>();
        animator.controller = controller;
        animator.body = playerRenderer;
        animator.idleFrames = new[] { frames[0], frames[1], frames[2], frames[3] };
        animator.runFrames = new[] { frames[4], frames[5], frames[6], frames[7] };
        animator.jumpFrames = new[] { frames[8], frames[9] };
        animator.crouchFrames = new[] { frames[10], frames[11] };
        animator.framesPerSecond = 12f;
        return player;
    }

    private static void AddAtmosphere(Sprite gray, Sprite black, Sprite[] props, int level)
    {
        SpriteRenderer fogA = CreateSpriteObject("Back Fog A", gray, new Vector2(3f, 2f), new Vector2(25f, 0.16f)).GetComponent<SpriteRenderer>();
        fogA.color = new Color(0.85f, 0.85f, 0.88f, 0.08f);
        fogA.sortingOrder = -20;
        ParallaxLayer2D fogAParallax = fogA.gameObject.AddComponent<ParallaxLayer2D>();
        fogAParallax.horizontalFactor = 0.28f;
        fogAParallax.verticalFactor = 0.08f;
        fogAParallax.ambientDrift = new Vector2(0.018f, 0f);
        SpriteRenderer fogB = CreateSpriteObject("Back Fog B", gray, new Vector2(14f, 2.8f), new Vector2(22f, 0.12f)).GetComponent<SpriteRenderer>();
        fogB.color = new Color(0.8f, 0.8f, 0.82f, 0.06f);
        fogB.sortingOrder = -20;
        ParallaxLayer2D fogBParallax = fogB.gameObject.AddComponent<ParallaxLayer2D>();
        fogBParallax.horizontalFactor = 0.42f;
        fogBParallax.verticalFactor = 0.14f;
        fogBParallax.ambientDrift = new Vector2(-0.012f, 0f);

        CreateDecoration("Ruined Pillar A", props[8], new Vector2(4f + level, -0.7f), 1.15f, -8);
        CreateDecoration("Hanging Cage", props[9], new Vector2(11f + level, 1.8f), 0.9f, -7);
        CreateDecoration("Ruined Pillar B", props[8], new Vector2(20f, -0.9f), 0.8f, -8);
    }

    private static void BuildForestLevel(Sprite terrain, Sprite[] props)
    {
        CreateTerrain("Terrain Start", terrain, new Vector2(-4f, -2.65f), new Vector2(10f, 0.8f));
        CreateTerrain("Terrain Middle", terrain, new Vector2(4.5f, -2.65f), new Vector2(5f, 0.8f));
        CreateTerrain("Terrain End", terrain, new Vector2(12.5f, -2.65f), new Vector2(9f, 0.8f));
        CreateOneWayTerrain("Upper Path", terrain, new Vector2(6.5f, 0.1f), new Vector2(4.2f, 0.55f));

        CreateCrate(props[0], new Vector2(-4f, -1.55f));
        DeathTrap spikes = CreateSpikes(props[5], new Vector2(4.5f, -1.85f));
        CreatePressureSwitch(props[1], new Vector2(-2.1f, -2.12f), spikes);
        CreateCheckpoint(props[2], new Vector2(2.5f, -1.3f));
        CreateLadder(props[4], new Vector2(4.9f, -1.15f));
        CreateHidingSpot(props[9], new Vector2(9f, -1.1f));
        CreateEnemy("Ash Warden", props[8], new Vector2(10.8f, -1.25f), EnemyAI2D.EnemyKind.SorrowSoul, 2.2f, 2.6f);
        CreateHorrorEvent("a", props[9], new Vector2(0.6f, -0.8f), HorrorEvent2D.HorrorStyle.BackgroundRise);
        CreateKey(props[7], new Vector2(7.2f, 1.05f));
        CreateDoor(props[3], new Vector2(13.5f, -1.25f));
        CreateExit(props[10], new Vector2(16.2f, -1.15f));
    }

    private static void BuildRuinsLevel(Sprite terrain, Sprite[] props)
    {
        CreateTerrain("Terrain Start", terrain, new Vector2(-5.5f, -2.65f), new Vector2(7f, 0.8f));
        CreateTerrain("Terrain Island", terrain, new Vector2(2f, -2.65f), new Vector2(4f, 0.8f));
        CreateTerrain("Terrain End", terrain, new Vector2(13.5f, -2.65f), new Vector2(9f, 0.8f));
        CreateMovingPlatform("Moving Bridge A", terrain, new Vector2(-1.1f, -1.6f), new Vector2(2.1f, 0.45f), new Vector2(1.8f, 0f), 1.25f);
        CreateMovingPlatform("Moving Bridge B", terrain, new Vector2(5f, -0.9f), new Vector2(2.2f, 0.45f), new Vector2(3.5f, 0f), 1.05f);
        CreateOneWayTerrain("Upper Ruin", terrain, new Vector2(12.5f, 0.35f), new Vector2(4.5f, 0.55f));

        CreateCheckpoint(props[2], new Vector2(1f, -1.3f));
        CreateHidingSpot(props[9], new Vector2(9.65f, -1.1f));
        CreateEnemy("Abandoned Soul", props[8], new Vector2(12.8f, -1.25f), EnemyAI2D.EnemyKind.SorrowSoul, 1.25f, 3.5f);
        CreateHorrorEvent("a", props[8], new Vector2(4.2f, -0.7f), HorrorEvent2D.HorrorStyle.ForegroundRush);
        CreateLadder(props[4], new Vector2(10.7f, -1.15f));
        DeathTrap saw = CreateSaw(props[6], new Vector2(14.4f, -1.2f), new Vector2(0f, 2.3f), 1.5f);
        CreateLever(props[11], new Vector2(11.8f, 1.2f), saw);
        CreateKey(props[7], new Vector2(13.5f, 1.3f));
        CreateDoor(props[3], new Vector2(16.2f, -1.25f));
        CreateExit(props[10], new Vector2(17.45f, -1.15f));
    }

    private static void BuildFactoryLevel(Sprite terrain, Sprite[] props)
    {
        CreateTerrain("Terrain Start", terrain, new Vector2(-4f, -2.65f), new Vector2(10f, 0.8f));
        CreateTerrain("Terrain Middle", terrain, new Vector2(6f, -2.65f), new Vector2(8f, 0.8f));
        CreateTerrain("Terrain End", terrain, new Vector2(15.5f, -2.65f), new Vector2(7f, 0.8f));
        CreateTerrain("Low Tunnel Ceiling", terrain, new Vector2(-2.8f, -0.75f), new Vector2(4.2f, 0.9f));
        CreateOneWayTerrain("Factory Catwalk", terrain, new Vector2(7.5f, 0.2f), new Vector2(4.8f, 0.5f));

        CreateCrate(props[0], new Vector2(3f, -1.55f));
        CreateCheckpoint(props[2], new Vector2(2.1f, -1.3f));
        CreateHidingSpot(props[9], new Vector2(12.3f, -1.05f));
        CreateEnemy("Flesh Sentinel", props[6], new Vector2(14.5f, -1.2f), EnemyAI2D.EnemyKind.GrotesqueDemon, 1.2f, 4.4f);
        CreateHorrorEvent("a", props[8], new Vector2(0f, 1.2f), HorrorEvent2D.HorrorStyle.EnvironmentShift);
        DeathTrap sawA = CreateSaw(props[6], new Vector2(5f, -1.15f), new Vector2(2.4f, 0f), 1.9f);
        CreateSaw(props[6], new Vector2(11.8f, -1.2f), new Vector2(0f, 2.2f), 1.55f);
        // The safety lever must be reachable before the saw crosses the ladder route.
        CreateLever(props[11], new Vector2(3.55f, -1.15f), sawA);
        CreateLadder(props[4], new Vector2(6f, -1.15f));
        CreateKey(props[7], new Vector2(9f, 1.15f));
        CreateDoor(props[3], new Vector2(16f, -1.25f));
        CreateExit(props[10], new Vector2(18.2f, -1.15f));
    }

    private static void BuildCavernsLevel(Sprite terrain, Sprite[] props)
    {
        CreateTerrain("Terrain Start", terrain, new Vector2(-6f, -2.65f), new Vector2(6f, 0.8f));
        CreateTerrain("Terrain Shelf", terrain, new Vector2(1f, -0.55f), new Vector2(4f, 0.55f));
        CreateTerrain("Terrain Middle", terrain, new Vector2(7.5f, -2.65f), new Vector2(5f, 0.8f));
        CreateTerrain("Terrain End", terrain, new Vector2(16f, -2.65f), new Vector2(8f, 0.8f));
        CreateMovingPlatform("Cavern Lift A", terrain, new Vector2(-2.2f, -2f), new Vector2(2f, 0.45f), new Vector2(0f, 2.5f), 1.2f);
        CreateMovingPlatform("Cavern Bridge", terrain, new Vector2(3.5f, -0.8f), new Vector2(2f, 0.45f), new Vector2(2.5f, -1.1f), 1.1f);
        CreateMovingPlatform("Cavern Lift B", terrain, new Vector2(10.7f, -2f), new Vector2(2f, 0.45f), new Vector2(0f, 2.7f), 1.3f);
        CreateOneWayTerrain("Key Ledge", terrain, new Vector2(13.5f, 0.55f), new Vector2(3.5f, 0.5f));

        CreateCheckpoint(props[2], new Vector2(7f, -1.3f));
        CreateHidingSpot(props[9], new Vector2(14f, -1.05f));
        CreateEnemy("Guilt Bearer", props[8], new Vector2(16f, -1.25f), EnemyAI2D.EnemyKind.GrotesqueDemon, 2.7f, 4.9f);
        CreateHorrorEvent("a", props[9], new Vector2(5.8f, 1.5f), HorrorEvent2D.HorrorStyle.SpawnThreat);
        CreateSpikes(props[5], new Vector2(8.7f, -1.85f));
        CreateLadder(props[4], new Vector2(12.2f, -1.1f));
        CreateKey(props[7], new Vector2(13.7f, 1.45f));
        CreateDoor(props[3], new Vector2(18f, -1.25f));
        CreateExit(props[10], new Vector2(19.35f, -1.15f));
    }

    private static void BuildEscapeLevel(Sprite terrain, Sprite[] props)
    {
        CreateTerrain("Terrain Start", terrain, new Vector2(-5f, -2.65f), new Vector2(8f, 0.8f));
        CreateTerrain("Terrain Middle", terrain, new Vector2(4f, -2.65f), new Vector2(8f, 0.8f));
        CreateTerrain("Terrain Final", terrain, new Vector2(15f, -2.65f), new Vector2(12f, 0.8f));
        CreateOneWayTerrain("Escape Upper", terrain, new Vector2(12.5f, 0.4f), new Vector2(5f, 0.55f));
        CreateMovingPlatform("Escape Bridge", terrain, new Vector2(8.2f, -1f), new Vector2(2.2f, 0.45f), new Vector2(2.2f, 1.2f), 1.3f);

        CreateCrate(props[0], new Vector2(-4f, -1.55f));
        DeathTrap spikes = CreateSpikes(props[5], new Vector2(3.6f, -1.85f));
        CreatePressureSwitch(props[1], new Vector2(-2.2f, -2.12f), spikes);
        CreateCheckpoint(props[2], new Vector2(6.2f, -1.3f));
        CreateHidingSpot(props[9], new Vector2(9.5f, -1.05f));
        CreateEnemy("Threshold Demon", props[6], new Vector2(16.6f, -1.2f), EnemyAI2D.EnemyKind.GrotesqueDemon, 3f, 5.2f);
        CreateHorrorEvent("a", props[8], new Vector2(7.4f, 1.2f), HorrorEvent2D.HorrorStyle.CameraPulse);
        CreateHorrorEvent("b", props[9], new Vector2(17.8f, -0.7f), HorrorEvent2D.HorrorStyle.ForegroundRush);
        // Keep the moving saw away from the ladder so climbing never becomes impossible.
        DeathTrap saw = CreateSaw(props[6], new Vector2(15.8f, -1.15f), new Vector2(0f, 2.5f), 1.8f);
        CreateLever(props[11], new Vector2(12f, 1.2f), saw);
        CreateLadder(props[4], new Vector2(11f, -1.1f));
        CreateKey(props[7], new Vector2(14f, 1.35f));
        CreateDoor(props[3], new Vector2(19f, -1.25f));
        CreateExit(props[10], new Vector2(20.25f, -1.15f));
    }

    private static GameObject CreateTerrain(string name, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.layer = LayerMask.NameToLayer("Ground");
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = size;
        renderer.sortingOrder = 2;
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(size.x, size.y * 0.78f);
        collider.offset = new Vector2(0f, -size.y * 0.11f);
        return obj;
    }

    private static GameObject CreateMovingPlatform(string name, Sprite terrain, Vector2 position, Vector2 size, Vector2 offset, float speed)
    {
        GameObject platform = CreateTerrain(name, terrain, position, size);
        Rigidbody2D body = platform.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        MovingPlatform2D mover = platform.AddComponent<MovingPlatform2D>();
        mover.offset = offset;
        mover.speed = speed;
        return platform;
    }

    private static GameObject CreateOneWayTerrain(string name, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject platform = CreateTerrain(name, sprite, position, size);
        BoxCollider2D collider = platform.GetComponent<BoxCollider2D>();
        collider.usedByEffector = true;
        PlatformEffector2D effector = platform.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.useOneWayGrouping = true;
        effector.surfaceArc = 175f;
        effector.rotationalOffset = 0f;
        return platform;
    }

    private static GameObject CreateCrate(Sprite sprite, Vector2 position)
    {
        GameObject box = CreateSpriteObject("Push Box", sprite, position, new Vector2(0.72f, 0.72f));
        box.layer = LayerMask.NameToLayer("Ground");
        BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.35f, 1.35f);
        Rigidbody2D body = box.AddComponent<Rigidbody2D>();
        body.mass = 3.2f;
        body.linearDamping = 1.5f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        box.AddComponent<PushPullObject2D>();
        box.GetComponent<SpriteRenderer>().sortingOrder = 8;
        return box;
    }

    private static PressureSwitch2D CreatePressureSwitch(Sprite sprite, Vector2 position, DeathTrap target)
    {
        GameObject obj = CreateTriggerObject("Pressure Switch", sprite, position, new Vector2(0.72f, 0.45f));
        BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(1.65f, 0.6f);
        PressureSwitch2D pressure = obj.AddComponent<PressureSwitch2D>();
        pressure.targetTrap = target;
        pressure.indicator = obj.GetComponent<SpriteRenderer>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 7;
        return pressure;
    }

    private static void CreateCheckpoint(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateTriggerObject("Checkpoint", sprite, position, new Vector2(0.72f, 0.72f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(0.8f, 1.7f);
        obj.AddComponent<Checkpoint>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 7;
    }

    private static void CreateLadder(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateTriggerObject("Climb Zone", sprite, position, new Vector2(0.72f, 1.45f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(0.85f, 3.1f);
        obj.AddComponent<ClimbZone2D>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 6;
    }

    private static DeathTrap CreateSpikes(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateTriggerObject("Spike Trap", sprite, position, new Vector2(1.05f, 0.58f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(1.65f, 0.65f);
        DeathTrap trap = obj.AddComponent<DeathTrap>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 9;
        return trap;
    }

    private static DeathTrap CreateSaw(Sprite sprite, Vector2 position, Vector2 offset, float speed)
    {
        GameObject obj = CreateTriggerObject("Moving Saw", sprite, position, new Vector2(0.58f, 0.58f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(1.2f, 1.2f);
        DeathTrap trap = obj.AddComponent<DeathTrap>();
        SimpleMover2D mover = obj.AddComponent<SimpleMover2D>();
        mover.localOffset = offset;
        mover.speed = speed;
        obj.GetComponent<SpriteRenderer>().sortingOrder = 10;
        return trap;
    }

    private static void CreateLever(Sprite sprite, Vector2 position, DeathTrap target)
    {
        GameObject obj = CreateTriggerObject("Lever", sprite, position, new Vector2(0.52f, 0.52f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(1.25f, 1.45f);
        LeverSwitch2D lever = obj.AddComponent<LeverSwitch2D>();
        lever.targetTrap = target;
        lever.leverRenderer = obj.GetComponent<SpriteRenderer>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 9;
    }

    private static void CreateKey(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateTriggerObject("Key", sprite, position, new Vector2(0.3f, 0.3f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(1.8f, 1.8f);
        obj.AddComponent<CollectKey>();
        obj.AddComponent<CollectibleFloat2D>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 12;
    }

    private static void CreateDoor(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateSpriteObject("Locked Door", sprite, position, new Vector2(1.05f, 1.05f));
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.85f, 1.75f);
        obj.AddComponent<DoorGoal>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 11;
    }

    private static void CreateExit(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateTriggerObject("Finish Zone", sprite, position, new Vector2(1.05f, 1.05f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(0.9f, 1.8f);
        obj.AddComponent<FinishZone>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 8;
    }

    private static void CreateHidingSpot(Sprite sprite, Vector2 position)
    {
        if (creatureFrames != null && creatureFrames.Length > 2) sprite = creatureFrames[2];
        GameObject obj = CreateTriggerObject("Hiding Alcove", sprite, position, new Vector2(0.9f, 1.15f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(1.25f, 1.65f);
        obj.AddComponent<HidingSpot2D>();
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = 18;
        renderer.color = Color.white;
    }

    private static void CreateEnemy(string name, Sprite sprite, Vector2 position, EnemyAI2D.EnemyKind kind, float patrol, float chase)
    {
        if (creatureFrames != null && creatureFrames.Length > 1)
        {
            sprite = creatureFrames[kind == EnemyAI2D.EnemyKind.SorrowSoul ? 0 : 1];
        }
        Vector2 scale = kind == EnemyAI2D.EnemyKind.SorrowSoul ? new Vector2(0.72f, 1.05f) : new Vector2(1.05f, 1.2f);
        GameObject obj = CreateTriggerObject(name, sprite, position, scale);
        obj.GetComponent<BoxCollider2D>().size = new Vector2(0.75f, 1.35f);
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = 17;
        renderer.color = Color.white;
        EnemyAI2D ai = obj.AddComponent<EnemyAI2D>();
        ai.kind = kind;
        ai.patrolDistance = patrol;
        ai.patrolSpeed = kind == EnemyAI2D.EnemyKind.SorrowSoul ? 1.15f : 1.7f;
        ai.chaseSpeed = chase;
        ai.viewDistance = kind == EnemyAI2D.EnemyKind.SorrowSoul ? 5.6f : 7.2f;
        ai.searchTime = kind == EnemyAI2D.EnemyKind.SorrowSoul ? 2f : 3.4f;
        ai.obstacleMask = LayerMask.GetMask("Ground");
    }

    private static void CreateHorrorEvent(string id, Sprite sprite, Vector2 position, HorrorEvent2D.HorrorStyle style)
    {
        if (creatureFrames != null && creatureFrames.Length > 3) sprite = creatureFrames[3];
        GameObject reveal = CreateSpriteObject("Horror Manifestation " + id, sprite, position + new Vector2(1.2f, 1.5f), new Vector2(1.35f, 1.35f));
        SpriteRenderer revealRenderer = reveal.GetComponent<SpriteRenderer>();
        revealRenderer.sortingOrder = style == HorrorEvent2D.HorrorStyle.ForegroundRush ? 80 : -4;
        revealRenderer.color = Color.white;

        GameObject trigger = new GameObject("Horror Event " + id);
        trigger.transform.position = position;
        BoxCollider2D collider = trigger.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.2f, 6f);
        HorrorEvent2D horror = trigger.AddComponent<HorrorEvent2D>();
        horror.eventId = id;
        horror.style = style;
        horror.revealObject = reveal;
        reveal.SetActive(false);
    }

    private static void AddHospitalClues(Sprite[] props, int level)
    {
        string[] names = { "Bent IV Stand", "Pulse Cable", "Hospital Wristband", "Defibrillator Plate", "White Bed Rail" };
        Vector2[] positions =
        {
            new Vector2(-1f, -1.25f), new Vector2(3.5f, -1.9f), new Vector2(8.5f, 0.72f),
            new Vector2(12.8f, -1.3f), new Vector2(18.8f, -1.15f)
        };
        Sprite clueSprite = hospitalFrames != null && hospitalFrames.Length >= level
            ? hospitalFrames[level - 1]
            : props[(level + 7) % props.Length];
        float clueScale = level == 1 ? 0.78f : 0.58f;
        GameObject clue = CreateSpriteObject(names[level - 1], clueSprite, positions[level - 1], new Vector2(clueScale, clueScale));
        SpriteRenderer renderer = clue.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = -5;
        renderer.color = level == 5
            ? new Color(0.9f, 0.95f, 0.9f, 0.75f)
            : new Color(0.55f, 0.58f, 0.6f, 0.32f);

        string[] echoes =
        {
            "No podia seguir.",
            "Prometi que volveria.",
            "Sus voces quedaron arriba.",
            "El peso no era castigo. Era miedo.",
            "Todavia me esperan."
        };
        GameObject echoZone = new GameObject("Memory Echo " + level);
        echoZone.transform.position = new Vector2(positions[level - 1].x, -0.65f);
        BoxCollider2D collider = echoZone.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(2.4f, 4.2f);
        NarrativeEcho2D echo = echoZone.AddComponent<NarrativeEcho2D>();
        echo.message = echoes[level - 1];
    }

    private static void CreateDecoration(string name, Sprite sprite, Vector2 position, float scale, int order)
    {
        GameObject obj = CreateSpriteObject(name, sprite, position, new Vector2(scale, scale));
        obj.GetComponent<SpriteRenderer>().sortingOrder = order;
    }

    [MenuItem("Tools/UMBRA/Validate Complete Game")]
    public static void ValidateProject()
    {
        var errors = new List<string>();

        if (EditorBuildSettings.scenes.Length != ScenePaths.Length)
        {
            errors.Add("Build Settings must contain exactly five UMBRA chapters.");
        }

        for (int i = 0; i < ScenePaths.Length; i++)
        {
            string scenePath = ScenePaths[i];
            if (!File.Exists(scenePath))
            {
                errors.Add("Missing level scene: " + scenePath);
                continue;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            string prefix = "Level " + (i + 1) + ": ";
            ValidateObject<GameManager>("GameManager", prefix, errors);
            ValidateObject<UmbraAudio>("Audio Ambiente", prefix, errors);
            ValidateObject<PlayerController2D>("Player", prefix, errors);
            ValidateObject<ShadowDash2D>("Player", prefix, errors);
            ValidateObject<PlayerRespawn>("Player", prefix, errors);
            ValidateObject<PlayerSpriteAnimator>("Player", prefix, errors);
            ValidateObject<CameraFollow2D>("Main Camera", prefix, errors);
            int parallaxLayers = Object.FindObjectsByType<ParallaxLayer2D>().Length;
            if (parallaxLayers < 5)
            {
                errors.Add(prefix + "needs at least five parallax layers, found " + parallaxLayers + ".");
            }
            ValidateObject<Checkpoint>("Checkpoint", prefix, errors);
            ValidateObject<CollectKey>("Key", prefix, errors);
            ValidateObject<DoorGoal>("Locked Door", prefix, errors);
            ValidateObject<FinishZone>("Finish Zone", prefix, errors);
            ValidateObject<HidingSpot2D>("Hiding Alcove", prefix, errors);
            ValidateObject<NarrativeEcho2D>("Memory Echo " + (i + 1), prefix, errors);
            ValidateVisibleSprite("Terrain Start", prefix, errors);
            ValidateVisibleSprite("Key", prefix, errors);
            ValidateVisibleSprite("Locked Door", prefix, errors);
            ValidateVisibleSprite("Finish Zone", prefix, errors);

            PlayerController2D player = Object.FindAnyObjectByType<PlayerController2D>();
            BoxCollider2D playerCollider = player != null ? player.GetComponent<BoxCollider2D>() : null;
            if (player == null || player.groundCheck == null || player.groundLayer.value == 0)
            {
                errors.Add(prefix + "player ground detection is not configured.");
            }

            ClimbZone2D climbZone = Object.FindAnyObjectByType<ClimbZone2D>();
            PlatformEffector2D climbPlatform = Object.FindObjectsByType<PlatformEffector2D>()
                .OrderBy(effector => Mathf.Abs(effector.transform.position.x - climbZone.transform.position.x))
                .FirstOrDefault();
            Collider2D climbCollider = climbZone != null ? climbZone.GetComponent<Collider2D>() : null;
            Collider2D platformCollider = climbPlatform != null ? climbPlatform.GetComponent<Collider2D>() : null;
            if (climbZone == null || climbPlatform == null || climbCollider == null || platformCollider == null ||
                Mathf.Abs(climbZone.transform.position.x - climbPlatform.transform.position.x) > platformCollider.bounds.extents.x ||
                climbCollider.bounds.max.y < platformCollider.bounds.max.y + 0.2f)
            {
                errors.Add(prefix + "ladder does not pass safely through its upper platform.");
            }

            HidingSpot2D hidingSpot = Object.FindAnyObjectByType<HidingSpot2D>();
            Physics2D.SyncTransforms();
            RaycastHit2D groundBelowHiding = hidingSpot != null
                ? Physics2D.Raycast(hidingSpot.transform.position, Vector2.down, 3f, LayerMask.GetMask("Ground"))
                : default;
            if (hidingSpot == null || groundBelowHiding.collider == null)
            {
                errors.Add(prefix + "hiding spot has no stable ground underneath.");
            }

            foreach (string supportedObject in new[] { "Player", "Checkpoint", "Hiding Alcove", "Key", "Locked Door", "Finish Zone" })
            {
                ValidateGroundSupport(supportedObject, prefix, errors);
            }
            ValidateGroundSupport("Lever", prefix, errors, false);
            ValidateGroundSupport("Pressure Switch", prefix, errors, false);
            ValidateHorizontalRoute(prefix, errors);
            ValidateLadderHazards(climbZone, climbCollider, prefix, errors);

            if (playerCollider == null || playerCollider.sharedMaterial == null || playerCollider.sharedMaterial.friction > 0.01f)
            {
                errors.Add(prefix + "player requires zero friction.");
            }

            PushPullObject2D pushBox = Object.FindAnyObjectByType<PushPullObject2D>();
            Rigidbody2D pushBoxBody = pushBox != null ? pushBox.GetComponent<Rigidbody2D>() : null;
            if ((i == 0 || i == 2 || i == 4) && pushBox == null)
            {
                errors.Add(prefix + "push box is missing.");
            }

            if (pushBox != null &&
                (pushBoxBody == null || pushBoxBody.mass < 3f || pushBoxBody.linearDamping < 1f ||
                 pushBox.pushSpeed < 3.2f || pushBox.maxHorizontalSpeed < pushBox.pushSpeed ||
                 pushBox.maxHorizontalSpeed > 4.5f || pushBox.acceleration < 25f || pushBox.braking <= 0f))
            {
                errors.Add(prefix + "push box control is not configured.");
            }

            PlayerSpriteAnimator animator = Object.FindAnyObjectByType<PlayerSpriteAnimator>();
            if (animator == null || animator.idleFrames.Length != 4 || animator.runFrames.Length != 4 ||
                animator.jumpFrames.Length != 2 || animator.crouchFrames.Length != 2 || animator.framesPerSecond < 10f)
            {
                errors.Add(prefix + "player animation setup is incomplete.");
            }

            Camera camera = Camera.main;
            if (camera == null || !camera.orthographic || camera.transform.position.z > -1f)
            {
                errors.Add(prefix + "main camera is invalid.");
            }

            if (camera == null || camera.GetComponent<AudioListener>() == null)
            {
                errors.Add(prefix + "main camera requires an AudioListener.");
            }

            if (Object.FindObjectsByType<AudioListener>().Length != 1)
            {
                errors.Add(prefix + "scene requires exactly one AudioListener.");
            }

            if (Object.FindObjectsByType<DeathTrap>().Length < 2)
            {
                errors.Add(prefix + "needs at least one visible danger plus the fall detector.");
            }

            EnemyAI2D enemy = Object.FindAnyObjectByType<EnemyAI2D>();
            if (enemy == null || enemy.chaseSpeed <= enemy.patrolSpeed || enemy.viewDistance <= 0f || enemy.obstacleMask.value == 0)
            {
                errors.Add(prefix + "enemy AI is missing or misconfigured.");
            }
            else
            {
                string enemyArt = AssetDatabase.GetAssetPath(enemy.GetComponent<SpriteRenderer>().sprite);
                if (!enemyArt.Contains("/Creatures/Frames/")) errors.Add(prefix + "enemy still uses prop placeholder art.");
            }

            int horrorCount = Object.FindObjectsByType<HorrorEvent2D>().Length;
            int expectedHorrorCount = i == 4 ? 2 : 1;
            if (horrorCount != expectedHorrorCount)
            {
                errors.Add(prefix + "expected " + expectedHorrorCount + " horror events, found " + horrorCount + ".");
            }

            GameObject hospitalClue = GameObject.Find(new[] { "Bent IV Stand", "Pulse Cable", "Hospital Wristband", "Defibrillator Plate", "White Bed Rail" }[i]);
            string clueArt = hospitalClue != null && hospitalClue.GetComponent<SpriteRenderer>() != null
                ? AssetDatabase.GetAssetPath(hospitalClue.GetComponent<SpriteRenderer>().sprite)
                : string.Empty;
            if (!clueArt.Contains("/Hospital/Frames/")) errors.Add(prefix + "hospital clue art is missing.");

            ValidatePropSprite("Key", prefix, errors);
            ValidatePropSprite("Locked Door", prefix, errors);
            ValidatePropSprite("Finish Zone", prefix, errors);
        }

        EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
        if (errors.Count > 0)
        {
            throw new System.Exception("UMBRA validation failed:\n- " + string.Join("\n- ", errors));
        }

        Debug.Log("UMBRA VALIDATION PASSED: all five chapters and their main gameplay objects are configured.");
    }

    public static void RebuildAndValidate()
    {
        BuildScene();
        ValidateProject();
    }

    public static void RebuildValidateAndCapture()
    {
        RebuildAndValidate();
        if (!Application.isBatchMode)
        {
            CaptureAllPreviews();
        }

        File.WriteAllText(MarkerPath, SetupVersion + "\n");
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/UMBRA/Open Chapter 2")]
    public static void OpenChapter2ForEditing()
    {
        EditorSceneManager.OpenScene(ScenePaths[1], OpenSceneMode.Single);
        Debug.Log("UMBRA: Chapter 2 opened for editing.");
    }

    [MenuItem("Tools/UMBRA/Capture All Chapter Previews")]
    public static void CaptureAllPreviews()
    {
        Directory.CreateDirectory("Logs/LevelPreviews");
        for (int i = 0; i < ScenePaths.Length; i++)
        {
            EditorSceneManager.OpenScene(ScenePaths[i], OpenSceneMode.Single);
            CaptureCamera("Logs/LevelPreviews/Level_" + (i + 1).ToString("00") + ".png");
            Camera camera = Camera.main;
            camera.transform.position = new Vector3(i == 4 ? 16f : 11.5f, 0.5f, -10f);
            CaptureCamera("Logs/LevelPreviews/Chapter_" + (i + 1).ToString("00") + "_Mid.png");
        }

        EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
        Debug.Log("UMBRA PREVIEWS PASSED: all five chapter cameras rendered correctly.");
    }

    private static void CaptureCamera(string outputPath)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            throw new System.Exception("Preview failed: camera is missing.");
        }

        const int width = 960;
        const int height = 540;
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        Texture2D preview = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture previous = RenderTexture.active;
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        preview.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        preview.Apply();

        Color32 background = preview.GetPixel(0, 0);
        int differentPixels = 0;
        Color32[] pixels = preview.GetPixels32();
        for (int i = 0; i < pixels.Length; i += 20)
        {
            Color32 pixel = pixels[i];
            if (Mathf.Abs(pixel.r - background.r) > 8 || Mathf.Abs(pixel.g - background.g) > 8 || Mathf.Abs(pixel.b - background.b) > 8)
            {
                differentPixels++;
            }
        }

        File.WriteAllBytes(outputPath, preview.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = previous;
        Object.DestroyImmediate(preview);
        Object.DestroyImmediate(renderTexture);

        if (differentPixels < 100)
        {
            throw new System.Exception("Preview appears empty: " + outputPath);
        }
    }

    private static void ValidateObject<T>(string objectName, string prefix, List<string> errors) where T : Component
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null || obj.GetComponent<T>() == null)
        {
            errors.Add(prefix + objectName + " needs " + typeof(T).Name + ".");
        }
    }

    private static void ValidateGroundSupport(
        string objectName,
        string prefix,
        List<string> errors,
        bool required = true)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            if (required) errors.Add(prefix + objectName + " is missing for support validation.");
            return;
        }

        RaycastHit2D support = Physics2D.Raycast(
            obj.transform.position,
            Vector2.down,
            3.5f,
            LayerMask.GetMask("Ground"));
        if (support.collider == null)
        {
            errors.Add(prefix + objectName + " has no reachable ground underneath.");
        }
    }

    private static void ValidateHorizontalRoute(string prefix, List<string> errors)
    {
        GameObject player = GameObject.Find("Player");
        GameObject exit = GameObject.Find("Finish Zone");
        if (player == null || exit == null) return;

        var intervals = new List<Vector2>();
        foreach (Collider2D ground in Object.FindObjectsByType<Collider2D>())
        {
            if (ground.gameObject.layer != LayerMask.NameToLayer("Ground") || ground.isTrigger) continue;
            Bounds bounds = ground.bounds;
            float min = bounds.min.x;
            float max = bounds.max.x;
            MovingPlatform2D moving = ground.GetComponent<MovingPlatform2D>();
            if (moving != null)
            {
                min = Mathf.Min(min, min + moving.offset.x);
                max = Mathf.Max(max, max + moving.offset.x);
            }
            intervals.Add(new Vector2(min, max));
        }

        intervals = intervals.OrderBy(interval => interval.x).ToList();
        float coveredUntil = player.transform.position.x;
        const float maximumSafeGap = 3.25f;
        foreach (Vector2 interval in intervals)
        {
            if (interval.y < coveredUntil) continue;
            if (interval.x - coveredUntil > maximumSafeGap)
            {
                errors.Add(prefix + "main route contains an unsafe horizontal gap of " +
                    (interval.x - coveredUntil).ToString("0.00") + " units.");
                return;
            }
            coveredUntil = Mathf.Max(coveredUntil, interval.y);
            if (coveredUntil >= exit.transform.position.x) return;
        }

        if (coveredUntil < exit.transform.position.x)
        {
            errors.Add(prefix + "main route does not reach the finish zone safely.");
        }
    }

    private static void ValidateLadderHazards(
        ClimbZone2D climbZone,
        Collider2D climbCollider,
        string prefix,
        List<string> errors)
    {
        if (climbZone == null || climbCollider == null) return;

        foreach (DeathTrap trap in Object.FindObjectsByType<DeathTrap>())
        {
            SimpleMover2D mover = trap.GetComponent<SimpleMover2D>();
            Collider2D trapCollider = trap.GetComponent<Collider2D>();
            if (mover == null || trapCollider == null) continue;

            Bounds swept = trapCollider.bounds;
            Bounds moved = trapCollider.bounds;
            moved.center += (Vector3)mover.localOffset;
            swept.Encapsulate(moved);
            if (!swept.Intersects(climbCollider.bounds)) continue;

            LeverSwitch2D safetyLever = Object.FindObjectsByType<LeverSwitch2D>()
                .FirstOrDefault(lever => lever.targetTrap == trap);
            if (safetyLever == null || safetyLever.transform.position.x >= climbZone.transform.position.x - 0.75f)
            {
                errors.Add(prefix + "a moving trap blocks the ladder before its safety lever can be reached.");
            }
        }
    }

    private static void ValidateVisibleSprite(string objectName, string prefix, List<string> errors)
    {
        GameObject obj = GameObject.Find(objectName);
        SpriteRenderer renderer = obj != null ? obj.GetComponent<SpriteRenderer>() : null;
        if (renderer == null || renderer.sprite == null || !renderer.enabled)
        {
            errors.Add(prefix + objectName + " needs visible art.");
        }
    }

    private static void ValidatePropSprite(string objectName, string prefix, List<string> errors)
    {
        GameObject obj = GameObject.Find(objectName);
        SpriteRenderer renderer = obj != null ? obj.GetComponent<SpriteRenderer>() : null;
        string path = renderer != null && renderer.sprite != null ? AssetDatabase.GetAssetPath(renderer.sprite) : string.Empty;
        if (!path.Contains("/Props/Frames/"))
        {
            errors.Add(prefix + objectName + " is still using placeholder art.");
        }
    }

    private static Sprite CreateColorSprite(string name, Color color)
    {
        string path = "Assets/Art/" + name + ".png";
        if (!File.Exists(path))
        {
            Texture2D texture = new Texture2D(16, 16);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        return ImportSingleSprite(path, 16f, false);
    }

    private static Sprite CreateTerrainSprite()
    {
        const int width = 256;
        const int height = 64;
        const string path = "Assets/Art/terrain_forest_tile.png";
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color soil = new Color(0.055f, 0.055f, 0.065f, 1f);
        Color edge = new Color(0.16f, 0.16f, 0.18f, 1f);
        var random = new System.Random(UMBRAHash());

        for (int x = 0; x < width; x++)
        {
            int top = 48 + (int)(Mathf.Sin(x * 0.17f) * 3f) + random.Next(-2, 3);
            for (int y = 0; y < height; y++)
            {
                if (y > top)
                {
                    texture.SetPixel(x, y, clear);
                }
                else if (y > top - 4)
                {
                    texture.SetPixel(x, y, edge);
                }
                else
                {
                    float variation = random.Next(-5, 6) / 255f;
                    texture.SetPixel(x, y, new Color(soil.r + variation, soil.g + variation, soil.b + variation, 1f));
                }
            }
        }

        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        return ImportSingleSprite(path, 64f, true);
    }

    private static int UMBRAHash()
    {
        return 8217;
    }

    private static Sprite LoadSpriteAsset(string path, float pixelsPerUnit)
    {
        if (!File.Exists(path))
        {
            throw new System.Exception("Required art asset is missing: " + path);
        }

        return ImportSingleSprite(path, pixelsPerUnit, false);
    }

    private static Sprite[] CreateSheetFrames(
        string sheetPath,
        string framesFolder,
        string filePrefix,
        int columns,
        int rows,
        float worldHeight)
    {
        if (!File.Exists(sheetPath))
        {
            throw new System.Exception("Spritesheet is missing: " + sheetPath);
        }

        Texture2D sheet = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        sheet.LoadImage(File.ReadAllBytes(sheetPath));
        int cellWidth = sheet.width / columns;
        int cellHeight = sheet.height / rows;
        Directory.CreateDirectory(framesFolder);
        Sprite[] frames = new Sprite[columns * rows];
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = (row * columns) + column;
                int sourceY = sheet.height - ((row + 1) * cellHeight);
                Texture2D frame = new Texture2D(cellWidth, cellHeight, TextureFormat.RGBA32, false);
                frame.SetPixels(sheet.GetPixels(column * cellWidth, sourceY, cellWidth, cellHeight));
                frame.Apply();
                string framePath = framesFolder + "/" + filePrefix + "_" + index.ToString("00") + ".png";
                File.WriteAllBytes(framePath, frame.EncodeToPNG());
                Object.DestroyImmediate(frame);
                frames[index] = ImportSingleSprite(framePath, cellHeight / worldHeight, false);
            }
        }

        Object.DestroyImmediate(sheet);
        return frames;
    }

    private static Sprite ImportSingleSprite(string path, float pixelsPerUnit, bool tiled)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.wrapMode = tiled ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        var importerSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(importerSettings);
        importerSettings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(importerSettings);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static PhysicsMaterial2D CreateNoFrictionMaterial()
    {
        const string path = "Assets/Art/Player_NoFriction.physicsMaterial2D";
        PhysicsMaterial2D material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (material == null)
        {
            material = new PhysicsMaterial2D("Player No Friction");
            AssetDatabase.CreateAsset(material, path);
        }

        material.friction = 0f;
        material.bounciness = 0f;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject CreateSpriteObject(string name, Sprite sprite, Vector2 position, Vector2 scale)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        return obj;
    }

    private static GameObject CreateTriggerObject(string name, Sprite sprite, Vector2 position, Vector2 scale)
    {
        GameObject obj = CreateSpriteObject(name, sprite, position, scale);
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        return obj;
    }

    private static void EnsureLayer(string layerName, int preferredIndex)
    {
        if (LayerMask.NameToLayer(layerName) != -1)
        {
            return;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        SerializedProperty preferred = layers.GetArrayElementAtIndex(preferredIndex);
        if (string.IsNullOrEmpty(preferred.stringValue))
        {
            preferred.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            return;
        }

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }

        throw new System.Exception("No free layer slot found for " + layerName);
    }
}
