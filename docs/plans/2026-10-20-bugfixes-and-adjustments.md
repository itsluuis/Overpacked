# Plan de Diseño: Corrección de Bugs y Ajustes de Gameplay (Beta 1.6 / Patch)

**Fecha:** 20 de Octubre de 2026  
**Estado:** Validado con Brainstorming  
**Área:** Gameplay, Lógica Proposicional Simbólica, UI, Visuales Ambientales  

---

## 1. Resumen de Entendimiento (Understanding Summary)
* **Objetivo:** Resolver el problema crítico de escala en drag & drop de ítems y cajas, reintegrar proposiciones moleculares compuestas ricas con notación formal y lista de ítems en las órdenes estándar, incrementar el tamaño de fuente en +10px para legibilidad, implementar la animación en bucle de 4 frames para lluvia y tormenta, ajustar la profundidad de capas de carteles/cintas (`SortingOrder`) y ratificar la calibración 1920x1080 de la cámara.
* **Propósito:** Brindar una experiencia de juego limpia, fluida y directamente alineada con los requerimientos pedagógicos de la cátedra de Lógica Simbólica.
* **Restricciones:** No tocar la cámara (ya calibrada a 1920x1080), preservar el ciclo de creación de cajas continuas en `BoxSpawner`, mantener la tarjeta compacta superior en el corcho y el detalle completo en el hover ampliado.

---

## 2. Supuestos y Requerimientos No-Funcionales
* **Escalas:** La escala base de la caja en mesa y despacho es `0.20f`. El proxy de arrastre de ítems en `DraggableItemSource` hereda `lossyScale` del slot del estante.
* **Lógica Simbólica:** Las órdenes estándar combinan condiciones ambientales (clima, temperatura) con requerimientos de embalaje usando conectivos lógicos formales ($\land, \lor, \rightarrow, \leftrightarrow, \neg$).
* **Rendimiento:** La animación climática de 4 frames cicla con temporizador ligero en `HUDEnvironmentVisuals` sin instanciaciones ni recolección de basura.
* **Profundidad:** Cintas en `SortingOrder = 2`, Carteles en `SortingOrder = 1` (pata detrás de la cinta).

---

## 3. Registro de Decisiones (Decision Log)

| Decisión | Alternativas Consideradas | Justificación |
|---|---|---|
| **Escala en Drag & Drop** | 1. Modificar prefab base.<br>2. Cachear `_baseScale` en `DraggableBox` y usar `lossyScale` en `DraggableItemSource`. | La opción 2 elimina de raíz el sobreescalado sin alterar las proporciones de los demás elementos de la escena. |
| **Generador de Proposiciones Moleculares** | 1. Motor abstracto AST.<br>2. Generación procedural de fórmulas moleculares compuestas con formalismo lógico y evaluación booleana directa. | La opción 2 es limpia, predecible, altamente testeable y cubre exactamente los ejemplos requeridos (implicaciones, bicondicionales y conjunciones compuestas). |
| **Animación de Clima** | 1. `AnimatorController` con clips.<br>2. Bucle secuencial de 4 sprites por código a ~6.6 FPS en `HUDEnvironmentVisuals`. | La opción 2 es ligera, sin dependencias complejas de clips en Assets, y reacciona de inmediato al cambio de estado climático. |
| **Jerarquía de Capas** | 1. Modificar posiciones en Z.<br>2. Ajustar `SortingOrder` (Cintas = 2, Carteles = 1). | En proyectos 2D de Unity, `SortingOrder` es el estándar más robusto e inmune a problemas de frustum o paralaje. |
| **Cámara** | 1. Cambiar resolución/scripts.<br>2. Mantener cámara intacta (`orthographicSize = 5.4`) y configurar Game View a 1920x1080. | El fondo `mapa_gameplay_almacen.png` ya mide exactamente 1920x1080 (19.2 x 10.8 unidades mundiales a PPU 100). |

---

## 4. Especificación Técnica de Implementación

### 4.1. `DraggableBox.cs`
* Declarar variable `private Vector3 _baseScale;`.
* En `Awake()` / `Start()`: `_baseScale = transform.localScale;`.
* En `OnMouseDown()`: Aplicar leve feedback sin romper la escala (`transform.localScale = _baseScale * 1.02f;`).
* En `OnMouseUp()`: Restaurar `transform.localScale = _baseScale;`.
* Preservar estrictamente las llamadas `Spawner.NotifyBoxMovedFromTable()`.

### 4.2. `DraggableItemSource.cs`
* Al crear `_dragInstance` en `OnMouseDown()`:
  ```csharp
  _dragInstance.transform.localScale = transform.lossyScale;
  ```
* En `OnMouseUp()`: Asegurar destrucción limpia de `_dragInstance`.

### 4.3. `EnvironmentalCondition.cs` & `OrderGenerator.cs`
* Ampliar `EnvironmentalCondition` para admitir composiciones complejas:
  * Campo `public string FormalFormula;` (ej. `(p ∧ q) → ¬s`).
  * Soporte para antecedente compuesto (`LeftPredicateSecondary`, `OperatorSecondary`) o casos estructurados específicos:
    * `(Lloviendo ∧ Frío) → ¬FilmTérmico`
    * `Calor ↔ (¬Espuma ∧ ¬FilmTérmico)`
    * `Frío → FilmTérmico`
    * `Tormenta ↔ Espuma`
    * `Soleado`
* Actualizar `OrderGenerator.cs` con la tabla calibrada de condiciones y cálculo de bonus lógicos.

### 4.4. `OrderCardUI.cs`
* Incrementar en +10px el `fontSize` de los elementos textuales:
  * Encabezado: +10px.
  * `ConditionText`: +10px (mostrando `<b>Regla:</b> {cond.NaturalLanguageText}\n<color=#003366><b>Fórmula:</b> {cond.FormalFormula}</color>`).
  * `ItemsListText`: +10px (mostrando cada ítem con cantidad explícita).
* Ajustar dimensiones del panel desplegado (`BigDetailsBackground`) para contener holgadamente el texto aumentado.

### 4.5. `HUDEnvironmentVisuals.cs`
* Añadir campos serializados:
  * `public Sprite[] WindowRainFrames;` (4 frames)
  * `public Sprite[] WindowStormFrames;` (4 frames)
* En `Update()`, si el clima es `Rainy` o `Stormy`, ciclar el frame cada `0.15f` segundos:
  ```csharp
  _weatherAnimTimer += Time.deltaTime;
  if (_weatherAnimTimer >= 0.15f)
  {
      _weatherAnimTimer = 0f;
      _weatherFrameIndex = (_weatherFrameIndex + 1) % 4;
      WindowWeatherRenderer.sprite = isRain ? WindowRainFrames[_weatherFrameIndex] : WindowStormFrames[_weatherFrameIndex];
  }
  ```
* Si el clima es `Sunny` o `Cloudy`, asignar sprite estático y reiniciar temporizador.

### 4.6. `SceneSetupTools.cs` / Configuración de Escena
* `Banda_Despacho` y `Banda_Revision`: `sortingOrder = 2`.
* `Cartel_Despacho` y `Cartel_Revision`: `sortingOrder = 1`.
* `Slots_Despacho` (Selectores 45%): `sortingOrder = 3`.
* Asignar los arrays de frames de lluvia y tormenta a `HUDEnvironmentVisuals` en la escena.
