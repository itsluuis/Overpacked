# 📋 Documento de Diseño: Optimización de Interacción (Cajón de Operadores, Cuaderno 7x y Post-it)

> **Fecha:** 23 de Octubre de 2026  
> **Proyecto:** Overpacked (LogiPack) — Lógica Simbólica  
> **Estado:** Aprobado y Validado (Skill: Brainstorming)  

---

## 1. 🎯 Resumen de Entendimiento (Understanding Summary)
* **Apertura Confiable del Selector de Operadores (Ranura 2):** Implementar detección de clic directa con el *New Input System* (`Pointer.current`) y conversión de coordenadas ortográficas de cámara 2D con caching en `Awake()`, garantizando apertura inmediata e inmune a capas de UI.
* **Calibración y Ampliación del Cajón 1x4:** Incrementar la escala del cajón a `1.15f` en la mesa de trabajo (`-4.63f, -3.45f`), con offsets precisos para las 4 casillas (`-2.15f, -0.72f, +0.72f, +2.15f`) para que los símbolos ($\land, \lor, \rightarrow, \leftrightarrow$) encajen exactamente en el arte del sprite `cajon_1x4.png`.
* **Ciclo Completo de Animación del Cuaderno Didáctico:** 
  - Al hacer clic en reposo (margen derecho medio, tamaño `70x70`), viaja al centro de la pantalla (`(0, 0)`) y escala **7 veces** su tamaño original (`Vector3.one * 7.0f`).
  - Despliega las páginas de contenido y activa el `DarkOverlay`.
  - Al pulsar el botón "CERRAR" o hacer clic fuera del cuaderno (en el `DarkOverlay`), se ejecuta la animación inversa, regresando suavemente al margen derecho a su tamaño original `1.0f` y reanudando la partida.
* **Reubicación del Post-It en la Esquina Superior Derecha:** Posicionar el Post-It en `anchor = (1, 1)` con offset `(-25, -25)`, manteniéndolo despejado de los botones de pausa y cuaderno ubicados en el margen derecho central.
* **Aumento Tipográfico del Post-It (+6px):** Título a 26pt, Fórmula a 40pt (bold) y Descripción natural a 24pt.

---

## 2. 📝 Registro de Decisiones (Decision Log)

| ID | Tema | Decisión Aprobada | Alternativas Consideradas | Justificación Técnica |
|---|---|---|---|---|
| **D1** | Input Ranura 2 & Botones | Caching de `Camera.main` y `Collider2D` en `Awake()`; chequeo en `Update()` con `Pointer.current.press.wasPressedThisFrame` y `_collider.OverlapPoint(worldPos)` | Eventos legacy `OnMouseDown()` | Incompatibilidad de `OnMouseDown()` con New Input System en Unity 6 y colisiones 2D |
| **D2** | Escala y Posiciones Cajón | Escala `1.15f` en `(-4.63f, -3.45f)` con posiciones locales `[-2.15f, -0.72f, +0.72f, +2.15f]` | Escala `0.70f` con paso lineal `1.70f` | El cajón era pequeño y los símbolos quedaban desfasados de las casillas |
| **D3** | Animación Cuaderno 7x | Interpolación en corrutina con `unscaledDeltaTime` de `1.0f` a `7.0f` hacia el centro; cierre y retorno automático a `1.0f` por botón o clic en overlay | Tamaño fijo o apertura instantánea sin animación inversa | Máxima inmersión visual y cumplimiento estricto del feedback pedagógico |
| **D4** | Posición Post-it | Esquina superior derecha `(1, 1)` en `(-25, -25)` con fuentes +6px | A la izquierda de la pausa o superior izquierda | Despeja completamente el lateral derecho y la zona de reloj/dinero a la izquierda |

---

## 3. 🛠️ Plan de Implementación Incremental

### Fase 1: Controladores de Input Directo
- Modificar `OperatorDrawerButton.cs` para implementar la clase `OperatorSlotTrigger` y `OperatorDrawerButton` con el patrón aprobado de caching de cámara/collider y detección de puntero en `Update()`.

### Fase 2: Cuaderno Didáctico (Animación 7x y Retorno)
- Actualizar `GameplayNotebookGuideUI.cs` para:
  - Cachear posición idle y escala idle (`Vector3.one`).
  - Soportar animación hacia el centro con escala `Vector3.one * 7.0f`.
  - Asegurar que al cerrar, se anime de vuelta a la posición idle y escala idle.
  - Configurar el `DarkOverlay` con listener para cerrar al hacer clic fuera.

### Fase 3: Calibración Visual en SceneSetupTools
- Actualizar `SceneSetupTools.cs`:
  - Cajón 1x4 a escala `1.15f`, posición `y = -3.45f`, con offsets en `±2.15f` y `±0.72f`.
  - Post-It anclado a `(1, 1)` en `(-25, -25)` con fuentes +6px (`26pt`, `40pt`, `24pt`).
  - Orden de jerarquía del Canvas: `DarkOverlay` y `Button_NotebookGuide` correctamente ordenados.

### Fase 4: Reconstrucción y Verificación en Play Mode
- Ejecutar el menú `LogiPack/Construir Escena de Gameplay`.
- Entrar en Play Mode, verificar despliegue del cajón, selección de operador, apertura/cierre 7x del cuaderno y visualización del Post-It en la esquina superior derecha.
