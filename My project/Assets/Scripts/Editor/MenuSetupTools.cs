using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using LogiPack.UI;
using LogiPack.UI.Menu;

namespace LogiPack.Editor
{
    public static class MenuSetupTools
    {
        [MenuItem("LogiPack/Construir Menú Principal")]
        public static void BuildMainMenuScene()
        {
            string scenePath = "Assets/Scenes/MainMenuScene.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Fuente oficial Fredoka
            Font fredokaBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Font/static/Fredoka-Bold.ttf");
            if (fredokaBold == null) fredokaBold = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 0. Servicios Persistentes Centrales (AudioManager, GameCursorManager, SceneTransitionManager)
            EnsureAppCoreServices();

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

            // 2. EventSystem (New Input System)
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<InputSystemUIInputModule>();

            // 3. Fondo de la Mesa de Papeles
            GameObject bgGO = new GameObject("Background_Desk");
            bgGO.transform.position = Vector3.zero;
            var bgSR = bgGO.AddComponent<SpriteRenderer>();
            bgSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Maps/mesa_menu_papeles.png");
            bgSR.sortingOrder = -5;

            // 4. Canvas Principal
            GameObject canvasGO = new GameObject("MenuCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCam;
            canvas.planeDistance = 9f;
            canvas.sortingOrder = 10;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Controlador de Menú
            var menuController = canvasGO.AddComponent<MainMenuController>();

            // 5. Botón invisible de Clic Exterior (Mesa)
            GameObject outsideBtnGO = new GameObject("OutsideDeskClickArea");
            outsideBtnGO.transform.SetParent(canvasGO.transform, false);
            var outsideRT = outsideBtnGO.AddComponent<RectTransform>();
            outsideRT.anchorMin = Vector2.zero;
            outsideRT.anchorMax = Vector2.one;
            outsideRT.offsetMin = Vector2.zero;
            outsideRT.offsetMax = Vector2.zero;
            var outsideImg = outsideBtnGO.AddComponent<Image>();
            outsideImg.color = Color.clear; // Totalmente transparente
            var outsideBtn = outsideBtnGO.AddComponent<Button>();
            menuController.OutsideDeskClickButton = outsideBtn;

            // 6. Hemisferio Izquierdo (Logo y 3 Botones - Versión Original)
            GameObject leftHemiGO = new GameObject("LeftHemisphere");
            leftHemiGO.transform.SetParent(canvasGO.transform, false);
            var leftRT = leftHemiGO.AddComponent<RectTransform>();
            leftRT.anchorMin = new Vector2(0.5f, 0.5f);
            leftRT.anchorMax = new Vector2(0.5f, 0.5f);
            leftRT.pivot = new Vector2(0.5f, 0.5f);
            leftRT.anchoredPosition = new Vector2(-460, 0);
            leftRT.sizeDelta = new Vector2(850, 950);

            var leftCG = leftHemiGO.AddComponent<CanvasGroup>();
            menuController.LeftHemisphereGroup = leftCG;

            // Logo Overpacked (Original)
            GameObject logoGO = new GameObject("Logo_Overpacked");
            logoGO.transform.SetParent(leftHemiGO.transform, false);
            var logoRT = logoGO.AddComponent<RectTransform>();
            logoRT.anchoredPosition = new Vector2(0, 320);
            logoRT.sizeDelta = new Vector2(760, 310);
            var logoImg = logoGO.AddComponent<Image>();
            logoImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/logo_over_packed.png");
            logoImg.preserveAspect = true;

            // Sprite botón amarillo
            Sprite yellowBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_amarillo_rectangular.png");
            Sprite greenBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/boton_verde_rectangular.png");
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttoms/pantalla_fin_jornada.png");

            // Botones Principales (Originales 520x125 px, fuente 46px)
            // Botón 1: JUGAR
            menuController.PlayButton = CreateMenuButton(leftHemiGO.transform, "PlayButton", new Vector2(0, 50), new Vector2(520, 125), yellowBtnSprite, "JUGAR", fredokaBold, 46);

            // Botón 2: OPCIONES
            menuController.OptionsButton = CreateMenuButton(leftHemiGO.transform, "OptionsButton", new Vector2(0, -90), new Vector2(520, 125), yellowBtnSprite, "OPCIONES", fredokaBold, 46);

            // Botón 3: SALIR
            menuController.QuitButton = CreateMenuButton(leftHemiGO.transform, "QuitButton", new Vector2(0, -230), new Vector2(520, 125), yellowBtnSprite, "SALIR", fredokaBold, 46);

            // 7. Cuaderno Interactivo de Modos (Hemisferio Derecho - Versión Original)
            GameObject cuadernoRootGO = new GameObject("Cuaderno_Interactivo");
            cuadernoRootGO.transform.SetParent(canvasGO.transform, false);
            var cuadernoRT = cuadernoRootGO.AddComponent<RectTransform>();
            cuadernoRT.anchorMin = new Vector2(0.5f, 0.5f);
            cuadernoRT.anchorMax = new Vector2(0.5f, 0.5f);
            cuadernoRT.pivot = new Vector2(0.5f, 0.5f);
            cuadernoRT.anchoredPosition = new Vector2(460, -20);
            cuadernoRT.sizeDelta = new Vector2(1000, 750);
            cuadernoRT.localScale = new Vector3(0.84f, 0.84f, 1f);

            var cuadernoImg = cuadernoRootGO.AddComponent<Image>();
            cuadernoImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites_sheets/cuaderno_cerrado.png");
            cuadernoImg.preserveAspect = true;

            var cuadernoCtrl = cuadernoRootGO.AddComponent<CuadernoMenuController>();
            cuadernoCtrl.NotebookImage = cuadernoImg;
            cuadernoCtrl.CuadernoCerrado = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites_sheets/cuaderno_cerrado.png");
            cuadernoCtrl.CuadernoAbierto = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites_sheets/cuaderno_abierto.png");
            cuadernoCtrl.AbrirFrames = new Sprite[4];
            for (int i = 1; i <= 4; i++)
            {
                cuadernoCtrl.AbrirFrames[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Sprites_sheets/individual_frames/abrir_paso_{i}.png");
            }
            cuadernoCtrl.MainMenu = menuController;
            menuController.Cuaderno = cuadernoCtrl;

            // 8. Contenedor de Páginas Abiertas (Dentro del Cuaderno - Versión Original)
            GameObject pagesContainerGO = new GameObject("OpenPagesContainer");
            pagesContainerGO.transform.SetParent(cuadernoRootGO.transform, false);
            var pagesRT = pagesContainerGO.AddComponent<RectTransform>();
            pagesRT.anchorMin = Vector2.zero;
            pagesRT.anchorMax = Vector2.one;
            pagesRT.offsetMin = Vector2.zero;
            pagesRT.offsetMax = Vector2.zero;
            cuadernoCtrl.OpenPagesContainer = pagesContainerGO;

            Color darkInkColor = new Color(0.18f, 0.12f, 0.08f);

            // === PÁGINA IZQUIERDA ===
            // Título: "Modos de juego"
            CreateTextElement(pagesContainerGO.transform, "Title_Modos", new Vector2(-240, 240), new Vector2(400, 60), "Modos de juego", fredokaBold, 32, darkInkColor, TextAnchor.MiddleCenter);

            // Rectángulo 1: Tutorial
            GameObject tutBtnGO = new GameObject("ModeCard_Tutorial");
            tutBtnGO.transform.SetParent(pagesContainerGO.transform, false);
            var tutRT = tutBtnGO.AddComponent<RectTransform>();
            tutRT.anchoredPosition = new Vector2(-240, 110);
            tutRT.sizeDelta = new Vector2(360, 110);
            var tutImg = tutBtnGO.AddComponent<Image>();
            tutImg.color = new Color(0.96f, 0.92f, 0.82f, 0.9f);
            var tutBtn = tutBtnGO.AddComponent<Button>();
            cuadernoCtrl.TutorialButton = tutBtn;

            // Borde / Highlight Tutorial
            GameObject tutHlGO = new GameObject("Highlight");
            tutHlGO.transform.SetParent(tutBtnGO.transform, false);
            var tutHlRT = tutHlGO.AddComponent<RectTransform>();
            tutHlRT.anchorMin = Vector2.zero;
            tutHlRT.anchorMax = Vector2.one;
            tutHlRT.offsetMin = new Vector2(-4, -4);
            tutHlRT.offsetMax = new Vector2(4, 4);
            var tutHlImg = tutHlGO.AddComponent<Image>();
            tutHlImg.color = new Color(0.20f, 0.65f, 0.25f, 0.55f);
            tutHlImg.raycastTarget = false;
            tutHlGO.transform.SetAsFirstSibling();
            cuadernoCtrl.TutorialHighlight = tutHlImg;

            CreateTextElement(tutBtnGO.transform, "Text", Vector2.zero, new Vector2(340, 100), "PRÁCTICA\n<size=15>Jornada de Prueba • Turno 4 Minutos</size>", fredokaBold, 22, darkInkColor, TextAnchor.MiddleCenter);

            // Rectángulo 2: Jornada Infinita
            GameObject infBtnGO = new GameObject("ModeCard_Infinite");
            infBtnGO.transform.SetParent(pagesContainerGO.transform, false);
            var infRT = infBtnGO.AddComponent<RectTransform>();
            infRT.anchoredPosition = new Vector2(-240, -30);
            infRT.sizeDelta = new Vector2(360, 110);
            var infImg = infBtnGO.AddComponent<Image>();
            infImg.color = new Color(0.96f, 0.92f, 0.82f, 0.9f);
            var infBtn = infBtnGO.AddComponent<Button>();
            cuadernoCtrl.InfiniteButton = infBtn;

            // Borde / Highlight Infinite
            GameObject infHlGO = new GameObject("Highlight");
            infHlGO.transform.SetParent(infBtnGO.transform, false);
            var infHlRT = infHlGO.AddComponent<RectTransform>();
            infHlRT.anchorMin = Vector2.zero;
            infHlRT.anchorMax = Vector2.one;
            infHlRT.offsetMin = new Vector2(-4, -4);
            infHlRT.offsetMax = new Vector2(4, 4);
            var infHlImg = infHlGO.AddComponent<Image>();
            infHlImg.color = new Color(0.1f, 0.1f, 0.1f, 0.15f);
            infHlImg.raycastTarget = false;
            infHlGO.transform.SetAsFirstSibling();
            cuadernoCtrl.InfiniteHighlight = infHlImg;

            CreateTextElement(infBtnGO.transform, "Text", Vector2.zero, new Vector2(340, 100), "JORNADA INFINITA\n<size=15>Desafío Contrarreloj • Rondas y Racha</size>", fredokaBold, 22, darkInkColor, TextAnchor.MiddleCenter);

            // Botón Volver (Pie de página izquierda)
            cuadernoCtrl.BackButton = CreateMenuButton(pagesContainerGO.transform, "BackButton", new Vector2(-240, -180), new Vector2(220, 52), yellowBtnSprite, "◀ VOLVER", fredokaBold, 18);

            // === PÁGINA DERECHA ===
            // Título del Modo Seleccionado
            cuadernoCtrl.ModeTitleText = CreateTextElement(pagesContainerGO.transform, "ModeTitleText", new Vector2(240, 240), new Vector2(380, 50), "PRÁCTICA", fredokaBold, 28, new Color(0.15f, 0.45f, 0.18f), TextAnchor.MiddleCenter);

            // Descripción del Modo
            var descText = CreateTextElement(pagesContainerGO.transform, "ModeDescriptionText", new Vector2(240, 45), new Vector2(360, 280), CuadernoMenuController.TUTORIAL_DESC, fredokaBold, 18, darkInkColor, TextAnchor.UpperLeft);
            descText.lineSpacing = 1.15f;
            descText.verticalOverflow = VerticalWrapMode.Overflow;
            cuadernoCtrl.ModeDescriptionText = descText;

            // Botón Verde "Empecemos"
            cuadernoCtrl.StartButton = CreateMenuButton(pagesContainerGO.transform, "StartButton", new Vector2(240, -180), new Vector2(300, 65), greenBtnSprite, "EMPECEMOS ▶", fredokaBold, 24);

            pagesContainerGO.SetActive(false);

            // 9. Modal de Opciones Funcional (Sliders de volumen + Volver)
            var optionsUI = BuildOptionsMenuHierarchy(canvasGO.transform, fredokaBold, cardSprite, yellowBtnSprite, greenBtnSprite, isGameplay: false);
            menuController.OptionsUI = optionsUI;
            menuController.OptionsModal = optionsUI.ContentPanel;

            // 10. Actualizar Build Settings con ambas escenas
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/GameplayScene.unity", true)
            };

            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[LogiPack] ¡Escena MainMenuScene restaurada fielmente al diseño original con Opciones funcionales!</b></color>");
        }

        public static void EnsureAppCoreServices()
        {
            var existingCore = Object.FindFirstObjectByType<LogiPack.Audio.AudioManager>();
            if (existingCore == null)
            {
                GameObject coreGO = new GameObject("AppCore");
                var audioMgr = coreGO.AddComponent<LogiPack.Audio.AudioManager>();
                audioMgr.MenuMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/menu/The_Picking_Floor.mp3");
                audioMgr.TutorialMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/tutorial/The_Sorting_Floor.mp3");
                audioMgr.InfiniteMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Jornada infinita/Operational_Weight.mp3");

                var cursorMgr = coreGO.AddComponent<LogiPack.UI.GameCursorManager>();
                cursorMgr.DefaultCursor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/CozyUI/Cursors/128px/Cursor1_128px.png");
                cursorMgr.ClickedCursor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/CozyUI/Cursors/128px/Cursor1Clicked_128px.png");

                coreGO.AddComponent<LogiPack.Managers.SceneTransitionManager>();
            }
        }

        public static OptionsMenuUI BuildOptionsMenuHierarchy(Transform parent, Font font, Sprite cardSprite, Sprite yellowBtnSprite, Sprite greenBtnSprite, bool isGameplay)
        {
            GameObject rootGO = new GameObject("OptionsMenuManager");
            rootGO.transform.SetParent(parent, false);
            var rootRT = rootGO.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            var optionsUI = rootGO.AddComponent<OptionsMenuUI>();
            optionsUI.SetIsGameplayScene(isGameplay);

            GameObject optModalGO = new GameObject("OverlayPanel");
            optModalGO.transform.SetParent(rootGO.transform, false);
            var optModalRT = optModalGO.AddComponent<RectTransform>();
            optModalRT.anchorMin = Vector2.zero;
            optModalRT.anchorMax = Vector2.one;
            optModalRT.offsetMin = Vector2.zero;
            optModalRT.offsetMax = Vector2.zero;

            var optOverlay = optModalGO.AddComponent<Image>();
            optOverlay.color = new Color(0, 0, 0, 0.70f);

            optionsUI.ContentPanel = optModalGO;
            optionsUI.OverlayBackground = optOverlay;

            GameObject optCardGO = new GameObject("OptionsCard");
            optCardGO.transform.SetParent(optModalGO.transform, false);
            var optCardRT = optCardGO.AddComponent<RectTransform>();
            optCardRT.sizeDelta = new Vector2(620, isGameplay ? 460 : 400);
            var optCardImg = optCardGO.AddComponent<Image>();
            optCardImg.sprite = cardSprite;

            Color darkInk = new Color(0.1f, 0.1f, 0.1f, 1f);

            CreateTextElement(optCardGO.transform, "Title", new Vector2(0, isGameplay ? 165 : 135), new Vector2(500, 50), isGameplay ? "PAUSA - AJUSTES" : "AJUSTES DE JUEGO", font, 28, darkInk, TextAnchor.MiddleCenter);

            // Slider Música
            Text musicVal;
            optionsUI.MusicSlider = CreateVolumeSlider(optCardGO.transform, "MusicSliderRow", new Vector2(0, isGameplay ? 80 : 50), new Vector2(520, 45), "Música:", font, out musicVal);
            optionsUI.MusicValueText = musicVal;

            // Slider SFX
            Text sfxVal;
            optionsUI.SFXSlider = CreateVolumeSlider(optCardGO.transform, "SFXSliderRow", new Vector2(0, isGameplay ? 15 : -15), new Vector2(520, 45), "Efectos SFX:", font, out sfxVal);
            optionsUI.SFXValueText = sfxVal;

            if (isGameplay)
            {
                // Botón Verde: SEGUIR CON LA PARTIDA
                optionsUI.ResumeButton = CreateMenuButton(optCardGO.transform, "ResumeBtn", new Vector2(0, -70), new Vector2(340, 55), greenBtnSprite, "SEGUIR CON LA PARTIDA", font, 22);

                // Botón Amarillo: SALIR AL MENÚ
                optionsUI.ReturnToMenuButton = CreateMenuButton(optCardGO.transform, "ReturnMenuBtn", new Vector2(0, -145), new Vector2(340, 55), yellowBtnSprite, "SALIR AL MENÚ", font, 22);
            }
            else
            {
                // Botón Amarillo: VOLVER
                optionsUI.BackButton = CreateMenuButton(optCardGO.transform, "BackBtn", new Vector2(0, -110), new Vector2(260, 55), yellowBtnSprite, "◀ VOLVER", font, 22);
            }

            optModalGO.SetActive(false);
            return optionsUI;
        }

        public static Slider CreateVolumeSlider(Transform parent, string name, Vector2 pos, Vector2 size, string labelText, Font font, out Text valueText)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.anchoredPosition = pos;
            rootRT.sizeDelta = size;

            // Etiqueta del Slider
            GameObject lblGO = new GameObject("Label");
            lblGO.transform.SetParent(root.transform, false);
            RectTransform lblRT = lblGO.AddComponent<RectTransform>();
            lblRT.anchoredPosition = new Vector2(-size.x * 0.28f, 0);
            lblRT.sizeDelta = new Vector2(size.x * 0.40f, 35);
            Text lbl = lblGO.AddComponent<Text>();
            lbl.font = font;
            lbl.fontSize = 20;
            lbl.fontStyle = FontStyle.Bold;
            lbl.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            lbl.alignment = TextAnchor.MiddleLeft;
            lbl.text = labelText;

            // Slider GameObject
            GameObject sliderGO = new GameObject("Slider");
            sliderGO.transform.SetParent(root.transform, false);
            RectTransform sliderRT = sliderGO.AddComponent<RectTransform>();
            sliderRT.anchoredPosition = new Vector2(size.x * 0.08f, 0);
            sliderRT.sizeDelta = new Vector2(size.x * 0.42f, 22);

            // Fondo del slider
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderGO.transform, false);
            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.25f, 0.45f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGO.transform, false);
            RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.sizeDelta = new Vector2(-10, 0);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRT = fill.AddComponent<RectTransform>();
            fillRT.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.20f, 0.70f, 0.30f, 1f); // Verde fresco

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderGO.transform, false);
            var handleAreaRT = handleArea.AddComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.sizeDelta = new Vector2(-10, 0);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform handleRT = handle.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(22, 28);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = new Color(0.95f, 0.95f, 0.95f, 1f);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.fillRect = fillRT;
            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // Texto de porcentaje (ej: 100%)
            GameObject valGO = new GameObject("ValueText");
            valGO.transform.SetParent(root.transform, false);
            RectTransform valRT = valGO.AddComponent<RectTransform>();
            valRT.anchoredPosition = new Vector2(size.x * 0.42f, 0);
            valRT.sizeDelta = new Vector2(70, 35);
            valueText = valGO.AddComponent<Text>();
            valueText.font = font;
            valueText.fontSize = 20;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            valueText.alignment = TextAnchor.MiddleCenter;
            valueText.text = "100%";

            return slider;
        }

        public static Button CreateMenuButton(Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite, string label, Font font, int fontSize)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = btnGO.AddComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject txtGO = new GameObject("Text");
            txtGO.transform.SetParent(btnGO.transform, false);
            var txtRT = txtGO.AddComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;

            var txt = txtGO.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Texto negro carbón para máxima legibilidad
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = label;

            return btn;
        }

        public static Text CreateTextElement(Transform parent, string name, Vector2 pos, Vector2 size, string text, Font font, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var txt = go.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.color = color;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.text = text;

            return txt;
        }
    }
}
