# Overpacked: Cierre de Jornada y Reconfiguración Económica - Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implementar el balance económico modular de pedidos (12 ítems, recompensas según complejidad/materiales, propinas tricolor, penalizaciones justas) y la Pantalla de Resultados y Evaluación al culminar la jornada a las 22:00 PM según las especificaciones del GDD y Lógica Simbólica.

**Architecture:** 
- Arquitectura orientada a eventos en Unity 6 con ScriptableObjects (`ItemData`) enriquecidos con Tiers y precios base.
- Generador procedural (`OrderGenerator`) con fórmula modular matemática ($Recompensa = Ítems + Embalaje + Lógica$).
- `PackagingZoneManager` y `OrderManager` calculan propinas dinámicas según la barra tricolor (Verde +25%, Amarillo +10%, Rojo +0%).
- `EconomyManager` centraliza estadísticas financieras, cuotas de 4 min ($350, $700, $1000) y cálculo de prestigio del GDD con sobre-cuota (+1.5% por pedido extra).
- `DayEndSummaryUI` provee una interfaz modal estilo *Papers, Please / Overcooked* activada por `EnvironmentManager.OnDayCompleted`.
- Automatización completa mediante `SceneSetupTools.cs`.

**Tech Stack:** Unity 6 (6000.6.5f1), C# (.NET Standard 2.1), URP 2D, Unity UI (uGUI), Unity New Input System.

---

### Task 1: Matriz de 12 Ítems y Modelo de Datos en `ItemData.cs`

**Files:**
- Modify: `My project/Assets/Scripts/Core/ItemData.cs`

**Step 1: Extender `ItemData.cs` con precio base y tiers**
Agregar campos `BasePrice`, `Tier` y enum de tiers ($10 básico, $18 medio, $28 avanzado):

```csharp
namespace LogiPack.Core
{
    public enum ItemTier
    {
        Tier1_Basic = 10,
        Tier2_Medium = 18,
        Tier3_Advanced = 28
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "LogiPack/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string ItemId;
        public string ItemName;
        public Sprite Icon;
        public Color BaseColor = Color.white;
        public ItemAttributes Attributes;

        [Header("Economía")]
        public ItemTier Tier = ItemTier.Tier1_Basic;
        public int BasePrice = 10;
        public string FamilyName = "General";

        public bool HasAttribute(ItemAttributes attr)
        {
            return (Attributes & attr) == attr;
        }
    }
}
```

**Step 2: Verificar compilación en Unity**
- Validar mediante MCP `recompile_status`.

---

### Task 2: Modelo de Pedido con Desglose y Propinas en `Order.cs`

**Files:**
- Modify: `My project/Assets/Scripts/Orders/Order.cs`

**Step 1: Añadir desglose de costos y cálculo de propina en `Order.cs`**
- Almacenar `ItemsCost`, `PackagingBonus`, `LogicBonus` y `BaseReward`.
- Función `GetTipPercent()`:
  - $\ge 60\%$ tiempo restante $\rightarrow$ 25% (Zona Verde).
  - $25\% \le t < 60\%$ $\rightarrow$ 10% (Zona Amarilla).
  - $< 25\%$ $\rightarrow$ 0% (Zona Roja).
- Función `GetTipAmount()` y `GetTotalPayment()`.

```csharp
public class Order
{
    public string OrderId;
    public EnvironmentalCondition Condition;
    public List<OrderItemRequirement> Requirements = new List<OrderItemRequirement>();
    
    public float MaxTime;
    public float RemainingTime;

    // Desglose económico
    public int ItemsCost;
    public int PackagingBonus;
    public int LogicBonus;
    public int MonetaryReward => ItemsCost + PackagingBonus + LogicBonus;

    public bool IsExpired => RemainingTime <= 0;
    public float TimeNormalized => Mathf.Clamp01(RemainingTime / MaxTime);

    public float GetTipPercent()
    {
        float ratio = TimeNormalized;
        if (ratio >= 0.60f) return 0.25f; // Zona Verde: +25%
        if (ratio >= 0.25f) return 0.10f; // Zona Amarilla: +10%
        return 0f;                       // Zona Roja: +0%
    }

    public int GetTipAmount()
    {
        return Mathf.RoundToInt(MonetaryReward * GetTipPercent());
    }

    public int GetTotalPayment()
    {
        return MonetaryReward + GetTipAmount();
    }
}
```

---

### Task 3: Generador Procedural Modular en `OrderGenerator.cs`

**Files:**
- Modify: `My project/Assets/Scripts/Orders/OrderGenerator.cs`

**Step 1: Implementar fórmula de cálculo matemático**
- Sumar `Item.BasePrice * count` para cada ítem requerido.
- Calcular bonus de embalaje:
  - Sin embalaje especial (o condición no requiere material específico) $\rightarrow$ +$0.
  - 1 material (Espuma o Film) $\rightarrow$ +$15.
  - Ambos materiales (Espuma $\land$ Film) $\rightarrow$ +$35.
- Calcular bonus de complejidad lógica:
  - Proposición atómica simple (`Operator == LogicOperator.None`) $\rightarrow$ +$10.
  - Fórmula molecular con conectivo ($\land, \lor, \rightarrow, \oplus$) $\rightarrow$ +$20.
  - Bicondicional formal ($\leftrightarrow$) $\rightarrow$ +$35.
- Asignar `ItemsCost`, `PackagingBonus`, `LogicBonus` al instanciar `Order`.

---

### Task 4: Despacho con Propinas en `PackagingZoneManager` y `OrderManager`

**Files:**
- Modify: `My project/Assets/Scripts/Managers/PackagingZoneManager.cs`
- Modify: `My project/Assets/Scripts/Managers/OrderManager.cs`

**Step 1: Actualizar evento de despacho para incluir propina**
- En `PackagingZoneManager`, calcular la propina al momento exacto del despacho:
  `int tip = candidateOrder.GetTipAmount();`
- Enviar evento: `OnOrderDispatched?.Invoke(box, candidateOrder, tip);`
- En `OrderManager.HandleOrderDispatched`:
  `Economy.AddEarnings(order.MonetaryReward, tip);`

---

### Task 5: Rediseño Integral de `EconomyManager.cs`

**Files:**
- Modify: `My project/Assets/Scripts/Managers/EconomyManager.cs`

**Step 1: Estructurar estadísticas completas y calibración del GDD**
- Cuotas oficiales para jornada de 4 min:
  - Mínima: $350 (1 Estrella).
  - Media: $700 (2 Estrellas).
  - Máxima: $1,000 (3 Estrellas).
- Métricas auditables:
  - `GrossEarnings`: Suma de recompensas base ganadas.
  - `TotalTips`: Suma de propinas ganadas.
  - `TotalPenalties`: Suma de penalizaciones por caducidad.
  - `CurrentMoney`: Saldo neto en mano = `GrossEarnings + TotalTips - TotalPenalties` (nunca menor a 0 en pantalla, pero trackeado con precisión).
  - `CompletedOrdersCount` y `ExpiredOrdersCount`.
- Struct / DTO `DailySummary`:
  ```csharp
  public struct DailySummary
  {
      public int Stars;                // 0 a 3
      public string RatingTitle;       // "Día Pésimo", "Día Normal", "Buen Día", "Día Excelente"
      public float PrestigeImpact;     // e.g. -5.0%, 0.0%, +3.0%, +7.0% + bonus
      public float ExtraBonusPercent;  // e.g. +4.5% si superó la cuota máxima
      public int GrossEarnings;
      public int TotalTips;
      public int TotalPenalties;
      public int NetMoney;
      public int CompletedOrders;
      public int ExpiredOrders;
  }
  ```
- Método `CalculateDailySummary()`:
  - 0 Estrellas: < $350 $\rightarrow$ Prestigio -5%, "Día Pésimo".
  - 1 Estrella: $350 - $699 $\rightarrow$ Prestigio +0%, "Día Normal".
  - 2 Estrellas: $700 - $999 $\rightarrow$ Prestigio +3.0%, "Buen Día".
  - 3 Estrellas: $\ge $1,000 $\rightarrow$ Prestigio +7.0% + $(\lfloor(Net - 1000)/70\rfloor \times 1.5\%)$, "Día Excelente".
- Método `ResetForNewDay()`: Limpia acumuladores para iniciar el día siguiente.

---

### Task 6: Control de Ciclo y Fin de Jornada en `EnvironmentManager.cs`

**Files:**
- Modify: `My project/Assets/Scripts/Managers/EnvironmentManager.cs`

**Step 1: Evento formal de culminación y reinicio de día**
- Al llegar `_elapsedRealTime >= REAL_DAY_DURATION` (22:00 PM):
  - Activar `IsDayOver = true`.
  - Disparar `OnDayCompleted?.Invoke()`.
- Agregar método público `RestartDay()`:
  - Reinicia reloj a las 06:00 AM, `_elapsedRealTime = 0`, clima despejado, temperatura normal.
  - Dispara `OnEnvironmentChanged`.

---

### Task 7: Pantalla Modal de Resultados (`DayEndSummaryUI.cs`)

**Files:**
- Create: `My project/Assets/Scripts/UI/DayEndSummaryUI.cs`

**Step 1: Crear componente visual con estética Overcooked / Papers, Please**
- Fondo oscurecido semitransparente con tarjeta central de reporte de auditoría.
- Header: *"📋 REPORTE DE JORNADA LABORAL — 22:00 PM"*.
- Estrellas dinámicas: 3 contenedores de estrellas (⭐ doradas con brillo o apagadas ⚫).
- Título de Calificación Oficial (ej. *"⭐⭐ DÍA EXCELENTE"*).
- Badge de Prestigio: *"Prestigio de la Empresa: +11.5%"* (verde si sube, gris neutro, rojo si baja).
- Desglose Financiero detallado:
  - 📦 Pedidos Entregados: `X pedidos (+${GrossEarnings})`
  - ⚡ Propinas por Agilidad: `+${TotalTips}`
  - ⏳ Pedidos Caducados: `Y pedidos (-${TotalPenalties})`
  - 💵 Saldo Neto Final: `<b>${NetMoney}</b>`
- Comparativa de Cuotas alcanzadas:
  - Mínima ($350) `[✓ Alerza]`
  - Media ($700) `[✓ Alerza]`
  - Máxima ($1,000) `[✓ Alerza]`
- Botón interactivo:
  - *"▶ Siguiente Jornada / Reiniciar Turno"*: Invoca `Environment.RestartDay()`, limpia pedidos y cierra el panel modal.

---

### Task 8: Automatización y Configuración de Escena en `SceneSetupTools.cs`

**Files:**
- Modify: `My project/Assets/Scripts/Editor/SceneSetupTools.cs`

**Step 1: Crear los 12 ScriptableObjects de Ítems en `Assets/Data/Items/`**
- Familia 1 (Metales): Tornillo Estándar ($10), Perno Galvanizado ($18), Perno de Titanio ($28).
- Familia 2 (Electricidad/Herramientas): Cable Simple ($10), Enchufe Aislado ($18), Batería de Litio ($28).
- Familia 3 (Óptica/Frágiles): Vidrio Plano ($10), Lente Halógeno ($18), Cristal de Laboratorio ($28).
- Familia 4 (Textiles): Tela de Algodón ($10), Lona Impermeable ($18), Tejido Ignífugo ($28).

**Step 2: Configurar la Estantería 4x3 con los 12 Ítems**
- Columna 0 (Metales): Tier 3 (Arriba), Tier 2 (Medio), Tier 1 (Abajo).
- Columna 1 (Electricidad): Tier 3, Tier 2, Tier 1.
- Columna 2 (Óptica): Tier 3, Tier 2, Tier 1.
- Columna 3 (Textiles): Tier 3, Tier 2, Tier 1.

**Step 3: Construir el Panel `DayEndSummaryPanel` en el Canvas**
- Maquetar jerarquía UI con textos de alto contraste, layout elements y botón de reinicio enlazado a `DayEndSummaryUI`.

---

### Task 9: Validación en Unity 6 y Play Mode

**Steps:**
1. Recompilar y ejecutar `LogiPack -> Construir Todo con UI`.
2. Validar consola (0 errores, 0 warnings).
3. Probar en Play Mode acelerando o simulando el tiempo hasta las 22:00 PM.
4. Verificar que se despliega el panel modal con las estrellas, cálculo de propinas y prestigio.
5. Probar el botón de reinicio y verificar que el día comienza de nuevo a las 06:00 AM limpiamente.
