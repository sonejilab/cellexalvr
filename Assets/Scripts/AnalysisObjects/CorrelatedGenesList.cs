using CellexalVR.AnalysisLogic;
using CellexalVR.Extensions;
using CellexalVR.General;
using CellexalVR.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{

    /// <summary>
    /// Represents a list of correlated and anati correlated genes.
    /// </summary>
    public class CorrelatedGenesList : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject listNodePrefab;
        public ClickableTextPanel sourceGeneListNode;
        public List<GameObject> listNodes;
        public List<ClickableTextPanel> correlatedPanels;
        public List<ClickableTextPanel> anticorrelatedPanels;

        //private StatusDisplay statusDisplay;
        //private StatusDisplay statusDisplayHUD;
        //private StatusDisplay statusDisplayFar;
        //private SelectionToolHandler selectionToolHandler;
        private SelectionManager selectionManager;

        private void Start()
        {
            //statusDisplay = referenceManager.statusDisplay;
            //statusDisplayHUD = referenceManager.statusDisplayHUD;
            //statusDisplayFar = referenceManager.statusDisplayFar;
            selectionManager = referenceManager.selectionManager;
            SetVisible(false);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
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
                correlatedPanels[i].SetText(correlatedGenes[i], Definitions.Measurement.GENE);
                anticorrelatedPanels[i].SetText(anticorrelatedGenes[i], Definitions.Measurement.GENE);
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

        void TurnOff()
        {
            SetVisible(false);
        }

        /// <summary>
        /// For multiplayer use. Listnode cant be sent as RPC call so send name of node directly.
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="type"></param>
        public void CalculateCorrelatedGenes(string nodeName, Extensions.Definitions.Measurement type)
        {
            StartCoroutine(CalculateCorrelatedGenesCoroutine(nodeName, type));
        }

        /// <summary>
        /// Calculates the genes correlated and anti correlated to a certain gene.
        /// </summary>
        /// <param name="index"> The genes index in the list of previous searches. </param>
        /// <param name="name"> The genes name. </param>
        public void CalculateCorrelatedGenes(ClickableTextPanel node, Extensions.Definitions.Measurement type)
        {
            StartCoroutine(CalculateCorrelatedGenesCoroutine(node.NameOfThing, type));
        }


        private IEnumerator CalculateCorrelatedGenesCoroutine(string nodeName, Extensions.Definitions.Measurement type)
        {
            //var statusId = statusDisplay.AddStatus("Calculating genes correlated to " + nodeName);
            //var statusIdHUD = statusDisplayHUD.AddStatus("Calculating genes correlated to " + nodeName);
            //var statusIdFar = statusDisplayFar.AddStatus("Calculating genes correlated to " + nodeName);
            string function = "get.genes.cor.to";
            string outputFile = (CellexalUser.UserSpecificFolder + @"\Resources\" + nodeName + ".correlated.txt").UnFixFilePath();
            string facsTypeArg = (type == Extensions.Definitions.Measurement.FACS) ? "TRUE" : "FALSE";
            string args = "cellexalObj" + ", \"" + nodeName + "\", \"" + outputFile + "\", " + facsTypeArg;

            string script = function + "(" + args + ")";
            // First wait until other processes are finished before trying to start this one.
            while (File.Exists(CellexalUser.UserSpecificFolder + "\\server.input.R"))
            {
                yield return null;
            }
            CellexalLog.Log("Calculating correlated genes with R script " + script);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            RScriptRunner.RunScript(script);

            // Wait for this process to finish.
            while (File.Exists(CellexalUser.UserSpecificFolder + "\\server.input.R"))
            {
                yield return null;
            }

            // wait some frames for files to be created.
            //for (int i = 0; i < 10; i++)
            //{
            //}
            yield return null;
            stopwatch.Stop();
            CellexalLog.Log("Correlated genes R script finished in " + stopwatch.Elapsed.ToString());
            // r script is done, read the results.
            string[] lines = File.ReadAllLines(outputFile);

            // if the file is not 2 lines, something probably went wrong
            if (lines.Length != 2)
            {
                CellexalLog.Log("Correlated genes file at " + CellexalLog.FixFilePath(outputFile) + " was not 2 lines long. Actual length: " + lines.Length);
                yield break;
                //Debug.LogWarning("Correlated genes file at " + outputFile + " was not 2 lines long. Actual length: " + lines.Length);
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
            CellexalEvents.CorrelatedGenesCalculated.Invoke();
            referenceManager.notificationManager.SpawnNotification("Correlated genes calculation finished.");
            //statusDisplay.RemoveStatus(statusId);
            //statusDisplayHUD.RemoveStatus(statusIdHUD);
            //statusDisplayFar.RemoveStatus(statusIdFar);
        }
        




#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy || Event.current != null && Event.current.type == EventType.Repaint)
                return;

            // remove null entries
            for (int i = 0; i < listNodes.Count; ++i)
            {
                if (listNodes[i] == null)
                {
                    listNodes.RemoveAt(i);
                    i--;
                }
            }


            BuildList();
        }

        public void BuildList()
        {
            int numberOfPanels = referenceManager.previousSearchesList.numberOfPanels;
            if (numberOfPanels < 1)
            {
                return;
            }
            Mesh quadPrefab = referenceManager.previousSearchesList.quadPrefab;
            // remove old nodes
            foreach (var oldNode in listNodes)
            {
                if (oldNode != null)
                {
                    UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(oldNode.gameObject); };
                }
            }

            listNodes.Clear();
            correlatedPanels.Clear();
            anticorrelatedPanels.Clear();

            PanelRaycaster panelRaycaster = gameObject.GetComponentInParent<PanelRaycaster>();

            // generate new nodes
            for (int i = 0; i < numberOfPanels; ++i)
            {
                GameObject newCorrelatedPanel = Instantiate(listNodePrefab, gameObject.transform);
                newCorrelatedPanel.SetActive(true);
                GameObject newAnticorrelatedPanel = Instantiate(listNodePrefab, gameObject.transform);
                newAnticorrelatedPanel.SetActive(true);
                // get the child components
                ClickableTextPanel correlatedPanel = newCorrelatedPanel.GetComponentInChildren<ClickableTextPanel>();
                ClickableTextPanel anticorrelatedPanel = newAnticorrelatedPanel.GetComponentInChildren<ClickableTextPanel>();
                // add the components to the lists
                listNodes.Add(newCorrelatedPanel);
                listNodes.Add(newAnticorrelatedPanel);
                correlatedPanels.Add(correlatedPanel);
                anticorrelatedPanels.Add(anticorrelatedPanel);

                // assign meshes
                Mesh previousSearchesListNodeMesh = new Mesh();
                correlatedPanel.GetComponent<MeshFilter>().sharedMesh = quadPrefab;
                anticorrelatedPanel.GetComponent<MeshFilter>().sharedMesh = quadPrefab;
                // assign materials
                correlatedPanel.GetComponent<MeshRenderer>().sharedMaterial = panelRaycaster.keyNormalMaterial;
                anticorrelatedPanel.GetComponent<MeshRenderer>().sharedMaterial = panelRaycaster.keyNormalMaterial;

                newCorrelatedPanel.gameObject.name = "Correlated List Node " + (i + 1);
                newCorrelatedPanel.transform.localPosition = Vector3.zero;
                newAnticorrelatedPanel.gameObject.name = "Anti-Correlated List Node " + (i + 1);
                newAnticorrelatedPanel.transform.localPosition = Vector3.zero;
                // assign positions
                KeyboardItem correlatedItem = newCorrelatedPanel.GetComponent<KeyboardItem>();
                KeyboardItem anticorrelatedItem = newAnticorrelatedPanel.GetComponent<KeyboardItem>();
                correlatedItem.position = new Vector2Int(-12, i);
                correlatedItem.size = new Vector2(3, 1);
                anticorrelatedItem.position = new Vector2Int(-15, i);
                anticorrelatedItem.size = new Vector2(3, 1);
            }
        }

#endif
    }

}

