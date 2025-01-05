using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace ED.Editor
{
    [InitializeOnLoad]
    public class OpenSceneDropdown
    {
        static OpenSceneDropdown() => EditorToolbarEx.AddLeft(0, OnToolbarGUI);

        private const string name = "Manage Scenes";
        private const string tooltip = "Load and Unload scenes in Editor";
        private const string icon_path = "BuildSettings.Editor.Small";

        private static Texture Icon => EditorGUIUtility.Load(icon_path) as Texture;

        private static readonly GUIContent button_label = new GUIContent(name, Icon, tooltip);

        static void OnToolbarGUI() {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            if (GUILayout.Button(button_label)) DrawPopup();
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawPopup() {
            var dropdown_menu = new GenericMenu();
            SceneDropdownUtility.GetDropdownList().ForEach(d => dropdown_menu.AddItem(
                new GUIContent(d.dropdown),
                d.is_loaded,
                Select,
                d));
            dropdown_menu.ShowAsContext();
        }

        private static void Select(object obj) {
            var data = (SceneData) obj;
            if (data.is_loaded) {
                if (SceneManager.sceneCount < 2) return;
                var scene = data.scene;
                if (scene.isDirty && !EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new Scene[] {scene})) return;
                EditorSceneManager.CloseScene(scene, true);
            }
            else {
                var path = data.path;
                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }
        }
    }
}
