using UnityEngine;
using System.Collections;


namespace CellexalVR.Interaction
{
    public class OperatorKeyboardHandler : KeyboardHandler
    {
        public override string[][] Layouts { get; protected set; } = {
            new string[] { "=", "!=",
                           ">", "<",
                           ">=", "<=" }
        };



#if UNITY_EDITOR
        public void BuildKeyboard()
        {
            OpenPrefab(out GameObject prefab, out OperatorKeyboardHandler keyboardHandler);

            base.BuildKeyboard(keyboardHandler);
            ClosePrefab(prefab);
        }
#endif
    }
#if UNITY_EDITOR

    /// <summary>
    /// Editor class for the <see cref="OperatorKeyboardHandler"/> to add a "Build keyboard" button.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(OperatorKeyboardHandler), true)]
    [UnityEditor.CanEditMultipleObjects]
    public class OperatorKeyboardHandlerEditor : UnityEditor.Editor
    {
        private OperatorKeyboardHandler instance;

        void OnEnable()
        {
            instance = (OperatorKeyboardHandler)target;
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
            catch (System.ArgumentException)
            {
                // I think this happens because BuildKeyboard opens a prefab using UnityEditor.PrefabUtility.LoadPrefabContents
                // which opens a second (hidden) inspector which glitches out because it's called from OnInspectorGUI.
            }
        }
    }
#endif
}
