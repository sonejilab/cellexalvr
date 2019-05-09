using CellexalVR.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR.InteractionSystem;
using VRTK;

namespace CellexalVR.General
{
    [ExecuteInEditMode]
    public class SceneBuilder : MonoBehaviour
    {
        public GameObject InputReader;
        private GameObject _InputReader;
        public GameObject CameraRig;
        private GameObject _CameraRig;
        public GameObject VRTK;
        private GameObject _VRTK;
        public GameObject Managers;
        private GameObject _Managers;
        public GameObject Generators;
        private GameObject _Generators;
        public GameObject SQLiter;
        private GameObject _SQLiter;
        public GameObject EventSystem;
        private GameObject _EventSystem;


        public GameObject Floor;
        private GameObject _Floor;
        public GameObject Calculators;
        private GameObject _Calculators;
        public GameObject LightForTesting;
        private GameObject _LightForTesting;
        public GameObject MenuHolder;
        private GameObject _MenuHolder;


        public GameObject Loader;
        private GameObject _Loader;
        public GameObject Keyboard;
        private GameObject _Keyboard;
        public GameObject WebBrowser;
        private GameObject _WebBrowser;


        public GameObject SettingsMenu;
        private GameObject _SettingsMenu;
        public GameObject Console;
        private GameObject _Console;
        public GameObject FPSCanvas;
        private GameObject _FPSCanvas;
        public GameObject WaitingCanvas;
        private GameObject _WaitingCanvas;

        private List<GameObject> instances;
        private IEnumerator buildSceneEnumerator;
        private bool buildingScene = false;

        private SceneBuilder()
        {
            EditorApplication.update += Update;
            instances = new List<GameObject>();
        }

        private void Update()
        {
            if (!buildingScene)
            {
                EditorUtility.ClearProgressBar();
            }
            else if (buildSceneEnumerator != null)
            {
                if (!((WaitForSecondsRealtime)buildSceneEnumerator.Current).keepWaiting)
                    buildSceneEnumerator.MoveNext();
            }
        }

        public void BuildScene(bool forceInstantiate = false)
        {
            //if (Application.isPlaying)
            //{
            //    return;
            //}
            // instantiate missing gameobjects
            buildSceneEnumerator = BuildSceneCoroutine(forceInstantiate);
            buildSceneEnumerator.MoveNext();
        }

        private IEnumerator BuildSceneCoroutine(bool forceInstantiate)
        {
            buildingScene = true;
            EditorUtility.DisplayProgressBar("Building scene", "Instantiating objects", 0f);
            InstantiateSceneAsset(ref _InputReader, InputReader, forceInstantiate);
            yield return new WaitForSecondsRealtime(0.25f);
            EditorUtility.DisplayProgressBar("Building scene", "Instantiating objects", 0.1f);
            InstantiateSceneAsset(ref _CameraRig, CameraRig, forceInstantiate);
            InstantiateSceneAsset(ref _VRTK, VRTK, forceInstantiate);
            InstantiateSceneAsset(ref _Managers, Managers, forceInstantiate);
            InstantiateSceneAsset(ref _Generators, Generators, forceInstantiate);
            InstantiateSceneAsset(ref _SQLiter, SQLiter, forceInstantiate);
            InstantiateSceneAsset(ref _EventSystem, EventSystem, forceInstantiate);
            InstantiateSceneAsset(ref _Floor, Floor, forceInstantiate);
            InstantiateSceneAsset(ref _Calculators, Calculators, forceInstantiate);
            InstantiateSceneAsset(ref _LightForTesting, LightForTesting, forceInstantiate);
            InstantiateSceneAsset(ref _MenuHolder, MenuHolder, forceInstantiate);
            InstantiateSceneAsset(ref _Loader, Loader, forceInstantiate);
            InstantiateSceneAsset(ref _Keyboard, Keyboard, forceInstantiate);
            InstantiateSceneAsset(ref _WebBrowser, WebBrowser, forceInstantiate);
            InstantiateSceneAsset(ref _SettingsMenu, SettingsMenu, forceInstantiate);
            InstantiateSceneAsset(ref _Console, Console, forceInstantiate);
            InstantiateSceneAsset(ref _FPSCanvas, FPSCanvas, forceInstantiate);
            InstantiateSceneAsset(ref _WaitingCanvas, WaitingCanvas, forceInstantiate);
            yield return new WaitForSecondsRealtime(0.25f);
            EditorUtility.DisplayProgressBar("Building scene", "Setting references", 0.5f);
            _InputReader.GetComponent<ReferenceManager>().AttemptSetReferences();
            yield return new WaitForSecondsRealtime(0.1f);
            EditorUtility.DisplayProgressBar("Building scene", "Running OnValidate", 0.6f);

            // run onvalidate on everything
            foreach (var instance in instances)
            {
                var allChildren = instance.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var child in allChildren)
                {
                    // MonoBehaviour.SendMessage() does no work in inactive gameobjects, use reflection instead
                    if (child.GetType().GetMethod("OnValidate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) != null)
                    {
                        child.Invoke("OnValidate", 0f);
                    }
                }
            }

            yield return new WaitForSecondsRealtime(0.25f);
            EditorUtility.DisplayProgressBar("Building scene", "Setting references", 0.7f);
            // set up missing references 
            VRTK_SDKManager sdkmanager = _VRTK.GetComponent<VRTK_SDKManager>();
            _CameraRig.GetComponent<Player>().hands = new Hand[] { sdkmanager.scriptAliasLeftController.GetComponent<Hand>(), sdkmanager.scriptAliasRightController.GetComponent<Hand>() };
            sdkmanager.actualBoundaries = _CameraRig;
            sdkmanager.actualHeadset = _CameraRig.transform.FindDeepChild("Camera (eye)").gameObject;
            sdkmanager.actualLeftController = _CameraRig.transform.Find("Controller (left)").gameObject;
            sdkmanager.actualRightController = _CameraRig.transform.Find("Controller (right)").gameObject;
            sdkmanager.modelAliasLeftController = sdkmanager.actualLeftController.transform.Find("Model").gameObject;
            sdkmanager.modelAliasRightController = sdkmanager.actualRightController.transform.Find("Model").gameObject;

            _Keyboard.GetComponent<CellexalVR.Interaction.KeyboardHandler>().BuildKeyboard();

            EditorUtility.DisplayProgressBar("Building scene", "Done", 1f);
            EditorUtility.ClearProgressBar();
            buildingScene = false;
        }

        private void InstantiateSceneAsset(ref GameObject instance, GameObject prefab, bool forceInstantiate)
        {
            if (instance != null && forceInstantiate)
            {
                GameObject copy = instance;
                UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(copy.gameObject); };
                instances.Remove(instance);
                instance = null;
            }
            if (instance == null)
            {
                instance = PrefabUtility.InstantiatePrefab(prefab as GameObject) as GameObject;
                instance.name = prefab.name;
                instances.Add(instance);

            }
        }

        public void RemoveInstances()
        {
            foreach (var i in instances)
            {
                UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(i.gameObject); };
            }
            instances.Clear();
        }

        public void AutoPopulateGameObjects()
        {
            InputReader = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/InputReader.prefab");
            CameraRig = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/[CameraRig].prefab");
            VRTK = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/[VRTK].prefab");
            Managers = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Managers.prefab");
            Generators = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Generators.prefab");
            SQLiter = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/SQLiter.prefab");
            EventSystem = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/EventSystem.prefab");
            Floor = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Floor.prefab");
            Calculators = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Calculator cluster.prefab");
            LightForTesting = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Light For Testing.prefab");
            MenuHolder = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/MenuHolder.prefab");
            Loader = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Tron_Loader.prefab");
            Keyboard = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Keyboard Setup.prefab");
            WebBrowser = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/WebBrowser.prefab");
            SettingsMenu = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DesktopUI/Settings Menu.prefab");
            Console = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DesktopUI/Console.prefab");
            FPSCanvas = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DesktopUI/FPS canvas.prefab");
            WaitingCanvas = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DesktopUI/WaitingCanvas.prefab");
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SceneBuilder))]
    public class SceneBuilderEditor : UnityEditor.Editor
    {

        private SceneBuilder instance;

        void OnEnable()
        {
            instance = (SceneBuilder)target;
        }


        public override void OnInspectorGUI()
        {

            if (GUILayout.Button("Build scene"))
            {
                instance.BuildScene();
            }
            if (GUILayout.Button("Force build scene"))
            {
                instance.BuildScene(true);
            }
            if (GUILayout.Button("Remove objects"))
            {
                instance.RemoveInstances();
            }
            if (GUILayout.Button("Auto-populate gameobjects"))
            {
                instance.AutoPopulateGameObjects();
            }
            DrawDefaultInspector();

        }


    }
#endif
}