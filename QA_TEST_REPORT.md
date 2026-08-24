# UMBRA - Reporte de pruebas QA

Fecha: 21 de agosto de 2026
Motor: Unity 6000.5.2f1
Plataforma: Windows 64 bits

## Resultado general

APROBADO. No se detectaron errores de compilacion, referencias rotas ni
excepciones durante las pruebas automaticas realizadas.

## Pruebas realizadas

- Compilacion de todos los scripts de juego y editor.
- Validacion de las cinco escenas incluidas en Build Settings.
- Renderizado visual del inicio, mitad y final de cada nivel (15 capturas).
- Build para Windows con resultado `Success`.
- 200 ciclos completos de carga de los cinco niveles.
- 1,000 cargas de nivel verificadas dentro del ejecutable.
- Comprobacion de jugador, camara, checkpoint, llave, puerta y salida.
- Comprobacion de colliders solidos y triggers.
- Comprobacion de trampas, palancas, placas y plataformas moviles.
- Comprobacion de dos checkpoints y una ruta extendida por nivel.
- Comprobacion de que la meta esta despues de la coordenada X 118.
- Comprobacion automatica de apoyo en terreno para cajas, placas, llaves,
  checkpoints, puertas, salidas y decoraciones de suelo.
- Prueba fisica de caida junto a una pared en cada nivel.
- Prueba del limite de velocidad y frenado automatico de las cajas.
- Prueba de ritmo: la caja debe recorrer entre 2.8 y 4.3 unidades al empujarla durante un segundo.
- Verificacion del reinicio completo de escena conservando el checkpoint.
- Verificacion de friccion cero, gravedad, interpolacion y valores finitos.
- Verificacion de un unico `AudioListener` y amplitud audible en el ambiente.
- Captura del primer frame del ejecutable para descartar una ventana vacia.

## Resultados medidos

- Cargas aprobadas: 1,000 de 1,000.
- Fallos del test: 0.
- Errores de compilacion: 0.
- Advertencias de compilacion: 0.
- `NullReferenceException`: 0.
- `MissingReferenceException`: 0.
- Cierres inesperados: 0.
- Escenas configuradas: 5 de 5.
- Capturas visuales revisadas: 15 de 15.
- Niveles con dos checkpoints: 5 de 5.
- Niveles con longitud extendida: 5 de 5.

## Riesgo restante

La automatizacion verifica estabilidad, configuracion, longitud y fisica base.
Los niveles apuntan a unos cinco minutos en una primera partida, pero el tiempo
exacto cambia segun la habilidad del jugador. El balance de dificultad y la
comodidad de cada salto deben seguir evaluandose mediante playtests humanos.
