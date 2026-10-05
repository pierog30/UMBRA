# UMBRA - Reporte de verificacion

Fecha: 6 de septiembre de 2026  
Motor: Unity 6000.5.2f1  
Plataforma: Windows 64 bits

## Resultado

- Compilacion de scripts: aprobada.
- Validacion de referencias y colliders en cinco escenas: aprobada.
- Build Windows `Builds/Windows/UMBRA.exe`: Success.
- Prueba de estres dentro del ejecutable: 20 recorridos, 100 de 100 escenas cargadas, codigo de salida 0.
- Pruebas automaticas de movimiento, coyote time, jump buffer, friccion, gravedad, caja y audio: aprobadas.
- Render de las cinco camaras: aprobado; capturas en `Logs/LevelPreviews`.
- Excepciones `NullReferenceException` o `MissingReferenceException`: ninguna en las pruebas.
- Errores y advertencias de compilacion: ninguno tras la revision v2.

## Cobertura funcional inspeccionada

Se comprobaron por configuracion: checkpoints, reinicio de escena, guardado de progreso, puertas,
llaves/sellos, palancas, placas, cajas, plataformas, trampas, escondites, IA con linea de vision,
seis eventos de terror persistentes, pausa, volumen y final hospitalario.

La revision v2 tambien valida que cada capitulo tenga IA, escondite, eco narrativo, limites de camara,
arte de criatura correcto y el numero esperado de eventos de terror. Las trampas se prueban armando y
desarmando su collider durante la ejecucion.

Revision del 7 de septiembre: las cinco plataformas situadas sobre escaleras se convirtieron en
plataformas unidireccionales y las zonas de trepar se extendieron por encima del borde. El diagnostico
subio fisicamente por las cinco escaleras sin colisionar con el techo. Resultado final: 5/5 aprobadas,
sin excepciones, errores ni advertencias.

Revision adicional del nivel 2: se despejo el aterrizaje posterior al puente, se retiro la trampa que
bloqueaba esa llegada y se recolocaron el escondite y el enemigo sobre terreno estable. La validacion
comprueba ahora que el escondite tenga suelo real. El ejecutable completo supero 3 recorridos consecutivos
(15 cargas de nivel), incluidas 3 cargas del capitulo 2, con 0 errores de ejecucion.

Revision integral de los cinco niveles, 7 de septiembre: se corrigio la progresion circular del capitulo 3
moviendo la palanca antes de la sierra que protege la escalera; en el capitulo 5 se separo la trayectoria de
la sierra de la escalera; y las salidas de los capitulos 2, 4 y 5 se colocaron completamente sobre terreno.
El editor ahora conserva el capitulo que estaba abierto. Se agregaron validaciones de soporte para jugador,
checkpoint, escondite, llave, puerta, salida, palanca y placa; continuidad horizontal de la ruta; y peligros
moviles que cruzan escaleras sin una palanca previa. Las diez capturas (inicio y mitad) pasaron la inspeccion.
Build final: Success, sin warnings de C#. Estres final: 20 recorridos, 100/100 cargas aprobadas y 0 errores.

## Limite de la verificacion

No se realizo un playtest humano completo con teclado de principio a fin. La solucion de los acertijos,
los saltos y las persecuciones se validaron mediante geometria, configuracion y diagnosticos automaticos;
el balance fino requiere una sesion humana en Play Mode.

## Hito 1 - Semana 5

Revision del 14 de septiembre de 2026: se incorporo al `GameManager` un sistema visible y persistente de
intentos y caidas. El primer intento comienza en 1; cada muerte incrementa ambos valores antes de recargar
la escena desde el ultimo checkpoint. El HUD muestra llave/recurso, intento, caidas y progreso del capitulo
actual sobre el total de cinco niveles. Al iniciar una nueva partida se reinician los contadores.

- Compilacion y validacion de las cinco escenas: aprobadas.
- Build independiente: `Builds/Hito1_Semana05/UMBRA_Hito1_Semana05.exe`.
- Resultado del build: Success, codigo de salida 0.
- Smoke test del ejecutable: 5 de 5 niveles cargados y validados, codigo de salida 0.
- Sistemas cubiertos: interaccion con E y por contacto, llave como recurso, progreso por capitulos,
  checkpoints, estado de muerte, intentos y reaparicion.

## Patrones de diseno

Revision del 23 de septiembre de 2026: se aplicaron Observer y Strategy a mecanicas existentes sin
alterar la configuracion de las cinco escenas. `UmbraGameEvents` funciona como publicador tipado de
interacciones, progreso y estado del jugador. `UmbraEventJournal` se suscribe como observador y muestra
el ultimo evento sin que llaves, puertas, checkpoints, palancas o el `GameManager` dependan de ese HUD.

`DeathTrap` ahora delega la respuesta al contacto en `ITrapResponseStrategy`. La estrategia `Lethal`
conserva la muerte y reaparicion existentes, mientras `WarningOnly` reproduce una advertencia audiovisual
sin eliminar al jugador. El diagnostico cambia de estrategia durante la prueba y verifica ambos nombres
antes de restaurar la configuracion original.

- Validacion del editor: `UMBRA VALIDATION PASSED`, codigo de salida 0.
- Build independiente: `Builds/Hito1_Semana06/UMBRA_Hito1_Semana06.exe`.
- Resultado del build: Success, codigo de salida 0.
- Smoke test: 5 de 5 niveles cargados, Observer activo y estrategias configurables, codigo de salida 0.
- Evidencia runtime: el log contiene publicaciones `UMBRA OBSERVER` en los cinco capitulos.
