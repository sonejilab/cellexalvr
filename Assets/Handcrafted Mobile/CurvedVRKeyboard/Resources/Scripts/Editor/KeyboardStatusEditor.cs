using CurvedVRKeyboard;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CurvedVRKeyboard {

    /// <summary>
    /// Special editor for keyboard status
    /// </summary>
    [CustomEditor(typeof(KeyboardStatus))]
    [CanEditMultipleObjects]
    public class KeyboardStatusEditor: Editor {

        #region GUI_STRINGS
        private static GUIContent OUTPUT = new GUIContent("Gameobject Output", "field receiving input from the keyboard (Text,InputField,TextMeshPro)");
        private static GUIContent OUTPUT_LENGTH = new GUIContent("Output Length", "Maximum output text length");
        private const string OUTPUT_TYPE = "Choose Output Script Type";
        #endregion


        private const string TEXT = "text";

        private KeyboardStatus keyboardStatus;
        private Component[] componentsWithText;
        private string[] scriptsNames;

        private bool notNullTargetAndChanged;
        private int currentSelected = 0;
        private int previousSelected = 0;
       
        private void Awake () {
            keyboardStatus = target as KeyboardStatus;
            ClearReflectionData();

            if(keyboardStatus.targetGameObject != null) {
                GetComponentsName();
            }
        }

        /// <summary>
        /// Recovers Components having parameter called "text" attached to target
        /// gameobject. Later it changes them to array of string used in popup
        /// </summary>
        private void GetComponentsName () {
            componentsWithText = keyboardStatus.targetGameObject.GetComponents<Component>()
                .Where(x => x.GetType().GetProperty(TEXT) != null).ToArray();
            scriptsNames = componentsWithText.Select(x => x.GetType().ToString()).ToArray<String>();
            currentSelected = 0;
            notNullTargetAndChanged = true;
        }

        public override void OnInspectorGUI () {
            keyboardStatus = target as KeyboardStatus;
            keyboardStatus.maxOutputLength = EditorGUILayout.IntField(OUTPUT_LENGTH, keyboardStatus.maxOutputLength);            
            DrawTargetGameObjectFields();
            DrawPopupList();
            keyboardStatus.isReflectionPossible = IsReflectionPossible();
            HandleValuesChanges();
        }

        private void DrawTargetGameObjectFields () {
            EditorGUI.BeginChangeCheck();
            keyboardStatus.targetGameObject = EditorGUILayout.ObjectField(OUTPUT, keyboardStatus.targetGameObject, typeof(GameObject), true) as GameObject;
            if(keyboardStatus.targetGameObject != null && EditorGUI.EndChangeCheck()) { //if not null and changed this frame
                GetComponentsName();
            }
            if(keyboardStatus.targetGameObject == null && EditorGUI.EndChangeCheck()) {// if set to null on this frame
                ClearReflectionData();
            }
        }

        private void DrawPopupList () {
            GUI.enabled = IsReflectionPossible();
            currentSelected = EditorGUILayout.Popup(OUTPUT_TYPE, currentSelected, scriptsNames);

            if(previousSelected != currentSelected) {//if popup value was changed
                notNullTargetAndChanged = true;
            }
            previousSelected = currentSelected;

            if(IsReflectionPossible() && notNullTargetAndChanged) { //if reflection is possible and popup value was changed this frame
                GetTextParameterViaReflection();
            }

        }

        private void GetTextParameterViaReflection () {
            notNullTargetAndChanged = false;
            keyboardStatus.typeHolder = componentsWithText[currentSelected];
            keyboardStatus.targetGameObject = componentsWithText[currentSelected].gameObject;
            keyboardStatus.output = (string)componentsWithText[currentSelected]
                .GetType().GetProperty(TEXT).GetValue(componentsWithText[currentSelected], null);
        }

        

        private void ClearReflectionData () {
            componentsWithText = new Component[0];
            scriptsNames = new string[0];
        }

        public bool IsReflectionPossible () {
            return keyboardStatus.targetGameObject != null && componentsWithText.Length > 0;
        }

        private void HandleValuesChanges () {
            if(GUI.changed) {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorUtility.SetDirty(keyboardStatus);
            }
        }
    }

}