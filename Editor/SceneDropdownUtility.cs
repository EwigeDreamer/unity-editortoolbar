using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;

namespace ED.Editor
{
    public struct SceneData
    {
        public Scene scene;
        public int id;
        public string name;
        public string path;
        public string dropdown;
        public bool is_loaded;

        public SceneData(SceneData other) {
            scene = other.scene;
            id = other.id;
            name = other.name;
            path = other.path;
            dropdown = other.dropdown;
            is_loaded = other.is_loaded;
        }

        public bool Equals(SceneData other) {
            if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(other.path))
                return path.Equals(other.path);
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(other.name))
                return name.Equals(other.name);
            return scene == other.scene;
        }
    }

    public static class SceneDropdownUtility
    {
        public static List<SceneData> GetDropdownList() {
            var openedScenes = Enumerable
                .Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Select(s => new SceneData {
                    scene = s,
                    id = s.buildIndex,
                    name = string.IsNullOrWhiteSpace(s.path) ? "Untitled" : s.name,
                    path = s.path,
                    is_loaded = s.isLoaded
                })
                .GroupBy(d => d.name)
                .SelectMany(g => {
                    if (g.Count() < 2)
                        return g
                            .Select(d => new SceneData(d) {
                                dropdown = d.name + GetDirtyPostfix(d.scene)
                            });
                    int counter = 0;
                    return g
                        .Select(d => new SceneData(d) {
                            dropdown = $"{d.name} ({counter++})" + GetDirtyPostfix(d.scene)
                        });
                })
                .ToList();

            var buildScenes = Enumerable
                .Range(0, SceneManager.sceneCountInBuildSettings)
                .Select(i => (id: i, path: SceneUtility.GetScenePathByBuildIndex(i)))
                .Select(t => (
                    scene: SceneManager.GetSceneByBuildIndex(t.id),
                    id: t.id,
                    name: GetSceneNameFromPath(t.path),
                    path: t.path))
                .Select(t => new SceneData {
                    scene = t.scene,
                    id = t.id,
                    name = t.name,
                    path = t.path,
                    is_loaded = t.scene.isLoaded,
                    dropdown = t.id < 0 ? t.name : $"[{t.id}] {t.name}" + GetDirtyPostfix(SceneManager.GetSceneByBuildIndex(t.id))
                })
                .ToList();

            var mainDropdownScenes = buildScenes
                .Concat(openedScenes)
                .GroupBy(d => $"[{d.name}][{d.path}]")
                .Select(g => g.First())
                .ToList();

            var otherDropdownScenes = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p=>(scene: SceneManager.GetSceneByPath(p), path: p))
                .Select(t => new SceneData {
                    scene = t.scene,
                    id = t.scene.buildIndex,
                    name = GetSceneNameFromPath(t.path),
                    path = t.path,
                    is_loaded = t.scene.isLoaded,
                    dropdown = $"other/{t.path}".Replace("/Assets/", "/")
                })
                .Where(d => mainDropdownScenes.All(md => !md.Equals(d)))
                .OrderBy(d => d.path);

            var result = mainDropdownScenes
                .Concat(otherDropdownScenes);

            return result.ToList();
        }

        private static string GetSceneNameFromPath(string path) {
            if (string.IsNullOrWhiteSpace(path)) return "???";
            if ((path.LastIndexOf('.') - path.LastIndexOf('/') - 1) <= 0) return "???";
            return path.Replace('\\', '/').Substring(path.LastIndexOf('/') + 1, path.LastIndexOf('.') - path.LastIndexOf('/') - 1);
        }

        private static string GetDirtyPostfix(Scene scene) => scene.isDirty ? " *" : "";
    }
}
