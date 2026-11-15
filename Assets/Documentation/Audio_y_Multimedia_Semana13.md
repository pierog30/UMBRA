# UMBRA Semana 13 Audio y multimedia

## Arquitectura de audio

UMBRA utiliza un objeto `Audio Ambiente` con el componente `UmbraAudio` en cada uno de los cinco capítulos. El componente crea dos `AudioSource` en tiempo de ejecución. El primero reproduce ambiente continuo en bucle y el segundo atiende efectos mediante `PlayOneShot`. La cámara principal contiene el único `AudioListener` de la escena.

La separación evita que un efecto interrumpa el ambiente. El canal de ambiente usa volumen base 0,42 y el canal de efectos 0,80. El volumen maestro se aplica con `AudioListener.volume` y se conserva en `PlayerPrefs`. Semana 13 añade controles independientes para ambiente y efectos mediante `SetMixVolumes`.

## Recursos sonoros

Los sonidos se generan de forma procedural con `AudioClip.Create`, por lo que el proyecto no depende de archivos externos ni presenta problemas de licencia. El ambiente combina ruido filtrado y tonos graves. Los efectos usan tonos breves con envolvente senoidal para evitar cortes bruscos.

| Recurso | Reproducción | Finalidad |
| --- | --- | --- |
| Umbra Wind | Bucle continuo | Ambiente oscuro del escenario |
| Jump | Una vez | Confirma el salto |
| Step | Una vez | Refuerza el desplazamiento |
| Heavy landing | Una vez | Comunica una caída o aterrizaje |
| Pickup | Una vez | Confirma llaves y checkpoints |
| Mechanism | Una vez | Palancas, placas, puertas y dash |
| Death | Una vez | Informa la derrota y reduce el ambiente |
| Terror sting | Una vez | Enemigos, trampas y eventos de terror |
| Breath | Periódico | Mantiene tensión durante el control del jugador |

## Eventos del juego

- `PlayerController2D` reproduce salto, paso, aterrizaje y mecanismo.
- `GameManager` reproduce recogida y muerte.
- `Checkpoint` reproduce recogida.
- `DoorGoal`, `LeverSwitch2D`, `PressureSwitch2D` y `ShadowDash2D` reproducen mecanismo.
- `EnemyAI2D`, `HorrorEvent2D` y `TrapResponseStrategy` reproducen el golpe de terror.

Cada reproducción actualiza `LastEvent` y publica `SoundPlayed`. Esto permite conectar una interfaz, subtítulos o pruebas sin modificar los scripts que originan la acción.

## Mezcla dinámica

Los eventos `Scare` y `Death` activan una reducción temporal del ambiente. `UmbraAudio` baja el canal hasta `scareDuckVolume` y lo recupera gradualmente con `ambienceFadeSpeed`. El efecto principal queda más claro sin detener el bucle ambiental.

## Configuración por escena

Los capítulos `Chapter_01_El_Fondo` hasta `Chapter_05_El_Umbral` contienen un `AudioListener` habilitado en `Main Camera` y un componente `UmbraAudio` en `Audio Ambiente`. Los `AudioSource` aparecen durante Play porque el controlador los crea y configura en `Awake`.

## Demostración sugerida

1. Abrir un capítulo y comprobar que `Main Camera` tiene un solo `AudioListener`.
2. Seleccionar `Audio Ambiente` y mostrar `UmbraAudio`.
3. Ejecutar Play y comprobar los dos `AudioSource` creados en tiempo de ejecución.
4. Mover al jugador para escuchar pasos, salto y aterrizaje.
5. Obtener una llave o checkpoint para escuchar `Pickup`.
6. Activar una palanca, puerta o dash para escuchar `Mechanism`.
7. Acercarse a un enemigo o activar un evento para escuchar `Scare` y notar la reducción temporal del ambiente.
8. Abrir el menú de pausa, modificar el volumen maestro y confirmar que el valor persiste.

