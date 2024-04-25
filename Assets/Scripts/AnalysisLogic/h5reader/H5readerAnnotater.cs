﻿using CellexalVR.General;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;

namespace CellexalVR.AnalysisLogic.H5reader
{

    public class H5readerAnnotater : MonoBehaviour
    {
        public RectTransform display;
        public GameObject textBoxPrefab;
        public GameObject projectionObject;
        public RectTransform projectionRect;
        public ReferenceManager referenceManager;
        public TextMeshProUGUI configViewer;

        Process p;
        StreamReader reader;
        H5ReaderAnnotatorTextBoxScript keys;
        private Dictionary<string, string> config;

        /* Saving data types as the following. 'O' seems to work as unicode string aswell 
        '?'	boolean
        'b'	(signed) byte
        'B'	unsigned byte
        'i'	(signed) integer
        'u'	unsigned integer
        'f'	floating-point
        'c'	complex-floating point
        'm'	timedelta
        'M'	datetime
        'O'	(Python) objects
        'S', 'a'	zero-terminated bytes (not recommended)
        'U'	Unicode string
        'V'	raw data (void)
        */

        private Dictionary<string, char> configDataTypes;

        public List<ProjectionObjectScript> projectionObjectScripts;
        private string path = "LCA_142K_umap_phate_loom";

        private void Start()
        {
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }


        public void Init(string path)
        {
            this.path = path;
            config = new Dictionary<string, string>();
            configDataTypes = new Dictionary<string, char>();

            string[] files = Directory.GetFiles("Data\\" + path);
            string filePath = "";
            foreach (string s in files)
            {
                if (s.EndsWith(".loom") || s.EndsWith(".h5ad"))
                    filePath = s;
            }

            projectionObjectScripts = new List<ProjectionObjectScript>();
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
            keys.annotationName = filePath;
            keys.isTop = true;
            keys.annotater = this;
            string standard_output;
            while ((standard_output = reader.ReadLine()) != null)
            {
                if (standard_output.Contains("xx"))
                    break;
                if (!standard_output.Contains("("))
                    continue;
                keys.Insert(standard_output, this);
            }
            keys.FillContent(display);
            float contentSize = keys.UpdatePosition(10f);
            print(contentSize);
            display.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, contentSize);
        }

        public void AddToConfig(string key, string value, char dtype)
        {
            if (!config.ContainsKey(key))
                config.Add(key, value);
            else
                config[key] = value;

            if (!configDataTypes.ContainsKey(key))
                configDataTypes.Add(key, dtype);
            else
                configDataTypes[key] = dtype;
        }

        public void RemoveFromConfig(string key)
        {
            if(config.ContainsKey(key))
                config.Remove(key);

            if (configDataTypes.ContainsKey(key))
                configDataTypes.Remove(key);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                keys.UpdatePosition(10f);
            }
            string text = "";

            if (configDataTypes.ContainsKey("cellnames") && (configDataTypes["cellnames"] == 'S' || configDataTypes["cellnames"] == 'a'))
                text += "ascii true" + Environment.NewLine;

            foreach (KeyValuePair<string, string> entry in config)
            {
                text += entry.Key + " " + entry.Value + Environment.NewLine;
            }
            configViewer.SetText(text);
        }

        public void CreateConfigFile()
        {
            if (!config.ContainsKey("cellnames"))
            {
                CellexalError.SpawnError("Unfinished config", "Cell names have to be added");
                return;
            }

            if (!config.ContainsKey("genenames")) { 
                CellexalError.SpawnError("Unfinished config", "Gene names have to be added");
                return;
            }

    
            using (StreamWriter outputFile = new StreamWriter(Path.Combine("Data\\" + path, "config.conf")))
            {
                //The cellnames are saved in ascii, we guess everything is saved in ascii.
                if (configDataTypes["cellnames"] == 'S' || configDataTypes["cellnames"] == 'a')
                    outputFile.WriteLine("ascii true");

                foreach (KeyValuePair<string, string> kvp in config)
                {
                    outputFile.WriteLine(kvp.Key + " " + kvp.Value.ToString());
                }

            }
            referenceManager.inputReader.ReadFolder(path);
            referenceManager.h5ReaderAnnotatorScriptManager.RemoveAnnotator(path);
            //manager.RemoveAnnotator(path);
        }

        public void AddProjectionObject(int type)
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
                    projection.Init(ProjectionObjectScript.projectionType.p3D);
                    rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rect.rect.width * (1.1f) * projectionObjectScripts.Count, rect.rect.width);
                    projectionObjectScripts.Add(projection);
                    projection.h5readerAnnotater = this;
                    break;
                case 1:
                    print("2D");
                    go = Instantiate(projectionObject, projectionRect);
                    projection = go.GetComponent<ProjectionObjectScript>();
                    projection.Init(ProjectionObjectScript.projectionType.p2D_sep);
                    rect = go.GetComponent<RectTransform>();
                    rect.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rect.rect.width * (1.1f) * projectionObjectScripts.Count, rect.rect.width);
                    projectionObjectScripts.Add(projection);
                    projection.h5readerAnnotater = this;

                    break;
            }
        }
    }
}