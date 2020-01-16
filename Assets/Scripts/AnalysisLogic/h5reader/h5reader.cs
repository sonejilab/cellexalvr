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
    public string[,] _sep_coordResult;
    public string[] _velResult;
    public string filePath;
    public Dictionary<string, string> conf;
    public string conditions;
    public bool sparse;
    public bool geneXcell;
    public List<string> projections;


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
        conf = new Dictionary<string, string>();

        UnityEngine.Debug.Log(path);
        string[] files = Directory.GetFiles(path);
        string configFile = "";
        
        foreach(string s in files)
        {
            if (s.EndsWith(".conf"))
                configFile = s;
            else if (s.EndsWith(".loom") || s.EndsWith(".h5ad"))
                filePath = s;
        }
        

        if (configFile == "")
        {
            UnityEngine.Debug.Log("No config file for " + path);
        }
        else
        {
            projections = new List<string>();
            string[] lines = File.ReadAllLines(configFile);
            foreach (string l in lines)
            {
                if (l == "")
                    continue;
                UnityEngine.Debug.Log(l);
                string[] kvp = l.Split(new char[] { ' ' }, 2);
                if (kvp[0] == "sparse")
                    sparse = bool.Parse(kvp[1]);
                else if (kvp[0] == "gene_x_cell")
                    geneXcell = bool.Parse(kvp[1]);
                else if (kvp[0].StartsWith("X") || kvp[0].StartsWith("Y")) {
                    string proj = kvp[0].Split('_')[1];
                    if (!projections.Contains(proj))
                        projections.Add(proj);

                    conf.Add(kvp[0], "f['" + kvp[1] + "']");
                }   
                else if (kvp[0].StartsWith("custom"))
                    conf.Add(kvp[0], kvp[1]);
                else
                    conf.Add(kvp[0], "f['" + kvp[1] + "']");



            }
        }
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

        string file_name = filePath;
        startInfo.Arguments = "ann.py " + file_name;
        p.StartInfo = startInfo;
        p.Start();


        writer = p.StandardInput;

        yield return null;  

        reader = p.StandardOutput;

        yield return null;

        var watch = Stopwatch.StartNew();
        if (conf.ContainsKey("custom_cellnames"))
            writer.WriteLine(conf["custom_cellnames"]);
        else
            writer.WriteLine(conf["cellnames"] + "[:].tolist()");

        while (reader.Peek() == 0)
            yield return null;

        string output = reader.ReadLine();
        output = output.Substring(1, output.Length - 2);
        cellname2index = new Dictionary<string, int>();
        index2cellname = output.Split(',');
        for (int i = 0; i < index2cellname.Length; i++)
        {
            index2cellname[i] = index2cellname[i].Replace(" ", "").Replace("'", "");
            
            if(!cellname2index.ContainsKey(index2cellname[i]))
                cellname2index.Add(index2cellname[i], i);
 
            if (i == 0 || i == 1 || i == index2cellname.Length - 1)
                UnityEngine.Debug.Log(index2cellname[i]);
                
            if (i % (index2cellname.Length / 3) == 0)
                yield return null;
        }
        if (conf.ContainsKey("custom_genenames"))
            writer.WriteLine(conf["custom_genenames"]);
        else
            writer.WriteLine(conf["genenames"] + "[:].tolist()");

        
        while (reader.Peek() == 0)
            yield return null;
        output = reader.ReadLine();
        output = output.Substring(1, output.Length - 2);
        genename2index = new Dictionary<string, int>();
        index2genename = output.Split(',');
        for (int i = 0; i < index2genename.Length; i++)
        {
            index2genename[i] = index2genename[i].Replace(" ", "").Replace("'", "").ToUpper();

            if (i == 0 || i == 1 || i == index2genename.Length - 1)
                UnityEngine.Debug.Log(index2genename[i]);

            if(!genename2index.ContainsKey(index2genename[i]))
                genename2index.Add(index2genename[i], i);

            if (i % (index2genename.Length / 3) == 0)
                yield return null;
        }
        watch.Stop();

        UnityEngine.Debug.Log("H5reader booted and read all names in " + watch.ElapsedMilliseconds + " ms");
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
        string output;
        if (boi == "sep") {
            UnityEngine.Debug.Log("We got it");
            conditions = "2D_sep";
            writer.WriteLine(conf["X_sep"] + "[:].tolist()");
            while (reader.Peek() == 0)
                yield return null;

            
            output = reader.ReadLine().Replace("[", "").Replace("]", "");
            string[] Xcoords = output.Split(',');

            writer.WriteLine(conf["Y_sep"] + "[:].tolist()");
            while (reader.Peek() == 0)
                yield return null;

            output = reader.ReadLine().Replace("[", "").Replace("]", "");
            string[] Ycoords = output.Split(',');

            _coordResult = Xcoords.Concat(Ycoords).ToArray();
            

        }
        else
        {
            writer.WriteLine(conf["X_" + boi] + "[:,:].tolist()");

            while (reader.Peek() == 0)
                yield return null;

            output = reader.ReadLine().Replace("[", "").Replace("]", "");
            string[] coords = output.Split(',');

            _coordResult = coords;
        }


        watch.Stop();
        UnityEngine.Debug.Log("Reading all coords: " + watch.ElapsedMilliseconds);
        busy = false;
    }

    /// <summary>
    /// Get the phate velocities from the file
    /// </summary>
    /// <returns>_velResult</returns>
    public IEnumerator GetVelocites(string graph = "_phate")
    {
        busy = true;
        var watch = Stopwatch.StartNew();


        writer.WriteLine(conf["vel" + graph] + " [:,:].tolist()");
        while (reader.Peek() == 0)
            yield return null;


        string output = reader.ReadLine().Replace("[", "").Replace("]", "");
        _velResult = output.Split(',');

        watch.Stop();
        UnityEngine.Debug.Log("Read all velocities for "+graph+" in " + watch.ElapsedMilliseconds);
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




        if (geneXcell)
        {
            if (sparse)
                writer.WriteLine(conf["cellexpr"] + "[" + geneindex + ",:].data.tolist()");
            else
                writer.WriteLine(conf["cellexpr"] +"[" + geneindex + ",:]["+conf["cellexpr"] + "[" + geneindex + ",:].nonzero()].tolist()");
        }
        else
        {
            if (sparse)
                writer.WriteLine(conf["cellexpr"] + "[:," + geneindex + "].data.tolist()");
            else
                writer.WriteLine(conf["cellexpr"] + "[:," + geneindex + "][" + conf["cellexpr"] + "[:," + geneindex + "].nonzero()].tolist()");
        }


        while (reader.Peek() == 0)
            yield return null;

        string output = reader.ReadLine();
        output = output.Substring(1, output.Length - 2);
        string[] splitted = output.Split(',');



        if (geneXcell)
            writer.WriteLine(conf["cellexpr"] + "[" + geneindex + ",:].nonzero()[0].tolist()");
        else
            writer.WriteLine(conf["cellexpr"] + "[:," + geneindex + "].nonzero()[0].tolist()");

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
                try
                {
                    _result.Add(new CellExpressionPair(index2cellname[int.Parse(indices[i])], expr, -1));

                }catch(Exception e)
                {
                    UnityEngine.Debug.Log(indices[i]);
                    break;
                }
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


