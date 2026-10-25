# Plan de Diseño y Corrección de Bugs: Tutorial, HUD y Cierre de Jornada

**Fecha:** 25 de Octubre de 2026  
**Estado:** Aprobado por el usuario (Decisiones D1 a D6)  
**Alcance:** `TutorialScene.unity`, `SceneSetupTools.cs`, `AssemblyWorkbenchManager.cs`, `PropositionalBox.cs`, `TutorialDirector.cs`, `TutorialHighlightFX.cs`, `TutorialDialogueUI.cs`, `DayEndSummaryUI.cs`, `CuadernoMenuController.cs`, `MenuSetupTools.cs`.

---

## 1. Resumen de Comprensión (Understanding Summary)

* **Propósito:** Resolver de forma definitiva una ronda de 4 bugs operativos/visuales en el tutorial y 3 ajustes de presentación, eliminando carteles innecesarios, alineando zonas nativas impresas, evitando la duplicación de cajas y homologando la terminología a "Prueba Laboral" sin prestigio empresarial.
* **Restricción Esencial:** La lógica de armado, casillas y drag & drop debe mantenerse 100% fiel e idéntica a la jornada normal.
* **Consumo Terminal:** Toda caja en la ranura de resultado solo debe desvincularse cuando sea consumida efectivamente por despacho, revisión o anidamiento como sub-caja.

---

## 2. Decision Log Consolidado (D1 - D6)

| ID | Área | Decisión Aprobada | Justificación Técnica |
| :--- | :--- | :--- | :--- |
| **D1** | **Presencia de Mochi** | Mochi solo visible durante el diálogo (`SetActive(false)` al ceder el turno). | Elimina al 100% el bloqueo visual y físico del cajón de operadores en $x = -4.63$ y las ranuras. |
| **D2** | **Ciclo de Vida de Caja** | Retención de posesión de `_activeResultBox`; limpieza de operandos solo en consumo terminal (`OnActiveBoxDestroyed`). | Evita la acumulación de cajas duplicadas y textos solapados ("p p q") al cancelar o fallar un arrastre. |
| **D3** | **HighlightFX Estático** | Sin rebote senoidal (`BounceAmplitude = 0`), `sortingOrder >= 25`, centrado con `offset = Vector3.zero`. | Máxima precisión visual sin vibraciones ópticas molestas. |
| **D4** | **Zonas Nativas de Despacho/Revisión** | Eliminar carteles de madera. Colliders invisibles `BoxCollider2D` con `isTrigger = true` y tamaño $1.6 \times 1.6$ en $(2.37, 1.12)$ y $(2.37, -0.94)$. | Respeta el arte original de `mesa_trabajo.png` y activa limpiamente `hit.name.Contains("Despacho")` y `TrashBin`. |
| **D5** | **Botón de Menú HUD** | Corregir ruta a `Assets/Art/CozyUI/Buttons/Square/SquareButton1_wood.png` (remover 's' de 'Squares'). | Restaura el botón de madera con el icono de casa idéntico a la jornada normal. |
| **D6** | **Textos y Prestigio** | Sustituir "Operario Graduado" por "Capacitación Completada", homologar "Inducción" a "Prueba", y desactivar el Prestigio Empresarial en el tutorial (`PrestigeBadgeText.SetActive(false)`). | Coherencia con el carácter formativo del tutorial, asegurando reactivar el prestigio en `RenderDaySummary` normal. |

---

## 3. Especificación Técnica por Archivo

### A. `AssemblyWorkbenchManager.cs` y `PropositionalBox.cs`
1. En `PropositionalBox.cs`:
   - En `OnMouseDown()`, eliminar la llamada `workbench.OnResultBoxPickedUp(this)`. La caja en arrastre no desvincula la propiedad en la mesa.
   - En `OnMouseUp()`, si el drop es inválido, `ReturnToHome()` devuelve la caja a `ResultSlot_Transform.position`.
2. En `AssemblyWorkbenchManager.cs`:
   - Mantener `_activeResultBox` asignado mientras la caja exista.
   - En `OnActiveBoxDestroyed(PropositionalBox box)`:
     ```csharp
     if (_activeResultBox == box)
     {
         _activeResultBox = null;
         ClearOperand(0);
         ClearOperand(2);
     }
     ```
   - En `SpawnOrUpdateResultBox`: si `_activeResultBox != null`, simplemente llamar a `_activeResultBox.SetFormula(node, depth)` y actualizar sus iconos en lugar de instanciar un clon.

### B. `SceneSetupTools.cs` (Construcción de `TutorialScene.unity`)
1. **Zonas Nativas:**
   - Retirar creación de `SpriteRenderer` para `cartel_despacho.png` y `cartel_revision.png`.
   - Crear `Zona_Despacho` en `Vector3(2.37f, 1.12f, 0f)` con `BoxCollider2D` `(1.6f, 1.6f)`, `isTrigger = true`.
   - Crear `Zona_Revision` en `Vector3(2.37f, -0.94f, 0f)` con `BoxCollider2D` `(1.6f, 1.6f)`, `isTrigger = true`, `tag = "TrashBin"`.
2. **Botón HUD:**
   - Corregir línea 1049: `Assets/Art/CozyUI/Buttons/Square/SquareButton1_wood.png`.
3. **TutorialHighlightFX:**
   - Asignar `BounceAmplitude = 0f`.
   - Fijar `sortingOrder = 35`.

### C. `TutorialDirector.cs` y `TutorialDialogueUI.cs`
1. Al terminar cada bloque de diálogo (`onComplete`), invocar `DialogueUI.HideDialogue(keepMochiVisible: false)`.
2. Durante los pasos interactivos, Mochi y su contenedor de UI permanecen desactivados (`SetActive(false)`), liberando el espacio para que el jugador manipule el cajón de operadores y las ranuras.
3. Despacho en TutorialDirector apunta a `Zona_Despacho.transform` (`Vector3(2.37f, 1.12f, 0f)`).

### D. `DayEndSummaryUI.cs`
1. En `RenderTutorialSummary()`:
   - `HeaderTitleText.text = "🎓 PRUEBA LABORAL COMPLETADA\n<size=18>SUPERVISIÓN: MOCHI MILES</size>";`
   - `RatingTitleText.text = "<color=#00FFAA><b>CAPACITACIÓN COMPLETADA</b></color>";`
   - `if (PrestigeBadgeText != null) PrestigeBadgeText.gameObject.SetActive(false);`
   - `FinancialBreakdownText.text`: `"📦 Pedidos de Prueba: 5/5..."`
2. En `RenderDaySummary()` (juego normal):
   - `if (PrestigeBadgeText != null) PrestigeBadgeText.gameObject.SetActive(true);` para blindar que la jornada regular siempre muestre el prestigio.

### E. `CuadernoMenuController.cs` y `MenuSetupTools.cs`
1. Reemplazar todas las apariciones de "INDUCCIÓN" por "PRUEBA" ("PRUEBA LABORAL", "INICIAR PRUEBA ▶", "PRUEBA (TUTORIAL)").
