using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using TMPro;

namespace CellexalVR.Interaction
{
    public class AnnotationTextPanel : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private Cell[] cells;

        private LineRenderer line;
        private TextMeshPro textMesh;
        private GraphInfoPanelRotator infoPanel;

        private void Start()
        {
            line = GetComponentInChildren<LineRenderer>();
            textMesh = GetComponentInChildren<TextMeshPro>();
            infoPanel = GetComponentInChildren<GraphInfoPanelRotator>();
        }

        public void SetCells(int group, Vector3 spawnPosition)
        {
            this.cells = ReferenceManager.instance.cellManager.GetCells(group);
            if (line == null)
            {
                line = GetComponentInChildren<LineRenderer>();
            }
            if (textMesh == null)
            {
                textMesh = GetComponentInChildren<TextMeshPro>();
            }
            transform.localPosition = spawnPosition;
            Vector3 dir = (Vector3.zero - transform.localPosition).normalized;
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.zero - (0.2f * dir));
            textMesh.transform.localPosition = Vector3.zero - (0.2f * dir);
        }


        public void Highlight(string toggle)
        {
            referenceManager.cellManager.HighlightCells(cells, toggle.Equals("on"));
        }
    }
}