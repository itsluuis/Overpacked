# Plan de Diseño: Menú de Inicio y Arquitectura de Modos de Juego (Overpacked)

**Fecha:** 20 de Octubre de 2026  
**Estado:** Validado con Brainstorming  
**Área:** Menú Principal, Animación Cinemática de UI, Arquitectura de Modos de Juego, Gameplay Loop  

---

## 1. Resumen de Entendimiento (Understanding Summary)
* **Objetivo:** Crear una nueva escena `MainMenuScene.unity` con fondo de escritorio (`mesa_menu_papeles.png`), dos hemisferios interactivos (derecho con logo y 3 botones amarillos; izquierdo con cuaderno cerrado que al pulsar "JUGAR" se abre y desplaza al centro al 80% de pantalla). En el cuaderno abierto se presentan los modos `"Tutorial"` (modo base actual) y `"Jornada infinita"` (desafío por rondas con cuotas crecientes, tiempo decreciente de 60s a 30s, racha x1.5 y marco plateado superior). Además, se incorpora la navegación de regreso al menú desde el cuaderno y desde las pantallas de fin de jornada/partida.
* **Propósito:** Ofrecer una portada inmersiva y pulida que canalice adecuadamente los dos modos de juego del proyecto académico de Lógica Simbólica, garantizando que el modo tutorial preserve las reglas de cátedra y el modo infinito brinde una experiencia arcade frenética.
* **Restricciones:** No mezclar las reglas de ambos modos, asegurar transiciones fluidas de UI a 1920×1080 sin tirones, y permitir cerrar el cuaderno con botón o clic exterior.

---

## 2. Supuestos y Requerimientos No-Funcionales
* **Resolución:** Cámara ortográfica a 1920×1080 (`orthographicSize = 5.4f`) coincidente con `mesa_menu_papeles.png` (PPU 100).
* **Animación:** Corrutina suave de ~0.55s que interpola posición, escala y cicla los frames de apertura (`abrir_paso_1..4` $\rightarrow$ `cuaderno_abierto.png`).
* **Persistencia de Modo:** `GameModeManager` estático que almacena el modo seleccionado antes de cargar `GameplayScene.unity`.
* **Rendimiento:** Cero instanciaciones dinámicas pesadas en runtime; reutilización de Canvas e imágenes serializadas.

---

## 3. Registro de Decisiones (Decision Log)

| Decisión | Alternativas Consideradas | Justificación |
|---|---|---|
| **Arquitectura de Escenas** | 1. Duplicar escenas de gameplay.<br>2. Una sola `GameplayScene.unity` con controlador de modo condicional. | La opción 2 evita duplicación de mantenimiento visual, prefabs y código. |
| **Apertura del Cuaderno** | 1. Mecanismo Animator de Unity.<br>2. Corrutina controlada en C# con `CanvasGroup` e interpolación matemática. | La opción 2 es precisa, permite enlazar eventos de clic exterior y no depende de estados frágiles del Animator. |
| **Cuota en Modo Infinito** | 1. Texto genérico en esquina.<br>2. Marco superior dedicado con el sprite `marco_dinero_plateado.png`. | La opción 2 aprovecha el asset importado y destaca claramente el objetivo de la ronda en el HUD. |
| **Cierre del Cuaderno** | 1. Solo botón.<br>2. Solo clic exterior.<br>3. Combinación de botón "Volver" en hoja izquierda y clic en la mesa de madera. | La opción 3 ofrece máxima ergonomía y accesibilidad al usuario. |

---

## 4. Especificación Técnica de Componentes

### 4.1. `MainMenuController.cs` (`LogiPack.UI.Menu`)
* Gestiona el `CanvasGroup` del hemisferio derecho (Logo + Botones "JUGAR", "OPCIONES", "SALIR").
* Al hacer clic en "JUGAR", desvanece el hemisferio derecho (`alpha = 0`) y le ordena a `CuadernoMenuController` abrir el cuaderno.
* Al cerrar el cuaderno, desvanece de vuelta el hemisferio derecho (`alpha = 1`).
* Botón "OPCIONES": Despliega un panel modal provisional de ajustes.
* Botón "SALIR": Cierra la aplicación.

### 4.2. `CuadernoMenuController.cs` (`LogiPack.UI.Menu`)
* Referencia a los sprites de apertura:
  * `Sprite CuadernoCerrado`
  * `Sprite[] AbrirFrames` (4 frames)
  * `Sprite CuadernoAbierto`
* Estado de posición y escala:
  * Reposo: En hemisferio izquierdo (`anchoredPosition = (-480, -20)`), escala `0.45f`.
  * Abierto: En centro `(0, 0)`, escala `0.85f` (ocupando ~80% de la pantalla).
* Contenido de páginas:
  * **Hoja Izquierda:** Título "Modos de juego", botón "Tutorial" (seleccionado por defecto), botón "Jornada infinita", botón "Volver".
  * **Hoja Derecha:** Encabezado del modo, descripción oficial y botón verde rectangular "Empecemos" (`boton_verde_rectangular.png`).
* Textos oficiales:
  * **Tutorial:**  
    *“Tu primer turno, aquí te presentamos el almacén y todos sus productos, cómo embalar, cómo entregar, cómo tu trabajo cambia a lo largo del día y cuándo terminas.”*
  * **Jornada infinita:**  
    *“Con una cuota y tiempo límite para alcanzarla, será mejor ponerse serios; alcanza la cuota esperada a tiempo para avanzar de ronda a una más difícil hasta el infinito. ¿Serás capaz de poder manejar todo ese trabajo tú solo?”*
* Botón "Empecemos": Asigna `GameModeManager.ActiveMode` y ejecuta `SceneManager.LoadScene("GameplayScene")`.

### 4.3. `GameModeManager.cs` (`LogiPack.Core`)
```csharp
namespace LogiPack.Core
{
    public enum GameSessionMode
    {
        Tutorial,
        Infinite
    }

    public static class GameModeManager
    {
        public static GameSessionMode ActiveMode = GameSessionMode.Tutorial;
    }
}
```

### 4.4. `InfiniteModeController.cs` (`LogiPack.Gameplay`)
* Se ejecuta en `GameplayScene.unity` solo cuando `GameModeManager.ActiveMode == GameSessionMode.Infinite`.
* Activa el marco superior `marco_dinero_plateado.png` mostrando `Ronda N • Cuota: $Actual / $Meta`.
* Temporizador por ronda: Inicia en 60s, disminuye cada ronda progresivamente hasta un límite de 30s (tardando mínimo 4 rondas completas).
* Cuota: Comienza en $500 y escala porcentualmente (~15-20% por ronda).
* Racha (Streak): Cada despacho exitoso sin descartar ni fallar otorga un multiplicador de x1.5 al dinero obtenido.
* Si el dinero de la ronda alcanza la cuota antes de expirar el tiempo:
  * Pasa a la siguiente ronda.
  * Reinicia el temporizador al tiempo correspondiente a la nueva ronda.
  * Reinicia el monto de la ronda a $0 (para medir la nueva cuota).
  * Limpia los pedidos activos y cajas, y genera 5 órdenes continuas de inmediato.
* Si el tiempo expira sin alcanzar la cuota:
  * Activa la pantalla de fin de partida en `DayEndSummaryUI`.

### 4.5. `DayEndSummaryUI.cs` (`LogiPack.UI`)
* **En Modo Tutorial:** Añadir el botón **"Volver al Menú"** (`MainMenuScene.unity`).
* **En Modo Infinito:** Mostrar el resumen de fin de partida con:
  * Rondas / cuotas superadas.
  * Monto alcanzado por ronda.
  * Suma total de dinero acumulado de todas las rondas.
  * Botón **"Volver a Jugar"** (reinicia en Ronda 1).
  * Botón **"Volver al Menú"** (carga `MainMenuScene.unity`).

### 4.6. `MenuSetupTools.cs` (`LogiPack.Editor`)
* Herramienta de automatización `[MenuItem("LogiPack/Construir Menú Principal")]` que construye de forma reproducible `Assets/Scenes/MainMenuScene.unity` con todos los componentes y referencias asignadas.
