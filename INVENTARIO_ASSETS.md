# UMBRAL — Inventario completo de Assets

Actualizado: 7 de septiembre de 2026  
Proyecto: Unity 6000.5.2f1, Windows 64 bits

## Resumen

`Assets` contiene 88 archivos de contenido (23,77 MB) y 103 archivos `.meta` administrados por Unity.

| Tipo | Cantidad | Uso |
|---|---:|---|
| PNG | 50 | Fondos, personaje, criaturas, utilería, pistas hospitalarias y texturas básicas |
| C# | 26 | 24 componentes de juego y 2 herramientas de editor |
| Escenas Unity | 10 | 5 capítulos activos y 5 escenas antiguas de referencia |
| Physics Material 2D | 1 | Evita que el jugador se adhiera a paredes |
| Archivo de control | 1 | Registra que la generación inicial del proyecto terminó |

La carpeta `Assets/Audio` no contiene clips importados. `UmbraAudio.cs` genera en ejecución el viento,
pasos, salto, caída, muerte, recogida, mecanismos, susto y respiración.

## Escenas

Las cinco escenas activas del build, en orden, son:

| Capítulo | Archivo | Contenido principal |
|---|---|---|
| 1 — El Fondo | `Scenes/Chapter_01_El_Fondo.unity` | Bosque, caja, placa, pinchos, checkpoint, escalera, alma, llave, puerta y salida |
| 2 — Los Abandonados | `Scenes/Chapter_02_Los_Abandonados.unity` | Ruinas, dos puentes móviles, escondite, alma, escalera, sierra/palanca, llave y salida |
| 3 — La Carne | `Scenes/Chapter_03_La_Carne.unity` | Fábrica, túnel para agacharse, caja, dos sierras, palanca segura, escalera y demonio |
| 4 — El Peso | `Scenes/Chapter_04_El_Peso.unity` | Cavernas, tres plataformas móviles, pinchos, escalera, llave y demonio |
| 5 — El Umbral | `Scenes/Chapter_05_El_Umbral.unity` | Escape, caja/placa, puente, escalera, sierra separada, dos eventos de terror y final hospitalario |

Estas cinco escenas antiguas permanecen como referencia y no forman parte del build:

- `Scenes/Level_01_Forest.unity`
- `Scenes/Level_02_Ruins.unity`
- `Scenes/Level_03_Factory.unity`
- `Scenes/Level_04_Caverns.unity`
- `Scenes/Level_05_Escape.unity`

## Arte

### Fondos

Cada fondo mide 1672 × 941 px y se repite tres veces para cubrir el capítulo.

| Archivo | Capítulo | Tamaño aproximado |
|---|---|---:|
| `Art/Backgrounds/foggy_forest.png` | 1 | 1727,8 KB |
| `Art/Backgrounds/foggy_ruins.png` | 2 | 1766,3 KB |
| `Art/Backgrounds/abandoned_factory.png` | 3 | 1907,6 KB |
| `Art/Backgrounds/deep_caverns.png` | 4 | 1940,4 KB |
| `Art/Backgrounds/final_escape.png` | 5 | 1682,4 KB |

### Personaje

- `Art/Character/umbra_character_sheet.png`: hoja fuente de 1448 × 1086 px.
- `Art/Character/Frames/character_00.png` a `character_03.png`: animación idle.
- `Art/Character/Frames/character_04.png` a `character_07.png`: correr y trepar.
- `Art/Character/Frames/character_08.png` y `character_09.png`: salto/subida y caída.
- `Art/Character/Frames/character_10.png` y `character_11.png`: agacharse.
- Cada frame mide 362 × 362 px. Los doce frames están usados por los cinco capítulos.

### Criaturas

- `Art/Creatures/umbral_creatures_sheet.png`: hoja fuente de 1224 × 1285 px.
- `Art/Creatures/Frames/creature_00.png`: alma de pena, usada en los capítulos 1 y 2.
- `Art/Creatures/Frames/creature_01.png`: demonio grotesco, usado en los capítulos 3, 4 y 5.
- `Art/Creatures/Frames/creature_02.png`: escondite/armario, usado en los cinco capítulos.
- `Art/Creatures/Frames/creature_03.png`: manifestaciones de terror, seis usos en total.
- Cada frame mide 612 × 642 px.

### Pistas hospitalarias

- `Art/Hospital/umbral_hospital_clues_sheet.png`: hoja fuente de 1536 × 1024 px.
- `hospital_clue_00.png`: soporte de suero doblado, capítulo 1.
- `hospital_clue_01.png`: cable de pulso, capítulo 2.
- `hospital_clue_02.png`: pulsera hospitalaria, capítulo 3.
- `hospital_clue_03.png`: placa de desfibrilador, capítulo 4.
- `hospital_clue_04.png`: baranda blanca de cama, capítulo 5.
- `hospital_clue_05.png`: sexto recorte disponible, todavía sin colocar.
- Cada recorte mide 512 × 512 px.

### Utilería

- `Art/Props/umbra_props_sheet.png`: hoja fuente de 1448 × 1086 px.
- `prop_00.png`: caja empujable.
- `prop_01.png`: placa de presión.
- `prop_02.png`: checkpoint/linterna.
- `prop_03.png`: puerta cerrada.
- `prop_04.png`: escalera.
- `prop_05.png`: pinchos.
- `prop_06.png`: sierra móvil.
- `prop_07.png`: llave/sello.
- `prop_08.png`: pilares y decoración arruinada.
- `prop_09.png`: jaula colgante y decoración.
- `prop_10.png`: portal de salida.
- `prop_11.png`: palanca.
- Cada frame mide 362 × 362 px y todos tienen al menos un uso en los capítulos activos.

### Texturas y elementos auxiliares

| Archivo | Medidas | Estado |
|---|---:|---|
| `Art/terrain_forest_tile.png` | 256 × 64 | Terreno de todos los niveles; 28 referencias activas |
| `Art/black_square.png` | 16 × 16 | Niebla mortal inferior; 5 referencias activas |
| `Art/gray_square.png` | 16 × 16 | Bandas de niebla; 10 referencias activas |
| `Art/dark_gray_square.png` | 16 × 16 | Recurso auxiliar sin referencia directa actual |
| `Art/light_square.png` | 16 × 16 | Recurso auxiliar sin referencia directa actual |
| `Art/spike_triangle.png` | 32 × 32 | Recurso antiguo; los pinchos actuales usan `prop_05` |
| `Art/Items/old_iron_key.png` | 1774 × 887 | Concepto antiguo; la llave activa usa `prop_07` |
| `Art/Player_NoFriction.physicsMaterial2D` | — | Asignado al jugador en los cinco capítulos |

Las hojas fuente no aparecen directamente en las escenas porque el constructor genera y asigna sus
frames individuales. No deben confundirse con recursos sin uso.

## Scripts de juego

| Script | Responsabilidad |
|---|---|
| `AwakeningSequence2D.cs` | Despertar inicial y fundido del protagonista |
| `CameraFollow2D.cs` | Seguimiento suavizado, límites y trauma de cámara |
| `Checkpoint.cs` | Activa y guarda el punto de reaparición |
| `ClimbZone2D.cs` | Entrada y salida de zonas de escalera |
| `CollectibleFloat2D.cs` | Flotación e inclinación visual de objetos coleccionables |
| `CollectKey.cs` | Recoge y persiste la llave/sello del capítulo |
| `DeathTrap.cs` | Mata al jugador y permite armar/desarmar trampas |
| `DoorGoal.cs` | Puerta bloqueada por llave e interacción con `E` |
| `EnemyAI2D.cs` | Patrulla, visión, persecución, búsqueda y retorno |
| `FinishZone.cs` | Completa el capítulo al entrar en el portal |
| `GameManager.cs` | Flujo del juego, pausa, guardado, UI, capítulos y final |
| `HidingSpot2D.cs` | Ocultamiento y salida con `E` |
| `HorrorEvent2D.cs` | Eventos persistentes de terror y manifestaciones |
| `LeverSwitch2D.cs` | Desactiva una trampa mediante palanca |
| `MovingPlatform2D.cs` | Movimiento de puentes/elevadores y transporte del jugador |
| `NarrativeEcho2D.cs` | Muestra recuerdos narrativos al cruzar zonas |
| `PlayerController2D.cs` | Movimiento, carrera, salto, coyote time, buffer, escalera y agachado |
| `PlayerRespawn.cs` | Muerte, checkpoint y reaparición |
| `PlayerSpriteAnimator.cs` | Selección de frames idle, carrera, salto y agachado |
| `PressureSwitch2D.cs` | Detecta la caja y controla una trampa |
| `PushPullObject2D.cs` | Empujar/jalar cajas con aceleración y freno |
| `SimpleMover2D.cs` | Movimiento sinusoidal de sierras |
| `UmbraAudio.cs` | Generación procedural y reproducción de todo el audio |
| `UmbraRuntimeDiagnostics.cs` | Pruebas automáticas de escenas, física y sistemas |

Los 24 scripts de ejecución y las 2 herramientas de editor suman aproximadamente 3680 líneas.

## Herramientas de editor y control

- `Editor/UmbraPrototypeBuilder.cs`: genera los cinco capítulos, configura el build, valida geometría,
  referencias, rutas, soportes, peligros de escalera y captura vistas de QA.
- `Editor/UmbraTechnicalReport.cs`: inspecciona capas, matriz de colisión, colliders, triggers,
  Rigidbody2D, materiales, mecanismos, IA y componentes, y regenera `REPORTE_TECNICO_UNITY.md`.
- `UMBRA_SETUP_DONE.txt`: versión de control utilizada para evitar reconstrucciones automáticas innecesarias.
- Los 103 archivos `.meta` conservan GUID, importación y referencias de Unity. No deben borrarse ni editarse
  manualmente.

## Estado de uso

- Activos en el juego: las cinco escenas `Chapter_*`, todos los scripts, los doce frames del personaje,
  los cuatro frames de criaturas, los doce props, cinco pistas hospitalarias, cinco fondos, terreno,
  niebla y el material sin fricción.
- Fuentes necesarias para regeneración: las cuatro hojas `*_sheet.png`.
- Disponibles sin colocación actual: `hospital_clue_05.png`, `dark_gray_square.png` y `light_square.png`.
- Recursos antiguos conservados: `spike_triangle.png`, `Items/old_iron_key.png` y las cinco escenas `Level_*`.
