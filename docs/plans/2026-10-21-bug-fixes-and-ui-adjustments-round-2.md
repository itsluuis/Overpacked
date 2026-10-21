# Plan de Implementación: Segunda Tanda de Bugs y Ajustes (Beta 1.6.2)

> **For AI Assistants:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Resolver el amontonamiento de ítems en la caja mediante un pool fijo de 5 casillas (`PackageBox.cs`), corregir el recorte que ocultaba las estrellas en el modal de fin de jornada (`DayEndSummaryUI.cs` y `SceneSetupTools.cs`), redistribuir los anclajes de las tarjetas de pedidos de empresa para evitar cortes de texto (`OrderCardPrefab`), y elevar el logo 3x y duplicar el tamaño de los botones del Menú Principal (`MenuSetupTools.cs`).

**Architecture:** 
- **Pool Fijo de Casillas:** Eliminación de toda instanciación/destrucción dinámica en tiempo de juego. 5 `SpriteRenderer` pre-instanciados como hijos de `ItemContainer` en `PackageBox.cs`, activados y centrados dinámicamente según la cantidad de ítems presentes en `Contents`.
- **Renderizado de Estrellas:** `verticalOverflow = VerticalWrapMode.Overflow` y altura aumentada a 80px en `StarsDisplayText` para evitar el truncamiento por altura de línea de `Fredoka-Bold` a 58px.
- **Flujo de Textos en Post-its:** Redistribución de anclajes de `ConditionText` (58% a 76%) e `ItemsListText` (4% a 58%) con tipografía a 21px y `verticalOverflow = Overflow` para garantizar la lectura de fórmulas y candidatos válidos sin cortes.
- **Jerarquía y Escala del Menú:** Logo de Overpacked reposicionado en la zona superior ($Y = 310$, $780 \times 320\text{ px}$) y 3 botones amarillos ampliados al doble ($520 \times 125\text{ px}$, fuente 46px en negro carbón).

**Tech Stack:** Unity 6 (6000.6.5f1), C# (.NET Standard 2.1), Universal Render Pipeline (URP 2D), Unity UI (uGUI), New Input System.

---

### Task 1: Pool Fijo de 5 Casillas de Feedback en `PackageBox.cs`
**Files:**
- Modify: `My project/Assets/Scripts/Gameplay/PackageBox.cs`

**Steps:**
1. Declarar un array privado `private SpriteRenderer[] _feedbackSlots;`.
2. En `EnsureVisualOverlays()`:
   - Crear si no existen 5 GameObjects hijos de `ItemContainer` nombrados `Slot_0` hasta `Slot_4`.
   - Cada uno con componente `SpriteRenderer`, `sortingOrder = 10`, escala `Vector3.one * 0.055f` y desactivado por defecto (`SetActive(false)`).
   - Asignarlos a `_feedbackSlots`.
3. En `UpdateVisualContents()`:
   - Extraer la lista de ítems de `Contents`.
   - Si `count == 0`, desactivar los 5 slots y salir.
   - Para $i = 0$ hasta 4:
     - Si $i < count$: Activar `_feedbackSlots[i].gameObject.SetActive(true)`, asignar sprite `itemsList[i].Icon`, y calcular su posición local:
       `xPos = -((count - 1) * 0.40f) * 0.5f + (i * 0.40f);`
       `_feedbackSlots[i].transform.localPosition = new Vector3(xPos, 0.95f, -0.1f);`
     - Si $i \ge count$: Desactivar `_feedbackSlots[i].gameObject.SetActive(false)`.

---

### Task 2: Corrección de Truncamiento de Estrellas en Modales de Fin de Jornada
**Files:**
- Modify: `My project/Assets/Scripts/Editor/SceneSetupTools.cs`
- Modify: `My project/Assets/Scripts/UI/DayEndSummaryUI.cs`

**Steps:**
1. En `SceneSetupTools.cs`:
   - Configurar `StarsDisplayText` con tamaño `new Vector2(700, 80)` y `anchoredPosition = new Vector2(0, 150)`.
   - Añadir `dayEndUI.StarsDisplayText.verticalOverflow = VerticalWrapMode.Overflow;`.
2. En `DayEndSummaryUI.cs`:
   - En `RenderSummary()` y `RenderInfiniteSummary()`, asegurar que `StarsDisplayText.verticalOverflow = VerticalWrapMode.Overflow;`.

---

### Task 3: Redistribución de Áreas y Flujo de Texto en Tarjetas Fucsia
**Files:**
- Modify: `My project/Assets/Scripts/Editor/SceneSetupTools.cs`
- Modify: `My project/Assets/Scripts/UI/OrderCardUI.cs`

**Steps:**
1. En `SceneSetupTools.cs` (`OrderCardPrefab`):
   - `ConditionText`:
     - `anchorMin = new Vector2(0, 0.58f);`
     - `anchorMax = new Vector2(1, 0.76f);`
     - `fontSize = 22;`
     - `offsetMin = new Vector2(35, 0);`
     - `offsetMax = new Vector2(-35, -5);`
   - `ItemsListText`:
     - `anchorMin = new Vector2(0, 0.04f);`
     - `anchorMax = new Vector2(1, 0.58f);`
     - `fontSize = 21;`
     - `lineSpacing = 1.05f;`
     - `offsetMin = new Vector2(35, 8);`
     - `offsetMax = new Vector2(-35, -5);`
     - `verticalOverflow = VerticalWrapMode.Overflow;`
2. En `OrderCardUI.cs`:
   - Reforzar que en `UpdateCardData()` el interlineado y tamaño de fuente sean óptimos para lotes de empresa.

---

### Task 4: Menú Principal: Logo Elevado 3x y Botones Ampliados al Doble
**Files:**
- Modify: `My project/Assets/Scripts/Editor/MenuSetupTools.cs`

**Steps:**
1. En `MenuSetupTools.cs`:
   - `LeftHemisphere`: `sizeDelta = new Vector2(850, 950);`
   - `Logo_Overpacked`:
     - `anchoredPosition = new Vector2(0, 310);`
     - `sizeDelta = new Vector2(780, 320);`
   - Botón `PlayButton`:
     - `anchoredPosition = new Vector2(0, 75);`
     - `sizeDelta = new Vector2(520, 125);`
     - `fontSize = 46;`
   - Botón `OptionsButton`:
     - `anchoredPosition = new Vector2(0, -70);`
     - `sizeDelta = new Vector2(520, 125);`
     - `fontSize = 46;`
   - Botón `QuitButton`:
     - `anchoredPosition = new Vector2(0, -215);`
     - `sizeDelta = new Vector2(520, 125);`
     - `fontSize = 46;`

---

### Task 5: Recompilación, Reconstrucción de Escenas y Verificación con Unity MCP
**Files:**
- Rebuild: `MainMenuScene.unity` y `GameplayScene.unity`

**Steps:**
1. Llamar a `recompile` y confirmar 0 errores.
2. Ejecutar `eval` para `BuildMainMenuScene()` y `BuildGameplayScene()`.
3. En Play Mode:
   - Probar Menú Principal: Verificar logo elevado en la parte superior y botones de $520 \times 125$ px con texto negro grande a 46px.
   - Probar Gameplay: Agregar 1, 2, 3, 4, 5 ítems a la caja; verificar que con 3, 4 y 5 ítems las casillas se ordenan horizontalmente limpias, centradas y sin distorsión alguna.
   - Probar Tarjetas Fucsia: Abrir hover de pedido de empresa y verificar que el texto del objetivo ("...NO de titanio", "...instrumental de") y candidatos válidos se leen completos sin cortes.
   - Probar Fin de Jornada: Verificar que las estrellas doradas (`★ ★ ☆`) aparecen claramente sobre el texto de calificación.
