# UMBRA - Prototipo Unity 2D

Prototipo inicial para el curso de Diseno y Desarrollo de Juegos Interactivos II.

La idea es mantenerlo con nivel de estudiante avanzado: scripts cortos, objetos simples,
arte propio sencillo y una escena entendible para poder explicarla en clase.

## Como abrirlo

1. Abre Unity Hub.
2. Inicia sesion o activa la licencia personal si Unity lo pide.
3. Agrega la carpeta `UMBRA` como proyecto.
4. Abre el proyecto con Unity `6000.5.2f1`.
5. Abre `Assets/Scenes/Level_01_Forest.unity`.
6. Si las escenas no aparecen, usa `Tools > UMBRA > Rebuild All Five Levels`.

## Controles

- `A/D` o flechas: moverse.
- `Space`, `W` o flecha arriba: saltar.
- `S` o flecha abajo: agacharse.
- Empuja la caja caminando contra ella; manten `E` para jalarla.
- `W/S` o flechas verticales: trepar cuando estas sobre la escalera.
- `Esc`: pausar o continuar.
- Toca el checkpoint para actualizar el punto de respawn en los niveles largos.
- Toma la llave para abrir la puerta.
- Evita las trampas.
- `R`: recargar el nivel desde el ultimo checkpoint y restaurar cajas, trampas y mecanismos.

## Que incluye

- Movimiento 2D basico con `Rigidbody2D`.
- Salto y deteccion de suelo.
- Caja empujable.
- Caja que puede empujarse y jalarse con `E`.
- Interruptor de presion conectado a una trampa.
- Zona trepable.
- Trampa de pinchos.
- Checkpoint.
- Guardado automatico del checkpoint y la llave con `PlayerPrefs`.
- Llave y puerta.
- Cinco niveles conectados: bosque, ruinas, fabrica, cavernas y escape.
- Progresion automatica de un nivel al siguiente.
- Meta final y creditos completos al terminar el nivel 5.
- Menu inicial, pausa, muerte/reinicio rapido y creditos.
- Viento y ambiente grave continuo, con efectos audibles de pasos, salto, llave, mecanismos y muerte.
- Camara que sigue al jugador.
- Estetica monocromatica atmosferica inspirada en juegos de siluetas.
- Cinco fondos originales y objetos ilustrados coherentes.
- Spritesheet de 12 cuadros para quieto, carrera, salto y agachado.
- Llave ilustrada con movimiento flotante.

## Relacion con el PDF

Esta entrega cubre los 5 niveles del alcance planteado. Incluye mecanicas base,
acertijos ambientales, trampas, arte propio, audio, guardado por escena y el ciclo
completo desde el nivel 1 hasta los creditos del nivel 5.

## Siguiente paso recomendado

Agregar animaciones especificas de trepar, empujar y morir, y realizar playtests
con distintos jugadores para ajustar la dificultad.
