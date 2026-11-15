# UMBRA Semana 14 Adaptación móvil

## Decisión de plataforma

UMBRA mantiene Windows como plataforma objetivo del Hito 2 porque el prototipo actual se diseñó para teclado, pantalla horizontal y una presentación de 1280 por 720. La Semana 14 añade una capa móvil opcional para demostrar la viabilidad técnica sin eliminar ni alterar la entrada de PC.

## Adaptación implementada

El archivo `Assets/Scripts/UmbraInput.cs` contiene dos responsabilidades:

- `UmbraInput` unifica teclado y entrada táctil. Los scripts de gameplay consultan una sola API para movimiento, salto, interacción, impulso, pausa y reinicio.
- `UmbraMobileControls` crea en tiempo de ejecución una interfaz táctil persistente. En Android o iOS aparece automáticamente. En el Editor y en Windows se activa o desactiva con F10 para realizar pruebas.

La implementación evita modificar las escenas. `RuntimeInitializeOnLoadMethod` crea el objeto `UMBRA Mobile Controls` después de cargar el capítulo y `DontDestroyOnLoad` lo conserva al cambiar de nivel.

## Resolución y orientación

- Orientación prevista: horizontal.
- Resolución de referencia: 1280 por 720.
- Escalado: cada botón usa un factor calculado a partir del ancho y alto disponibles.
- Área segura: la interfaz parte de `Screen.safeArea` y convierte sus coordenadas al sistema de `OnGUI`.
- Frecuencia objetivo: 60 cuadros por segundo.
- Relación de aspecto: el escenario sigue usando cámara horizontal y admite las relaciones configuradas en Player Settings.

## Mapa de controles

| Acción | PC | Móvil |
| --- | --- | --- |
| Movimiento | A y D o flechas | Cruceta izquierda y derecha |
| Escalera | W y S o flechas | Cruceta arriba y abajo |
| Agacharse | S o flecha abajo | Cruceta abajo |
| Correr | Shift | Botón Correr |
| Saltar | Espacio, W o flecha arriba | Botón Saltar |
| Interactuar | E | Botón Acción |
| Impulso Umbral | Q o Control derecho | Botón Impulso |
| Pausa | Escape | Botón II |
| Reinicio | R | Botón R |

## Integración con el gameplay

`PlayerController2D` consume los ejes y estados de `UmbraInput`. `ShadowDash2D` usa la señal de impulso. `DoorGoal`, `LeverSwitch2D` y `HidingSpot2D` reciben la señal de acción. `GameManager` permite comenzar, pausar, reiniciar y avanzar de nivel desde la interfaz táctil.

Los textos también se adaptan. Cuando el modo móvil está activo, el juego muestra instrucciones como `TOCA SALTAR O ACCIÓN`, `ACCIÓN ABRIR` e `IMPULSO UMBRAL` sin depender de las letras del teclado.

## Pruebas realizadas

1. Unity 6000.5.2f1 compiló todos los scripts con código de salida 0.
2. La vista móvil se activó con F10 dentro de Chapter 01 El Fondo.
3. Unity mostró el área segura, la cruceta y los botones táctiles durante Play.
4. Un clic sobre Saltar inició el capítulo, lo que confirmó que la entrada táctil llega a `GameManager`.
5. La escena conservó HUD, parallax, audio y gameplay después de activar el modo móvil.
6. Los controles de teclado permanecieron disponibles porque `UmbraInput` combina ambas fuentes.

## Alcance y limitaciones

La prueba actual corresponde a una simulación funcional en el Editor. La instalación de Unity disponible incluye soporte de Windows y WebGL, pero no el módulo Android. Para producir un APK se necesita instalar Android Build Support, SDK, NDK y OpenJDK desde Unity Hub, definir un identificador de paquete y ejecutar una prueba en un dispositivo físico.

Antes de publicar una versión móvil también conviene medir consumo de memoria, temperatura, duración de batería, latencia táctil y legibilidad en pantallas con notch. La arquitectura de entrada ya permite realizar esas pruebas sin volver a escribir las mecánicas.

## Demostración sugerida

1. Ejecutar Chapter 01 El Fondo con controles de PC.
2. Presionar F10 para activar la vista móvil.
3. Explicar el área segura y la orientación horizontal.
4. Pulsar Saltar o Acción para iniciar.
5. Usar cruceta, salto, impulso y acción en una parte del nivel.
6. Presionar F10 y comprobar que el teclado sigue funcionando.

