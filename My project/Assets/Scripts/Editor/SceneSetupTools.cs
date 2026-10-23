using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using LogiPack.Core;
using LogiPack.Gameplay;
using LogiPack.Logic;
using LogiPack.Managers;
using LogiPack.UI;

namespace LogiPack.Editor
{
    public static class SceneSetupTools
    {
        [MenuItem("LogiPack/Construir Escena de Gameplay")]
        public static void BuildGameplayScene()
        {
            string scenePath = "Assets/Scenes/GameplayScene.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 0. Servicios Persistentes Centrales
            MenuSetupTools.EnsureAppCoreServices();

            // Fuente oficial Fredoka
            Font fredokaBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Font/static/Fredoka-Bold.ttf");
            if (fredokaBold == null) fredokaBold = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Configurar Cámara (1920x1080 16:9 con orthographicSize = 5.4f)
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = 5.4f;
                mainCam.transform.position = new Vector3(0, 0, -10);
                mainCam.backgroundColor = new Color(0.10f, 0.12f, 0.15f);
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }

            // 2. EventSystem (Unity 6 New Input System)
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<InputSystemUIInputModule>();

            // 3. Fondo de la Mesa con Diagrama (1920x1080)
            Sprite deskSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mesa_trabajo_diagrama.png");
            if (deskSprite == null) deskSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mesa_trabajo.png");

            GameObject deskGO = new GameObject("Background_Desk");
            deskGO.transform.position = Vector3.zero;
            var deskSR = deskGO.AddComponent<SpriteRenderer>();
            deskSR.sprite = deskSprite;
            deskSR.sortingOrder = 0;

            // 4. Cargar Ítems desde Assets/Data/Items/
            var titanioItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_f1_t3_Perno_de_Titanio.asset");
            var tornilloItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_f1_t1_Tornillo_Estándar.asset");
            var litioItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_f2_t3_Batería_de_Litio.asset");
            var cableItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_f2_t1_Cable_Simple.asset");
            var cristalItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_f3_t3_Cristal_de_Laboratorio.asset");
            var vidrioItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_f3_t1_Vidrio_Plano.asset");
            var ignifugoItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_f4_t3_Tejido_Ignífugo.asset");
            var algodonItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_f4_t1_Tela_de_Algodón.asset");

            // Sprites de Ítems
            Sprite pernoTitanioSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/perno_titanio.png");
            Sprite tornilloSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/tornillo_estandar.png");
            Sprite bateriaLitioSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/bateria_litio.png");
            Sprite cableSimpleSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/cable_simple.png");
            Sprite cristalLabSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/cristal_laboratorio.png");
            Sprite vidrioPlanoSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/vidrio_plano.png");
            Sprite tejidoIgnifugoSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/tejido_ignifugo.png");
            Sprite telaAlgodonSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/tela_algodon.png");

            // 5. Crear Cuadrículas 2x2 de Ítems (Fuentes Infinitas)
            // Cuadrícula Izquierda (p y q)
            GameObject leftDrawerGO = new GameObject("Cuadricula_Items_Izquierda");
            CreateItemSourceSlot(leftDrawerGO.transform, "Item_p_Positivo", new Vector3(-4.88f, 1.35f, 0f), "p", false, titanioItem, pernoTitanioSpr);
            CreateItemSourceSlot(leftDrawerGO.transform, "Item_p_Negado", new Vector3(-3.81f, 1.35f, 0f), "p", true, tornilloItem, tornilloSpr);
            CreateItemSourceSlot(leftDrawerGO.transform, "Item_q_Positivo", new Vector3(-4.88f, 0.28f, 0f), "q", false, litioItem, bateriaLitioSpr);
            CreateItemSourceSlot(leftDrawerGO.transform, "Item_q_Negado", new Vector3(-3.81f, 0.28f, 0f), "q", true, cableItem, cableSimpleSpr);

            // Cuadrícula Derecha (s y r)
            GameObject rightDrawerGO = new GameObject("Cuadricula_Items_Derecha");
            CreateItemSourceSlot(rightDrawerGO.transform, "Item_s_Positivo", new Vector3(-1.07f, 1.35f, 0f), "s", false, cristalItem, cristalLabSpr);
            CreateItemSourceSlot(rightDrawerGO.transform, "Item_s_Negado", new Vector3(0.00f, 1.35f, 0f), "s", true, vidrioItem, vidrioPlanoSpr);
            CreateItemSourceSlot(rightDrawerGO.transform, "Item_r_Positivo", new Vector3(-1.07f, 0.28f, 0f), "r", false, ignifugoItem, tejidoIgnifugoSpr);
            CreateItemSourceSlot(rightDrawerGO.transform, "Item_r_Negado", new Vector3(0.00f, 0.28f, 0f), "r", true, algodonItem, telaAlgodonSpr);

            // 6. Prefab de Caja Proposicional
            Sprite boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/caja_carton_cerrada_isometrica.png");
            if (boxSprite == null) boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/caja.png");

            GameObject boxPrefabGO = new GameObject("PropositionalBoxPrefab");
            boxPrefabGO.transform.localScale = Vector3.one * 0.11f;
            var boxSR = boxPrefabGO.AddComponent<SpriteRenderer>();
            boxSR.sprite = boxSprite;
            boxSR.sortingOrder = 15;

            var boxCol = boxPrefabGO.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(9f, 9f);

            var propBox = boxPrefabGO.AddComponent<PropositionalBox>();
            propBox.DefaultBoxSprite = boxSprite;

            // Etiqueta flotante en la caja
            GameObject formulaCanvasGO = new GameObject("FormulaLabelCanvas");
            formulaCanvasGO.transform.SetParent(boxPrefabGO.transform, false);
            formulaCanvasGO.transform.localPosition = new Vector3(0f, -2.5f, 0f);

            var fCanvas = formulaCanvasGO.AddComponent<Canvas>();
            fCanvas.renderMode = RenderMode.WorldSpace;
            fCanvas.sortingOrder = 18;

            var fRT = formulaCanvasGO.GetComponent<RectTransform>();
            fRT.sizeDelta = new Vector2(350, 70);
            fRT.localScale = Vector3.one * 0.08f;

            var formulaOutline = formulaCanvasGO.AddComponent<Outline>();
            formulaOutline.effectColor = new Color(0.95f, 0.95f, 0.95f, 0.9f);
            formulaOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var fText = formulaCanvasGO.AddComponent<Text>();
            fText.alignment = TextAnchor.MiddleCenter;
            fText.fontSize = 28;
            fText.color = new Color(0.12f, 0.12f, 0.12f);
            fText.font = fredokaBold;
            propBox.FormulaLabel = fText;

            string boxPrefabPath = "Assets/Prefabs/PropositionalBoxPrefab.prefab";
            GameObject savedBoxPrefab = PrefabUtility.SaveAsPrefabAsset(boxPrefabGO, boxPrefabPath);
            GameObject.DestroyImmediate(boxPrefabGO);

            // 7. Zona de Armado 1x3 y Selector de Operador
            GameObject assemblyAreaGO = new GameObject("Zona_Armado_1x3");
            var workbench = assemblyAreaGO.AddComponent<AssemblyWorkbenchManager>();
            workbench.DefaultBoxSprite = boxSprite;
            workbench.PropositionalBoxPrefab = savedBoxPrefab;

            // Ranura 1 (Operando A)
            GameObject slot1GO = new GameObject("Ranura_Armado_1");
            slot1GO.transform.SetParent(assemblyAreaGO.transform, false);
            slot1GO.transform.position = new Vector3(-5.98f, -1.73f, 0f);
            var col1 = slot1GO.AddComponent<BoxCollider2D>();
            col1.size = new Vector2(1.5f, 1.5f);
            col1.isTrigger = true;
            workbench.Slot1_Transform = slot1GO.transform;

            // Ranura 2 (Operador)
            GameObject slot2GO = new GameObject("Ranura_Armado_Operador");
            slot2GO.transform.SetParent(assemblyAreaGO.transform, false);
            slot2GO.transform.position = new Vector3(-4.63f, -1.73f, 0f);
            var col2 = slot2GO.AddComponent<BoxCollider2D>();
            col2.size = new Vector2(1.5f, 1.5f);
            slot2GO.AddComponent<OperatorSlotTrigger>();
            workbench.Slot2_Operator_Transform = slot2GO.transform;

            // Canvas del símbolo del operador
            GameObject opCanvasGO = new GameObject("OperatorCanvas");
            opCanvasGO.transform.SetParent(slot2GO.transform, false);
            opCanvasGO.transform.localPosition = Vector3.zero;
            var opCanvas = opCanvasGO.AddComponent<Canvas>();
            opCanvas.renderMode = RenderMode.WorldSpace;
            opCanvas.sortingOrder = 14;
            var opRT = opCanvasGO.GetComponent<RectTransform>();
            opRT.sizeDelta = new Vector2(150, 150);
            opRT.localScale = Vector3.one * 0.015f;

            var opText = opCanvasGO.AddComponent<Text>();
            opText.text = "∧";
            opText.alignment = TextAnchor.MiddleCenter;
            opText.fontSize = 58;
            opText.color = Color.white;
            opText.font = fredokaBold;
            opText.raycastTarget = false;
            workbench.OperatorSymbolText = opText;

            // Cajón 1x4 Desplegable debajo de la Ranura 2
            Sprite drawer1x4Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/cajon_1x4.png");
            GameObject drawer1x4GO = new GameObject("Cajon_1x4_Operadores");
            drawer1x4GO.transform.SetParent(assemblyAreaGO.transform, false);
            drawer1x4GO.transform.position = new Vector3(-4.63f, -3.45f, 0f);
            drawer1x4GO.transform.localScale = Vector3.one * 1.15f;
            var drawerSR = drawer1x4GO.AddComponent<SpriteRenderer>();
            drawerSR.sprite = drawer1x4Sprite;
            drawerSR.sortingOrder = 40;

            // 4 Botones en el cajón 1x4 (∧, ∨, →, ↔) centrados con precisión pixel-perfect
            float[] btnOffsetsX = { -1.74f, -0.58f, 0.58f, 1.74f };
            PropositionalOperator[] ops = { PropositionalOperator.And, PropositionalOperator.Or, PropositionalOperator.Conditional, PropositionalOperator.Biconditional };
            string[] opSymbols = { "∧", "∨", "→", "↔" };

            for (int i = 0; i < 4; i++)
            {
                GameObject btnGO = new GameObject($"Btn_Op_{opSymbols[i]}");
                btnGO.transform.SetParent(drawer1x4GO.transform, false);
                btnGO.transform.localPosition = new Vector3(btnOffsetsX[i], 0f, 0f);

                var btnCol = btnGO.AddComponent<BoxCollider2D>();
                btnCol.size = new Vector2(1.05f, 1.05f);

                var drawerBtn = btnGO.AddComponent<OperatorDrawerButton>();
                drawerBtn.Operator = ops[i];
                drawerBtn.Workbench = workbench;

                // Texto del símbolo
                GameObject btnTextGO = new GameObject("SymbolText");
                btnTextGO.transform.SetParent(btnGO.transform, false);
                btnTextGO.transform.localPosition = Vector3.zero;
                var btnCanvas = btnTextGO.AddComponent<Canvas>();
                btnCanvas.renderMode = RenderMode.WorldSpace;
                btnCanvas.sortingOrder = 42;
                var btnRT = btnTextGO.GetComponent<RectTransform>();
                btnRT.sizeDelta = new Vector2(120, 120);
                btnRT.localScale = Vector3.one * 0.02f;

                var btnTextComp = btnTextGO.AddComponent<Text>();
                btnTextComp.text = opSymbols[i];
                btnTextComp.alignment = TextAnchor.MiddleCenter;
                btnTextComp.fontSize = 32;
                btnTextComp.color = Color.white;
                btnTextComp.font = fredokaBold;
                btnTextComp.raycastTarget = false;
            }
            drawer1x4GO.SetActive(false);
            workbench.OperatorDrawer1x4 = drawer1x4GO;

            // Ranura 3 (Operando B)
            GameObject slot3GO = new GameObject("Ranura_Armado_3");
            slot3GO.transform.SetParent(assemblyAreaGO.transform, false);
            slot3GO.transform.position = new Vector3(-3.30f, -1.73f, 0f);
            var col3 = slot3GO.AddComponent<BoxCollider2D>();
            col3.size = new Vector2(1.5f, 1.5f);
            col3.isTrigger = true;
            workbench.Slot3_Transform = slot3GO.transform;

            // Ranura de Resultado (al lado de la flecha)
            GameObject resultSlotGO = new GameObject("Ranura_Resultado");
            resultSlotGO.transform.SetParent(assemblyAreaGO.transform, false);
            resultSlotGO.transform.position = new Vector3(-0.10f, -1.73f, 0f);
            workbench.ResultSlot_Transform = resultSlotGO.transform;

            // 8. Zonas de Despacho y Revisión
            GameObject despachoGO = new GameObject("Zona_Despacho");
            despachoGO.transform.position = new Vector3(2.37f, 1.12f, 0f);
            var dCol = despachoGO.AddComponent<BoxCollider2D>();
            dCol.size = new Vector2(1.6f, 1.6f);
            dCol.isTrigger = true;

            GameObject revisionGO = new GameObject("Zona_Revision");
            revisionGO.tag = "TrashBin";
            revisionGO.transform.position = new Vector3(2.37f, -0.94f, 0f);
            var rCol = revisionGO.AddComponent<BoxCollider2D>();
            rCol.size = new Vector2(1.6f, 1.6f);
            rCol.isTrigger = true;

            // 9. Managers Lógicos
            GameObject managersGO = new GameObject("[Managers]");
            var orderManager = managersGO.AddComponent<PropositionalOrderManager>();
            var economyManager = managersGO.AddComponent<EconomyManager>();
            var envManager = managersGO.AddComponent<EnvironmentManager>();
            var floatingFX = managersGO.AddComponent<FloatingFeedbackFX>();
            orderManager.Economy = economyManager;

            // Asignar los 8 ítems a la lista de PropositionalOrderManager
            orderManager.AllItems.Clear();
            if (titanioItem != null) orderManager.AllItems.Add(titanioItem);
            if (tornilloItem != null) orderManager.AllItems.Add(tornilloItem);
            if (litioItem != null) orderManager.AllItems.Add(litioItem);
            if (cableItem != null) orderManager.AllItems.Add(cableItem);
            if (cristalItem != null) orderManager.AllItems.Add(cristalItem);
            if (vidrioItem != null) orderManager.AllItems.Add(vidrioItem);
            if (ignifugoItem != null) orderManager.AllItems.Add(ignifugoItem);
            if (algodonItem != null) orderManager.AllItems.Add(algodonItem);

            // 10. Canvas UI Principal (1920x1080)
            GameObject canvasGO = new GameObject("GameplayCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // HUD Superior Izquierdo: Reloj LED y Dinero
            Sprite[] clockFrames = new Sprite[17];
            for (int h = 6; h <= 22; h++)
            {
                string framePath = $"Assets/Art/Maps_Assets/reloj_individual_frames/reloj_{h:D2}_00.png";
                clockFrames[h - 6] = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            }
            Sprite moneyPlateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/marco_dinero_plateado.png");

            GameObject clockGO = new GameObject("HUD_Clock");
            clockGO.transform.SetParent(canvasGO.transform, false);
            var clockRT = clockGO.AddComponent<RectTransform>();
            clockRT.anchorMin = new Vector2(0f, 1f);
            clockRT.anchorMax = new Vector2(0f, 1f);
            clockRT.pivot = new Vector2(0f, 1f);
            clockRT.anchoredPosition = new Vector2(35, -25);
            clockRT.sizeDelta = new Vector2(176, 106);

            var clockImg = clockGO.AddComponent<Image>();
            if (clockFrames.Length > 0 && clockFrames[0] != null)
            {
                clockImg.sprite = clockFrames[0];
            }

            GameObject moneyGO = new GameObject("HUD_Money");
            moneyGO.transform.SetParent(canvasGO.transform, false);
            var moneyRT = moneyGO.AddComponent<RectTransform>();
            moneyRT.anchorMin = new Vector2(0f, 1f);
            moneyRT.anchorMax = new Vector2(0f, 1f);
            moneyRT.pivot = new Vector2(0f, 1f);
            moneyRT.anchoredPosition = new Vector2(35, -135);
            moneyRT.sizeDelta = new Vector2(176, 56);

            if (moneyPlateSprite != null)
            {
                var moneyBg = moneyGO.AddComponent<Image>();
                moneyBg.sprite = moneyPlateSprite;
            }

            GameObject moneyTextGO = new GameObject("MoneyText");
            moneyTextGO.transform.SetParent(moneyGO.transform, false);
            var moneyTextRT = moneyTextGO.AddComponent<RectTransform>();
            moneyTextRT.sizeDelta = new Vector2(160, 45);
            moneyTextRT.anchoredPosition = Vector2.zero;

            var moneyText = moneyTextGO.AddComponent<Text>();
            moneyText.text = "$0";
            moneyText.alignment = TextAnchor.MiddleCenter;
            moneyText.fontSize = 26;
            moneyText.font = fredokaBold;
            moneyText.color = new Color(0.95f, 0.85f, 0.2f); // Dorado

            var hudManager = canvasGO.AddComponent<HUDManagerUI>();
            hudManager.Environment = envManager;
            hudManager.Economy = economyManager;
            hudManager.ClockImage = clockImg;
            hudManager.ClockSprites = clockFrames;
            hudManager.MoneyText = moneyText;

            // Post-it Pedido Activo (Esquina Superior Derecha)
            Sprite postitSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/popstick_amarillo_grande.png");
            Sprite thumbtackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tachuela_roja.png");

            GameObject postitGO = new GameObject("Order_PostIt_Container");
            postitGO.transform.SetParent(canvasGO.transform, false);
            var postitRT = postitGO.AddComponent<RectTransform>();
            postitRT.anchorMin = new Vector2(1f, 1f);
            postitRT.anchorMax = new Vector2(1f, 1f);
            postitRT.pivot = new Vector2(1f, 1f);
            postitRT.anchoredPosition = new Vector2(-25, -25);
            postitRT.sizeDelta = new Vector2(340, 340);

            var postitImg = postitGO.AddComponent<Image>();
            postitImg.sprite = postitSprite;

            // Tachuela
            if (thumbtackSprite != null)
            {
                GameObject thumbtackGO = new GameObject("Thumbtack");
                thumbtackGO.transform.SetParent(postitGO.transform, false);
                var ttRT = thumbtackGO.AddComponent<RectTransform>();
                ttRT.anchorMin = new Vector2(0.5f, 1f);
                ttRT.anchorMax = new Vector2(0.5f, 1f);
                ttRT.anchoredPosition = new Vector2(0, -12);
                ttRT.sizeDelta = new Vector2(32, 32);
                var ttImg = thumbtackGO.AddComponent<Image>();
                ttImg.sprite = thumbtackSprite;
            }

            // Header Pedido (+6px -> 28pt)
            GameObject postitHeaderGO = new GameObject("OrderHeader");
            postitHeaderGO.transform.SetParent(postitGO.transform, false);
            var phRT = postitHeaderGO.AddComponent<RectTransform>();
            phRT.anchorMin = new Vector2(0.5f, 1f);
            phRT.anchorMax = new Vector2(0.5f, 1f);
            phRT.anchoredPosition = new Vector2(0, -45);
            phRT.sizeDelta = new Vector2(300, 38);
            var phText = postitHeaderGO.AddComponent<Text>();
            phText.alignment = TextAnchor.MiddleCenter;
            phText.fontSize = 28;
            phText.font = fredokaBold;
            phText.color = new Color(0.25f, 0.20f, 0.05f);

            // Fórmula FBF (+6px -> 38pt)
            GameObject postitFBFGO = new GameObject("OrderFBF");
            postitFBFGO.transform.SetParent(postitGO.transform, false);
            var fbfRT = postitFBFGO.AddComponent<RectTransform>();
            fbfRT.anchorMin = new Vector2(0.5f, 0.5f);
            fbfRT.anchorMax = new Vector2(0.5f, 0.5f);
            fbfRT.anchoredPosition = new Vector2(0, 15);
            fbfRT.sizeDelta = new Vector2(310, 70);
            var fbfText = postitFBFGO.AddComponent<Text>();
            fbfText.alignment = TextAnchor.MiddleCenter;
            fbfText.fontSize = 38;
            fbfText.font = fredokaBold;
            fbfText.color = new Color(0.12f, 0.12f, 0.12f); // Negro carbón

            // Lenguaje Natural (+6px -> 24pt)
            GameObject postitNatLangGO = new GameObject("OrderNaturalLanguage");
            postitNatLangGO.transform.SetParent(postitGO.transform, false);
            var nlRT = postitNatLangGO.AddComponent<RectTransform>();
            nlRT.anchorMin = new Vector2(0.5f, 0f);
            nlRT.anchorMax = new Vector2(0.5f, 0f);
            nlRT.anchoredPosition = new Vector2(0, 52);
            nlRT.sizeDelta = new Vector2(300, 90);
            var nlText = postitNatLangGO.AddComponent<Text>();
            nlText.alignment = TextAnchor.MiddleCenter;
            nlText.fontSize = 24;
            nlText.lineSpacing = 1.15f;
            nlText.font = fredokaBold;
            nlText.color = new Color(0.28f, 0.28f, 0.28f);

            orderManager.OrderPostitContainer = postitGO;
            orderManager.OrderHeaderTitleText = phText;
            orderManager.OrderFBFFormulaText = fbfText;
            orderManager.OrderNaturalLanguageText = nlText;

            // 11. Botón de Pausa (Lateral Derecho Central)
            Sprite woodSquareBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/CozyUI/Buttons/Square/SquareButton1_wood.png");
            Sprite homeIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/CozyUI/Icons/icono_casa.png");

            GameObject pauseBtnGO = new GameObject("Button_Pause");
            pauseBtnGO.transform.SetParent(canvasGO.transform, false);
            var pauseRT = pauseBtnGO.AddComponent<RectTransform>();
            pauseRT.anchorMin = new Vector2(1f, 0.5f);
            pauseRT.anchorMax = new Vector2(1f, 0.5f);
            pauseRT.anchoredPosition = new Vector2(-60, 50);
            pauseRT.sizeDelta = new Vector2(70, 70);

            var pauseImg = pauseBtnGO.AddComponent<Image>();
            pauseImg.sprite = woodSquareBtnSprite;
            var pauseBtn = pauseBtnGO.AddComponent<Button>();
            pauseBtn.targetGraphic = pauseImg;

            if (homeIconSprite != null)
            {
                GameObject homeIconGO = new GameObject("Icon_Home");
                homeIconGO.transform.SetParent(pauseBtnGO.transform, false);
                var homeIconRT = homeIconGO.AddComponent<RectTransform>();
                homeIconRT.sizeDelta = new Vector2(44, 44);
                homeIconRT.anchoredPosition = Vector2.zero;
                var homeIconImg = homeIconGO.AddComponent<Image>();
                homeIconImg.sprite = homeIconSprite;
                homeIconImg.preserveAspect = true;
            }

            // 12. Botón del Cuaderno de Teoría (Debajo de Pausa en lateral derecho)
            Sprite notebookClosedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites_sheets/cuaderno_cerrado.png");
            Sprite notebookOpenSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites_sheets/cuaderno_abierto.png");

            Sprite[] abrirFrames = new Sprite[4];
            for (int f = 1; f <= 4; f++)
            {
                abrirFrames[f - 1] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Sprites_sheets/individual_frames/abrir_paso_{f}.png");
            }

            GameObject notebookBtnGO = new GameObject("Button_NotebookGuide");
            notebookBtnGO.transform.SetParent(canvasGO.transform, false);
            var nbRT = notebookBtnGO.AddComponent<RectTransform>();
            nbRT.anchorMin = new Vector2(1f, 0.5f);
            nbRT.anchorMax = new Vector2(1f, 0.5f);
            nbRT.pivot = new Vector2(0.5f, 0.5f);
            nbRT.anchoredPosition = new Vector2(-60, -45);
            nbRT.sizeDelta = new Vector2(70, 70);

            var nbImg = notebookBtnGO.AddComponent<Image>();
            nbImg.sprite = notebookClosedSprite;
            nbImg.preserveAspect = true;
            var nbBtn = notebookBtnGO.AddComponent<Button>();
            nbBtn.targetGraphic = nbImg;

            // Modal Desacoplado: Cuaderno Abierto en Pantalla Completa (1450x945, 100% opaco)
            GameObject nbModalRootGO = new GameObject("Notebook_Modal_Root");
            nbModalRootGO.transform.SetParent(canvasGO.transform, false);
            var nbModalRT = nbModalRootGO.AddComponent<RectTransform>();
            nbModalRT.anchorMin = Vector2.zero;
            nbModalRT.anchorMax = Vector2.one;
            nbModalRT.sizeDelta = Vector2.zero;
            nbModalRT.anchoredPosition = Vector2.zero;

            // Overlay Oscuro de fondo (bloqueador de clics en gameplay)
            GameObject nbDarkOverlay = new GameObject("DarkOverlay");
            nbDarkOverlay.transform.SetParent(nbModalRootGO.transform, false);
            var nbDarkRT = nbDarkOverlay.AddComponent<RectTransform>();
            nbDarkRT.anchorMin = Vector2.zero;
            nbDarkRT.anchorMax = Vector2.one;
            nbDarkRT.sizeDelta = Vector2.zero;
            var nbDarkImg = nbDarkOverlay.AddComponent<Image>();
            nbDarkImg.color = new Color(0, 0, 0, 0.75f);
            var overlayCloseBtn = nbDarkOverlay.AddComponent<Button>();

            // Ventana Principal del Cuaderno Abierto (6x más grande que el icono original, 100% opaco)
            GameObject nbWindowGO = new GameObject("Notebook_Window");
            nbWindowGO.transform.SetParent(nbModalRootGO.transform, false);
            var nbWindowRT = nbWindowGO.AddComponent<RectTransform>();
            nbWindowRT.anchorMin = new Vector2(0.5f, 0.5f);
            nbWindowRT.anchorMax = new Vector2(0.5f, 0.5f);
            nbWindowRT.pivot = new Vector2(0.5f, 0.5f);
            nbWindowRT.anchoredPosition = Vector2.zero;
            nbWindowRT.sizeDelta = new Vector2(1450, 945);

            var nbWindowImg = nbWindowGO.AddComponent<Image>();
            nbWindowImg.sprite = notebookOpenSprite;
            nbWindowImg.color = Color.white; // 100% sólido, sin transparencia
            nbWindowImg.preserveAspect = true;

            // Página Izquierda: Lógica Simbólica
            GameObject leftPageTextGO = new GameObject("LeftPageText");
            leftPageTextGO.transform.SetParent(nbWindowGO.transform, false);
            var lpRT = leftPageTextGO.AddComponent<RectTransform>();
            lpRT.anchorMin = new Vector2(0.5f, 0.5f);
            lpRT.anchorMax = new Vector2(0.5f, 0.5f);
            lpRT.pivot = new Vector2(0.5f, 0.5f);
            lpRT.anchoredPosition = new Vector2(-360, 15);
            lpRT.sizeDelta = new Vector2(530, 680);
            var lpText = leftPageTextGO.AddComponent<Text>();
            lpText.font = fredokaBold;
            lpText.fontSize = 28;
            lpText.color = new Color(0.12f, 0.12f, 0.12f);
            lpText.lineSpacing = 1.2f;
            lpText.text = "<b>LÓGICA PROPOSICIONAL</b>\n\n" +
                          "<b>∧ : Conjunción (y)</b>\nVerdadero si ambos cumplen.\n\n" +
                          "<b>∨ : Disyunción (o)</b>\nVerdadero si al menos uno cumple.\n\n" +
                          "<b>→ : Condicional</b>\n'Si A entonces B'.\n\n" +
                          "<b>↔ : Bicondicional</b>\n'A si y solo si B'.\n\n" +
                          "<b>~ : Negación (no)</b>\nInvierte el valor de verdad.";

            // Página Derecha: Catálogo de Ítems
            GameObject rightPageTextGO = new GameObject("RightPageText");
            rightPageTextGO.transform.SetParent(nbWindowGO.transform, false);
            var rpRT = rightPageTextGO.AddComponent<RectTransform>();
            rpRT.anchorMin = new Vector2(0.5f, 0.5f);
            rpRT.anchorMax = new Vector2(0.5f, 0.5f);
            rpRT.pivot = new Vector2(0.5f, 0.5f);
            rpRT.anchoredPosition = new Vector2(360, 30);
            rpRT.sizeDelta = new Vector2(530, 640);
            var rpText = rightPageTextGO.AddComponent<Text>();
            rpText.font = fredokaBold;
            rpText.fontSize = 26;
            rpText.color = new Color(0.12f, 0.12f, 0.12f);
            rpText.lineSpacing = 1.2f;
            rpText.text = "<b>CATÁLOGO DE PROPOSICIONES</b>\n\n" +
                          "<b>p</b>: Perno Titanio  |  <b>~p</b>: Tornillo\n" +
                          "<b>q</b>: Batería Litio  |  <b>~q</b>: Cable\n" +
                          "<b>s</b>: Cristal Lab.   |  <b>~s</b>: Vidrio\n" +
                          "<b>r</b>: Ignífugo       |  <b>~r</b>: Algodón\n\n" +
                          "<i>Tip: Ranura 1 es término directo.\nRanura 3 es término negado (~).</i>";

            // Botón Cerrar Cuaderno
            Sprite yellowBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_amarillo_rectangular.png");
            GameObject closeNbBtnGO = new GameObject("CloseNotebookBtn");
            closeNbBtnGO.transform.SetParent(nbWindowGO.transform, false);
            var cnbRT = closeNbBtnGO.AddComponent<RectTransform>();
            cnbRT.anchorMin = new Vector2(0.5f, 0.5f);
            cnbRT.anchorMax = new Vector2(0.5f, 0.5f);
            cnbRT.pivot = new Vector2(0.5f, 0.5f);
            cnbRT.anchoredPosition = new Vector2(360, -370);
            cnbRT.sizeDelta = new Vector2(220, 56);
            var cnbImg = closeNbBtnGO.AddComponent<Image>();
            cnbImg.sprite = yellowBtnSprite;
            var cnbBtn = closeNbBtnGO.AddComponent<Button>();
            cnbBtn.targetGraphic = cnbImg;

            GameObject closeNbTextGO = new GameObject("Text");
            closeNbTextGO.transform.SetParent(closeNbBtnGO.transform, false);
            var cnbTextRT = closeNbTextGO.AddComponent<RectTransform>();
            cnbTextRT.sizeDelta = cnbRT.sizeDelta;
            var cnbText = closeNbTextGO.AddComponent<Text>();
            cnbText.text = "CERRAR";
            cnbText.alignment = TextAnchor.MiddleCenter;
            cnbText.fontSize = 24;
            cnbText.font = fredokaBold;
            cnbText.color = new Color(0.1f, 0.1f, 0.1f);

            var guideUI = notebookBtnGO.AddComponent<GameplayNotebookGuideUI>();
            guideUI.TriggerButton = nbBtn;
            guideUI.ModalRoot = nbModalRootGO;
            guideUI.NotebookWindowRect = nbWindowRT;
            guideUI.NotebookWindowImage = nbWindowImg;
            guideUI.CloseButton = cnbBtn;
            guideUI.OverlayCloseButton = overlayCloseBtn;
            guideUI.CuadernoAbierto = notebookOpenSprite;
            guideUI.AbrirFrames = abrirFrames;

            nbModalRootGO.SetActive(false);

            // 13. Modal de Opciones / Pausa de Gameplay
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/pantalla_fin_jornada.png");
            Sprite greenBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_verde_rectangular.png");
            var optionsUI = MenuSetupTools.BuildOptionsMenuHierarchy(canvasGO.transform, fredokaBold, cardSprite, yellowBtnSprite, greenBtnSprite, isGameplay: true);

            optionsUI.ScreenPauseButton = pauseBtn;
            pauseBtn.onClick.AddListener(optionsUI.OpenOptions);

            // 14. Modal de Fin de Jornada (22:00 PM)
            GameObject dayEndRootGO = new GameObject("DayEndSummaryManager");
            dayEndRootGO.transform.SetParent(canvasGO.transform, false);
            var deRT = dayEndRootGO.AddComponent<RectTransform>();
            deRT.anchorMin = Vector2.zero;
            deRT.anchorMax = Vector2.one;
            deRT.offsetMin = Vector2.zero;
            deRT.offsetMax = Vector2.zero;

            var dayEndUI = dayEndRootGO.AddComponent<DayEndSummaryUI>();
            dayEndUI.Environment = envManager;
            dayEndUI.Economy = economyManager;

            // Overlay oscuro
            GameObject deModalGO = new GameObject("DayEndOverlay");
            deModalGO.transform.SetParent(dayEndRootGO.transform, false);
            var deModalRT = deModalGO.AddComponent<RectTransform>();
            deModalRT.anchorMin = Vector2.zero;
            deModalRT.anchorMax = Vector2.one;
            deModalRT.offsetMin = Vector2.zero;
            deModalRT.offsetMax = Vector2.zero;

            var deOverlayImg = deModalGO.AddComponent<Image>();
            deOverlayImg.color = new Color(0, 0, 0, 0.75f);
            dayEndUI.OverlayBackground = deOverlayImg;
            dayEndUI.ContentPanel = deModalGO;

            // Tarjeta de reporte (pantalla_fin_jornada.png)
            GameObject cardPanelGO = new GameObject("SummaryCard");
            cardPanelGO.transform.SetParent(deModalGO.transform, false);
            var cpRT = cardPanelGO.AddComponent<RectTransform>();
            cpRT.anchorMin = new Vector2(0.5f, 0.5f);
            cpRT.anchorMax = new Vector2(0.5f, 0.5f);
            cpRT.pivot = new Vector2(0.5f, 0.5f);
            cpRT.sizeDelta = new Vector2(760, 580);

            var cpImg = cardPanelGO.AddComponent<Image>();
            cpImg.sprite = cardSprite;

            Color darkReportColor = new Color(0.12f, 0.12f, 0.12f);

            // 1. Título del Reporte
            dayEndUI.HeaderTitleText = CreateModalText(cardPanelGO.transform, "HeaderTitleText", new Vector2(0, 220), new Vector2(700, 60), 28, fredokaBold, darkReportColor, TextAnchor.MiddleCenter);

            // 2. Estrellas
            dayEndUI.StarsDisplayText = CreateModalText(cardPanelGO.transform, "StarsDisplayText", new Vector2(0, 155), new Vector2(600, 65), 52, fredokaBold, new Color(0.95f, 0.8f, 0.1f), TextAnchor.MiddleCenter);

            // 3. Título de Calificación
            dayEndUI.RatingTitleText = CreateModalText(cardPanelGO.transform, "RatingTitleText", new Vector2(0, 100), new Vector2(700, 40), 24, fredokaBold, darkReportColor, TextAnchor.MiddleCenter);

            // 4. Badge de Prestigio
            dayEndUI.PrestigeBadgeText = CreateModalText(cardPanelGO.transform, "PrestigeBadgeText", new Vector2(0, 60), new Vector2(700, 32), 18, fredokaBold, new Color(0.2f, 0.5f, 0.7f), TextAnchor.MiddleCenter);

            // 5. Desglose Financiero
            GameObject breakdownBoxGO = new GameObject("BreakdownBox");
            breakdownBoxGO.transform.SetParent(cardPanelGO.transform, false);
            var boxRT = breakdownBoxGO.AddComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 0.5f);
            boxRT.anchorMax = new Vector2(0.5f, 0.5f);
            boxRT.pivot = new Vector2(0.5f, 0.5f);
            boxRT.anchoredPosition = new Vector2(0, -45);
            boxRT.sizeDelta = new Vector2(680, 150);

            dayEndUI.FinancialBreakdownText = CreateModalText(breakdownBoxGO.transform, "FinancialBreakdownText", Vector2.zero, new Vector2(660, 140), 18, fredokaBold, darkReportColor, TextAnchor.MiddleLeft);

            // 6. Quotas Status
            dayEndUI.QuotasStatusText = CreateModalText(cardPanelGO.transform, "QuotasStatusText", new Vector2(0, -145), new Vector2(680, 45), 18, fredokaBold, new Color(0.3f, 0.3f, 0.3f), TextAnchor.MiddleCenter);

            // 7. Botón Siguiente Turno (Verde)
            GameObject nextDayBtnGO = new GameObject("NextDayButton");
            nextDayBtnGO.transform.SetParent(cardPanelGO.transform, false);
            var deBtnRT = nextDayBtnGO.AddComponent<RectTransform>();
            deBtnRT.anchorMin = new Vector2(0.5f, 0.5f);
            deBtnRT.anchorMax = new Vector2(0.5f, 0.5f);
            deBtnRT.pivot = new Vector2(0.5f, 0.5f);
            deBtnRT.anchoredPosition = new Vector2(0, -215);
            deBtnRT.sizeDelta = new Vector2(340, 58);

            var nextBtnImg = nextDayBtnGO.AddComponent<Image>();
            nextBtnImg.sprite = greenBtnSprite;
            nextBtnImg.color = Color.white;

            var nextBtn = nextDayBtnGO.AddComponent<Button>();
            nextBtn.targetGraphic = nextBtnImg;
            dayEndUI.NextDayButton = nextBtn;

            GameObject btnTxtGO = new GameObject("Text");
            btnTxtGO.transform.SetParent(nextDayBtnGO.transform, false);
            var btnTxtRT = btnTxtGO.AddComponent<RectTransform>();
            btnTxtRT.anchorMin = Vector2.zero;
            btnTxtRT.anchorMax = Vector2.one;
            btnTxtRT.offsetMin = Vector2.zero;
            btnTxtRT.offsetMax = Vector2.zero;

            var btnTxt = btnTxtGO.AddComponent<Text>();
            btnTxt.font = fredokaBold;
            btnTxt.fontSize = 20;
            btnTxt.color = new Color(0.1f, 0.1f, 0.1f);
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.text = "VOLVER A INTENTAR";
            dayEndUI.NextDayButtonText = btnTxt;

            deOverlayImg.enabled = false;
            deModalGO.SetActive(false);

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[LogiPack] ¡Escena de Gameplay Proposicional 100% reconstruida e integrada!</b></color>");
        }

        private static Text CreateModalText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, Font font, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var txt = go.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            return txt;
        }

        private static void CreateItemSourceSlot(Transform parent, string name, Vector3 pos, string varName, bool isNeg, ItemData item, Sprite sprite)
        {
            GameObject slotGO = new GameObject(name);
            slotGO.transform.SetParent(parent, false);
            slotGO.transform.position = pos;

            var sr = slotGO.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : (item != null ? item.Icon : null);
            sr.sortingOrder = 5;

            // Escalar para que encaje limpiamente en el cajón (1024px -> ~1 unidad)
            slotGO.transform.localScale = Vector3.one * 0.09f;

            var col = slotGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(12f, 12f); // Al multiplicar por 0.09f da ~1.1 unidades reales

            var src = slotGO.AddComponent<PropositionalItemSource>();
            src.VariableName = varName;
            src.IsNegated = isNeg;
            src.Item = item;
            src.VisualRenderer = sr;
        }
    }
}
