# Plan de Diseño: Sistema de Rankings y Paginación del Cuaderno de Modos

**Fecha:** 25 de Octubre de 2026  
**Estado:** Aprobado por el usuario (Decisiones D1 a D6)  
**Alcance:** `MainMenuScene.unity`, `CuadernoMenuController.cs`, `MenuSetupTools.cs`, `RankingManager.cs` (Nuevo), `DayEndSummaryUI.cs`.

---

## 1. Resumen de Comprensión (Understanding Summary)

* **Propósito:** Implementar un sistema persistente de ranking (Top 5) de mejores puntuaciones para la **Jornada Normal** y la **Jornada Infinita**, y refactorizar el Cuaderno del Menú Principal convirtiéndolo en un libro de 3 hojas físicas dobles.
* **Navegación:** Se utilizan las flechas CozyUI importadas (`ArrowBrownOuline22.png` a la izquierda y `ArrowBrownOuline23.png` a la derecha) con animación de 5 frames (`cuaderno_pasar_pagina_der_izq` e `izq_der`).
* **Estructura de las 3 Hojas:**
  * **Hoja 0 (Prueba Laboral / Tutorial):**
    * *Izquierda:* Título `"PRUEBA LABORAL"`, descripción del temario y botón verde `"INICIAR PRUEBA ▶"`.
    * *Derecha:* Ficha de supervisión con Mochi Miles, temario de 5 competencias lógicas y badge de estado (`"COMPLETADA [✓]"` vs `"PENDIENTE [!]"`).
  * **Hoja 1 (Jornada Normal):**
    * *Izquierda:* Título `"JORNADA NORMAL"`, descripción (turno 06:00-22:00, cuotas) y botón verde `"EMPECEMOS ▶"`.
    * *Derecha:* Tabla Top 5 de mejores ganancias netas (`$XXXX`), estrellas (`★`) y fecha (`dd/MM HH:mm`). Podio coloreado con Rich Text (`#FFD700`, `#D0D0D0`, `#CD7F32`).
  * **Hoja 2 (Jornada Infinita):**
    * *Izquierda:* Título `"JORNADA INFINITA"`, descripción (contrarreloj) y botón verde `"A TRABAJAR ▶"`.
    * *Derecha:* Tabla Top 5 de resistencia: Ronda Máxima alcanzada, Dinero acumulado total y fecha.

---

## 2. Decision Log Consolidado (D1 - D6)

| ID | Área | Decisión Aprobada | Justificación Técnica |
| :--- | :--- | :--- | :--- |
| **D1** | **Persistencia** | `RankingManager.cs` estático con `PlayerPrefs` y `JsonUtility`. | Desacoplado de GameObjects vivos; guardado atómico inmediato utilizable desde cualquier escena. |
| **D2** | **Formato Top 5** | 5 filas fijas con podio Rich Text (`#1` oro, `#2` plata, `#3` bronce, `#4`, `#5`) y relleno con guiones si está vacío. | Cumple con el requerimiento del profesor sin depender de emojis o glifos que puedan romperse. |
| **D3** | **Paginación 3 Hojas** | Contenedor con 3 objetos hijos (`Sheet_0_Tutorial`, `Sheet_1_NormalDay`, `Sheet_2_Infinite`). | Layouts limpios e independientes por hoja, editables y estables en Unity. |
| **D4** | **Flechas CozyUI** | Flecha 22 (izq) y Flecha 23 (der) en $Y = -230$. Flecha izq oculta en Hoja 0 y flecha der oculta en Hoja 2. | Guía clara al jugador y previene desbordamiento del índice de páginas (0 a 2). |
| **D5** | **Candado Anti-Spam** | Flag booleano `_isFlipping` en corrutina de cambio y botón de inicio. | Previene clics rápidos, animaciones simultáneas o cambios de escena en pleno volteo. |
| **D6** | **Audio Integrado** | `AudioManager.Instance.PlaySFX(PageFlipClip)` en corrutina de flip. | Respeta la configuración y sliders de volumen de efectos de sonido. |

---

## 3. Especificación Técnica por Componente

### A. `RankingManager.cs` (`Assets/Scripts/Core/RankingManager.cs`)
1. Modelos:
   * `NormalDayRecord`: `int NetMoney`, `int Stars`, `string Timestamp`.
   * `InfiniteRecord`: `int RoundsCleared`, `int TotalMoney`, `string Timestamp`.
   * Wrappers `NormalDayRankingData` e `InfiniteRankingData`.
2. Métodos públicos:
   * `RecordNormalDayScore(int netMoney, int stars)`
   * `RecordInfiniteScore(int roundsCleared, int totalMoney)`
   * `SetTutorialCompleted(bool completed)`
   * `IsTutorialCompleted() -> bool`
   * `GetNormalTop5() -> List<NormalDayRecord>` (retorna lista vacía si no hay registros).
   * `GetInfiniteTop5() -> List<InfiniteRecord>`

### B. Inyección en `DayEndSummaryUI.cs`
1. En `RenderDaySummary(DaySummaryData summary)`:
   * `RankingManager.RecordNormalDayScore(summary.NetMoney, summary.Stars);`
2. En `RenderArcadeSummary(int roundsCleared, int totalMoney, List<int> roundEarnings)`:
   * `RankingManager.RecordInfiniteScore(roundsCleared, totalMoney);`
3. En `RenderTutorialSummary(int completedOrders, int earnings)`:
   * `RankingManager.SetTutorialCompleted(true);`

### C. `CuadernoMenuController.cs` (Refactorización del Cuaderno)
1. Referencias a las 3 hojas: `Sheet_Tutorial`, `Sheet_NormalDay`, `Sheet_Infinite`.
2. Referencias a botones de flecha: `Button_PrevPage` (`ArrowBrownOuline22`), `Button_NextPage` (`ArrowBrownOuline23`).
3. Componente de volteo de página:
   * `PageFlipOverlay` (`Image`) con spritesheets `cuaderno_pasar_pagina_der_izq` (avance) y `cuaderno_pasar_pagina_izq_der` (retroceso).
   * En Frame 2 (90°): conmutación de hojas y repoblado de tablas de ranking.
   * Audio vía `AudioManager.Instance.PlaySFX(PageFlipClip)`.

### D. `MenuSetupTools.cs` (Construcción de la Escena `MainMenuScene.unity`)
1. Creación de la jerarquía de las 3 hojas con Canvas y componentes UI en la página izquierda y derecha.
2. Formateo de las tablas del Top 5 con la fuente `Fredoka-Bold` y Rich Text de colores.
3. Asignación de sprites de flechas de CozyUI en $X = \pm 380, Y = -230$.
