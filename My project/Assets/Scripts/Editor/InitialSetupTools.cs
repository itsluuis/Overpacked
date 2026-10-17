using System.IO;
using UnityEditor;
using UnityEngine;
using LogiPack.Core;

namespace LogiPack.Editor
{
    public static class InitialSetupTools
    {
        [MenuItem("LogiPack/Construir Todo con UI")]
        public static void BuildAll()
        {
            CreateTestSetup();
            SceneSetupTools.BuildGameplayScene();
            Debug.Log("<color=green><b>[LogiPack] ¡Todo construido e integrado con éxito!</b></color>");
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
            CreateItemSO("item_f1_t1", "Tornillo Estándar", "Fijación / Metales", ItemTier.Tier1_Basic, 10, "Assets/Sprites/item_f1_t1_screw.png", ItemAttributes.Metal, Color.white);
            CreateItemSO("item_f1_t2", "Perno Galvanizado", "Fijación / Metales", ItemTier.Tier2_Medium, 18, "Assets/Sprites/item_f1_t2_galv_bolt.png", ItemAttributes.Metal, Color.white);
            CreateItemSO("item_f1_t3", "Perno de Titanio", "Fijación / Metales", ItemTier.Tier3_Advanced, 28, "Assets/Sprites/item_f1_t3_titanium_bolt.png", ItemAttributes.Metal, Color.white);

            // Familia 2: Electricidad / Herramientas (Electrical, Tool)
            CreateItemSO("item_f2_t1", "Cable Simple", "Electricidad", ItemTier.Tier1_Basic, 10, "Assets/Sprites/item_f2_t1_wire.png", ItemAttributes.Electrical | ItemAttributes.Tool, Color.white);
            CreateItemSO("item_f2_t2", "Enchufe Aislado", "Electricidad", ItemTier.Tier2_Medium, 18, "Assets/Sprites/item_f2_t2_plug.png", ItemAttributes.Electrical | ItemAttributes.Tool, Color.white);
            CreateItemSO("item_f2_t3", "Batería de Litio", "Electricidad", ItemTier.Tier3_Advanced, 28, "Assets/Sprites/item_f2_t3_battery.png", ItemAttributes.Electrical | ItemAttributes.Tool, Color.white);

            // Familia 3: Óptica / Frágiles (Fragile)
            CreateItemSO("item_f3_t1", "Vidrio Plano", "Óptica / Frágiles", ItemTier.Tier1_Basic, 10, "Assets/Sprites/item_f3_t1_glass_pane.png", ItemAttributes.Fragile, Color.white);
            CreateItemSO("item_f3_t2", "Lente Halógeno", "Óptica / Frágiles", ItemTier.Tier2_Medium, 18, "Assets/Sprites/item_f3_t2_halogen_lens.png", ItemAttributes.Fragile, Color.white);
            CreateItemSO("item_f3_t3", "Cristal de Laboratorio", "Óptica / Frágiles", ItemTier.Tier3_Advanced, 28, "Assets/Sprites/item_f3_t3_lab_crystal.png", ItemAttributes.Fragile, Color.white);

            // Familia 4: Textiles / Coberturas (Fabric)
            CreateItemSO("item_f4_t1", "Tela de Algodón", "Textiles", ItemTier.Tier1_Basic, 10, "Assets/Sprites/item_f4_t1_cotton.png", ItemAttributes.Fabric, Color.white);
            CreateItemSO("item_f4_t2", "Lona Impermeable", "Textiles", ItemTier.Tier2_Medium, 18, "Assets/Sprites/item_f4_t2_tarp.png", ItemAttributes.Fabric, Color.white);
            CreateItemSO("item_f4_t3", "Tejido Ignífugo", "Textiles", ItemTier.Tier3_Advanced, 28, "Assets/Sprites/item_f4_t3_fireproof.png", ItemAttributes.Fabric, Color.white);

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
    }
}
