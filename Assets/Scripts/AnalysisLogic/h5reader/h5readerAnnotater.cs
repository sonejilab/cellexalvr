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
    Process p;
    StreamReader reader;
    H5ReaderAnnotatorTextBoxScript keys;
    Dictionary<string, string> config;

    void Start()
    {
        print("TETS");
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
            print(standard_output);
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
}
