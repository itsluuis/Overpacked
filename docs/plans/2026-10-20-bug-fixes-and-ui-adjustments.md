# Plan de Implementación: Ajustes de UI y Corrección de Bugs (Beta 1.6.1)

> **For AI Assistants:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Invertir y reescalar los hemisferios del Menú Principal (logo/botones a la izquierda con texto negro; cuaderno ampliado a la derecha), incorporar la malla de feedback horizontal de máximo 5 ítems sobre la caja (`PackageBox`), congelar completamente el reloj, clima y caducidad de pedidos al abrirse los modales de fin de jornada (`GameModeManager.IsGameplayPaused`), y aumentar la tipografía de los reportes (+10px en Tutorial, +14px en Infinito) con texto negro en todos los botones amarillos y verdes.

**Architecture:** Enfoque centralizado y no intrusivo: flag estático `GameModeManager.IsGameplayPaused` para pausar los ciclos `Update()` de `OrderManager`, `EnvironmentManager` e `InfiniteModeController` sin alterar `Time.timeScale`; componente `ItemsFeedbackRack` desacoplado dentro de `PackageBox` para feedback visual ordenado de hasta 5 ítems sin afectar el drag & drop ni las estanterías; e inversión limpia de coordenadas y escalas en `MenuSetupTools.cs` y `SceneSetupTools.cs`.

**Tech Stack:** Unity 6 (6000.6.5f1), C# (.NET Standard 2.1), Universal Render Pipeline (URP 2D), Unity UI (uGUI), New Input System.

---

### Task 1: Control Centralizado de Pausa de Sesión en `GameModeManager`
**Files:**
- Modify: `My project/Assets/Scripts/Core/GameModeManager.cs`

**Steps:**
1. Añadir propiedad estática `public static bool IsGameplayPaused { get; set; } = false;`.
2. Crear método `public static void SetGameplayPaused(bool paused)` que actualice el flag y emita debug log si cambia el estado.
3. Asegurar que `ActiveMode` o el inicio de partida restablezca `IsGameplayPaused = false`.

---

### Task 2: Detención de Temporizadores, Spawner y Clima al Fin de Jornada
**Files:**
- Modify: `My project/Assets/Scripts/Managers/OrderManager.cs`
- Modify: `My project/Assets/Scripts/Managers/EnvironmentManager.cs`
- Modify: `My project/Assets/Scripts/Gameplay/InfiniteModeController.cs`
- Modify: `My project/Assets/Scripts/UI/DayEndSummaryUI.cs`

**Steps:**
1. En `OrderManager.Update()`: colocar guard clause al inicio:
   ```csharp
   if (GameModeManager.IsGameplayPaused) return;
   ```
   Esto evita que se generen pedidos y que se resten los tiempos de los pedidos activos (impidiendo que caduquen o resten dinero con el modal abierto).
2. En `EnvironmentManager.Update()`: colocar guard clause al inicio:
   ```csharp
   if (GameModeManager.IsGameplayPaused) return;
   ```
   Esto congela el avance del reloj LED y los cambios de clima y temperatura.
3. En `InfiniteModeController.Update()`: colocar guard clause al inicio para detener el temporizador de ronda si está pausado.
4. En `DayEndSummaryUI.cs`:
   - En `SetModalVisible(bool visible)`: invocar `GameModeManager.IsGameplayPaused = visible;`.
   - En `OnClickNextDay()` y `OnClickRetryInfinite()`: asegurar que `GameModeManager.IsGameplayPaused = false;` antes de reanudar o reiniciar el ciclo.

---

### Task 3: Capacidad Máxima y Malla Horizontal de Feedback en la Caja (`PackageBox`)
**Files:**
- Modify: `My project/Assets/Scripts/Gameplay/PackageBox.cs`

**Steps:**
1. Declarar constante `public const int MAX_CAPACITY = 5;`.
2. En `AddItem(ItemData item)`:
   - Validar `if (_items.Count >= MAX_CAPACITY) return false;`. Esto es por instancia y nunca afectará a una nueva caja vacía generada por `BoxSpawner`.
3. Crear / actualizar el contenedor horizontal `_feedbackRack` (GameObject hijo a `local Y = +1.4f` sobre la caja):
   - Cada ítem añadido crea un sprite en una fila horizontal equidistante centrada ($X \in [-1.2f, +1.2f]$).
   - Escala de cada ícono pequeña (`0.35f` local respecto a la caja) para que los 5 quepan holgadamente sin tocarse ni alterar los ítems de las estanterías.
4. En `SetState(BoxState newState)`:
   - Si `newState == BoxState.InPackagingSlot` (al colocarla en la cinta de despacho) o `BoxState.Discarded`: apagar `_feedbackRack.SetActive(false)`.
   - Al estar en la mesa de armado (`BoxState.OnTable`): mantener `_feedbackRack.SetActive(true)`.

---

### Task 4: Inversión de Hemisferios, Escalas y Colores en el Menú Principal
**Files:**
- Modify: `My project/Assets/Scripts/Editor/MenuSetupTools.cs`
- Modify: `My project/Assets/Scripts/UI/MainMenuController.cs`
- Modify: `My project/Assets/Scripts/UI/CuadernoMenuController.cs`

**Steps:**
1. En `MenuSetupTools.BuildMainMenuScene()`:
   - **Hemisferio Izquierdo (`LeftHemisphere` en $X = -460$):**
     - Logo Overpacked: tamaño $600 \times 260$ px (2x y ensanchado armónicamente).
     - Botones Amarillos ("JUGAR", "OPCIONES", "SALIR"): tamaño $360 \times 80$ px, fuente 38px (+10px), color de texto **negro carbón (`#1A1A1A`)**.
   - **Hemisferio Derecho (Cuaderno en $X = +460$):**
     - Tamaño inicial del cuaderno cerrado: escala $0.84$ (2 veces más grande).
   - **Páginas del Cuaderno Abierto:**
     - Botón "◀ VOLVER": texto en **negro**.
     - Botón "EMPECEMOS ▶": texto en **negro**.
2. En `CuadernoMenuController.cs`:
   - `_closedPosition = new Vector2(460f, -20f)` (en la derecha).
   - `_openPosition = Vector2.zero` (al centro).
   - `_closedScale = new Vector3(0.84f, 0.84f, 1f)` (2x).
   - `_openScale = new Vector3(1.25f, 1.25f, 1f)` (3x respecto a la original de 0.42).
3. En `MainMenuController.cs`:
   - Renombrar `RightHemisphereGroup` a `LeftHemisphereGroup` (o apuntar al grupo del logo/botones) para que haga fade-out al abrir el cuaderno.

---

### Task 5: Tipografía y Botones en Modales de Fin de Jornada
**Files:**
- Modify: `My project/Assets/Scripts/UI/DayEndSummaryUI.cs`
- Modify: `My project/Assets/Scripts/Editor/SceneSetupTools.cs`

**Steps:**
1. **Regla de Botones:** En `SceneSetupTools.cs`, asignar color de texto **negro carbón (`#1A1A1A`)** a:
   - `NextDayButton` (verde)
   - `RetryInfiniteButton` (verde)
   - `ReturnToMenuButton` (amarillo)
2. **Textos del Modal Tutorial (+10px):**
   - Header Title: 24 $\rightarrow$ 34 px
   - Stars Display: 48 $\rightarrow$ 58 px
   - Rating Title: 22 $\rightarrow$ 32 px
   - Prestige Badge: 17 $\rightarrow$ 27 px
   - Financial Breakdown: 17 $\rightarrow$ 27 px
   - Quotas Status: 16 $\rightarrow$ 26 px
   - Botón Principal: Cambiar texto a **"VOLVER A INTENTAR"** con fuente 22 px en negro.
3. **Textos del Modal Infinito (+14px tras "RONDAS SUPERADAS"):**
   - En `DayEndSummaryUI.RenderInfiniteSummary()`:
     - RatingTitleText: 22 $\rightarrow$ 36 px
     - PrestigeBadgeText: 17 $\rightarrow$ 31 px
     - FinancialBreakdownText: 17 $\rightarrow$ 31 px
     - QuotasStatusText: 16 $\rightarrow$ 30 px
     - Botón "▶ VOLVER A JUGAR" y "🏠 MENÚ PRINCIPAL": fuente 22 px en negro.

---

### Task 6: Recompilación, Reconstrucción de Escenas y Verificación en Play Mode
**Files:**
- Rebuild: `MainMenuScene.unity` vía `LogiPack/Construir Menú Principal`
- Rebuild: `GameplayScene.unity` vía `LogiPack/Construir Escena de Gameplay`

**Steps:**
1. Ejecutar `recompile` en Unity MCP y verificar 0 errores.
2. Ejecutar `menu` para reconstruir ambas escenas.
3. Entrar a Play Mode:
   - Probar Menú Principal: hemisferio izquierdo (logo 2x, botones con texto negro a 38px), cuaderno derecho 2x que al abrir viaja al centro y escala 3x.
   - Probar Gameplay: meter 5 ítems en la caja (verificar malla horizontal ordenada sobre la caja con ítems pequeños). Intentar meter un 6to ítem (verificar que no entra). Despachar la caja a la cinta (verificar que se precinta y la malla se oculta). Verificar que la nueva caja en la mesa admite 5 ítems normalmente.
   - Probar Fin de Jornada: verificar que el reloj, clima y pedidos se detienen por completo mientras el modal está abierto. Verificar textos grandes y botones con texto negro.
4. Capturar screenshots de verificación.
