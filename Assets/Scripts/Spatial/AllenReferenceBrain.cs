using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CellexalVR.DesktopUI;
using CellexalVR.Interaction;
using Spatial;
using UnityEditor;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class AllenReferenceBrain : MonoBehaviour
{
    public static AllenReferenceBrain instance;

    public GameObject rootModel;
    public GameObject models;
    public Dictionary<int, SpatialReferenceModelPart> idToModelDictionary = new Dictionary<int, SpatialReferenceModelPart>();
    public Dictionary<string, string> acronyms = new Dictionary<string, string>();
    public Dictionary<string, int> names = new Dictionary<string, int>();
    public Transform midPoint;
    public BrainPartButton brainPartButtonPrefab;
    public List<LoadReferenceModelMeshButton> suggestionButtons = new List<LoadReferenceModelMeshButton>();
    public SteamVR_Action_Boolean controllerAction = SteamVR_Input.GetBooleanAction("TriggerClick");
    public Dictionary<string, GameObject> spawnedParts = new Dictionary<string, GameObject>();

    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Vector3 startScale;
    [HideInInspector] public Quaternion startRotation;


    private ReferenceModelKeyboard keyboard;
    private List<BrainPartButton> brainPartButtons = new List<BrainPartButton>();
    private const float yInc = 0.20f;
    private const float zInc = 0.65f;
    private BoxCollider boxCollider;
    private bool controllerInside;
    private int frameCount;

    private bool spread;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        keyboard = GetComponentInChildren<ReferenceModelKeyboard>(true);
        //Make list of spawned brain models and add meshes for all.
        foreach (LoadReferenceModelMeshButton b in GetComponentsInChildren<LoadReferenceModelMeshButton>())
        {
            suggestionButtons.Add(b);
        }

        foreach (Transform model in models.transform)
        {
            idToModelDictionary[int.Parse(model.gameObject.name)] = model.GetComponent<SpatialReferenceModelPart>();
        }

        string filePath = "structure_info.csv";

        //Create instance of material for each color.

        using (StreamReader sr = new StreamReader(filePath))
        {
            string header = sr.ReadLine();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] words = line.Split(',');
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
                //print($"id: {id}, name: {n}, acr: {acronym}, {model.gameObject.name}");
            }
        }
        // root model is the main brain model. have active and transparent on start...
        //idToModelDictionary[997].color = Color.white / 10f;
        SpawnModel("root");
        keyboard.gameObject.SetActive(false);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    Spread();
        //}
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    StartCoroutine(SplitMeshes());
        //}
        if ((int)(Time.realtimeSinceStartup) % 2 == 0)
        {
            CheckForController();
        }

        if (Player.instance.rightHand == null) return;
        if (controllerAction.GetStateDown(Player.instance.rightHand.handType))
        {
            if (controllerInside)
            {
                keyboard.gameObject.SetActive(!keyboard.gameObject.activeSelf);
            }
        }
    }

    private void CheckForController()
    {
        //if (!boxCollider.enabled) return;
        Collider[] colliders = Physics.OverlapBox(transform.TransformPoint(boxCollider.center), boxCollider.size / 2, transform.rotation, 1 << LayerMask.NameToLayer("Controller") | LayerMask.NameToLayer("Player"));
        if (colliders.Any(x => x.CompareTag("Player") || x.CompareTag("Controller")))
        {
            controllerInside = true;
        }
        else
        {
            controllerInside = false;
        }

    }



    public void RemovePart(string modelName)
    {
        spawnedParts[modelName].SetActive(false);
        spawnedParts.Remove(modelName);
        var b = brainPartButtons.Find(x => x.ModelName == modelName);
        brainPartButtons.Remove(b);
        Destroy(b.gameObject);
        UpdateButtonPositions();
    }


    [ConsoleCommand("brainModel", aliases: new string[] { "spawnbrainmodel", "sbm" })]
    public SpatialReferenceModelPart SpawnModel(int id)
    {
        idToModelDictionary.TryGetValue(id, out SpatialReferenceModelPart objToSpawn);
        if (objToSpawn == null) return objToSpawn;
        objToSpawn.gameObject.SetActive(!objToSpawn.gameObject.activeSelf);
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            //r.material.SetColor("_Col", objToSpawn.color);
            //r.material.color = objToSpawn.color;
        }
        spawnedParts[objToSpawn.modelName] = objToSpawn.gameObject;
        return objToSpawn;
    }

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
    }

    private void UpdateButtonPositions()
    {
        for (int i = 0; i < brainPartButtons.Count; i++)
        {
            BrainPartButton button = brainPartButtons[i];
            button.transform.localPosition = new Vector3(0, -0.17f - ((i / 5) * yInc), 1.2f - ((i % 5) * zInc));
        }
    }

    private void CreateButton(string modelName, Color color)
    {
        BrainPartButton button = Instantiate(brainPartButtonPrefab, keyboard.transform);
        brainPartButtons.Add(button);
        button.transform.localPosition = new Vector3(0, -0.17f - (((brainPartButtons.Count - 1) / 5) * yInc), 1.2f - (((brainPartButtons.Count - 1) % 5) * zInc));
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

    public void Spread()
    {
        foreach (GameObject obj in spawnedParts.Values)
        {
            StartCoroutine(obj.GetComponent<SpatialReferenceModelPart>().Spread());
        }

    }


    public void UpdateSuggestions(string filter)
    {
        List<string> filteredNames = new List<string>();
        List<string> allNames = new List<string>();
        foreach (string n in names.Keys)
        {
            if (n.Contains(filter))
            {
                filteredNames.Add(n);
            }
            if (filteredNames.Count > 3)
            {
                break;
            }
        }
        if (filteredNames.Count < 3)
        {
            foreach (string n in acronyms.Keys)
            {
                if (n.Contains(filter))
                {
                    filteredNames.Add(acronyms[n]);
                }
                if (filteredNames.Count > 3)
                {
                    break;
                }
            }
        }

        for (int i = 0; i < filteredNames.Count; i++)
        {
            suggestionButtons[i].ModelName = filteredNames[i];
        }

    }


    private IEnumerator SplitMeshes()
    {
        //SpatialReferenceModelPart part = idToModelDictionary[46];
        int i = 1;
        foreach (SpatialReferenceModelPart part in idToModelDictionary.Values)
        {
            part.gameObject.SetActive(true);
            part.SplitMesh();
            yield return new WaitForSeconds(0.1f);
            part.gameObject.SetActive(false);
            i++;
        }

    }
}