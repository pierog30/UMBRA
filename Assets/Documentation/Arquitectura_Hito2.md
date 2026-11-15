# UMBRA Hito 2

## Alcance

Este archivo identifica la organización técnica y gráfica utilizada por la versión funcional del Hito 2. Las cinco escenas `Chapter_...` son las únicas escenas activas en `EditorBuildSettings`. Las cinco escenas `Level_...` se conservan en `Assets/Scenes/Legacy` como referencia y no forman parte del ejecutable.

## Organización del proyecto

```text
Assets
|- Art
|  |- Backgrounds
|  |- Character/Frames
|  |- Creatures/Frames
|  |- Hospital/Frames
|  |- Items
|  `- Props/Frames
|- Documentation
|- Editor
|- Scenes
|  |- Chapter_01_El_Fondo.unity
|  |- Chapter_02_Los_Abandonados.unity
|  |- Chapter_03_La_Carne.unity
|  |- Chapter_04_El_Peso.unity
|  |- Chapter_05_El_Umbral.unity
|  `- Legacy
`- Scripts
```

## Arquitectura técnica

1. `GameManager` conserva progreso, intentos, llave, cambio de escena y estado general.
2. `PlayerController2D`, `ShadowDash2D`, `PlayerRespawn` y `PlayerSpriteAnimator` componen al jugador.
3. Los objetos interactivos mantienen scripts específicos: `CollectKey`, `DoorGoal`, `Checkpoint`, `HidingSpot2D`, `PushPullObject2D`, `PressureSwitch2D` y `LeverSwitch2D`.
4. `EnemyAI2D`, `DeathTrap`, `MovingPlatform2D` y `ClimbZone2D` controlan amenazas y recorrido.
5. `UmbraGameEvents` publica eventos. `UmbraEventJournal`, el HUD y el audio reaccionan sin que los objetos se conozcan directamente.
6. `UmbraPrototypeBuilder`, `UmbraTechnicalReport` y `UmbraHito2Build` permanecen en `Assets/Editor`, por lo que no forman parte de la lógica de ejecución.

## Arquitectura gráfica

- Fondos: cinco imágenes panorámicas, una por capítulo.
- Personaje: una hoja principal y doce cuadros recortados.
- Criaturas: una hoja principal y cuatro cuadros recortados.
- Hospital: una hoja de pistas y seis cuadros recortados.
- Props: una hoja principal y doce cuadros recortados.
- Recursos adicionales: llave, terreno, trampa, cuadrados de color y material físico sin fricción.
- `ParallaxLayer2D` separa fondo y niebla del terreno jugable. Los colliders permanecen en el mundo y no se desplazan con las capas visuales.

## Capas y reglas

- `Default`: jugador, sensores, enemigos, puertas, llaves y lógica general.
- `Ground`: terreno, plataformas, cajas y obstáculos.
- `UI`: interfaz de Unity cuando corresponde.
- El jugador detecta suelo mediante la máscara `Ground`.
- Los triggers detectan presencia o progreso y los colliders sólidos bloquean movimiento.

## Convenciones del Hito 2

- Las escenas entregables usan el prefijo `Chapter_` y un número de dos dígitos.
- Los scripts de editor se guardan en `Assets/Editor`.
- Los recursos gráficos se organizan por función y conservan sus archivos `.meta`.
- El build se genera con `Tools/UMBRA/Build Hito 2` y produce `Builds/Hito2/UMBRA_Hito2.exe`.
- Las escenas de respaldo permanecen en `Assets/Scenes/Legacy` y no se agregan al Build Settings.
