using UnityEditor;
using UnityEngine;

namespace LogiPack.Editor
{
    public static class SceneInspectorHelper
    {
        [MenuItem("LogiPack/Inspeccionar Estado de Escena")]
        public static void CheckScene()
        {
            var canvas = GameObject.Find("Canvas");
            var orderBar = GameObject.Find("OrderBar");
            var hud = GameObject.Find("HUDPanel");

            Debug.Log($"<color=yellow>[LogiPack Inspector]</color> Canvas: {(canvas != null ? "PRESENTE" : "AUSENTE")}, OrderBar: {(orderBar != null ? "PRESENTE" : "AUSENTE")}, HUD: {(hud != null ? "PRESENTE" : "AUSENTE")}");
        }
    }
}
