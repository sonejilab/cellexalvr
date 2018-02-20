using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// Represents a list of correlated and anati correlated genes.
/// </summary>
public class CorrelatedGenesList : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public CorrelatedGenesListNode sourceGeneListNode;
    public List<CorrelatedGenesListNode> correlatedGenesList;
    public List<CorrelatedGenesListNode> anticorrelatedGenesList;

    private StatusDisplay statusDisplay;
    private StatusDisplay statusDisplayHUD;
    private StatusDisplay statusDisplayFar;
    private SelectionToolHandler selectionToolHandler;
    private string outputFile = Directory.GetCurrentDirectory() + @"\Resources\correlated_genes.txt";

    private void Start()
    {
        statusDisplay = referenceManager.statusDisplay;
        statusDisplayHUD = referenceManager.statusDisplayHUD;
        statusDisplayFar = referenceManager.statusDisplayFar;
        selectionToolHandler = referenceManager.selectionToolHandler;
        SetVisible(false);
    }
    /// <summary>
    /// Fills the list with genenames. The list has room for 10 correlated and 10 anto correlated genes.
    /// </summary>
    /// <param name="correlatedGenes"> The names of the correlated genes. </param>
    /// <param name="anticorrelatedGenes"> The names of the anti correlated genes. </param>
    public void PopulateList(string geneName, string[] correlatedGenes, string[] anticorrelatedGenes)
    {
        if (correlatedGenes.Length != 10 || anticorrelatedGenes.Length != 10)
        {
            Debug.LogWarning("Correlated genes arrays was not of length 10. Actual lengths: " + correlatedGenes.Length + " and " + anticorrelatedGenes.Length);
            return;
        }
        sourceGeneListNode.textMesh.text = geneName;
        // fill the list
        for (int i = 0; i < 10; i++)
        {
            correlatedGenesList[i].GeneName = correlatedGenes[i];
            anticorrelatedGenesList[i].GeneName = anticorrelatedGenes[i];
        }
    }

    /// <summary>
    /// Activates or deactivates all underlying renderers and colliders.
    /// </summary>
    /// <param name="visible"> True if activating renderers and colliders, false if deactivating. </param>
    public void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = visible;
        }
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = visible;
        }
    }

    /// <summary>
    /// Calculates the genes correlated and anti correlated to a certain gene.
    /// </summary>
    /// <param name="index"> The genes index in the list of previous searches. </param>
    /// <param name="geneName"> The genes name. </param>
    public void CalculateCorrelatedGenes(int index, string geneName)
    {
        StartCoroutine(CalculateCorrelatedGenesCoroutine(index, geneName));
    }

    private IEnumerator CalculateCorrelatedGenesCoroutine(int index, string geneName)
    {
        string args = selectionToolHandler.DataDir + " " + geneName + " " + outputFile;
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\get_correlated_genes.R";
        CellExAlLog.Log("Calculating correlated genes with R script " + CellExAlLog.FixFilePath(rScriptFilePath) + " with the arguments: " + args);
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        var statusId = statusDisplay.AddStatus("Calculating genes correlated to " + geneName);
        var statusIdHUD = statusDisplayHUD.AddStatus("Calculating genes correlated to " + geneName);
        var statusIdFar = statusDisplayFar.AddStatus("Calculating genes correlated to " + geneName);
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
            CellExAlLog.Log("Correlated genes file at " + CellExAlLog.FixFilePath(outputFile) + " was not 2 lines long. Actual length: " + lines.Length);
            //Debug.LogWarning("Correlated genes file at " + outputFile + " was not 2 lines long. Actual length: " + lines.Length);
            yield break;
        }

        string[] correlatedGenes = lines[0].Split(null);
        string[] anticorrelatedGenes = lines[1].Split(null);
        SetVisible(true);
        if (correlatedGenes.Length != 10 || anticorrelatedGenes.Length != 10)
        {
            CellExAlLog.Log("Correlated genes file at " + CellExAlLog.FixFilePath(outputFile) + " was incorrectly formatted.",
                            "\tExpected lengths: 10 plus 10 genes.",
                            "\tActual lengths: " + correlatedGenes.Length + " plus " + anticorrelatedGenes.Length + " genes");
            yield break;
        }
        CellExAlLog.Log("Successfully calculated genes correlated to " + geneName);
        PopulateList(geneName, correlatedGenes, anticorrelatedGenes);
        // set the texture to a happy face :)
        var button = referenceManager.previousSearchesList.correlatedGenesButtons[index];
        button.SetTexture(GetComponentInParent<PreviousSearchesList>().correlatedGenesButtonHighlightedTexture);
        statusDisplay.RemoveStatus(statusId);
        statusDisplayHUD.RemoveStatus(statusIdHUD);
        statusDisplayFar.RemoveStatus(statusIdFar);
    }


}

