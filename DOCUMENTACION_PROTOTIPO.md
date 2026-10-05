# UMBRA - Documentacion tecnica breve

El proyecto conserva la version instalada de Unity y no agrega dependencias externas.
`UmbraPrototypeBuilder` queda por compatibilidad con la base original, pero genera el juego definitivo
*UMBRA* y sus escenas `Chapter_01` a `Chapter_05`.

Sistemas principales:

- `PlayerController2D`: caminar, correr, salto tolerante, agacharse, trepar, empujar/jalar y esconderse.
- `ShadowDash2D`: Impulso Umbral con `Q` o `Ctrl derecho`, estela visual, recarga y bloqueo en estados incompatibles.
- `ParallaxLayer2D`: profundidad visual mediante fondos y nieblas con velocidades diferenciadas.
- `EnemyAI2D`: almas y demonios con patrulla, vigilancia, persecucion, busqueda y retorno.
- `HorrorEvent2D`: eventos reutilizables persistentes por partida.
- `GameManager`: portada, pausa, volumen, guardado, muerte, transiciones y final del hospital.
- `PlayerRespawn`: checkpoints persistentes; morir recarga la escena para restaurar mecanismos.
- `CameraFollow2D`: seguimiento suave, limites de zona y trauma de camara.
- `UmbraAudio`: ambiente, respiracion, pasos, aterrizajes, mecanismos y golpes de terror generados localmente.
- `UmbraRuntimeDiagnostics`: diagnostico automatizado ejecutable con `-umbraSmoke`.

Escena inicial: `Assets/Scenes/Chapter_01_El_Fondo.unity`.
