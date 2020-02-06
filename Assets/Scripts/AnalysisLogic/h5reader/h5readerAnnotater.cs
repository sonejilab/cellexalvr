using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;
using CellexalVR.AnalysisObjects;
using SQLiter;
using CellexalVR.General;
using TMPro;

public class h5readerAnnotater : MonoBehaviour
{
    public RectTransform display;
    public GameObject textBoxPrefab;
    public GameObject projectionObject3D;
    public GameObject projectionObject2D;
    public RectTransform projectionRect;
    public ReferenceManager referenceManager;

    Process p;
    StreamReader reader;
    H5ReaderAnnotatorTextBoxScript keys;
    public Dictionary<string, string> config;
    private ArrayList projectionObjectScripts;
    private string path = "LCA_142K_umap_phate_loom";

    void Start()
    {
        if (!referenceManager)
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
    }

    public void init(string path)
    {
        this.path = path;
        config = new Dictionary<string, string>();
        string[] files = Directory.GetFiles("Data\\" + path);
        string filePath = "";
        foreach (string s in files)
        {
            if (s.EndsWith(".loom") || s.EndsWith(".h5ad"))
                filePath = s;
        }

        projectionObjectScripts = new ArrayList();
        p = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;

        startInfo.FileName = "py.exe";

        startInfo.Arguments = "crawl.py " + filePath;
        p.StartInfo = startInfo;
        p.Start();
        reader = p.StandardOutput;

        //Read all keys from the loom file
        GameObject go = Instantiate(textBoxPrefab);
        keys = go.GetComponent<H5ReaderAnnotatorTextBoxScript>();
        go.name = filePath;
        keys.name = filePath;
        keys.isTop = true;
        keys.annotater = this;
        string standard_output;
        while ((standard_output = reader.ReadLine()) != null)
        {
            if (standard_output.Contains("xx"))
                break;
            if (!standard_output.Contains("("))
                continue;
            keys.insert(standard_output, this);
        }
        keys.fillContent(display);
        float contentSize = keys.updatePosition(10f);
        display.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, contentSize);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            keys.updatePosition(10f);
        }
    }

    public void createConfigFile()
    {

        foreach(ProjectionObjectScript p in projectionObjectScripts)
        {
            print(p.name);
            print(p.coordsPath);
            print(p.velocityPath);
            config.Add("X_" + p.name, p.coordsPath);
            config.Add("vel_" + p.name, p.velocityPath);
        }

        using (StreamWriter outputFile = new StreamWriter(Path.Combine("Data\\" + path, "config.conf")))
        {
            foreach(KeyValuePair<string,string> kvp in config)
            {
                outputFile.WriteLine(kvp.Key + " " + kvp.Value.ToString());
            }
            
        }
        referenceManager.inputReader.ReadFolder(path);
    }

    public void addProjectionObject(int type)
    {
        GameObject go;
        ProjectionObjectScript projection;
        switch (type)
        {
            case 0:
                print("pressed");
                go = Instantiate(projectionObject3D, projectionRect);
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rect.rect.width*(1.1f) * projectionObjectScripts.Count, rect.rect.width);
                projection = go.GetComponent<ProjectionObjectScript>();
                projectionObjectScripts.Add(projection);
                break;
            case 1:
                print("pressed");
                go = Instantiate(projectionObject2D, projectionRect);
                projection = go.GetComponent<ProjectionObjectScript>();
                break;
        }
    }
}
