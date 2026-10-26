using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using LogiPack.Tutorial;
using LogiPack.UI;
using LogiPack.Checkout;

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

            // 3. Fondo de la Mesa Limpia (1920x1080)
            Sprite deskSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mesa_trabajo.png");
            if (deskSprite == null) deskSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mesa_trabajo_diagrama.png");

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

            // HUD de Cuota para Modo Jornada Infinita
            GameObject quotaPanelGO = new GameObject("HUD_InfiniteQuota");
            quotaPanelGO.transform.SetParent(canvasGO.transform, false);
            var qpRT = quotaPanelGO.AddComponent<RectTransform>();
            qpRT.anchorMin = new Vector2(0f, 1f);
            qpRT.anchorMax = new Vector2(0f, 1f);
            qpRT.pivot = new Vector2(0f, 1f);
            qpRT.anchoredPosition = new Vector2(35, -25);
            qpRT.sizeDelta = new Vector2(240, 180);

            // 1. Temporizador de Ronda
            GameObject qTimerGO = new GameObject("RoundTimerText");
            qTimerGO.transform.SetParent(quotaPanelGO.transform, false);
            var qtRT = qTimerGO.AddComponent<RectTransform>();
            qtRT.anchoredPosition = new Vector2(0, 0);
            qtRT.sizeDelta = new Vector2(240, 45);
            var qtText = qTimerGO.AddComponent<Text>();
            qtText.font = fredokaBold;
            qtText.fontSize = 32;
            qtText.color = Color.white;
            qtText.alignment = TextAnchor.MiddleLeft;
            qtText.text = "60s";
            var qtShadow = qTimerGO.AddComponent<Shadow>();
            qtShadow.effectColor = new Color(0, 0, 0, 0.8f);
            qtShadow.effectDistance = new Vector2(2, -2);

            // 2. Ronda Actual
            GameObject qRoundGO = new GameObject("QuotaRoundText");
            qRoundGO.transform.SetParent(quotaPanelGO.transform, false);
            var qrRT = qRoundGO.AddComponent<RectTransform>();
            qrRT.anchoredPosition = new Vector2(0, -42);
            qrRT.sizeDelta = new Vector2(240, 32);
            var qrText = qRoundGO.AddComponent<Text>();
            qrText.font = fredokaBold;
            qrText.fontSize = 24;
            qrText.color = new Color(0.95f, 0.85f, 0.2f);
            qrText.alignment = TextAnchor.MiddleLeft;
            qrText.text = "RONDA 1";
            var qrShadow = qRoundGO.AddComponent<Shadow>();
            qrShadow.effectColor = new Color(0, 0, 0, 0.8f);

            // 3. Marco Plateado con Cuota Acumulada / Meta
            GameObject qFrameGO = new GameObject("QuotaFrame");
            qFrameGO.transform.SetParent(quotaPanelGO.transform, false);
            var qfRT = qFrameGO.AddComponent<RectTransform>();
            qfRT.anchoredPosition = new Vector2(0, -80);
            qfRT.sizeDelta = new Vector2(220, 56);
            if (moneyPlateSprite != null)
            {
                var qfImg = qFrameGO.AddComponent<Image>();
                qfImg.sprite = moneyPlateSprite;
            }
            GameObject qAmountGO = new GameObject("QuotaAmountText");
            qAmountGO.transform.SetParent(qFrameGO.transform, false);
            var qaRT = qAmountGO.AddComponent<RectTransform>();
            qaRT.anchorMin = Vector2.zero; qaRT.anchorMax = Vector2.one;
            qaRT.offsetMin = Vector2.zero; qaRT.offsetMax = Vector2.zero;
            var qaText = qAmountGO.AddComponent<Text>();
            qaText.font = fredokaBold;
            qaText.fontSize = 22;
            qaText.color = Color.white;
            qaText.alignment = TextAnchor.MiddleCenter;
            qaText.text = "$0 / $500";

            // 4. Badge de Racha
            GameObject qStreakGO = new GameObject("StreakBadgeText");
            qStreakGO.transform.SetParent(quotaPanelGO.transform, false);
            var qsRT = qStreakGO.AddComponent<RectTransform>();
            qsRT.anchoredPosition = new Vector2(0, -145);
            qsRT.sizeDelta = new Vector2(240, 30);
            var qsText = qStreakGO.AddComponent<Text>();
            qsText.font = fredokaBold;
            qsText.fontSize = 18;
            qsText.color = new Color(1f, 0.6f, 0.1f);
            qsText.alignment = TextAnchor.MiddleLeft;
            qsText.text = "🔥 RACHA x1.5";
            qsText.gameObject.SetActive(false);

            quotaPanelGO.SetActive(false);

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
            phRT.sizeDelta = new Vector2(300, 42);
            var phText = postitHeaderGO.AddComponent<Text>();
            phText.alignment = TextAnchor.MiddleCenter;
            phText.fontSize = 28;
            phText.resizeTextForBestFit = true;
            phText.resizeTextMinSize = 14;
            phText.resizeTextMaxSize = 28;
            phText.font = fredokaBold;
            phText.color = new Color(0.25f, 0.20f, 0.05f);

            // Fórmula FBF (+6px -> 38pt)
            GameObject postitFBFGO = new GameObject("OrderFBF");
            postitFBFGO.transform.SetParent(postitGO.transform, false);
            var fbfRT = postitFBFGO.AddComponent<RectTransform>();
            fbfRT.anchorMin = new Vector2(0.5f, 0.5f);
            fbfRT.anchorMax = new Vector2(0.5f, 0.5f);
            fbfRT.anchoredPosition = new Vector2(0, 15);
            fbfRT.sizeDelta = new Vector2(310, 80);
            var fbfText = postitFBFGO.AddComponent<Text>();
            fbfText.alignment = TextAnchor.MiddleCenter;
            fbfText.fontSize = 38;
            fbfText.resizeTextForBestFit = true;
            fbfText.resizeTextMinSize = 14;
            fbfText.resizeTextMaxSize = 38;
            fbfText.font = fredokaBold;
            fbfText.color = new Color(0.12f, 0.12f, 0.12f); // Negro carbón

            // Lenguaje Natural (+6px -> 24pt)
            GameObject postitNatLangGO = new GameObject("OrderNaturalLanguage");
            postitNatLangGO.transform.SetParent(postitGO.transform, false);
            var nlRT = postitNatLangGO.AddComponent<RectTransform>();
            nlRT.anchorMin = new Vector2(0.5f, 0f);
            nlRT.anchorMax = new Vector2(0.5f, 0f);
            nlRT.anchoredPosition = new Vector2(0, 52);
            nlRT.sizeDelta = new Vector2(300, 95);
            var nlText = postitNatLangGO.AddComponent<Text>();
            nlText.alignment = TextAnchor.MiddleCenter;
            nlText.fontSize = 24;
            nlText.resizeTextForBestFit = true;
            nlText.resizeTextMinSize = 14;
            nlText.resizeTextMaxSize = 24;
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

            // 12. Modal Desacoplado: Cuaderno de Teoría y Tablas de Verdad (Multihopágina)
            Sprite yellowBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_amarillo_rectangular.png");
            var guideUI = BuildGameplayNotebookModal(canvasGO.transform, nbBtn, fredokaBold, yellowBtnSprite, notebookOpenSprite, abrirFrames);

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
            boxRT.anchoredPosition = new Vector2(0, -35);
            boxRT.sizeDelta = new Vector2(680, 180);

            dayEndUI.FinancialBreakdownText = CreateModalText(breakdownBoxGO.transform, "FinancialBreakdownText", Vector2.zero, new Vector2(660, 175), 18, fredokaBold, darkReportColor, TextAnchor.MiddleLeft);
            dayEndUI.FinancialBreakdownText.verticalOverflow = VerticalWrapMode.Overflow;

            // 6. Quotas Status
            dayEndUI.QuotasStatusText = CreateModalText(cardPanelGO.transform, "QuotasStatusText", new Vector2(0, -150), new Vector2(680, 45), 18, fredokaBold, new Color(0.3f, 0.3f, 0.3f), TextAnchor.MiddleCenter);

            // 7. Botón Volver a Intentar / Siguiente Turno (Verde - Izquierda)
            GameObject nextDayBtnGO = new GameObject("NextDayButton");
            nextDayBtnGO.transform.SetParent(cardPanelGO.transform, false);
            var deBtnRT = nextDayBtnGO.AddComponent<RectTransform>();
            deBtnRT.anchorMin = new Vector2(0.5f, 0.5f);
            deBtnRT.anchorMax = new Vector2(0.5f, 0.5f);
            deBtnRT.pivot = new Vector2(0.5f, 0.5f);
            deBtnRT.anchoredPosition = new Vector2(-150, -215);
            deBtnRT.sizeDelta = new Vector2(260, 58);

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

            // Botón Reintentar Modo Infinito (Verde - Misma posición izquierda)
            GameObject retryInfBtnGO = new GameObject("RetryInfiniteButton");
            retryInfBtnGO.transform.SetParent(cardPanelGO.transform, false);
            var rinfRT = retryInfBtnGO.AddComponent<RectTransform>();
            rinfRT.anchorMin = new Vector2(0.5f, 0.5f);
            rinfRT.anchorMax = new Vector2(0.5f, 0.5f);
            rinfRT.pivot = new Vector2(0.5f, 0.5f);
            rinfRT.anchoredPosition = new Vector2(-150, -215);
            rinfRT.sizeDelta = new Vector2(260, 58);

            var rinfImg = retryInfBtnGO.AddComponent<Image>();
            rinfImg.sprite = greenBtnSprite;
            var rinfBtn = retryInfBtnGO.AddComponent<Button>();
            rinfBtn.targetGraphic = rinfImg;
            dayEndUI.RetryInfiniteButton = rinfBtn;

            GameObject rinfTxtGO = new GameObject("Text");
            rinfTxtGO.transform.SetParent(retryInfBtnGO.transform, false);
            var rinfTxtRT = rinfTxtGO.AddComponent<RectTransform>();
            rinfTxtRT.anchorMin = Vector2.zero; rinfTxtRT.anchorMax = Vector2.one;
            rinfTxtRT.offsetMin = Vector2.zero; rinfTxtRT.offsetMax = Vector2.zero;
            var rinfTxt = rinfTxtGO.AddComponent<Text>();
            rinfTxt.font = fredokaBold;
            rinfTxt.fontSize = 20;
            rinfTxt.color = new Color(0.1f, 0.1f, 0.1f);
            rinfTxt.alignment = TextAnchor.MiddleCenter;
            rinfTxt.text = "VOLVER A INTENTAR";
            retryInfBtnGO.SetActive(false);

            // Botón Volver al Menú Principal (Amarillo - Derecha)
            GameObject menuBtnGO = new GameObject("ReturnToMenuButton");
            menuBtnGO.transform.SetParent(cardPanelGO.transform, false);
            var menuBtnRT = menuBtnGO.AddComponent<RectTransform>();
            menuBtnRT.anchorMin = new Vector2(0.5f, 0.5f);
            menuBtnRT.anchorMax = new Vector2(0.5f, 0.5f);
            menuBtnRT.pivot = new Vector2(0.5f, 0.5f);
            menuBtnRT.anchoredPosition = new Vector2(150, -215);
            menuBtnRT.sizeDelta = new Vector2(260, 58);

            var menuBtnImg = menuBtnGO.AddComponent<Image>();
            menuBtnImg.sprite = yellowBtnSprite;
            var menuBtn = menuBtnGO.AddComponent<Button>();
            menuBtn.targetGraphic = menuBtnImg;
            dayEndUI.ReturnToMenuButton = menuBtn;

            GameObject menuTxtGO = new GameObject("Text");
            menuTxtGO.transform.SetParent(menuBtnGO.transform, false);
            var menuTxtRT = menuTxtGO.AddComponent<RectTransform>();
            menuTxtRT.anchorMin = Vector2.zero; menuTxtRT.anchorMax = Vector2.one;
            menuTxtRT.offsetMin = Vector2.zero; menuTxtRT.offsetMax = Vector2.zero;
            var menuTxt = menuTxtGO.AddComponent<Text>();
            menuTxt.font = fredokaBold;
            menuTxt.fontSize = 20;
            menuTxt.color = new Color(0.1f, 0.1f, 0.1f);
            menuTxt.alignment = TextAnchor.MiddleCenter;
            menuTxt.text = "VOLVER AL MENÚ";

            // Wire listeners en runtime
            nextBtn.onClick.AddListener(dayEndUI.OnClickNextDay);
            rinfBtn.onClick.AddListener(dayEndUI.OnClickRetryInfinite);
            menuBtn.onClick.AddListener(dayEndUI.OnClickReturnToMenu);

            // Controlador de Modo Jornada Infinita
            var infiniteCtrl = managersGO.AddComponent<InfiniteModeController>();
            infiniteCtrl.Economy = economyManager;
            infiniteCtrl.PropositionalOrders = orderManager;
            infiniteCtrl.SummaryUI = dayEndUI;
            infiniteCtrl.QuotaPanel = quotaPanelGO;
            infiniteCtrl.QuotaRoundText = qrText;
            infiniteCtrl.QuotaAmountText = qaText;
            infiniteCtrl.RoundTimerText = qtText;
            infiniteCtrl.StreakBadgeText = qsText;

            deOverlayImg.enabled = false;
            deModalGO.SetActive(false);

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[LogiPack] ¡Escena de Gameplay Proposicional 100% reconstruida e integrada!</b></color>");
        }

        [MenuItem("LogiPack/Construir Escena de Tutorial")]
        public static void BuildTutorialScene()
        {
            string scenePath = "Assets/Scenes/TutorialScene.unity";
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

            // 3. Fondo de la Mesa Limpia (1920x1080)
            Sprite deskSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mesa_trabajo.png");
            if (deskSprite == null) deskSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mesa_trabajo_diagrama.png");

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

            // 5. Crear Cuadrículas 2x2 de Ítems
            GameObject leftDrawerGO = new GameObject("Cuadricula_Items_Izquierda");
            var itemP = CreateItemSourceSlot(leftDrawerGO.transform, "Item_p_Positivo", new Vector3(-4.88f, 1.35f, 0f), "p", false, titanioItem, pernoTitanioSpr);
            var itemNotP = CreateItemSourceSlot(leftDrawerGO.transform, "Item_p_Negado", new Vector3(-3.81f, 1.35f, 0f), "p", true, tornilloItem, tornilloSpr);
            var itemQ = CreateItemSourceSlot(leftDrawerGO.transform, "Item_q_Positivo", new Vector3(-4.88f, 0.28f, 0f), "q", false, litioItem, bateriaLitioSpr);
            var itemNotQ = CreateItemSourceSlot(leftDrawerGO.transform, "Item_q_Negado", new Vector3(-3.81f, 0.28f, 0f), "q", true, cableItem, cableSimpleSpr);

            GameObject rightDrawerGO = new GameObject("Cuadricula_Items_Derecha");
            var itemS = CreateItemSourceSlot(rightDrawerGO.transform, "Item_s_Positivo", new Vector3(-1.07f, 1.35f, 0f), "s", false, cristalItem, cristalLabSpr);
            var itemNotS = CreateItemSourceSlot(rightDrawerGO.transform, "Item_s_Negado", new Vector3(0.00f, 1.35f, 0f), "s", true, vidrioItem, vidrioPlanoSpr);
            var itemR = CreateItemSourceSlot(rightDrawerGO.transform, "Item_r_Positivo", new Vector3(-1.07f, 0.28f, 0f), "r", false, ignifugoItem, tejidoIgnifugoSpr);
            var itemNotR = CreateItemSourceSlot(rightDrawerGO.transform, "Item_r_Negado", new Vector3(0.00f, 0.28f, 0f), "r", true, algodonItem, telaAlgodonSpr);

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

            GameObject labelCanvasGO = new GameObject("FormulaLabelCanvas");
            labelCanvasGO.transform.SetParent(boxPrefabGO.transform, false);
            labelCanvasGO.transform.localPosition = new Vector3(0f, -2.5f, 0f);

            var lCanvas = labelCanvasGO.AddComponent<Canvas>();
            lCanvas.renderMode = RenderMode.WorldSpace;
            lCanvas.sortingOrder = 19;

            var lrt = labelCanvasGO.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(350, 70);
            lrt.localScale = Vector3.one * 0.08f;

            var labelText = labelCanvasGO.AddComponent<Text>();
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontSize = 28;
            labelText.font = fredokaBold;
            labelText.color = new Color(0.12f, 0.12f, 0.12f);
            propBox.FormulaLabel = labelText;

            boxPrefabGO.SetActive(false);

            // 7. Zona de Armado 1x3 y Selector de Operadores
            GameObject workbenchGO = new GameObject("AssemblyWorkbench");
            var workbench = workbenchGO.AddComponent<AssemblyWorkbenchManager>();
            workbench.DefaultBoxSprite = boxSprite;
            workbench.PropositionalBoxPrefab = boxPrefabGO;

            GameObject slot1GO = new GameObject("Ranura_Armado_1");
            slot1GO.transform.position = new Vector3(-5.98f, -1.73f, 0f);
            var col1 = slot1GO.AddComponent<BoxCollider2D>();
            col1.size = new Vector2(1.2f, 1.2f);
            workbench.Slot1_Transform = slot1GO.transform;

            GameObject slot2GO = new GameObject("Ranura_Armado_2_Operador");
            slot2GO.transform.position = new Vector3(-4.63f, -1.73f, 0f);
            var col2 = slot2GO.AddComponent<BoxCollider2D>();
            col2.size = new Vector2(1.2f, 1.2f);
            var trigger2 = slot2GO.AddComponent<OperatorSlotTrigger>();
            trigger2.Workbench = workbench;
            workbench.Slot2_Operator_Transform = slot2GO.transform;

            GameObject opTextGO = new GameObject("OperatorSymbol");
            opTextGO.transform.SetParent(slot2GO.transform, false);
            opTextGO.transform.localPosition = Vector3.zero;
            var opCanvas = opTextGO.AddComponent<Canvas>();
            opCanvas.renderMode = RenderMode.WorldSpace;
            opCanvas.sortingOrder = 12;
            var opTextRT = opTextGO.GetComponent<RectTransform>();
            opTextRT.sizeDelta = new Vector2(100, 100);
            opTextRT.localScale = Vector3.one * 0.015f;
            var opText = opTextGO.AddComponent<Text>();
            opText.font = fredokaBold;
            opText.fontSize = 42;
            opText.alignment = TextAnchor.MiddleCenter;
            opText.color = Color.white;
            opText.text = "∧";
            workbench.OperatorSymbolText = opText;

            GameObject slot3GO = new GameObject("Ranura_Armado_3");
            slot3GO.transform.position = new Vector3(-3.30f, -1.73f, 0f);
            var col3 = slot3GO.AddComponent<BoxCollider2D>();
            col3.size = new Vector2(1.2f, 1.2f);
            workbench.Slot3_Transform = slot3GO.transform;

            GameObject resultSlotGO = new GameObject("Ranura_Resultado_Caja");
            resultSlotGO.transform.position = new Vector3(-0.10f, -1.73f, 0f);
            var colRes = resultSlotGO.AddComponent<BoxCollider2D>();
            colRes.size = new Vector2(1.3f, 1.3f);
            workbench.ResultSlot_Transform = resultSlotGO.transform;

            // Cajón 1x4 de Selección de Operadores
            Sprite drawerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/cajon_1x4.png");
            GameObject drawerGO = new GameObject("Cajon_Operadores_1x4");
            drawerGO.transform.position = new Vector3(-4.63f, -3.45f, 0f);
            drawerGO.transform.localScale = Vector3.one * 1.15f;
            var drawerSR = drawerGO.AddComponent<SpriteRenderer>();
            drawerSR.sprite = drawerSprite;
            drawerSR.sortingOrder = 20;

            workbench.OperatorDrawer1x4 = drawerGO;

            PropositionalOperator[] ops = new PropositionalOperator[]
            {
                PropositionalOperator.And,
                PropositionalOperator.Or,
                PropositionalOperator.Conditional,
                PropositionalOperator.Biconditional
            };

            float[] opCentersX = new float[] { -1.74f, -0.58f, 0.58f, 1.74f };
            for (int i = 0; i < 4; i++)
            {
                GameObject btnOpGO = new GameObject($"Btn_Operator_{ops[i]}");
                btnOpGO.transform.SetParent(drawerGO.transform, false);
                btnOpGO.transform.localPosition = new Vector3(opCentersX[i], -0.11f, 0f);

                var bcol = btnOpGO.AddComponent<BoxCollider2D>();
                bcol.size = new Vector2(1.1f, 1.1f);

                var opBtn = btnOpGO.AddComponent<OperatorDrawerButton>();
                opBtn.Operator = ops[i];
                opBtn.Workbench = workbench;

                GameObject symGO = new GameObject("SymbolText");
                symGO.transform.SetParent(btnOpGO.transform, false);
                symGO.transform.localPosition = Vector3.zero;

                var symCanvas = symGO.AddComponent<Canvas>();
                symCanvas.renderMode = RenderMode.WorldSpace;
                symCanvas.sortingOrder = 22;

                var symRT = symGO.GetComponent<RectTransform>();
                symRT.sizeDelta = new Vector2(100, 100);
                symRT.localScale = Vector3.one * 0.012f;

                var symText = symGO.AddComponent<Text>();
                symText.font = fredokaBold;
                symText.fontSize = 32;
                symText.alignment = TextAnchor.MiddleCenter;
                symText.color = Color.white;
                symText.text = ops[i].GetSymbol();
            }

            drawerGO.SetActive(false);

            // 8. Zonas Nativas de Despacho y Revisión (Recuadros impresos en mesa_trabajo.png)
            GameObject dispatchGO = new GameObject("Zona_Despacho");
            dispatchGO.transform.position = new Vector3(2.37f, 1.12f, 0f);
            var dispCol = dispatchGO.AddComponent<BoxCollider2D>();
            dispCol.size = new Vector2(1.6f, 1.6f);
            dispCol.isTrigger = true;

            GameObject revisionGO = new GameObject("Zona_Revision");
            revisionGO.tag = "TrashBin";
            revisionGO.transform.position = new Vector3(2.37f, -0.94f, 0f);
            var revCol = revisionGO.AddComponent<BoxCollider2D>();
            revCol.size = new Vector2(1.6f, 1.6f);
            revCol.isTrigger = true;

            // 9. Managers de Lógica, Economía y Ambiente
            GameObject logicMgrGO = new GameObject("LogicManagers");
            var economyManager = logicMgrGO.AddComponent<EconomyManager>();
            var envManager = logicMgrGO.AddComponent<EnvironmentManager>();
            envManager.IsDayPaused = true; // Congelado para tutorial

            // 10. Canvas Principal
            GameObject canvasGO = new GameObject("TutorialCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Reloj Digital LED (Congelado en 06:00)
            Sprite reloj0600 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/reloj_individual_frames/reloj_06_00.png");
            GameObject relojGO = new GameObject("RelojDigital_LED");
            relojGO.transform.SetParent(canvasGO.transform, false);
            var relojRT = relojGO.AddComponent<RectTransform>();
            relojRT.anchorMin = new Vector2(0f, 1f);
            relojRT.anchorMax = new Vector2(0f, 1f);
            relojRT.pivot = new Vector2(0f, 1f);
            relojRT.anchoredPosition = new Vector2(35, -25);
            relojRT.sizeDelta = new Vector2(165, 85);
            var relojImg = relojGO.AddComponent<Image>();
            relojImg.sprite = reloj0600;
            relojImg.preserveAspect = true;

            // Marco Dinero
            Sprite marcoDineroSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/marco_dinero_plateado.png");
            GameObject moneyGO = new GameObject("Marco_Dinero");
            moneyGO.transform.SetParent(canvasGO.transform, false);
            var moneyRT = moneyGO.AddComponent<RectTransform>();
            moneyRT.anchorMin = new Vector2(0f, 1f);
            moneyRT.anchorMax = new Vector2(0f, 1f);
            moneyRT.pivot = new Vector2(0f, 1f);
            moneyRT.anchoredPosition = new Vector2(35, -135);
            moneyRT.sizeDelta = new Vector2(165, 65);
            var moneyImg = moneyGO.AddComponent<Image>();
            moneyImg.sprite = marcoDineroSpr;
            moneyImg.preserveAspect = true;

            GameObject moneyTxtGO = new GameObject("MoneyText");
            moneyTxtGO.transform.SetParent(moneyGO.transform, false);
            var mtxtRT = moneyTxtGO.AddComponent<RectTransform>();
            mtxtRT.anchorMin = Vector2.zero;
            mtxtRT.anchorMax = Vector2.one;
            mtxtRT.offsetMin = new Vector2(35, 5);
            mtxtRT.offsetMax = new Vector2(-10, -5);
            var mtxt = moneyTxtGO.AddComponent<Text>();
            mtxt.font = fredokaBold;
            mtxt.fontSize = 24;
            mtxt.color = new Color(0.1f, 0.1f, 0.1f);
            mtxt.alignment = TextAnchor.MiddleCenter;
            mtxt.text = "$0";

            var hudUI = canvasGO.AddComponent<HUDManagerUI>();
            hudUI.Environment = envManager;
            hudUI.Economy = economyManager;
            hudUI.MoneyText = mtxt;
            hudUI.ClockImage = relojImg;

            // 11. Post-it de Pedido Proposicional (Esquina Superior Derecha)
            Sprite postitSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/popstick_amarillo_grande.png");
            GameObject postitGO = new GameObject("OrderPostit_Container");
            postitGO.transform.SetParent(canvasGO.transform, false);
            var postitRT = postitGO.AddComponent<RectTransform>();
            postitRT.anchorMin = new Vector2(1f, 1f);
            postitRT.anchorMax = new Vector2(1f, 1f);
            postitRT.pivot = new Vector2(1f, 1f);
            postitRT.anchoredPosition = new Vector2(-25, -25);
            postitRT.sizeDelta = new Vector2(340, 340);
            var postitImg = postitGO.AddComponent<Image>();
            postitImg.sprite = postitSpr;

            Color darkInk = new Color(0.12f, 0.12f, 0.12f);
            var headerTxt = CreateModalText(postitGO.transform, "HeaderTitle", new Vector2(0, 115), new Vector2(310, 40), 28, fredokaBold, darkInk, TextAnchor.MiddleCenter);
            var formulaTxt = CreateModalText(postitGO.transform, "FormulaFBF", new Vector2(0, 35), new Vector2(310, 70), 38, fredokaBold, darkInk, TextAnchor.MiddleCenter);
            var naturalTxt = CreateModalText(postitGO.transform, "NaturalLang", new Vector2(0, -65), new Vector2(300, 100), 24, fredokaBold, darkInk, TextAnchor.MiddleCenter);
            naturalTxt.lineSpacing = 1.15f;

            var propOrderMgr = logicMgrGO.AddComponent<PropositionalOrderManager>();
            propOrderMgr.AutoSpawnNextOrder = false;
            propOrderMgr.OrderPostitContainer = postitGO;
            propOrderMgr.OrderHeaderTitleText = headerTxt;
            propOrderMgr.OrderFBFFormulaText = formulaTxt;
            propOrderMgr.OrderNaturalLanguageText = naturalTxt;
            propOrderMgr.Economy = economyManager;
            propOrderMgr.AllItems = new List<ItemData> { titanioItem, tornilloItem, litioItem, cableItem, cristalItem, vidrioItem, ignifugoItem, algodonItem };

            // 12. Botones Laterales HUD: Pausa y Cuaderno de Teoría
            Sprite pauseWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/CozyUI/Buttons/Square/SquareButton1_wood.png");
            Sprite houseIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/icono_casa.png");

            GameObject pauseBtnGO = new GameObject("Button_Pause");
            pauseBtnGO.transform.SetParent(canvasGO.transform, false);
            var pauseRT = pauseBtnGO.AddComponent<RectTransform>();
            pauseRT.anchorMin = new Vector2(1f, 0.5f);
            pauseRT.anchorMax = new Vector2(1f, 0.5f);
            pauseRT.pivot = new Vector2(1f, 0.5f);
            pauseRT.anchoredPosition = new Vector2(-60, 50);
            pauseRT.sizeDelta = new Vector2(70, 70);
            var pauseImg = pauseBtnGO.AddComponent<Image>();
            pauseImg.sprite = pauseWoodSprite;
            var pauseBtn = pauseBtnGO.AddComponent<Button>();
            pauseBtn.targetGraphic = pauseImg;

            GameObject pauseIconGO = new GameObject("Icon");
            pauseIconGO.transform.SetParent(pauseBtnGO.transform, false);
            var pIconRT = pauseIconGO.AddComponent<RectTransform>();
            pIconRT.sizeDelta = new Vector2(45, 45);
            var pIconImg = pauseIconGO.AddComponent<Image>();
            pIconImg.sprite = houseIconSprite;
            pIconImg.raycastTarget = false;

            // Botón de Cuaderno de Teoría
            Sprite notebookClosedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites_sheets/cuaderno_cerrado.png");
            Sprite notebookOpenSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites_sheets/cuaderno_abierto.png");
            Sprite[] abrirFrames = new Sprite[4];
            for (int i = 1; i <= 4; i++)
            {
                abrirFrames[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Sprites_sheets/individual_frames/abrir_paso_{i}.png");
            }

            GameObject notebookBtnGO = new GameObject("Button_NotebookGuide");
            notebookBtnGO.transform.SetParent(canvasGO.transform, false);
            var nbBtnRT = notebookBtnGO.AddComponent<RectTransform>();
            nbBtnRT.anchorMin = new Vector2(1f, 0.5f);
            nbBtnRT.anchorMax = new Vector2(1f, 0.5f);
            nbBtnRT.pivot = new Vector2(1f, 0.5f);
            nbBtnRT.anchoredPosition = new Vector2(-60, -45);
            nbBtnRT.sizeDelta = new Vector2(70, 70);
            var nbImg = notebookBtnGO.AddComponent<Image>();
            nbImg.sprite = notebookClosedSprite;
            nbImg.preserveAspect = true;
            var nbBtn = notebookBtnGO.AddComponent<Button>();
            nbBtn.targetGraphic = nbImg;

            // 12. Modal Desacoplado: Cuaderno de Teoría y Tablas de Verdad (Multihopágina)
            Sprite yellowBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_amarillo_rectangular.png");
            var guideUI = BuildGameplayNotebookModal(canvasGO.transform, nbBtn, fredokaBold, yellowBtnSprite, notebookOpenSprite, abrirFrames);

            // 13. Modal de Opciones / Pausa de Gameplay
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/pantalla_fin_jornada.png");
            Sprite greenBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_verde_rectangular.png");
            var optionsUI = MenuSetupTools.BuildOptionsMenuHierarchy(canvasGO.transform, fredokaBold, cardSprite, yellowBtnSprite, greenBtnSprite, isGameplay: true);
            optionsUI.ScreenPauseButton = pauseBtn;
            pauseBtn.onClick.AddListener(optionsUI.OpenOptions);

            // 14. Modal de Fin de Jornada (Resumen Tutorial)
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
            dayEndUI.HeaderTitleText = CreateModalText(cardPanelGO.transform, "HeaderTitleText", new Vector2(0, 220), new Vector2(700, 60), 28, fredokaBold, darkReportColor, TextAnchor.MiddleCenter);
            dayEndUI.StarsDisplayText = CreateModalText(cardPanelGO.transform, "StarsDisplayText", new Vector2(0, 155), new Vector2(600, 65), 52, fredokaBold, new Color(0.95f, 0.8f, 0.1f), TextAnchor.MiddleCenter);
            dayEndUI.RatingTitleText = CreateModalText(cardPanelGO.transform, "RatingTitleText", new Vector2(0, 100), new Vector2(700, 40), 24, fredokaBold, darkReportColor, TextAnchor.MiddleCenter);
            dayEndUI.PrestigeBadgeText = CreateModalText(cardPanelGO.transform, "PrestigeBadgeText", new Vector2(0, 60), new Vector2(700, 32), 18, fredokaBold, new Color(0.2f, 0.5f, 0.7f), TextAnchor.MiddleCenter);

            GameObject breakdownBoxGO = new GameObject("BreakdownBox");
            breakdownBoxGO.transform.SetParent(cardPanelGO.transform, false);
            var boxRT = breakdownBoxGO.AddComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 0.5f);
            boxRT.anchorMax = new Vector2(0.5f, 0.5f);
            boxRT.pivot = new Vector2(0.5f, 0.5f);
            boxRT.anchoredPosition = new Vector2(0, -45);
            boxRT.sizeDelta = new Vector2(680, 150);

            dayEndUI.FinancialBreakdownText = CreateModalText(breakdownBoxGO.transform, "FinancialBreakdownText", Vector2.zero, new Vector2(660, 140), 18, fredokaBold, darkReportColor, TextAnchor.MiddleLeft);
            dayEndUI.QuotasStatusText = CreateModalText(cardPanelGO.transform, "QuotasStatusText", new Vector2(0, -145), new Vector2(680, 45), 18, fredokaBold, new Color(0.3f, 0.3f, 0.3f), TextAnchor.MiddleCenter);

            // Botón Repetir Tutorial (Verde)
            GameObject nextDayBtnGO = new GameObject("NextDayButton");
            nextDayBtnGO.transform.SetParent(cardPanelGO.transform, false);
            var deBtnRT = nextDayBtnGO.AddComponent<RectTransform>();
            deBtnRT.anchorMin = new Vector2(0.5f, 0.5f);
            deBtnRT.anchorMax = new Vector2(0.5f, 0.5f);
            deBtnRT.pivot = new Vector2(0.5f, 0.5f);
            deBtnRT.anchoredPosition = new Vector2(-150, -215);
            deBtnRT.sizeDelta = new Vector2(280, 58);
            var nextBtnImg = nextDayBtnGO.AddComponent<Image>();
            nextBtnImg.sprite = greenBtnSprite;
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
            btnTxt.text = "REPETIR TUTORIAL";
            dayEndUI.NextDayButtonText = btnTxt;

            // Botón Salir al Menú Principal (Amarillo)
            GameObject menuBtnGO = new GameObject("ReturnToMenuButton");
            menuBtnGO.transform.SetParent(cardPanelGO.transform, false);
            var menuBtnRT = menuBtnGO.AddComponent<RectTransform>();
            menuBtnRT.anchorMin = new Vector2(0.5f, 0.5f);
            menuBtnRT.anchorMax = new Vector2(0.5f, 0.5f);
            menuBtnRT.pivot = new Vector2(0.5f, 0.5f);
            menuBtnRT.anchoredPosition = new Vector2(150, -215);
            menuBtnRT.sizeDelta = new Vector2(280, 58);
            var menuBtnImg = menuBtnGO.AddComponent<Image>();
            menuBtnImg.sprite = yellowBtnSprite;
            var menuBtn = menuBtnGO.AddComponent<Button>();
            menuBtn.targetGraphic = menuBtnImg;
            dayEndUI.ReturnToMenuButton = menuBtn;

            GameObject menuTxtGO = new GameObject("Text");
            menuTxtGO.transform.SetParent(menuBtnGO.transform, false);
            var menuTxtRT = menuTxtGO.AddComponent<RectTransform>();
            menuTxtRT.anchorMin = Vector2.zero;
            menuTxtRT.anchorMax = Vector2.one;
            menuTxtRT.offsetMin = Vector2.zero;
            menuTxtRT.offsetMax = Vector2.zero;
            var menuTxt = menuTxtGO.AddComponent<Text>();
            menuTxt.font = fredokaBold;
            menuTxt.fontSize = 20;
            menuTxt.color = new Color(0.1f, 0.1f, 0.1f);
            menuTxt.alignment = TextAnchor.MiddleCenter;
            menuTxt.text = "MENÚ PRINCIPAL";

            deOverlayImg.enabled = false;
            deModalGO.SetActive(false);

            // ==========================================
            // 15. SISTEMA DE TUTORIAL DEDICADO
            // ==========================================

            // A) Bloqueador de Raycasts en Canvas
            GameObject blockerGO = new GameObject("TutorialInteractionBlocker");
            blockerGO.transform.SetParent(canvasGO.transform, false);
            var blockerRT = blockerGO.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            var blocker = blockerGO.AddComponent<TutorialInteractionBlocker>();

            // B) UI de Diálogos y Mochi Miles
            GameObject dialogueRootGO = new GameObject("TutorialDialogueUI");
            dialogueRootGO.transform.SetParent(canvasGO.transform, false);
            var dialogueRT = dialogueRootGO.AddComponent<RectTransform>();
            dialogueRT.anchorMin = Vector2.zero;
            dialogueRT.anchorMax = Vector2.one;
            dialogueRT.offsetMin = Vector2.zero;
            dialogueRT.offsetMax = Vector2.zero;

            var dialogueUI = dialogueRootGO.AddComponent<TutorialDialogueUI>();

            // Cargar Sprites de Mochi Miles
            Sprite mochiBrazos = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/NPC/mochi_miles_brazos_cruzados.png");
            Sprite mochiMostrando = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/NPC/mochi_miles_mostrando.png");
            Sprite mochiOjos = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/NPC/mochi_miles_cintura_ojos_cerrados.png");

            dialogueUI.MochiBrazosCruzados = mochiBrazos;
            dialogueUI.MochiMostrando = mochiMostrando;
            dialogueUI.MochiCinturaOjosCerrados = mochiOjos;

            // Sprites de Cajas de Diálogo
            dialogueUI.BoxPequeno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/cuadro_dialogo_pequeno.png");
            dialogueUI.BoxMediano = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/cuadro_dialogo_mediano.png");
            dialogueUI.BoxGrande = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/cuadro_dialogo_grande.png");

            // Mochi Portrait en Canvas (anclado al borde inferior)
            GameObject mochiImgGO = new GameObject("MochiPortrait");
            mochiImgGO.transform.SetParent(dialogueRootGO.transform, false);
            var mochiRT = mochiImgGO.AddComponent<RectTransform>();
            mochiRT.anchorMin = new Vector2(0f, 0f);
            mochiRT.anchorMax = new Vector2(0f, 0f);
            mochiRT.pivot = new Vector2(0.5f, 0f);
            mochiRT.anchoredPosition = new Vector2(230, 0); // Al ras del fondo visual
            mochiRT.sizeDelta = new Vector2(540, 740);

            var mochiImg = mochiImgGO.AddComponent<Image>();
            mochiImg.sprite = mochiBrazos;
            mochiImg.preserveAspect = true;
            dialogueUI.MochiImage = mochiImg;

            // Caja de Diálogo
            GameObject boxGO = new GameObject("DialogueBox");
            boxGO.transform.SetParent(dialogueRootGO.transform, false);
            var boxRT2 = boxGO.AddComponent<RectTransform>();
            boxRT2.anchorMin = new Vector2(0f, 0f);
            boxRT2.anchorMax = new Vector2(0f, 0f);
            boxRT2.pivot = new Vector2(0f, 0f);
            boxRT2.anchoredPosition = new Vector2(490, 40);
            boxRT2.sizeDelta = new Vector2(850, 340);

            var boxImg = boxGO.AddComponent<Image>();
            boxImg.sprite = dialogueUI.BoxGrande;
            dialogueUI.DialogueBoxImage = boxImg;

            // Texto de Diálogo
            GameObject dTxtGO = new GameObject("DialogueText");
            dTxtGO.transform.SetParent(boxGO.transform, false);
            var dTxtRT = dTxtGO.AddComponent<RectTransform>();
            dTxtRT.anchorMin = Vector2.zero;
            dTxtRT.anchorMax = Vector2.one;
            dTxtRT.offsetMin = new Vector2(50, 35);
            dTxtRT.offsetMax = new Vector2(-50, -35);

            var dTxt = dTxtGO.AddComponent<Text>();
            dTxt.font = fredokaBold;
            dTxt.fontSize = 26;
            dTxt.color = new Color(0.96f, 0.96f, 0.98f);
            dTxt.lineSpacing = 1.2f;
            var dOutline = dTxtGO.AddComponent<Outline>();
            dOutline.effectColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);
            dOutline.effectDistance = new Vector2(1.5f, -1.5f);
            dialogueUI.DialogueText = dTxt;

            // Flechita parpadeante de continuación
            GameObject arrowGO = new GameObject("ContinueArrow");
            arrowGO.transform.SetParent(boxGO.transform, false);
            var arrowRT = arrowGO.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1f, 0f);
            arrowRT.anchorMax = new Vector2(1f, 0f);
            arrowRT.pivot = new Vector2(1f, 0f);
            arrowRT.anchoredPosition = new Vector2(-30f, 25f);
            arrowRT.sizeDelta = new Vector2(40, 40);
            var arrowTxt = arrowGO.AddComponent<Text>();
            arrowTxt.font = fredokaBold;
            arrowTxt.fontSize = 28;
            arrowTxt.color = new Color(1f, 0.85f, 0.2f);
            arrowTxt.alignment = TextAnchor.MiddleCenter;
            arrowTxt.text = "▼";
            var aOutline = arrowGO.AddComponent<Outline>();
            aOutline.effectColor = new Color(0.1f, 0.08f, 0.02f, 0.95f);
            aOutline.effectDistance = new Vector2(1.5f, -1.5f);
            dialogueUI.ContinueArrowIndicator = arrowGO;
            arrowGO.SetActive(false);

            // C) Indicador Visual de Resaltado (World Space 2D)
            GameObject highlightGO = new GameObject("TutorialHighlightFX");
            var highlightFX = highlightGO.AddComponent<TutorialHighlightFX>();
            Sprite selectorSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/selector_cuadrado.png");
            highlightFX.ArrowSprite = selectorSpr;
            highlightFX.BounceAmplitude = 0f;

            // D) Director del Tutorial
            GameObject directorGO = new GameObject("TutorialDirector");
            var director = directorGO.AddComponent<TutorialDirector>();
            director.DialogueUI = dialogueUI;
            director.HighlightFX = highlightFX;
            director.Workbench = workbench;
            director.OrderManager = propOrderMgr;
            director.DayEndSummary = dayEndUI;
            director.EnvManager = envManager;

            // Asignar puntos de interés
            director.DrawerP = itemP != null ? itemP.transform : null;
            director.DrawerNotP = itemNotP != null ? itemNotP.transform : null;
            director.DrawerQ = itemQ != null ? itemQ.transform : null;
            director.DrawerNotQ = itemNotQ != null ? itemNotQ.transform : null;
            director.DrawerS = itemS != null ? itemS.transform : null;
            director.DrawerR = itemR != null ? itemR.transform : null;
            director.Slot1 = slot1GO.transform;
            director.Slot2_Operator = slot2GO.transform;
            director.Slot3 = slot3GO.transform;
            director.ResultSlot = resultSlotGO.transform;
            director.DispatchZone = dispatchGO.transform;
            director.NotebookButtonHUD = notebookBtnGO.transform;

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[LogiPack] ¡Escena TutorialScene.unity 100% construida e integrada con éxito!</b></color>");
        }

        #region Cuaderno de Teoría Multihopágina (Lógica y Tablas de Verdad)

        public static GameplayNotebookGuideUI BuildGameplayNotebookModal(
            Transform canvasTransform,
            Button triggerBtn,
            Font fredokaBold,
            Sprite yellowBtnSprite,
            Sprite notebookOpenSprite,
            Sprite[] abrirFrames)
        {
            GameObject nbModalRootGO = new GameObject("Notebook_Modal_Root");
            nbModalRootGO.transform.SetParent(canvasTransform, false);
            var nbModalRT = nbModalRootGO.AddComponent<RectTransform>();
            nbModalRT.anchorMin = Vector2.zero;
            nbModalRT.anchorMax = Vector2.one;
            nbModalRT.sizeDelta = Vector2.zero;
            nbModalRT.anchoredPosition = Vector2.zero;

            GameObject nbDarkOverlay = new GameObject("DarkOverlay");
            nbDarkOverlay.transform.SetParent(nbModalRootGO.transform, false);
            var nbDarkRT = nbDarkOverlay.AddComponent<RectTransform>();
            nbDarkRT.anchorMin = Vector2.zero;
            nbDarkRT.anchorMax = Vector2.one;
            nbDarkRT.sizeDelta = Vector2.zero;
            var nbDarkImg = nbDarkOverlay.AddComponent<Image>();
            nbDarkImg.color = new Color(0, 0, 0, 0.75f);
            var overlayCloseBtn = nbDarkOverlay.AddComponent<Button>();

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
            nbWindowImg.color = Color.white;
            nbWindowImg.preserveAspect = true;

            // HOJA 0: Lógica Proposicional y Catálogo
            GameObject page0GO = new GameObject("Page_0_Basics");
            page0GO.transform.SetParent(nbWindowGO.transform, false);
            var p0RT = page0GO.AddComponent<RectTransform>();
            p0RT.anchorMin = Vector2.zero; p0RT.anchorMax = Vector2.one;
            p0RT.sizeDelta = Vector2.zero;

            GameObject left0TextGO = new GameObject("LeftPageText");
            left0TextGO.transform.SetParent(page0GO.transform, false);
            var lp0RT = left0TextGO.AddComponent<RectTransform>();
            lp0RT.anchorMin = new Vector2(0.5f, 0.5f); lp0RT.anchorMax = new Vector2(0.5f, 0.5f);
            lp0RT.pivot = new Vector2(0.5f, 0.5f);
            lp0RT.anchoredPosition = new Vector2(-360, 15);
            lp0RT.sizeDelta = new Vector2(530, 680);
            var lp0Text = left0TextGO.AddComponent<Text>();
            lp0Text.font = fredokaBold;
            lp0Text.fontSize = 28;
            lp0Text.color = new Color(0.12f, 0.12f, 0.12f);
            lp0Text.lineSpacing = 1.2f;
            lp0Text.text = "<b>LÓGICA PROPOSICIONAL</b>\n\n" +
                          "<b>∧ : Conjunción (y)</b>\nVerdadero si ambos cumplen.\n\n" +
                          "<b>∨ : Disyunción (o)</b>\nVerdadero si al menos uno cumple.\n\n" +
                          "<b>→ : Condicional</b>\n'Si A entonces B'.\n\n" +
                          "<b>↔ : Bicondicional</b>\n'A si y solo si B'.\n\n" +
                          "<b>~ : Negación (no)</b>\nInvierte el valor de verdad.";

            GameObject right0TextGO = new GameObject("RightPageText");
            right0TextGO.transform.SetParent(page0GO.transform, false);
            var rp0RT = right0TextGO.AddComponent<RectTransform>();
            rp0RT.anchorMin = new Vector2(0.5f, 0.5f); rp0RT.anchorMax = new Vector2(0.5f, 0.5f);
            rp0RT.pivot = new Vector2(0.5f, 0.5f);
            rp0RT.anchoredPosition = new Vector2(360, 30);
            rp0RT.sizeDelta = new Vector2(530, 640);
            var rp0Text = right0TextGO.AddComponent<Text>();
            rp0Text.font = fredokaBold;
            rp0Text.fontSize = 26;
            rp0Text.color = new Color(0.12f, 0.12f, 0.12f);
            rp0Text.lineSpacing = 1.2f;
            rp0Text.text = "<b>CATÁLOGO DE PROPOSICIONES</b>\n\n" +
                          "<b>p</b>: Perno Titanio  |  <b>~p</b>: Tornillo\n" +
                          "<b>q</b>: Batería Litio  |  <b>~q</b>: Cable\n" +
                          "<b>s</b>: Cristal Lab.   |  <b>~s</b>: Vidrio\n" +
                          "<b>r</b>: Ignífugo       |  <b>~r</b>: Algodón\n\n" +
                          "<i>Tip: Ranura 1 es término directo.\nRanura 3 es término negado (~).</i>";

            // HOJA 1: Teoría de Conjuntos en el Almacén
            GameObject page1GO = new GameObject("Page_1_SetTheory");
            page1GO.transform.SetParent(nbWindowGO.transform, false);
            var p1RT = page1GO.AddComponent<RectTransform>();
            p1RT.anchorMin = Vector2.zero; p1RT.anchorMax = Vector2.one;
            p1RT.sizeDelta = Vector2.zero;

            GameObject left1TextGO = new GameObject("LeftPageText_Set");
            left1TextGO.transform.SetParent(page1GO.transform, false);
            var lp1RT = left1TextGO.AddComponent<RectTransform>();
            lp1RT.anchorMin = new Vector2(0.5f, 0.5f); lp1RT.anchorMax = new Vector2(0.5f, 0.5f);
            lp1RT.pivot = new Vector2(0.5f, 0.5f);
            lp1RT.anchoredPosition = new Vector2(-360, 20);
            lp1RT.sizeDelta = new Vector2(530, 680);
            var lp1Text = left1TextGO.AddComponent<Text>();
            lp1Text.font = fredokaBold;
            lp1Text.fontSize = 24; // Aumentado a 24pt para cubrir el lienzo como Hoja 0
            lp1Text.color = new Color(0.12f, 0.12f, 0.12f);
            lp1Text.lineSpacing = 1.18f;
            lp1Text.verticalOverflow = VerticalWrapMode.Overflow;
            lp1Text.text = "<b>TEORÍA DE CONJUNTOS EN ALMACÉN</b>\n\n" +
                          "<b>x ∈ S</b> : Pertenencia del insumo 'x' a la orden 'S'.\n\n" +
                          "<b>∪ : Unión (A ∪ B)</b>\n" +
                          "Insumos que pertenecen a A o a B (o a ambos).\n" +
                          "<i>Ej: Metales ∪ Textiles (fijaciones o telas).</i>\n\n" +
                          "<b>∩ : Intersección (A ∩ B)</b>\n" +
                          "Insumos que cumplen ambas condiciones a la vez.\n" +
                          "<i>Ej: Metales ∩ VIP (Perno de Titanio p).</i>\n\n" +
                          "<b>− : Diferencia (A − B)</b>\n" +
                          "Insumos que están en A pero NO en B.\n" +
                          "<i>Ej: Metales − VIP (Tornillo Estándar ~p).</i>\n\n" +
                          "<b>Aᶜ : Complemento</b>\n" +
                          "Todo el universo del almacén EXCEPTO el conjunto A.\n" +
                          "<i>Ej: (Metales)ᶜ = Cualquier ítem no metálico.</i>";

            GameObject right1TextGO = new GameObject("RightPageText_Set");
            right1TextGO.transform.SetParent(page1GO.transform, false);
            var rp1RT = right1TextGO.AddComponent<RectTransform>();
            rp1RT.anchorMin = new Vector2(0.5f, 0.5f); rp1RT.anchorMax = new Vector2(0.5f, 0.5f);
            rp1RT.pivot = new Vector2(0.5f, 0.5f);
            rp1RT.anchoredPosition = new Vector2(360, 20);
            rp1RT.sizeDelta = new Vector2(550, 700);
            var rp1Text = right1TextGO.AddComponent<Text>();
            rp1Text.font = fredokaBold;
            rp1Text.fontSize = 24; // Aumentado a 24pt
            rp1Text.color = new Color(0.12f, 0.12f, 0.12f);
            rp1Text.lineSpacing = 1.18f;
            rp1Text.verticalOverflow = VerticalWrapMode.Overflow;
            rp1Text.text = "<b>CONJUNTOS PROPOSICIONALES</b>\n\n" +
                          "<b>• Conjunto de Negadas (¬P):</b>\n" +
                          "Insumos Básicos del Almacén:\n" +
                          "<b>~p</b>: Tornillo Estándar  |  <b>~q</b>: Cable Simple\n" +
                          "<b>~s</b>: Vidrio Plano        |  <b>~r</b>: Tela de Algodón\n\n" +
                          "<b>• Conjunto de Directas (P):</b>\n" +
                          "Insumos VIP / Avanzados:\n" +
                          "<b>p</b>: Perno Titanio   |  <b>q</b>: Batería Litio\n" +
                          "<b>s</b>: Cristal Lab.    |  <b>r</b>: Tejido Ignífugo\n\n" +
                          "<b>• Familias del Almacén:</b>\n" +
                          "Metales, Electricidad, Óptica, Textiles.\n\n" +
                          "<b>• Atributos Especiales:</b>\n" +
                          "Herramienta, Eléctrico, Frágil, Tela.";

            // HOJA 2: Tablas de Verdad (Auditoría y Leyes Lógicas)
            GameObject page2GO = new GameObject("Page_2_TruthTables");
            page2GO.transform.SetParent(nbWindowGO.transform, false);
            var p2RT = page2GO.AddComponent<RectTransform>();
            p2RT.anchorMin = Vector2.zero; p2RT.anchorMax = Vector2.one;
            p2RT.sizeDelta = Vector2.zero;

            GameObject left2TextGO = new GameObject("LeftPageText_TT");
            left2TextGO.transform.SetParent(page2GO.transform, false);
            var lp2RT = left2TextGO.AddComponent<RectTransform>();
            lp2RT.anchorMin = new Vector2(0.5f, 0.5f); lp2RT.anchorMax = new Vector2(0.5f, 0.5f);
            lp2RT.pivot = new Vector2(0.5f, 0.5f);
            lp2RT.anchoredPosition = new Vector2(-360, 20);
            lp2RT.sizeDelta = new Vector2(560, 720);
            var lp2Text = left2TextGO.AddComponent<Text>();
            lp2Text.font = fredokaBold;
            lp2Text.fontSize = 21; // Tipografía aumentada a 21pt para óptima legibilidad
            lp2Text.color = new Color(0.12f, 0.12f, 0.12f);
            lp2Text.lineSpacing = 1.16f;
            lp2Text.verticalOverflow = VerticalWrapMode.Overflow;
            lp2Text.text = "<b>TABLAS DE VERDAD Y LEYES LÓGICAS</b>\n\n" +
                          "<b>✓ 5 TAUTOLOGÍAS (Siempre Verdaderas ⊤):</b>\n" +
                          "1. <b>p ∨ ~p</b> : \"O llueve o no llueve\" (Tercero excluido)\n" +
                          "2. <b>p → p</b> : \"Si hay stock, entonces hay stock\" (Identidad)\n" +
                          "3. <b>~(p ∧ ~p)</b> : \"No ocurre que llueva y no a la vez\"\n" +
                          "4. <b>(p ∧ q) → p</b> : \"Si llueve y hace frío, seguro llueve\"\n" +
                          "5. <b>p → (q → p)</b> : \"Si llueve, si hace frío sigue lloviendo\"\n\n" +
                          "<b>✗ 5 CONTRADICCIONES (Siempre Falsas ⊥):</b>\n" +
                          "1. <b>p ∧ ~p</b> : \"Llueve y no llueve al mismo tiempo\"\n" +
                          "2. <b>~(p ∨ ~p)</b> : \"No es cierto que o llueva o no\"\n" +
                          "3. <b>p ↔ ~p</b> : \"Llueve si y solo si no llueve\" (Opuestos)\n" +
                          "4. <b>(p → q) ∧ (p ∧ ~q)</b> : \"Si llueve hace frío, pero no\"\n" +
                          "5. <b>~(p → p)</b> : \"Es falso que si hay stock, haya stock\"";

            GameObject right2TextGO = new GameObject("RightPageText_TT");
            right2TextGO.transform.SetParent(page2GO.transform, false);
            var rp2RT = right2TextGO.AddComponent<RectTransform>();
            rp2RT.anchorMin = new Vector2(0.5f, 0.5f); rp2RT.anchorMax = new Vector2(0.5f, 0.5f);
            rp2RT.pivot = new Vector2(0.5f, 0.5f);
            rp2RT.anchoredPosition = new Vector2(360, 20);
            rp2RT.sizeDelta = new Vector2(560, 720);
            var rp2Text = right2TextGO.AddComponent<Text>();
            rp2Text.font = fredokaBold;
            rp2Text.fontSize = 22; // Tipografía aumentada a 22pt
            rp2Text.color = new Color(0.12f, 0.12f, 0.12f);
            rp2Text.lineSpacing = 1.18f;
            rp2Text.verticalOverflow = VerticalWrapMode.Overflow;
            rp2Text.text = "<b>CONTINGENCIA Y GUÍA DE AUDITORÍA</b>\n\n" +
                          "<b>⚡ 5 CONTINGENCIAS (Casos Mixtos V y F):</b>\n" +
                          "1. <b>p ∧ q</b> : \"Llueve y hace frío\" (V solo si ambos ocurren)\n" +
                          "2. <b>p ∨ q</b> : \"Llueve o hace frío\" (V si al menos uno ocurre)\n" +
                          "3. <b>p → q</b> : \"Si llueve, hace frío\" (Falsa solo si V → F)\n" +
                          "4. <b>p ↔ q</b> : \"Llueve si y solo si hace frío\" (V si coinciden)\n" +
                          "5. <b>p ∧ ~q</b> : \"Llueve pero no hace frío\" (V solo en p=V, q=F)\n\n" +
                          "<b>GUÍA RÁPIDA DE DESPACHO:</b>\n" +
                          "• <b>Todo V</b> → Sello <b>TAUTOLOGÍA</b>\n" +
                          "• <b>Todo F</b> → Sello <b>CONTRADICCIÓN</b>\n" +
                          "• <b>V y F mixtos</b> → Sello <b>CONTINGENCIA</b>\n\n" +
                          "<i>Atajos: 1 (Tautología), 2 (Contradicción), 3 (Contingencia).</i>";

            Sprite arrowLeftSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/CozyUI/Arrows/ArrowBrownOuline22.png");
            Sprite arrowRightSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/CozyUI/Arrows/ArrowBrownOuline23.png");

            GameObject prevBtnGO = new GameObject("Button_PrevPage");
            prevBtnGO.transform.SetParent(nbWindowGO.transform, false);
            var prevRT = prevBtnGO.AddComponent<RectTransform>();
            prevRT.anchoredPosition = new Vector2(-540, -370);
            prevRT.sizeDelta = new Vector2(30, 42);
            var prevImg = prevBtnGO.AddComponent<Image>();
            prevImg.sprite = arrowLeftSpr;
            prevImg.preserveAspect = true;
            var prevBtn = prevBtnGO.AddComponent<Button>();
            prevBtn.targetGraphic = prevImg;

            GameObject nextBtnGO = new GameObject("Button_NextPage");
            nextBtnGO.transform.SetParent(nbWindowGO.transform, false);
            var nextRT = nextBtnGO.AddComponent<RectTransform>();
            nextRT.anchoredPosition = new Vector2(540, -370);
            nextRT.sizeDelta = new Vector2(30, 42);
            var nextImg = nextBtnGO.AddComponent<Image>();
            nextImg.sprite = arrowRightSpr;
            nextImg.preserveAspect = true;
            var nextBtn = nextBtnGO.AddComponent<Button>();
            nextBtn.targetGraphic = nextImg;

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

            var guideUI = triggerBtn.gameObject.AddComponent<GameplayNotebookGuideUI>();
            guideUI.TriggerButton = triggerBtn;
            guideUI.ModalRoot = nbModalRootGO;
            guideUI.NotebookWindowRect = nbWindowRT;
            guideUI.NotebookWindowImage = nbWindowImg;
            guideUI.CloseButton = cnbBtn;
            guideUI.OverlayCloseButton = overlayCloseBtn;
            guideUI.PageBasics = page0GO;
            guideUI.PageSetTheory = page1GO;
            guideUI.PageTruthTables = page2GO;
            guideUI.NextPageButton = nextBtn;
            guideUI.PrevPageButton = prevBtn;
            guideUI.CuadernoAbierto = notebookOpenSprite;
            guideUI.AbrirFrames = abrirFrames;

            nbModalRootGO.SetActive(false);
            return guideUI;
        }

        #endregion

        #region Construcción de CheckoutScene (Modo de Auditoría y Tablas de Verdad)

        [MenuItem("LogiPack/Construir Escena de Check-out")]
        public static void BuildCheckoutScene()
        {
            string scenePath = "Assets/Scenes/CheckoutScene.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 0. Servicios Persistentes Centrales
            MenuSetupTools.EnsureAppCoreServices();

            // Fuente oficial Fredoka
            Font fredokaBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Font/static/Fredoka-Bold.ttf");
            if (fredokaBold == null) fredokaBold = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Configurar Cámara
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

            // 3. Fondo de Mesa de Metal Limpia (Sin reloj ni marco de dinero)
            Sprite cleanDeskSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Maps/mesa_trabajo_metal_limpia.png")
                .OfType<Sprite>()
                .FirstOrDefault();
            if (cleanDeskSprite == null) cleanDeskSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mesa_trabajo_metal_limpia.png");
            if (cleanDeskSprite == null) cleanDeskSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mesa_trabajo.png");

            GameObject deskGO = new GameObject("Background_CleanMetalDesk");
            deskGO.transform.position = Vector3.zero;
            var deskSR = deskGO.AddComponent<SpriteRenderer>();
            deskSR.sprite = cleanDeskSprite;
            deskSR.sortingOrder = 0;

            // 4. Canvas UI Principal
            GameObject canvasGO = new GameObject("CheckoutCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Sprites
            Sprite yellowBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_amarillo_rectangular.png");
            Sprite greenBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_verde_rectangular.png");
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/pantalla_fin_jornada.png");
            Sprite notebookClosedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites_sheets/cuaderno_cerrado.png");
            Sprite notebookOpenSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites_sheets/cuaderno_abierto.png");
            var abrirFrames = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites_sheets/cuaderno_abrir.png")
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();

            // 5. Director General
            GameObject directorGO = new GameObject("CheckoutDirector");
            var director = directorGO.AddComponent<CheckoutDirector>();

            // 6. HUD Superior Izquierdo
            GameObject hudGO = new GameObject("HUD_TopLeft");
            hudGO.transform.SetParent(canvasGO.transform, false);
            var hudRT = hudGO.AddComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(0f, 1f);
            hudRT.anchorMax = new Vector2(0f, 1f);
            hudRT.pivot = new Vector2(0f, 1f);
            hudRT.anchoredPosition = new Vector2(50, -40);
            hudRT.sizeDelta = new Vector2(400, 100);

            GameObject roundGO = new GameObject("RoundText");
            roundGO.transform.SetParent(hudGO.transform, false);
            var rRT = roundGO.AddComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0f, 1f); rRT.anchorMax = new Vector2(0f, 1f);
            rRT.pivot = new Vector2(0f, 1f);
            rRT.anchoredPosition = new Vector2(0, 0);
            rRT.sizeDelta = new Vector2(380, 40);
            var rText = roundGO.AddComponent<Text>();
            rText.font = fredokaBold;
            rText.fontSize = 28;
            rText.color = Color.white;
            rText.alignment = TextAnchor.MiddleLeft;
            var rShadow = roundGO.AddComponent<Shadow>();
            rShadow.effectColor = new Color(0, 0, 0, 0.7f);
            rShadow.effectDistance = new Vector2(1, -1);
            director.RoundText = rText;

            GameObject progGO = new GameObject("ProgressText");
            progGO.transform.SetParent(hudGO.transform, false);
            var pRT = progGO.AddComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0f, 1f); pRT.anchorMax = new Vector2(0f, 1f);
            pRT.pivot = new Vector2(0f, 1f);
            pRT.anchoredPosition = new Vector2(0, -42);
            pRT.sizeDelta = new Vector2(380, 30);
            var pText = progGO.AddComponent<Text>();
            pText.font = fredokaBold;
            pText.fontSize = 20;
            pText.color = new Color(0.90f, 0.66f, 0.14f);
            pText.alignment = TextAnchor.MiddleLeft;
            var pShadow = progGO.AddComponent<Shadow>();
            pShadow.effectColor = new Color(0, 0, 0, 0.7f);
            pShadow.effectDistance = new Vector2(1, -1);
            director.SheetProgressText = pText;

            // 7. Tríptico de Expedientes
            GameObject tripticoGO = new GameObject("TripticoContainer");
            tripticoGO.transform.SetParent(canvasGO.transform, false);
            var tripRT = tripticoGO.AddComponent<RectTransform>();
            tripRT.anchorMin = new Vector2(0.5f, 0.5f);
            tripRT.anchorMax = new Vector2(0.5f, 0.5f);
            tripRT.pivot = new Vector2(0.5f, 0.5f);
            tripRT.anchoredPosition = new Vector2(0, 45);
            tripRT.sizeDelta = new Vector2(1100, 500);

            float[] sheetX = new float[] { -360f, 0f, 360f };
            director.Sheets = new CheckoutSheetUI[3];
            for (int i = 0; i < 3; i++)
            {
                director.Sheets[i] = CreateCheckoutSheetCard(tripticoGO.transform, $"Sheet_{i + 1}", new Vector2(sheetX[i], 0), fredokaBold);
            }

            // 8. Botones de Clasificación Inferiores
            GameObject buttonsGO = new GameObject("DecisionButtonsContainer");
            buttonsGO.transform.SetParent(canvasGO.transform, false);
            var btnsRT = buttonsGO.AddComponent<RectTransform>();
            btnsRT.anchorMin = new Vector2(0.5f, 0f);
            btnsRT.anchorMax = new Vector2(0.5f, 0f);
            btnsRT.pivot = new Vector2(0.5f, 0f);
            btnsRT.anchoredPosition = new Vector2(0, 80);
            btnsRT.sizeDelta = new Vector2(1100, 120);

            director.TautologyButton = CreateDecisionButton(buttonsGO.transform, "Btn_Tautology", new Vector2(-360, 0), new Vector2(290, 70), greenBtnSprite, "TAUTOLOGÍA [1]", fredokaBold, 22);
            director.ContradictionButton = CreateDecisionButton(buttonsGO.transform, "Btn_Contradiction", new Vector2(0, 0), new Vector2(290, 70), yellowBtnSprite, "CONTRADICCIÓN [2]", fredokaBold, 22);
            director.ContingencyButton = CreateDecisionButton(buttonsGO.transform, "Btn_Contingency", new Vector2(360, 0), new Vector2(290, 70), yellowBtnSprite, "CONTINGENCIA [3]", fredokaBold, 22);

            GameObject hintsGO = new GameObject("KeyboardHints");
            hintsGO.transform.SetParent(buttonsGO.transform, false);
            var hRT = hintsGO.AddComponent<RectTransform>();
            hRT.anchoredPosition = new Vector2(0, -50);
            hRT.sizeDelta = new Vector2(900, 30);
            var hText = hintsGO.AddComponent<Text>();
            hText.font = fredokaBold;
            hText.fontSize = 17;
            hText.color = new Color(0.70f, 0.72f, 0.75f);
            hText.alignment = TextAnchor.MiddleCenter;
            hText.text = "Atajos: [1 / Q] Tautología  •  [2 / W] Contradicción  •  [3 / E] Contingencia";

            // 9. Botones Superiores Derechos: Pausa (Madera + Casa) y Cuaderno Guía
            Sprite pauseWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/CozyUI/Buttons/Square/SquareButton1_wood.png");
            Sprite houseIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/icono_casa.png");

            GameObject pauseBtnGO = new GameObject("Button_Pause");
            pauseBtnGO.transform.SetParent(canvasGO.transform, false);
            var pauseRT = pauseBtnGO.AddComponent<RectTransform>();
            pauseRT.anchorMin = new Vector2(1f, 1f); pauseRT.anchorMax = new Vector2(1f, 1f);
            pauseRT.pivot = new Vector2(1f, 1f);
            pauseRT.anchoredPosition = new Vector2(-40, -35);
            pauseRT.sizeDelta = new Vector2(65, 65);
            var pauseImg = pauseBtnGO.AddComponent<Image>();
            pauseImg.sprite = pauseWoodSprite;
            var pauseBtn = pauseBtnGO.AddComponent<Button>();
            pauseBtn.targetGraphic = pauseImg;

            GameObject pauseIconGO = new GameObject("Icon");
            pauseIconGO.transform.SetParent(pauseBtnGO.transform, false);
            var pIconRT = pauseIconGO.AddComponent<RectTransform>();
            pIconRT.sizeDelta = new Vector2(42, 42);
            var pIconImg = pauseIconGO.AddComponent<Image>();
            pIconImg.sprite = houseIconSprite;
            pIconImg.raycastTarget = false;

            GameObject nbBtnGO = new GameObject("Button_NotebookGuide");
            nbBtnGO.transform.SetParent(canvasGO.transform, false);
            var nbRT = nbBtnGO.AddComponent<RectTransform>();
            nbRT.anchorMin = new Vector2(1f, 1f); nbRT.anchorMax = new Vector2(1f, 1f);
            nbRT.pivot = new Vector2(1f, 1f);
            nbRT.anchoredPosition = new Vector2(-120, -35);
            nbRT.sizeDelta = new Vector2(65, 65);
            var nbImg = nbBtnGO.AddComponent<Image>();
            nbImg.sprite = notebookClosedSprite;
            nbImg.preserveAspect = true;
            var nbBtn = nbBtnGO.AddComponent<Button>();
            nbBtn.targetGraphic = nbImg;

            // 10. Modales (Se crean al final del Canvas para garantizar que se dibujen encima del tríptico y botones)
            BuildGameplayNotebookModal(canvasGO.transform, nbBtn, fredokaBold, yellowBtnSprite, notebookOpenSprite, abrirFrames);

            var optionsUI = MenuSetupTools.BuildOptionsMenuHierarchy(canvasGO.transform, fredokaBold, cardSprite, yellowBtnSprite, greenBtnSprite, isGameplay: true);
            optionsUI.ScreenPauseButton = pauseBtn;
            pauseBtn.onClick.AddListener(optionsUI.OpenOptions);

            director.SummaryUI = CreateCheckoutSummaryModal(canvasGO.transform, fredokaBold, cardSprite, greenBtnSprite, yellowBtnSprite);

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green><b>[SceneSetupTools] ¡CheckoutScene construida con éxito en Assets/Scenes/CheckoutScene.unity!</b></color>");
        }

        private static CheckoutSheetUI CreateCheckoutSheetCard(Transform parent, string name, Vector2 position, Font font)
        {
            GameObject sheetGO = new GameObject(name);
            sheetGO.transform.SetParent(parent, false);
            var rt = sheetGO.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(320, 460);
            var cg = sheetGO.AddComponent<CanvasGroup>();

            var sheetUI = sheetGO.AddComponent<CheckoutSheetUI>();
            sheetUI.RootRectTransform = rt;
            sheetUI.SheetCanvasGroup = cg;

            GameObject focusGO = new GameObject("FocusHighlight");
            focusGO.transform.SetParent(sheetGO.transform, false);
            var fRT = focusGO.AddComponent<RectTransform>();
            fRT.sizeDelta = new Vector2(334, 474);
            var fImg = focusGO.AddComponent<Image>();
            fImg.color = new Color(0.90f, 0.66f, 0.14f, 0.85f);
            sheetUI.HighlightBorder = focusGO;
            focusGO.SetActive(false);

            GameObject paperGO = new GameObject("PaperBackground");
            paperGO.transform.SetParent(sheetGO.transform, false);
            var pRT = paperGO.AddComponent<RectTransform>();
            pRT.sizeDelta = new Vector2(320, 460);
            var pImg = paperGO.AddComponent<Image>();
            pImg.color = new Color(0.98f, 0.97f, 0.95f, 1.0f);

            var pOutline = paperGO.AddComponent<Outline>();
            pOutline.effectColor = new Color(0.82f, 0.79f, 0.75f, 0.9f);
            pOutline.effectDistance = new Vector2(2, -2);

            GameObject headerGO = new GameObject("Header");
            headerGO.transform.SetParent(sheetGO.transform, false);
            var hRT = headerGO.AddComponent<RectTransform>();
            hRT.anchoredPosition = new Vector2(0, 185);
            hRT.sizeDelta = new Vector2(280, 35);
            var hText = headerGO.AddComponent<Text>();
            hText.font = font;
            hText.fontSize = 20;
            hText.color = new Color(0.43f, 0.41f, 0.38f);
            hText.alignment = TextAnchor.MiddleCenter;
            sheetUI.HeaderText = hText;

            MenuSetupTools.CreateDividerLine(sheetGO.transform, new Vector2(0, 160), 280f, 1.5f, new Color(0.83f, 0.81f, 0.77f, 0.85f));

            GameObject fbfLabelGO = new GameObject("FBFLabel");
            fbfLabelGO.transform.SetParent(sheetGO.transform, false);
            var flRT = fbfLabelGO.AddComponent<RectTransform>();
            flRT.anchoredPosition = new Vector2(0, 130);
            flRT.sizeDelta = new Vector2(280, 25);
            var flText = fbfLabelGO.AddComponent<Text>();
            flText.font = font;
            flText.fontSize = 13;
            flText.color = new Color(0.55f, 0.52f, 0.48f);
            flText.alignment = TextAnchor.MiddleCenter;
            flText.text = "FÓRMULA BIEN FORMADA";

            GameObject fbfGO = new GameObject("FormulaText");
            fbfGO.transform.SetParent(sheetGO.transform, false);
            var fRT2 = fbfGO.AddComponent<RectTransform>();
            fRT2.anchoredPosition = new Vector2(0, 65);
            fRT2.sizeDelta = new Vector2(290, 90);
            var fText = fbfGO.AddComponent<Text>();
            fText.font = font;
            fText.fontSize = 32;
            fText.color = new Color(0.11f, 0.11f, 0.11f);
            fText.alignment = TextAnchor.MiddleCenter;
            sheetUI.FormulaText = fText;

            MenuSetupTools.CreateDividerLine(sheetGO.transform, new Vector2(0, 0), 280f, 1.5f, new Color(0.83f, 0.81f, 0.77f, 0.85f));

            GameObject nlLabelGO = new GameObject("NaturalLanguageLabel");
            nlLabelGO.transform.SetParent(sheetGO.transform, false);
            var nlRT = nlLabelGO.AddComponent<RectTransform>();
            nlRT.anchoredPosition = new Vector2(0, -30);
            nlRT.sizeDelta = new Vector2(280, 25);
            var nlText = nlLabelGO.AddComponent<Text>();
            nlText.font = font;
            nlText.fontSize = 13;
            nlText.color = new Color(0.55f, 0.52f, 0.48f);
            nlText.alignment = TextAnchor.MiddleCenter;
            nlText.text = "INTERPRETACIÓN EN LENGUAJE NATURAL";

            GameObject nlGO = new GameObject("NaturalLanguageText");
            nlGO.transform.SetParent(sheetGO.transform, false);
            var nlRT2 = nlGO.AddComponent<RectTransform>();
            nlRT2.anchoredPosition = new Vector2(0, -95);
            nlRT2.sizeDelta = new Vector2(280, 95);
            var nlText2 = nlGO.AddComponent<Text>();
            nlText2.font = font;
            nlText2.fontStyle = FontStyle.Italic;
            nlText2.fontSize = 17;
            nlText2.color = new Color(0.24f, 0.22f, 0.20f);
            nlText2.alignment = TextAnchor.MiddleCenter;
            nlText2.lineSpacing = 1.25f;
            sheetUI.NaturalLanguageText = nlText2;

            GameObject footerGO = new GameObject("Footer");
            footerGO.transform.SetParent(sheetGO.transform, false);
            var ftRT = footerGO.AddComponent<RectTransform>();
            ftRT.anchoredPosition = new Vector2(0, -195);
            ftRT.sizeDelta = new Vector2(280, 25);
            var ftText = footerGO.AddComponent<Text>();
            ftText.font = font;
            ftText.fontSize = 12;
            ftText.color = new Color(0.63f, 0.60f, 0.56f);
            ftText.alignment = TextAnchor.MiddleCenter;
            ftText.text = "AUDITORÍA DE DESPACHO • LOGIPACK";

            GameObject stampGO = new GameObject("StampContainer");
            stampGO.transform.SetParent(sheetGO.transform, false);
            var sRT = stampGO.AddComponent<RectTransform>();
            sRT.anchoredPosition = new Vector2(0, 10);
            sRT.sizeDelta = new Vector2(280, 85);
            sheetUI.StampContainer = stampGO;
            sheetUI.StampRectTransform = sRT;

            var stampBorderImg = stampGO.AddComponent<Image>();
            stampBorderImg.color = new Color(0.11f, 0.50f, 0.20f, 0.95f);
            sheetUI.StampBorderImage = stampBorderImg;

            GameObject stampInnerGO = new GameObject("StampInner");
            stampInnerGO.transform.SetParent(stampGO.transform, false);
            var siRT = stampInnerGO.AddComponent<RectTransform>();
            siRT.sizeDelta = new Vector2(272, 77);
            var siImg = stampInnerGO.AddComponent<Image>();
            siImg.color = new Color(1f, 1f, 1f, 0.92f);

            GameObject stampTxtGO = new GameObject("StampText");
            stampTxtGO.transform.SetParent(stampInnerGO.transform, false);
            var stRT = stampTxtGO.AddComponent<RectTransform>();
            stRT.sizeDelta = siRT.sizeDelta;
            var stText = stampTxtGO.AddComponent<Text>();
            stText.font = font;
            stText.fontSize = 25;
            stText.fontStyle = FontStyle.Bold;
            stText.alignment = TextAnchor.MiddleCenter;
            stText.color = new Color(0.11f, 0.50f, 0.20f, 0.95f);
            stText.text = "✓ DESPACHADO";
            sheetUI.StampText = stText;

            stampGO.SetActive(false);

            return sheetUI;
        }

        private static Button CreateDecisionButton(Transform parent, string name, Vector2 position, Vector2 size, Sprite btnSprite, string label, Font font, int fontSize)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            var img = btnGO.AddComponent<Image>();
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced;

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            var txtRT = textGO.AddComponent<RectTransform>();
            txtRT.sizeDelta = size;

            var txt = textGO.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.10f, 0.10f, 0.10f);
            txt.text = label;

            return btn;
        }

        private static CheckoutSummaryUI CreateCheckoutSummaryModal(Transform parent, Font font, Sprite cardSprite, Sprite greenBtn, Sprite yellowBtn)
        {
            GameObject modalRootGO = new GameObject("CheckoutSummaryModal");
            modalRootGO.transform.SetParent(parent, false);
            var mRT = modalRootGO.AddComponent<RectTransform>();
            mRT.anchorMin = Vector2.zero; mRT.anchorMax = Vector2.one;
            mRT.sizeDelta = Vector2.zero;
            mRT.anchoredPosition = Vector2.zero;

            var cg = modalRootGO.AddComponent<CanvasGroup>();
            var summaryUI = modalRootGO.AddComponent<CheckoutSummaryUI>();
            summaryUI.ModalRoot = modalRootGO;
            summaryUI.ModalCanvasGroup = cg;

            GameObject darkOverlay = new GameObject("DarkOverlay");
            darkOverlay.transform.SetParent(modalRootGO.transform, false);
            var doRT = darkOverlay.AddComponent<RectTransform>();
            doRT.anchorMin = Vector2.zero; doRT.anchorMax = Vector2.one;
            doRT.sizeDelta = Vector2.zero;
            var doImg = darkOverlay.AddComponent<Image>();
            doImg.color = new Color(0, 0, 0, 0.78f);

            GameObject cardGO = new GameObject("SummaryCard");
            cardGO.transform.SetParent(modalRootGO.transform, false);
            var cRT = cardGO.AddComponent<RectTransform>();
            cRT.anchoredPosition = Vector2.zero;
            cRT.sizeDelta = new Vector2(760, 580);
            var cImg = cardGO.AddComponent<Image>();
            cImg.sprite = cardSprite;
            cImg.type = Image.Type.Sliced;
            cImg.color = Color.white;

            GameObject titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(cardGO.transform, false);
            var tRT = titleGO.AddComponent<RectTransform>();
            tRT.anchoredPosition = new Vector2(0, 220);
            tRT.sizeDelta = new Vector2(650, 50);
            var tText = titleGO.AddComponent<Text>();
            tText.font = font;
            tText.fontSize = 32;
            tText.fontStyle = FontStyle.Bold;
            tText.alignment = TextAnchor.MiddleCenter;
            tText.color = new Color(0.13f, 0.13f, 0.13f);
            tText.text = "REPORTE DE AUDITORÍA";
            summaryUI.TitleText = tText;

            GameObject starsGO = new GameObject("StarsText");
            starsGO.transform.SetParent(cardGO.transform, false);
            var sRT = starsGO.AddComponent<RectTransform>();
            sRT.anchoredPosition = new Vector2(0, 160);
            sRT.sizeDelta = new Vector2(400, 55);
            var sText = starsGO.AddComponent<Text>();
            sText.font = font;
            sText.fontSize = 42;
            sText.fontStyle = FontStyle.Bold;
            sText.alignment = TextAnchor.MiddleCenter;
            sText.color = new Color(0.90f, 0.66f, 0.14f);
            sText.text = "★ ★ ★";
            summaryUI.StarsText = sText;

            GameObject rankGO = new GameObject("RankTitleText");
            rankGO.transform.SetParent(cardGO.transform, false);
            var rRT = rankGO.AddComponent<RectTransform>();
            rRT.anchoredPosition = new Vector2(0, 105);
            rRT.sizeDelta = new Vector2(650, 40);
            var rText = rankGO.AddComponent<Text>();
            rText.font = font;
            rText.fontSize = 24;
            rText.fontStyle = FontStyle.Bold;
            rText.alignment = TextAnchor.MiddleCenter;
            rText.color = new Color(0.15f, 0.15f, 0.15f);
            rText.text = "AUDITOR LÓGICO EXPERTO";
            summaryUI.RankTitleText = rText;

            MenuSetupTools.CreateDividerLine(cardGO.transform, new Vector2(0, 75), 550f, 1.5f, new Color(0.83f, 0.81f, 0.77f, 0.85f));

            GameObject hitsGO = new GameObject("HitsSummaryText");
            hitsGO.transform.SetParent(cardGO.transform, false);
            var hRT = hitsGO.AddComponent<RectTransform>();
            hRT.anchoredPosition = new Vector2(0, 40);
            hRT.sizeDelta = new Vector2(650, 35);
            var hText = hitsGO.AddComponent<Text>();
            hText.font = font;
            hText.fontSize = 21;
            hText.alignment = TextAnchor.MiddleCenter;
            hText.color = new Color(0.25f, 0.25f, 0.25f);
            hText.text = "8 / 9 Expedientes Correctos (89% de Precisión)";
            summaryUI.HitsSummaryText = hText;

            GameObject breakGO = new GameObject("RoundBreakdownText");
            breakGO.transform.SetParent(cardGO.transform, false);
            var bRT = breakGO.AddComponent<RectTransform>();
            bRT.anchoredPosition = new Vector2(0, -45);
            bRT.sizeDelta = new Vector2(450, 100);
            var bText = breakGO.AddComponent<Text>();
            bText.font = font;
            bText.fontSize = 19;
            bText.alignment = TextAnchor.MiddleCenter;
            bText.lineSpacing = 1.35f;
            bText.color = new Color(0.43f, 0.41f, 0.38f);
            bText.text = "• Ronda 1:  3 / 3 aciertos\n• Ronda 2:  3 / 3 aciertos\n• Ronda 3:  2 / 3 aciertos";
            summaryUI.RoundBreakdownText = bText;

            summaryUI.RetryButton = CreateDecisionButton(cardGO.transform, "Btn_Retry", new Vector2(-155, -195), new Vector2(250, 60), greenBtn, "REINTENTAR", font, 20);
            summaryUI.MainMenuButton = CreateDecisionButton(cardGO.transform, "Btn_MainMenu", new Vector2(155, -195), new Vector2(250, 60), yellowBtn, "MENÚ PRINCIPAL", font, 20);

            modalRootGO.SetActive(false);
            return summaryUI;
        }

        #endregion

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

        private static GameObject CreateItemSourceSlot(Transform parent, string name, Vector3 pos, string varName, bool isNeg, ItemData item, Sprite sprite)
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

            return slotGO;
        }

        #region Métodos de Test en PlayMode

        [MenuItem("LogiPack/Test/Checkout/Abrir Cuaderno Teoria")]
        public static void TestCheckoutOpenTheory()
        {
            var guide = Object.FindAnyObjectByType<GameplayNotebookGuideUI>();
            if (guide != null) guide.OpenGuide();
        }

        [MenuItem("LogiPack/Test/Checkout/Siguiente Pagina Teoria")]
        public static void TestCheckoutNextTheory()
        {
            var guide = Object.FindAnyObjectByType<GameplayNotebookGuideUI>();
            if (guide != null) guide.NextPage();
        }

        [MenuItem("LogiPack/Test/Checkout/Anterior Pagina Teoria")]
        public static void TestCheckoutPrevTheory()
        {
            var guide = Object.FindAnyObjectByType<GameplayNotebookGuideUI>();
            if (guide != null) guide.PrevPage();
        }

        [MenuItem("LogiPack/Test/Checkout/Cerrar Cuaderno Teoria")]
        public static void TestCheckoutCloseTheory()
        {
            var guide = Object.FindAnyObjectByType<GameplayNotebookGuideUI>();
            if (guide != null) guide.CloseGuide();
        }

        [MenuItem("LogiPack/Test/Checkout/Clasificar Tautologia")]
        public static void TestCheckoutClassifyTautology()
        {
            if (CheckoutDirector.Instance != null)
                CheckoutDirector.Instance.OnSubmitClassification(PropositionEvaluation.Tautology);
        }

        [MenuItem("LogiPack/Test/Checkout/Clasificar Contradiccion")]
        public static void TestCheckoutClassifyContradiction()
        {
            if (CheckoutDirector.Instance != null)
                CheckoutDirector.Instance.OnSubmitClassification(PropositionEvaluation.Contradiction);
        }

        [MenuItem("LogiPack/Test/Checkout/Clasificar Contingencia")]
        public static void TestCheckoutClassifyContingency()
        {
            if (CheckoutDirector.Instance != null)
                CheckoutDirector.Instance.OnSubmitClassification(PropositionEvaluation.Contingency);
        }

        [MenuItem("LogiPack/Test/Checkout/Diagnosticar Jerarquia")]
        public static void TestDiagnoseHierarchy()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.Log("[Diagnostico] No se encontró Canvas");
                return;
            }
            Debug.Log($"[Diagnostico] Canvas: {canvas.name}");
            PrintHierarchy(canvas.transform, 0);
        }

        private static void PrintHierarchy(Transform t, int indent)
        {
            string pad = new string('-', indent * 2);
            var img = t.GetComponent<Image>();
            string imgInfo = img != null ? $" [Image active={img.enabled}, color={img.color}, sprite={(img.sprite != null ? img.sprite.name : "null")}]" : "";
            Debug.Log($"{pad} {t.name} (activeSelf={t.gameObject.activeSelf}, activeInHierarchy={t.gameObject.activeInHierarchy}){imgInfo}");
            for (int i = 0; i < t.childCount; i++)
            {
                PrintHierarchy(t.GetChild(i), indent + 1);
            }
        }

        #endregion
    }
}
