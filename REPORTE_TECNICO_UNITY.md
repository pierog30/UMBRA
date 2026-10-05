# UMBRA — Reporte técnico completo de Unity

Generado automáticamente desde las cinco escenas activas.

## Configuración global

| Propiedad | Valor |
|---|---|
| Producto | UMBRA |
| Versión Unity | 6000.5.2f1 |
| Resolución predeterminada | 1280 × 720 |
| Modo de ventana | Windowed |
| Física | Physics 2D, paso fijo 1/60 s |
| Gravedad global 2D | 0, -9.81 |
| Velocidad objetivo | 60 FPS con VSync 1 |

### Capas configuradas

| Índice | Nombre | Función en UMBRA |
|---:|---|---|
| 0 | `Default` | Jugador, sensores, enemigos, puertas, llaves y lógica general |
| 1 | `TransparentFX` | Capa integrada de Unity |
| 2 | `Ignore Raycast` | Capa integrada de Unity |
| 4 | `Water` | Capa integrada de Unity |
| 5 | `UI` | Capa integrada de Unity |
| 6 | `Ground` | Terreno, plataformas, cajas y obstáculos; máscara usada por suelo, visión y bordes |

### Matriz de colisión relevante

| Par | Colisiona |
|---|---|
| `Default` ↔ `Default` | Sí |
| `Default` ↔ `Ground` | Sí |
| `Ground` ↔ `Ground` | Sí |

### Escenas incluidas en el build

1. `Assets/Scenes/Chapter_01_El_Fondo.unity` — activa
2. `Assets/Scenes/Chapter_02_Los_Abandonados.unity` — activa
3. `Assets/Scenes/Chapter_03_La_Carne.unity` — activa
4. `Assets/Scenes/Chapter_04_El_Peso.unity` — activa
5. `Assets/Scenes/Chapter_05_El_Umbral.unity` — activa

## Chapter 01 El Fondo

| Elemento | Cantidad |
|---|---:|
| GameObjects | 32 |
| Collider2D totales | 18 |
| Colliders sólidos | 7 |
| Triggers | 11 |
| Rigidbody2D dinámicos | 2 |
| Rigidbody2D cinemáticos | 0 |
| Componentes de lógica MonoBehaviour | 27 |
| Plataformas unidireccionales | 1 |
| Trampas `DeathTrap` | 2 |
| Enemigos | 1 |
| Plataformas móviles | 0 |

### Configuración del jugador

| Propiedad | Valor |
|---|---|
| Velocidad | 6.5 |
| Multiplicador al correr | 1.28 |
| Fuerza de salto | 12 |
| Velocidad de escalada | 4.5 |
| Aceleración / desaceleración | 55 / 70 |
| Control aéreo | 0.75 |
| Coyote time / jump buffer | 0.12 s / 0.12 s |
| Radio de suelo | 0.18; máscara `Ground` |
| Collider de pie | Box 0,52 × 1,35; al agacharse usa 58 % de altura |
| Mecánica secundaria | Impulso umbral con `Q`; velocidad 16.5; duración 0.16 s; recarga 1.15 s |

### Cámara

Ortográfica, tamaño 4.7, suavizado 9, límites desde (-7, -1) hasta (19, 3.5). Incluye `AudioListener` y trauma visual.
El escenario utiliza 5 capas `ParallaxLayer2D`. Sus factores horizontales son 0.42, 0.12, 0.28; la niebla incorpora deriva ambiental independiente.

### Objetos físicos, colliders y triggers

Los tamaños `local` son los valores del componente; `mundo` incluye la escala del Transform.

| GameObject | Posición | Escala | Layer | Collider | Trigger | Rigidbody2D | Datos físicos | Componentes relevantes |
|---|---|---|---|---|---|---|---|---|
| `Player` | (-7, -1.45) | (1, 1) | `Default` | Box local 0.52 × 1.35; mundo 0.52 × 1.35 | No | Dynamic | masa 1, gravedad 3, Continuous, Interpolate, rotación Z bloqueada, material `Player_NoFriction` (fricción 0, rebote 0) | PlayerController2D, ShadowDash2D, PlayerRespawn, PlayerSpriteAnimator, AwakeningSequence2D |
| `Push Box` | (-4, -1.55) | (0.72, 0.72) | `Ground` | Box local 1.35 × 1.35; mundo 0.97 × 0.97 | No | Dynamic | masa 3.2, gravedad 1, Continuous, Interpolate, rotación Z bloqueada | PushPullObject2D |
| `Terrain Start` | (-4, -2.65) | (1, 1) | `Ground` | Box local 10 × 0.62; mundo 10 × 0.62 | No | — | — | — |
| `Pressure Switch` | (-2.1, -2.12) | (0.72, 0.45) | `Default` | Box local 1.65 × 0.6; mundo 1.19 × 0.27 | Sí | — | — | PressureSwitch2D |
| `Memory Echo 1` | (-1, -0.65) | (1, 1) | `Default` | Box local 2.4 × 4.2; mundo 2.4 × 4.2 | Sí | — | — | NarrativeEcho2D |
| `Horror Event a` | (0.6, -0.8) | (1, 1) | `Default` | Box local 1.2 × 6; mundo 1.2 × 6 | Sí | — | — | HorrorEvent2D |
| `Checkpoint` | (2.5, -1.3) | (0.72, 0.72) | `Default` | Box local 0.8 × 1.7; mundo 0.58 × 1.22 | Sí | — | — | Checkpoint |
| `Spike Trap` | (4.5, -1.85) | (1.05, 0.58) | `Default` | Box local 1.65 × 0.65; mundo 1.73 × 0.38 | Sí | — | — | DeathTrap |
| `Terrain Middle` | (4.5, -2.65) | (1, 1) | `Ground` | Box local 5 × 0.62; mundo 5 × 0.62 | No | — | — | — |
| `Climb Zone` | (4.9, -1.15) | (0.72, 1.45) | `Default` | Box local 0.85 × 3.1; mundo 0.61 × 4.5 | Sí | — | — | ClimbZone2D |
| `Upper Path` | (6.5, 0.1) | (1, 1) | `Ground` | Box local 4.2 × 0.43; mundo 4.2 × 0.43 | No | — | usa effector | PlatformEffector2D |
| `Key` | (7.2, 1.05) | (0.3, 0.3) | `Default` | Box local 1.8 × 1.8; mundo 0.54 × 0.54 | Sí | — | — | CollectKey, CollectibleFloat2D |
| `Bottomless Fog` | (8, -7) | (38, 1) | `Default` | Box local 1 × 1; mundo 38 × 1 | Sí | — | — | DeathTrap |
| `Hiding Alcove` | (9, -1.1) | (0.9, 1.15) | `Default` | Box local 1.25 × 1.65; mundo 1.13 × 1.9 | Sí | — | — | HidingSpot2D |
| `Ash Warden` | (10.8, -1.25) | (0.72, 1.05) | `Default` | Box local 0.75 × 1.35; mundo 0.54 × 1.42 | Sí | — | — | EnemyAI2D |
| `Terrain End` | (12.5, -2.65) | (1, 1) | `Ground` | Box local 9 × 0.62; mundo 9 × 0.62 | No | — | — | — |
| `Locked Door` | (13.5, -1.25) | (1.05, 1.05) | `Default` | Box local 0.85 × 1.75; mundo 0.89 × 1.84 | No | — | — | DoorGoal |
| `Finish Zone` | (16.2, -1.15) | (1.05, 1.05) | `Default` | Box local 0.9 × 1.8; mundo 0.94 × 1.89 | Sí | — | — | FinishZone |

### Ajustes de IA, plataformas y mecanismos

| Objeto | Tipo | Configuración |
|---|---|---|
| `Ash Warden` | IA SorrowSoul | patrulla 2.2; velocidad 1.15; persecución 2.6; visión 5.6; búsqueda 2 s; obstáculos `Ground` |
| `Upper Path` | Plataforma unidireccional | one-way=Sí; agrupación=Sí; arco superficial 175° |
| `Pressure Switch` | Placa | detecta `PushPullObject2D`; controla `Spike Trap` |

### Componentes de gameplay presentes

- `AwakeningSequence2D`: 1
- `CameraFollow2D`: 1
- `Checkpoint`: 1
- `ClimbZone2D`: 1
- `CollectibleFloat2D`: 1
- `CollectKey`: 1
- `DeathTrap`: 2
- `DoorGoal`: 1
- `EnemyAI2D`: 1
- `FinishZone`: 1
- `GameManager`: 1
- `HidingSpot2D`: 1
- `HorrorEvent2D`: 1
- `NarrativeEcho2D`: 1
- `ParallaxLayer2D`: 5
- `PlayerController2D`: 1
- `PlayerRespawn`: 1
- `PlayerSpriteAnimator`: 1
- `PressureSwitch2D`: 1
- `PushPullObject2D`: 1
- `ShadowDash2D`: 1
- `UmbraAudio`: 1

## Chapter 02 Los Abandonados

| Elemento | Cantidad |
|---|---:|
| GameObjects | 33 |
| Collider2D totales | 19 |
| Colliders sólidos | 8 |
| Triggers | 11 |
| Rigidbody2D dinámicos | 1 |
| Rigidbody2D cinemáticos | 2 |
| Componentes de lógica MonoBehaviour | 28 |
| Plataformas unidireccionales | 1 |
| Trampas `DeathTrap` | 2 |
| Enemigos | 1 |
| Plataformas móviles | 2 |

### Configuración del jugador

| Propiedad | Valor |
|---|---|
| Velocidad | 6.5 |
| Multiplicador al correr | 1.28 |
| Fuerza de salto | 12 |
| Velocidad de escalada | 4.5 |
| Aceleración / desaceleración | 55 / 70 |
| Control aéreo | 0.75 |
| Coyote time / jump buffer | 0.12 s / 0.12 s |
| Radio de suelo | 0.18; máscara `Ground` |
| Collider de pie | Box 0,52 × 1,35; al agacharse usa 58 % de altura |
| Mecánica secundaria | Impulso umbral con `Q`; velocidad 16.5; duración 0.16 s; recarga 1.15 s |

### Cámara

Ortográfica, tamaño 4.7, suavizado 9, límites desde (-7, -1) hasta (19, 3.5). Incluye `AudioListener` y trauma visual.
El escenario utiliza 5 capas `ParallaxLayer2D`. Sus factores horizontales son 0.28, 0.12, 0.42; la niebla incorpora deriva ambiental independiente.

### Objetos físicos, colliders y triggers

Los tamaños `local` son los valores del componente; `mundo` incluye la escala del Transform.

| GameObject | Posición | Escala | Layer | Collider | Trigger | Rigidbody2D | Datos físicos | Componentes relevantes |
|---|---|---|---|---|---|---|---|---|
| `Player` | (-7, -1.45) | (1, 1) | `Default` | Box local 0.52 × 1.35; mundo 0.52 × 1.35 | No | Dynamic | masa 1, gravedad 3, Continuous, Interpolate, rotación Z bloqueada, material `Player_NoFriction` (fricción 0, rebote 0) | PlayerController2D, ShadowDash2D, PlayerRespawn, PlayerSpriteAnimator |
| `Terrain Start` | (-5.5, -2.65) | (1, 1) | `Ground` | Box local 7 × 0.62; mundo 7 × 0.62 | No | — | — | — |
| `Moving Bridge A` | (-1.1, -1.6) | (1, 1) | `Ground` | Box local 2.1 × 0.35; mundo 2.1 × 0.35 | No | Kinematic | masa 1, gravedad 1, Discrete, None | MovingPlatform2D |
| `Checkpoint` | (1, -1.3) | (0.72, 0.72) | `Default` | Box local 0.8 × 1.7; mundo 0.58 × 1.22 | Sí | — | — | Checkpoint |
| `Terrain Island` | (2, -2.65) | (1, 1) | `Ground` | Box local 4 × 0.62; mundo 4 × 0.62 | No | — | — | — |
| `Memory Echo 2` | (3.5, -0.65) | (1, 1) | `Default` | Box local 2.4 × 4.2; mundo 2.4 × 4.2 | Sí | — | — | NarrativeEcho2D |
| `Horror Event a` | (4.2, -0.7) | (1, 1) | `Default` | Box local 1.2 × 6; mundo 1.2 × 6 | Sí | — | — | HorrorEvent2D |
| `Moving Bridge B` | (5, -0.9) | (1, 1) | `Ground` | Box local 2.2 × 0.35; mundo 2.2 × 0.35 | No | Kinematic | masa 1, gravedad 1, Discrete, None | MovingPlatform2D |
| `Bottomless Fog` | (8, -7) | (38, 1) | `Default` | Box local 1 × 1; mundo 38 × 1 | Sí | — | — | DeathTrap |
| `Hiding Alcove` | (9.65, -1.1) | (0.9, 1.15) | `Default` | Box local 1.25 × 1.65; mundo 1.13 × 1.9 | Sí | — | — | HidingSpot2D |
| `Climb Zone` | (10.7, -1.15) | (0.72, 1.45) | `Default` | Box local 0.85 × 3.1; mundo 0.61 × 4.5 | Sí | — | — | ClimbZone2D |
| `Lever` | (11.8, 1.2) | (0.52, 0.52) | `Default` | Box local 1.25 × 1.45; mundo 0.65 × 0.75 | Sí | — | — | LeverSwitch2D |
| `Upper Ruin` | (12.5, 0.35) | (1, 1) | `Ground` | Box local 4.5 × 0.43; mundo 4.5 × 0.43 | No | — | usa effector | PlatformEffector2D |
| `Abandoned Soul` | (12.8, -1.25) | (0.72, 1.05) | `Default` | Box local 0.75 × 1.35; mundo 0.54 × 1.42 | Sí | — | — | EnemyAI2D |
| `Key` | (13.5, 1.3) | (0.3, 0.3) | `Default` | Box local 1.8 × 1.8; mundo 0.54 × 0.54 | Sí | — | — | CollectKey, CollectibleFloat2D |
| `Terrain End` | (13.5, -2.65) | (1, 1) | `Ground` | Box local 9 × 0.62; mundo 9 × 0.62 | No | — | — | — |
| `Moving Saw` | (14.4, -1.2) | (0.58, 0.58) | `Default` | Box local 1.2 × 1.2; mundo 0.7 × 0.7 | Sí | — | — | DeathTrap, SimpleMover2D |
| `Locked Door` | (16.2, -1.25) | (1.05, 1.05) | `Default` | Box local 0.85 × 1.75; mundo 0.89 × 1.84 | No | — | — | DoorGoal |
| `Finish Zone` | (17.45, -1.15) | (1.05, 1.05) | `Default` | Box local 0.9 × 1.8; mundo 0.94 × 1.89 | Sí | — | — | FinishZone |

### Ajustes de IA, plataformas y mecanismos

| Objeto | Tipo | Configuración |
|---|---|---|
| `Abandoned Soul` | IA SorrowSoul | patrulla 1.25; velocidad 1.15; persecución 3.5; visión 5.6; búsqueda 2 s; obstáculos `Ground` |
| `Moving Bridge B` | Plataforma móvil | offset (3.5, 0); velocidad 1.05; Rigidbody Kinematic |
| `Moving Bridge A` | Plataforma móvil | offset (1.8, 0); velocidad 1.25; Rigidbody Kinematic |
| `Moving Saw` | Trampa móvil | offset (0, 2.3); velocidad 1.5 |
| `Upper Ruin` | Plataforma unidireccional | one-way=Sí; agrupación=Sí; arco superficial 175° |
| `Lever` | Palanca | tecla E; controla `Moving Saw` |

### Componentes de gameplay presentes

- `CameraFollow2D`: 1
- `Checkpoint`: 1
- `ClimbZone2D`: 1
- `CollectibleFloat2D`: 1
- `CollectKey`: 1
- `DeathTrap`: 2
- `DoorGoal`: 1
- `EnemyAI2D`: 1
- `FinishZone`: 1
- `GameManager`: 1
- `HidingSpot2D`: 1
- `HorrorEvent2D`: 1
- `LeverSwitch2D`: 1
- `MovingPlatform2D`: 2
- `NarrativeEcho2D`: 1
- `ParallaxLayer2D`: 5
- `PlayerController2D`: 1
- `PlayerRespawn`: 1
- `PlayerSpriteAnimator`: 1
- `ShadowDash2D`: 1
- `SimpleMover2D`: 1
- `UmbraAudio`: 1

## Chapter 03 La Carne

| Elemento | Cantidad |
|---|---:|
| GameObjects | 34 |
| Collider2D totales | 20 |
| Colliders sólidos | 8 |
| Triggers | 12 |
| Rigidbody2D dinámicos | 2 |
| Rigidbody2D cinemáticos | 0 |
| Componentes de lógica MonoBehaviour | 29 |
| Plataformas unidireccionales | 1 |
| Trampas `DeathTrap` | 3 |
| Enemigos | 1 |
| Plataformas móviles | 0 |

### Configuración del jugador

| Propiedad | Valor |
|---|---|
| Velocidad | 6.5 |
| Multiplicador al correr | 1.28 |
| Fuerza de salto | 12 |
| Velocidad de escalada | 4.5 |
| Aceleración / desaceleración | 55 / 70 |
| Control aéreo | 0.75 |
| Coyote time / jump buffer | 0.12 s / 0.12 s |
| Radio de suelo | 0.18; máscara `Ground` |
| Collider de pie | Box 0,52 × 1,35; al agacharse usa 58 % de altura |
| Mecánica secundaria | Impulso umbral con `Q`; velocidad 16.5; duración 0.16 s; recarga 1.15 s |

### Cámara

Ortográfica, tamaño 4.7, suavizado 9, límites desde (-7, -1) hasta (19, 3.5). Incluye `AudioListener` y trauma visual.
El escenario utiliza 5 capas `ParallaxLayer2D`. Sus factores horizontales son 0.12, 0.42, 0.28; la niebla incorpora deriva ambiental independiente.

### Objetos físicos, colliders y triggers

Los tamaños `local` son los valores del componente; `mundo` incluye la escala del Transform.

| GameObject | Posición | Escala | Layer | Collider | Trigger | Rigidbody2D | Datos físicos | Componentes relevantes |
|---|---|---|---|---|---|---|---|---|
| `Player` | (-7, -1.45) | (1, 1) | `Default` | Box local 0.52 × 1.35; mundo 0.52 × 1.35 | No | Dynamic | masa 1, gravedad 3, Continuous, Interpolate, rotación Z bloqueada, material `Player_NoFriction` (fricción 0, rebote 0) | PlayerController2D, ShadowDash2D, PlayerRespawn, PlayerSpriteAnimator |
| `Terrain Start` | (-4, -2.65) | (1, 1) | `Ground` | Box local 10 × 0.62; mundo 10 × 0.62 | No | — | — | — |
| `Low Tunnel Ceiling` | (-2.8, -0.75) | (1, 1) | `Ground` | Box local 4.2 × 0.7; mundo 4.2 × 0.7 | No | — | — | — |
| `Horror Event a` | (0, 1.2) | (1, 1) | `Default` | Box local 1.2 × 6; mundo 1.2 × 6 | Sí | — | — | HorrorEvent2D |
| `Checkpoint` | (2.1, -1.3) | (0.72, 0.72) | `Default` | Box local 0.8 × 1.7; mundo 0.58 × 1.22 | Sí | — | — | Checkpoint |
| `Push Box` | (3, -1.55) | (0.72, 0.72) | `Ground` | Box local 1.35 × 1.35; mundo 0.97 × 0.97 | No | Dynamic | masa 3.2, gravedad 1, Continuous, Interpolate, rotación Z bloqueada | PushPullObject2D |
| `Lever` | (3.55, -1.15) | (0.52, 0.52) | `Default` | Box local 1.25 × 1.45; mundo 0.65 × 0.75 | Sí | — | — | LeverSwitch2D |
| `Moving Saw` | (5, -1.15) | (0.58, 0.58) | `Default` | Box local 1.2 × 1.2; mundo 0.7 × 0.7 | Sí | — | — | DeathTrap, SimpleMover2D |
| `Climb Zone` | (6, -1.15) | (0.72, 1.45) | `Default` | Box local 0.85 × 3.1; mundo 0.61 × 4.5 | Sí | — | — | ClimbZone2D |
| `Terrain Middle` | (6, -2.65) | (1, 1) | `Ground` | Box local 8 × 0.62; mundo 8 × 0.62 | No | — | — | — |
| `Factory Catwalk` | (7.5, 0.2) | (1, 1) | `Ground` | Box local 4.8 × 0.39; mundo 4.8 × 0.39 | No | — | usa effector | PlatformEffector2D |
| `Bottomless Fog` | (8, -7) | (38, 1) | `Default` | Box local 1 × 1; mundo 38 × 1 | Sí | — | — | DeathTrap |
| `Memory Echo 3` | (8.5, -0.65) | (1, 1) | `Default` | Box local 2.4 × 4.2; mundo 2.4 × 4.2 | Sí | — | — | NarrativeEcho2D |
| `Key` | (9, 1.15) | (0.3, 0.3) | `Default` | Box local 1.8 × 1.8; mundo 0.54 × 0.54 | Sí | — | — | CollectKey, CollectibleFloat2D |
| `Moving Saw` | (11.8, -1.2) | (0.58, 0.58) | `Default` | Box local 1.2 × 1.2; mundo 0.7 × 0.7 | Sí | — | — | DeathTrap, SimpleMover2D |
| `Hiding Alcove` | (12.3, -1.05) | (0.9, 1.15) | `Default` | Box local 1.25 × 1.65; mundo 1.13 × 1.9 | Sí | — | — | HidingSpot2D |
| `Flesh Sentinel` | (14.5, -1.2) | (1.05, 1.2) | `Default` | Box local 0.75 × 1.35; mundo 0.79 × 1.62 | Sí | — | — | EnemyAI2D |
| `Terrain End` | (15.5, -2.65) | (1, 1) | `Ground` | Box local 7 × 0.62; mundo 7 × 0.62 | No | — | — | — |
| `Locked Door` | (16, -1.25) | (1.05, 1.05) | `Default` | Box local 0.85 × 1.75; mundo 0.89 × 1.84 | No | — | — | DoorGoal |
| `Finish Zone` | (18.2, -1.15) | (1.05, 1.05) | `Default` | Box local 0.9 × 1.8; mundo 0.94 × 1.89 | Sí | — | — | FinishZone |

### Ajustes de IA, plataformas y mecanismos

| Objeto | Tipo | Configuración |
|---|---|---|
| `Flesh Sentinel` | IA GrotesqueDemon | patrulla 1.2; velocidad 1.7; persecución 4.4; visión 7.2; búsqueda 3.4 s; obstáculos `Ground` |
| `Moving Saw` | Trampa móvil | offset (0, 2.2); velocidad 1.55 |
| `Moving Saw` | Trampa móvil | offset (2.4, 0); velocidad 1.9 |
| `Factory Catwalk` | Plataforma unidireccional | one-way=Sí; agrupación=Sí; arco superficial 175° |
| `Lever` | Palanca | tecla E; controla `Moving Saw` |

### Componentes de gameplay presentes

- `CameraFollow2D`: 1
- `Checkpoint`: 1
- `ClimbZone2D`: 1
- `CollectibleFloat2D`: 1
- `CollectKey`: 1
- `DeathTrap`: 3
- `DoorGoal`: 1
- `EnemyAI2D`: 1
- `FinishZone`: 1
- `GameManager`: 1
- `HidingSpot2D`: 1
- `HorrorEvent2D`: 1
- `LeverSwitch2D`: 1
- `NarrativeEcho2D`: 1
- `ParallaxLayer2D`: 5
- `PlayerController2D`: 1
- `PlayerRespawn`: 1
- `PlayerSpriteAnimator`: 1
- `PushPullObject2D`: 1
- `ShadowDash2D`: 1
- `SimpleMover2D`: 2
- `UmbraAudio`: 1

## Chapter 04 El Peso

| Elemento | Cantidad |
|---|---:|
| GameObjects | 34 |
| Collider2D totales | 20 |
| Colliders sólidos | 10 |
| Triggers | 10 |
| Rigidbody2D dinámicos | 1 |
| Rigidbody2D cinemáticos | 3 |
| Componentes de lógica MonoBehaviour | 27 |
| Plataformas unidireccionales | 1 |
| Trampas `DeathTrap` | 2 |
| Enemigos | 1 |
| Plataformas móviles | 3 |

### Configuración del jugador

| Propiedad | Valor |
|---|---|
| Velocidad | 6.5 |
| Multiplicador al correr | 1.28 |
| Fuerza de salto | 12 |
| Velocidad de escalada | 4.5 |
| Aceleración / desaceleración | 55 / 70 |
| Control aéreo | 0.75 |
| Coyote time / jump buffer | 0.12 s / 0.12 s |
| Radio de suelo | 0.18; máscara `Ground` |
| Collider de pie | Box 0,52 × 1,35; al agacharse usa 58 % de altura |
| Mecánica secundaria | Impulso umbral con `Q`; velocidad 16.5; duración 0.16 s; recarga 1.15 s |

### Cámara

Ortográfica, tamaño 4.7, suavizado 9, límites desde (-7, -1) hasta (19, 3.5). Incluye `AudioListener` y trauma visual.
El escenario utiliza 5 capas `ParallaxLayer2D`. Sus factores horizontales son 0.12, 0.28, 0.42; la niebla incorpora deriva ambiental independiente.

### Objetos físicos, colliders y triggers

Los tamaños `local` son los valores del componente; `mundo` incluye la escala del Transform.

| GameObject | Posición | Escala | Layer | Collider | Trigger | Rigidbody2D | Datos físicos | Componentes relevantes |
|---|---|---|---|---|---|---|---|---|
| `Player` | (-7, -1.45) | (1, 1) | `Default` | Box local 0.52 × 1.35; mundo 0.52 × 1.35 | No | Dynamic | masa 1, gravedad 3, Continuous, Interpolate, rotación Z bloqueada, material `Player_NoFriction` (fricción 0, rebote 0) | PlayerController2D, ShadowDash2D, PlayerRespawn, PlayerSpriteAnimator |
| `Terrain Start` | (-6, -2.65) | (1, 1) | `Ground` | Box local 6 × 0.62; mundo 6 × 0.62 | No | — | — | — |
| `Cavern Lift A` | (-2.2, -2) | (1, 1) | `Ground` | Box local 2 × 0.35; mundo 2 × 0.35 | No | Kinematic | masa 1, gravedad 1, Discrete, None | MovingPlatform2D |
| `Terrain Shelf` | (1, -0.55) | (1, 1) | `Ground` | Box local 4 × 0.43; mundo 4 × 0.43 | No | — | — | — |
| `Cavern Bridge` | (3.5, -0.8) | (1, 1) | `Ground` | Box local 2 × 0.35; mundo 2 × 0.35 | No | Kinematic | masa 1, gravedad 1, Discrete, None | MovingPlatform2D |
| `Horror Event a` | (5.8, 1.5) | (1, 1) | `Default` | Box local 1.2 × 6; mundo 1.2 × 6 | Sí | — | — | HorrorEvent2D |
| `Checkpoint` | (7, -1.3) | (0.72, 0.72) | `Default` | Box local 0.8 × 1.7; mundo 0.58 × 1.22 | Sí | — | — | Checkpoint |
| `Terrain Middle` | (7.5, -2.65) | (1, 1) | `Ground` | Box local 5 × 0.62; mundo 5 × 0.62 | No | — | — | — |
| `Bottomless Fog` | (8, -7) | (38, 1) | `Default` | Box local 1 × 1; mundo 38 × 1 | Sí | — | — | DeathTrap |
| `Spike Trap` | (8.7, -1.85) | (1.05, 0.58) | `Default` | Box local 1.65 × 0.65; mundo 1.73 × 0.38 | Sí | — | — | DeathTrap |
| `Cavern Lift B` | (10.7, -2) | (1, 1) | `Ground` | Box local 2 × 0.35; mundo 2 × 0.35 | No | Kinematic | masa 1, gravedad 1, Discrete, None | MovingPlatform2D |
| `Climb Zone` | (12.2, -1.1) | (0.72, 1.45) | `Default` | Box local 0.85 × 3.1; mundo 0.61 × 4.5 | Sí | — | — | ClimbZone2D |
| `Memory Echo 4` | (12.8, -0.65) | (1, 1) | `Default` | Box local 2.4 × 4.2; mundo 2.4 × 4.2 | Sí | — | — | NarrativeEcho2D |
| `Key Ledge` | (13.5, 0.55) | (1, 1) | `Ground` | Box local 3.5 × 0.39; mundo 3.5 × 0.39 | No | — | usa effector | PlatformEffector2D |
| `Key` | (13.7, 1.45) | (0.3, 0.3) | `Default` | Box local 1.8 × 1.8; mundo 0.54 × 0.54 | Sí | — | — | CollectKey, CollectibleFloat2D |
| `Hiding Alcove` | (14, -1.05) | (0.9, 1.15) | `Default` | Box local 1.25 × 1.65; mundo 1.13 × 1.9 | Sí | — | — | HidingSpot2D |
| `Guilt Bearer` | (16, -1.25) | (1.05, 1.2) | `Default` | Box local 0.75 × 1.35; mundo 0.79 × 1.62 | Sí | — | — | EnemyAI2D |
| `Terrain End` | (16, -2.65) | (1, 1) | `Ground` | Box local 8 × 0.62; mundo 8 × 0.62 | No | — | — | — |
| `Locked Door` | (18, -1.25) | (1.05, 1.05) | `Default` | Box local 0.85 × 1.75; mundo 0.89 × 1.84 | No | — | — | DoorGoal |
| `Finish Zone` | (19.35, -1.15) | (1.05, 1.05) | `Default` | Box local 0.9 × 1.8; mundo 0.94 × 1.89 | Sí | — | — | FinishZone |

### Ajustes de IA, plataformas y mecanismos

| Objeto | Tipo | Configuración |
|---|---|---|
| `Guilt Bearer` | IA GrotesqueDemon | patrulla 2.7; velocidad 1.7; persecución 4.9; visión 7.2; búsqueda 3.4 s; obstáculos `Ground` |
| `Cavern Lift A` | Plataforma móvil | offset (0, 2.5); velocidad 1.2; Rigidbody Kinematic |
| `Cavern Lift B` | Plataforma móvil | offset (0, 2.7); velocidad 1.3; Rigidbody Kinematic |
| `Cavern Bridge` | Plataforma móvil | offset (2.5, -1.1); velocidad 1.1; Rigidbody Kinematic |
| `Key Ledge` | Plataforma unidireccional | one-way=Sí; agrupación=Sí; arco superficial 175° |

### Componentes de gameplay presentes

- `CameraFollow2D`: 1
- `Checkpoint`: 1
- `ClimbZone2D`: 1
- `CollectibleFloat2D`: 1
- `CollectKey`: 1
- `DeathTrap`: 2
- `DoorGoal`: 1
- `EnemyAI2D`: 1
- `FinishZone`: 1
- `GameManager`: 1
- `HidingSpot2D`: 1
- `HorrorEvent2D`: 1
- `MovingPlatform2D`: 3
- `NarrativeEcho2D`: 1
- `ParallaxLayer2D`: 5
- `PlayerController2D`: 1
- `PlayerRespawn`: 1
- `PlayerSpriteAnimator`: 1
- `ShadowDash2D`: 1
- `UmbraAudio`: 1

## Chapter 05 El Umbral

| Elemento | Cantidad |
|---|---:|
| GameObjects | 37 |
| Collider2D totales | 22 |
| Colliders sólidos | 8 |
| Triggers | 14 |
| Rigidbody2D dinámicos | 2 |
| Rigidbody2D cinemáticos | 1 |
| Componentes de lógica MonoBehaviour | 31 |
| Plataformas unidireccionales | 1 |
| Trampas `DeathTrap` | 3 |
| Enemigos | 1 |
| Plataformas móviles | 1 |

### Configuración del jugador

| Propiedad | Valor |
|---|---|
| Velocidad | 6.5 |
| Multiplicador al correr | 1.28 |
| Fuerza de salto | 12 |
| Velocidad de escalada | 4.5 |
| Aceleración / desaceleración | 55 / 70 |
| Control aéreo | 0.75 |
| Coyote time / jump buffer | 0.12 s / 0.12 s |
| Radio de suelo | 0.18; máscara `Ground` |
| Collider de pie | Box 0,52 × 1,35; al agacharse usa 58 % de altura |
| Mecánica secundaria | Impulso umbral con `Q`; velocidad 16.5; duración 0.16 s; recarga 1.15 s |

### Cámara

Ortográfica, tamaño 4.7, suavizado 9, límites desde (-7, -1) hasta (22, 3.5). Incluye `AudioListener` y trauma visual.
El escenario utiliza 5 capas `ParallaxLayer2D`. Sus factores horizontales son 0.12, 0.42, 0.28; la niebla incorpora deriva ambiental independiente.

### Objetos físicos, colliders y triggers

Los tamaños `local` son los valores del componente; `mundo` incluye la escala del Transform.

| GameObject | Posición | Escala | Layer | Collider | Trigger | Rigidbody2D | Datos físicos | Componentes relevantes |
|---|---|---|---|---|---|---|---|---|
| `Player` | (-7, -1.45) | (1, 1) | `Default` | Box local 0.52 × 1.35; mundo 0.52 × 1.35 | No | Dynamic | masa 1, gravedad 3, Continuous, Interpolate, rotación Z bloqueada, material `Player_NoFriction` (fricción 0, rebote 0) | PlayerController2D, ShadowDash2D, PlayerRespawn, PlayerSpriteAnimator |
| `Terrain Start` | (-5, -2.65) | (1, 1) | `Ground` | Box local 8 × 0.62; mundo 8 × 0.62 | No | — | — | — |
| `Push Box` | (-4, -1.55) | (0.72, 0.72) | `Ground` | Box local 1.35 × 1.35; mundo 0.97 × 0.97 | No | Dynamic | masa 3.2, gravedad 1, Continuous, Interpolate, rotación Z bloqueada | PushPullObject2D |
| `Pressure Switch` | (-2.2, -2.12) | (0.72, 0.45) | `Default` | Box local 1.65 × 0.6; mundo 1.19 × 0.27 | Sí | — | — | PressureSwitch2D |
| `Spike Trap` | (3.6, -1.85) | (1.05, 0.58) | `Default` | Box local 1.65 × 0.65; mundo 1.73 × 0.38 | Sí | — | — | DeathTrap |
| `Terrain Middle` | (4, -2.65) | (1, 1) | `Ground` | Box local 8 × 0.62; mundo 8 × 0.62 | No | — | — | — |
| `Checkpoint` | (6.2, -1.3) | (0.72, 0.72) | `Default` | Box local 0.8 × 1.7; mundo 0.58 × 1.22 | Sí | — | — | Checkpoint |
| `Horror Event a` | (7.4, 1.2) | (1, 1) | `Default` | Box local 1.2 × 6; mundo 1.2 × 6 | Sí | — | — | HorrorEvent2D |
| `Bottomless Fog` | (8, -7) | (38, 1) | `Default` | Box local 1 × 1; mundo 38 × 1 | Sí | — | — | DeathTrap |
| `Escape Bridge` | (8.2, -1) | (1, 1) | `Ground` | Box local 2.2 × 0.35; mundo 2.2 × 0.35 | No | Kinematic | masa 1, gravedad 1, Discrete, None | MovingPlatform2D |
| `Hiding Alcove` | (9.5, -1.05) | (0.9, 1.15) | `Default` | Box local 1.25 × 1.65; mundo 1.13 × 1.9 | Sí | — | — | HidingSpot2D |
| `Climb Zone` | (11, -1.1) | (0.72, 1.45) | `Default` | Box local 0.85 × 3.1; mundo 0.61 × 4.5 | Sí | — | — | ClimbZone2D |
| `Lever` | (12, 1.2) | (0.52, 0.52) | `Default` | Box local 1.25 × 1.45; mundo 0.65 × 0.75 | Sí | — | — | LeverSwitch2D |
| `Escape Upper` | (12.5, 0.4) | (1, 1) | `Ground` | Box local 5 × 0.43; mundo 5 × 0.43 | No | — | usa effector | PlatformEffector2D |
| `Key` | (14, 1.35) | (0.3, 0.3) | `Default` | Box local 1.8 × 1.8; mundo 0.54 × 0.54 | Sí | — | — | CollectKey, CollectibleFloat2D |
| `Terrain Final` | (15, -2.65) | (1, 1) | `Ground` | Box local 12 × 0.62; mundo 12 × 0.62 | No | — | — | — |
| `Moving Saw` | (15.8, -1.15) | (0.58, 0.58) | `Default` | Box local 1.2 × 1.2; mundo 0.7 × 0.7 | Sí | — | — | DeathTrap, SimpleMover2D |
| `Threshold Demon` | (16.6, -1.2) | (1.05, 1.2) | `Default` | Box local 0.75 × 1.35; mundo 0.79 × 1.62 | Sí | — | — | EnemyAI2D |
| `Horror Event b` | (17.8, -0.7) | (1, 1) | `Default` | Box local 1.2 × 6; mundo 1.2 × 6 | Sí | — | — | HorrorEvent2D |
| `Memory Echo 5` | (18.8, -0.65) | (1, 1) | `Default` | Box local 2.4 × 4.2; mundo 2.4 × 4.2 | Sí | — | — | NarrativeEcho2D |
| `Locked Door` | (19, -1.25) | (1.05, 1.05) | `Default` | Box local 0.85 × 1.75; mundo 0.89 × 1.84 | No | — | — | DoorGoal |
| `Finish Zone` | (20.25, -1.15) | (1.05, 1.05) | `Default` | Box local 0.9 × 1.8; mundo 0.94 × 1.89 | Sí | — | — | FinishZone |

### Ajustes de IA, plataformas y mecanismos

| Objeto | Tipo | Configuración |
|---|---|---|
| `Threshold Demon` | IA GrotesqueDemon | patrulla 3; velocidad 1.7; persecución 5.2; visión 7.2; búsqueda 3.4 s; obstáculos `Ground` |
| `Escape Bridge` | Plataforma móvil | offset (2.2, 1.2); velocidad 1.3; Rigidbody Kinematic |
| `Moving Saw` | Trampa móvil | offset (0, 2.5); velocidad 1.8 |
| `Escape Upper` | Plataforma unidireccional | one-way=Sí; agrupación=Sí; arco superficial 175° |
| `Pressure Switch` | Placa | detecta `PushPullObject2D`; controla `Spike Trap` |
| `Lever` | Palanca | tecla E; controla `Moving Saw` |

### Componentes de gameplay presentes

- `CameraFollow2D`: 1
- `Checkpoint`: 1
- `ClimbZone2D`: 1
- `CollectibleFloat2D`: 1
- `CollectKey`: 1
- `DeathTrap`: 3
- `DoorGoal`: 1
- `EnemyAI2D`: 1
- `FinishZone`: 1
- `GameManager`: 1
- `HidingSpot2D`: 1
- `HorrorEvent2D`: 2
- `LeverSwitch2D`: 1
- `MovingPlatform2D`: 1
- `NarrativeEcho2D`: 1
- `ParallaxLayer2D`: 5
- `PlayerController2D`: 1
- `PlayerRespawn`: 1
- `PlayerSpriteAnimator`: 1
- `PressureSwitch2D`: 1
- `PushPullObject2D`: 1
- `ShadowDash2D`: 1
- `SimpleMover2D`: 1
- `UmbraAudio`: 1

## Significado de la configuración

- `Trigger = Sí`: detecta entradas/salidas y ejecuta lógica, pero no bloquea físicamente.
- `Trigger = No`: forma una superficie o barrera sólida.
- `Dynamic`: Rigidbody afectado por gravedad y fuerzas; se usa para jugador y cajas.
- `Kinematic`: Rigidbody movido por código; se usa para plataformas móviles.
- Sin Rigidbody: objeto estático o sensor; los triggers funcionan porque el jugador sí posee Rigidbody.
- `Continuous`: reduce el riesgo de atravesar colliders a velocidades altas.
- `Interpolate`: suaviza visualmente el movimiento entre pasos físicos.
- Las plataformas superiores usan `PlatformEffector2D`: se atraviesan desde abajo y sostienen desde arriba.
