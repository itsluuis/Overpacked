# 📋 ESPECIFICACIÓN DE DISEÑO E INTEGRACIÓN DE ASSETS — OVERPACKED

## 1. 📌 Resumen de Entendimiento
El rediseño integral de la pantalla de gameplay adapta todos los assets vectoriales en alta definición recién incorporados, respetando la maqueta aprobada por el usuario y garantizando una experiencia visual cartoon, limpia y altamente legible para la entrega académica de Lógica Simbólica en Unity 6 (URP 2D).

---

## 2. 📝 Registro de Decisiones (Decision Log)

* **Decisión 1 (Cajas Abiertas vs. Cerradas):** La caja de empaque en la mesa de trabajo (`Assembling`) utiliza `caja_carton_abierta.png` con sus ítems visibles. Al ser depositada en cualquier ranura de la banda de despacho (`InPackagingSlot`), commuta a `caja_carton_cerrada.png` y oculta los ítems interiores para reflejar que fue sellada con cinta adhesiva.
* **Decisión 2 (Slots con Indicador Gráfico):** Tanto los slots de espera en la banda de despacho como el área de descarte en la banda de revisión se renderizan mediante `selector_cuadrado.png` con opacidad calibrada al 45% (`Color(1, 1, 1, 0.45f)`).
* **Decisión 3 (Banda de Revisión en Reemplazo del Basurero):** Se sustituye el concepto de basurero por una banda transportadora dedicada a la derecha con `cartel_revision.png`. Al depositar una caja allí, el sistema la destruye de forma justificada en el lore del almacén y genera de inmediato una nueva caja en la mesa.
* **Decisión 4 (Post-its Temáticos con Tachuelas Físicas):** 
  * *Pedidos Estándar:* Nota adhesiva pequeña amarilla (`popstick_amarillo_pequeno.png`) con tachuela roja (`tachuela_roja.png`). En hover: despliega la nota grande (`popstick_amarillo_grande.png`).
  * *Pedidos Especiales de Empresa:* Nota adhesiva pequeña fucsia (`popstick_fucsia_pequeno.png`) con tachuela amarilla (`tachuela_amarilla.png`). En hover: despliega la nota grande (`popstick_fucsia_grande.png`).
* **Decisión 5 (Barra de Tiempo Decreciente con Relleno Horizontal):** La barra de tiempo de cada tarjeta utiliza `Image.Type.Filled` horizontal (de derecha a izquierda) combinada con el degradado tricolor (Verde $\ge 60\%$, Amarillo $25\% \dots 60\%$, Rojo $< 25\%$).
* **Decisión 6 (Márgenes Seguros y Tipografía Fredoka):** Todos los textos dentro de los post-its cuentan con padding/márgenes interiores seguros para no cortarse con la silueta orgánica del papel, utilizando la tipografía oficial `Fredoka` en alta nitidez.
* **Decisión 7 (Estanterías Flanqueantes 2×3):** La cuadrícula de 12 ítems se distribuye en dos estanterías físicas (`estanteria_3_pisos.png`):
  * *Izquierda:* Columnas 1 (Metales) y 2 (Electricidad), filas Tier 1 a 3.
  * *Derecha:* Columnas 3 (Óptica) y 4 (Textiles), filas Tier 1 a 3.

---

## 3. 🗺️ Arquitectura de Escena y Capas Visuales (Sorting Layers)

```
[ Fondo Clima Exterior: -2 ]  --> clima_soleado / nublado / lloviendo / tormenta (detrás de la ventana y compuerta)
[ Almacén Gameplay: 0 ]       --> mapa_gameplay_almacen (con transparencia alfa en ventana y compuerta)
[ Props de Muro y Pared: 1 ]  --> TV Temperatura (arriba) y TV Clima (abajo) a la izquierda de ventana
                                  Reloj Digital LED hora a hora a la derecha de ventana
                                  Cartel Despacho (izq) y Cartel Revisión (der)
                                  Estantería 2x3 Izquierda y Estantería 2x3 Derecha
                                  Corcho superior con Post-Its de Pedidos
[ Superficies de Trabajo: 2 ] --> Mesa Central de Madera
                                  Banda Transportadora Despacho (izquierda) y Banda Revisión (derecha)
                                  Pila de Cajas Planas (caja_carton_sin_armar_frontal)
[ Objetos Interactivos: 3 ]   --> Caja Activa en Mesa (caja_carton_abierta con overlays e ítems)
                                  Insumos arrastrables en estantes (12 sprites HD)
                                  Selectores cuadrados (45% opacidad) en slots
[ Interfaz Canvas UI: 4 ]     --> Botones de Embalaje (Espuma y Film) en la base de la mesa
                                  Marco de Dinero Plateado con saldo actual
                                  Hover desplegable de notas grandes (Raycast Target prioritario)
                                  Modal Fin de Jornada (pantalla_fin_jornada) a las 22:00 PM
```

---

## 4. 🧩 Plan de Implementación por Fases

1. **Fase 1: Motor de Cajas y Sprites de Ítems (Data & Core)**
   * Actualizar `InitialSetupTools.cs` para asignar a los 12 `ItemData` los nuevos sprites de `Assets/Art/Items/`.
   * Actualizar `PackageBox.cs` para soportar cambio de sprite (`OpenBoxSprite` vs `ClosedBoxSprite`), ocultando los hijos de ítems internos en `InPackagingSlot`.
2. **Fase 2: Reconstrucción de la Escena y Mobiliario Físico (`SceneSetupTools.cs`)**
   * Configurar el fondo `mapa_gameplay_almacen.png` y la capa climática trasera interactiva con `EnvironmentManager`.
   * Posicionar Monitores TV a la izquierda y Reloj Digital a la derecha de la ventana.
   * Maquetar las dos estanterías 2×3 a los lados de la mesa central con los colliders y fuentes de arrastre New Input System.
   * Maquetar la mesa central con `caja_carton_sin_armar_frontal`, la caja activa y los botones de embalaje (`boton_embalaje_espuma`, `boton_embalaje_film_termico`).
   * Configurar la banda izquierda (despacho) con `cartel_despacho` y 5 slots delimitados con `selector_cuadrado` (45% alpha).
   * Configurar la banda derecha (revisión) con `cartel_revision` y el slot de descarte delimitado con `selector_cuadrado` (45% alpha).
3. **Fase 3: Sistema de Post-Its y HUD Dinámico**
   * Rediseñar `OrderCardUI.cs` para renderizar el post-it amarillo/fucsia con su tachuela respectiva y la barra de tiempo decreciente horizontal.
   * Integrar la expansión en hover con nota grande respetando márgenes seguros y la fuente `Fredoka`.
   * Conectar `HUDManagerUI` para reflejar el frame del reloj y las pantallas de TV de pronóstico (`NEXT`).
4. **Fase 4: Verificación Integral en Runtime vía MCP**
   * Ejecutar `LogiPack -> Construir Todo con UI`.
   * Probar en Play Mode con MCP: avance del reloj, cambio de clima visible en la ventana, arrastre de ítems desde ambas estanterías, sellado de caja al entrar a despacho, descarte en revisión y despacho automático con acreditación monetaria.
