# UMBRA

Videojuego 2D de plataformas, acertijos y terror psicologico creado en Unity 6000.5.2f1.
La version actual es jugable de principio a fin con arte y sonido provisionales locales.

## Ejecutar

- En Unity: abrir `Assets/Scenes/Chapter_01_El_Fondo.unity` y pulsar Play.
- En Windows: ejecutar `Builds/Windows/UMBRA.exe`.
- Para regenerar las escenas: `Tools > UMBRA > Rebuild Complete Game`.
- Inventario completo: `INVENTARIO_ASSETS.md`.
- Física, layers, triggers y Rigidbody2D: `REPORTE_TECNICO_UNITY.md`.
- Resultados de pruebas: `QA_TEST_REPORT.md`.

## Controles

- `A/D` o flechas: caminar.
- `Shift`: correr.
- `Espacio`, `W` o flecha arriba: saltar.
- `S` o flecha abajo: agacharse o bajar al trepar.
- `E`: jalar objetos, accionar palancas y entrar/salir de escondites.
- `Q` o `Ctrl derecho`: activar el Impulso Umbral; queda bloqueado mientras se recarga y no se puede usar agachado, trepando ni oculto.
- `Esc`: pausa y control de volumen.
- `R`: reiniciar desde el ultimo checkpoint.
- En la portada: `Enter` inicia, `C` continua el capitulo desbloqueado y `N` empieza una partida nueva.

## Recorrido

1. El Fondo: despertar, caja y placa de presion, trepar, primera amenaza y susto ambiental.
2. Los Abandonados: plataformas moviles, alma perseguidora, escondite y palanca.
3. La Carne: tunel, arquitectura roja, demonio con vigilancia y persecucion.
4. El Peso: ascensores, plataformas de precision, culpa y amenaza avanzada.
5. El Umbral: combina mecanismos, persecucion final, luz y despertar en el hospital.

Los enemigos respetan obstaculos, tienen estados configurables y no detectan a un jugador escondido.
Los seis eventos de terror usan apariciones del fondo o primer plano, cambios de entorno y camara;
se guardan para no repetirse al morir. La muerte recarga la escena y restaura los acertijos conservando
el checkpoint.

La revision visual v2 incorpora sprites propios para almas, demonios, escondites, manifestaciones y
pistas hospitalarias. Las trampas comunican visualmente cuando estan desarmadas, puertas y palancas
muestran su tecla de interaccion y las plataformas transportan al jugador sin reparentarlo.

La revision integral posterior garantiza soporte bajo los objetivos, continuidad de rutas, escaleras
atravesables, palancas accesibles antes de los peligros que controlan y salidas completamente apoyadas.
Unity conserva ahora el ultimo capitulo abierto en vez de regresar siempre al primero al recompilar.

## Recursos pendientes

Los fondos, sprites y sonidos actuales son recursos provisionales del proyecto. Para una entrega final
conviene sustituir las criaturas, objetos hospitalarios y animaciones de empujar, esconderse y morir,
y realizar playtests humanos de dificultad y ritmo.
