# 📦 Especificación de Diseño: Órdenes Especiales de Empresas (Teoría de Conjuntos)

## 1. Resumen de Entendimiento (Understanding Summary)
* **Objetivo:** Introducir pedidos especiales emitidos por "Empresas Clientes" con nombres simpáticos e infantiles, cuyas solicitudes se resuelven mediante operaciones formales de Teoría de Conjuntos ($\cup, \cap, -, A^c$).
* **Frecuencia:** Aparecen con un **15% de probabilidad** (el 85% restante son pedidos estándar).
* **Mecánica de Empaque (Cardinalidad Flexible):** La orden pide una cantidad total $|S| = N$ de insumos (2 a 3 unidades). El jugador puede empacar cualquier combinación válida de ítems dentro del subconjunto resultante (incluso unidades repetidas del mismo ítem).
* **Presentación Temática:** Texto en lenguaje natural inmersivo de almacén, sin fórmulas matemáticas explícitas en pantalla.
* **Economía y Temporizador:** Alta recompensa base ($100 a $150), mayor tiempo de armado (70 a 80 segundos) y penalización reducida por caducidad ($35 a $45).
* **Embalaje Opcional:** Pueden exigir opcionalmente materiales de protección (Espuma / Film Térmico).

---

## 2. Supuestos y Restricciones (Assumptions & Constraints)
1. **Garantía de No Vacuidad:** Toda regla de conjuntos generada proceduralmente garantiza al menos 1 ítem válido existente en la estantería ($S \neq \emptyset$).
2. **Capacidad de Caja:** Los pedidos solicitan 2 o 3 unidades en total, adaptándose a la capacidad visual de la caja.
3. **Propinas Implícitas por Velocidad:** La comanda solo muestra el pago base ofrecido. La propina (+25%, +10%, +0%) se calcula y abona automáticamente en el momento del despacho según el color de la barra de tiempo.

---

## 3. Registro de Decisiones (Decision Log)

| ID | Decisión | Alternativas Consideradas | Razón de la Elección |
| :---: | :--- | :--- | :--- |
| **DEC-01** | Teoría de conjuntos como mecánica activa de ritmo rápido. | Pantallas pedagógicas o cuestionarios tipo test. | El usuario solicitó un juego genuino y divertido con el contenido de la materia, no una herramienta escolar aburrida. |
| **DEC-02** | Selección flexible por cardinalidad ($|S| = N$). | Subconjunto exacto rígido. | Otorga libertad estratégica y agilidad al empaque, permitiendo al jugador elegir qué piezas del conjunto empacar. |
| **DEC-03** | Enfoque 1: Motor Modular `SetTheoryRule`. | Plantillas fijas en código o herencia polimórfica (`CompanyOrder`). | Máxima modularidad, fácil mantenimiento y cero riesgo de romper el sistema de pedidos existente. |
| **DEC-04** | Nombres infantiles y cómicos para las empresas cliente. | Nombres corporativos serios. | Mantiene el tono divertido, simpático y estilo caricatura (*Overcooked / Wilmot's Warehouse*). |
| **DEC-05** | No mostrar el texto de "+propina" en la comanda. | Desglose predictivo de propina en la tarjeta. | Mantiene la comanda limpia; la propina se gana orgánicamente por la agilidad del jugador en el despacho. |

---

## 4. Matriz de Clasificación de Ítems (Universo $U = 12$)

| Ítem | Ubicación Estante | Familia | Tier | Atributos Físicos (`ItemAttributes`) |
| :--- | :---: | :---: | :---: | :--- |
| **Tornillo Estándar** | Col 0, Fila 2 | Metales | Tier 1 ($10) | `Metal` |
| **Perno Galvanizado** | Col 0, Fila 1 | Metales | Tier 2 ($18) | `Metal` \| `Tool` |
| **Perno de Titanio** | Col 0, Fila 0 | Metales | Tier 3 ($28) | `Metal` \| `Tool` |
| **Cable Simple** | Col 1, Fila 2 | Electricidad | Tier 1 ($10) | `Electrical` |
| **Enchufe Aislado** | Col 1, Fila 1 | Electricidad | Tier 2 ($18) | `Electrical` \| `Tool` |
| **Batería de Litio** | Col 1, Fila 0 | Electricidad | Tier 3 ($28) | `Electrical` \| `Fragile` |
| **Vidrio Plano** | Col 2, Fila 2 | Óptica | Tier 1 ($10) | `Fragile` |
| **Lente Halógeno** | Col 2, Fila 1 | Óptica | Tier 2 ($18) | `Fragile` \| `Electrical` |
| **Cristal de Lab.** | Col 2, Fila 0 | Óptica | Tier 3 ($28) | `Fragile` \| `Tool` |
| **Tela de Algodón** | Col 3, Fila 2 | Textiles | Tier 1 ($10) | `Fabric` |
| **Lona Impermeable** | Col 3, Fila 1 | Textiles | Tier 2 ($18) | `Fabric` \| `Tool` |
| **Tejido Ignífugo** | Col 3, Fila 0 | Textiles | Tier 3 ($28) | `Fabric` |

---

## 5. Diseño Técnico y Arquitectura de Componentes

### 5.1. `SetTheoryRule.cs` (`LogiPack.Logic`)
* Enum `SetOperator`: `Union`, `Intersection`, `Difference`, `Complement`.
* Struct `SetCriterion`: Criterio por `FamilyName`, `ItemAttributes` o `ItemTier`.
* Método `HashSet<ItemData> ResolveValidItems(List<ItemData> universe)`: Resuelve y retorna los ítems que cumplen la regla.

### 5.2. `Order.cs` (`LogiPack.Orders`)
* Nuevas propiedades: `IsCompanyOrder`, `SetRule`, `TargetItemCount`, `CompanyName`, `CustomPenalty`.
* Modificación de `MatchesItems(Dictionary<ItemData, int> packedItems)`:
  * Si es orden de empresa: verifica que la suma total sea `TargetItemCount` y que todos los ítems pertenezcan a `ResolveValidItems`.
  * Si es estándar: conserva la comparación exacta existente.

### 5.3. `OrderGenerator.cs` (`LogiPack.Orders`)
* Tirada de 15% para generar orden de empresa.
* Genera regla válida ($S \neq \emptyset$), selecciona nombre temático cómico (*Ferretería Pim Pom Pim*, *Chispitas & Voltios Bip-Bop*, etc.), asigna duración (70-80s), recompensa ($100-$150) y penalización ($35-$45).

### 5.4. `OrderCardUI.cs` (`LogiPack.UI`)
* Respaldo visual con tinte dorado/ámbar si `IsCompanyOrder == true`.
* Título con nombre de la empresa.
* Texto temático de solicitud en el panel de detalle con el pago base ofrecido (sin el cálculo previo de propina).
