using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            // 8. Contenedor de Páginas Abiertas (Dentro del Cuaderno - 3 Hojas Paginadas con Rankings)
            GameObject pagesContainerGO = new GameObject("OpenPagesContainer");
            pagesContainerGO.transform.SetParent(cuadernoRootGO.transform, false);
            var pagesRT = pagesContainerGO.AddComponent<RectTransform>();
            pagesRT.anchorMin = Vector2.zero;
            pagesRT.anchorMax = Vector2.one;
            pagesRT.offsetMin = Vector2.zero;
            pagesRT.offsetMax = Vector2.zero;
            cuadernoCtrl.OpenPagesContainer = pagesContainerGO;

            Color darkInkColor = new Color(0.13f, 0.13f, 0.13f);     // Tinta carbón #222222
            Color neutralGrayColor = new Color(0.43f, 0.41f, 0.38f); // Gris neutro #6E6862
            Color dividerColor = new Color(0.83f, 0.81f, 0.77f, 0.85f); // Divisor tenue #D5CEC5

            // ==========================================
            // HOJA 0: PRUEBA LABORAL (TUTORIAL)
            // ==========================================
            GameObject sheet0GO = new GameObject("Sheet_0_Tutorial");
            sheet0GO.transform.SetParent(pagesContainerGO.transform, false);
            var s0RT = sheet0GO.AddComponent<RectTransform>();
            s0RT.anchorMin = Vector2.zero; s0RT.anchorMax = Vector2.one;
            s0RT.offsetMin = Vector2.zero; s0RT.offsetMax = Vector2.zero;
            cuadernoCtrl.SheetTutorial = sheet0GO;

            // Página Izquierda Hoja 0
            CreateTextElement(sheet0GO.transform, "Title", new Vector2(-240, 230), new Vector2(380, 50), "PRUEBA LABORAL", fredokaBold, 28, darkInkColor, TextAnchor.MiddleCenter);
            var desc0 = CreateTextElement(sheet0GO.transform, "Description", new Vector2(-240, 45), new Vector2(360, 250), CuadernoMenuController.TUTORIAL_DESC, fredokaBold, 18, neutralGrayColor, TextAnchor.UpperLeft);
            desc0.lineSpacing = 1.18f;
            cuadernoCtrl.StartTutorialButton = CreateMenuButton(sheet0GO.transform, "StartButton", new Vector2(-240, -145), new Vector2(300, 65), greenBtnSprite, "INICIAR PRUEBA", fredokaBold, 22);

            // Página Derecha Hoja 0 (Ficha de Capacitación)
            CreateTextElement(sheet0GO.transform, "RightTitle", new Vector2(240, 230), new Vector2(380, 50), "SUPERVISIÓN: MOCHI MILES", fredokaBold, 24, darkInkColor, TextAnchor.MiddleCenter);
            cuadernoCtrl.TutorialStatusText = CreateTextElement(sheet0GO.transform, "StatusText", new Vector2(240, 160), new Vector2(380, 40), "ESTADO: PENDIENTE [!]", fredokaBold, 19, neutralGrayColor, TextAnchor.MiddleCenter);

            CreateDividerLine(sheet0GO.transform, new Vector2(240, 135), 365f, 1.5f, dividerColor);

            CreateTextElement(sheet0GO.transform, "TemarioTitle", new Vector2(240, 105), new Vector2(360, 35), "Temario de Competencias Lógicas:", fredokaBold, 20, darkInkColor, TextAnchor.MiddleLeft);
            string temarioLines =
                "1. Conjunción: p ∧ q\n" +
                "2. Negación y Disyunción: ~q ∨ p\n" +
                "3. El Condicional: p → s\n" +
                "4. Sub-cajas Anidadas: (p ∧ q) → s\n" +
                "5. El Bicondicional: (p ∨ r) ↔ s";
            var temarioTxt = CreateTextElement(sheet0GO.transform, "TemarioContent", new Vector2(240, -15), new Vector2(360, 180), temarioLines, fredokaBold, 18, neutralGrayColor, TextAnchor.UpperLeft);
            temarioTxt.lineSpacing = 1.35f;

            // ==========================================
            // HOJA 1: JORNADA NORMAL
            // ==========================================
            GameObject sheet1GO = new GameObject("Sheet_1_NormalDay");
            sheet1GO.transform.SetParent(pagesContainerGO.transform, false);
            var s1RT = sheet1GO.AddComponent<RectTransform>();
            s1RT.anchorMin = Vector2.zero; s1RT.anchorMax = Vector2.one;
            s1RT.offsetMin = Vector2.zero; s1RT.offsetMax = Vector2.zero;
            cuadernoCtrl.SheetNormalDay = sheet1GO;

            // Página Izquierda Hoja 1
            CreateTextElement(sheet1GO.transform, "Title", new Vector2(-240, 230), new Vector2(380, 50), "JORNADA NORMAL", fredokaBold, 28, darkInkColor, TextAnchor.MiddleCenter);
            var desc1 = CreateTextElement(sheet1GO.transform, "Description", new Vector2(-240, 45), new Vector2(360, 250), CuadernoMenuController.NORMAL_DAY_DESC, fredokaBold, 18, neutralGrayColor, TextAnchor.UpperLeft);
            desc1.lineSpacing = 1.18f;
            cuadernoCtrl.StartNormalDayButton = CreateMenuButton(sheet1GO.transform, "StartButton", new Vector2(-240, -145), new Vector2(300, 65), greenBtnSprite, "EMPECEMOS", fredokaBold, 22);

            // Página Derecha Hoja 1 (Top 5 Ganancias - Formato Tabular)
            CreateTextElement(sheet1GO.transform, "RightTitle", new Vector2(240, 230), new Vector2(380, 50), "MEJORES GANANCIAS (TOP 5)", fredokaBold, 24, darkInkColor, TextAnchor.MiddleCenter);

            // Fila de Encabezado Tabular
            GameObject header1GO = new GameObject("HeaderRow");
            header1GO.transform.SetParent(sheet1GO.transform, false);
            var h1RT = header1GO.AddComponent<RectTransform>();
            h1RT.anchoredPosition = new Vector2(240, 175);
            h1RT.sizeDelta = new Vector2(365, 30);
            CreateTextElement(header1GO.transform, "H_Pos", new Vector2(-160, 0), new Vector2(45, 25), "#", fredokaBold, 15, neutralGrayColor, TextAnchor.MiddleLeft);
            CreateTextElement(header1GO.transform, "H_Metric", new Vector2(-72.5f, 0), new Vector2(130, 25), "SALDO NETO", fredokaBold, 15, neutralGrayColor, TextAnchor.MiddleLeft);
            CreateTextElement(header1GO.transform, "H_Details", new Vector2(87.5f, 0), new Vector2(190, 25), "CALIFICACIÓN / FECHA", fredokaBold, 15, neutralGrayColor, TextAnchor.MiddleRight);

            CreateDividerLine(sheet1GO.transform, new Vector2(240, 155), 365f, 1.5f, dividerColor);

            cuadernoCtrl.NormalDayRows = new RankingRowUI[5];
            float[] rowY = new float[] { 115f, 60f, 5f, -50f, -105f };
            for (int r = 0; r < 5; r++)
            {
                cuadernoCtrl.NormalDayRows[r] = CreateRankingRow(sheet1GO.transform, $"Row_{r + 1}", new Vector2(240, rowY[r]), fredokaBold, darkInkColor, neutralGrayColor, dividerColor);
            }

            // ==========================================
            // HOJA 2: JORNADA INFINITA
            // ==========================================
            GameObject sheet2GO = new GameObject("Sheet_2_Infinite");
            sheet2GO.transform.SetParent(pagesContainerGO.transform, false);
            var s2RT = sheet2GO.AddComponent<RectTransform>();
            s2RT.anchorMin = Vector2.zero; s2RT.anchorMax = Vector2.one;
            s2RT.offsetMin = Vector2.zero; s2RT.offsetMax = Vector2.zero;
            cuadernoCtrl.SheetInfinite = sheet2GO;

            // Página Izquierda Hoja 2
            CreateTextElement(sheet2GO.transform, "Title", new Vector2(-240, 230), new Vector2(380, 50), "JORNADA INFINITA", fredokaBold, 28, darkInkColor, TextAnchor.MiddleCenter);
            var desc2 = CreateTextElement(sheet2GO.transform, "Description", new Vector2(-240, 45), new Vector2(360, 250), CuadernoMenuController.INFINITE_DESC, fredokaBold, 18, neutralGrayColor, TextAnchor.UpperLeft);
            desc2.lineSpacing = 1.18f;
            cuadernoCtrl.StartInfiniteButton = CreateMenuButton(sheet2GO.transform, "StartButton", new Vector2(-240, -145), new Vector2(300, 65), greenBtnSprite, "A TRABAJAR", fredokaBold, 22);

            // Página Derecha Hoja 2 (Top 5 Resistencia - Formato Tabular)
            CreateTextElement(sheet2GO.transform, "RightTitle", new Vector2(240, 230), new Vector2(380, 50), "RÉCORDS DE RESISTENCIA (TOP 5)", fredokaBold, 24, darkInkColor, TextAnchor.MiddleCenter);

            // Fila de Encabezado Tabular
            GameObject header2GO = new GameObject("HeaderRow");
            header2GO.transform.SetParent(sheet2GO.transform, false);
            var h2RT = header2GO.AddComponent<RectTransform>();
            h2RT.anchoredPosition = new Vector2(240, 175);
            h2RT.sizeDelta = new Vector2(365, 30);
            CreateTextElement(header2GO.transform, "H_Pos", new Vector2(-160, 0), new Vector2(45, 25), "#", fredokaBold, 15, neutralGrayColor, TextAnchor.MiddleLeft);
            CreateTextElement(header2GO.transform, "H_Metric", new Vector2(-72.5f, 0), new Vector2(130, 25), "RONDA MÁX.", fredokaBold, 15, neutralGrayColor, TextAnchor.MiddleLeft);
            CreateTextElement(header2GO.transform, "H_Details", new Vector2(87.5f, 0), new Vector2(190, 25), "TOTAL / FECHA", fredokaBold, 15, neutralGrayColor, TextAnchor.MiddleRight);

            CreateDividerLine(sheet2GO.transform, new Vector2(240, 155), 365f, 1.5f, dividerColor);

            cuadernoCtrl.InfiniteRows = new RankingRowUI[5];
            for (int r = 0; r < 5; r++)
            {
                cuadernoCtrl.InfiniteRows[r] = CreateRankingRow(sheet2GO.transform, $"Row_{r + 1}", new Vector2(240, rowY[r]), fredokaBold, darkInkColor, neutralGrayColor, dividerColor);
            }

            // ==========================================
            // HOJA 3: MODO CHECK-OUT (AUDITORÍA DE TABLAS DE VERDAD)
            // ==========================================
            GameObject sheet3GO = new GameObject("Sheet_3_Checkout");
            sheet3GO.transform.SetParent(pagesContainerGO.transform, false);
            var s3RT = sheet3GO.AddComponent<RectTransform>();
            s3RT.anchorMin = Vector2.zero; s3RT.anchorMax = Vector2.one;
            s3RT.offsetMin = Vector2.zero; s3RT.offsetMax = Vector2.zero;
            cuadernoCtrl.SheetCheckout = sheet3GO;

            // Página Izquierda Hoja 3
            CreateTextElement(sheet3GO.transform, "Title", new Vector2(-240, 230), new Vector2(380, 50), "MODO CHECK-OUT", fredokaBold, 28, darkInkColor, TextAnchor.MiddleCenter);
            var desc3 = CreateTextElement(sheet3GO.transform, "Description", new Vector2(-240, 45), new Vector2(360, 250), CuadernoMenuController.CHECKOUT_DESC, fredokaBold, 18, neutralGrayColor, TextAnchor.UpperLeft);
            desc3.lineSpacing = 1.18f;
            cuadernoCtrl.StartCheckoutButton = CreateMenuButton(sheet3GO.transform, "StartButton", new Vector2(-240, -145), new Vector2(300, 65), greenBtnSprite, "INICIAR AUDITORÍA", fredokaBold, 22);

            // Página Derecha Hoja 3 (Top 5 Auditoría - Formato Tabular)
            CreateTextElement(sheet3GO.transform, "RightTitle", new Vector2(240, 230), new Vector2(380, 50), "RÉCORDS DE AUDITORÍA (TOP 5)", fredokaBold, 24, darkInkColor, TextAnchor.MiddleCenter);

            // Fila de Encabezado Tabular
            GameObject header3GO = new GameObject("HeaderRow");
            header3GO.transform.SetParent(sheet3GO.transform, false);
            var h3RT = header3GO.AddComponent<RectTransform>();
            h3RT.anchoredPosition = new Vector2(240, 175);
            h3RT.sizeDelta = new Vector2(365, 30);
            CreateTextElement(header3GO.transform, "H_Pos", new Vector2(-160, 0), new Vector2(45, 25), "#", fredokaBold, 15, neutralGrayColor, TextAnchor.MiddleLeft);
            CreateTextElement(header3GO.transform, "H_Metric", new Vector2(-72.5f, 0), new Vector2(130, 25), "ACIERTOS", fredokaBold, 15, neutralGrayColor, TextAnchor.MiddleLeft);
            CreateTextElement(header3GO.transform, "H_Details", new Vector2(87.5f, 0), new Vector2(190, 25), "CALIFICACIÓN / FECHA", fredokaBold, 15, neutralGrayColor, TextAnchor.MiddleRight);

            CreateDividerLine(sheet3GO.transform, new Vector2(240, 155), 365f, 1.5f, dividerColor);

            cuadernoCtrl.CheckoutRows = new RankingRowUI[5];
            for (int r = 0; r < 5; r++)
            {
                cuadernoCtrl.CheckoutRows[r] = CreateRankingRow(sheet3GO.transform, $"Row_{r + 1}", new Vector2(240, rowY[r]), fredokaBold, darkInkColor, neutralGrayColor, dividerColor);
            }

            // ==========================================
            // CONTROLES INFERIORES: FLECHAS EN ESQUINAS Y VOLVER
            // ==========================================
            Sprite arrowLeftSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/CozyUI/Arrows/ArrowBrownOuline22.png");
            Sprite arrowRightSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/CozyUI/Arrows/ArrowBrownOuline23.png");

            // Flecha Izquierda (Esquina inferior exterior izquierda)
            GameObject prevBtnGO = new GameObject("Button_PrevPage");
            prevBtnGO.transform.SetParent(pagesContainerGO.transform, false);
            var prevRT = prevBtnGO.AddComponent<RectTransform>();
            prevRT.anchoredPosition = new Vector2(-430, -290);
            prevRT.sizeDelta = new Vector2(30, 42);
            var prevImg = prevBtnGO.AddComponent<Image>();
            prevImg.sprite = arrowLeftSpr;
            prevImg.preserveAspect = true;
            cuadernoCtrl.PrevPageButton = prevBtnGO.AddComponent<Button>();
            cuadernoCtrl.PrevPageButton.targetGraphic = prevImg;

            // Flecha Derecha (Esquina inferior exterior derecha)
            GameObject nextBtnGO = new GameObject("Button_NextPage");
            nextBtnGO.transform.SetParent(pagesContainerGO.transform, false);
            var nextRT = nextBtnGO.AddComponent<RectTransform>();
            nextRT.anchoredPosition = new Vector2(430, -290);
            nextRT.sizeDelta = new Vector2(30, 42);
            var nextImg = nextBtnGO.AddComponent<Image>();
            nextImg.sprite = arrowRightSpr;
            nextImg.preserveAspect = true;
            cuadernoCtrl.NextPageButton = nextBtnGO.AddComponent<Button>();
            cuadernoCtrl.NextPageButton.targetGraphic = nextImg;

            // Botón Volver (Pie de página izquierda, sin flechas)
            cuadernoCtrl.BackButton = CreateMenuButton(pagesContainerGO.transform, "BackButton", new Vector2(-240, -225), new Vector2(200, 50), yellowBtnSprite, "VOLVER", fredokaBold, 18);

            // ==========================================
            // OVERLAY DE ANIMACIÓN DE VOLTEO DE PÁGINA
            // ==========================================
            GameObject flipOverlayGO = new GameObject("PageFlipOverlay");
            flipOverlayGO.transform.SetParent(pagesContainerGO.transform, false);
            var flipRT = flipOverlayGO.AddComponent<RectTransform>();
            flipRT.anchorMin = Vector2.zero; flipRT.anchorMax = Vector2.one;
            flipRT.offsetMin = Vector2.zero; flipRT.offsetMax = Vector2.zero;
            var flipImg = flipOverlayGO.AddComponent<Image>();
            flipImg.raycastTarget = true;
            cuadernoCtrl.PageFlipOverlay = flipImg;
            flipOverlayGO.SetActive(false);

            // Cargar frames de volteo ordenados canónicamente por nombre (_0 a _4)
            cuadernoCtrl.PageFlipForwardSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites_sheets/cuaderno_pasar_pagina_der_izq.png")
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();

            cuadernoCtrl.PageFlipBackwardSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites_sheets/cuaderno_pasar_pagina_izq_der.png")
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();

            pagesContainerGO.SetActive(false);

            // 9. Modal de Opciones Funcional (Sliders de volumen + Volver)
            var optionsUI = BuildOptionsMenuHierarchy(canvasGO.transform, fredokaBold, cardSprite, yellowBtnSprite, greenBtnSprite, isGameplay: false);
            menuController.OptionsUI = optionsUI;
            menuController.OptionsModal = optionsUI.ContentPanel;

            // 10. Actualizar Build Settings con las tres escenas
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/GameplayScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/TutorialScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/CheckoutScene.unity", true)
            };

            EditorSceneManager.SaveScene(scene, scenePath);

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

        public static void CreateDividerLine(Transform parent, Vector2 pos, float width, float height = 1.5f, Color? color = null)
        {
            GameObject lineGO = new GameObject("DividerLine");
            lineGO.transform.SetParent(parent, false);
            var rt = lineGO.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, height);
            var img = lineGO.AddComponent<Image>();
            img.color = color ?? new Color(0.83f, 0.81f, 0.77f, 0.85f);
            img.raycastTarget = false;
        }

        public static RankingRowUI CreateRankingRow(Transform parent, string name, Vector2 pos, Font font, Color darkInkColor, Color neutralGrayColor, Color dividerColor)
        {
            GameObject rowGO = new GameObject(name);
            rowGO.transform.SetParent(parent, false);
            var rowRT = rowGO.AddComponent<RectTransform>();
            rowRT.anchoredPosition = pos;
            rowRT.sizeDelta = new Vector2(365, 40);

            // Col 1: PosText "01" (ancho 45px, alineado izq)
            var posTxt = CreateTextElement(rowGO.transform, "Pos", new Vector2(-160, 0), new Vector2(45, 36), "01", font, 18, darkInkColor, TextAnchor.MiddleLeft);
            posTxt.fontStyle = FontStyle.Normal;

            // Col 2: MetricText "$4,250" o "Ronda 8" (ancho 130px, alineado izq, bold)
            var metricTxt = CreateTextElement(rowGO.transform, "Metric", new Vector2(-72.5f, 0), new Vector2(130, 36), "---", font, 18, darkInkColor, TextAnchor.MiddleLeft);
            metricTxt.fontStyle = FontStyle.Bold;

            // Col 3: DetailsText "★★★ • 25/10 16:55" (ancho 190px, alineado der)
            var detailsTxt = CreateTextElement(rowGO.transform, "Details", new Vector2(87.5f, 0), new Vector2(190, 36), "Sin registro", font, 16, neutralGrayColor, TextAnchor.MiddleRight);
            detailsTxt.fontStyle = FontStyle.Normal;

            // Línea divisoria al pie de la fila
            CreateDividerLine(rowGO.transform, new Vector2(0, -20), 365, 1.5f, dividerColor);

            return new RankingRowUI
            {
                PosText = posTxt,
                MetricText = metricTxt,
                DetailsText = detailsTxt
            };
        }
        #region Métodos de Test en PlayMode

        [MenuItem("LogiPack/Test/Abrir Cuaderno")]
        public static void TestOpenNotebook()
        {
            var c = Object.FindAnyObjectByType<CuadernoMenuController>();
            if (c != null) c.OpenNotebook();
        }

        [MenuItem("LogiPack/Test/Ir a Hoja 0 (Tutorial)")]
        public static void TestGoToSheet0()
        {
            var c = Object.FindAnyObjectByType<CuadernoMenuController>();
            if (c != null) c.ShowSheetInstant(0);
        }

        [MenuItem("LogiPack/Test/Ir a Hoja 1 (Normal)")]
        public static void TestGoToSheet1()
        {
            var c = Object.FindAnyObjectByType<CuadernoMenuController>();
            if (c != null) c.ShowSheetInstant(1);
        }

        [MenuItem("LogiPack/Test/Ir a Hoja 2 (Infinita)")]
        public static void TestGoToSheet2()
        {
            var c = Object.FindAnyObjectByType<CuadernoMenuController>();
            if (c != null) c.ShowSheetInstant(2);
        }

        [MenuItem("LogiPack/Test/Ir a Hoja 3 (Checkout)")]
        public static void TestGoToSheet3()
        {
            var c = Object.FindAnyObjectByType<CuadernoMenuController>();
            if (c != null) c.ShowSheetInstant(3);
        }

        [MenuItem("LogiPack/Test/Click Siguiente Hoja")]
        public static void TestNextSheet()
        {
            var c = Object.FindAnyObjectByType<CuadernoMenuController>();
            if (c != null) c.OnNextPageClicked();
        }

        [MenuItem("LogiPack/Test/Click Anterior Hoja")]
        public static void TestPrevSheet()
        {
            var c = Object.FindAnyObjectByType<CuadernoMenuController>();
            if (c != null) c.OnPrevPageClicked();
        }

        #endregion
    }
}
