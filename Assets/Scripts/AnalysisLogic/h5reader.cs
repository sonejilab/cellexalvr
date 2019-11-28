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
    public string[] index2genename;
    public string[] index2cellname;
    public bool busy;
    public ArrayList _result;
    public string[] _coordResult;
    public string[] _velResult;
    public string filePath;

    public float LowestExpression { get; private set; }
    public float HighestExpression { get; private set; }

    private enum FileTypes
    {
        anndata = 0,
        loom = 1
    }

    private FileTypes fileType;
    // Start is called before the first frame update
    /// <summary>
    /// H5reader
    /// </summary>
    /// <param name="path">filename in the Data folder</param>
    public h5reader(string path)
    {
        filePath = path;
    }

    /// <summary>
    /// Coroutine for connecting to the file
    /// </summary>
    /// <returns>All genenames and cellnames from the file are saved in the class</returns>
    public IEnumerator ConnectToFile()
    {
        busy = true;
        p = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;
        //startInfo.CreateNoWindow = true;

        startInfo.FileName = "py.exe";

        string file_name = "Data/" + filePath;
        startInfo.Arguments = "ann.py " + file_name;
        p.StartInfo = startInfo;
        p.Start();


        writer = p.StandardInput;

        yield return null;

        reader = p.StandardOutput;

        yield return null;

        var watch = Stopwatch.StartNew();
        if (Path.GetExtension(file_name) == ".loom")
        {
            writer.WriteLine("f['col_attrs']['obs_names'][:].tolist()");
            fileType = FileTypes.loom;

        }
        else if (Path.GetExtension(file_name) == ".h5ad")
        {
            writer.WriteLine("[i[0] for i in f['obs'][:]]");
            fileType = FileTypes.anndata;

        }

        while (reader.Peek() == 0)
            yield return null;

        string output = reader.ReadLine();
        UnityEngine.Debug.Log(output);
        output = output.Substring(1, output.Length - 2);
        cellname2index = new Dictionary<string, int>();
        index2cellname = output.Split(',');
        for (int i = 0; i < index2cellname.Length; i++)
        {
            if (i > 0)
                index2cellname[i] = index2cellname[i].Substring(2, index2cellname[i].Length - 3);
            else if (i == index2cellname.Length - 1)
                index2cellname[i] = index2cellname[i].Substring(1, index2cellname[i].Length - 2);
            else
                index2cellname[i] = index2cellname[i].Substring(1, index2cellname[i].Length - 2);
            cellname2index.Add(index2cellname[i], i);
            if (i == 0 || i == 1 || i == index2cellname.Length - 1)
                UnityEngine.Debug.Log(index2cellname[i]);

            if (i % (index2cellname.Length / 3) == 0)
                yield return null;
        }

        if (fileType == FileTypes.loom)
        {
            writer.WriteLine("f['row_attrs']['var_names'][:].tolist()");
        }
        else if (fileType == FileTypes.anndata)
        {
            writer.WriteLine("[i[0] for i in f['var'][:]]");
        }
        while (reader.Peek() == 0)
            yield return null;
        output = reader.ReadLine();
        output = output.Substring(1, output.Length - 2);
        genename2index = new Dictionary<string, int>();
        index2genename = output.Split(',');
        int counter = 0;
        for (int i = 0; i < index2genename.Length; i++)
        {

            index2genename[i] = index2genename[i].Substring(2, index2genename[i].Length - 3);
            if (i == 100)
                UnityEngine.Debug.Log(index2genename[i]);
            genename2index.Add(index2genename[i], counter++);

            if (i % (index2genename.Length / 3) == 0)
                yield return null;
        }
        watch.Stop();

        UnityEngine.Debug.Log("H5reader booted and read all names in :" + watch.ElapsedMilliseconds + " ms");
        busy = false;

    }

    /// <summary>
    /// Get 3D coordinates from file
    /// </summary>
    /// <param name="boi">The graph type, (umap or phate)</param>
    /// <returns>Coroutine, use _coordResult</returns>
    public IEnumerator GetCoords(string boi)
    {
        busy = true;
        var watch = Stopwatch.StartNew();
            
        if (fileType == FileTypes.loom)
            writer.WriteLine("f['col_attrs']['X_"+boi+"'][:,:].tolist()");
        else if (fileType == FileTypes.anndata)
            writer.WriteLine("f['obsm']['X_"+boi+"'][:,:].tolist()");
            
        while (reader.Peek() == 0)
            yield return null;

        string output = reader.ReadLine().Replace("[", "").Replace("]", "");
        string[] coords = output.Split(',');

        watch.Stop();
        UnityEngine.Debug.Log("Reading all coords: " + watch.ElapsedMilliseconds);
        _coordResult = coords;
        busy = false;
    }

    /// <summary>
    /// Get the phate velocities from the file
    /// </summary>
    /// <returns>_velResult</returns>
    public IEnumerator GetVelocites()
    {
        busy = true;
        var watch = Stopwatch.StartNew();

        if (fileType == FileTypes.loom)
            writer.WriteLine("f['col_attrs']['velocity_phate'][:,:].tolist()");
        else if (fileType == FileTypes.anndata)
            writer.WriteLine("f['obsm']['velocity_phate'][:,:].tolist()");

        while (reader.Peek() == 0)
            yield return null;


        string output = reader.ReadLine().Replace("[", "").Replace("]", "");
        _velResult = output.Split(',');

        watch.Stop();
        UnityEngine.Debug.Log("Reading all velocities: " + watch.ElapsedMilliseconds);
        busy = false;
    }

    /// <summary>
    /// Reads expressions of gene on all cells, returns list of CellExpressionPair
    /// </summary>
    /// <param name="geneName">gene name</param>
    /// <param name="coloringMethod">no idea</param>
    /// <returns>_result</returns>
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


