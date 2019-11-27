using System;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{

    public class LegendRaycaster : MonoBehaviour
    {
        private ReferenceManager referenceManager;
        private LegendManager legendManager;
        private SteamVR_Controller.Device device;
        private SteamVR_TrackedObject rightController;
        private Transform raycastingSource;
        private ControllerModelSwitcher controllerModelSwitcher;
        private int layerMask;
        private int savedGeneExpressionHistogramHitX = -1;

        private void Start()
        {

            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            if (CrossSceneInformation.Normal)
            {
                rightController = referenceManager.rightController;
                raycastingSource = rightController.transform;
                controllerModelSwitcher = referenceManager.controllerModelSwitcher;
                legendManager = gameObject.GetComponent<LegendManager>();
            }
            layerMask = 1 << LayerMask.NameToLayer("GraphLayer");

        }

        void Update()
        {
            if (device == null && CrossSceneInformation.Normal)
            {
                device = SteamVR_Controller.Input((int)rightController.index);
            }

            if (CrossSceneInformation.Normal || CrossSceneInformation.Tutorial)
            {
                bool correctModel = controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
                                    || controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard
                                    || controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.WebBrowser;
                if (correctModel)
                {
                    Raycast();
                }
            }
        }

        private void Raycast()
        {
            raycastingSource = referenceManager.rightLaser.transform;
            RaycastHit hit;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit, 100f, layerMask);
            if (hit.collider && hit.collider.gameObject == legendManager.gameObject)
            {
                if (legendManager.activeLegend == LegendManager.Legend.GeneExpressionLegend)
                {
                    var localPos = legendManager.WorldToRelativeHistogramPos(hit.point);
                    if (localPos.x >= 0f && localPos.x <= 1f && localPos.y >= 0f && localPos.y <= 1f)
                    {
                        HandleHitGeneExpressionHistogram(localPos);
                        // we hit the gene expression histogram, in the histogram area
                        if (device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
                        {
                            // if the trigger was pressed
                            HandleClickDownGeneExpressionHistogram(localPos);
                        }
                        else if (device.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
                        {
                            // if the trigger was released
                            HandleClickUpGeneExpressionHistogram(localPos);
                        }
                    }
                    else
                    {
                        // we hit the legend but not the right area
                        legendManager.geneExpressionHistogram.DeactivateHighlightArea();
                    }
                }
            }
            else
            {
                // we hit nothing of interest
                legendManager.geneExpressionHistogram.DeactivateHighlightArea();
            }
        }

        private void HandleHitGeneExpressionHistogram(Vector3 hit)
        {
            int hitIndex = (int)(hit.x * legendManager.geneExpressionHistogram.NumberOfBars);

            if (savedGeneExpressionHistogramHitX != -1)
            {
                legendManager.geneExpressionHistogram.MoveHighlightArea(hitIndex, savedGeneExpressionHistogramHitX);
            }
            else
            {
                legendManager.geneExpressionHistogram.MoveHighlightArea(hitIndex, hitIndex);
            }
        }

        private void HandleClickDownGeneExpressionHistogram(Vector3 hit)
        {
            savedGeneExpressionHistogramHitX = (int)(hit.x * legendManager.geneExpressionHistogram.NumberOfBars);
        }

        private void HandleClickUpGeneExpressionHistogram(Vector3 hit)
        {
            int hitIndex = (int)(hit.x * legendManager.geneExpressionHistogram.NumberOfBars);
            legendManager.geneExpressionHistogram.MoveSelectedArea(hitIndex, savedGeneExpressionHistogramHitX);
            savedGeneExpressionHistogramHitX = -1;
        }
    }
}
