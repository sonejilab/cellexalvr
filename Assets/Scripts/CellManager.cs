﻿using SQLiter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class represent a manager that holds all the cells.
/// </summary>
public class CellManager : MonoBehaviour
{

    public Cell cell;
    public List<Material> materialList;
    public SQLite database;
    private Dictionary<string, Cell> cells;
    public SteamVR_TrackedController right;

    void Awake()
    {
        cells = new Dictionary<string, Cell>();
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
    public void ColorGraphsByPreviousExpression(int index)
    {

        foreach (Cell c in cells.Values)
        {
            c.ColorByPreviousExpression(index);
        }
        GetComponent<AudioSource>().Play();
        Debug.Log("FEEL THE PULSE");
        SteamVR_Controller.Input((int)right.controllerIndex).TriggerHapticPulse(2000);
    }
    


    /// <summary>
    /// Colors all GraphPoints in all current Graphs based on their expression of a gene.
    /// </summary>
    /// <param name="geneName"> The name of the gene. </param>
    public void ColorGraphsByGene(string geneName)
    {
        StartCoroutine(QueryDatabase(geneName));
    }

    private IEnumerator QueryDatabase(string geneName)
    {
        // if there is already a query running, wait for it to finish
        while (database.QueryRunning)
            yield return null;

        database.QueryGene(geneName);

        while (database.QueryRunning)
            yield return null;

        GetComponent<AudioSource>().Play();
        Debug.Log("FEEL THE PULSE");
        SteamVR_Controller.Input((int)right.controllerIndex).TriggerHapticPulse(2000);
        ArrayList expressions = database._result;
        foreach (Cell c in cells.Values)
        {
            c.ColorByExpression(0);
        }
        for (int i = 0; i < expressions.Count; ++i)
        {
            string cell = ((CellExpressionPair)expressions[i]).Cell;
            cells[cell].ColorByExpression((int)((CellExpressionPair)expressions[i]).Expression);
        }
        foreach (Cell c in cells.Values)
        {
            c.SaveExpression();
        }
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
            // value hasn't been normalized ocrrectly
            print(facs + " " + index);
        }
        cells[cellName].AddFacs(facs, index);
    }

    /// <summary>
    /// Color all graphpoints according to a column in the index.facs file
    /// </summary>
    public void ColorByIndex(string name)
    {
        foreach (Cell cell in cells.Values)
        {
            cell.ColorByIndex(name);
        }
    }
}
