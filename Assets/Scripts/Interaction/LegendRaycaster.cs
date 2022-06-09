using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{

    public class LegendRaycaster : MonoBehaviour
    {
        [HideInInspector]
        public int savedGeneExpressionHistogramHitX = -1;

        private ReferenceManager referenceManager;
        private LegendManager legendManager;


        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        private Transform raycastingSource;
        private ControllerModelSwitcher controllerModelSwitcher;
        private int layerMask;
        private float clickStartTime;

        private bool triggerClick = false;
        private bool triggerUp = false;

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
            layerMask = 1 << LayerMask.NameToLayer("EnvironmentButtonLayer");

            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
            CellexalEvents.RightTriggerUp.AddListener(OnTriggerUp);

        }

        private void Update()
        {
            if (!CrossSceneInformation.Normal && !CrossSceneInformation.Tutorial) return;
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);
            if ((!CrossSceneInformation.Normal && !CrossSceneInformation.Tutorial)
                || controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Menu)
            {
                return;
            }
            bool correctModel = referenceManager.rightLaser.enabled;
            if (correctModel)
            {
                Raycast();
                triggerClick = false;
                triggerUp = false;
            }
        }

        private void Raycast()
        {
            raycastingSource = referenceManager.rightLaser.transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward),
                out var hit, 100f, layerMask);
            if (hit.collider && hit.collider.gameObject == legendManager.gameObject)
            {
                if (legendManager.desiredLegend == LegendManager.Legend.GeneExpressionLegend)
                {
                    var localPos = legendManager.WorldToRelativeHistogramPos(hit.point);
                    if (localPos.x >= 0f && localPos.x <= 1f && localPos.y >= 0f && localPos.y <= 1f)
                    {
                        HandleHitGeneExpressionHistogram(localPos);
                        //if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                        if (triggerClick)
                        {
                            // if the trigger was pressed
                            HandleClickDownGeneExpressionHistogram(localPos);
                        }
                        // we hit the gene expression histogram, in the histogram area
                        //else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                        else if (triggerUp)
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
                    if (triggerUp)
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
            if (triggerUp)
            {
                // if the trigger was released
                savedGeneExpressionHistogramHitX = -1;
            }
        }

        private void OnTriggerClick()
        {
            triggerClick = true;
        }

        private void OnTriggerUp()
        {
            triggerUp = true;
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
            int hitIndex = (int)(hit.x * legendManager.geneExpressionHistogram.NumberOfBars);
            if (legendManager.geneExpressionHistogram.selectedArea.activeSelf)
            {
                legendManager.geneExpressionHistogram.DeactivateSelectedArea();
                referenceManager.multiuserMessageSender.SendMessageDeactivateSelectedArea();
            }
            else
            {
                legendManager.geneExpressionHistogram.MoveSelectedArea(hitIndex, savedGeneExpressionHistogramHitX);
                referenceManager.multiuserMessageSender.SendMessageMoveSelectedArea(hitIndex,
                    savedGeneExpressionHistogramHitX);
            }
            savedGeneExpressionHistogramHitX = -1;
        }
    }
}
