using System;
using System.Collections.Generic;
using CellexalVR.AnalysisLogic;
using UnityEngine;

namespace CellexalVR.Filters
{
    /// <summary>
    /// Represents a filter that can be used with the selection tool to only select some cells that fulfill some criteria.
    /// </summary>
    public class Filter
    {
        public BooleanExpression.Expr Expression { get; set; }

        /// <summary>
        /// Checks if a cell passes this filter.
        /// </summary>
        /// <param name="cell">The cell to check.</param>
        /// <returns>True if the cell passed this filter, false otherwise.</returns>
        /// 
        public bool Pass(Cell cell)
        {
            return Expression.Eval(cell);
        }

        public List<string> GetGenes(bool onlyPercent = false)
        {
            List<string> result = new List<string>();
            Expression.GetGenes(ref result, onlyPercent);
            return result;
        }

        public List<string> GetFacs(bool onlyPercent = false)
        {
            List<string> result = new List<string>();
            Expression.GetFacs(ref result, onlyPercent);
            return result;
        }

        public List<string> GetAttributes()
        {
            List<string> result = new List<string>();
            Expression.GetAttributes(ref result);
            return result;
        }

    }
}