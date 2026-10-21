# Overpacked Beta 1.7 Pre-release 1.0.1: Partículas de Temperatura y Modo Práctica

## 1. Resumen de Entendimiento (Understanding Summary)
* **Objetivo 1 (Menú Principal):** El modo antes conocido como "Tutorial" pasa a llamarse "PRÁCTICA", con su nueva redacción de bienvenida extendida y subtítulo *"PRÁCTICA\n<size=15>Jornada de Prueba • Turno 4 Minutos</size>"*.
* **Objetivo 2 (Gameplay Ambiental):** Sistema reactivo de partículas en tiempo real a la temperatura de la hora actual (`CurrentState.Temperature`) tanto en Modo Práctica como en Modo Infinito:
  * **Frío (`Cold`):** Estrellas celestes claro de 4 puntas que caen suavemente desde arriba.
  * **Calor (`Hot`):** Gradiente ondulante vertical (amarillo a naranja) ascendente desde abajo.
  * **Templado (`Normal`):** Sin partículas (almacén limpio).
* **Profundidad de Render:** `sortingOrder = 3` (por encima de las estanterías en `1` y cintas en `2`, pero estrictamente por detrás de la mesa de empaque en `5`, tarjetas de pedidos en `25` y Canvas en `50`).
* **Pausa:** Las partículas continúan flotando suavemente en segundo plano durante la pausa (`GameModeManager.IsGameplayPaused`), manteniendo la estética *cozy* del almacén vivo.

---

## 2. Bitácora de Decisiones (Decision Log)

| ID | Decisión | Alternativas consideradas | Justificación |
| :---: | :--- | :--- | :--- |
| **DEC-01** | **ParticleSystem 2D Nativo** en espacio de mundo. | UI Canvas Image pooling, Shader Post-Processing URP. | Rendimiento nativo en GPU sin ensuciamiento de Canvas (*canvas dirtying*), físicas naturales. |
| **DEC-02** | **Profundidad de Renderizado (`sortingOrder = 3`)**: Por encima de estanterías (`1`), pero detrás de la mesa de trabajo (`5`), caja y pedidos (`25`). | Capa frontal invasiva, o capa de fondo detrás de las paredes. | Flotan por el aire de la fábrica sin tapar jamás el texto de un pedido ni la caja que tienes en la mesa. |
| **DEC-03** | Mantener enum interno `GameSessionMode.Tutorial` y modificar exclusivamente textos UI. | Renombrar enum en todo el codebase (`GameSessionMode.Practice`). | Evita regresiones en persistencia, estadísticas o referencias serializadas en escenas. |
| **DEC-04** | **Desuscripción estricta en `OnDestroy()`** con verificación de nulidad (`if (envManager != null)`). | No desuscribirse o usar eventos estáticos sin limpiar. | **Requisito obligatorio:** Garantiza cero fugas de memoria (*memory leaks*) y evita `MissingReferenceException` al reiniciar jornada o volver al menú. |
| **DEC-05** | **Gestión de emisión progresiva (`emission.enabled = true/false`)**: Al cambiar el clima, detener la emisión del estado anterior y activar el nuevo para que las partículas se extingan con naturalidad. | Destruir y reinstanciar `ParticleSystem` en cada hora. | Cero *Garbage Collector*, transición orgánica y fluida. |
| **DEC-06** | **Comportamiento en Pausa**: Las partículas siguen flotando en segundo plano (no se congelan). | Forzar congelación manual con `Time.timeScale = 0`. | Respeta la arquitectura de pausa del juego (`GameModeManager.SetGameplayPaused(true)`), mantiene la UI viva y crea un ambiente *cozy* tipo *Overcooked*. |

---

## 3. Especificación Técnica

### 3.1 Sprite de la Estrella de Frío
* Ubicación: `Assets/Art/Sprites_sheets/estrella_4_puntas_frio.png`
* Características: Estrella estilizada de 4 puntas con centro brillante y tonos celeste pastel (#A8E6CF / #B3E5FC), formato PNG RGBA transparente de 64x64 px.

### 3.2 Componente `AmbientTemperatureFX.cs`
* Ubicación: `Assets/Scripts/Gameplay/AmbientTemperatureFX.cs`
* Escucha: `EnvironmentManager.OnEnvironmentChanged`
* Referencias:
  * `ColdParticleSystem`: emite hacia abajo con ligera deriva horizontal (Noise / Force over Lifetime).
  * `HeatParticleSystem`: emite hacia arriba con gradiente dorado/naranja y movimiento senoidal suave.
* Subscripción y Limpieza:
  * Suscribe en `Start()` y aplica estado inicial.
  * Desuscribe en `OnDestroy()`.

### 3.3 Textos en `CuadernoMenuController.cs` y `MenuSetupTools.cs`
* `PRACTICA_DESC`:
  > "Nuevo por aqui? no te preocupes, puedes hacer una jornada de prueba para descubrir el almacen, como funcionan los pedidos, como puede cambiar la jornada a lo largo del dia, y que tienes que cumplir al finalizar tu turno. Asustado? tomatelo facil, podras intentarlo cuantas veces necesites hasta que te acostumbres"
* Título de tarjeta / selector: `PRÁCTICA`
* Subtítulo: `Jornada de Prueba • Turno 4 Minutos`
