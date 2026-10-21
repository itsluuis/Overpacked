# Plan de Implementación: Audio, Opciones, Transiciones y Pulido Final (Beta 1.7)

> **For AI Assistants:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implementar el sistema de audio persistente (`AudioManager`), cursores personalizados Cozy UI (`GameCursorManager`), transiciones de escena con cortinilla de 1.0s (`SceneTransitionManager`), modal de opciones/pausa unificado (`OptionsMenuUI`), y el rebalanceo visual del Menú Principal (logo ampliado a $900 \times 370$ px, botones reducidos al 50% en la parte inferior) y botón de casa en Gameplay.

**Architecture:**
- **Servicios Persistentes (`AppCore`):** `AudioManager`, `GameCursorManager` y `SceneTransitionManager` creados bajo `DontDestroyOnLoad` con autoinstanciación singleton.
- **New Input System:** Captura de tecla ESC y estado de pulsación del cursor con `Keyboard.current` y `Pointer.current`.
- **UI Modular:** `OptionsMenuUI` utilizado tanto en Menú Principal como en la escena de Gameplay, con sliders horizontales para Música y SFX vinculados a `PlayerPrefs`.
- **Automatización de Escenas:** Actualización de `MenuSetupTools.cs` y `SceneSetupTools.cs` para generar toda la jerarquía con un solo clic.

**Tech Stack:** Unity 6 (6000.6.5f1), C# (.NET Standard 2.1), Universal Render Pipeline (URP 2D), Unity UI (uGUI), New Input System.

---

### Task 1: `AudioManager.cs`
**Files:**
- Create: `My project/Assets/Scripts/Audio/AudioManager.cs`

**Steps:**
1. Crear el singleton persistente `AudioManager` con `DontDestroyOnLoad`.
2. Crear dos `AudioSource`: `_musicSource` (en bucle) y `_sfxSource`.
3. Cargar clips serializados de música: `menu/The_Picking_Floor.mp3`, `tutorial/The_Sorting_Floor.mp3` y `Jornada infinita/Operational_Weight.mp3`.
4. Implementar métodos:
   - `SetMusicVolume(float volume)` (persiste en `PlayerPrefs` `"MusicVolume"`).
   - `SetSFXVolume(float volume)` (persiste en `PlayerPrefs` `"SFXVolume"`).
   - `PlayMusicForCurrentScene()` que detecta la escena activa y el `GameModeManager.CurrentMode`.
   - `PlaySFX(AudioClip clip, float volumeScale = 1f)`.
   - Corrutina de crossfade suave de música.

---

### Task 2: `GameCursorManager.cs`
**Files:**
- Create: `My project/Assets/Scripts/UI/GameCursorManager.cs`

**Steps:**
1. Crear el singleton persistente `GameCursorManager` con `DontDestroyOnLoad`.
2. Cargar texturas de cursor: `Cursor1_128px.png` y `Cursor1Clicked_128px.png`.
3. Detectar el estado `Pointer.current.press.isPressed` en `Update()`.
4. Si cambia el estado de presión, invocar `Cursor.SetCursor` con `CursorMode.Auto`.
5. Reaplicar cursor en `OnApplicationFocus(true)`.

---

### Task 3: `SceneTransitionManager.cs`
**Files:**
- Create: `My project/Assets/Scripts/Managers/SceneTransitionManager.cs`

**Steps:**
1. Crear el singleton persistente `SceneTransitionManager` con `DontDestroyOnLoad`.
2. Crear dinámicamente un Canvas con `RenderMode.ScreenSpaceOverlay`, `sortingOrder = 999` y una cortina negra rectangular (`Image`).
3. Corrutina `LoadSceneWithWipe(string sceneName, float duration = 1.0f)`:
   - Bloquear raycasts.
   - Deslizar la cortina de derecha a izquierda en 0.5s usando `Time.unscaledDeltaTime`.
   - Cargar la escena con `SceneManager.LoadSceneAsync(sceneName)`.
   - Revelar la pantalla descubriendo la cortina hacia la izquierda en 0.5s.
   - Desbloquear raycasts.

---

### Task 4: `OptionsMenuUI.cs`
**Files:**
- Create: `My project/Assets/Scripts/UI/OptionsMenuUI.cs`

**Steps:**
1. Crear el componente `OptionsMenuUI`:
   - Sliders de volumen para Música y SFX con etiquetas numéricas (0% - 100%).
   - Botón *"SEGUIR CON LA PARTIDA"* (para Gameplay).
   - Botón *"SALIR AL MENÚ"* (para Gameplay).
   - Botón *"◀ VOLVER"* (para Menú Principal).
2. Leer y sincronizar valores iniciales desde `PlayerPrefs`.
3. Escuchar la tecla ESC con `Keyboard.current.escapeKey.wasPressedThisFrame` para pausar/despausar.
4. Integrar con `GameModeManager.IsGameplayPaused`.

---

### Task 5: Actualizar Menú Principal (`MenuSetupTools.cs` y `CuadernoMenuController.cs`)
**Files:**
- Modify: `My project/Assets/Scripts/Editor/MenuSetupTools.cs`
- Modify: `My project/Assets/Scripts/UI/CuadernoMenuController.cs`

**Steps:**
1. En `MenuSetupTools.cs`:
   - `Logo_Overpacked`: ancho 900, alto 370, $Y = 300$.
   - Botones "JUGAR", "OPCIONES", "SALIR": tamaño $260 \times 65$, fuente 24px bold en negro carbón, posiciones $Y = -60, -145, -230$.
   - Integrar prefab/panel de `OptionsMenuUI` vinculado al botón "OPCIONES".
2. En `CuadernoMenuController.cs`:
   - Al pulsar "EMPECEMOS ▶", invocar `SceneTransitionManager.Instance.LoadSceneWithWipe("GameplayScene", 1.0f)`.

---

### Task 6: Actualizar Gameplay (`SceneSetupTools.cs`)
**Files:**
- Modify: `My project/Assets/Scripts/Editor/SceneSetupTools.cs`

**Steps:**
1. Crear botón de pausa de madera:
   - Sprite `SquareButton1_wood.png` ($70 \times 70$ px).
   - Icono `icono_casa.png` en el interior.
   - Posición anclada al lateral derecho central ($X = -50, Y = 0$).
2. Instanciar el panel `OptionsMenuUI` en el Canvas de Gameplay.
3. Conectar el clic del botón de casa para abrir el panel de pausa.

---

### Task 7: Recompilación, Reconstrucción de Escenas y Verificación con Unity MCP
**Files:**
- Rebuild: `MainMenuScene.unity` y `GameplayScene.unity`

**Steps:**
1. Recompilar con `recompile` y confirmar 0 errores.
2. Ejecutar `BuildMainMenuScene()` y `BuildGameplayScene()`.
3. Probar en Play Mode:
   - Menú de inicio: logo 900x370, botones 260x65, cursor Cozy UI y música de oficina.
   - Panel de opciones en Menú: sliders de volumen.
   - Transición de 1.0s hacia Gameplay con cortina de derecha a izquierda y cambio de música.
   - En Gameplay: Botón de casa en el lateral derecho y tecla ESC pausando el juego.
   - Salir al menú con la transición de cortina.

---

### Task 8: Actualización de Documentación
**Files:**
- Modify: `GUIA_CONTINUIDAD_PROYECTO.md`
- Modify: `GDD_Concepto_LogiPack.md`
