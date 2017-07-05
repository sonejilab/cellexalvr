//using HDF5DotNet;
using SQLiter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour
{

    public Cell cell;
    public List<Material> materialList;
    public SQLite database;
    //private ArrayList geneNames;
    private Dictionary<string, Cell> cells;
    //private H5FileId h5file;

    void Awake()
    {
        cells = new Dictionary<string, Cell>();
        //geneNames = new ArrayList();
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

    public Cell GetCell(string label)
    {
        return cells[label];
    }

    public void ColorGraphsByGene(string geneName)
    {
        ArrayList expressions = database.QueryGene(geneName);
        //string[] hdfCells = HdfExtensions.Read1DArray<string>(h5file, "cells");
        //float[] expressions = HdfExtensions.Read1DArray<float>(h5file, "expression/" + geneName);
        foreach (Cell c in cells.Values)
        {
            c.ColorByExpression(0);
        }
        for (int i = 0; i < expressions.Count; ++i)
        {
            string cell = ((CellExpressionPair) expressions[i]).Cell;
            cells[cell].ColorByExpression((int) ((CellExpressionPair) expressions[i]).Expression);
        }

    }

    //public bool GeneExists(string geneName)
    //{
    //    return geneNames.Contains(geneName);
    //}

    //public void SetGeneExpression(string cellName, string geneName, int slot)
    //{
    //    Cell cell;
    //    cells.TryGetValue(cellName, out cell);
    //    cell.SetExpressionData(geneName, slot);
    //    if (!geneNames.Contains(geneName))
    //    {
    //        geneNames.Add(geneName);
    //    }
    //}

}
