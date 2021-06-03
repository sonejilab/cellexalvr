using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CellexalVR.DesktopUI;
using CellexalVR.Interaction;
using Spatial;
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
    public SteamVR_Action_Boolean controllerAction = SteamVR_Input.GetBooleanAction("Teleport");

    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Vector3 startScale;
    [HideInInspector] public Quaternion startRotation;


    private ReferenceModelKeyboard keyboard;
    private Dictionary<string, GameObject> spawnedParts = new Dictionary<string, GameObject>();
    private List<BrainPartButton> brainPartButtons = new List<BrainPartButton>();
    private const float yInc = 0.20f;
    private const float zInc = 0.65f;
    private BoxCollider boxCollider;
    private bool controllerInside;
    private int frameCount;

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
        print(filePath);
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
                model.color = c;
                model.id = id;

                acronyms[acronym] = n;
                names[n] = id;
                //print($"id: {id}, name: {n}, acr: {acronym}, {model.gameObject.name}");
            }
        }
        // root model is the main brain model. have active and transparent on start...
        idToModelDictionary[997].color = Color.white / 10f;
        SpawnModel("root");
        keyboard.gameObject.SetActive(false);
    }

    private void Update()
    {
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

        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    SpreadOutParts();
        //}
    }

    private void CheckForController()
    {
        if (!boxCollider.enabled) return;
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
        objToSpawn.gameObject.SetActive(!objToSpawn.gameObject.activeSelf);
        objToSpawn.GetComponentInChildren<Renderer>().material.color = objToSpawn.color;
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
}