# UMBRA - Patrones de diseno

## Problema identificado

Los scripts de interaccion necesitaban comunicar cambios de llave, checkpoint, puerta, palanca,
ocultamiento y estado del jugador. Si cada objeto conociera directamente al HUD, al registro y a otros
sistemas futuros, cualquier nueva salida obligaria a modificar varios scripts de gameplay. Las trampas
tambien tenian una unica respuesta fija: llamar directamente a `PlayerRespawn.Die()`.

## Observer

`UmbraGameEvents` centraliza tres canales tipados: interaccion, progreso y estado del jugador. Los
publicadores solo emiten datos. `UmbraEventJournal` se registra y se retira mediante `OnEnable` y
`OnDisable`, por lo que puede reemplazarse o coexistir con otros observadores sin editar a los emisores.

Flujo aplicado:

```text
CollectKey / DoorGoal / Checkpoint / Lever / PlayerController
                           |
                           v
                   UmbraGameEvents
                           |
                           v
              UmbraEventJournal (Observer)
```

## Strategy

`DeathTrap` conserva su responsabilidad de detectar el contacto, pero delega la respuesta a
`ITrapResponseStrategy`. `LethalTrapResponseStrategy` provoca muerte y reaparicion.
`WarningTrapResponseStrategy` reproduce sonido y trauma de camara. El modo se elige con
`TrapResponseMode`, sin duplicar el detector ni modificar al jugador.

```text
DeathTrap -> ITrapResponseStrategy
              |-- Lethal: PlayerRespawn.Die()
              `-- WarningOnly: audio + camera trauma
```

## Diferencia de organizacion

Antes, el objeto ejecutaba una consecuencia concreta y cualquier salida visual exigia una referencia
directa. Ahora, los eventos permiten multiples observadores y la trampa cambia de comportamiento mediante
una estrategia intercambiable. Esta estructura reduce dependencias, conserva los componentes pequenos y
facilita agregar logros, telemetria, subtitulos o nuevos tipos de peligro.

## Evidencia de validacion

- Unity 6000.5.2f1 compilo sin errores.
- Las cinco escenas pasaron `UmbraPrototypeBuilder.ValidateProject`.
- El build Windows termino con `Result: Success`.
- El ejecutable cargo y valido 5 de 5 niveles con codigo de salida 0.
- El log de runtime registro eventos `UMBRA OBSERVER` en cada capitulo.
