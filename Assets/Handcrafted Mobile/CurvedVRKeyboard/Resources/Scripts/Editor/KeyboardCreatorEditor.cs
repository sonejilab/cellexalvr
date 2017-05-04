using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;

namespace CurvedVRKeyboard {

    /// <summary>
    /// Special inspector for Keyboard
    /// </summary>
    [CustomEditor(typeof(KeyboardCreator))]
    [CanEditMultipleObjects]
    public class KeyboardCreatorEditor: Editor {


        #region GUI_STRINGS
        private const string PRIMARY_SETUP = "Primary setup";
        private const string MATERIALS = "Materials";
        private const string OPTIONAL_SETUP = "Optional setup";
        private const string SPACE_SETUP = "Space button setup";

        private const int SPACING_MATERIALS = 10;
        private const int SPACING_OPTIONAL_SETUP = 20;
        private const int SPACING_WARINING = 20;

        private static GUIContent RAYCASTING_SOURCE_CONTENT = new GUIContent("Raycasting source","Object from which raycasting will take place");
        private static GUIContent CURVATURE_CONTENT = new GUIContent ("Curvature(%)","Curvature of keyboard in range [0,1]");
        private static GUIContent CLICK_INPUT_COMMAND_CONTENT = new GUIContent("Click input name","Input from Edit -> ProjectSettings -> Input");

        private static GUIContent DEFAULT_MATERIAL_CONTENT = new GUIContent("Normal material","Material used with keys normal state");
        private static GUIContent SELECTED_MATERIAL_CONTENT = new GUIContent("Selected material", "Material used with keys selected state");
        private static GUIContent PRESSED_MATERIAL_CONTENT = new GUIContent("Pressed material", "Material used with keys pressed state");

        private static GUIContent SPACE_9SLICE_CONTENT = new GUIContent("9sliced sprite", "If different image or sliced sprite for space is required, set a new sprite here");
        private static GUIContent SLICE_PROPORTIONS_CONTENT = new GUIContent("Slice proportions", "Change values to adjust proper size of sprite on space");
        private static GUIContent REFRESH_SPACE_MATERIAL_BUTTON = new GUIContent("Refresh space material", "Refresh spacebar materials if keys' materials were changed");

        private const string FIND_SOURCE_BUTTON = "Raycasting source missing. Press to set default camera";
        private const string NO_CAMERA_ERROR = "Camera was not found. Add a camera to scene";
        private const string REFRESH_SPACE_UNDO = "Refresh Space";
        #endregion

        private KeyboardCreator keyboardCreator;
        private ErrorReporter errorReporter;
        private Vector3 keyboardScale;

        private bool noCameraFound = false;
        private bool isRaycastingSourceSet = false;



        private void Awake () {
            keyboardCreator = target as KeyboardCreator;

            if(!Application.isPlaying || !keyboardCreator.gameObject.isStatic) {// Always when not playing or (playing and keyboard is not static)
                keyboardCreator.wasStaticOnStart = false;
                KeyboardItem.forceInit = true;
                if(keyboardCreator.RaycastingSource != null) {
                    keyboardCreator.ManageKeys();
                }
                keyboardScale = keyboardCreator.transform.localScale;
            }
        }

        public override void OnInspectorGUI () {
            keyboardCreator = target as KeyboardCreator;
            keyboardCreator.checkErrors();
            errorReporter = ErrorReporter.Instance;

            if(errorReporter.currentStatus == ErrorReporter.Status.None || !Application.isPlaying) {// (Playing and was static at start) or always when not playing
                DrawPrimary();
                DrawMaterials();
                DrawSpace();
                GUI.enabled = true;
                CameraFinder();
                HandleValuesChanges();
            }
            DisplayWarnings();
        }

        private void DrawPrimary () {
            GUILayout.Label(PRIMARY_SETUP, EditorStyles.boldLabel);
            keyboardCreator.RaycastingSource = EditorGUILayout.ObjectField(RAYCASTING_SOURCE_CONTENT, keyboardCreator.RaycastingSource, typeof(Transform), true) as Transform;

            isRaycastingSourceSet = (keyboardCreator.RaycastingSource != null);
            GUI.enabled = isRaycastingSourceSet;

            keyboardCreator.Curvature = EditorGUILayout.Slider(CURVATURE_CONTENT, keyboardCreator.Curvature, 0f, 1f);
            

            keyboardCreator.ClickHandle = EditorGUILayout.TextField(CLICK_INPUT_COMMAND_CONTENT, keyboardCreator.ClickHandle);
        }

        private void DrawMaterials () {
            GUILayout.Space(SPACING_MATERIALS);
            GUILayout.Label(MATERIALS, EditorStyles.boldLabel);
            keyboardCreator.KeyNormalMaterial = EditorGUILayout.ObjectField(DEFAULT_MATERIAL_CONTENT, keyboardCreator.KeyNormalMaterial, typeof(Material), true) as Material;
            keyboardCreator.KeySelectedMaterial = EditorGUILayout.ObjectField(SELECTED_MATERIAL_CONTENT, keyboardCreator.KeySelectedMaterial, typeof(Material), true) as Material;
            keyboardCreator.KeyPressedMaterial = EditorGUILayout.ObjectField(PRESSED_MATERIAL_CONTENT, keyboardCreator.KeyPressedMaterial, typeof(Material), true) as Material;
        }

        private void DrawSpace () {
            GUILayout.Space(SPACING_OPTIONAL_SETUP);
            GUILayout.Label(OPTIONAL_SETUP, EditorStyles.boldLabel);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(SPACE_SETUP, EditorStyles.boldLabel);

            keyboardCreator.SpaceSprite = EditorGUILayout.ObjectField(SPACE_9SLICE_CONTENT, keyboardCreator.SpaceSprite, typeof(Sprite), true) as Sprite;

            bool isSpritePresent = keyboardCreator.SpaceSprite != null;
            GUI.enabled = isSpritePresent && GUI.enabled; // if raycasting source && space sprite are set

            keyboardCreator.ReferencedPixels = EditorGUILayout.FloatField(SLICE_PROPORTIONS_CONTENT, keyboardCreator.ReferencedPixels);

            if(GUILayout.Button(REFRESH_SPACE_MATERIAL_BUTTON)) {
                Undo.RegisterCompleteObjectUndo(keyboardCreator.gameObject, REFRESH_SPACE_UNDO);
                keyboardCreator.ReloadSpaceMaterials();
            }
            GUILayout.EndVertical();

        }

        private void CameraFinder () {
            if(!isRaycastingSourceSet) {
                if(GUILayout.Button(FIND_SOURCE_BUTTON)) {//if seach camera button press
                    SearchForCamera();
                }
                if(noCameraFound) { // After button press there is no camera
                    GUILayout.Label(NO_CAMERA_ERROR);
                }
            }
        }

        private void HandleValuesChanges () {
            if(GUI.changed) {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorUtility.SetDirty(keyboardCreator);
            }
        }

        private void DisplayWarnings () {
            if(keyboardCreator.RaycastingSource != null) {
                GUILayout.Space(SPACING_WARINING);
                NotifyErrors();
            }
        }

        /// <summary>
        /// Handles label with error.
        /// </summary>
        private void NotifyErrors () {
            if(errorReporter.ShouldMessageBeDisplayed()) {
                EditorGUILayout.HelpBox(errorReporter.GetMessage(), errorReporter.GetMessageType());
            }
        }

        /// <summary>
        /// Checks if gameobject (whole keyboard) scale was changed
        /// </summary>
        private void HandleScaleChange () {
            float neededXScale = float.NaN;
            if(keyboardCreator.transform.localScale.x != keyboardScale.x) { // X scale changed
                neededXScale = keyboardCreator.transform.localScale.x;
            } else if(keyboardCreator.transform.localScale.z != keyboardScale.z) { // Z scale changed
                neededXScale = keyboardCreator.transform.localScale.z;
            }

            if(!float.IsNaN(neededXScale)) {// If change was made
                ChangeScale(neededXScale, keyboardCreator.transform.localScale.y);
            }
        }

        /// <summary>
        /// Keeps x and z scale bound together. Resizes keybaord
        /// </summary>
        /// <param name="horiziontalScale"> scale in x or z </param>
        /// <param name="y">scale in y</param>
        private void ChangeScale ( float horiziontalScale, float y ) {
            keyboardScale.x = keyboardScale.z = horiziontalScale;
            keyboardScale.y = y;
            keyboardCreator.transform.localScale = keyboardScale;
        }

        /// <summary>
        /// Searches for available camera on scene
        /// </summary>
        private void SearchForCamera () {
            if(Camera.allCameras.Length != 0) {//If there is camera on scene
                noCameraFound = false;
                keyboardCreator.RaycastingSource = Camera.allCameras[0].transform;
            } else {
                noCameraFound = true;
            }
        }
    }
}
