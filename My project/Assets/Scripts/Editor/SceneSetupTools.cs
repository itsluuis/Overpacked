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

            // 1. Configurar Cámara
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = 6.0f;
                mainCam.transform.position = new Vector3(0, 0, -10);
                mainCam.backgroundColor = new Color(0.12f, 0.15f, 0.20f);
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }

            // 2. EventSystem (con InputSystemUIInputModule de Unity 6)
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<InputSystemUIInputModule>();

            // 3. Crear Prefab de la Caja
            string prefabsDir = "Assets/Prefabs";
            if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

            string boxPrefabPath = "Assets/Prefabs/PackageBoxPrefab.prefab";
            GameObject tempBox = new GameObject("PackageBoxPrefab");
            var tempSR = tempBox.AddComponent<SpriteRenderer>();
            tempSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/box_package.png");

            var tempCol = tempBox.AddComponent<BoxCollider2D>();
            tempCol.size = new Vector2(2.2f, 2.2f);

            var pBox = tempBox.AddComponent<PackageBox>();
            tempBox.AddComponent<DraggableBox>();

            // Overlays visuales para materiales de embalaje
            GameObject foamGO = new GameObject("FoamOverlay");
            foamGO.transform.parent = tempBox.transform;
            foamGO.transform.localPosition = new Vector3(0, 0, -0.05f);
            foamGO.transform.localScale = Vector3.one * 1.05f;
            var foamSR = foamGO.AddComponent<SpriteRenderer>();
            foamSR.sprite = tempSR.sprite;
            foamSR.color = new Color(1.0f, 0.92f, 0.35f, 0.55f);
            foamSR.sortingOrder = 5;
            foamGO.SetActive(false);
            pBox.FoamOverlay = foamGO;

            GameObject filmGO = new GameObject("ThermalFilmOverlay");
            filmGO.transform.parent = tempBox.transform;
            filmGO.transform.localPosition = new Vector3(0, 0, -0.08f);
            filmGO.transform.localScale = Vector3.one * 1.10f;
            var filmSR = filmGO.AddComponent<SpriteRenderer>();
            filmSR.sprite = tempSR.sprite;
            filmSR.color = new Color(0.25f, 0.85f, 1.0f, 0.50f);
            filmSR.sortingOrder = 6;
            filmGO.SetActive(false);
            pBox.ThermalFilmOverlay = filmGO;

            GameObject boxPrefab = PrefabUtility.SaveAsPrefabAsset(tempBox, boxPrefabPath);
            GameObject.DestroyImmediate(tempBox);

            // 4. Managers
            GameObject managersGO = new GameObject("=== MANAGERS ===");
            var envMgr = managersGO.AddComponent<EnvironmentManager>();
            var ecoMgr = managersGO.AddComponent<EconomyManager>();
            var packMgr = managersGO.AddComponent<PackagingZoneManager>();
            var orderMgr = managersGO.AddComponent<OrderManager>();
            var boxSpawner = managersGO.AddComponent<BoxSpawner>();

            orderMgr.Environment = envMgr;
            orderMgr.Economy = ecoMgr;
            orderMgr.PackagingZone = packMgr;

            // Cargar Items y ordenar por ItemId para garantizar consistencia
            string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Data/Items" });
            orderMgr.AvailableItems = new List<ItemData>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null) orderMgr.AvailableItems.Add(item);
            }
            orderMgr.AvailableItems.Sort((a, b) => a.ItemId.CompareTo(b.ItemId));

            // 5. Mesa de Armado (Zona inferior con amplio espacio)
            GameObject tableGO = new GameObject("=== MESA_ARMADO ===");
            tableGO.transform.position = new Vector3(2.4f, -4.0f, 0);
            boxSpawner.SpawnPoint = tableGO.transform;
            boxSpawner.BoxPrefab = boxPrefab;

            // 6. Estantería de Items: 4 columnas para los 4 items, repetidas en 3 filas verticales
            GameObject shelfGO = new GameObject("=== ESTANTERIA_ITEMS ===");
            Sprite screwSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item_screw.png");
            Sprite plateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item_plate.png");
            Sprite fabricSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item_fabric.png");
            Sprite wireSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item_wire.png");
            Sprite[] itemSprites = { screwSprite, plateSprite, fabricSprite, wireSprite };

            float[] colX = { -3.6f, -1.2f, 1.2f, 3.6f };
            float[] rowY = { 2.1f, 0.7f, -0.7f };

            for (int c = 0; c < 4; c++)
            {
                var itemData = (c < orderMgr.AvailableItems.Count) ? orderMgr.AvailableItems[c] : null;
                Sprite sp = (c < itemSprites.Length) ? itemSprites[c] : null;

                for (int r = 0; r < 3; r++)
                {
                    string itemName = itemData != null ? itemData.ItemName : $"Item_{c}";
                    GameObject itemSlot = new GameObject($"ShelfItem_C{c}R{r}_{itemName}");
                    itemSlot.transform.parent = shelfGO.transform;
                    itemSlot.transform.position = new Vector3(colX[c], rowY[r], 0);

                    var sr = itemSlot.AddComponent<SpriteRenderer>();
                    if (sp != null) sr.sprite = sp;

                    var col = itemSlot.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(1.2f, 1.2f);

                    var dragSource = itemSlot.AddComponent<DraggableItemSource>();
                    dragSource.Item = itemData;
                    dragSource.VisualRenderer = sr;
                }
            }

            // 7. Zona de Embalaje (5 Slots con clara separación física entre ellos)
            GameObject packingGO = new GameObject("=== ZONA_EMBALAJE ===");
            packMgr.SlotTransforms = new Transform[5];
            Sprite slotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/slot_packaging.png");

            float[] slotX = { -8.1f, -6.15f, -4.2f, -2.25f, -0.3f };
            for (int i = 0; i < 5; i++)
            {
                GameObject slotGO = new GameObject($"Slot_Packaging_{i}");
                slotGO.transform.parent = packingGO.transform;
                slotGO.transform.position = new Vector3(slotX[i], -4.0f, 0);
                slotGO.transform.localScale = Vector3.one * 0.85f; // Separación física clara de 0.5 unidades

                var slotSR = slotGO.AddComponent<SpriteRenderer>();
                slotSR.sprite = slotSprite;
                slotSR.sortingOrder = -1;

                var slotCol = slotGO.AddComponent<BoxCollider2D>();
                slotCol.isTrigger = true;
                slotCol.size = new Vector2(1.8f, 1.8f);

                var pSlot = slotGO.AddComponent<PackagingSlot>();
                pSlot.SlotIndex = i;
                pSlot.ZoneManager = packMgr;

                packMgr.SlotTransforms[i] = slotGO.transform;
            }

            // 8. Basurero (Esquina inferior derecha)
            GameObject trashGO = new GameObject("TrashBin");
            trashGO.tag = "TrashBin";
            trashGO.transform.position = new Vector3(7.8f, -4.0f, 0);
            trashGO.transform.localScale = Vector3.one * 0.9f;
            var trashSR = trashGO.AddComponent<SpriteRenderer>();
            trashSR.sprite = slotSprite;
            trashSR.color = new Color(0.85f, 0.25f, 0.25f);

            var trashCol = trashGO.AddComponent<BoxCollider2D>();
            trashCol.isTrigger = true;
            trashCol.size = new Vector2(2.0f, 2.0f);

            // 9. Construir Prefab de OrderCardUI
            GameObject tempCard = new GameObject("OrderCardPrefab");
            var cardRT = tempCard.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(175, 75);

            var layoutElem = tempCard.AddComponent<LayoutElement>();
            layoutElem.minWidth = 175;
            layoutElem.preferredWidth = 175;
            layoutElem.flexibleWidth = 0;
            layoutElem.minHeight = 75;
            layoutElem.preferredHeight = 75;
            layoutElem.flexibleHeight = 0;

            var cardBG = tempCard.AddComponent<Image>();
            cardBG.color = new Color(0.14f, 0.18f, 0.25f, 0.95f);

            var cardUI = tempCard.AddComponent<OrderCardUI>();

            // ID Text
            GameObject idGO = new GameObject("OrderIdText");
            idGO.transform.SetParent(tempCard.transform, false);
            var idText = idGO.AddComponent<Text>();
            idText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            idText.fontSize = 17;
            idText.fontStyle = FontStyle.Bold;
            idText.color = Color.white;
            idText.alignment = TextAnchor.MiddleLeft;
            var idRT = idGO.GetComponent<RectTransform>();
            idRT.anchorMin = new Vector2(0, 0.45f);
            idRT.anchorMax = new Vector2(0.6f, 1f);
            idRT.offsetMin = new Vector2(10, 0);
            idRT.offsetMax = new Vector2(0, -6);
            cardUI.OrderIdText = idText;

            // Reward Text
            GameObject rewGO = new GameObject("RewardText");
            rewGO.transform.SetParent(tempCard.transform, false);
            var rewText = rewGO.AddComponent<Text>();
            rewText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rewText.fontSize = 17;
            rewText.fontStyle = FontStyle.Bold;
            rewText.color = new Color(0.3f, 0.95f, 0.4f);
            rewText.alignment = TextAnchor.MiddleRight;
            var rewRT = rewGO.GetComponent<RectTransform>();
            rewRT.anchorMin = new Vector2(0.6f, 0.45f);
            rewRT.anchorMax = new Vector2(1f, 1f);
            rewRT.offsetMin = new Vector2(0, 0);
            rewRT.offsetMax = new Vector2(-10, -6);
            cardUI.RewardText = rewText;

            // Time Bar BG & Fill
            GameObject timeBgGO = new GameObject("TimeBarBG");
            timeBgGO.transform.SetParent(tempCard.transform, false);
            var timeBgImg = timeBgGO.AddComponent<Image>();
            timeBgImg.color = new Color(0.1f, 0.1f, 0.14f, 0.9f);
            var timeBgRT = timeBgGO.GetComponent<RectTransform>();
            timeBgRT.anchorMin = new Vector2(0, 0);
            timeBgRT.anchorMax = new Vector2(1, 0.28f);
            timeBgRT.offsetMin = new Vector2(8, 8);
            timeBgRT.offsetMax = new Vector2(-8, 0);

            GameObject timeFillGO = new GameObject("TimeBarFill");
            timeFillGO.transform.SetParent(timeBgGO.transform, false);
            var fillImg = timeFillGO.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.85f, 0.3f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillOrigin(fillImg);
            var fillRT = timeFillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            cardUI.TimeBarFill = fillImg;

            // Hover Details Panel (Flotante hacia abajo)
            GameObject detailsGO = new GameObject("DetailsPanel");
            detailsGO.transform.SetParent(tempCard.transform, false);
            var detBG = detailsGO.AddComponent<Image>();
            detBG.color = new Color(0.08f, 0.11f, 0.16f, 0.98f);
            var detRT = detailsGO.GetComponent<RectTransform>();
            detRT.anchorMin = new Vector2(0, 0);
            detRT.anchorMax = new Vector2(1, 0);
            detRT.pivot = new Vector2(0.5f, 1f);
            detRT.anchoredPosition = new Vector2(0, -6);
            detRT.sizeDelta = new Vector2(280, 150);

            // Condición Lógica Text
            GameObject condGO = new GameObject("ConditionText");
            condGO.transform.SetParent(detailsGO.transform, false);
            var condText = condGO.AddComponent<Text>();
            condText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            condText.fontSize = 15;
            condText.fontStyle = FontStyle.Bold;
            condText.color = new Color(0.4f, 0.85f, 1f);
            condText.alignment = TextAnchor.UpperLeft;
            var condRT = condGO.GetComponent<RectTransform>();
            condRT.anchorMin = new Vector2(0, 0.55f);
            condRT.anchorMax = new Vector2(1, 1);
            condRT.offsetMin = new Vector2(10, 6);
            condRT.offsetMax = new Vector2(-10, -6);
            cardUI.ConditionText = condText;

            // Items Requeridos Text
            GameObject reqsGO = new GameObject("ItemsListText");
            reqsGO.transform.SetParent(detailsGO.transform, false);
            var reqsText = reqsGO.AddComponent<Text>();
            reqsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            reqsText.fontSize = 15;
            reqsText.color = Color.white;
            reqsText.alignment = TextAnchor.UpperLeft;
            var reqsRT = reqsGO.GetComponent<RectTransform>();
            reqsRT.anchorMin = new Vector2(0, 0);
            reqsRT.anchorMax = new Vector2(1, 0.55f);
            reqsRT.offsetMin = new Vector2(10, 6);
            reqsRT.offsetMax = new Vector2(-10, -2);
            cardUI.ItemsListText = reqsText;

            cardUI.DetailsPanel = detailsGO;
            detailsGO.SetActive(false);

            string cardPrefabPath = "Assets/Prefabs/OrderCardPrefab.prefab";
            GameObject cardPrefab = PrefabUtility.SaveAsPrefabAsset(tempCard, cardPrefabPath);
            GameObject.DestroyImmediate(tempCard);

            // 10. Canvas UI Principal
            GameObject canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 2.5f; // Genera fuentes a alta resolución para nitidez
            canvasGO.AddComponent<GraphicRaycaster>();

            // Zona Verde: Barra Superior Derecha de Pedidos (Acumulación Compacta de derecha a izquierda)
            GameObject orderBarGO = new GameObject("OrderBar");
            orderBarGO.transform.SetParent(canvasGO.transform, false);
            var orderBarRT = orderBarGO.AddComponent<RectTransform>();
            orderBarRT.anchorMin = new Vector2(1, 1);
            orderBarRT.anchorMax = new Vector2(1, 1);
            orderBarRT.pivot = new Vector2(1, 1);
            orderBarRT.anchoredPosition = new Vector2(-25, -20);
            orderBarRT.sizeDelta = new Vector2(950, 85);

            var hlg = orderBarGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var orderBarUI = orderBarGO.AddComponent<OrderBarUI>();
            orderBarUI.OrdersManager = orderMgr;
            orderBarUI.OrderCardPrefab = cardPrefab;
            orderBarUI.CardsContainer = orderBarGO.transform;

            // Zona Roja: Panel Superior Izquierdo HUD (Reloj, Dinero, Clima/Temp con Pronóstico)
            GameObject hudGO = new GameObject("HUDPanel");
            hudGO.transform.SetParent(canvasGO.transform, false);
            var hudRT = hudGO.AddComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(0, 1);
            hudRT.anchorMax = new Vector2(0, 1);
            hudRT.pivot = new Vector2(0, 1);
            hudRT.anchoredPosition = new Vector2(25, -20);
            hudRT.sizeDelta = new Vector2(460, 150);

            var hudBG = hudGO.AddComponent<Image>();
            hudBG.color = new Color(0.08f, 0.11f, 0.16f, 0.95f);

            var hudManager = hudGO.AddComponent<HUDManagerUI>();
            hudManager.Environment = envMgr;
            hudManager.Economy = ecoMgr;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Textos del HUD con tamaño nítido y legible
            hudManager.ClockText = CreateHudText(hudGO.transform, "ClockText", new Vector2(16, -10), "🕒 06:00 AM", 22, font, Color.white);
            hudManager.MoneyText = CreateHudText(hudGO.transform, "MoneyText", new Vector2(16, -42), "💰 $0", 22, font, new Color(0.3f, 0.95f, 0.4f));
            hudManager.WeatherText = CreateHudText(hudGO.transform, "WeatherText", new Vector2(16, -78), "🌤️ Despejado ☀️ (Próx: 🌧️ Lluvia)", 18, font, new Color(0.95f, 0.90f, 0.5f));
            hudManager.TemperatureText = CreateHudText(hudGO.transform, "TempText", new Vector2(16, -112), "🌡️ Templado 🍃 (Próx: ❄️ Frío)", 18, font, new Color(0.5f, 0.85f, 1f));

            // Estación de Embalaje UI (Colocada a la DERECHA de la estantería de items)
            GameObject stationGO = new GameObject("PackagingStation");
            stationGO.transform.SetParent(canvasGO.transform, false);
            var stationRT = stationGO.AddComponent<RectTransform>();
            stationRT.anchorMin = new Vector2(0.5f, 0.5f);
            stationRT.anchorMax = new Vector2(0.5f, 0.5f);
            stationRT.pivot = new Vector2(0f, 0.5f);
            stationRT.anchoredPosition = new Vector2(490, 75); // A la derecha de la columna 3 (+3.6 world x)
            stationRT.sizeDelta = new Vector2(210, 170);

            var stationBG = stationGO.AddComponent<Image>();
            stationBG.color = new Color(0.08f, 0.11f, 0.16f, 0.95f);

            var stationVLG = stationGO.AddComponent<VerticalLayoutGroup>();
            stationVLG.spacing = 14;
            stationVLG.childAlignment = TextAnchor.MiddleCenter;
            stationVLG.childControlWidth = false;
            stationVLG.childControlHeight = false;
            stationVLG.childForceExpandWidth = false;
            stationVLG.childForceExpandHeight = false;

            var stationUI = stationGO.AddComponent<PackagingStationUI>();

            // Botón Espuma
            CreateStationButton(stationGO.transform, "FoamButton", "🧽 Espuma", font, out Button foamBtn, out Image foamImg, out Text foamTxt);
            stationUI.FoamButton = foamBtn;
            stationUI.FoamButtonImage = foamImg;
            stationUI.FoamButtonText = foamTxt;

            // Botón Film Térmico
            CreateStationButton(stationGO.transform, "ThermalFilmButton", "❄️ Film Térmico", font, out Button filmBtn, out Image filmImg, out Text filmTxt);
            stationUI.ThermalFilmButton = filmBtn;
            stationUI.ThermalFilmButtonImage = filmImg;
            stationUI.ThermalFilmButtonText = filmTxt;

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[LogiPack] ¡Escena de Gameplay reconstruida con UI completa, Estación de Embalaje y HUD!</b></color>");
        }

        private static void fillOrigin(Image img)
        {
            img.fillOrigin = 0;
        }

        private static GameObject CreateStationButton(Transform parent, string name, string label, Font font, out Button btn, out Image img, out Text txt)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 52);

            var layout = btnGO.AddComponent<LayoutElement>();
            layout.minWidth = 180;
            layout.preferredWidth = 180;
            layout.minHeight = 52;
            layout.preferredHeight = 52;

            img = btnGO.AddComponent<Image>();
            img.color = new Color(0.20f, 0.25f, 0.33f, 0.95f);

            btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            txt = textGO.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 17;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = label;

            return btnGO;
        }

        private static Text CreateHudText(Transform parent, string name, Vector2 pos, string defaultText, int size, Font font, Color color)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            var rt = textGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(420, 28);

            var txt = textGO.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = size;
            txt.fontStyle = FontStyle.Bold;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.text = defaultText;
            return txt;
        }
    }
}
