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
    public ReferenceManager referenceManager;
    public PreviousSearchesListNode listNode;

    private CorrelatedGenesList correlatedGenesList;
    private SelectionToolHandler selectionToolHandler;
    private StatusDisplay statusDisplay;
    private bool calculatingGenes = false;
    private new Renderer renderer;
    private string outputFile = Directory.GetCurrentDirectory() + @"\Resources\correlated_genes.txt";

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        correlatedGenesList = referenceManager.correlatedGenesList;
        selectionToolHandler = referenceManager.selectionToolHandler;
        statusDisplay = referenceManager.statusDisplay;
    }

    /// <summary>
    /// Runs the R script that calculates the correlated and anti correlated genes and populates the lists with those genes.
    /// </summary>
    public void CalculateCorrelatedGenes()
    {
        if (listNode.GeneName == "")
            return;
        StartCoroutine(CalculateCorrelatedGenesCoroutine());
    }
    /// <summary>
    /// Sets the texture of this button.
    /// </summary>
    /// <param name="newTexture"> The new texture. </param>
    public void SetTexture(Texture newTexture)
    {
        if (!calculatingGenes)
        {
            renderer.material.mainTexture = newTexture;
        }
    }

    IEnumerator CalculateCorrelatedGenesCoroutine()
    {
        calculatingGenes = true;
        var geneName = listNode.GeneName;
        string args = selectionToolHandler.DataDir + " " + geneName + " " + outputFile;
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\get_correlated_genes.R";
        CellExAlLog.Log("Calculating correlated genes with R script " + rScriptFilePath + " with the arguments: " + args);
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        var statusId = statusDisplay.AddStatus("Calculating genes correlated to " + geneName);
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        CellExAlLog.Log("Correlated genes R script finished in " + stopwatch.Elapsed.ToString());
        // r script is done, read the results.
        string[] lines = File.ReadAllLines(outputFile);
        // if the file is not 2 lines, something probably went wrong
        if (lines.Length != 2)
        {
            CellExAlLog.Log("Correlated genes file at " + outputFile + " was not 2 lines long. Actual length: " + lines.Length);
            Debug.LogWarning("Correlated genes file at " + outputFile + " was not 2 lines long. Actual length: " + lines.Length);
            yield break;
        }

        string[] correlatedGenes = lines[0].Split(null);
        string[] anticorrelatedGenes = lines[1].Split(null);
        correlatedGenesList.SetVisible(true);
        if (correlatedGenes.Length != 10 || anticorrelatedGenes.Length != 10)
        {
            CellExAlLog.Log("Correlated genes file at " + outputFile + " was incorrectly formatted.",
                            "\tExpected lengths: 10 plus 10 genes.",
                            "\tActual lengths: " + correlatedGenes.Length + " plus " + anticorrelatedGenes.Length + " genes");
            yield break;
        }
        CellExAlLog.Log("Successfully calculated genes correlated to " + geneName);
        correlatedGenesList.PopulateList(geneName, correlatedGenes, anticorrelatedGenes);
        // set the texture to a happy face :)
        calculatingGenes = false;
        SetTexture(GetComponentInParent<PreviousSearchesList>().correlatedGenesButtonHighlightedTexture);
        statusDisplay.RemoveStatus(statusId);
    }
}
