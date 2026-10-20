using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using LogiPack.Core;
using LogiPack.Logic;
using LogiPack.Orders;

namespace LogiPack.Editor
{
    public static class InitialSetupTools
    {
        [MenuItem("LogiPack/Construir Todo con UI")]
        public static void BuildAll()
        {
            CreateTestSetup();
            RunSetTheoryValidation();
            SceneSetupTools.BuildGameplayScene();
            Debug.Log("<color=green><b>[LogiPack] ¡Todo construido, validado e integrado con éxito!</b></color>");
        }

        [MenuItem("LogiPack/Crear Items y Escena de Prueba")]
        public static void CreateTestSetup()
        {
            string dataPath = "Assets/Data/Items";
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            // Familia 1: Fijación / Metales (Metal)
            CreateItemSO("item_f1_t1", "Tornillo Estándar", "Fijación / Metales", ItemTier.Tier1_Basic, 10, "Assets/Art/Items/tornillo_estandar.png", ItemAttributes.Metal, Color.white);
            CreateItemSO("item_f1_t2", "Perno Galvanizado", "Fijación / Metales", ItemTier.Tier2_Medium, 18, "Assets/Art/Items/perno_galvanizado.png", ItemAttributes.Metal, Color.white);
            CreateItemSO("item_f1_t3", "Perno de Titanio", "Fijación / Metales", ItemTier.Tier3_Advanced, 28, "Assets/Art/Items/perno_titanio.png", ItemAttributes.Metal, Color.white);

            // Familia 2: Electricidad / Herramientas (Electrical, Tool)
            CreateItemSO("item_f2_t1", "Cable Simple", "Electricidad", ItemTier.Tier1_Basic, 10, "Assets/Art/Items/cable_simple.png", ItemAttributes.Electrical | ItemAttributes.Tool, Color.white);
            CreateItemSO("item_f2_t2", "Enchufe Aislado", "Electricidad", ItemTier.Tier2_Medium, 18, "Assets/Art/Items/enchufe_aislado.png", ItemAttributes.Electrical | ItemAttributes.Tool, Color.white);
            CreateItemSO("item_f2_t3", "Batería de Litio", "Electricidad", ItemTier.Tier3_Advanced, 28, "Assets/Art/Items/bateria_litio.png", ItemAttributes.Electrical | ItemAttributes.Tool, Color.white);

            // Familia 3: Óptica / Frágiles (Fragile)
            CreateItemSO("item_f3_t1", "Vidrio Plano", "Óptica / Frágiles", ItemTier.Tier1_Basic, 10, "Assets/Art/Items/vidrio_plano.png", ItemAttributes.Fragile, Color.white);
            CreateItemSO("item_f3_t2", "Lente Halógeno", "Óptica / Frágiles", ItemTier.Tier2_Medium, 18, "Assets/Art/Items/lente_halogeno.png", ItemAttributes.Fragile, Color.white);
            CreateItemSO("item_f3_t3", "Cristal de Laboratorio", "Óptica / Frágiles", ItemTier.Tier3_Advanced, 28, "Assets/Art/Items/cristal_laboratorio.png", ItemAttributes.Fragile, Color.white);

            // Familia 4: Textiles / Coberturas (Fabric)
            CreateItemSO("item_f4_t1", "Tela de Algodón", "Textiles", ItemTier.Tier1_Basic, 10, "Assets/Art/Items/tela_algodon.png", ItemAttributes.Fabric, Color.white);
            CreateItemSO("item_f4_t2", "Lona Impermeable", "Textiles", ItemTier.Tier2_Medium, 18, "Assets/Art/Items/lona_impermeable.png", ItemAttributes.Fabric, Color.white);
            CreateItemSO("item_f4_t3", "Tejido Ignífugo", "Textiles", ItemTier.Tier3_Advanced, 28, "Assets/Art/Items/tejido_ignifugo.png", ItemAttributes.Fabric, Color.white);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateItemSO(string id, string name, string family, ItemTier tier, int price, string spritePath, ItemAttributes attrs, Color baseColor)
        {
            // Asegurar que la textura se importe como Sprite 2D en Unity
            var importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            string assetPath = $"Assets/Data/Items/{id}_{name.Replace(" ", "_")}.asset";
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemData>();
                item.ItemId = id;
                item.ItemName = name;
                item.FamilyName = family;
                item.Tier = tier;
                item.BasePrice = price;
                item.Attributes = attrs;
                item.BaseColor = baseColor;
                item.Icon = sp;

                AssetDatabase.CreateAsset(item, assetPath);
            }
            else
            {
                item.ItemId = id;
                item.ItemName = name;
                item.FamilyName = family;
                item.Tier = tier;
                item.BasePrice = price;
                item.Attributes = attrs;
                item.BaseColor = baseColor;
                if (sp != null)
                {
                    item.Icon = sp;
                }
                EditorUtility.SetDirty(item);
            }
        }

        public static void RunSetTheoryValidation()
        {
            Debug.Log("<color=cyan><b>[LogiPack] Validando Modelo de Teoría de Conjuntos (Módulo 1)...</b></color>");

            // 1. Cargar el Universo de los 12 ítems
            string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Data/Items" });
            var universe = new List<ItemData>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var it = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (it != null) universe.Add(it);
            }

            if (universe.Count != 12)
            {
                Debug.LogWarning($"[LogiPack] Advertencia: El universo actual tiene {universe.Count} ítems en lugar de 12.");
            }

            // 2. Probar Unión: Metales ∪ Textiles
            var unionRule = new SetTheoryRule(
                SetOperator.Union,
                SetCriterion.CreateFamily("Metales", "Metales"),
                SetCriterion.CreateFamily("Textiles", "Textiles"),
                "Metales o Textiles"
            );
            var unionItems = unionRule.ResolveValidItems(universe);

            // 3. Probar Intersección: Óptica ∩ Eléctricos (Lente Halógeno)
            var interRule = new SetTheoryRule(
                SetOperator.Intersection,
                SetCriterion.CreateFamily("Óptica", "Óptica"),
                SetCriterion.CreateAttribute(ItemAttributes.Electrical, "Eléctricos"),
                "Óptica Eléctrica"
            );
            var interItems = interRule.ResolveValidItems(universe);

            // 4. Probar Diferencia: Electricidad − Frágiles
            var diffRule = new SetTheoryRule(
                SetOperator.Difference,
                SetCriterion.CreateFamily("Electricidad", "Eléctricos"),
                SetCriterion.CreateAttribute(ItemAttributes.Fragile, "Frágiles"),
                "Eléctricos no frágiles"
            );
            var diffItems = diffRule.ResolveValidItems(universe);

            // 5. Probar Complemento: (Metales)ᶜ
            var compRule = new SetTheoryRule(
                SetOperator.Complement,
                SetCriterion.CreateFamily("Metales", "Metales"),
                null,
                "No Metales"
            );
            var compItems = compRule.ResolveValidItems(universe);

            // 6. Probar validación de orden con O(1) y selección flexible
            var testOrder = new Order(
                "EMP-TEST",
                "Ferretería Pim Pom Pim",
                unionRule,
                unionItems,
                3,
                new EnvironmentalCondition(),
                75f,
                140,
                40
            );

            ItemData tornillo = universe.Find(i => i.ItemId == "item_f1_t1");
            ItemData tela = universe.Find(i => i.ItemId == "item_f4_t1");
            ItemData cable = universe.Find(i => i.ItemId == "item_f2_t1");

            if (tornillo != null && tela != null && cable != null)
            {
                var validPacked = new Dictionary<ItemData, int> { { tornillo, 2 }, { tela, 1 } };
                bool passValid = testOrder.MatchesItems(validPacked);

                var repeatPacked = new Dictionary<ItemData, int> { { tornillo, 3 } };
                bool passRepeat = testOrder.MatchesItems(repeatPacked);

                var illegalPacked = new Dictionary<ItemData, int> { { tornillo, 1 }, { tela, 1 }, { cable, 1 } };
                bool passRejectIllegal = !testOrder.MatchesItems(illegalPacked);

                var wrongCount = new Dictionary<ItemData, int> { { tornillo, 2 } };
                bool passRejectCount = !testOrder.MatchesItems(wrongCount);

                if (passValid && passRepeat && passRejectIllegal && passRejectCount)
                {
                    Debug.Log("<color=green><b>[LogiPack] ¡Validación de Teoría de Conjuntos: 100% EXITOSA (Unión, Intersección, Diferencia, Complemento, Cardinalidad O(1))!</b></color>");
                }
                else
                {
                    Debug.LogError("[LogiPack] Falló alguna prueba de validación de orden de empresa.");
                }
            }
        }
    }
}
