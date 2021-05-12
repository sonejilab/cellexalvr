using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CellexalVR.DesktopUI;
using Spatial;
using UnityEngine;

public class AllenReferenceBrain : MonoBehaviour
{
    public static AllenReferenceBrain instance;

    public GameObject rootModel;
    public GameObject models;
    public Dictionary<int, SpatialReferenceModelPart> idToModelDictionary = new Dictionary<int, SpatialReferenceModelPart>();
    public Dictionary<string, string> acronyms = new Dictionary<string, string>();
    public Dictionary<string, int> names = new Dictionary<string, int>();


    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Vector3 startScale;
    [HideInInspector] public Quaternion startRotation;

    public List<LoadReferenceModelMeshButton> suggestionButtons = new List<LoadReferenceModelMeshButton>();

    private List<GameObject> spawnedParts = new List<GameObject>();

    // private Dictionary<int, Tuple<string, Color> idToNameDictionary = new Dictionary<int, Tuple<string, Color>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {

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
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    SpreadOutParts();
        //}
    }



    [ConsoleCommand("brainModel", aliases: new string[] { "spawnbrainmodel", "sbm" })]
    public void SpawnModel(int id)
    {
        idToModelDictionary.TryGetValue(id, out SpatialReferenceModelPart objToSpawn);
        objToSpawn.gameObject.SetActive(!objToSpawn.gameObject.activeSelf);
        objToSpawn.GetComponentInChildren<Renderer>().material.color = objToSpawn.color;
        spawnedParts.Add(objToSpawn.gameObject);
        //Instantiate(objToSpawn, transform);
    }

    public void SpawnModel(string name)
    {
        SpawnModel(GetModelId(name));
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

    private void SpreadOutParts()
    {
        foreach (GameObject part in spawnedParts)
        {
            MeshRenderer mr = part.GetComponentInChildren<MeshRenderer>();
            MeshFilter mf = part.GetComponentInChildren<MeshFilter>();
            var position = mf.mesh.vertices[0];
            //var position = mr.transform.localPosition;
            var dir = (position - models.transform.localPosition).normalized;
            part.transform.localPosition += dir;
        }
    }
}