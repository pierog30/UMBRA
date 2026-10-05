using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class UmbraTechnicalReport
{
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Chapter_01_El_Fondo.unity",
        "Assets/Scenes/Chapter_02_Los_Abandonados.unity",
        "Assets/Scenes/Chapter_03_La_Carne.unity",
        "Assets/Scenes/Chapter_04_El_Peso.unity",
        "Assets/Scenes/Chapter_05_El_Umbral.unity"
    };

    [MenuItem("Tools/UMBRAL/Generate Technical Physics Report")]
    public static void Generate()
    {
        var report = new StringBuilder();
        report.AppendLine("# UMBRAL — Reporte técnico completo de Unity");
        report.AppendLine();
        report.AppendLine("Generado automáticamente desde las cinco escenas activas.");
        report.AppendLine();
        AppendProjectSettings(report);

        foreach (string scenePath in ScenePaths)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Physics2D.SyncTransforms();
            AppendScene(report, scenePath);
        }

        report.AppendLine("## Significado de la configuración");
        report.AppendLine();
        report.AppendLine("- `Trigger = Sí`: detecta entradas/salidas y ejecuta lógica, pero no bloquea físicamente.");
        report.AppendLine("- `Trigger = No`: forma una superficie o barrera sólida.");
        report.AppendLine("- `Dynamic`: Rigidbody afectado por gravedad y fuerzas; se usa para jugador y cajas.");
        report.AppendLine("- `Kinematic`: Rigidbody movido por código; se usa para plataformas móviles.");
        report.AppendLine("- Sin Rigidbody: objeto estático o sensor; los triggers funcionan porque el jugador sí posee Rigidbody.");
        report.AppendLine("- `Continuous`: reduce el riesgo de atravesar colliders a velocidades altas.");
        report.AppendLine("- `Interpolate`: suaviza visualmente el movimiento entre pasos físicos.");
        report.AppendLine("- Las plataformas superiores usan `PlatformEffector2D`: se atraviesan desde abajo y sostienen desde arriba.");

        File.WriteAllText("REPORTE_TECNICO_UNITY.md", report.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
        Debug.Log("UMBRAL TECHNICAL REPORT GENERATED: REPORTE_TECNICO_UNITY.md");
    }

    private static void AppendProjectSettings(StringBuilder report)
    {
        report.AppendLine("## Configuración global");
        report.AppendLine();
        report.AppendLine("| Propiedad | Valor |");
        report.AppendLine("|---|---|");
        report.AppendLine("| Producto | " + Escape(PlayerSettings.productName) + " |");
        report.AppendLine("| Versión Unity | " + Escape(Application.unityVersion) + " |");
        report.AppendLine("| Resolución predeterminada | " + PlayerSettings.defaultScreenWidth + " × " + PlayerSettings.defaultScreenHeight + " |");
        report.AppendLine("| Modo de ventana | " + PlayerSettings.fullScreenMode + " |");
        report.AppendLine("| Física | Physics 2D, paso fijo 1/60 s |");
        report.AppendLine("| Gravedad global 2D | " + F(Physics2D.gravity.x) + ", " + F(Physics2D.gravity.y) + " |");
        report.AppendLine("| Velocidad objetivo | 60 FPS con VSync 1 |");
        report.AppendLine();

        report.AppendLine("### Capas configuradas");
        report.AppendLine();
        report.AppendLine("| Índice | Nombre | Función en UMBRAL |");
        report.AppendLine("|---:|---|---|");
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (string.IsNullOrEmpty(layerName)) continue;
            string purpose = layerName == "Ground"
                ? "Terreno, plataformas, cajas y obstáculos; máscara usada por suelo, visión y bordes"
                : layerName == "Default" ? "Jugador, sensores, enemigos, puertas, llaves y lógica general" : "Capa integrada de Unity";
            report.AppendLine("| " + i + " | `" + Escape(layerName) + "` | " + purpose + " |");
        }
        report.AppendLine();

        report.AppendLine("### Matriz de colisión relevante");
        report.AppendLine();
        report.AppendLine("| Par | Colisiona |");
        report.AppendLine("|---|---|");
        AppendCollisionPair(report, "Default", "Default");
        AppendCollisionPair(report, "Default", "Ground");
        AppendCollisionPair(report, "Ground", "Ground");
        report.AppendLine();

        report.AppendLine("### Escenas incluidas en el build");
        report.AppendLine();
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
            report.AppendLine((i + 1) + ". `" + Escape(scene.path) + "` — " + (scene.enabled ? "activa" : "desactivada"));
        }
        report.AppendLine();
    }

    private static void AppendCollisionPair(StringBuilder report, string first, string second)
    {
        int a = LayerMask.NameToLayer(first);
        int b = LayerMask.NameToLayer(second);
        bool collides = a >= 0 && b >= 0 && !Physics2D.GetIgnoreLayerCollision(a, b);
        report.AppendLine("| `" + first + "` ↔ `" + second + "` | " + (collides ? "Sí" : "No") + " |");
    }

    private static void AppendScene(StringBuilder report, string scenePath)
    {
        Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        Collider2D[] colliders = Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include);
        Rigidbody2D[] bodies = Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include);
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
        int triggers = colliders.Count(collider => collider.isTrigger);
        int solids = colliders.Length - triggers;

        report.AppendLine("## " + Path.GetFileNameWithoutExtension(scenePath).Replace('_', ' '));
        report.AppendLine();
        report.AppendLine("| Elemento | Cantidad |");
        report.AppendLine("|---|---:|");
        report.AppendLine("| GameObjects | " + transforms.Length + " |");
        report.AppendLine("| Collider2D totales | " + colliders.Length + " |");
        report.AppendLine("| Colliders sólidos | " + solids + " |");
        report.AppendLine("| Triggers | " + triggers + " |");
        report.AppendLine("| Rigidbody2D dinámicos | " + bodies.Count(body => body.bodyType == RigidbodyType2D.Dynamic) + " |");
        report.AppendLine("| Rigidbody2D cinemáticos | " + bodies.Count(body => body.bodyType == RigidbodyType2D.Kinematic) + " |");
        report.AppendLine("| Componentes de lógica MonoBehaviour | " + behaviours.Length + " |");
        report.AppendLine("| Plataformas unidireccionales | " + Object.FindObjectsByType<PlatformEffector2D>(FindObjectsInactive.Include).Length + " |");
        report.AppendLine("| Trampas `DeathTrap` | " + Object.FindObjectsByType<DeathTrap>(FindObjectsInactive.Include).Length + " |");
        report.AppendLine("| Enemigos | " + Object.FindObjectsByType<EnemyAI2D>(FindObjectsInactive.Include).Length + " |");
        report.AppendLine("| Plataformas móviles | " + Object.FindObjectsByType<MovingPlatform2D>(FindObjectsInactive.Include).Length + " |");
        report.AppendLine();

        AppendPlayer(report);
        AppendCamera(report);
        AppendPhysicsObjects(report, colliders, bodies);
        AppendMechanisms(report);
        AppendGameplayComponents(report, behaviours);
    }

    private static void AppendPlayer(StringBuilder report)
    {
        PlayerController2D player = Object.FindAnyObjectByType<PlayerController2D>();
        if (player == null) return;
        report.AppendLine("### Configuración del jugador");
        report.AppendLine();
        report.AppendLine("| Propiedad | Valor |");
        report.AppendLine("|---|---|");
        report.AppendLine("| Velocidad | " + F(player.moveSpeed) + " |");
        report.AppendLine("| Multiplicador al correr | " + F(player.runMultiplier) + " |");
        report.AppendLine("| Fuerza de salto | " + F(player.jumpForce) + " |");
        report.AppendLine("| Velocidad de escalada | " + F(player.climbSpeed) + " |");
        report.AppendLine("| Aceleración / desaceleración | " + F(player.acceleration) + " / " + F(player.deceleration) + " |");
        report.AppendLine("| Control aéreo | " + F(player.airControl) + " |");
        report.AppendLine("| Coyote time / jump buffer | " + F(player.coyoteTime) + " s / " + F(player.jumpBufferTime) + " s |");
        report.AppendLine("| Radio de suelo | " + F(player.groundRadius) + "; máscara `Ground` |");
        report.AppendLine("| Collider de pie | Box 0,52 × 1,35; al agacharse usa 58 % de altura |");
        ShadowDash2D dash = player.GetComponent<ShadowDash2D>();
        if (dash != null)
        {
            report.AppendLine("| Mecánica secundaria | Impulso umbral con `" + dash.dashKey + "`; velocidad " +
                F(dash.dashSpeed) + "; duración " + F(dash.dashDuration) + " s; recarga " + F(dash.cooldown) + " s |");
        }
        report.AppendLine();
    }

    private static void AppendCamera(StringBuilder report)
    {
        Camera camera = Camera.main;
        CameraFollow2D follow = camera != null ? camera.GetComponent<CameraFollow2D>() : null;
        if (camera == null || follow == null) return;
        report.AppendLine("### Cámara");
        report.AppendLine();
        report.AppendLine("Ortográfica, tamaño " + F(camera.orthographicSize) + ", suavizado " + F(follow.smoothSpeed) +
            ", límites desde (" + F(follow.minBounds.x) + ", " + F(follow.minBounds.y) + ") hasta (" +
            F(follow.maxBounds.x) + ", " + F(follow.maxBounds.y) + "). Incluye `AudioListener` y trauma visual.");
        ParallaxLayer2D[] layers = Object.FindObjectsByType<ParallaxLayer2D>(FindObjectsInactive.Include);
        report.AppendLine("El escenario utiliza " + layers.Length + " capas `ParallaxLayer2D`. Sus factores horizontales son " +
            string.Join(", ", layers.Select(layer => F(layer.horizontalFactor)).Distinct()) +
            "; la niebla incorpora deriva ambiental independiente.");
        report.AppendLine();
    }

    private static void AppendPhysicsObjects(StringBuilder report, Collider2D[] colliders, Rigidbody2D[] bodies)
    {
        var objects = new HashSet<GameObject>(colliders.Select(collider => collider.gameObject));
        foreach (Rigidbody2D body in bodies) objects.Add(body.gameObject);

        report.AppendLine("### Objetos físicos, colliders y triggers");
        report.AppendLine();
        report.AppendLine("Los tamaños `local` son los valores del componente; `mundo` incluye la escala del Transform.");
        report.AppendLine();
        report.AppendLine("| GameObject | Posición | Escala | Layer | Collider | Trigger | Rigidbody2D | Datos físicos | Componentes relevantes |");
        report.AppendLine("|---|---|---|---|---|---|---|---|---|");
        foreach (GameObject obj in objects.OrderBy(item => item.transform.position.x).ThenBy(item => item.name))
        {
            Collider2D collider = obj.GetComponent<Collider2D>();
            Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
            string colliderText = DescribeCollider(collider);
            string bodyText = body == null ? "—" : body.bodyType.ToString();
            string physics = DescribeBody(body, collider);
            string components = string.Join(", ", obj.GetComponents<Component>()
                .Where(component => component != null && component is not Transform && component is not SpriteRenderer && component is not Collider2D && component is not Rigidbody2D)
                .Select(component => component.GetType().Name));
            report.AppendLine("| `" + Escape(obj.name) + "` | (" + F(obj.transform.position.x) + ", " + F(obj.transform.position.y) + ") | (" +
                F(obj.transform.lossyScale.x) + ", " + F(obj.transform.lossyScale.y) + ") | `" +
                Escape(LayerMask.LayerToName(obj.layer)) + "` | " + colliderText + " | " +
                (collider != null && collider.isTrigger ? "Sí" : "No") + " | " + bodyText + " | " + physics + " | " +
                (string.IsNullOrEmpty(components) ? "—" : Escape(components)) + " |");
        }
        report.AppendLine();
    }

    private static string DescribeCollider(Collider2D collider)
    {
        if (collider == null) return "—";
        if (collider is BoxCollider2D box) return "Box local " + F(box.size.x) + " × " + F(box.size.y) +
            "; mundo " + F(collider.bounds.size.x) + " × " + F(collider.bounds.size.y);
        if (collider is CircleCollider2D circle) return "Circle r local=" + F(circle.radius) +
            "; mundo " + F(collider.bounds.size.x) + " × " + F(collider.bounds.size.y);
        if (collider is CapsuleCollider2D capsule) return "Capsule local " + F(capsule.size.x) + " × " + F(capsule.size.y) +
            "; mundo " + F(collider.bounds.size.x) + " × " + F(collider.bounds.size.y);
        return Escape(collider.GetType().Name);
    }

    private static void AppendMechanisms(StringBuilder report)
    {
        report.AppendLine("### Ajustes de IA, plataformas y mecanismos");
        report.AppendLine();
        report.AppendLine("| Objeto | Tipo | Configuración |");
        report.AppendLine("|---|---|---|");

        foreach (EnemyAI2D enemy in Object.FindObjectsByType<EnemyAI2D>(FindObjectsInactive.Include))
        {
            report.AppendLine("| `" + Escape(enemy.name) + "` | IA " + enemy.kind + " | patrulla " + F(enemy.patrolDistance) +
                "; velocidad " + F(enemy.patrolSpeed) + "; persecución " + F(enemy.chaseSpeed) + "; visión " +
                F(enemy.viewDistance) + "; búsqueda " + F(enemy.searchTime) + " s; obstáculos `Ground` |");
        }

        foreach (MovingPlatform2D platform in Object.FindObjectsByType<MovingPlatform2D>(FindObjectsInactive.Include))
        {
            report.AppendLine("| `" + Escape(platform.name) + "` | Plataforma móvil | offset (" + F(platform.offset.x) + ", " +
                F(platform.offset.y) + "); velocidad " + F(platform.speed) + "; Rigidbody Kinematic |");
        }

        foreach (SimpleMover2D mover in Object.FindObjectsByType<SimpleMover2D>(FindObjectsInactive.Include))
        {
            report.AppendLine("| `" + Escape(mover.name) + "` | Trampa móvil | offset (" + F(mover.localOffset.x) + ", " +
                F(mover.localOffset.y) + "); velocidad " + F(mover.speed) + " |");
        }

        foreach (PlatformEffector2D effector in Object.FindObjectsByType<PlatformEffector2D>(FindObjectsInactive.Include))
        {
            report.AppendLine("| `" + Escape(effector.name) + "` | Plataforma unidireccional | one-way=" +
                (effector.useOneWay ? "Sí" : "No") + "; agrupación=" + (effector.useOneWayGrouping ? "Sí" : "No") +
                "; arco superficial " + F(effector.surfaceArc) + "° |");
        }

        foreach (PressureSwitch2D pressure in Object.FindObjectsByType<PressureSwitch2D>(FindObjectsInactive.Include))
        {
            report.AppendLine("| `" + Escape(pressure.name) + "` | Placa | detecta `PushPullObject2D`; controla `" +
                Escape(pressure.targetTrap != null ? pressure.targetTrap.name : "sin objetivo") + "` |");
        }

        foreach (LeverSwitch2D lever in Object.FindObjectsByType<LeverSwitch2D>(FindObjectsInactive.Include))
        {
            report.AppendLine("| `" + Escape(lever.name) + "` | Palanca | tecla E; controla `" +
                Escape(lever.targetTrap != null ? lever.targetTrap.name : "sin objetivo") + "` |");
        }
        report.AppendLine();
    }

    private static string DescribeBody(Rigidbody2D body, Collider2D collider)
    {
        var parts = new List<string>();
        if (body != null)
        {
            parts.Add("masa " + F(body.mass));
            parts.Add("gravedad " + F(body.gravityScale));
            parts.Add(body.collisionDetectionMode.ToString());
            parts.Add(body.interpolation.ToString());
            if (body.freezeRotation) parts.Add("rotación Z bloqueada");
        }
        if (collider != null && collider.sharedMaterial != null)
        {
            parts.Add("material `" + Escape(collider.sharedMaterial.name) + "` (fricción " +
                F(collider.sharedMaterial.friction) + ", rebote " + F(collider.sharedMaterial.bounciness) + ")");
        }
        if (collider != null && collider.usedByEffector) parts.Add("usa effector");
        return parts.Count == 0 ? "—" : string.Join(", ", parts);
    }

    private static void AppendGameplayComponents(StringBuilder report, MonoBehaviour[] behaviours)
    {
        report.AppendLine("### Componentes de gameplay presentes");
        report.AppendLine();
        foreach (var group in behaviours.GroupBy(behaviour => behaviour.GetType().Name).OrderBy(group => group.Key))
        {
            report.AppendLine("- `" + Escape(group.Key) + "`: " + group.Count());
        }
        report.AppendLine();
    }

    private static string F(float value) => value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
    private static string Escape(string value) => string.IsNullOrEmpty(value) ? "—" : value.Replace("|", "\\|");
}
