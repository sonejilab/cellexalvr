using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{

    public class LegendRaycaster : MonoBehaviour
    {
        [HideInInspector]
        public int savedGeneExpressionHistogramHitX = -1;

        private ReferenceManager referenceManager;
        private LegendManager legendManager;
        private Hand rightHand;
        private Transform raycastingSource;
        private ControllerModelSwitcher controllerModelSwitcher;
        private int layerMask;
        private float clickStartTime;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            if (CrossSceneInformation.Normal)
            {
                controllerModelSwitcher = referenceManager.controllerModelSwitcher;
                legendManager = gameObject.GetComponent<LegendManager>();
            }
            layerMask = 1 << LayerMask.NameToLayer("EnvironmentButtonLayer");

        }

        private void Update()
        {
            if ((!CrossSceneInformation.Normal && !CrossSceneInformation.Tutorial) || controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Menu) return; // not currently using legends in these game modes
            if (Player.instance.rightHand == null || Player.instance.leftHand == null) return; // hands are not initiated. can not raycast
            bool correctModel = referenceManager.laserPointerController.rightLaser.active;
            if (correctModel)
            {
                Raycast();
            }
        }

        private void Raycast()
        {
            raycastingSource = referenceManager.laserPointerController.rightLaser.transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward),
                out RaycastHit hit, 100f, layerMask);
            if (hit.collider && hit.collider.gameObject == legendManager.gameObject)
            {
                if (legendManager.desiredLegend == LegendManager.Legend.GeneExpressionLegend)
                {
                    var localPos = legendManager.WorldToRelativeHistogramPos(hit.point);
                    if (localPos.x >= 0f && localPos.x <= 1f && localPos.y >= 0f && localPos.y <= 1f)
                    {
                        HandleHitGeneExpressionHistogram(localPos);
                        if (rightHand.grabPinchAction.GetStateDown(rightHand.handType))
                        {
                            // if the trigger was pressed
                            HandleClickDownGeneExpressionHistogram(localPos);
                        }
                        // we hit the gene expression histogram, in the histogram area
                        else if (rightHand.grabPinchAction.GetStateUp(rightHand.handType))
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
                    if (rightHand.grabPinchAction.GetStateUp(rightHand.handType))
                    {
                        // if the trigger was released
                        savedGeneExpressionHistogramHitX = -1;
                    }
                }
            }
            else
            {
                // we hit nothing of interest
                legendManager.geneExpressionHistogram.DeactivateHighlightArea();
            }
            
            if (rightHand.grabPinchAction.GetStateUp(rightHand.handType))
            {
                // if the trigger was released
                savedGeneExpressionHistogramHitX = -1;
            }
        }

        private void HandleHitGeneExpressionHistogram(Vector3 hit)
        {
            int hitIndex = (int)(hit.x * legendManager.geneExpressionHistogram.NumberOfBars);
            int maxX = savedGeneExpressionHistogramHitX != -1 ? savedGeneExpressionHistogramHitX : hitIndex;
            legendManager.geneExpressionHistogram.MoveHighlightArea(hitIndex, maxX);
            referenceManager.multiuserMessageSender.SendMessageMoveHighlightArea(hitIndex, maxX);
            // if (savedGeneExpressionHistogramHitX != -1)
            // {
            //     legendManager.geneExpressionHistogram.MoveHighlightArea(hitIndex, savedGeneExpressionHistogramHitX);
            // }
            // else
            // {
            //     legendManager.geneExpressionHistogram.MoveHighlightArea(hitIndex, hitIndex);
            // }
        }

        private void HandleClickDownGeneExpressionHistogram(Vector3 hit)
        {
            clickStartTime = Time.time;
            savedGeneExpressionHistogramHitX = (int)(hit.x * legendManager.geneExpressionHistogram.NumberOfBars);
        }

        private void HandleClickUpGeneExpressionHistogram(Vector3 hit)
        {
            if (Time.time - clickStartTime < 0.1f)
            {
                legendManager.geneExpressionHistogram.DeactivateSelectedArea();
                referenceManager.multiuserMessageSender.SendMessageDeactivateSelectedArea();
            }
            else
            {
                int hitIndex = (int)(hit.x * legendManager.geneExpressionHistogram.NumberOfBars);
                legendManager.geneExpressionHistogram.MoveSelectedArea(hitIndex, savedGeneExpressionHistogramHitX);
                referenceManager.multiuserMessageSender.SendMessageMoveSelectedArea(hitIndex,
                    savedGeneExpressionHistogramHitX);
                savedGeneExpressionHistogramHitX = -1;
            }
        }
    }
}
