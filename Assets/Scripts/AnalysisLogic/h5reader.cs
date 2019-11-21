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

public class h5reader
{
    Process p;
    StreamWriter writer;
    StreamReader reader;
    Dictionary<string, int> chromeLengths;
    Dictionary<string, int> cellname2index;
    Dictionary<string, int> genename2index;
    string[] index2genename;
    string[] index2cellname;
    public bool busy = false;
    public ArrayList _result;
    public Dictionary<String, float[]> _coordResult;

    public float LowestExpression { get; private set; }
    public float HighestExpression { get; private set; }

    private enum FileTypes
    {
        anndata = 0,
        loom = 1
    }

    private FileTypes fileType;
    // Start is called before the first frame update
    public h5reader()
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
        startInfo.Arguments = "ann.py " + file_name;
        p.StartInfo = startInfo;
        p.Start();


        writer = p.StandardInput;

        Thread.Sleep(500);
        reader = p.StandardOutput;

        Thread.Sleep(100);
        var watch = Stopwatch.StartNew();
        if (Path.GetExtension(file_name) == ".loom")
        {
            writer.WriteLine("list(f['col_attrs']['obs_names'][:])");
            fileType = FileTypes.loom;

        }
        else if (Path.GetExtension(file_name) == ".h5ad")
        {
            writer.Write("[i[0] for i in f['obs'][:]]\n");
            fileType = FileTypes.anndata;

        }
        string output = reader.ReadLine();
        output = output.Substring(1, output.Length - 2);
        cellname2index = new Dictionary<string, int>();
        int counter = 0;
        index2cellname = output.Split(',');
        for (int i = 0; i < index2cellname.Length; i++)
        {
            index2cellname[i] = index2cellname[i].Substring(2, index2cellname[i].Length - 3);
            cellname2index.Add(index2cellname[i], i);
            if (i == 100)
                UnityEngine.Debug.Log(index2cellname[i]);

        }

        watch.Stop();
        UnityEngine.Debug.Log("Ellapsed time: " + watch.ElapsedMilliseconds);
        //print(output);

        watch = System.Diagnostics.Stopwatch.StartNew();
        if (fileType == FileTypes.loom)
        {
            writer.Write("list(f['row_attrs']['var_names'][:])\n");
        }
        else if (fileType == FileTypes.anndata)
        {
            writer.Write("[i[0] for i in f['var'][:]]\n");
        }
        output = reader.ReadLine();
        output = output.Substring(1, output.Length - 2);
        genename2index = new Dictionary<string, int>();
        counter = 0;
        index2genename = output.Split(',');

        for (int i = 0; i < index2genename.Length; i++)
        {

            index2genename[i] = index2genename[i].Substring(2, index2genename[i].Length - 3);
            if (i == 100)
                UnityEngine.Debug.Log(index2genename[i]);
            genename2index.Add(index2genename[i], counter++);
        }
        watch.Stop();
        UnityEngine.Debug.Log("Ellapsed time: " + watch.ElapsedMilliseconds);


        watch = System.Diagnostics.Stopwatch.StartNew();

        watch.Stop();

        GetCoords();
    }

    public void GetCoords()
    {
        busy = true;
        var watch = Stopwatch.StartNew();

        _coordResult = new Dictionary<string, float[]>();
            
        if (fileType == FileTypes.loom)
            writer.WriteLine("f['col_attrs']['X_phate'][:,:].tolist()");

        //while (reader.Peek() == 0)
        //    yield return null;


        string output = reader.ReadLine().Replace("[", "").Replace("]", "");
        string[] coords = output.Split(',');

        for (int i = 0; i < 10; i++)
            UnityEngine.Debug.Log(coords[i]);




        watch.Stop();
        UnityEngine.Debug.Log("Reading all coords: " + watch.ElapsedMilliseconds);

        busy = false;
    }


    public IEnumerator colorbygene(string geneName, GraphManager.GeneExpressionColoringMethods coloringMethod)
    {
        busy = true;
        _result = new ArrayList();
        int geneindex = genename2index[geneName.ToUpper()];

        if (fileType == FileTypes.anndata)
            writer.Write("list(f['layers']['spliced'][:," + geneindex + "].data)\n");
        else if (fileType == FileTypes.loom)
            writer.Write("list(f['layers']['spliced'][" + geneindex + ",:][f['layers']['spliced'][" + geneindex + ",:].nonzero()])\n");

        while (reader.Peek() == 0)
            yield return null;

        string output = reader.ReadLine();
        output = output.Substring(1, output.Length - 2);
        string[] splitted = output.Split(',');

        if (fileType == FileTypes.anndata)
            writer.Write("list(f['layers']['spliced'][:," + geneindex + "].nonzero()[0])\n");
        else if (fileType == FileTypes.loom)
            writer.Write("list(f['layers']['spliced'][" + geneindex + ",:].nonzero()[0])\n");

        while (reader.Peek() == 0)
            yield return null;

        output = reader.ReadLine();
        //UnityEngine.Debug.Log(output);
        output = output.Substring(1, output.Length - 2);
        string[] indices = output.Split(',');
        LowestExpression = float.MaxValue;
        HighestExpression = float.MinValue;
        if (coloringMethod == GraphManager.GeneExpressionColoringMethods.EqualExpressionRanges)
        {
            // put results in equally sized buckets

            for (int i = 0; i < splitted.Length; i++)
            {

                float expr = float.Parse(splitted[i]);
                if (expr > HighestExpression)
                {
                    HighestExpression = expr;
                }
                if (expr < LowestExpression)
                {
                    LowestExpression = expr;
                }
                _result.Add(new CellExpressionPair(index2cellname[int.Parse(indices[i])], expr, -1));
            }
            if (HighestExpression == LowestExpression)
            {
                HighestExpression += 1;
            }
            float binSize = (HighestExpression - LowestExpression) / CellexalConfig.Config.GraphNumberOfExpressionColors;

            foreach (CellExpressionPair pair in _result)
            {
                pair.Color = (int)((pair.Expression - LowestExpression) / binSize);
            }
        }
        else
        {
            List<CellExpressionPair> result = new List<CellExpressionPair>();
            LowestExpression = float.MaxValue;
            HighestExpression = float.MinValue;
            // put the same number of results in each bucket, ordered
            for (int i = 0; i < splitted.Length; i++)
            {
                CellExpressionPair newPair = new CellExpressionPair(index2cellname[int.Parse(indices[i])], float.Parse(splitted[i]), -1);
                result.Add(newPair);
                float expr = newPair.Expression;
                if (expr > HighestExpression)
                {
                    HighestExpression = expr;
                }
                if (expr < LowestExpression)
                {
                    LowestExpression = expr;
                }
            }

            if (HighestExpression == LowestExpression)
            {
                HighestExpression += 1;
            }
            // sort the list based on gene expressions
            result.Sort();

            int binsize = result.Count / CellexalConfig.Config.GraphNumberOfExpressionColors;
            for (int j = 0; j < result.Count; ++j)
            {
                result[j].Color = j / binsize;
            }
            _result.AddRange(result);
        }

        busy = false;
    }
}


