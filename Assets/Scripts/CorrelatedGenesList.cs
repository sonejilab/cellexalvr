using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents a list of correlated and anati correlated genes.
/// </summary>
public class CorrelatedGenesList : MonoBehaviour
{
    public CorrelatedGenesListNode sourceGeneListNode;
    public List<CorrelatedGenesListNode> correlatedGenesList;
    public List<CorrelatedGenesListNode> anticorrelatedGenesList;

    private void Start()
    {
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
}

