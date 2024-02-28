﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CellexalVR.DesktopUI;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Spatial;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// This class takes care of the brain reference model. The mouse brain model is downloaded using the allen sdk https://allensdk.readthedocs.io/en/latest/. 
    /// The reference model is useful when a spatial mouse brain dataset is analyzed.
    /// To place the data points in relation to the brain glass organ.
    /// Ids, names and acronyms are retrieved via the allen sdk and these can be used to search trough brain parts.
    /// </summary>
    public class AllenReferenceBrain : MonoBehaviour
    {
        public static AllenReferenceBrain instance;

        public bool loadBrain;
        public GameObject rootModel;
        public GameObject models;
        public Dictionary<int, SpatialReferenceModelPart> idToModelDictionary = new Dictionary<int, SpatialReferenceModelPart>();
        public Dictionary<string, string> acronyms = new Dictionary<string, string>();
        public Dictionary<string, int> names = new Dictionary<string, int>();
        public BrainPartButton brainPartButtonPrefab;
        private List<LoadReferenceModelMeshButton> suggestionButtons = new List<LoadReferenceModelMeshButton>();
        public Dictionary<string, GameObject> spawnedParts = new Dictionary<string, GameObject>();

        [HideInInspector] public Vector3 startPosition;
        [HideInInspector] public Vector3 startScale;
        [HideInInspector] public Quaternion startRotation;
        [HideInInspector] public List<int> nonSplittableMeshes = new List<int> { 8, 997, 567, 824 };

        [SerializeField] private ReferenceModelKeyboard keyboard;
        [SerializeField] private BoxCollider boxCollider;
        private List<BrainPartButton> brainPartButtons = new List<BrainPartButton>();
        private const float yInc = 0.20f;
        private const float zInc = 0.65f;
        private bool controllerInside;
        private int suggestionOffset = 0;
        private string currentFilter;
        private bool structureInfoRead;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (!loadBrain)
            {
                gameObject.SetActive(false);
            }
        }

        private void ReadMeshData()
        {
            foreach (LoadReferenceModelMeshButton b in GetComponentsInChildren<LoadReferenceModelMeshButton>())
            {
                suggestionButtons.Add(b);
            }

            foreach (Transform model in models.transform)
            {
                idToModelDictionary[int.Parse(model.gameObject.name)] = model.GetComponent<SpatialReferenceModelPart>();
            }

            string filePath = Path.Combine(Application.streamingAssetsPath, "structure_info.csv");
            using (StreamReader sr = new StreamReader(filePath))
            {
                string header = sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] words = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"); // monster regex to handle comma inside quotes (inside part name in this case) https://stackoverflow.com/questions/18893390/splitting-on-comma-outside-quotes
                    int id = int.Parse(words[0]);
                    string n = words[1].Trim(new char[] { ' ', '\"' });
                    string acronym = words[2].Trim(new char[] { ' ', '\"' });
                    ColorUtility.TryParseHtmlString("#" + words[3].Trim(' '), out Color c);
                    if (!idToModelDictionary.ContainsKey(id)) continue;
                    SpatialReferenceModelPart model = idToModelDictionary[id];
                    model.modelAcronym = acronym;
                    model.modelName = n;

                    acronyms[acronym] = n;
                    model.id = id;
                    names[n] = id;
                    if (id == 997)
                    {
                        continue;
                    }
                    model.SetColor(c);
                }
            }
            structureInfoRead = true;
            // root model is the main brain model. have active and transparent on start...
        }

        private void OnEnable()
        {
            if (!loadBrain)
                return;
            else if (!structureInfoRead)
                ReadMeshData();
            boxCollider = GetComponent<BoxCollider>();
            keyboard = GetComponentInChildren<ReferenceModelKeyboard>(true);
            SpawnModel("root");
            UpdateSuggestions();
            //CellexalEvents.GraphsLoaded.AddListener(ParentBrain);
        }

        /// <summary>
        /// For the spatial dataset graph points to be placed in relation to the glass brain it parents the object to the spatial graph transform.
        /// </summary>
        public void Toggle(bool toggle)
        {
            if (!structureInfoRead)
                ReadMeshData();
            gameObject.SetActive(toggle);
            keyboard.gameObject.SetActive(toggle);
            if (toggle)
                SpawnModel("root");
            //transform.parent = GameObject.Find("slice_coords_spatial").transform;
            //transform.localPosition = Vector3.one * -0.5f;
            //transform.localScale = Vector3.one;
            //transform.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if ((int)(Time.realtimeSinceStartup) % 2 == 0)
            {
                CheckForController();
            }
        }

        private void CheckForController()
        {
            Collider[] colliders = Physics.OverlapBox(transform.TransformPoint(boxCollider.center),
                boxCollider.size / 2, transform.rotation,
                1 << LayerMask.NameToLayer("Controller") | LayerMask.NameToLayer("Player"));
            if (colliders.Any(x => x.CompareTag("Player") || x.CompareTag("GameController")))
            {
                controllerInside = true;
            }
            else
            {
                controllerInside = false;
            }
        }

        /// <summary>
        /// Activates the keyboard to use to search trough brain part models to spawn.
        /// </summary>
        /// <param name="activate"></param>
        public void ActivateKeyboard(bool activate)
        {
            keyboard.gameObject.SetActive(activate);
        }

        /// <summary>
        /// Remove a spawn glass brain part.
        /// </summary>
        /// <param name="modelName"></param>
        public void RemovePart(string modelName)
        {
            spawnedParts[modelName].SetActive(false);
            spawnedParts.Remove(modelName);
            var b = brainPartButtons.Find(x => x.ModelName == modelName);
            brainPartButtons.Remove(b);
            Destroy(b.gameObject);
            UpdateButtonPositions();
        }


        /// <summary>
        /// Spawns a glass brain part.
        /// </summary>
        /// <param name="id">ID of the brain part.</param>
        /// <returns></returns>
        [ConsoleCommand("brainModel", aliases: new string[] { "spawnbrainmodel", "sbm" })]
        public SpatialReferenceModelPart SpawnModel(int id)
        {
            idToModelDictionary.TryGetValue(id, out SpatialReferenceModelPart objToSpawn);
            if (objToSpawn == null) return objToSpawn;
            objToSpawn.gameObject.SetActive(!objToSpawn.gameObject.activeSelf);
            spawnedParts[objToSpawn.modelName] = objToSpawn.gameObject;
            return objToSpawn;
        }

        /// <summary>
        /// Spawn a glass brain part.
        /// </summary>
        /// <param name="modelName">Name of the model.</param>
        public void SpawnModel(string modelName)
        {
            spawnedParts.TryGetValue(modelName, out GameObject spawnedPart);
            if (spawnedPart != null && spawnedPart.activeSelf)
            {
                return;
            }
            else
            {
                SpatialReferenceModelPart part = SpawnModel(GetModelId(modelName));
                CreateButton(part.modelName, part.color);
            }
            UpdateSuggestions(keyboard.output.text, suggestionOffset);
        }


        /// <summary>
        /// Spawning a brain part creates a button to toggle it on/off. These positions are updated each time a new one is added.
        /// </summary>
        private void UpdateButtonPositions()
        {
            for (int i = 0; i < brainPartButtons.Count; i++)
            {
                BrainPartButton button = brainPartButtons[i];
                button.transform.localPosition = new Vector3(0, -0.17f - ((i / 4) * yInc), 1.2f - ((i % 4) * zInc));
            }
        }

        /// <summary>
        /// Create a button corresponding to the brain part spawned. The button toggles the part on/off. 
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="color"></param>
        private void CreateButton(string modelName, Color color)
        {
            BrainPartButton button = Instantiate(brainPartButtonPrefab, keyboard.transform);
            brainPartButtons.Add(button);
            button.transform.localPosition = new Vector3(0, -0.17f - (((brainPartButtons.Count - 1) / 4) * yInc), 1.2f - (((brainPartButtons.Count - 1) % 4) * zInc));
            button.gameObject.SetActive(true);
            button.ModelName = modelName;
            button.modelPart = spawnedParts[modelName];
            color /= 2f;
            button.meshStandardColor = color;
            color *= 2f;
            button.meshHighlightColor = color;
            MeshRenderer mr = button.GetComponent<MeshRenderer>();
            mr.material.color = color;
        }

        /// <summary>
        /// Given a name of a model retrieves the corresponding id.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private int GetModelId(string n)
        {
            if (!idToModelDictionary.ContainsKey(names[n]))
            {
                return idToModelDictionary[names[acronyms[n]]].id;
            }
            else
            {
                return idToModelDictionary[names[n]].id;
            }
        }

        /// <summary>
        /// Spread out the spawned brain parts. Distances the parts from the center.
        /// </summary>
        public void Spread()
        {
            foreach (GameObject obj in spawnedParts.Values)
            {
                obj.GetComponent<SpatialReferenceModelPart>().Spread();
            }
        }

        /// <summary>
        /// Scroll through the brain part suggestions.
        /// </summary>
        /// <param name="dir">Up/down</param>
        public void ScrollSuggestions(int dir)
        {
            suggestionOffset = Mathf.Max(0, Mathf.Min(suggestionOffset + dir, names.Keys.Count - 1));
            UpdateSuggestions(keyboard.output.text, suggestionOffset);
        }

        /// <summary>
        /// Writing on the keyboard filters the brain part names/acronyms.
        /// </summary>
        /// <param name="filter">The letters that the brain part must contains.</param>
        /// <param name="offset">Used for the suggestions to be correct.</param>
        public void UpdateSuggestions(string filter = "", int offset = 0)
        {
            if (!filter.Equals(currentFilter))
            {
                suggestionOffset = 0;
                currentFilter = filter;
            }
            if (filter.Equals("Enter a name"))
            {
                filter = "";
            }
            List<string> filteredNames = new List<string>();
            List<string> allNames = names.Keys.ToList();
            List<string> allFilteredNames = new List<string>();
            allNames.Sort();
            if (!filter.Equals(string.Empty))
            {
                for (int i = 0; i < allNames.Count; i++)
                {
                    string n = allNames[i];
                    if (n.Contains(filter) && !spawnedParts.ContainsKey(n) && !BrainMeshButtonAdded(n))
                    {
                        allFilteredNames.Add(n);
                    }

                }
                if (allFilteredNames.Count < 3)
                {
                    List<string> acrnms = acronyms.Keys.ToList();
                    acrnms.Sort();
                    for (int i = offset; i < acrnms.Count; i++)
                    {
                        string n = acrnms[i];
                        if (n.Contains(filter))
                        {
                            allFilteredNames.Add(acronyms[n]);
                        }
                        if (filteredNames.Count > 3)
                        {
                            break;
                        }
                    }
                }
            }

            else
            {
                allFilteredNames = allNames;
            }

            int j = 0;
            int k = offset;
            while (j < 4 && k + offset < allFilteredNames.Count)
            {
                string n = allFilteredNames[k++];
                if (spawnedParts.ContainsKey(n) || BrainMeshButtonAdded(n)) continue;
                filteredNames.Add(n);
                j++;
            }


            for (int i = 0; i < filteredNames.Count; i++)
            {
                suggestionButtons[i].ModelName = filteredNames[i];
            }

        }

        /// <summary>
        /// Checks if the brain part button is already added.
        /// </summary>
        /// <param name="n">Name of model to the corresponding button.</param>
        /// <returns></returns>
        private bool BrainMeshButtonAdded(string n)
        {
            foreach (BrainPartButton bpb in brainPartButtons)
            {
                if (bpb.ModelName.Equals(n)) return true;
            }
            return false;
        }

        /// <summary>
        /// Helper function to deal with the imported meshes.
        /// </summary>
        /// <returns></returns>
        public IEnumerator PopulateMeshes()
        {
            var models = GetComponentsInChildren<SpatialReferenceModelPart>(true);
            print(models.Length);
            foreach (SpatialReferenceModelPart model in models)
            {
                var parts = model.GetComponentsInChildren<MeshRenderer>(true);
                string modelName = model.gameObject.name;
                foreach (MeshRenderer mr in parts)
                {
                    string partName = $"{modelName}_{mr.gameObject.name}";
                    var m = (Mesh)Resources.Load($"meshparts/{partName}");
                    mr.GetComponent<MeshFilter>().mesh = m;
                    yield return null;
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Many meshes are two mirror images (left and right side). For spread, overlap and the colliders to work these need to be split into seperate meshes.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SplitMeshesCoroutine()
        {
            // 567, 824
            //SpatialReferenceModelPart part = idToModelDictionary[46];
            List<SpatialReferenceModelPart> parts = new List<SpatialReferenceModelPart> { idToModelDictionary[44] };
            //List<SpatialReferenceModelPart> parts = idToModelDictionary.Values.ToList();
            int i = 1;
            foreach (SpatialReferenceModelPart part in parts)
            {
                part.gameObject.SetActive(true);
                //part.SplitMesh();
                yield return new WaitForSeconds(0.1f);
                part.gameObject.SetActive(false);
                i++;
            }

        }

        public void SplitMeshes()
        {
            StartCoroutine(SplitMeshesCoroutine());
        }

        [CustomEditor(typeof(AllenReferenceBrain))]
        public class ColliderGeneratorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                AllenReferenceBrain myTarget = (AllenReferenceBrain)target;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Read Structure Info"))
                {
                    myTarget.ReadMeshData();
                }
                GUILayout.EndHorizontal();

                DrawDefaultInspector();
            }

        }


#endif
    }

}