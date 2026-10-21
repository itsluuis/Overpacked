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
                mainCam.backgroundColor = new Color(0.12f, 0.15f, 0.20f);
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }

            // 2. EventSystem (Unity 6 New Input System)
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<InputSystemUIInputModule>();

            // 3. Crear Prefab de la Caja con soporte de Caja Abierta y Cerrada
            string prefabsDir = "Assets/Prefabs";
            if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

            Sprite boxOpenSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/caja_carton_abierta.png");
            Sprite boxClosedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/caja_carton_cerrada.png");

            string boxPrefabPath = "Assets/Prefabs/PackageBoxPrefab.prefab";
            GameObject tempBox = new GameObject("PackageBoxPrefab");
            tempBox.transform.localScale = Vector3.one * 0.20f; // Escala proporcionada para 1024px

            var tempSR = tempBox.AddComponent<SpriteRenderer>();
            tempSR.sprite = boxOpenSprite;
            tempSR.sortingOrder = 5;

            var tempCol = tempBox.AddComponent<BoxCollider2D>();
            tempCol.size = new Vector2(10.0f, 10.0f); // 2.0 unidades reales al multiplicar por escala 0.20

            var pBox = tempBox.AddComponent<PackageBox>();
            pBox.OpenBoxSprite = boxOpenSprite;
            pBox.ClosedBoxSprite = boxClosedSprite;
            tempBox.AddComponent<DraggableBox>();

            // Overlays de embalaje sobre la caja
            GameObject foamGO = new GameObject("FoamOverlay");
            foamGO.transform.parent = tempBox.transform;
            foamGO.transform.localPosition = new Vector3(0, 0, -0.05f);
            foamGO.transform.localScale = Vector3.one * 1.05f;
            var foamSR = foamGO.AddComponent<SpriteRenderer>();
            foamSR.sprite = boxOpenSprite;
            foamSR.color = new Color(1.0f, 0.92f, 0.35f, 0.55f);
            foamSR.sortingOrder = 7;
            foamGO.SetActive(false);
            pBox.FoamOverlay = foamGO;

            GameObject filmGO = new GameObject("ThermalFilmOverlay");
            filmGO.transform.parent = tempBox.transform;
            filmGO.transform.localPosition = new Vector3(0, 0, -0.08f);
            filmGO.transform.localScale = Vector3.one * 1.10f;
            var filmSR = filmGO.AddComponent<SpriteRenderer>();
            filmSR.sprite = boxOpenSprite;
            filmSR.color = new Color(0.25f, 0.85f, 1.0f, 0.50f);
            filmSR.sortingOrder = 8;
            filmGO.SetActive(false);
            pBox.ThermalFilmOverlay = filmGO;

            GameObject boxPrefab = PrefabUtility.SaveAsPrefabAsset(tempBox, boxPrefabPath);
            GameObject.DestroyImmediate(tempBox);

            // 4. Fondos del Escenario (Capa Clima Exterior y Capa Almacén)
            GameObject bgWeatherGO = new GameObject("Background_Weather");
            bgWeatherGO.transform.position = Vector3.zero;
            var weatherSR = bgWeatherGO.AddComponent<SpriteRenderer>();
            weatherSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/clima_soleado.png");
            weatherSR.sortingOrder = -2; // Visible a través de la ventana y compuerta

            GameObject bgWarehouseGO = new GameObject("Background_Warehouse");
            bgWarehouseGO.transform.position = Vector3.zero;
            var warehouseSR = bgWarehouseGO.AddComponent<SpriteRenderer>();
            warehouseSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mapa_gameplay_almacen.png");
            warehouseSR.sortingOrder = 0; // Lienzo principal de las paredes y piso

            // 5. Monitores TV a la Izquierda de la Ventana
            GameObject tvTempGO = new GameObject("TV_Temperatura_Forecast");
            tvTempGO.transform.position = new Vector3(-5.6f, 3.3f, 0);
            tvTempGO.transform.localScale = Vector3.one * 0.55f;
            var tvTempSR = tvTempGO.AddComponent<SpriteRenderer>();
            tvTempSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tv_temperatura_frames/tv_temperatura_calor.png");
            tvTempSR.sortingOrder = 1;

            GameObject tvClimaGO = new GameObject("TV_Clima_Forecast");
            tvClimaGO.transform.position = new Vector3(-5.6f, 1.9f, 0);
            tvClimaGO.transform.localScale = Vector3.one * 0.55f;
            var tvClimaSR = tvClimaGO.AddComponent<SpriteRenderer>();
            tvClimaSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tv_clima_frames/tv_clima_soleado.png");
            tvClimaSR.sortingOrder = 1;

            // 6. Reloj Digital a la Derecha de la Ventana
            GameObject clockGO = new GameObject("Reloj_Digital");
            clockGO.transform.position = new Vector3(6.8f, 2.6f, 0);
            clockGO.transform.localScale = Vector3.one * 0.65f;
            var clockSR = clockGO.AddComponent<SpriteRenderer>();
            clockSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/reloj_individual_frames/reloj_06_00.png");
            clockSR.sortingOrder = 1;

            // Cargar los 17 frames del reloj (06_00 a 22_00)
            Sprite[] clockFrames = new Sprite[17];
            for (int h = 6; h <= 22; h++)
            {
                clockFrames[h - 6] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Maps_Assets/reloj_individual_frames/reloj_{h:D2}_00.png");
            }

            // 7. Managers del Sistema
            GameObject managersGO = new GameObject("=== MANAGERS ===");
            var envMgr = managersGO.AddComponent<EnvironmentManager>();
            var ecoMgr = managersGO.AddComponent<EconomyManager>();
            var packMgr = managersGO.AddComponent<PackagingZoneManager>();
            var orderMgr = managersGO.AddComponent<OrderManager>();
            var boxSpawner = managersGO.AddComponent<BoxSpawner>();
            var infiniteCtrl = managersGO.AddComponent<InfiniteModeController>();

            orderMgr.Environment = envMgr;
            orderMgr.Economy = ecoMgr;
            orderMgr.PackagingZone = packMgr;

            infiniteCtrl.Economy = ecoMgr;
            infiniteCtrl.Orders = orderMgr;
            infiniteCtrl.Packaging = packMgr;

            // Controlador de Visuales Ambientales
            var envVisuals = managersGO.AddComponent<HUDEnvironmentVisuals>();
            envVisuals.Environment = envMgr;
            envVisuals.ClockRenderer = clockSR;
            envVisuals.ClockSprites = clockFrames;
            envVisuals.WeatherTVRenderer = tvClimaSR;
            envVisuals.WeatherTVSunny = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tv_clima_frames/tv_clima_soleado.png");
            envVisuals.WeatherTVCloudy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tv_clima_frames/tv_clima_nublado.png");
            envVisuals.WeatherTVRainy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tv_clima_frames/tv_clima_lloviendo.png");
            envVisuals.WeatherTVStormy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tv_clima_frames/tv_clima_tormenta.png");

            envVisuals.TempTVRenderer = tvTempSR;
            envVisuals.TempTVCold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tv_temperatura_frames/tv_temperatura_frio.png");
            envVisuals.TempTVNormal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tv_temperatura_frames/tv_temperatura_templado.png");
            envVisuals.TempTVHot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tv_temperatura_frames/tv_temperatura_calor.png");

            envVisuals.WindowWeatherRenderer = weatherSR;
            envVisuals.WindowWeatherSunny = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/clima_soleado.png");
            envVisuals.WindowWeatherCloudy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/clima_nublado.png");
            envVisuals.WindowWeatherRainy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/clima_lloviendo.png");
            envVisuals.WindowWeatherStormy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/clima_tormenta.png");

            envVisuals.WindowRainFrames = new Sprite[4];
            for (int f = 1; f <= 4; f++)
            {
                envVisuals.WindowRainFrames[f - 1] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Maps/clima_lloviendo_frames/clima_lloviendo_fase_{f}.png");
            }

            envVisuals.WindowStormFrames = new Sprite[4];
            for (int f = 1; f <= 4; f++)
            {
                envVisuals.WindowStormFrames[f - 1] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Maps/clima_tormenta_frames/clima_tormenta_fase_{f}.png");
            }

            // 7.1 Sistema de Partículas Ambientales de Temperatura (Frío/Calor)
            BuildAmbientTemperatureFX(envMgr);

            // Cargar los 12 Ítems
            string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Data/Items" });
            orderMgr.AvailableItems = new List<ItemData>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null) orderMgr.AvailableItems.Add(item);
            }
            orderMgr.AvailableItems.Sort((a, b) => a.ItemId.CompareTo(b.ItemId));

            // 8. Mesa Central de Trabajo y Cajas
            GameObject tableGO = new GameObject("=== MESA_ARMADO ===");
            tableGO.transform.position = new Vector3(-0.4f, -2.5f, 0);
            boxSpawner.SpawnPoint = tableGO.transform;
            boxSpawner.BoxPrefab = boxPrefab;

            // Pila de cajas sin armar (decorador visual junto a la caja)
            GameObject boxStackGO = new GameObject("Pila_Cajas_Sin_Armar");
            boxStackGO.transform.position = new Vector3(1.15f, -2.55f, 0);
            boxStackGO.transform.localScale = Vector3.one * 0.18f;
            var boxStackSR = boxStackGO.AddComponent<SpriteRenderer>();
            boxStackSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/caja_carton_sin_armar_frontal.png");
            boxStackSR.sortingOrder = 4;

            // 9. Dos Estanterías Físicas (2 Columnas x 3 Filas cada una)
            Sprite shelfSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/estanteria_3_pisos.png");

            // Estantería Izquierda (Metales y Electricidad)
            GameObject shelfLeftGO = new GameObject("Estanteria_Izquierda");
            shelfLeftGO.transform.position = new Vector3(-5.4f, -1.0f, 0);
            shelfLeftGO.transform.localScale = new Vector3(0.58f, 0.58f, 1f);
            var shelfLeftSR = shelfLeftGO.AddComponent<SpriteRenderer>();
            shelfLeftSR.sprite = shelfSprite;
            shelfLeftSR.sortingOrder = 1;

            // Estantería Derecha (Óptica y Textiles)
            GameObject shelfRightGO = new GameObject("Estanteria_Derecha");
            shelfRightGO.transform.position = new Vector3(5.4f, -1.0f, 0);
            shelfRightGO.transform.localScale = new Vector3(0.58f, 0.58f, 1f);
            var shelfRightSR = shelfRightGO.AddComponent<SpriteRenderer>();
            shelfRightSR.sprite = shelfSprite;
            shelfRightSR.sortingOrder = 1;

            // Coordenadas de casillas de items
            float[] colX = { -6.15f, -4.65f, 4.65f, 6.15f };
            float[] rowY = { 0.2f, -1.0f, -2.2f }; // r=0: Tier 3 (Arriba) | r=1: Tier 2 (Centro) | r=2: Tier 1 (Abajo)

            for (int c = 0; c < 4; c++)
            {
                Transform parentShelf = (c < 2) ? shelfLeftGO.transform : shelfRightGO.transform;
                for (int r = 0; r < 3; r++)
                {
                    int tierNumber = 3 - r; // r=0 -> t3, r=1 -> t2, r=2 -> t1
                    string targetId = $"item_f{c + 1}_t{tierNumber}";
                    var itemData = orderMgr.AvailableItems.Find(it => it.ItemId == targetId);

                    string itemName = itemData != null ? itemData.ItemName : $"Item_F{c + 1}_T{tierNumber}";
                    GameObject itemSlot = new GameObject($"ShelfItem_C{c}R{r}_{itemName}");
                    itemSlot.transform.parent = parentShelf;
                    itemSlot.transform.position = new Vector3(colX[c], rowY[r], 0);
                    itemSlot.transform.localScale = Vector3.one * 0.12f; // Escala adaptada para 1024px

                    var sr = itemSlot.AddComponent<SpriteRenderer>();
                    if (itemData != null && itemData.Icon != null)
                    {
                        sr.sprite = itemData.Icon;
                        sr.color = itemData.BaseColor;
                    }
                    sr.sortingOrder = 3;

                    var col = itemSlot.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(10.0f, 10.0f); // 1.2 unidades reales al multiplicar por escala 0.12

                    var dragSource = itemSlot.AddComponent<DraggableItemSource>();
                    dragSource.Item = itemData;
                    dragSource.VisualRenderer = sr;
                }
            }

            // 10. Bandas Transportadoras y Carteles
            Sprite conveyorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/banda_transportadora_recta.png");
            Sprite selectorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/selector_cuadrado.png");

            // Banda Izquierda: Despacho (5 Slots de espera)
            GameObject conveyorLeftGO = new GameObject("Banda_Despacho");
            conveyorLeftGO.transform.position = new Vector3(-6.1f, -4.4f, 0);
            conveyorLeftGO.transform.localScale = new Vector3(0.85f, 0.65f, 1f);
            var convLeftSR = conveyorLeftGO.AddComponent<SpriteRenderer>();
            convLeftSR.sprite = conveyorSprite;
            convLeftSR.sortingOrder = 2; // Cintas por delante de los carteles

            GameObject signDespachoGO = new GameObject("Cartel_Despacho");
            signDespachoGO.transform.position = new Vector3(-3.6f, -3.4f, 0);
            signDespachoGO.transform.localScale = Vector3.one * 0.42f;
            var signDespSR = signDespachoGO.AddComponent<SpriteRenderer>();
            signDespSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/cartel_despacho.png");
            signDespSR.sortingOrder = 1; // Carteles por detrás de la cinta transportadora

            packMgr.SlotTransforms = new Transform[5];
            GameObject slotsDespachoRoot = new GameObject("Slots_Despacho");
            float[] slotX = { -8.2f, -7.2f, -6.2f, -5.2f, -4.2f };
            for (int i = 0; i < 5; i++)
            {
                GameObject slotGO = new GameObject($"Slot_Packaging_{i}");
                slotGO.transform.parent = slotsDespachoRoot.transform;
                slotGO.transform.position = new Vector3(slotX[i], -4.15f, 0);
                slotGO.transform.localScale = Vector3.one * 0.35f;

                var slotSR = slotGO.AddComponent<SpriteRenderer>();
                slotSR.sprite = selectorSprite;
                slotSR.color = new Color(1f, 1f, 1f, 0.45f); // 45% opacidad solicitada
                slotSR.sortingOrder = 3;

                var slotCol = slotGO.AddComponent<BoxCollider2D>();
                slotCol.isTrigger = true;
                slotCol.size = new Vector2(4.5f, 4.5f);

                var pSlot = slotGO.AddComponent<PackagingSlot>();
                pSlot.SlotIndex = i;
                pSlot.ZoneManager = packMgr;

                packMgr.SlotTransforms[i] = slotGO.transform;
            }

            // Banda Derecha: Revisión (Descarte con Tag TrashBin)
            GameObject conveyorRightGO = new GameObject("Banda_Revision");
            conveyorRightGO.transform.position = new Vector3(6.1f, -4.4f, 0);
            conveyorRightGO.transform.localScale = new Vector3(0.85f, 0.65f, 1f);
            var convRightSR = conveyorRightGO.AddComponent<SpriteRenderer>();
            convRightSR.sprite = conveyorSprite;
            convRightSR.sortingOrder = 2; // Cintas por delante de los carteles

            GameObject signRevisionGO = new GameObject("Cartel_Revision");
            signRevisionGO.transform.position = new Vector3(3.6f, -3.4f, 0);
            signRevisionGO.transform.localScale = Vector3.one * 0.42f;
            var signRevSR = signRevisionGO.AddComponent<SpriteRenderer>();
            signRevSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/cartel_revision.png");
            signRevSR.sortingOrder = 1; // Carteles por detrás de la cinta transportadora

            GameObject slotsRevisionRoot = new GameObject("Slots_Revision");
            GameObject revisionSlotGO = new GameObject("Slot_Revision_Descarte");
            revisionSlotGO.tag = "TrashBin";
            revisionSlotGO.transform.parent = slotsRevisionRoot.transform;
            revisionSlotGO.transform.position = new Vector3(6.1f, -4.15f, 0);
            revisionSlotGO.transform.localScale = Vector3.one * 0.35f;

            var revSlotSR = revisionSlotGO.AddComponent<SpriteRenderer>();
            revSlotSR.sprite = selectorSprite;
            revSlotSR.color = new Color(1f, 1f, 1f, 0.45f); // 45% opacidad
            revSlotSR.sortingOrder = 3;

            var revCol = revisionSlotGO.AddComponent<BoxCollider2D>();
            revCol.isTrigger = true;
            revCol.size = new Vector2(5.0f, 5.0f);

            // 11. Construir Prefab de OrderCardUI con Post-Its
            GameObject tempCard = new GameObject("OrderCardPrefab");
            var cardRT = tempCard.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(140, 140);

            var layoutElem = tempCard.AddComponent<LayoutElement>();
            layoutElem.minWidth = 140;
            layoutElem.preferredWidth = 140;
            layoutElem.minHeight = 140;
            layoutElem.preferredHeight = 140;

            var cardBG = tempCard.AddComponent<Image>();
            cardBG.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/popstick_amarillo_pequeno.png");
            cardBG.color = Color.white;

            var cardUI = tempCard.AddComponent<OrderCardUI>();
            cardUI.CardBackground = cardBG;
            cardUI.SmallYellowPostit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/popstick_amarillo_pequeno.png");
            cardUI.SmallFuchsiaPostit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/popstick_fucsia_pequeno.png");
            cardUI.BigYellowPostit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/popstick_amarillo_grande.png");
            cardUI.BigFuchsiaPostit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/popstick_fucsia_grande.png");
            cardUI.RedThumbtack = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tachuela_roja.png");
            cardUI.YellowThumbtack = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps_Assets/tachuela_amarilla.png");

            // Tachuela en el centro superior
            GameObject tackGO = new GameObject("Thumbtack");
            tackGO.transform.SetParent(tempCard.transform, false);
            var tackRT = tackGO.AddComponent<RectTransform>();
            tackRT.anchorMin = new Vector2(0.5f, 1f);
            tackRT.anchorMax = new Vector2(0.5f, 1f);
            tackRT.pivot = new Vector2(0.5f, 0.5f);
            tackRT.anchoredPosition = new Vector2(0, -6);
            tackRT.sizeDelta = new Vector2(30, 30);
            var tackImg = tackGO.AddComponent<Image>();
            tackImg.sprite = cardUI.RedThumbtack;
            cardUI.ThumbtackImage = tackImg;

            // ID / Título Text (fuente grande y legible con márgenes seguros)
            GameObject idGO = new GameObject("OrderIdText");
            idGO.transform.SetParent(tempCard.transform, false);
            var idText = idGO.AddComponent<Text>();
            idText.font = fredokaBold;
            idText.fontSize = 17;
            idText.fontStyle = FontStyle.Bold;
            idText.color = new Color(0.18f, 0.12f, 0.08f);
            idText.alignment = TextAnchor.MiddleCenter;
            var idRT = idGO.GetComponent<RectTransform>();
            idRT.anchorMin = new Vector2(0, 0.54f);
            idRT.anchorMax = new Vector2(1, 0.84f);
            idRT.offsetMin = new Vector2(10, 0);
            idRT.offsetMax = new Vector2(-10, 0);
            cardUI.OrderIdText = idText;

            // Reward Text
            GameObject rewGO = new GameObject("RewardText");
            rewGO.transform.SetParent(tempCard.transform, false);
            var rewText = rewGO.AddComponent<Text>();
            rewText.font = fredokaBold;
            rewText.fontSize = 24;
            rewText.fontStyle = FontStyle.Bold;
            rewText.color = new Color(0.12f, 0.48f, 0.18f);
            rewText.alignment = TextAnchor.MiddleCenter;
            var rewRT = rewGO.GetComponent<RectTransform>();
            rewRT.anchorMin = new Vector2(0, 0.28f);
            rewRT.anchorMax = new Vector2(1, 0.54f);
            rewRT.offsetMin = new Vector2(10, 0);
            rewRT.offsetMax = new Vector2(-10, 0);
            cardUI.RewardText = rewText;

            // Time Bar (Horizontal decreciente con padding seguro)
            GameObject timeBgGO = new GameObject("TimeBarBG");
            timeBgGO.transform.SetParent(tempCard.transform, false);
            var timeBgImg = timeBgGO.AddComponent<Image>();
            timeBgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.35f);
            var timeBgRT = timeBgGO.GetComponent<RectTransform>();
            timeBgRT.anchorMin = new Vector2(0.12f, 0.11f);
            timeBgRT.anchorMax = new Vector2(0.88f, 0.23f);
            timeBgRT.offsetMin = Vector2.zero;
            timeBgRT.offsetMax = Vector2.zero;

            GameObject timeFillGO = new GameObject("TimeBarFill");
            timeFillGO.transform.SetParent(timeBgGO.transform, false);
            var fillImg = timeFillGO.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.85f, 0.3f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            var fillRT = timeFillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            cardUI.TimeBarFill = fillImg;

            // Panel Desplegable Hover (Post-it Grande con márgenes generosos)
            GameObject detailsGO = new GameObject("DetailsPanel");
            detailsGO.transform.SetParent(tempCard.transform, false);
            var detBG = detailsGO.AddComponent<Image>();
            detBG.sprite = cardUI.BigYellowPostit;
            detBG.color = Color.white;
            cardUI.BigDetailsBackground = detBG;

            var detRT = detailsGO.GetComponent<RectTransform>();
            detRT.anchorMin = new Vector2(0.5f, 0f);
            detRT.anchorMax = new Vector2(0.5f, 0f);
            detRT.pivot = new Vector2(1f, 1f);
            detRT.anchoredPosition = new Vector2(90, -10);
            detRT.sizeDelta = new Vector2(450, 460);

            var detCanvas = detailsGO.AddComponent<Canvas>();
            detCanvas.overrideSorting = true;
            detCanvas.sortingOrder = 25;
            detailsGO.AddComponent<GraphicRaycaster>();

            // Header Título Text en el Post-it Grande (+10px)
            GameObject detHeaderGO = new GameObject("DetailsHeaderText");
            detHeaderGO.transform.SetParent(detailsGO.transform, false);
            var detHeaderText = detHeaderGO.AddComponent<Text>();
            detHeaderText.font = fredokaBold;
            detHeaderText.fontSize = 28; // +10px (era 18-20)
            detHeaderText.fontStyle = FontStyle.Bold;
            detHeaderText.color = new Color(0.18f, 0.12f, 0.08f);
            detHeaderText.alignment = TextAnchor.UpperLeft;
            detHeaderText.horizontalOverflow = HorizontalWrapMode.Wrap;
            var detHeaderRT = detHeaderGO.GetComponent<RectTransform>();
            detHeaderRT.anchorMin = new Vector2(0, 0.74f);
            detHeaderRT.anchorMax = new Vector2(1, 0.94f);
            detHeaderRT.offsetMin = new Vector2(35, 0);
            detHeaderRT.offsetMax = new Vector2(-35, -10);
            cardUI.DetailsHeaderText = detHeaderText;

            // Condición Lógica Text / Embalaje Solicitado
            GameObject condGO = new GameObject("ConditionText");
            condGO.transform.SetParent(detailsGO.transform, false);
            var condText = condGO.AddComponent<Text>();
            condText.font = fredokaBold;
            condText.fontSize = 22; // Tamaño armónico para 2-3 líneas
            condText.fontStyle = FontStyle.Bold;
            condText.color = new Color(0.16f, 0.12f, 0.08f);
            condText.alignment = TextAnchor.UpperLeft;
            condText.horizontalOverflow = HorizontalWrapMode.Wrap;
            var condRT = condGO.GetComponent<RectTransform>();
            condRT.anchorMin = new Vector2(0, 0.58f);
            condRT.anchorMax = new Vector2(1, 0.74f);
            condRT.offsetMin = new Vector2(35, 0);
            condRT.offsetMax = new Vector2(-35, -5);
            cardUI.ConditionText = condText;

            // Items Requeridos / Fórmula de Lote y Candidatos Válidos
            GameObject reqsGO = new GameObject("ItemsListText");
            reqsGO.transform.SetParent(detailsGO.transform, false);
            var reqsText = reqsGO.AddComponent<Text>();
            reqsText.font = fredokaBold;
            reqsText.fontSize = 21; // Espacio suficiente para 5-6 líneas sin cortes
            reqsText.lineSpacing = 1.05f;
            reqsText.color = new Color(0.20f, 0.15f, 0.10f);
            reqsText.alignment = TextAnchor.UpperLeft;
            reqsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            reqsText.verticalOverflow = VerticalWrapMode.Overflow;
            var reqsRT = reqsGO.GetComponent<RectTransform>();
            reqsRT.anchorMin = new Vector2(0, 0.04f);
            reqsRT.anchorMax = new Vector2(1, 0.58f);
            reqsRT.offsetMin = new Vector2(35, 8);
            reqsRT.offsetMax = new Vector2(-35, -5);
            cardUI.ItemsListText = reqsText;

            cardUI.DetailsPanel = detailsGO;
            detailsGO.SetActive(false);

            string cardPrefabPath = "Assets/Prefabs/OrderCardPrefab.prefab";
            GameObject cardPrefab = PrefabUtility.SaveAsPrefabAsset(tempCard, cardPrefabPath);
            GameObject.DestroyImmediate(tempCard);

            // 12. Canvas UI Principal
            GameObject canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCam;
            canvas.planeDistance = 9f;
            canvas.sortingOrder = 50;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 2.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Panel de Corcho: Barra Superior Derecha de Pedidos (Post-its anclados)
            GameObject orderBarGO = new GameObject("OrderBar");
            orderBarGO.transform.SetParent(canvasGO.transform, false);
            var orderBarRT = orderBarGO.AddComponent<RectTransform>();
            orderBarRT.anchorMin = new Vector2(1, 1);
            orderBarRT.anchorMax = new Vector2(1, 1);
            orderBarRT.pivot = new Vector2(1, 1);
            orderBarRT.anchoredPosition = new Vector2(-70, -32); // Con margen seguro respecto al borde derecho
            orderBarRT.sizeDelta = new Vector2(850, 150);

            var hlg = orderBarGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var orderBarUI = orderBarGO.AddComponent<OrderBarUI>();
            orderBarUI.OrdersManager = orderMgr;
            orderBarUI.OrderCardPrefab = cardPrefab;
            orderBarUI.CardsContainer = orderBarGO.transform;

            // Marco de Dinero (Esquina Superior Izquierda)
            GameObject moneyGO = new GameObject("Marco_Dinero");
            moneyGO.transform.SetParent(canvasGO.transform, false);
            var moneyRT = moneyGO.AddComponent<RectTransform>();
            moneyRT.anchorMin = new Vector2(0, 1);
            moneyRT.anchorMax = new Vector2(0, 1);
            moneyRT.pivot = new Vector2(0, 1);
            moneyRT.anchoredPosition = new Vector2(30, -20);
            moneyRT.sizeDelta = new Vector2(230, 80);

            var moneyImg = moneyGO.AddComponent<Image>();
            moneyImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/marco_dinero_plateado.png");

            var hudManager = moneyGO.AddComponent<HUDManagerUI>();
            hudManager.Economy = ecoMgr;
            hudManager.Environment = envMgr;

            GameObject moneyTxtGO = new GameObject("MoneyText");
            moneyTxtGO.transform.SetParent(moneyGO.transform, false);
            var moneyTxtRT = moneyTxtGO.AddComponent<RectTransform>();
            moneyTxtRT.anchorMin = Vector2.zero;
            moneyTxtRT.anchorMax = Vector2.one;
            moneyTxtRT.offsetMin = new Vector2(15, 0);
            moneyTxtRT.offsetMax = new Vector2(-15, 0);

            var moneyTxt = moneyTxtGO.AddComponent<Text>();
            moneyTxt.font = fredokaBold;
            moneyTxt.fontSize = 26;
            moneyTxt.fontStyle = FontStyle.Bold;
            moneyTxt.color = new Color(0.95f, 0.95f, 0.95f);
            moneyTxt.alignment = TextAnchor.MiddleCenter;
            moneyTxt.text = "$0";
            hudManager.MoneyText = moneyTxt;

            // Marco de Cuota para Jornada Infinita (Centro Superior con marco_dinero_plateado)
            GameObject quotaGO = new GameObject("Marco_Cuota_Infinita");
            quotaGO.transform.SetParent(canvasGO.transform, false);
            var quotaRT = quotaGO.AddComponent<RectTransform>();
            quotaRT.anchorMin = new Vector2(0.5f, 1f);
            quotaRT.anchorMax = new Vector2(0.5f, 1f);
            quotaRT.pivot = new Vector2(0.5f, 1f);
            quotaRT.anchoredPosition = new Vector2(0, -18);
            quotaRT.sizeDelta = new Vector2(340, 85);

            var quotaImg = quotaGO.AddComponent<Image>();
            quotaImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/marco_dinero_plateado.png");

            // 1. Ronda Text (zona superior derecha de la moneda)
            GameObject roundTxtGO = new GameObject("QuotaRoundText");
            roundTxtGO.transform.SetParent(quotaGO.transform, false);
            var roundTxtRT = roundTxtGO.AddComponent<RectTransform>();
            roundTxtRT.anchorMin = new Vector2(0.5f, 0.5f);
            roundTxtRT.anchorMax = new Vector2(0.5f, 0.5f);
            roundTxtRT.pivot = new Vector2(0.5f, 0.5f);
            roundTxtRT.anchoredPosition = new Vector2(-20, 16);
            roundTxtRT.sizeDelta = new Vector2(130, 24);
            var roundTxt = roundTxtGO.AddComponent<Text>();
            roundTxt.font = fredokaBold;
            roundTxt.fontSize = 15;
            roundTxt.fontStyle = FontStyle.Bold;
            roundTxt.color = new Color(1f, 0.85f, 0.2f);
            roundTxt.alignment = TextAnchor.MiddleCenter;
            roundTxt.text = "RONDA 1";

            // 2. Temporizador de Ronda (esquina superior derecha del marco)
            GameObject timerTxtGO = new GameObject("RoundTimerText");
            timerTxtGO.transform.SetParent(quotaGO.transform, false);
            var timerTxtRT = timerTxtGO.AddComponent<RectTransform>();
            timerTxtRT.anchorMin = new Vector2(0.5f, 0.5f);
            timerTxtRT.anchorMax = new Vector2(0.5f, 0.5f);
            timerTxtRT.pivot = new Vector2(0.5f, 0.5f);
            timerTxtRT.anchoredPosition = new Vector2(85, 16);
            timerTxtRT.sizeDelta = new Vector2(90, 24);
            var timerTxt = timerTxtGO.AddComponent<Text>();
            timerTxt.font = fredokaBold;
            timerTxt.fontSize = 15;
            timerTxt.fontStyle = FontStyle.Bold;
            timerTxt.color = Color.white;
            timerTxt.alignment = TextAnchor.MiddleCenter;
            timerTxt.text = "60s";

            // 3. Monto de Cuota (centro derecho del marco, libre de la moneda)
            GameObject quotaAmountTxtGO = new GameObject("QuotaAmountText");
            quotaAmountTxtGO.transform.SetParent(quotaGO.transform, false);
            var quotaAmountTxtRT = quotaAmountTxtGO.AddComponent<RectTransform>();
            quotaAmountTxtRT.anchorMin = new Vector2(0.5f, 0.5f);
            quotaAmountTxtRT.anchorMax = new Vector2(0.5f, 0.5f);
            quotaAmountTxtRT.pivot = new Vector2(0.5f, 0.5f);
            quotaAmountTxtRT.anchoredPosition = new Vector2(30, -12);
            quotaAmountTxtRT.sizeDelta = new Vector2(230, 32);
            var quotaAmountTxt = quotaAmountTxtGO.AddComponent<Text>();
            quotaAmountTxt.font = fredokaBold;
            quotaAmountTxt.fontSize = 22;
            quotaAmountTxt.fontStyle = FontStyle.Bold;
            quotaAmountTxt.color = Color.white;
            quotaAmountTxt.alignment = TextAnchor.MiddleCenter;
            quotaAmountTxt.text = "$0 / $500";

            // 4. Badge de Racha (flotando debajo del marco)
            GameObject streakTxtGO = new GameObject("StreakBadgeText");
            streakTxtGO.transform.SetParent(quotaGO.transform, false);
            var streakTxtRT = streakTxtGO.AddComponent<RectTransform>();
            streakTxtRT.anchorMin = new Vector2(0.5f, 0.5f);
            streakTxtRT.anchorMax = new Vector2(0.5f, 0.5f);
            streakTxtRT.pivot = new Vector2(0.5f, 0.5f);
            streakTxtRT.anchoredPosition = new Vector2(0, -62);
            streakTxtRT.sizeDelta = new Vector2(250, 26);
            var streakTxt = streakTxtGO.AddComponent<Text>();
            streakTxt.font = fredokaBold;
            streakTxt.fontSize = 15;
            streakTxt.fontStyle = FontStyle.Bold;
            streakTxt.color = new Color(1f, 0.6f, 0.1f);
            streakTxt.alignment = TextAnchor.MiddleCenter;
            streakTxt.text = "🔥 RACHA x1.5";
            streakTxtGO.SetActive(false);

            // Conectar referencias a InfiniteModeController
            infiniteCtrl.QuotaPanel = quotaGO;
            infiniteCtrl.QuotaRoundText = roundTxt;
            infiniteCtrl.RoundTimerText = timerTxt;
            infiniteCtrl.QuotaAmountText = quotaAmountTxt;
            infiniteCtrl.StreakBadgeText = streakTxt;

            // Estación de Botones de Embalaje (En la base de la mesa de armado)
            GameObject stationGO = new GameObject("PackagingStation");
            stationGO.transform.SetParent(canvasGO.transform, false);
            var stationRT = stationGO.AddComponent<RectTransform>();
            stationRT.anchorMin = new Vector2(0.5f, 0.5f);
            stationRT.anchorMax = new Vector2(0.5f, 0.5f);
            stationRT.pivot = new Vector2(0.5f, 0.5f);
            stationRT.anchoredPosition = new Vector2(-40, -420); // Base inferior de la mesa
            stationRT.sizeDelta = new Vector2(380, 110);

            var stationHLG = stationGO.AddComponent<HorizontalLayoutGroup>();
            stationHLG.spacing = 24;
            stationHLG.childAlignment = TextAnchor.MiddleCenter;
            stationHLG.childControlWidth = false;
            stationHLG.childControlHeight = false;

            var stationUI = stationGO.AddComponent<PackagingStationUI>();
            stationUI.InactiveColor = new Color(0.70f, 0.70f, 0.70f, 0.85f);
            stationUI.FoamActiveColor = Color.white;
            stationUI.ThermalActiveColor = Color.white;

            // Botón Espuma
            Sprite foamBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_embalaje_espuma.png");
            CreateArtStationButton(stationGO.transform, "FoamButton", foamBtnSprite, out Button foamBtn, out Image foamImg);
            stationUI.FoamButton = foamBtn;
            stationUI.FoamButtonImage = foamImg;

            // Botón Film Térmico
            Sprite filmBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_embalaje_film_termico.png");
            CreateArtStationButton(stationGO.transform, "ThermalFilmButton", filmBtnSprite, out Button filmBtn, out Image filmImg);
            stationUI.ThermalFilmButton = filmBtn;
            stationUI.ThermalFilmButtonImage = filmImg;

            // 13. Modal de Evaluación de Fin de Jornada (pantalla_fin_jornada)
            GameObject modalRootGO = new GameObject("DayEndSummaryModal");
            modalRootGO.transform.SetParent(canvasGO.transform, false);
            var modalRootRT = modalRootGO.AddComponent<RectTransform>();
            modalRootRT.anchorMin = Vector2.zero;
            modalRootRT.anchorMax = Vector2.one;
            modalRootRT.offsetMin = Vector2.zero;
            modalRootRT.offsetMax = Vector2.zero;

            var modalOverlayImg = modalRootGO.AddComponent<Image>();
            modalOverlayImg.color = new Color(0.04f, 0.06f, 0.09f, 0.85f);
            modalOverlayImg.raycastTarget = true;

            var modalCanvas = modalRootGO.AddComponent<Canvas>();
            modalCanvas.overrideSorting = true;
            modalCanvas.sortingOrder = 100;
            modalRootGO.AddComponent<GraphicRaycaster>();

            // Panel Ficha con sprite pantalla_fin_jornada
            GameObject cardPanelGO = new GameObject("SummaryCardPanel");
            cardPanelGO.transform.SetParent(modalRootGO.transform, false);
            var cardPanelRT = cardPanelGO.AddComponent<RectTransform>();
            cardPanelRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardPanelRT.pivot = new Vector2(0.5f, 0.5f);
            cardPanelRT.sizeDelta = new Vector2(800, 560);

            var cardPanelImg = cardPanelGO.AddComponent<Image>();
            cardPanelImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/pantalla_fin_jornada.png");
            cardPanelImg.color = Color.white;

            var dayEndUI = modalRootGO.AddComponent<DayEndSummaryUI>();
            dayEndUI.Environment = envMgr;
            dayEndUI.Economy = ecoMgr;
            dayEndUI.Orders = orderMgr;
            dayEndUI.ContentPanel = cardPanelGO;
            dayEndUI.OverlayBackground = modalOverlayImg;
            infiniteCtrl.SummaryUI = dayEndUI;

            Color darkReportColor = new Color(0.18f, 0.12f, 0.08f);

            // 1. Header Title Text (+10px)
            dayEndUI.HeaderTitleText = CreateModalText(cardPanelGO.transform, "HeaderTitleText", new Vector2(0, 215), new Vector2(700, 50), 34, fredokaBold, darkReportColor, TextAnchor.MiddleCenter);

            // 2. Stars Display Text (+10px) - Altura 80px y Overflow para soportar glifos dorados de 58px sin truncamiento
            dayEndUI.StarsDisplayText = CreateModalText(cardPanelGO.transform, "StarsDisplayText", new Vector2(0, 150), new Vector2(700, 80), 58, fredokaBold, new Color(0.95f, 0.75f, 0.10f), TextAnchor.MiddleCenter);
            dayEndUI.StarsDisplayText.verticalOverflow = VerticalWrapMode.Overflow;

            // 3. Rating Title Text (+10px)
            dayEndUI.RatingTitleText = CreateModalText(cardPanelGO.transform, "RatingTitleText", new Vector2(0, 100), new Vector2(700, 40), 32, fredokaBold, darkReportColor, TextAnchor.MiddleCenter);

            // 4. Prestige Badge Text (+10px)
            dayEndUI.PrestigeBadgeText = CreateModalText(cardPanelGO.transform, "PrestigeBadgeText", new Vector2(0, 60), new Vector2(700, 32), 27, fredokaBold, new Color(0.2f, 0.5f, 0.7f), TextAnchor.MiddleCenter);

            // 5. Financial Breakdown Box
            GameObject breakdownBoxGO = new GameObject("BreakdownBox");
            breakdownBoxGO.transform.SetParent(cardPanelGO.transform, false);
            var boxRT = breakdownBoxGO.AddComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 0.5f);
            boxRT.anchorMax = new Vector2(0.5f, 0.5f);
            boxRT.pivot = new Vector2(0.5f, 0.5f);
            boxRT.anchoredPosition = new Vector2(0, -45);
            boxRT.sizeDelta = new Vector2(680, 150);

            dayEndUI.FinancialBreakdownText = CreateModalText(breakdownBoxGO.transform, "FinancialBreakdownText", Vector2.zero, new Vector2(660, 140), 27, fredokaBold, darkReportColor, TextAnchor.MiddleLeft);

            // 6. Quotas Status Text (+10px)
            dayEndUI.QuotasStatusText = CreateModalText(cardPanelGO.transform, "QuotasStatusText", new Vector2(0, -145), new Vector2(680, 45), 26, fredokaBold, new Color(0.3f, 0.3f, 0.3f), TextAnchor.MiddleCenter);

            // 7. Botones de Acción (Estilo Overpacked con texto negro carbón)
            Sprite greenBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_verde_rectangular.png");
            Sprite yellowBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_amarillo_rectangular.png");
            Color blackInk = new Color(0.1f, 0.1f, 0.1f);

            // Botón A: Siguiente Jornada / Volver a Intentar (Modo Tutorial / Base) - Izquierda
            GameObject nextDayBtnGO = new GameObject("NextDayButton");
            nextDayBtnGO.transform.SetParent(cardPanelGO.transform, false);
            var btnRT = nextDayBtnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0.5f);
            btnRT.anchorMax = new Vector2(0.5f, 0.5f);
            btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.anchoredPosition = new Vector2(-155, -215);
            btnRT.sizeDelta = new Vector2(290, 56);

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
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.color = blackInk;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.text = "VOLVER A INTENTAR";
            dayEndUI.NextDayButtonText = btnTxt;

            // Botón B: Volver a Jugar (Modo Infinito) - Izquierda
            GameObject retryInfBtnGO = new GameObject("RetryInfiniteButton");
            retryInfBtnGO.transform.SetParent(cardPanelGO.transform, false);
            var retryRT = retryInfBtnGO.AddComponent<RectTransform>();
            retryRT.anchorMin = new Vector2(0.5f, 0.5f);
            retryRT.anchorMax = new Vector2(0.5f, 0.5f);
            retryRT.pivot = new Vector2(0.5f, 0.5f);
            retryRT.anchoredPosition = new Vector2(-155, -215);
            retryRT.sizeDelta = new Vector2(290, 56);

            var retryImg = retryInfBtnGO.AddComponent<Image>();
            retryImg.sprite = greenBtnSprite;
            retryImg.color = Color.white;

            var retryBtn = retryInfBtnGO.AddComponent<Button>();
            retryBtn.targetGraphic = retryImg;
            dayEndUI.RetryInfiniteButton = retryBtn;

            GameObject retryTxtGO = new GameObject("Text");
            retryTxtGO.transform.SetParent(retryInfBtnGO.transform, false);
            var retryTxtRT = retryTxtGO.AddComponent<RectTransform>();
            retryTxtRT.anchorMin = Vector2.zero;
            retryTxtRT.anchorMax = Vector2.one;
            retryTxtRT.offsetMin = Vector2.zero;
            retryTxtRT.offsetMax = Vector2.zero;

            var retryTxt = retryTxtGO.AddComponent<Text>();
            retryTxt.font = fredokaBold;
            retryTxt.fontSize = 20;
            retryTxt.fontStyle = FontStyle.Bold;
            retryTxt.color = blackInk;
            retryTxt.alignment = TextAnchor.MiddleCenter;
            retryTxt.text = "▶ VOLVER A JUGAR";
            retryInfBtnGO.SetActive(false);

            // Botón C: Volver al Menú Principal - Derecha
            GameObject returnMenuBtnGO = new GameObject("ReturnToMenuButton");
            returnMenuBtnGO.transform.SetParent(cardPanelGO.transform, false);
            var returnRT = returnMenuBtnGO.AddComponent<RectTransform>();
            returnRT.anchorMin = new Vector2(0.5f, 0.5f);
            returnRT.anchorMax = new Vector2(0.5f, 0.5f);
            returnRT.pivot = new Vector2(0.5f, 0.5f);
            returnRT.anchoredPosition = new Vector2(155, -215);
            returnRT.sizeDelta = new Vector2(290, 56);

            var returnImg = returnMenuBtnGO.AddComponent<Image>();
            returnImg.sprite = yellowBtnSprite;
            returnImg.color = Color.white;

            var returnBtn = returnMenuBtnGO.AddComponent<Button>();
            returnBtn.targetGraphic = returnImg;
            dayEndUI.ReturnToMenuButton = returnBtn;

            GameObject returnTxtGO = new GameObject("Text");
            returnTxtGO.transform.SetParent(returnMenuBtnGO.transform, false);
            var returnTxtRT = returnTxtGO.AddComponent<RectTransform>();
            returnTxtRT.anchorMin = Vector2.zero;
            returnTxtRT.anchorMax = Vector2.one;
            returnTxtRT.offsetMin = Vector2.zero;
            returnTxtRT.offsetMax = Vector2.zero;

            var returnTxt = returnTxtGO.AddComponent<Text>();
            returnTxt.font = fredokaBold;
            returnTxt.fontSize = 20;
            returnTxt.fontStyle = FontStyle.Bold;
            returnTxt.color = blackInk;
            returnTxt.alignment = TextAnchor.MiddleCenter;
            returnTxt.text = "🏠 MENÚ PRINCIPAL";

            modalOverlayImg.enabled = false;
            cardPanelGO.SetActive(false);

            // 14. Botón de Pausa / Opciones (Botón Cuadrado de Madera con icono de casa en el lateral derecho central)
            Sprite woodSquareBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/CozyUI/Buttons/Square/SquareButton1_wood.png");
            Sprite homeIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/icono_casa.png");

            GameObject pauseBtnGO = new GameObject("PauseHomeButton");
            pauseBtnGO.transform.SetParent(canvasGO.transform, false);
            var pauseBtnRT = pauseBtnGO.AddComponent<RectTransform>();
            pauseBtnRT.anchorMin = new Vector2(1, 0.5f);
            pauseBtnRT.anchorMax = new Vector2(1, 0.5f);
            pauseBtnRT.pivot = new Vector2(1, 0.5f);
            pauseBtnRT.anchoredPosition = new Vector2(-25, 0); // En el centro a la derecha
            pauseBtnRT.sizeDelta = new Vector2(70, 70);

            var pauseImg = pauseBtnGO.AddComponent<Image>();
            pauseImg.sprite = woodSquareBtnSprite;
            var pauseBtn = pauseBtnGO.AddComponent<Button>();
            pauseBtn.targetGraphic = pauseImg;

            GameObject homeIconGO = new GameObject("Icon_Home");
            homeIconGO.transform.SetParent(pauseBtnGO.transform, false);
            var homeIconRT = homeIconGO.AddComponent<RectTransform>();
            homeIconRT.anchorMin = new Vector2(0.5f, 0.5f);
            homeIconRT.anchorMax = new Vector2(0.5f, 0.5f);
            homeIconRT.sizeDelta = new Vector2(44, 44);
            homeIconRT.anchoredPosition = Vector2.zero;
            var homeIconImg = homeIconGO.AddComponent<Image>();
            homeIconImg.sprite = homeIconSprite;
            homeIconImg.preserveAspect = true;

            // 15. Modal de Opciones / Pausa de Gameplay
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/pantalla_fin_jornada.png");
            var optionsUI = MenuSetupTools.BuildOptionsMenuHierarchy(canvasGO.transform, fredokaBold, cardSprite, yellowBtnSprite, greenBtnSprite, isGameplay: true);

            pauseBtn.onClick.AddListener(optionsUI.OpenOptions);

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[LogiPack] ¡Escena de Gameplay 100% reconstruida e integrada con todos los assets!</b></color>");
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
            txt.fontStyle = FontStyle.Bold;
            txt.color = color;
            txt.alignment = alignment;
            return txt;
        }

        private static void CreateArtStationButton(Transform parent, string name, Sprite sprite, out Button btn, out Image img)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(165, 96);

            var layout = btnGO.AddComponent<LayoutElement>();
            layout.minWidth = 165;
            layout.preferredWidth = 165;
            layout.minHeight = 96;
            layout.preferredHeight = 96;

            img = btnGO.AddComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;

            btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
        }

        private static GameObject BuildAmbientTemperatureFX(EnvironmentManager envMgr)
        {
            GameObject fxRoot = new GameObject("[Environment_FX]");
            fxRoot.transform.position = Vector3.zero;
            var tempFX = fxRoot.AddComponent<AmbientTemperatureFX>();
            tempFX.EnvManager = envMgr;

            // 1. Partículas de Frío (Estrellas celestes de 4 puntas cayendo suavemente)
            GameObject coldGO = new GameObject("Cold_Particles");
            coldGO.transform.SetParent(fxRoot.transform, false);
            coldGO.transform.position = new Vector3(0f, 5.8f, 0f);
            coldGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Emite hacia abajo (+Z local = -Y mundo)
            var coldPS = coldGO.AddComponent<ParticleSystem>();
            var coldMain = coldPS.main;
            coldMain.startLifetime = 6.0f;
            coldMain.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.0f);
            coldMain.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.65f);
            coldMain.startColor = new Color(0.85f, 0.95f, 1.0f, 0.85f);
            coldMain.gravityModifier = 0f;
            coldMain.maxParticles = 50;
            coldMain.loop = true;
            coldMain.playOnAwake = false;
            coldMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var coldEmission = coldPS.emission;
            coldEmission.rateOverTime = 8.0f;
            coldEmission.enabled = false;

            var coldShape = coldPS.shape;
            coldShape.shapeType = ParticleSystemShapeType.Box;
            coldShape.scale = new Vector3(18f, 0.5f, 1f);

            var coldCol = coldPS.colorOverLifetime;
            coldCol.enabled = true;
            Gradient coldGrad = new Gradient();
            coldGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.85f, 0.95f, 1f), 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.85f, 0.2f), new GradientAlphaKey(0.85f, 0.8f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            coldCol.color = coldGrad;

            var coldNoise = coldPS.noise;
            coldNoise.enabled = true;
            coldNoise.strength = 0.35f;
            coldNoise.frequency = 0.3f;
            coldNoise.damping = true;

            var coldRenderer = coldGO.GetComponent<ParticleSystemRenderer>();
            coldRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            coldRenderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Material_Estrella_Frio.mat");
            coldRenderer.sortingLayerName = "Default";
            coldRenderer.sortingOrder = 3; // Por encima de estanterías (1) y cintas (2), por detrás de mesa (5), caja y pedidos (25)

            tempFX.ColdParticles = coldPS;

            // 2. Partículas de Calor (Ondas y gradiente cálido ascendiendo suavemente)
            GameObject heatGO = new GameObject("Heat_Particles");
            heatGO.transform.SetParent(fxRoot.transform, false);
            heatGO.transform.position = new Vector3(0f, -4.8f, 0f);
            heatGO.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Emite hacia arriba (+Z local = +Y mundo)
            var heatPS = heatGO.AddComponent<ParticleSystem>();
            var heatMain = heatPS.main;
            heatMain.startLifetime = 4.8f;
            heatMain.startSpeed = new ParticleSystem.MinMaxCurve(1.4f, 2.4f);
            heatMain.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            heatMain.startColor = new Color(1.0f, 0.9f, 0.5f, 0.75f);
            heatMain.gravityModifier = 0f;
            heatMain.maxParticles = 40;
            heatMain.loop = true;
            heatMain.playOnAwake = false;
            heatMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var heatEmission = heatPS.emission;
            heatEmission.rateOverTime = 12.0f;
            heatEmission.enabled = false;

            var heatShape = heatPS.shape;
            heatShape.shapeType = ParticleSystemShapeType.Box;
            heatShape.scale = new Vector3(18f, 0.5f, 1f);

            var heatCol = heatPS.colorOverLifetime;
            heatCol.enabled = true;
            Gradient heatGrad = new Gradient();
            heatGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.95f, 0.4f), 0.0f), new GradientColorKey(new Color(1f, 0.45f, 0.1f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0.7f, 0.7f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            heatCol.color = heatGrad;

            var heatNoise = heatPS.noise;
            heatNoise.enabled = true;
            heatNoise.strength = 0.5f;
            heatNoise.frequency = 0.4f;
            heatNoise.damping = true;

            var heatRenderer = heatGO.GetComponent<ParticleSystemRenderer>();
            heatRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            heatRenderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Material_Onda_Calor.mat");
            heatRenderer.sortingLayerName = "Default";
            heatRenderer.sortingOrder = 3; // Mismo plano atmosférico por detrás de la mesa y pedidos

            tempFX.HeatParticles = heatPS;

            return fxRoot;
        }
    }
}

