using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CellexalExtensions;
using UnityEngine;

/// <summary>
/// Represents a list of correlated and anati correlated genes.
/// </summary>
public class CorrelatedGenesList : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public ClickableTextPanel sourceGeneListNode;
    public List<ClickableTextPanel> correlatedGenesList;
    public List<ClickableTextPanel> anticorrelatedGenesList;

    private StatusDisplay statusDisplay;
    private StatusDisplay statusDisplayHUD;
    private StatusDisplay statusDisplayFar;
    private SelectionToolHandler selectionToolHandler;
    private PreviousSearchesListNode listNode;

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
    public void PopulateList(string geneName, Definitions.Measurement type, string[] correlatedGenes, string[] anticorrelatedGenes)
    {
        if (correlatedGenes.Length != 10 || anticorrelatedGenes.Length != 10)
        {
            Debug.LogWarning("Correlated genes arrays was not of length 10. Actual lengths: " + correlatedGenes.Length + " and " + anticorrelatedGenes.Length);
            return;
        }
        sourceGeneListNode.SetText(geneName, type);
        // fill the list
        for (int i = 0; i < 10; i++)
        {
            correlatedGenesList[i].SetText(correlatedGenes[i], Definitions.Measurement.GENE);
            anticorrelatedGenesList[i].SetText(anticorrelatedGenes[i], Definitions.Measurement.GENE);
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
    /// For multiplayer use. Listnode cant be sent as RPC call so send name of node directly.
    /// </summary>
    /// <param name="nodeName"></param>
    /// <param name="type"></param>
    public void CalculateCorrelatedGenes(string nodeName, CellexalExtensions.Definitions.Measurement type)
    {
        StartCoroutine(CalculateCorrelatedGenesCoroutine(nodeName, type));
    }

    /// <summary>
    /// Calculates the genes correlated and anti correlated to a certain gene.
    /// </summary>
    /// <param name="index"> The genes index in the list of previous searches. </param>
    /// <param name="name"> The genes name. </param>
    public void CalculateCorrelatedGenes(PreviousSearchesListNode node, CellexalExtensions.Definitions.Measurement type)
    {
        listNode = node;
        listNode.GetComponentInChildren<CorrelatedGenesButton>().SetPressed(true);
        listNode.SetPressed(true);
        StartCoroutine(CalculateCorrelatedGenesCoroutine(listNode.NameOfThing, type));
    }

    private IEnumerator CalculateCorrelatedGenesCoroutine(string nodeName, CellexalExtensions.Definitions.Measurement type)
    {
        string outputFile = Directory.GetCurrentDirectory() + @"\Resources\" + nodeName + ".correlated.txt";
        string facsTypeArg = (type == CellexalExtensions.Definitions.Measurement.FACS) ? "T" : "F";
        string args = selectionToolHandler.DataDir + " " + nodeName + " " + outputFile + " " + facsTypeArg;
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\get_correlated_genes.R";
        CellexalLog.Log("Calculating correlated genes with R script " + CellexalLog.FixFilePath(rScriptFilePath) + " with the arguments: " + args);
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        var statusId = statusDisplay.AddStatus("Calculating genes correlated to " + nodeName);
        var statusIdHUD = statusDisplayHUD.AddStatus("Calculating genes correlated to " + nodeName);
        var statusIdFar = statusDisplayFar.AddStatus("Calculating genes correlated to " + nodeName);
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        CellexalLog.Log("Correlated genes R script finished in " + stopwatch.Elapsed.ToString());
        // r script is done, read the results.
        string[] lines = File.ReadAllLines(outputFile);
        // if the file is not 2 lines, something probably went wrong
        if (lines.Length != 2)
        {
            CellexalLog.Log("Correlated genes file at " + CellexalLog.FixFilePath(outputFile) + " was not 2 lines long. Actual length: " + lines.Length);
            //Debug.LogWarning("Correlated genes file at " + outputFile + " was not 2 lines long. Actual length: " + lines.Length);
            yield break;
        }

        string[] correlatedGenes = lines[0].Split(null);
        string[] anticorrelatedGenes = lines[1].Split(null);
        SetVisible(true);
        if (correlatedGenes.Length != 10 || anticorrelatedGenes.Length != 10)
        {
            CellexalLog.Log("Correlated genes file at " + CellexalLog.FixFilePath(outputFile) + " was incorrectly formatted.",
                            "\tExpected lengths: 10 plus 10 genes.",
                            "\tActual lengths: " + correlatedGenes.Length + " plus " + anticorrelatedGenes.Length + " genes");
            yield break;
        }
        CellexalLog.Log("Successfully calculated genes correlated to " + nodeName);
        PopulateList(nodeName, type, correlatedGenes, anticorrelatedGenes);
        statusDisplay.RemoveStatus(statusId);
        statusDisplayHUD.RemoveStatus(statusIdHUD);
        statusDisplayFar.RemoveStatus(statusIdFar);
        if (listNode)
        {
            listNode.SetPressed(false);
            listNode.GetComponentInChildren<CorrelatedGenesButton>().SetPressed(false);
        }
    }


}

