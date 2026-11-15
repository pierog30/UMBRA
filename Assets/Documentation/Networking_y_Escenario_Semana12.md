# Semana 12 Networking y escenario dinámico

## Decisión de alcance

UMBRA mantiene un diseño para un jugador. El recorrido, la cámara, los checkpoints, la llave, la puerta, los eventos de terror y la inteligencia de enemigos dependen de una sola instancia local de `PlayerController2D`. El proyecto no contiene transporte de red, autenticación, salas, autoridad de servidor ni sincronización de estados.

Implementar multijugador en tiempo real durante este hito no aporta valor a la propuesta narrativa y obligaría a rediseñar la cámara, la progresión, el reinicio desde checkpoint, la física de objetos, los cambios de escena y la autoridad de los enemigos. Por esta razón, la Semana 12 presenta un análisis de viabilidad en lugar de integrar un paquete de networking.

## Alternativas evaluadas

| Alternativa | Viabilidad | Impacto técnico | Decisión |
| --- | --- | --- | --- |
| Cooperativo en tiempo real | Baja para el alcance actual | Requiere autoridad, sincronización de física, cámara compartida y recuperación ante desconexiones | No implementar |
| Fantasma de una partida anterior | Media | Guarda posiciones y tiempos sin sincronización simultánea | Posible mejora futura |
| Tabla de tiempos o logros | Alta | Envía resultados al finalizar un capítulo y no altera el gameplay | Mejor opción futura |

## Mejora del escenario

`HorrorEvent2D` utiliza cuatro estados observables: `Waiting`, `Activating`, `Active` y `Completed`. Cuando el jugador entra al trigger, el componente publica un evento, aplica trauma a la cámara y anima el objeto asociado con desplazamiento suavizado. La respuesta visual incorpora un pulso de escala y una transición temporal de color.

Los seis eventos configurados en las escenas cubren cinco respuestas distintas:

- Capítulo 1: `BackgroundRise`.
- Capítulo 2: `ForegroundRush`.
- Capítulo 3: `EnvironmentShift`.
- Capítulo 4: `SpawnThreat`.
- Capítulo 5: `ForegroundRush` y `CameraPulse`.

El Inspector expone `debugState`, `duration`, `motionCurve`, `pulseAmount`, `activeTint` y `persistForRun`. Estos campos permiten explicar la evolución del escenario durante Play sin depender únicamente de la percepción visual.

## Relación con los sistemas existentes

- `UmbraGameEvents` registra la activación del escenario.
- `UmbraEventJournal` muestra el evento recibido durante la demostración.
- `CameraFollow2D` aplica trauma visual según el tipo de evento.
- `PlayerPrefs` conserva los eventos persistentes durante la partida.
- `ParallaxLayer2D` mantiene movimiento ambiental continuo en fondos y niebla.
- `MovingPlatform2D`, `LeverSwitch2D` y `PressureSwitch2D` complementan la respuesta física del nivel.

## Criterio para una futura conexión en red

El equipo solo debería incorporar networking si una mecánica futura necesita interacción entre jugadores o comparación remota. Una tabla de tiempos asíncrona representa la primera prueba razonable porque no modifica la autoridad física ni la narrativa. Antes de implementarla se deben definir servicio, identidad del jugador, tratamiento de datos, manejo offline y validación de resultados.
