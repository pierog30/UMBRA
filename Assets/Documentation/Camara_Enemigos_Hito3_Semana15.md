# UMBRA Semana 15 Cámara y enemigos Hito 3

## Objetivo técnico

El Hito 3 consolida el comportamiento de enemigos y la lectura visual del escenario. La IA mantiene estados explícitos y la cámara combina seguimiento, adelanto por velocidad, límites, zonas con zoom y respuesta a eventos de peligro.

## Comportamiento de enemigos

`EnemyAI2D` utiliza los estados Patrol, Watch, Chase, Search y Return. La percepción considera distancia, tolerancia vertical, cono de visión y un Raycast contra `obstacleMask`. La revisión de Semana 15 añade un tiempo breve de gracia al perder contacto para evitar cambios de estado inestables. El demonio también comprueba suelo y paredes antes de avanzar.

Cuando la IA ve al jugador, registra `lastSeen`. Si pierde la línea de visión, se dirige a esa posición, busca durante un intervalo y regresa al origen. El escondite sigue bloqueando la detección mediante `PlayerController2D.IsHidden`. Cada cambio de estado publica un evento y la entrada en Chase activa sonido de terror y trauma de cámara.

## Sistema de cámara

`CameraFollow2D` sigue al jugador en `LateUpdate` y calcula adelanto horizontal a partir de la velocidad del `Rigidbody2D`. El seguimiento usa interpolación exponencial para mantener una respuesta estable con diferentes tasas de cuadros. Los límites generales impiden mostrar áreas externas al escenario.

`CameraZone2D` define una transición local. Al entrar el jugador puede cambiar offset, zoom ortográfico, velocidad de transición y límites opcionales. Al salir, la cámara recupera el perfil general de forma gradual. Cada capítulo incluye una zona de amenaza centrada en una sección con enemigo.

La cámara también conserva `AddTrauma`. Los enemigos y eventos de terror aplican una vibración breve sin modificar la física del jugador.

## Integración por capítulo

El configurador `UmbraWeek15Setup` actualiza los cinco capítulos. Ajusta parámetros de IA, mantiene la cámara principal y crea una zona de amenaza por escena. No modifica las entradas, checkpoints, llaves, puertas, dash, parallax ni audio existentes.

## Demostración funcional

1. Iniciar un capítulo y caminar para mostrar seguimiento y adelanto horizontal.
2. Llegar a un extremo para comprobar que la cámara respeta los límites.
3. Entrar en la zona de amenaza y observar la transición de offset y zoom.
4. Acercarse al enemigo desde su frente para activar Watch y Chase.
5. Romper la línea de visión para mostrar Search y Return.
6. Usar un escondite para confirmar que la IA no detecta al jugador oculto.
7. Volver a provocar Chase y mostrar sonido y trauma de cámara.

## Pruebas previstas

- Compilación de scripts sin errores.
- Cinco escenas activas en Build Settings.
- Ejecutable Windows de 64 bits.
- Prueba de inicio del build.
- Verificación visual de zonas en Scene y transiciones durante Play.

