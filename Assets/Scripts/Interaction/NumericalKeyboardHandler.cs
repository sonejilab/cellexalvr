using System;
using System.Linq;
using UnityEngine;

namespace CellexalVR.Interaction
{
    public class NumericalKeyboardHandler : KeyboardHandler
    {
        public override string[][] Layouts { get; protected set; } = {
            new string[] { "7", "8", "9",
                           "4", "5", "6",
                           "1", "2", "3", "Back",
                           "0", ".", "%", "Clear",
                           "Enter" }
        };

        /// <summary>
        /// Validates the currently written text.
        /// </summary>
        public void ValidateInput(string text)
        {
            // text must only contain one dot
            int indexOfSecondDot = IndexOfSecond(text, '.');
            if (indexOfSecondDot != -1)
            {
                text = text.Substring(0, indexOfSecondDot);
            }

            // nothing must be typed after a percent sign
            int indexOfPercent = text.IndexOf('%');
            if (indexOfPercent != -1)
            {
                text = text.Substring(0, indexOfPercent + 1);
            }
            SetAllOutputs(text);
        }

        private int IndexOfSecond(string s, char c)
        {
            bool firstFound = false;
            for (int i = 0; i < s.Length; ++i)
            {
                if (s[i] == c)
                {
                    if (!firstFound)
                    {
                        firstFound = true;
                    }
                    else
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public void UpdateFilterFromFilterCreator()
        {
            referenceManager.filterManager.UpdateFilterFromFilterCreator();
        }

#if UNITY_EDITOR
        public void BuildKeyboard()
        {
            OpenPrefab(out GameObject prefab, out NumericalKeyboardHandler keyboardHandler);
            if (keyboardHandler == null)
            {
                return;
            }
            base.BuildKeyboard(keyboardHandler);
            ClosePrefab(prefab);

            OnEdit = new KeyboardEvent();
            OnEdit.AddListener(ValidateInput);
            OnEdit.AddListener((string s) => referenceManager.filterManager.UpdateFilterFromFilterCreator());

            OnEnter = new KeyboardEvent();
            OnEnter.AddListener((string s) => SubmitOutput(false));
            OnEnter.AddListener((string s) => DismissKeyboard());


        }
#endif
    }
#if UNITY_EDITOR

    /// <summary>
    /// Editor class for the <see cref="NumericalKeyboardHandler"/> to add a "Build keyboard" button.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(NumericalKeyboardHandler), true)]
    [UnityEditor.CanEditMultipleObjects]
    public class NumericalKeyboardHandlerEditor : UnityEditor.Editor
    {
        private NumericalKeyboardHandler instance;

        void OnEnable()
        {
            instance = (NumericalKeyboardHandler)target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Build keyboard"))
            {

                instance.BuildKeyboard();
            }

            try
            {
                DrawDefaultInspector();
            }
            catch (ArgumentException)
            {
                // I think this happens because BuildKeyboard opens a prefab using UnityEditor.PrefabUtility.LoadPrefabContents
                // which opens a second (hidden) inspector which glitches out because it's called from OnInspectorGUI.
            }
        }
    }
#endif
}
