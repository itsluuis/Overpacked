# 📦 Documento de Diseño: Audio, Pausa, Opciones, Transiciones y Pulido Final (Beta 1.7)
> **Fecha:** 21 de Octubre de 2026  
> **Estado:** Validado mediante proceso formal de Brainstorming  
> **Motor:** Unity 6 (v6000.6.5f1) — Universal Render Pipeline (URP 2D)  

---

## 1. 📌 Resumen del Entendimiento (Understanding Summary)
Este diseño cubre los últimos cambios y sistemas necesarios para cerrar el proyecto *Overpacked*:
* **Sistema de Audio Centralizado (`AudioManager`):** Singleton persistente (`DontDestroyOnLoad`) con canales independientes para Música y SFX. Reproduce en bucle continuo las tres pistas originales del juego (`menu`, `tutorial`, `Jornada infinita`), con atenuación y persistencia de volumen, dejando canales y métodos preparados para futuros efectos de sonido (SFX).
* **Menú de Opciones y Sistema de Pausa Unificado (`OptionsMenuUI`):**
  * **En Gameplay:** Botón cuadrado de madera (`SquareButton1_wood.png`) de $70 \times 70$ px con icono de casa (`icono_casa.png`) en el lateral derecho central, y captura de la tecla **ESC** vía New Input System. Activa la pausa centralizada (`GameModeManager.IsGameplayPaused`) y muestra sliders de volumen para Música y SFX (0% a 100%), botón *"SEGUIR CON LA PARTIDA"* y botón *"SALIR AL MENÚ"*.
  * **En Menú Principal:** El botón `"OPCIONES"` despliega el mismo panel con los sliders de volumen y el botón para volver al menú.
* **Cursor Personalizado Cozy UI Global (`GameCursorManager`):** Sustituye el cursor genérico de Windows por `Cursor1_128px.png` (reposo) y `Cursor1Clicked_128px.png` (presionado / arrastre) en todo el juego a nivel de hardware (`Cursor.SetCursor`), gestionado reactivamente mediante el New Input System.
* **Rebalanceo Visual del Menú de Inicio (`MenuSetupTools`):**
  * Los 3 botones principales ("JUGAR", "OPCIONES", "SALIR") se reducen a la mitad (50% de escala, $260 \times 65$ px) con tipografía proporcional a 24px en negrita negro carbón (`#1A1A1A`) y se desplazan hacia la zona inferior ($Y = -60, -145, -230$).
  * El logotipo de *Overpacked* se ensancha y agranda a $900 \times 370$ px en $Y = 300$, consolidándose como el elemento protagonista del hemisferio izquierdo.
* **Transiciones de Pantalla con Cortinilla Horizontal (`SceneTransitionManager`):**
  * Wipe con cortina negra de derecha a izquierda de **1.0 segundo total** (0.5s entrada a negro + 0.5s salida a la nueva escena) al pasar del cuaderno a Gameplay y al regresar al Menú, sincronizando el cambio natural de música.

---

## 2. 📋 Supuestos Técnicos y Restricciones (Assumptions)
1. **Persistencia:** Los niveles de volumen se almacenan en `PlayerPrefs` (`"MusicVolume"`, `"SFXVolume"`) con valor inicial de `1.0f` (100%).
2. **New Input System:** Cero llamadas a APIs de Input legacy. La tecla ESC se detecta mediante `Keyboard.current.escapeKey.wasPressedThisFrame` y el estado del puntero con `Pointer.current.press.isPressed`.
3. **Desacoplamiento Temporal:** Las transiciones de escena y la UI de pausa operan mediante `Time.unscaledDeltaTime` para funcionar sin importar el estado de congelamiento del gameplay.
4. **No-Objetivos:** No se integran archivos de audio SFX en este sprint (los canales y métodos quedan listos en código), ni se agregan partículas adicionales en el almacén.

---

## 3. 📝 Registro de Decisiones (Decision Log)
* **Decisión 1: Arquitectura de Servicios Persistentes (`AppCore`):**
  * *Alternativas consideradas:* Gestores locales por escena o clase monolítica en `GameModeManager`.
  * *Razón de elección:* Garantiza que la música no se corte abruptamente al cambiar de escena, la transición fluya sin parpadeos y se respete el principio de responsabilidad única (SRP).
* **Decisión 2: SFX sin archivos físicos por ahora:**
  * *Alternativas consideradas:* Sonidos procedurales en C# o archivos externos.
  * *Razón de elección:* El usuario aportará los archivos de sonido posteriormente; el sistema deja preparados los métodos `PlaySFX(AudioClip)` y el slider de volumen.
* **Decisión 3: Duración de Cortina de Transición en 1.0 Segundo:**
  * *Alternativas consideradas:* 3.0 segundos.
  * *Razón de elección:* 1.0 segundo ofrece un ritmo de juego ágil, sin tiempos muertos, suficiente para un fundido elegante de pantalla y audio.

---

## 4. 🏗️ Especificación Técnica de Componentes

### 4.1. `AudioManager.cs` (`Assets/Scripts/Audio/`)
* **Singleton:** `Instance` persistente con `DontDestroyOnLoad`.
* **AudioSources:**
  * `_musicSource`: Loop = true, volumen vinculado a `PlayerPrefs.GetFloat("MusicVolume", 1f)`.
  * `_sfxSource`: Loop = false, volumen vinculado a `PlayerPrefs.GetFloat("SFXVolume", 1f)`.
* **Clips Serializados:**
  * `MenuMusic`: `Assets/Audio/Music/menu/The_Picking_Floor.mp3`
  * `TutorialMusic`: `Assets/Audio/Music/tutorial/The_Sorting_Floor.mp3`
  * `InfiniteMusic`: `Assets/Audio/Music/Jornada infinita/Operational_Weight.mp3`
* **API Pública:**
  * `PlayMusicForMode(GameSessionMode mode)`
  * `PlayMenuMusic()`
  * `SetMusicVolume(float volume)`
  * `SetSFXVolume(float volume)`
  * `PlaySFX(AudioClip clip, float volumeScale = 1f)`

### 4.2. `GameCursorManager.cs` (`Assets/Scripts/UI/`)
* **Texturas del Cursor:**
  * `DefaultCursor`: `Assets/Art/CozyUI/Cursors/128px/Cursor1_128px.png`
  * `ClickedCursor`: `Assets/Art/CozyUI/Cursors/128px/Cursor1Clicked_128px.png`
* **Hotspot:** `Vector2.zero` (esquina superior izquierda del puntero).
* **Mecanismo:** Utiliza `Cursor.SetCursor(texture, hotspot, CursorMode.Auto)`. Escucha el cambio de estado de clic del ratón sin ejecutar código redundante cuando el estado no cambia.

### 4.3. `SceneTransitionManager.cs` (`Assets/Scripts/Managers/`)
* **Canvas de Transición:** Canvas con `CanvasScaler` a $1920 \times 1080$, `RenderMode.ScreenSpaceOverlay`, `overrideSorting = true`, `sortingOrder = 999`, componente `GraphicRaycaster` con `blocksRaycasts = false` en reposo y `true` durante el wipe.
* **Cortina:** Imagen negra rectangular que cubre la pantalla.
* **Corrutina `LoadSceneWithWipe(string sceneName, float duration = 1.0f)`:**
  1. Activa `blocksRaycasts = true`.
  2. Desplaza/extiende la cortina negra de derecha a izquierda durante $0.5\text{s}$ con `Time.unscaledDeltaTime`.
  3. Ejecuta `SceneManager.LoadSceneAsync(sceneName)`.
  4. Descubre la pantalla de derecha a izquierda durante $0.5\text{s}$.
  5. Desactiva `blocksRaycasts = false`.

### 4.4. `OptionsMenuUI.cs` (`Assets/Scripts/UI/`)
* **Panel de Opciones:** Modal con fondo oscuro o madera, sliders para Música y SFX, texto con porcentaje (`0%` a `100%`).
* **Botones:**
  * En Menú Principal: Botón amarillo *"◀ VOLVER"*.
  * En Gameplay: Botón verde *"SEGUIR CON LA PARTIDA"* y botón amarillo *"SALIR AL MENÚ"*.
* **Controlador de Teclado:** Monitorea la tecla ESC para alternar visibilidad del modal y pausar/despausar el juego.

### 4.5. Actualización de Escenas y Herramientas de Editor
* **`MenuSetupTools.cs`:**
  * Logotipo Overpacked ampliado a $900 \times 370$ px en $Y = 300$.
  * Botones "JUGAR", "OPCIONES", "SALIR" reducidos a $260 \times 65$ px con texto 24px bold en $Y = -60, -145, -230$.
  * Vinculación del panel de opciones al botón "OPCIONES".
* **`SceneSetupTools.cs`:**
  * Creación del botón cuadrado de madera [SquareButton1_wood.png](file:///c:/Users/HP/Documents/Universidad%20Paez%20(Clases)/Semestre%204/Logica%20Simbolica/Proyecto%20final/My%20project/Assets/Art/CozyUI/Buttons/Square/SquareButton1_wood.png) de $70 \times 70$ px con [icono_casa.png](file:///c:/Users/HP/Documents/Universidad%20Paez%20(Clases)/Semestre%204/Logica%20Simbolica/Proyecto%20final/My%20project/Assets/Art/Buttoms/icono_casa.png) en $X = -50, Y = 0$ (anclado a la derecha).
  * Inserción del modal de pausa/opciones en el Canvas de Gameplay.
* **`CuadernoMenuController.cs`:**
  * Modificación del evento de "EMPECEMOS ▶" para invocar `SceneTransitionManager.Instance.LoadSceneWithWipe("GameplayScene", 1.0f)`.

---

## 5. 🧪 Plan de Validación y Pruebas
1. **Compilación en Unity 6:** 0 errores y 0 warnings en C#.
2. **Reconstrucción de Escenas:** Ejecución automatizada de `BuildMainMenuScene()` y `BuildGameplayScene()`.
3. **Verificación en Play Mode:**
   - Menú de inicio: Apariencia del logo dominante y botones reducidos al 50%.
   - Cursor Cozy UI: Interacción normal y al hacer clic.
   - Música de fondo sonando en menú.
   - Ajuste de slider de volumen y comprobación en tiempo real.
   - Transición de 1.0s con cortina negra de derecha a izquierda hacia Tutorial/Infinita.
   - En Gameplay: Música correspondiente reproduciéndose.
   - Pausa mediante tecla ESC o botón casa de madera.
   - Retorno al Menú Principal mediante la cortina de transición.
