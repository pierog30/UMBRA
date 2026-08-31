using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class UmbraPrototypeBuilder
{
    private const string MarkerPath = "Assets/UMBRA_SETUP_DONE.txt";
    private const string SetupVersion = "UMBRA Archive of Echoes build v19";
    private const float MainGroundY = -2.65f;
    private const float MainGroundHeight = 0.8f;
    private const float MainSurfaceY = MainGroundY + (MainGroundHeight * 0.28f);
    private const float PlayerGroundOffset = 0.74f;
    private const float MinimumGoalX = 118f;
    private const float RouteWidthForgiveness = 3.2f;
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Level_01_Forest.unity",
        "Assets/Scenes/Level_02_Ruins.unity",
        "Assets/Scenes/Level_03_Factory.unity",
        "Assets/Scenes/Level_04_Caverns.unity",
        "Assets/Scenes/Level_05_Escape.unity"
    };
    private static readonly Color[] LevelColors =
    {
        new Color(0.12f, 0.55f, 0.56f),
        new Color(0.55f, 0.22f, 0.53f),
        new Color(0.70f, 0.38f, 0.14f),
        new Color(0.18f, 0.38f, 0.62f),
        new Color(0.35f, 0.24f, 0.64f)
    };
    private static Sprite highlightSprite;

    [InitializeOnLoadMethod]
    private static void AutoBuildOnce()
    {
        EditorApplication.delayCall += () =>
        {
            if (File.Exists(MarkerPath) && File.ReadAllText(MarkerPath).Trim() == SetupVersion)
            {
                return;
            }

            RebuildValidateAndCapture();
        };
    }

    [MenuItem("Tools/UMBRA/Rebuild All Five Levels")]
    public static void BuildScene()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Art");
        EnsureLayer("Ground", 6);
        ConfigureWindowsPlayer();

        Sprite hidden = CreateColorSprite("hidden_square", new Color(0.02f, 0.02f, 0.025f, 1f));
        Sprite paper = CreateColorSprite("paper_square", new Color(0.95f, 0.9f, 0.72f, 1f));
        highlightSprite = paper;
        Sprite[] backgrounds =
        {
            LoadSpriteAsset("Assets/Art/Backgrounds/echo_garden.png", 100f),
            LoadSpriteAsset("Assets/Art/Backgrounds/echo_letters_city.png", 100f),
            LoadSpriteAsset("Assets/Art/Backgrounds/echo_clockwork.png", 100f),
            LoadSpriteAsset("Assets/Art/Backgrounds/echo_rain_library.png", 100f),
            LoadSpriteAsset("Assets/Art/Backgrounds/echo_observatory.png", 100f)
        };
        Sprite terrain = CreateTerrainSprite();
        Sprite[] characterFrames = CreateSheetFrames(
            "Assets/Art/Character/lumo_character_sheet.png",
            "Assets/Art/Character/LumoFrames",
            "lumo",
            4,
            3,
            1.7f,
            false);
        Sprite[] props = CreateSheetFrames(
            "Assets/Art/Props/echo_props_sheet.png",
            "Assets/Art/Props/EchoFrames",
            "echo_prop",
            4,
            3,
            2f,
            true);
        PhysicsMaterial2D noFriction = CreateNoFrictionMaterial();

        for (int level = 1; level <= ScenePaths.Length; level++)
        {
            BuildLevel(level, backgrounds[level - 1], terrain, hidden, paper, characterFrames, props, noFriction);
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
        Debug.Log("UMBRA: El Archivo de los Ecos created with five memory chapters.");
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
        Sprite backdrop,
        Sprite terrain,
        Sprite hidden,
        Sprite paper,
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
        camera.backgroundColor = Color.Lerp(LevelColors[level - 1], new Color(0.08f, 0.11f, 0.16f), 0.72f);

        for (int i = 0; i < 8; i++)
        {
            GameObject background = CreateSpriteObject(
                "Memory Backdrop " + (i + 1),
                backdrop,
                new Vector2(-7f + (i * 19f), -0.1f),
                new Vector2(1.15f, 1.15f));
            SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = -100;
            renderer.color = Color.white;
        }

        GameObject player = CreatePlayer(characterFrames, noFriction);
        CameraFollow2D follow = camera.gameObject.AddComponent<CameraFollow2D>();
        follow.target = player.transform;
        follow.smoothSpeed = 9f;
        follow.SnapToTarget();

        AddMemoryDetails(paper, props, level);

        switch (level)
        {
            case 1:
                BuildGardenLevel(terrain, props);
                break;
            case 2:
                BuildLettersLevel(terrain, props);
                break;
            case 3:
                BuildWorkshopLevel(terrain, props);
                break;
            case 4:
                BuildLibraryLevel(terrain, props);
                break;
            default:
                BuildObservatoryLevel(terrain, props);
                break;
        }

        GameObject killPlane = CreateTriggerObject("Lost Memory Fall", hidden, new Vector2(500f, -7f), new Vector2(3000f, 1f));
        killPlane.GetComponent<SpriteRenderer>().enabled = false;
        killPlane.AddComponent<DeathTrap>();

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePaths[level - 1]);
    }

    private static GameObject CreatePlayer(Sprite[] frames, PhysicsMaterial2D noFriction)
    {
        GameObject player = CreateSpriteObject(
            "Player",
            frames[0],
            new Vector2(-7f, MainSurfaceY + PlayerGroundOffset),
            Vector2.one);
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

    private static void AddMemoryDetails(Sprite paper, Sprite[] props, int level)
    {
        Color chapterColor = LevelColors[level - 1];
        SpriteRenderer ribbonA = CreateSpriteObject("Memory Ribbon A", paper, new Vector2(52f, 2f), new Vector2(125f, 0.10f)).GetComponent<SpriteRenderer>();
        ribbonA.color = new Color(chapterColor.r, chapterColor.g, chapterColor.b, 0.16f);
        ribbonA.sortingOrder = -20;
        SpriteRenderer ribbonB = CreateSpriteObject("Memory Ribbon B", paper, new Vector2(72f, 2.8f), new Vector2(118f, 0.07f)).GetComponent<SpriteRenderer>();
        ribbonB.color = new Color(0.96f, 0.64f, 0.28f, 0.12f);
        ribbonB.sortingOrder = -20;

        CreateDecoration("Memory Tablet A", props[8], new Vector2(7f, MainSurfaceY), 1.05f, -8);
        CreateDecoration("Memory Tablet B", props[8], new Vector2(48f, MainSurfaceY), 0.9f, -8);
        CreateDecoration("Memory Tablet C", props[8], new Vector2(96f, MainSurfaceY), 0.78f, -8);
    }

    private static void BuildGardenLevel(Sprite terrain, Sprite[] props)
    {
        CreateGroundRoute(terrain, "Garden", new[]
        {
            new Vector2(-3.5f, 11f), new Vector2(8f, 8f), new Vector2(19f, 8f),
            new Vector2(30f, 10f), new Vector2(43f, 12f), new Vector2(57f, 10f),
            new Vector2(71f, 12f), new Vector2(86f, 12f), new Vector2(104f, 20f),
            new Vector2(123f, 14f)
        });
        CreateTerrain("Garden Voice Path", terrain, new Vector2(19f, 0f), new Vector2(7f, 0.55f));
        CreateTerrain("Garden Ribbon Path", terrain, new Vector2(47f, 0.2f), new Vector2(8f, 0.55f));
        CreateTerrain("Garden Echo Path", terrain, new Vector2(104f, 0.15f), new Vector2(11f, 0.55f));

        DeathTrap firstSpikes = CreateSpikes(props[5], new Vector2(8f, MainSurfaceY));
        CreateCrate(props[0], new Vector2(-3.2f, MainSurfaceY));
        CreatePressureSwitch(props[1], new Vector2(0f, MainSurfaceY), firstSpikes);
        CreateLadder(props[4], new Vector2(14.9f, MainSurfaceY));
        DeathTrap firstSaw = CreateSaw(props[6], new Vector2(30f, -1.3f), new Vector2(0f, 2.2f), 1.45f);
        CreateLever(props[11], new Vector2(19f, SurfaceY(0f, 0.55f)), firstSaw);
        CreateCheckpoint(props[2], new Vector2(28f, MainSurfaceY));

        DeathTrap secondSpikes = CreateSpikes(props[5], new Vector2(70f, MainSurfaceY));
        CreateCrate(props[0], new Vector2(55f, MainSurfaceY));
        CreatePressureSwitch(props[1], new Vector2(61f, MainSurfaceY), secondSpikes);
        DeathTrap secondSaw = CreateSaw(props[6], new Vector2(86f, -1.25f), new Vector2(2.3f, 0f), 1.35f);
        CreateLever(props[11], new Vector2(80f, MainSurfaceY), secondSaw);
        CreateCheckpoint(props[2], new Vector2(75f, MainSurfaceY));
        CreateLadder(props[4], new Vector2(97.9f, MainSurfaceY));
        CreateKey(props[7], new Vector2(104f, 1.25f));
        CreateDoor(props[3], new Vector2(123f, MainSurfaceY));
        CreateExit(props[10], new Vector2(128f, MainSurfaceY));
    }

    private static void BuildLettersLevel(Sprite terrain, Sprite[] props)
    {
        CreateGroundRoute(terrain, "Letters", new[]
        {
            new Vector2(-4f, 10f), new Vector2(10f, 8f), new Vector2(23f, 9f),
            new Vector2(36f, 11f), new Vector2(50f, 12f), new Vector2(65f, 12f),
            new Vector2(80f, 12f), new Vector2(96f, 13f), new Vector2(114f, 20f),
            new Vector2(130f, 10f)
        });
        CreateMovingPlatform("Letter Bridge A", terrain, new Vector2(3f, -1.45f), new Vector2(2.2f, 0.45f), new Vector2(3f, 0f), 1.05f);
        CreateMovingPlatform("Envelope Lift", terrain, new Vector2(42.5f, -2f), new Vector2(2.2f, 0.45f), new Vector2(0f, 2.7f), 1.05f);
        CreateMovingPlatform("Letter Bridge B", terrain, new Vector2(72.5f, -1.2f), new Vector2(2.3f, 0.45f), new Vector2(3f, 0.8f), 0.95f);
        CreateTerrain("Unsent Letters Hall", terrain, new Vector2(50f, 0.3f), new Vector2(10f, 0.55f));
        CreateTerrain("Letter Echo Ledge", terrain, new Vector2(108f, 0.25f), new Vector2(10f, 0.55f));

        CreateSpikes(props[5], new Vector2(10f, MainSurfaceY));
        CreateSaw(props[6], new Vector2(23f, -1.25f), new Vector2(2.2f, 0f), 1.35f);
        CreateCheckpoint(props[2], new Vector2(34f, MainSurfaceY));
        CreateLadder(props[4], new Vector2(44.4f, MainSurfaceY));
        DeathTrap upperSaw = CreateSaw(props[6], new Vector2(59f, -1.2f), new Vector2(0f, 2.4f), 1.3f);
        CreateLever(props[11], new Vector2(50f, SurfaceY(0.3f, 0.55f)), upperSaw);
        CreateCheckpoint(props[2], new Vector2(70f, MainSurfaceY));
        DeathTrap lateSpikes = CreateSpikes(props[5], new Vector2(80f, MainSurfaceY));
        CreateLever(props[11], new Vector2(75f, MainSurfaceY), lateSpikes);
        CreateCheckpoint(props[2], new Vector2(90f, MainSurfaceY));
        CreateLadder(props[4], new Vector2(102.4f, MainSurfaceY));
        CreateKey(props[7], new Vector2(108f, 1.35f));
        CreateSaw(props[6], new Vector2(116f, -1.2f), new Vector2(2f, 0f), 1.45f);
        CreateDoor(props[3], new Vector2(126f, MainSurfaceY));
        CreateExit(props[10], new Vector2(130f, MainSurfaceY));
    }

    private static void BuildWorkshopLevel(Sprite terrain, Sprite[] props)
    {
        CreateGroundRoute(terrain, "Workshop", new[]
        {
            new Vector2(-3.5f, 11f), new Vector2(9f, 9f), new Vector2(22f, 9f),
            new Vector2(35f, 10f), new Vector2(49f, 11f), new Vector2(64f, 11f),
            new Vector2(79f, 11f), new Vector2(94f, 11f), new Vector2(110f, 15f),
            new Vector2(127f, 15f)
        });
        CreateTerrain("Workshop Low Passage A", terrain, new Vector2(-2.5f, -0.5f), new Vector2(5f, 0.7f));
        CreateTerrain("Workshop Clockwalk A", terrain, new Vector2(30f, 0.25f), new Vector2(9f, 0.5f));
        CreateTerrain("Workshop Low Passage B", terrain, new Vector2(52f, -0.45f), new Vector2(6f, 0.7f));
        CreateTerrain("Workshop Clockwalk B", terrain, new Vector2(94f, 0.3f), new Vector2(10f, 0.5f));
        CreateMovingPlatform("Borrowed Hour Lift", terrain, new Vector2(72f, -2f), new Vector2(2.2f, 0.45f), new Vector2(0f, 2.8f), 1.15f);

        DeathTrap firstSpikes = CreateSpikes(props[5], new Vector2(9f, MainSurfaceY));
        CreateCrate(props[0], new Vector2(-2f, MainSurfaceY));
        CreatePressureSwitch(props[1], new Vector2(2f, MainSurfaceY), firstSpikes);
        DeathTrap sawA = CreateSaw(props[6], new Vector2(35f, -1.25f), new Vector2(2.4f, 0f), 1.55f);
        CreateLadder(props[4], new Vector2(24.9f, MainSurfaceY));
        CreateLever(props[11], new Vector2(30f, SurfaceY(0.25f, 0.5f)), sawA);
        CreateCheckpoint(props[2], new Vector2(38.5f, MainSurfaceY));

        DeathTrap secondSpikes = CreateSpikes(props[5], new Vector2(79f, MainSurfaceY));
        CreateCrate(props[0], new Vector2(62f, MainSurfaceY));
        CreatePressureSwitch(props[1], new Vector2(68f, MainSurfaceY), secondSpikes);
        CreateCheckpoint(props[2], new Vector2(91f, MainSurfaceY));
        CreateLadder(props[4], new Vector2(88.4f, MainSurfaceY));
        CreateSaw(props[6], new Vector2(106f, -1.2f), new Vector2(2.5f, 0f), 1.45f);
        CreateKey(props[7], new Vector2(96f, 1.4f));
        CreateDoor(props[3], new Vector2(124f, MainSurfaceY));
        CreateExit(props[10], new Vector2(130f, MainSurfaceY));
    }

    private static void BuildLibraryLevel(Sprite terrain, Sprite[] props)
    {
        CreateGroundRoute(terrain, "Library", new[]
        {
            new Vector2(-4.5f, 12f), new Vector2(10f, 12f), new Vector2(24f, 10f),
            new Vector2(38f, 12f), new Vector2(52f, 12f), new Vector2(67f, 11f),
            new Vector2(82f, 10f), new Vector2(97f, 11f), new Vector2(113f, 15f),
            new Vector2(130f, 15f)
        });
        CreateMovingPlatform("Book Lift A", terrain, new Vector2(2.5f, -2f), new Vector2(2.2f, 0.45f), new Vector2(0f, 2.8f), 1.05f);
        CreateMovingPlatform("Rain Bridge A", terrain, new Vector2(17f, -1.1f), new Vector2(2.2f, 0.45f), new Vector2(3.2f, 0.8f), 0.95f);
        CreateMovingPlatform("Book Lift B", terrain, new Vector2(45f, -2f), new Vector2(2.2f, 0.45f), new Vector2(0f, 3f), 1.1f);
        CreateMovingPlatform("Rain Bridge B", terrain, new Vector2(74.5f, -1.1f), new Vector2(2.2f, 0.45f), new Vector2(3f, 0.9f), 0.9f);
        CreateMovingPlatform("Book Lift C", terrain, new Vector2(104f, -2f), new Vector2(2.2f, 0.45f), new Vector2(0f, 3f), 1.15f);
        CreateTerrain("Library Shelf A", terrain, new Vector2(24f, 0.35f), new Vector2(8f, 0.5f));
        CreateTerrain("Library Shelf B", terrain, new Vector2(82f, 0.4f), new Vector2(8f, 0.5f));
        CreateTerrain("Library Echo Ledge", terrain, new Vector2(113f, 0.45f), new Vector2(9f, 0.5f));

        CreateSaw(props[6], new Vector2(10f, -1.25f), new Vector2(2f, 0f), 1.35f);
        CreateLadder(props[4], new Vector2(19.4f, MainSurfaceY));
        CreateSpikes(props[5], new Vector2(27f, SurfaceY(0.35f, 0.5f)));
        CreateCheckpoint(props[2], new Vector2(36f, MainSurfaceY));
        CreateSpikes(props[5], new Vector2(67f, MainSurfaceY));
        CreateCheckpoint(props[2], new Vector2(92f, MainSurfaceY));
        CreateLadder(props[4], new Vector2(107.9f, MainSurfaceY));
        CreateKey(props[7], new Vector2(113f, 1.55f));
        CreateSaw(props[6], new Vector2(119f, -1.2f), new Vector2(2.2f, 0f), 1.45f);
        CreateDoor(props[3], new Vector2(127f, MainSurfaceY));
        CreateExit(props[10], new Vector2(131f, MainSurfaceY));
    }

    private static void BuildObservatoryLevel(Sprite terrain, Sprite[] props)
    {
        CreateGroundRoute(terrain, "Observatory", new[]
        {
            new Vector2(-3.5f, 11f), new Vector2(9f, 9f), new Vector2(22f, 9f),
            new Vector2(35f, 10f), new Vector2(49f, 10f), new Vector2(63f, 10f),
            new Vector2(77f, 10f), new Vector2(92f, 12f), new Vector2(109f, 16f),
            new Vector2(128f, 16f)
        });
        CreateTerrain("Observatory Gallery A", terrain, new Vector2(27f, 0.2f), new Vector2(8f, 0.55f));
        CreateTerrain("Observatory Gallery B", terrain, new Vector2(70f, 0.25f), new Vector2(9f, 0.55f));
        CreateTerrain("Returning Echo Route", terrain, new Vector2(108f, 0.3f), new Vector2(11f, 0.55f));
        CreateMovingPlatform("Constellation Bridge", terrain, new Vector2(15.5f, -1.1f), new Vector2(2.2f, 0.45f), new Vector2(2.8f, 1f), 1.05f);
        CreateMovingPlatform("Observatory Lift", terrain, new Vector2(84.5f, -2f), new Vector2(2.2f, 0.45f), new Vector2(0f, 3f), 1.1f);

        DeathTrap spikesA = CreateSpikes(props[5], new Vector2(9f, MainSurfaceY));
        CreateCrate(props[0], new Vector2(-3f, MainSurfaceY));
        CreatePressureSwitch(props[1], new Vector2(1f, MainSurfaceY), spikesA);
        DeathTrap sawA = CreateSaw(props[6], new Vector2(35f, -1.2f), new Vector2(0f, 2.5f), 1.45f);
        CreateLadder(props[4], new Vector2(22.4f, MainSurfaceY));
        CreateLever(props[11], new Vector2(27f, SurfaceY(0.2f, 0.55f)), sawA);
        CreateCheckpoint(props[2], new Vector2(47f, MainSurfaceY));

        DeathTrap spikesB = CreateSpikes(props[5], new Vector2(77f, MainSurfaceY));
        CreateCrate(props[0], new Vector2(61f, MainSurfaceY));
        CreatePressureSwitch(props[1], new Vector2(66f, MainSurfaceY), spikesB);
        CreateCheckpoint(props[2], new Vector2(72f, MainSurfaceY));
        CreateSaw(props[6], new Vector2(92f, -1.2f), new Vector2(2.3f, 0f), 1.45f);
        CreateCheckpoint(props[2], new Vector2(95f, MainSurfaceY));
        CreateLadder(props[4], new Vector2(101.9f, MainSurfaceY));
        CreateKey(props[7], new Vector2(108f, 1.4f));
        CreateSaw(props[6], new Vector2(116f, -1.15f), new Vector2(2.2f, 0f), 1.55f);
        CreateDoor(props[3], new Vector2(126f, MainSurfaceY));
        CreateExit(props[10], new Vector2(132f, MainSurfaceY));
    }

    private static void CreateGroundRoute(Sprite terrain, string prefix, Vector2[] sections)
    {
        for (int i = 0; i < sections.Length; i++)
        {
            Vector2 section = sections[i];
            string name = i == 0 ? "Terrain Start" : prefix + " Ground " + (i + 1);
            CreateTerrain(
                name,
                terrain,
                new Vector2(section.x, MainGroundY),
                new Vector2(section.y + RouteWidthForgiveness, MainGroundHeight));
        }
    }

    private static float SurfaceY(float centerY, float height)
    {
        return centerY + (height * 0.28f);
    }

    private static GameObject CreateGroundedSpriteObject(
        string name,
        Sprite sprite,
        float x,
        float surfaceY,
        Vector2 scale)
    {
        GameObject obj = CreateSpriteObject(name, sprite, new Vector2(x, 0f), scale);
        float scaledBottom = sprite.bounds.min.y * scale.y;
        obj.transform.position = new Vector3(x, surfaceY - scaledBottom, 0f);
        return obj;
    }

    private static void FitColliderFromBottom(
        BoxCollider2D collider,
        Sprite sprite,
        float widthFactor,
        float heightFactor)
    {
        Bounds bounds = sprite.bounds;
        collider.size = new Vector2(bounds.size.x * widthFactor, bounds.size.y * heightFactor);
        collider.offset = new Vector2(
            bounds.center.x,
            bounds.min.y + (collider.size.y * 0.5f));
    }

    private static void ExpandColliderFromBottom(
        BoxCollider2D collider,
        float minimumWidth,
        float minimumHeight)
    {
        float bottom = collider.offset.y - (collider.size.y * 0.5f);
        collider.size = new Vector2(
            Mathf.Max(collider.size.x, minimumWidth),
            Mathf.Max(collider.size.y, minimumHeight));
        collider.offset = new Vector2(collider.offset.x, bottom + (collider.size.y * 0.5f));
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

    private static GameObject CreateCrate(Sprite sprite, Vector2 position)
    {
        GameObject box = CreateGroundedSpriteObject("Memory Cube", sprite, position.x, position.y, new Vector2(0.60f, 0.60f));
        box.layer = LayerMask.NameToLayer("Ground");
        BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
        FitColliderFromBottom(collider, sprite, 0.86f, 0.86f);
        Rigidbody2D body = box.AddComponent<Rigidbody2D>();
        body.mass = 3.2f;
        body.linearDamping = 1.5f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        box.AddComponent<PushPullObject2D>();
        box.GetComponent<SpriteRenderer>().sortingOrder = 10;
        AddSpriteHighlight(box, "Cube Highlight", new Color(1f, 0.72f, 0.18f, 0.52f), 1.18f, 9);
        return box;
    }

    private static PressureSwitch2D CreatePressureSwitch(Sprite sprite, Vector2 position, DeathTrap target)
    {
        GameObject obj = CreateGroundedSpriteObject("Resonance Pad", sprite, position.x, position.y, new Vector2(0.72f, 0.45f));
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        FitColliderFromBottom(collider, sprite, 0.9f, 0.75f);
        PressureSwitch2D pressure = obj.AddComponent<PressureSwitch2D>();
        pressure.targetTrap = target;
        pressure.indicator = obj.GetComponent<SpriteRenderer>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 7;
        AddSpriteHighlight(obj, "Pad Highlight", new Color(1f, 0.48f, 0.24f, 0.42f), 1.14f, 6);

        float linkLength = Mathf.Abs(target.transform.position.x - position.x);
        GameObject link = CreateSpriteObject(
            "Resonance Link",
            highlightSprite,
            new Vector2((target.transform.position.x + position.x) * 0.5f, position.y + 0.06f),
            new Vector2(linkLength, 0.07f));
        SpriteRenderer linkRenderer = link.GetComponent<SpriteRenderer>();
        linkRenderer.color = new Color(1f, 0.38f, 0.28f, 0.58f);
        linkRenderer.sortingOrder = 5;
        ResonanceLink2D resonance = link.AddComponent<ResonanceLink2D>();
        resonance.targetTrap = target;
        return pressure;
    }

    private static void CreateCheckpoint(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateGroundedSpriteObject("Echo Lantern", sprite, position.x, position.y, new Vector2(0.72f, 0.72f));
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        FitColliderFromBottom(collider, sprite, 0.62f, 0.92f);
        ExpandColliderFromBottom(collider, 2.1f, 4.2f);
        Checkpoint checkpoint = obj.AddComponent<Checkpoint>();
        GameObject respawn = new GameObject("Respawn Spot");
        respawn.transform.SetParent(obj.transform);
        respawn.transform.position = new Vector3(position.x, position.y + PlayerGroundOffset, 0f);
        checkpoint.respawnSpot = respawn.transform;
        obj.GetComponent<SpriteRenderer>().sortingOrder = 7;
    }

    private static void CreateLadder(Sprite sprite, Vector2 position)
    {
        GameObject beacon = CreateSpriteObject(
            "Ladder Beacon",
            highlightSprite,
            new Vector2(position.x, position.y + 1.45f),
            new Vector2(0.92f, 3.15f));
        SpriteRenderer beaconRenderer = beacon.GetComponent<SpriteRenderer>();
        beaconRenderer.color = new Color(1f, 0.72f, 0.22f, 0.24f);
        beaconRenderer.sortingOrder = 5;
        VisualPulse2D beaconPulse = beacon.AddComponent<VisualPulse2D>();
        beaconPulse.scaleAmount = 0.025f;
        beaconPulse.minimumAlpha = 0.16f;
        beaconPulse.maximumAlpha = 0.32f;

        GameObject obj = CreateGroundedSpriteObject("Ribbon Ladder", sprite, position.x, position.y, new Vector2(0.82f, 1.50f));
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        FitColliderFromBottom(collider, sprite, 0.68f, 0.96f);
        obj.AddComponent<ClimbZone2D>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 8;
        AddSpriteHighlight(obj, "Ladder Highlight", new Color(1f, 0.78f, 0.28f, 0.44f), 1.10f, 7);
    }

    private static DeathTrap CreateSpikes(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateGroundedSpriteObject("Thorn Knot", sprite, position.x, position.y, new Vector2(1.05f, 0.58f));
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        FitColliderFromBottom(collider, sprite, 0.78f, 0.58f);
        DeathTrap trap = obj.AddComponent<DeathTrap>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 9;
        return trap;
    }

    private static DeathTrap CreateSaw(Sprite sprite, Vector2 position, Vector2 offset, float speed)
    {
        GameObject obj = CreateTriggerObject("Clockwork Hazard", sprite, position, new Vector2(0.58f, 0.58f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(1.2f, 1.2f);
        DeathTrap trap = obj.AddComponent<DeathTrap>();
        SimpleMover2D mover = obj.AddComponent<SimpleMover2D>();
        mover.localOffset = offset * 0.78f;
        mover.speed = speed * 0.82f;
        obj.GetComponent<SpriteRenderer>().sortingOrder = 10;
        return trap;
    }

    private static void CreateLever(Sprite sprite, Vector2 position, DeathTrap target)
    {
        GameObject obj = CreateGroundedSpriteObject("Tuning Fork", sprite, position.x, position.y, new Vector2(0.52f, 0.52f));
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        FitColliderFromBottom(collider, sprite, 0.8f, 0.94f);
        ExpandColliderFromBottom(collider, 2.2f, 3.2f);
        LeverSwitch2D lever = obj.AddComponent<LeverSwitch2D>();
        lever.targetTrap = target;
        lever.leverRenderer = obj.GetComponent<SpriteRenderer>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 9;
    }

    private static void CreateKey(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateTriggerObject("Echo Shard", sprite, position, new Vector2(0.42f, 0.42f));
        obj.GetComponent<BoxCollider2D>().size = new Vector2(3.2f, 3.2f);
        obj.AddComponent<CollectKey>();
        obj.AddComponent<CollectibleFloat2D>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 12;
        AddSpriteHighlight(obj, "Echo Highlight", new Color(0.32f, 0.92f, 1f, 0.50f), 1.24f, 11);
    }

    private static void CreateDoor(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateGroundedSpriteObject("Memory Threshold", sprite, position.x, position.y, new Vector2(1.05f, 1.05f));
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        FitColliderFromBottom(collider, sprite, 0.76f, 0.96f);
        ExpandColliderFromBottom(collider, 1.15f, 7.4f);
        DoorGoal door = obj.AddComponent<DoorGoal>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 11;
        AddSpriteHighlight(obj, "Threshold Highlight", new Color(1f, 0.58f, 0.22f, 0.48f), 1.10f, 10);

        GameObject barrier = CreateSpriteObject(
            "Memory Barrier",
            highlightSprite,
            new Vector2(position.x, position.y + 3.45f),
            new Vector2(0.78f, 6.9f));
        barrier.transform.SetParent(obj.transform, true);
        SpriteRenderer barrierRenderer = barrier.GetComponent<SpriteRenderer>();
        barrierRenderer.color = new Color(1f, 0.36f, 0.25f, 0.58f);
        barrierRenderer.sortingOrder = 9;
        door.barrierRenderer = barrierRenderer;
    }

    private static void CreateExit(Sprite sprite, Vector2 position)
    {
        GameObject obj = CreateGroundedSpriteObject("Return Portal", sprite, position.x, position.y, new Vector2(1.05f, 1.05f));
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        FitColliderFromBottom(collider, sprite, 0.68f, 0.92f);
        ExpandColliderFromBottom(collider, 2.2f, 4.2f);
        obj.AddComponent<FinishZone>();
        obj.GetComponent<SpriteRenderer>().sortingOrder = 8;
    }

    private static void CreateDecoration(string name, Sprite sprite, Vector2 position, float scale, int order)
    {
        GameObject obj = CreateGroundedSpriteObject(name, sprite, position.x, position.y, new Vector2(scale, scale));
        obj.GetComponent<SpriteRenderer>().sortingOrder = order;
    }

    private static void AddSpriteHighlight(
        GameObject target,
        string name,
        Color color,
        float scale,
        int sortingOrder)
    {
        SpriteRenderer source = target.GetComponent<SpriteRenderer>();
        GameObject highlight = new GameObject(name);
        highlight.transform.SetParent(target.transform, false);
        highlight.transform.localScale = new Vector3(scale, scale, 1f);
        SpriteRenderer renderer = highlight.AddComponent<SpriteRenderer>();
        renderer.sprite = source.sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        VisualPulse2D pulse = highlight.AddComponent<VisualPulse2D>();
        pulse.scaleAmount = 0.055f;
        pulse.minimumAlpha = Mathf.Max(0.12f, color.a * 0.55f);
        pulse.maximumAlpha = color.a;
    }

    [MenuItem("Tools/UMBRA/Validate Five Levels")]
    public static void ValidateProject()
    {
        var errors = new List<string>();

        if (EditorBuildSettings.scenes.Length != ScenePaths.Length)
        {
            errors.Add("Build Settings must contain exactly five UMBRA levels.");
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
            ValidateObject<PlayerRespawn>("Player", prefix, errors);
            ValidateObject<PlayerSpriteAnimator>("Player", prefix, errors);
            ValidateObject<CameraFollow2D>("Main Camera", prefix, errors);
            ValidateObject<Checkpoint>("Echo Lantern", prefix, errors);
            ValidateObject<CollectKey>("Echo Shard", prefix, errors);
            ValidateObject<DoorGoal>("Memory Threshold", prefix, errors);
            ValidateObject<FinishZone>("Return Portal", prefix, errors);
            ValidateVisibleSprite("Terrain Start", prefix, errors);
            ValidateVisibleSprite("Echo Shard", prefix, errors);
            ValidateVisibleSprite("Memory Threshold", prefix, errors);
            ValidateVisibleSprite("Return Portal", prefix, errors);
            ValidateInteractionReadability(i, prefix, errors);

            Checkpoint[] checkpoints = Object.FindObjectsByType<Checkpoint>();
            FinishZone finish = Object.FindAnyObjectByType<FinishZone>();
            if (checkpoints.Length < 2)
            {
                errors.Add(prefix + "requires at least two checkpoints for the extended route.");
            }

            if (finish == null || finish.transform.position.x < MinimumGoalX)
            {
                errors.Add(prefix + "route is too short.");
            }

            PlayerController2D player = Object.FindAnyObjectByType<PlayerController2D>();
            BoxCollider2D playerCollider = player != null ? player.GetComponent<BoxCollider2D>() : null;
            if (player == null || player.groundCheck == null || player.groundLayer.value == 0)
            {
                errors.Add(prefix + "player ground detection is not configured.");
            }

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

            ValidatePropSprite("Echo Shard", prefix, errors);
            ValidatePropSprite("Memory Threshold", prefix, errors);
            ValidatePropSprite("Return Portal", prefix, errors);
            ValidateGroundedGameplayObjects(prefix, errors);
        }

        EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
        if (errors.Count > 0)
        {
            throw new System.Exception("UMBRA validation failed:\n- " + string.Join("\n- ", errors));
        }

        Debug.Log("UMBRA VALIDATION PASSED: all five levels and their main gameplay objects are configured.");
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

    [MenuItem("Tools/UMBRA/Capture All Level Previews")]
    public static void CaptureAllPreviews()
    {
        Directory.CreateDirectory("Logs/LevelPreviews");
        float[] cameraPositions = { -7f, 58f, 123f };
        string[] sectionNames = { "Start", "Middle", "End" };
        for (int i = 0; i < ScenePaths.Length; i++)
        {
            EditorSceneManager.OpenScene(ScenePaths[i], OpenSceneMode.Single);
            Camera camera = Camera.main;
            for (int section = 0; section < cameraPositions.Length; section++)
            {
                camera.transform.position = new Vector3(cameraPositions[section], -0.2f, -10f);
                CaptureCamera(
                    "Logs/LevelPreviews/Level_" + (i + 1).ToString("00") +
                    "_" + sectionNames[section] + ".png");
            }
        }

        EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
        Debug.Log("UMBRA PREVIEWS PASSED: start, middle and end of all five levels rendered correctly.");
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
        if (!path.Contains("/Props/EchoFrames/"))
        {
            errors.Add(prefix + objectName + " is still using placeholder art.");
        }
    }

    private static void ValidateInteractionReadability(int levelIndex, string prefix, List<string> errors)
    {
        DoorGoal door = Object.FindAnyObjectByType<DoorGoal>();
        BoxCollider2D doorCollider = door != null ? door.GetComponent<BoxCollider2D>() : null;
        if (door == null || doorCollider == null || doorCollider.bounds.size.y < 7f ||
            door.barrierRenderer == null || door.barrierRenderer.sprite == null)
        {
            errors.Add(prefix + "memory threshold needs a tall visible barrier.");
        }

        ClimbZone2D ladder = Object.FindAnyObjectByType<ClimbZone2D>();
        if (ladder == null || GameObject.Find("Ladder Beacon") == null ||
            ladder.transform.Find("Ladder Highlight") == null)
        {
            errors.Add(prefix + "ladder needs a visible beacon and highlight.");
        }

        if (levelIndex == 0 || levelIndex == 2 || levelIndex == 4)
        {
            PushPullObject2D cube = Object.FindAnyObjectByType<PushPullObject2D>();
            if (cube == null || cube.transform.Find("Cube Highlight") == null ||
                Object.FindAnyObjectByType<ResonanceLink2D>() == null)
            {
                errors.Add(prefix + "cube puzzle needs highlights and a resonance link.");
            }
        }
    }

    private static void ValidateGroundedGameplayObjects(string prefix, List<string> errors)
    {
        Physics2D.SyncTransforms();
        int groundMask = LayerMask.GetMask("Ground");
        SpriteRenderer[] renderers = Object.FindObjectsByType<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            string objectName = renderer.gameObject.name;
            bool shouldBeGrounded = objectName == "Memory Cube" || objectName == "Resonance Pad" ||
                objectName == "Echo Lantern" || objectName == "Ribbon Ladder" ||
                objectName == "Thorn Knot" || objectName == "Tuning Fork" ||
                objectName == "Memory Threshold" || objectName == "Return Portal" ||
                objectName.StartsWith("Memory Tablet", System.StringComparison.Ordinal);
            if (!shouldBeGrounded)
            {
                continue;
            }

            Bounds bounds = renderer.bounds;
            Vector2 probe = new Vector2(bounds.center.x, bounds.min.y - 0.035f);
            Collider2D[] hits = Physics2D.OverlapPointAll(probe, groundMask);
            bool supported = false;
            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject != renderer.gameObject && !hit.isTrigger)
                {
                    supported = true;
                    break;
                }
            }

            if (!supported)
            {
                errors.Add(prefix + objectName + " is not supported by terrain.");
            }
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
        const string path = "Assets/Art/Terrain/echo_terrain_tile.png";
        if (!File.Exists(path))
        {
            throw new System.Exception("Required terrain asset is missing: " + path);
        }

        return ImportSingleSprite(path, 64f, true);
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
        float worldHeight,
        bool trimTransparent)
    {
        if (!File.Exists(sheetPath))
        {
            throw new System.Exception("Spritesheet is missing: " + sheetPath);
        }

        Texture2D sheet = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        sheet.LoadImage(File.ReadAllBytes(sheetPath));
        if (sheet.width % columns != 0 || sheet.height % rows != 0)
        {
            Object.DestroyImmediate(sheet);
            throw new System.Exception("Spritesheet needs an exact " + columns + "x" + rows + " grid: " + sheetPath);
        }

        int cellWidth = sheet.width / columns;
        int cellHeight = sheet.height / rows;
        Directory.CreateDirectory(framesFolder);
        Sprite[] frames = new Sprite[columns * rows];
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = (row * columns) + column;
                int sourceX = column * cellWidth;
                int sourceY = sheet.height - ((row + 1) * cellHeight);
                int cropX = 0;
                int cropY = 0;
                int cropWidth = cellWidth;
                int cropHeight = cellHeight;
                FindOpaqueBounds(
                    sheet,
                    sourceX,
                    sourceY,
                    cellWidth,
                    cellHeight,
                    out int opaqueX,
                    out int opaqueY,
                    out int opaqueWidth,
                    out int opaqueHeight);
                if (trimTransparent)
                {
                    cropX = opaqueX;
                    cropY = opaqueY;
                    cropWidth = opaqueWidth;
                    cropHeight = opaqueHeight;
                }

                Texture2D frame = new Texture2D(cropWidth, cropHeight, TextureFormat.RGBA32, false);
                frame.SetPixels(sheet.GetPixels(sourceX + cropX, sourceY + cropY, cropWidth, cropHeight));
                frame.Apply();
                string framePath = framesFolder + "/" + filePrefix + "_" + index.ToString("00") + ".png";
                File.WriteAllBytes(framePath, frame.EncodeToPNG());
                Object.DestroyImmediate(frame);
                float pixelsPerUnit = cellHeight / worldHeight;
                Vector2? pivot = trimTransparent
                    ? null
                    : new Vector2(
                        0.5f,
                        Mathf.Clamp01((opaqueY + (PlayerGroundOffset * pixelsPerUnit)) / cellHeight));
                frames[index] = ImportSingleSprite(framePath, pixelsPerUnit, false, pivot);
            }
        }

        Object.DestroyImmediate(sheet);
        return frames;
    }

    private static void FindOpaqueBounds(
        Texture2D sheet,
        int sourceX,
        int sourceY,
        int width,
        int height,
        out int cropX,
        out int cropY,
        out int cropWidth,
        out int cropHeight)
    {
        int minX = width;
        int minY = height;
        int maxX = -1;
        int maxY = -1;
        Color[] pixels = sheet.GetPixels(sourceX, sourceY, width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[(y * width) + x].a <= 0.04f)
                {
                    continue;
                }

                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if (maxX < minX || maxY < minY)
        {
            cropX = 0;
            cropY = 0;
            cropWidth = width;
            cropHeight = height;
            return;
        }

        cropX = minX;
        cropY = minY;
        cropWidth = (maxX - minX) + 1;
        cropHeight = (maxY - minY) + 1;
    }

    private static Sprite ImportSingleSprite(
        string path,
        float pixelsPerUnit,
        bool tiled,
        Vector2? customPivot = null)
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
        if (customPivot.HasValue)
        {
            importerSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            importerSettings.spritePivot = customPivot.Value;
        }
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
