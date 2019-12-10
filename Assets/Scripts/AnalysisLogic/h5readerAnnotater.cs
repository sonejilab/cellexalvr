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
    public GameObject textPrefab;
    Process p;
    StreamReader reader;
    KeyHolder keys;
    // Start is called before the first frame update
    void Start()
    {
        p = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;
        //startInfo.CreateNoWindow = true;

        startInfo.FileName = "py.exe";

        string file_name = "Data/LCA_142K_umap_phate.loom";
        startInfo.Arguments = "crawl.py " + file_name;
        p.StartInfo = startInfo;
        p.Start();
        reader = p.StandardOutput;

        //Read all keys from the loom file
        keys = new KeyHolder("LCA_142K_umap_phate.loom");
        string standard_output;
        while ((standard_output = reader.ReadLine()) != null)
        {
            if (standard_output.Contains("xx"))
                break;
            if (!standard_output.Contains("("))
                continue;
            keys.insert(standard_output);
        }

        float contentSize = keys.fillContent(display,10f);
        display.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, contentSize);
    }

    class KeyHolder
    {
        public Dictionary<string,KeyHolder> subkeys = new Dictionary<string, KeyHolder>();
        public KeyHolder parent;
        public string name;

        public KeyHolder(string name)
        {
            this.name = name;
        }

        public void insert(string name)
        {
            if (name.Contains("/"))
            {
                string parentKey = name.Substring(0, name.IndexOf("/"));
                string newName = name.Substring(name.IndexOf("/") + 1);
                if(!subkeys.ContainsKey(parentKey))
                    subkeys.Add(parentKey, new KeyHolder(parentKey));
                subkeys[parentKey].insert(newName);
            }
            else
            {
                subkeys.Add(name, new KeyHolder(name));
            }
        }

        public string outputPrint(string push = "")
        {
            string ret = push + name + "\n";
            foreach (KeyHolder k in subkeys.Values)
                ret+=k.outputPrint(push + "|--");
            return ret;
        }

        public float fillContent(RectTransform content, float offset = 0f, int depth = 0)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(content);
            RectTransform rect = go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();

            rect.localScale = new Vector3(1, 1, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 1);
            rect.localPosition = Vector3.zero;

            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, offset, 10f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);

            tmp.fontSize = 8;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            string text = "";
            for (int i = 0; i < depth; i++)
            {
                text += "--";
            }
            text += "> ";
            tmp.text = text + name;

            float temp = 0f;
            foreach (KeyHolder k in subkeys.Values)
            {
                temp += 10f;
                temp += k.fillContent(rect, temp,depth+1);
            }
                

            return temp;
        }
    }
}
