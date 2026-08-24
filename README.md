# UMBRA: El Archivo de los Ecos

Videojuego 2D para el curso de Diseno y Desarrollo de Juegos Interactivos II.

La propuesta sigue a Lumo, un viajero de tela con una luz en el pecho que recorre
recuerdos construidos con papel, ceramica y objetos cosidos. La atmosfera de los
plataformeros narrativos es una referencia general, pero la historia, el personaje,
la paleta, los objetos y el mundo visual son propios del proyecto.

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
- Empuja el cubo de memoria caminando contra el; manten `E` para jalarlo.
- `W/S` o flechas verticales: trepar cuando estas sobre la escalera.
- `Esc`: pausar o continuar.
- Toca un farol de eco para actualizar el punto de reaparicion.
- Recupera el fragmento de eco para abrir el umbral de memoria.
- Evita las trampas.
- `R`: recargar el nivel desde el ultimo checkpoint y restaurar cajas, trampas y mecanismos.

## Que incluye

- Movimiento 2D basico con `Rigidbody2D`.
- Salto y deteccion de suelo.
- Cubos de memoria que se pueden empujar y jalar con `E`.
- Placas de resonancia y diapasones conectados a peligros.
- Escaleras de cintas, nudos de espinas y engranajes moviles.
- Faroles de eco que funcionan como checkpoints.
- Guardado automatico del farol y del fragmento con `PlayerPrefs`.
- Fragmentos de eco, umbrales de memoria y portales de regreso.
- Cinco recuerdos conectados: jardin, ciudad de cartas, taller de horas,
  biblioteca bajo la lluvia y observatorio.
- Progresion automatica de un nivel al siguiente.
- Meta final y creditos completos al terminar el nivel 5.
- Menu inicial, pausa, muerte/reinicio rapido y creditos.
- Ambiente musical generado por codigo y efectos de pasos, salto, eco, mecanismos y caida.
- Camara que sigue al jugador.
- Estetica de collage artesanal con papel, tela, ceramica y color.
- Cinco fondos originales, una hoja de objetos y terreno cosido.
- Lumo cuenta con 12 cuadros para quieto, carrera, salto y agachado.
- Al recoger un fragmento aparece un pulso de color y el mensaje `ECO RECUPERADO`.

## Los cinco recuerdos

1. El Jardin de las Primeras Voces - infancia.
2. La Ciudad de las Cartas No Enviadas - adolescencia.
3. El Taller de las Horas Prestadas - adultez.
4. La Biblioteca Bajo la Lluvia - vejez.
5. El Observatorio de los Ecos que Regresan - aceptacion.

## Relacion con el PDF

Esta entrega mantiene los lineamientos del avance: desplazamiento lateral,
exploracion, acertijos ambientales, cinco etapas de vida y un cierre sobre la
aceptacion. La identidad del Archivo de los Ecos desarrolla esos puntos con una
direccion propia y evita depender de la apariencia de otro juego.

## Siguiente paso recomendado

Agregar animaciones especificas de trepar, empujar y morir, y realizar playtests
con distintos jugadores para ajustar la dificultad.
