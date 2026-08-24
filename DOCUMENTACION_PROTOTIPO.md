# UMBRA - Documentacion del prototipo

## Alcance actual

El proyecto actual contiene cinco niveles jugables conectados y cubre el alcance
de niveles descrito para UMBRA.

## Flujo jugable

1. Bosque: caja, interruptor, pinchos, escalera y llave.
2. Ruinas: plataformas moviles, palanca y rutas elevadas.
3. Fabrica: tunel agachado, sierras y pasarela industrial.
4. Cavernas: ascensores, abismos y plataformas de precision.
5. Escape: combina caja, interruptor, palanca, trampas y salida final.

## Scripts principales

- `PlayerController2D`: caminar, saltar, agacharse, trepar e interactuar.
- `PushPullObject2D`: jalar la caja cercana mientras se mantiene `E`.
- `PressureSwitch2D`: desactivar la trampa cuando la caja esta encima.
- `PlayerRespawn` y `Checkpoint`: muerte, reaparicion y guardado automatico.
- `GameManager`: menu, pausa, progreso, muerte, final y reinicio.
- `UmbraAudio`: viento y efectos generados en tiempo de ejecucion.
- `UmbraPrototypeBuilder`: construccion y validacion automatica de cinco escenas.
- `UmbraRuntimeDiagnostics`: prueba automatica de referencias en los cinco niveles.

## Arte y audio

Los cinco fondos, el personaje y la hoja de doce objetos son recursos originales
del prototipo. El terreno usa una textura irregular repetible y el audio se genera
por codigo para mantener el proyecto gratuito y autocontenido.

## Ejecucion

Abrir `Assets/Scenes/Level_01_Forest.unity` en Unity 6 y presionar Play. Tambien
existe una compilacion para Windows en `Builds/Windows/UMBRA.exe`.
