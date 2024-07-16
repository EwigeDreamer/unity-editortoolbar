using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;

namespace ED.Editor
{
    [InitializeOnLoad]
    public static class PlaySceneDropdown
    {
        static PlaySceneDropdown() {
            EditorToolbarEx.AddRight(0, OnToolbarGUI);
            EditorApplication.playModeStateChanged += CheckStateChange;
        }

        private const string name = "Play Scene";
        private const string tooltip = "Load and Play selected scene";
        private const string icon_path = "d_PlayButton";
        const string file_name = ".scene_setup_buckup.json";

        private static Texture Icon => EditorGUIUtility.Load(icon_path) as Texture;

        private static readonly GUIContent button_label = new(name, Icon, tooltip);

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
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SaveSetup();
            EditorSceneManager.OpenScene(data.path, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        private static void CheckStateChange(PlayModeStateChange state) {
            if (state == PlayModeStateChange.EnteredEditMode)
                RestoreSetup();
        }

        [Serializable]
        private class SetupHandler
        {
            public SetupHandler(SceneSetup[] setup) => this.setup = setup;
            public SceneSetup[] setup = null;
        }

        private static void SaveSetup() {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var handler = new SetupHandler(setup);
            var json = JsonUtility.ToJson(handler, true);
            var path = GetFilePath();
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllText(path, json);
        }

        private static void RestoreSetup() {
            var path = GetFilePath();
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            File.Delete(path);
            var handler = JsonUtility.FromJson<SetupHandler>(json);
            if (handler == null) return;
            var setup = handler.setup;
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        static string GetProjectPath() {
            string path = Application.dataPath;
            return path.Substring(0, path.Length - "/Assets".Length);
        }

        static string GetFilePath() => Path.Combine(GetProjectPath(), file_name);
    }
}
