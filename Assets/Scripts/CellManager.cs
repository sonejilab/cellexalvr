using SQLiter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using VRTK;

/// <summary>
/// This class represent a manager that holds all the cells.
/// </summary>
public class CellManager : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public List<Material> materialList;
    public VRTK_ControllerActions controllerActions;

    private SQLite database;
    private SteamVR_TrackedObject rightController;
    private PreviousSearchesListNode topListNode;
    private Dictionary<string, Cell> cells;
    private GameManager gameManager;
    private SelectionToolHandler selectionToolHandler;
    private GraphManager graphManager;
    private int coroutinesWaiting;

    void Awake()
    {
        cells = new Dictionary<string, Cell>();
    }

    private void Start()
    {
        database = referenceManager.database;
        rightController = referenceManager.rightController;
        topListNode = referenceManager.topListNode;
        gameManager = referenceManager.gameManager;
        selectionToolHandler = referenceManager.selectionToolHandler;
        graphManager = referenceManager.graphManager;
    }

    /// <summary>
    /// Attempts to add a cell to the dictionary
    /// </summary>
    /// <param name="label"> The cell's name </param>
    /// <returns> Returns a reference to the added cell </returns>

    public Cell AddCell(string label)
    {
        if (!cells.ContainsKey(label))
        {
            cells[label] = new Cell(label, materialList);
        }
        return cells[label];
    }

    public void CreateNewSelectionFromArray(string graphName, string[] cellnames, Color[] colors)
    {
        // finds any graph
        Graph graph = graphManager.FindGraph(graphName);
        for (int i = 0; i < cellnames.Length; ++i)
        {
            Cell cell = cells[cellnames[i]];
            selectionToolHandler.AddGraphpointToSelection(graph.points[cellnames[i]], colors[i], false);
            cell.SetColor(colors[i]);
        }
    }

    /// <summary>
    /// Toggles all cells which have an expression level > 0 by showing / hiding them from the graphs.
    /// </summary>
    public void ToggleExpressedCells()
    {
        foreach (Cell c in cells.Values)
        {
            if (c.ExpressionLevel > 0)
            {
                c.RemoveFromGraphs();
            }
        }
    }
    /// <summary>
    /// Toggles all cells which have an expression level == 0 by showing / hiding them from the graphs.
    /// </summary>
    public void ToggleNonExpressedCells()
    {
        foreach (Cell c in cells.Values)
        {
            if (c.ExpressionLevel == 0)
            {
                c.RemoveFromGraphs();
            }
        }
    }

    public Cell GetCell(string label)
    {
        return cells[label];
    }

    /// <summary>
    /// Color all cells based on a gene previously colored by
    /// </summary>
    public void ColorGraphsByPreviousExpression(string geneName)
    {
        foreach (Cell c in cells.Values)
        {
            c.ColorByPreviousExpression(geneName);
        }
        GetComponent<AudioSource>().Play();
        //Debug.Log("FEEL THE PULSE");
        SteamVR_Controller.Input((int)rightController.index).TriggerHapticPulse(2000);
    }



    /// <summary>
    /// Colors all GraphPoints in all current Graphs based on their expression of a gene.
    /// </summary>
    /// <param name="geneName"> The name of the gene. </param>
    public void ColorGraphsByGene(string geneName)
    {
        //SteamVR_Controller.Input((int)right.controllerIndex).TriggerHapticPulse(2000);
        controllerActions.TriggerHapticPulse(2000, (ushort)600, 0);
        StartCoroutine(QueryDatabase(geneName));
    }

    private IEnumerator QueryDatabase(string geneName)
    {
        if (coroutinesWaiting >= 1)
        {
            // If there is already another query  waiting for the current to finish we should probably abort.
            // This is just to make sure that a bug can't create many many coroutines that will form a long queue.
            CellExAlLog.Log("WARNING: Not querying database for " + geneName + " because there is already a query waiting.");
            yield break;
        }
        coroutinesWaiting++;

        // if there is already a query running, wait for it to finish
        while (database.QueryRunning)
            yield return null;

        coroutinesWaiting--;
        database.QueryGene(geneName);

        // now we have to wait for our query to return the results.
        while (database.QueryRunning)
            yield return null;

        GetComponent<AudioSource>().Play();
        SteamVR_Controller.Input((int)rightController.index).TriggerHapticPulse(2000);
        ArrayList expressions = database._result;
        // stop the coroutine if the gene was not in the database
        if (expressions.Count == 0)
        {
            CellExAlLog.Log("WARNING: The gene " + geneName + " was not found in the database");
            yield break;
        }
        foreach (Cell c in cells.Values)
        {
            c.ColorByExpression(0);
        }
        for (int i = 0; i < expressions.Count; ++i)
        {
            string cell = ((CellExpressionPair)expressions[i]).Cell;
            cells[cell].ColorByExpression((int)((CellExpressionPair)expressions[i]).Expression);
        }

        var removedGene = topListNode.UpdateList(geneName);
        //Debug.Log(topListNode.GeneName);
        foreach (Cell c in cells.Values)
        {
            c.SaveExpression(geneName, removedGene);
        }
        CellExAlLog.Log("Colored " + expressions.Count + " points according to the expression of " + geneName);
    }

    public void DeleteCells()
    {
        cells.Clear();
    }

    /// <summary>
    /// Color all cells that belong to a certain attribute.
    /// </summary>
    public void ColorByAttribute(string attributeType, Color color)
    {
        CellExAlLog.Log("Colored genes by " + attributeType);
        foreach (Cell cell in cells.Values)
        {
            cell.ColorByAttribute(attributeType, color);
        }
    }

    /// <summary>
    /// Adds an attribute to a cell. 
    /// </summary>
    /// <param name="cellname"> The cells name. </param>
    /// <param name="attributeType"> The attribute type / name </param>
    /// <param name="attribute"> The attribute value </param>
    public void AddAttribute(string cellname, string attributeType, string attribute)
    {
        cells[cellname].AddAttribute(attributeType, attribute);
    }

    internal void AddFacs(string cellName, string facs, int index)
    {
        if (index < 0 || index > 29)
        {
            // value hasn't been normalized correctly
            print(facs + " " + index);
        }
        cells[cellName].AddFacs(facs, index);
    }

    /// <summary>
    /// Color all graphpoints according to a column in the index.facs file
    /// </summary>
    public void ColorByIndex(string name)
    {
        CellExAlLog.Log("Colored genes by " + name);
        foreach (Cell cell in cells.Values)
        {
            cell.ColorByIndex(name);
        }
    }
}
