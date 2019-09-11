using System.IO;
using UnityEngine;
using System.Collections.Generic;
using CellexalVR.Interaction;

namespace CellexalVR.General
{
    /// <summary>
    /// Looks for html files in the user folder. Stores them in list together with browser. 
    /// </summary>
    public class ReportListGenerator : MonoBehaviour
    {
        public string[] htmlFiles;
        public GameObject reportNodePrefab;
        public SimpleWebBrowser.WebBrowser webBrowser;
        public ReferenceManager referenceManager;

        private float heightIncrement = 1;
        private int nrOfNodes;
        private List<string> files;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            nrOfNodes = 0;
            files = new List<string>();
        }

        /// <summary>
        /// Generates report list. Each html file is represented by a clickable text panel. When panel is clicked web navigates
        /// to that url.
        /// </summary>
        public void GenerateList()
        {
            //ClearList();
            if (Directory.Exists(CellexalUser.UserSpecificFolder))
                htmlFiles = Directory.GetFiles(CellexalUser.UserSpecificFolder, "*.html");
            foreach (string file in htmlFiles)
            {
                if (files.Contains(file))
                {
                    // Node is already in list. Skip it and move on..
                    return;
                }
                GameObject newNode = Instantiate(reportNodePrefab, Vector3.zero, Quaternion.identity);
                newNode.transform.parent = transform;
                newNode.transform.localPosition = new Vector3(0, 0, -heightIncrement * nrOfNodes);
                newNode.transform.localScale = reportNodePrefab.transform.localScale;
                newNode.transform.localRotation = Quaternion.Euler(90, 0, 0);
                ClickableReportPanel panel = newNode.GetComponent<ClickableReportPanel>();
                PanelRaycaster rayCaster = referenceManager.keyboardSwitch.GetComponent<PanelRaycaster>();
                panel.SetMaterials(rayCaster.keyNormalMaterial, rayCaster.keyHighlightMaterial, rayCaster.keyPressedMaterial, new Vector4(1f / files.Count, 1f, 1f, 1f));
                panel.SetText(file);
                panel.webBrowser = webBrowser;
                newNode.SetActive(true);
                files.Add(file);
                nrOfNodes++;
            }
        }

        private void ClearList()
        {
            foreach (ClickableReportPanel node in GetComponentsInChildren<ClickableReportPanel>())
            {
                Destroy(node.gameObject);
            }
            nrOfNodes = 0;
        }

    }
}