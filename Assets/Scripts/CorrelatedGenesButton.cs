using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// This class represents the button that calculates the correlated genes.
/// </summary>
public class CorrelatedGenesButton : MonoBehaviour
{
    public PreviousSearchesListNode listNode;
    public CorrelatedGenesList correlatedGenesList;
    public SelectionToolHandler selectionToolHandler;
    public StatusDisplay statusDisplay;
    private new Renderer renderer;
    private string outputFile = Directory.GetCurrentDirectory() + @"\Assets\Resources\correlated_genes.txt";

    private void Start()
    {
        renderer = GetComponent<Renderer>();
    }
    public void CalculateCorrelatedGenes()
    {
        if (listNode.GeneName == "")
            return;
        StartCoroutine(CalculateCorrelatedGenesCoroutine());
    }

    public void SetTexture(Texture newTexture)
    {
        renderer.material.mainTexture = newTexture;
    }

    IEnumerator CalculateCorrelatedGenesCoroutine()
    {
        var geneName = listNode.GeneName;
        Thread t = new Thread(() => GetCorrelatedGenes(geneName));
        var statusId = statusDisplay.AddStatus("Calculating genes correlated to " + geneName);
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        // r script is done, read the results.
        string[] lines = File.ReadAllLines(outputFile);
        if (lines.Length != 2)
            yield break;


        string[] correlatedGenes = lines[0].Split(null);
        string[] anticorrelatedGenes = lines[1].Split(null);
        correlatedGenesList.SetVisible(true);
        correlatedGenesList.PopulateList(correlatedGenes, anticorrelatedGenes);

        statusDisplay.RemoveStatus(statusId);
    }

    public void GetCorrelatedGenes(string geneName)
    {
        string home = Directory.GetCurrentDirectory();
        using (StreamReader r = new StreamReader(home + "/Assets/Config/config.txt"))
        {
            string rawInput = r.ReadToEnd();
            string[] input = rawInput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string rPath = input[0];
            //print("r start");
            if (!File.Exists(outputFile))
            {
                File.Create(outputFile);
            }
            Debug.Log("R Out: " + RScriptRunner.RunFromCmd(home + @"\Assets\Scripts\R\get_correlated_genes.R", rPath, selectionToolHandler.DataDir + " " + geneName + " " + outputFile));
        }
    }
}

