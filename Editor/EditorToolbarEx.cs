using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityToolbarExtender;
using System.Linq;
using System;

namespace ED.Editor
{
    [InitializeOnLoad]
    public static class EditorToolbarEx
    {
        private static List<KeyValuePair<int, Action>> _leftCallbacks = new();
        private static List<KeyValuePair<int, Action>> _rightCallbacks = new();

        static EditorToolbarEx()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnLeftGUI);
            ToolbarExtender.RightToolbarGUI.Add(OnRightGUI);
        }

        private static void OnLeftGUI() {
            GUILayout.FlexibleSpace();
            foreach (var p in _leftCallbacks) {
                p.Value?.Invoke();
                GUILayout.FlexibleSpace();
            }
        }

        private static void OnRightGUI() {
            GUILayout.FlexibleSpace();
            foreach (var p in _rightCallbacks) {
                p.Value?.Invoke();
                GUILayout.FlexibleSpace();
            }
        }

        public static void AddLeft(int order, Action on_gui) {
            if (on_gui == null) return;
            _leftCallbacks.Add(new KeyValuePair<int, Action>(order, on_gui));
            _leftCallbacks = _leftCallbacks.OrderBy(p => p.Key).ToList();
        }

        public static void AddRight(int order, Action on_gui) {
            if (on_gui == null) return;
            _rightCallbacks.Add(new KeyValuePair<int, Action>(order, on_gui));
            _rightCallbacks = _rightCallbacks.OrderBy(p => p.Key).ToList();
        }
    }

}