# UMBRA: El Archivo de los Ecos - Documentacion

## Alcance actual

El proyecto contiene cinco niveles jugables conectados. Se conserva el alcance
del documento inicial: plataformas 2D, exploracion, acertijos ambientales y un
recorrido simbolico por las etapas de la vida.

## Idea propia

Lumo despierta dentro de un archivo de recuerdos incompletos. Cada mundo esta
hecho con materiales artesanales y representa una etapa distinta. El objetivo no
es escapar de un bosque oscuro, sino reconstruir la memoria: activar mecanismos
de resonancia, recuperar un fragmento de eco y atravesar el portal de regreso.

## Flujo jugable

1. Jardin de las Primeras Voces: infancia, juego y primeras decisiones.
2. Ciudad de las Cartas No Enviadas: adolescencia y palabras pendientes.
3. Taller de las Horas Prestadas: adultez, trabajo y paso del tiempo.
4. Biblioteca Bajo la Lluvia: vejez, recuerdos ordenados y olvidados.
5. Observatorio de los Ecos que Regresan: aceptacion y cierre del archivo.

## Scripts principales

- `PlayerController2D`: caminar, saltar, agacharse, trepar e interactuar.
- `PushPullObject2D`: mover los cubos de memoria sin aceleraciones bruscas.
- `PressureSwitch2D`: activar una placa de resonancia con un cubo.
- `ResonanceLink2D`: comunicar visualmente que trampa pertenece a cada placa.
- `VisualPulse2D`: destacar objetos interactivos sin cambiar su mecanica.
- `DoorGoal`: controlar el umbral alto, su color y el bloqueo por fragmento.
- `PlayerRespawn` y `Checkpoint`: muerte, reaparicion y guardado automatico.
- `GameManager`: menu, pausa, progreso, muerte, final y reinicio.
- `UmbraAudio`: ambiente armonico y efectos generados en tiempo de ejecucion.
- `UmbraPrototypeBuilder`: construccion y validacion automatica de cinco escenas.
- `UmbraRuntimeDiagnostics`: prueba automatica de referencias en los cinco niveles.

## Arte y audio

Los cinco fondos, Lumo y la hoja de doce objetos fueron creados para esta version.
La direccion mezcla gouache, papel recortado, tela cosida y ceramica. El terreno
usa una textura repetible y el audio se genera por codigo para mantener el proyecto
gratuito y autocontenido.

Las interacciones usan un lenguaje de color sencillo: naranja indica un mecanismo
pendiente y turquesa indica que ya fue activado. Las escaleras tienen una baliza
luminosa, los cubos y fragmentos poseen contorno pulsante y las trampas se retraen
de forma visible en vez de desaparecer.

## Diferenciacion

La referencia a Limbo se limita al genero de plataformas narrativas con acertijos
ambientales. UMBRA evita su silueta infantil, el bosque monocromatico y la busqueda
de una salida. En su lugar usa un protagonista enmascarado de tela, color, cinco
recuerdos materiales y una meta centrada en reconstruir y aceptar la memoria.

## Ejecucion

Abrir `Assets/Scenes/Level_01_Forest.unity` en Unity 6 y presionar Play. Tambien
existe una compilacion para Windows en `Builds/Windows/UMBRA.exe`.
