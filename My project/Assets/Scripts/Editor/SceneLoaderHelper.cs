using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LogiPack.Editor
{
    public static class SceneLoaderHelper
    {
        [MenuItem("LogiPack/Recargar GameplayScene")]
        public static void ReloadScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/GameplayScene.unity");
            Debug.Log("<color=cyan><b>[LogiPack] GameplayScene recargada limpiamente.</b></color>");
        }
    }
}
