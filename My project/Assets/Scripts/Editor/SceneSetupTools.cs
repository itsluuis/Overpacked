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

            tempBox.AddComponent<PackageBox>();
            tempBox.AddComponent<DraggableBox>();

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

            // Cargar Items
            string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Data/Items" });
            orderMgr.AvailableItems = new List<ItemData>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null) orderMgr.AvailableItems.Add(item);
            }

            // 5. Mesa de Armado
            GameObject tableGO = new GameObject("=== MESA_ARMADO ===");
            tableGO.transform.position = new Vector3(0f, -2.5f, 0);
            boxSpawner.SpawnPoint = tableGO.transform;
            boxSpawner.BoxPrefab = boxPrefab;

            // 6. Estantería de Items
            GameObject shelfGO = new GameObject("=== ESTANTERIA_ITEMS ===");
            Sprite screwSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item_screw.png");
            Sprite plateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item_plate.png");
            Sprite fabricSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item_fabric.png");
            Sprite wireSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item_wire.png");
            Sprite[] sprites = { screwSprite, plateSprite, fabricSprite, wireSprite };

            for (int i = 0; i < orderMgr.AvailableItems.Count; i++)
            {
                var itemData = orderMgr.AvailableItems[i];
                GameObject itemSlot = new GameObject($"ShelfItem_{itemData.ItemName}");
                itemSlot.transform.parent = shelfGO.transform;
                itemSlot.transform.position = new Vector3(-6.5f, 2.0f - (i * 1.6f), 0);

                var sr = itemSlot.AddComponent<SpriteRenderer>();
                if (i < sprites.Length && sprites[i] != null) sr.sprite = sprites[i];

                var col = itemSlot.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.6f, 1.6f);

                var dragSource = itemSlot.AddComponent<DraggableItemSource>();
                dragSource.Item = itemData;
                dragSource.VisualRenderer = sr;
            }

            // 7. Zona de Embalaje (5 Slots)
            GameObject packingGO = new GameObject("=== ZONA_EMBALAJE ===");
            packMgr.SlotTransforms = new Transform[5];
            Sprite slotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/slot_packaging.png");

            for (int i = 0; i < 5; i++)
            {
                GameObject slotGO = new GameObject($"Slot_Packaging_{i}");
                slotGO.transform.parent = packingGO.transform;
                slotGO.transform.position = new Vector3(-3.5f + (i * 1.75f), 2.2f, 0);

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

            // 8. Basurero
            GameObject trashGO = new GameObject("TrashBin");
            trashGO.transform.position = new Vector3(6.5f, -3.5f, 0);
            var trashSR = trashGO.AddComponent<SpriteRenderer>();
            trashSR.sprite = slotSprite;
            trashSR.color = new Color(0.85f, 0.25f, 0.25f);

            var trashCol = trashGO.AddComponent<BoxCollider2D>();
            trashCol.isTrigger = true;
            trashCol.size = new Vector2(2.0f, 2.0f);

            // 9. Construir Prefab de OrderCardUI
            GameObject tempCard = new GameObject("OrderCardPrefab");
            var cardRT = tempCard.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(160, 70);

            var cardBG = tempCard.AddComponent<Image>();
            cardBG.color = new Color(0.18f, 0.24f, 0.32f, 0.95f);

            var cardUI = tempCard.AddComponent<OrderCardUI>();

            // ID Text
            GameObject idGO = new GameObject("OrderIdText");
            idGO.transform.SetParent(tempCard.transform, false);
            var idText = idGO.AddComponent<Text>();
            idText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            idText.fontSize = 13;
            idText.fontStyle = FontStyle.Bold;
            idText.color = Color.white;
            idText.alignment = TextAnchor.MiddleLeft;
            var idRT = idGO.GetComponent<RectTransform>();
            idRT.anchorMin = new Vector2(0, 0.5f);
            idRT.anchorMax = new Vector2(0.6f, 1f);
            idRT.offsetMin = new Vector2(8, 0);
            idRT.offsetMax = new Vector2(0, -6);
            cardUI.OrderIdText = idText;

            // Reward Text
            GameObject rewGO = new GameObject("RewardText");
            rewGO.transform.SetParent(tempCard.transform, false);
            var rewText = rewGO.AddComponent<Text>();
            rewText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rewText.fontSize = 13;
            rewText.fontStyle = FontStyle.Bold;
            rewText.color = new Color(0.3f, 0.9f, 0.4f);
            rewText.alignment = TextAnchor.MiddleRight;
            var rewRT = rewGO.GetComponent<RectTransform>();
            rewRT.anchorMin = new Vector2(0.6f, 0.5f);
            rewRT.anchorMax = new Vector2(1f, 1f);
            rewRT.offsetMin = new Vector2(0, 0);
            rewRT.offsetMax = new Vector2(-8, -6);
            cardUI.RewardText = rewText;

            // Time Bar BG & Fill
            GameObject timeBgGO = new GameObject("TimeBarBG");
            timeBgGO.transform.SetParent(tempCard.transform, false);
            var timeBgImg = timeBgGO.AddComponent<Image>();
            timeBgImg.color = new Color(0.1f, 0.1f, 0.12f, 0.8f);
            var timeBgRT = timeBgGO.GetComponent<RectTransform>();
            timeBgRT.anchorMin = new Vector2(0, 0);
            timeBgRT.anchorMax = new Vector2(1, 0.3f);
            timeBgRT.offsetMin = new Vector2(6, 6);
            timeBgRT.offsetMax = new Vector2(-6, 0);

            GameObject timeFillGO = new GameObject("TimeBarFill");
            timeFillGO.transform.SetParent(timeBgGO.transform, false);
            var fillImg = timeFillGO.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.85f, 0.3f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0; // Izquierda a derecha
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
            detBG.color = new Color(0.1f, 0.13f, 0.18f, 0.98f);
            var detRT = detailsGO.GetComponent<RectTransform>();
            detRT.anchorMin = new Vector2(0, 0);
            detRT.anchorMax = new Vector2(1, 0);
            detRT.pivot = new Vector2(0.5f, 1f);
            detRT.anchoredPosition = new Vector2(0, -6);
            detRT.sizeDelta = new Vector2(240, 140);

            // Condición Lógica Text
            GameObject condGO = new GameObject("ConditionText");
            condGO.transform.SetParent(detailsGO.transform, false);
            var condText = condGO.AddComponent<Text>();
            condText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            condText.fontSize = 11;
            condText.fontStyle = FontStyle.Bold;
            condText.color = new Color(0.4f, 0.8f, 1f);
            condText.alignment = TextAnchor.UpperLeft;
            var condRT = condGO.GetComponent<RectTransform>();
            condRT.anchorMin = new Vector2(0, 0.6f);
            condRT.anchorMax = new Vector2(1, 1);
            condRT.offsetMin = new Vector2(8, 4);
            condRT.offsetMax = new Vector2(-8, -4);
            cardUI.ConditionText = condText;

            // Items Requeridos Text
            GameObject reqsGO = new GameObject("ItemsListText");
            reqsGO.transform.SetParent(detailsGO.transform, false);
            var reqsText = reqsGO.AddComponent<Text>();
            reqsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            reqsText.fontSize = 11;
            reqsText.color = Color.white;
            reqsText.alignment = TextAnchor.UpperLeft;
            var reqsRT = reqsGO.GetComponent<RectTransform>();
            reqsRT.anchorMin = new Vector2(0, 0);
            reqsRT.anchorMax = new Vector2(1, 0.6f);
            reqsRT.offsetMin = new Vector2(8, 4);
            reqsRT.offsetMax = new Vector2(-8, -2);
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
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Barra Superior de Pedidos (OrderBarUI)
            GameObject orderBarGO = new GameObject("OrderBar");
            orderBarGO.transform.SetParent(canvasGO.transform, false);
            var orderBarRT = orderBarGO.AddComponent<RectTransform>();
            orderBarRT.anchorMin = new Vector2(0, 1);
            orderBarRT.anchorMax = new Vector2(1, 1);
            orderBarRT.pivot = new Vector2(0.5f, 1);
            orderBarRT.anchoredPosition = new Vector2(0, -10);
            orderBarRT.sizeDelta = new Vector2(0, 80);

            var hlg = orderBarGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            var orderBarUI = orderBarGO.AddComponent<OrderBarUI>();
            orderBarUI.OrdersManager = orderMgr;
            orderBarUI.OrderCardPrefab = cardPrefab;
            orderBarUI.CardsContainer = orderBarGO.transform;

            // Panel Superior Izquierdo: HUD (Reloj, Clima, Dinero)
            GameObject hudGO = new GameObject("HUDPanel");
            hudGO.transform.SetParent(canvasGO.transform, false);
            var hudRT = hudGO.AddComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(0, 1);
            hudRT.anchorMax = new Vector2(0, 1);
            hudRT.pivot = new Vector2(0, 1);
            hudRT.anchoredPosition = new Vector2(16, -16);
            hudRT.sizeDelta = new Vector2(200, 100);

            var hudManager = hudGO.AddComponent<HUDManagerUI>();
            hudManager.Environment = envMgr;
            hudManager.Economy = ecoMgr;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Textos del HUD
            hudManager.ClockText = CreateHudText(hudGO.transform, "ClockText", new Vector2(0, 0), "🕒 06:00 AM", 15, font, Color.white);
            hudManager.WeatherText = CreateHudText(hudGO.transform, "WeatherText", new Vector2(0, -25), "🌤️ Clear", 14, font, new Color(0.9f, 0.9f, 0.5f));
            hudManager.TemperatureText = CreateHudText(hudGO.transform, "TempText", new Vector2(0, -48), "🌡️ Normal", 14, font, new Color(0.5f, 0.85f, 1f));
            hudManager.MoneyText = CreateHudText(hudGO.transform, "MoneyText", new Vector2(0, -72), "💰 $0", 16, font, new Color(0.3f, 0.95f, 0.4f));

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[LogiPack] ¡Escena de Gameplay reconstruida con UI completa de Pedidos estilo Overcooked y HUD en pantalla!</b></color>");
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
            rt.sizeDelta = new Vector2(190, 24);

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
