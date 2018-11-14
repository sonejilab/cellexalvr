using System.IO;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class ReportListGenerator : MonoBehaviour
{
    public string[] htmlFiles;
    public GameObject reportNodePrefab;
    public SimpleWebBrowser.WebBrowser webBrowser;

    private float heightIncrement = 1;
    private int nrOfNodes;
    private List<string> files;

    private void Start()
    {
        nrOfNodes = 0;
        files = new List<string>();
        GenerateList();
    }

    public void GenerateList()
    {
        //ClearList();
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
            newNode.transform.localPosition = new Vector3(0, 0, - heightIncrement * nrOfNodes);
            newNode.transform.localScale = reportNodePrefab.transform.localScale;
            newNode.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ClickableReportPanel panel = newNode.GetComponent<ClickableReportPanel>();
            PanelRaycaster rayCaster = GetComponent<PanelRaycaster>();
            panel.SetMaterials(rayCaster.keyNormalMaterial, rayCaster.keyHighlightMaterial, rayCaster.keyPressedMaterial);
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
