using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Toolbars;

namespace ED.Editor
{
    [InitializeOnLoad]
    public static class PlaySceneDropdown
    {
        // static PlaySceneDropdown() {
        //     EditorToolbarEx.AddRight(0, OnToolbarGUI);
        //     EditorApplication.playModeStateChanged += CheckStateChange;
        // }

        private const string Name = "Play Scene";
        private const string Path = "Custom/" + Name;
        private const string Tooltip = "Load and Play selected scene";
        private const string IconPath = "d_PlayButton";
        private const string FileName = ".scene_setup_buckup.json";

        private static Texture2D Icon => EditorGUIUtility.Load(IconPath) as Texture2D;
        
        private static MainToolbarButton Button = new MainToolbarButton(new MainToolbarContent(Name, Icon, Tooltip), DrawPopup);
        
        [MainToolbarElement(Path, menuPriority = 101, defaultDockPosition = MainToolbarDockPosition.Left)]
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

        static string GetFilePath() => System.IO.Path.Combine(GetProjectPath(), FileName);
    }
}
