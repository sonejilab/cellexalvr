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
    Process p;
    StreamReader reader;
    H5ReaderAnnotatorTextBoxScript keys;
    Dictionary<string, string> config;

    private ArrayList projectionObjectScripts;

    void Start()
    {
        projectionObjectScripts = new ArrayList();
        p = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;

        startInfo.FileName = "py.exe";

        string file_name = "LCA_142K_umap_phate.loom";
        startInfo.Arguments = "crawl.py " + "Data/" + file_name;
        p.StartInfo = startInfo;
        p.Start();
        reader = p.StandardOutput;

        //Read all keys from the loom file
        GameObject go = Instantiate(textBoxPrefab);
        keys = go.GetComponent<H5ReaderAnnotatorTextBoxScript>();
        go.name = file_name;
        keys.name = file_name;
        keys.isTop = true;
        string standard_output;
        while ((standard_output = reader.ReadLine()) != null)
        {
            if (standard_output.Contains("xx"))
                break;
            if (!standard_output.Contains("("))
                continue;
            keys.insert(standard_output);
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
        string docPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Data";

        ArrayList list = keys.getTypeInChildren("3D");

        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "test.conf")))
        {
            foreach(H5ReaderAnnotatorTextBoxScript k in list)
            {
                string path = k.getPath();
                string name = k.name.Substring(0, k.name.LastIndexOf(":"));
                if(name.Substring(0, 2) != "X_")
                    name = "X_" + name;
                outputFile.WriteLine(name + " " + path);
            }
        }
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
