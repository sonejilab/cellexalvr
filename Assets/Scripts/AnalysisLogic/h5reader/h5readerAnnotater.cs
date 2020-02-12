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
    public H5ReaderAnnotatorScriptManager manager;
    public RectTransform display;
    public GameObject textBoxPrefab;
    public GameObject projectionObject;
    public RectTransform projectionRect;
    public ReferenceManager referenceManager;
    public TextMeshProUGUI configViewer;

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
        string text = "";

        foreach (KeyValuePair<string, string> entry in config)
        {
            text += entry.Key + " " + entry.Value + Environment.NewLine;
        }
        configViewer.SetText(text);
    }

    public void createConfigFile()
    {
        print("saving config");
        foreach(ProjectionObjectScript p in projectionObjectScripts)
        {
            foreach(KeyValuePair<string, string> kvp in p.paths)
            {
                config.Add(kvp.Key + "_" + p.name, kvp.Value);
            }
        }

        using (StreamWriter outputFile = new StreamWriter(Path.Combine("Data\\" + path, "config.conf")))
        {
            foreach(KeyValuePair<string,string> kvp in config)
            {
                outputFile.WriteLine(kvp.Key + " " + kvp.Value.ToString());
            }
            
        }
        referenceManager.inputReader.ReadFolder(path);
        manager.removeAnnotator(path);
    }

    public void addProjectionObject(int type)
    {
        GameObject go;
        ProjectionObjectScript projection;
        RectTransform rect;

        switch (type)
        {
            case 0:
                print("3D");
                go = Instantiate(projectionObject, projectionRect);
                rect = go.GetComponent<RectTransform>();
                projection = go.GetComponent<ProjectionObjectScript>();
                projection.init(ProjectionObjectScript.projectionType.p3D);
                rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rect.rect.width * (1.1f) * projectionObjectScripts.Count, rect.rect.width);

                projectionObjectScripts.Add(projection);
                break;
            case 1:
                print("2D");
                go = Instantiate(projectionObject, projectionRect);
                projection = go.GetComponent<ProjectionObjectScript>();
                projection.init(ProjectionObjectScript.projectionType.p2D_sep);
                rect = go.GetComponent<RectTransform>();
                rect.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rect.rect.width * (1.1f) * projectionObjectScripts.Count, rect.rect.width);
                projectionObjectScripts.Add(projection);
                break;
        }
    }
}
