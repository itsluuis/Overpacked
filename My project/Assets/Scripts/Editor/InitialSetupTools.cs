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

            CreateItemSO("item_01", "Tornillo", "Assets/Sprites/item_screw.png", ItemAttributes.Metal | ItemAttributes.Tool);
            CreateItemSO("item_02", "Plancha de Metal", "Assets/Sprites/item_plate.png", ItemAttributes.Metal);
            CreateItemSO("item_03", "Tela Rosa", "Assets/Sprites/item_fabric.png", ItemAttributes.Fabric | ItemAttributes.Fragile);
            CreateItemSO("item_04", "Cable Electrico", "Assets/Sprites/item_wire.png", ItemAttributes.Electrical | ItemAttributes.Metal);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateItemSO(string id, string name, string spritePath, ItemAttributes attrs)
        {
            string assetPath = $"Assets/Data/Items/{id}_{name.Replace(" ", "_")}.asset";
            if (AssetDatabase.LoadAssetAtPath<ItemData>(assetPath) != null) return;

            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            item.ItemId = id;
            item.ItemName = name;
            item.Attributes = attrs;

            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sp != null)
            {
                item.Icon = sp;
            }

            AssetDatabase.CreateAsset(item, assetPath);
        }
    }
}
