using CellexalVR.AnalysisObjects;
using CellexalVR.Extensions;
using CellexalVR.General;
using System.Collections.Generic;
using UnityEngine;
namespace CellexalVR.Interaction
{
    /// <summary>
    /// Represents the list of the 10 previous searches of genes.
    /// </summary>
    public class PreviousSearchesList : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        public List<PreviousSearchesLock> searchLocks = new List<PreviousSearchesLock>();
        public List<CorrelatedGenesPanel> correlatedGenesButtons = new List<CorrelatedGenesPanel>();
        public List<ClickableTextPanel> previousSearchesListNodes = new List<ClickableTextPanel>();

        private GameManager gameManager;

        private void Start()
        {
            gameManager = referenceManager.gameManager;
        }


        /// <summary>
        /// Checks if the list already contains an entry.
        /// </summary>
        /// <param name="name">THe name of the thing in the entry.</param>
        /// <param name="type">The type of the entry.</param>
        /// <param name="coloringMethod">The coloring method that was used.</param>
        /// <returns>True if an entry of this kind was already in the list, false otherwise.</returns>
        public bool Contains(string name, Definitions.Measurement type, GraphManager.GeneExpressionColoringMethods coloringMethod)
        {
            foreach (var node in previousSearchesListNodes)
            {
                if (node.NameOfThing == name && node.Type == type && node.ColoringMethod == coloringMethod)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Adds an entry to the list of saved searches. New entries are added to the top of the list and all entries below that are not locked are pushed down one step. 
        /// If the list is full, the bottom most entry will be removed from the list.
        /// </summary>
        /// <param name="name">The name of the thing we are adding to the list.</param>
        /// <param name="type">The type of the thing.</param>
        /// <param name="coloringMethod">The method of coloring that used.</param>
        /// <returns></returns>
        public string AddEntry(string name, Definitions.Measurement type, GraphManager.GeneExpressionColoringMethods coloringMethod)
        {
            bool pushingDown = false;
            string pushDownName = "";
            Definitions.Measurement pushDownType = Definitions.Measurement.INVALID;
            GraphManager.GeneExpressionColoringMethods pushDownColoringMethod = GraphManager.GeneExpressionColoringMethods.Linear;
            for (int i = 0; i < previousSearchesListNodes.Count; ++i)
            {
                var listNode = previousSearchesListNodes[i];
                // if this node is locked, just move on
                if (searchLocks[i].Locked)
                    continue;
                // if this node is empty, insert what we are inserting and return
                if (listNode.NameOfThing == "")
                {
                    if (!pushingDown)
                    {
                        listNode.ColoringMethod = coloringMethod;
                        listNode.SetText(name, type);
                        return "";
                    }
                    else
                    {
                        listNode.ColoringMethod = pushDownColoringMethod;
                        listNode.SetText(pushDownName, pushDownType);
                        return "";
                    }
                }
                // if we have not started the pushing down, then insert the new entry and save the old entry so we can push it down
                if (!pushingDown)
                {
                    pushingDown = true;
                    pushDownName = listNode.NameOfThing;
                    pushDownType = listNode.Type;
                    pushDownColoringMethod = listNode.ColoringMethod;
                    listNode.ColoringMethod = coloringMethod;
                    listNode.SetText(name, type);
                }
                else
                {
                    // swap the saved entry that should be pushed down to this node
                    var tempPushDownName = listNode.NameOfThing;
                    var tempPushDownType = listNode.Type;
                    var tempPushDownColoringMethod = listNode.ColoringMethod;

                    listNode.ColoringMethod = pushDownColoringMethod;
                    listNode.SetText(pushDownName, pushDownType);

                    pushDownName = tempPushDownName;
                    pushDownType = tempPushDownType;
                    pushDownColoringMethod = tempPushDownColoringMethod;
                }

            }
            return pushDownName;
        }

        /// <summary>
        /// Clears the list.
        /// </summary>
        public void ClearList()
        {
            foreach (var node in previousSearchesListNodes)
            {
                node.SetText("", Definitions.Measurement.INVALID);
            }
            foreach (var lockButton in searchLocks)
            {
                lockButton.Locked = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            gameObject.GetComponentsInChildren<PreviousSearchesLock>(searchLocks);
            gameObject.GetComponentsInChildren<ClickableTextPanel>(previousSearchesListNodes);
            gameObject.GetComponentsInChildren<CorrelatedGenesPanel>(correlatedGenesButtons);
            if (gameObject.activeInHierarchy)
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
#endif
    }

}