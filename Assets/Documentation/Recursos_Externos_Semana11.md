# Recursos externos y procedencia Semana 11

## Criterio de integración

UMBRA mantiene dependencias externas limitadas y verificables. La versión de la Semana 11 no incorpora paquetes comerciales ni recursos descargados del Asset Store. La IA, el audio y la animación de estados se resuelven con scripts del proyecto para evitar dependencias innecesarias. Los paquetes externos activos proceden del Unity Package Manager y aparecen en `Packages/manifest.json`.

## Paquetes de Unity

| Paquete | Versión declarada | Procedencia | Finalidad |
|---|---:|---|---|
| `com.unity.2d.sprite` | 1.0.0 | Unity Technologies, Package Manager | Importación y configuración de sprites 2D |
| `com.unity.2d.tilemap` | 1.0.0 | Unity Technologies, Package Manager | Soporte de escenarios 2D y superficies por tiles |
| `com.unity.ugui` | 2.0.0 | Unity Technologies, Package Manager | HUD, mensajes y barra de recarga |
| `com.unity.ide.visualstudio` | 2.0.23 | Unity Technologies, Package Manager | Integración del editor de código |
| `com.unity.test-framework` | 1.5.1 | Unity Technologies, Package Manager | Infraestructura de pruebas del proyecto |
| `com.unity.modules.imageconversion` | 1.0.0 | Módulo integrado de Unity | Conversión de capturas a JPG y PNG |
| `com.unity.modules.screencapture` | 1.0.0 | Módulo integrado de Unity | Captura de evidencia del gameplay |

Los términos de uso corresponden a Unity y a cada paquete oficial. El proyecto no incluye un archivo de licencia separado de un proveedor externo dentro de `Assets`.

## Recursos visuales

Los fondos, personajes, criaturas, props y pistas hospitalarias son recursos provisionales locales almacenados en `Assets/Art`. El repositorio no identifica una URL, autor externo ni número de factura del Asset Store. Por esa razón, la documentación los clasifica como recursos internos del prototipo y no atribuye una procedencia externa que no pueda demostrarse.

Los sprites utilizados por la IA son:

- `Assets/Art/Creatures/Frames/creature_00.png` para Sorrow Soul.
- `Assets/Art/Creatures/Frames/creature_01.png` para Grotesque Demon.
- `Assets/Art/Creatures/umbral_creatures_sheet.png` como hoja fuente.

`EnemyAI2D` controla orientación con `SpriteRenderer.flipX`. La revisión de Semana 11 añade una respuesta visual por estado mediante color y pulsación de escala. Patrol conserva el color base, Watch usa una advertencia cálida, Chase aplica rojo, Search aplica violeta y Return utiliza azul grisáceo.

## Audio

`UmbraAudio.cs` genera el ambiente y los efectos durante la ejecución. No existen clips de audio importados en `Assets/Audio`, por lo que no se agrega una licencia de banco de sonidos. Esta decisión evita distribuir archivos cuya procedencia no esté documentada.

## Control de incorporación futura

Antes de agregar un recurso del Asset Store o de otro repositorio, el equipo debe registrar el nombre, autor, URL, fecha de descarga, licencia, versión y escenas donde se utiliza. El archivo de licencia debe conservarse junto al recurso cuando sus términos lo exijan. Un recurso sin procedencia verificable no debe pasar a la versión final comercial.

