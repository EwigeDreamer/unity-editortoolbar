using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;

namespace ED.Editor
{
    internal static class OpenSceneDropdown
    {
        private const string Name = "Manage Scenes";
        private const string Path = "Custom/" + Name;
        private const string Tooltip = "Load and Unload scenes in Editor";
        private const string IconPath = "BuildSettings.Editor.Small";

        private static Texture2D Icon => EditorGUIUtility.Load(IconPath) as Texture2D;

        
        private static MainToolbarButton Button = new MainToolbarButton(new MainToolbarContent(Name, Icon, Tooltip), DrawPopup);
        
        [MainToolbarElement(Path, menuPriority = 100, defaultDockPosition = MainToolbarDockPosition.Left)]
        private static IEnumerable<MainToolbarElement> CreateMainToolbarButton()
        {
            Button.enabled = !EditorApplication.isPlaying;
            yield return Button;
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
