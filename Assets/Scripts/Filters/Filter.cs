using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisLogic;

namespace CellexalVR.Filters
{

    public class Filter : MonoBehaviour
    {

        public bool Pass(Cell cell)
        {
            return false;
        }
    }
}