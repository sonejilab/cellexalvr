using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.Extensions;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu.Buttons;
using CellexalVR.Menu.Buttons.Attributes;
using CellexalVR.Menu.Buttons.Facs;
using CellexalVR.Menu.Buttons.Selection;
using CellexalVR.SceneObjects;
using CellexalVR.Spatial;
using CellexalVR.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Entities;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Multiuser
{
    /// <summary>
    /// This class holds the remote-callable commands that are sent over network between to connected clients.
    /// To synchronize the scenes in multiplayer it means when a function is called on one client the same has to be done on the others. 
    /// Each function in this class represent one such function to synchronize the scenes.
    /// </summary>
    public class MultiuserMessageReceiver : Photon.MonoBehaviour
    {
        private MultiuserMessageSender multiuserMessageSender;
        public ReferenceManager referenceManager;

        private Dictionary<Collider, bool> colliders = new Dictionary<Collider, bool>();

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            if (referenceManager == null)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }

            multiuserMessageSender = referenceManager.multiuserMessageSender;
        }

        #region RPCs

        // these methods are basically messages that are sent over the network from on client to another.

        #region Loading

        [PunRPC]
        public void ReceiveMessageReadFolder(string path)
        {
            CellexalLog.Log("Received message to read folder at " + path);

            referenceManager.inputReader.ReadFolder(path);
            referenceManager.inputFolderGenerator.DestroyFolders();
        }

        [PunRPC]
        public void ReceiveMessageH5Config(string path, Dictionary<string, string> h5config)
        {
            CellexalLog.Log(
                "Received message to read folder at " + path + " with h5 config with size " + h5config.Count);

            referenceManager.inputReader.ReadFolder(path, h5config);
        }

        [PunRPC]
        public void ReceiveMessageSynchConfig(byte[] data)
        {
            CellexalLog.Log("Received message to synch config");
            referenceManager.configManager.SynchroniseConfig(data);
        }

        [PunRPC]
        public void ReceiveMessageLoadingMenu(bool delete)
        {
            CellexalLog.Log("Received message to reset to loading dataset scene");
            referenceManager.loaderController.ResetFolders(delete);
        }

        #endregion

        #region Interaction

        [PunRPC]
        public void ReceiveMessageDisableColliders(string n)
        {
            GameObject obj = GameObject.Find(n);
            if (obj == null)
                return;
            var rightController = ReferenceManager.instance.rightController.GetComponent<XRDirectInteractor>();
            var leftController = ReferenceManager.instance.leftController.GetComponent<XRDirectInteractor>();
            int layerMask = LayerMask.GetMask("GraphLayer");
            Collider[] overlapR = Physics.OverlapBox(rightController.transform.position, rightController.GetComponent<BoxCollider>().size / 2f,
                rightController.transform.rotation, layerMask, QueryTriggerInteraction.Collide);
            Collider[] overlapL = Physics.OverlapBox(leftController.transform.position, leftController.GetComponent<BoxCollider>().size / 2f,
                leftController.transform.rotation, layerMask, QueryTriggerInteraction.Collide);
            foreach (Collider col in overlapR)
            {
                if (col.gameObject == obj)
                {
                    rightController.SendMessage("OnTriggerExit", col);
                }
            }

            foreach (Collider col in overlapL)
            {
                if (col.gameObject == obj)
                {
                    leftController.SendMessage("OnTriggerExit", col);
                }
            }
            obj.TryGetComponent(out OffsetGrab interactable);
            if (interactable)
                interactable.enabled = false;
        }

        [PunRPC]
        public void ReceiveMessageEnableColliders(string n)
        {
            GameObject obj = GameObject.Find(n);
            if (obj == null)
                return;
            var rightController = ReferenceManager.instance.rightController.GetComponent<XRDirectInteractor>();
            var leftController = ReferenceManager.instance.leftController.GetComponent<XRDirectInteractor>();
            int layerMask = LayerMask.GetMask("GraphLayer");
            Collider[] overlapR = Physics.OverlapBox(rightController.transform.position, rightController.GetComponent<BoxCollider>().size / 2f,
                rightController.transform.rotation, layerMask, QueryTriggerInteraction.Collide);
            Collider[] overlapL = Physics.OverlapBox(leftController.transform.position, leftController.GetComponent<BoxCollider>().size / 2f,
                leftController.transform.rotation, layerMask, QueryTriggerInteraction.Collide);
            foreach (Collider col in overlapR)
            {
                if (col.gameObject == obj)
                {
                    rightController.SendMessage("OnTriggerEnter", col);
                }
            }

            foreach (Collider col in overlapL)
            {
                if (col.gameObject == obj)
                {
                    leftController.SendMessage("OnTriggerEnter", col);
                }
            }
            obj.TryGetComponent(out OffsetGrab interactable);
            if (interactable)
                interactable.enabled = true;
        }

        [PunRPC]
        public void ReceiveMessageToggleLaser(bool active, int ownerId, string ownerName)
        {
            if (ownerId != photonView.ownerId) return;
            MultiuserLaserManager mlm =
                referenceManager.multiuserMessageSender.GetComponentInChildren<MultiuserLaserManager>();
            bool exists = mlm.lasersLineRends.TryGetValue(ownerId, out LineRenderer lr);
            if (!exists)
            {
                lr = mlm.AddLaser(ownerId, ownerName);
                if (lr == null) return;
            }

            // lr.startColor = lr.endColor = referenceManager.rightLaser.validCollisionColor;
            // lr.material.color = lr.startColor = lr.endColor = referenceManager.rightLaser.validCollisionColor;
            lr.gameObject.SetActive(active);
        }

        [PunRPC]
        public void ReceiveMessageMoveLaser(float originX, float originY, float originZ,
            float hitX, float hitY, float hitZ, int ownerId, string ownerName)
        {
            if (ownerId != photonView.ownerId) return;
            MultiuserLaserManager mlm =
                referenceManager.multiuserMessageSender.GetComponentInChildren<MultiuserLaserManager>();
            // LineRenderer lr = mlm.GetLaser(ownerId);
            bool exists = mlm.lasersLineRends.TryGetValue(ownerId, out LineRenderer lr);
            if (!exists)
            {
                lr = mlm.AddLaser(ownerId, ownerName);
                if (lr == null) return;
                // lr.material.color = lr.startColor = lr.endColor = referenceManager.rightLaser.validCollisionColor;
            }

            lr.SetPosition(0, mlm.laserTransforms[ownerId].position);
            lr.SetPosition(1, new Vector3(hitX, hitY, hitZ));
            if (!lr.gameObject.activeSelf) lr.gameObject.SetActive(true);
        }

        [PunRPC]
        public void ReceiveMessageUpdateSliderValue(string sliderType, float value)
        {
            SliderController.sliderType slider = (SliderController.sliderType)Enum.Parse(typeof(SliderController.sliderType), sliderType);

            switch (slider)
            {
                case SliderController.sliderType.VELOCITY:
                    SliderController veloSlider = referenceManager.velocitySubMenu.particleSizeSlider;
                    veloSlider.UpdateSliderValue(value);
                    referenceManager.velocityGenerator.ChangeParticleSize(veloSlider.currentValue);
                    break;
                case SliderController.sliderType.PARTICLESIZE:
                case SliderController.sliderType.PARTICLESPREAD:
                case SliderController.sliderType.PARTICLEALPHA:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ReceiveMessageShowPDFPages()
        {
            referenceManager.pdfMesh.ShowMultiplePages();
        }


        #endregion

        #region Legend

        [PunRPC]
        public void ReceiveMessageToggleLegend()
        {
            if (referenceManager.legendManager.legendActive)
            {
                referenceManager.legendManager.DeactivateLegend();
            }
            else
            {
                referenceManager.legendManager.ActivateLegend();
            }
        }

        [PunRPC]
        public void ReceiveMessageMoveLegend(float posX, float posY, float posZ,
            float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
        {
            GameObject legend = referenceManager.legendManager.gameObject;
            if (legend == null || !legend.activeSelf) return;
            legend.transform.position = new Vector3(posX, posY, posZ);
            legend.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
            legend.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }

        [PunRPC]
        public void ReceiveMessageLegendUngrabbed(
            float posX, float posY, float posZ,
            float rotX, float rotY, float rotZ, float rotW,
            float velX, float velY, float velZ,
            float angVelX, float angVelY, float angVelZ)
        {
            LegendManager legend = referenceManager.legendManager;
            if (legend)
            {
                Rigidbody r = legend.GetComponent<Rigidbody>();
                legend.transform.position = new Vector3(posX, posY, posZ);
                legend.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                r.velocity = new Vector3(velX, velY, velZ);
                r.angularVelocity = new Vector3(angVelX, angVelY, angVelZ);
            }
        }

        [PunRPC]
        public void ReceiveMessageChangeLegend(string legendName)
        {
            switch (legendName)
            {
                case "GeneExpressionLegend":
                    referenceManager.legendManager.ActivateLegend(LegendManager.Legend.GeneExpressionLegend);
                    break;
                case "SelectionLegend":
                    referenceManager.legendManager.ActivateLegend(LegendManager.Legend.SelectionLegend);
                    break;
                case "AttributeLegend":
                    referenceManager.legendManager.ActivateLegend(LegendManager.Legend.AttributeLegend);
                    break;
            }
        }

        [PunRPC]
        public void ReceiveMessageAttributeLegendChangePage(bool dir)
        {
            referenceManager.legendManager.attributeLegend.ChangePage(dir);
        }

        [PunRPC]
        public void ReceiveMessageSelectionLegendChangePage(bool dir)
        {
            referenceManager.legendManager.selectionLegend.ChangePage(dir);
        }

        [PunRPC]
        public void ReceiveMessageChangeTab(int index)
        {
            referenceManager.legendManager.geneExpressionHistogram.SwitchToTab(index);
        }

        [PunRPC]
        public void ReceiveMessageDeactivateSelectedArea()
        {
            referenceManager.legendManager.geneExpressionHistogram.DeactivateSelectedArea();
        }

        [PunRPC]
        public void ReceiveMessageMoveSelectedArea(int hitIndex, int savedGeneExpressionHistogramHit)
        {
            referenceManager.legendManager.geneExpressionHistogram.MoveSelectedArea(hitIndex,
                savedGeneExpressionHistogramHit);
            referenceManager.legendManager.GetComponent<LegendRaycaster>().savedGeneExpressionHistogramHitX = -1;
        }

        [PunRPC]
        public void ReceiveMessageMoveHighlightArea(int minX, int maxX)
        {
            referenceManager.legendManager.geneExpressionHistogram.MoveHighlightArea(minX, maxX);
        }

        [PunRPC]
        public void ReceiveMessageSwitchMode(string mode)
        {
            GeneExpressionHistogram histogram = referenceManager.legendManager.geneExpressionHistogram;
            switch (mode)
            {
                case "Linear":
                    histogram.DesiredYAxisMode = GeneExpressionHistogram.YAxisMode.Linear;
                    break;
                case "Logarithmic":
                    histogram.DesiredYAxisMode = GeneExpressionHistogram.YAxisMode.Logarithmic;
                    break;
            }

            histogram.RecreateHistogram();
        }

        [PunRPC]
        public void ReceiveMessageChangeThreshold(int increment)
        {
            GeneExpressionHistogram histogram = referenceManager.legendManager.geneExpressionHistogram;
            histogram.TallestBarsToSkip += increment;
            histogram.RecreateHistogram();
        }

        #endregion

        #region Coloring

        [PunRPC]
        public void ReceiveMessageColorGraphsByGene(string geneName)
        {
            CellexalLog.Log("Received message to color all graphs by " + geneName);
            referenceManager.cellManager
                .ColorGraphsByGene(geneName); //, referenceManager.graphManager.GeneExpressionColoringMethod);
            referenceManager.geneKeyboard.SubmitOutput(false);
            referenceManager.autoCompleteList.ClearList();
        }

        [PunRPC]
        public void ReceiveMessageColoringMethodChanged(int newMode)
        {
            CellexalLog.Log("Received message to change coloring mode to " + newMode);
            referenceManager.coloringOptionsList.SwitchMode((GraphManager.GeneExpressionColoringMethods)newMode);
        }

        //[PunRPC]
        //public void ReceiveMessageColorGraphsByPreviousExpression(string geneName)
        //{
        //    CellexalLog.Log("Received message to color all graphs by " + geneName);
        //    referenceManager.cellManager.ColorGraphsByPreviousExpression(geneName);
        //}

        [PunRPC]
        public void ReceiveMessageColorByAttribute(string attributeType, bool toggle, int colIndex)
        {
            CellexalLog.Log("Received message to " + (toggle ? "toggle" : "untoggle") + " all graphs by attribute " +
                            attributeType);
            referenceManager.cellManager.ColorByAttribute(attributeType, toggle, colIndex: colIndex);
            var attributeButton = referenceManager.attributeSubMenu.FindButton(attributeType);
            attributeButton.ToggleOutline(toggle);
            attributeButton.GetComponent<ColorByAttributeButton>().colored = toggle;
        }

        [PunRPC]
        public void ReceiveMessageColorByAttributePointCloud(string attributeType, bool toggle)
        {
            CellexalLog.Log("Received message to " + (toggle ? "toggle" : "untoggle") + " all point clouds by attribute " +
                            attributeType);
            TextureHandler.instance.ColorCluster(attributeType, toggle);
            var attributeButton = referenceManager.attributeSubMenu.FindButton(attributeType);
            attributeButton.ToggleOutline(toggle);
            attributeButton.GetComponent<ColorByAttributeButton>().colored = toggle;
        }

        [PunRPC]
        public void ReceiveMessageToggleAllAttributesPointCloud(bool toggle)
        {
            CellexalLog.Log("Received message to " + (toggle ? "toggle" : "untoggle") + " all clusters in point clouds");
            TextureHandler.instance.ColorAllClusters(toggle);
            foreach (ColorByAttributeButton attributeButton in ReferenceManager.instance.attributeSubMenu.GetComponentsInChildren<ColorByAttributeButton>())
            {
                attributeButton.ToggleOutline(toggle);
                attributeButton.GetComponent<ColorByAttributeButton>().colored = toggle;
            }
        }

        [PunRPC]
        public void ReceiveMessageColorByIndex(string indexName)
        {
            CellexalLog.Log("Received message to color all graphs by index " + indexName);
            referenceManager.cellManager.ColorByIndex(indexName);
            referenceManager.indexMenu.FindButton(indexName).GetComponent<ColorByIndexButton>().TurnOff();
        }

        [PunRPC]
        public void ReceiveMessageToggleTransparency(bool toggle)
        {
            CellexalLog.Log("Received message to toggle transparency on gps" + toggle);
            referenceManager.graphManager.ToggleGraphPointTransparency(toggle);
            referenceManager.mainMenu.GetComponentInChildren<ToggleTransparencyButton>().Toggle = !toggle;
        }

        [PunRPC]
        public void ReceiveMessageAddCullingCube()
        {
            CellexalLog.Log("Received message to add culling cube");
            referenceManager.cullingFilterManager.AddCube();
        }

        [PunRPC]
        public void ReceiveMessageRemoveCullingCube()
        {
            CellexalLog.Log("Received message to remove culling cube");
            referenceManager.cullingFilterManager.RemoveCube();
        }

        [PunRPC]
        public void ReceiveMessageGenerateRandomColors(int n)
        {
            CellexalLog.Log(message: "Received message to generate " + n + " random colors");
            referenceManager.settingsMenu.GetComponent<DesktopUI.ColorMapManager>().DoGenerateRandomColors(n);
        }

        [PunRPC]
        public void ReceiveMessageGenerateRainbowColors(int n)
        {
            CellexalLog.Log(message: "Received message to generate " + n + " rainbow colors");
            referenceManager.settingsMenu.GetComponent<DesktopUI.ColorMapManager>().DoGenerateRainbowColors(n);
        }

        [PunRPC]
        public void ReceiveMessageHighlightCells(int group, bool highlight)
        {
            Cell[] cellsToHighlight;
            if (group != -1)
            {
                cellsToHighlight = referenceManager.cellManager.GetCells(group);
            }
            else
            {
                cellsToHighlight = referenceManager.cellManager.GetCells();
            }

            referenceManager.cellManager.HighlightCells(cellsToHighlight, highlight);
        }

        #endregion

        #region Keyboard

        [PunRPC]
        public void ReceiveMessageActivateKeyboard(bool activate)
        {
            referenceManager.keyboardSwitch.SetKeyboardVisible(activate);
        }

        [PunRPC]
        public void ReceiveMessageKeyClicked(string key)
        {
            referenceManager.geneKeyboard.AddText(key, false);
        }

        [PunRPC]
        public void ReceiveMessageKBackspaceKeyClicked()
        {
            referenceManager.geneKeyboard.BackSpace(false);
        }

        [PunRPC]
        public void ReceiveMessageClearKeyClicked()
        {
            referenceManager.geneKeyboard.Clear(false);
        }

        [PunRPC]
        public void ReceiveMessageSearchLockToggled(int index)
        {
            referenceManager.previousSearchesList.searchLocks[index].Click();
        }

        [PunRPC]
        public void ReceiveMessageAddAnnotation(string annotation, int index, string gpLabel)
        {
            CellexalLog.Log("Received message to add annotation: " + annotation);
            referenceManager.annotationManager.AddAnnotation(annotation, index, gpLabel);
        }

        [PunRPC]
        public void ReceiveMessageExportAnnotations()
        {
            CellexalLog.Log("Received message to export annotations");
            referenceManager.annotationManager.DumpAnnotatedSelectionToTextFile();
        }

        [PunRPC]
        public void ReceiveMessageClearExpressionColours()
        {
            CellexalLog.Log("Received message to clear expression colours on the graphs");
            referenceManager.graphManager.ClearExpressionColours();
        }

        [PunRPC]
        public void ReceiveMessageCalculateCorrelatedGenes(string geneName)
        {
            CellexalLog.Log("Received message to calculate genes correlated to " + geneName);
            referenceManager.correlatedGenesList.CalculateCorrelatedGenes(geneName, Definitions.Measurement.GENE);
        }

        [PunRPC]
        public void ReceiveMessageRecolorSelectionPoints()
        {
            CellexalLog.Log("Received message to recolor graph points by current selection");
            referenceManager.selectionManager.RecolorSelectionPoints();
        }

        [PunRPC]
        public void ReceiveMessageHandleHistoryPanelClick(string panelName)
        {
            ClickableHistoryPanel panel = referenceManager.sessionHistoryList.GetPanel(panelName);
            if (!panel)
            {
                CellexalLog.Log($"Could not find history panel with name: {panelName}");
                return;
            }
            panel.HandleClick();
        }

        #endregion

        #region Selection

        [PunRPC]
        public void ReceiveMessageConfirmSelection()
        {
            CellexalLog.Log("Received message to confirm selection");
            referenceManager.selectionManager.ConfirmSelection();
        }

        [PunRPC]
        public void ReceiveMessageAddSelect(string graphName, string label, int newGroup, float r, float g, float b)
        {
            referenceManager.selectionManager.DoClientSelectAdd(graphName, label, newGroup, new Color(r, g, b));
        }

        [PunRPC]
        public void ReceiveMessageAddSelectMany(string graphName, string[] labels, int newGroup, float r, float g, float b)
        {
            foreach (string label in labels)
            {
                referenceManager.selectionManager.DoClientSelectAdd(graphName, label, newGroup, new Color(r, g, b));
            }
        }

        [PunRPC]
        public void ReceiveMessageAddSelect(int[] indices, int[] groups)
        {
            List<Vector2Int> tupleList = new List<Vector2Int>();
            for (int i = 0; i < indices.Length; i++)
            {
                tupleList.Add(new Vector2Int(indices[i], groups[i]));
            }
            TextureHandler.instance.AddPointsToSelection(tupleList);
        }

        [PunRPC]
        public void ReceiveMessageSelectedRemoveMany(string graphName, string[] labels)
        {
            foreach (string label in labels)
            {
                referenceManager.selectionManager.RemoveGraphpointFromSelection(graphName, label);
            }
        }

        [PunRPC]
        public void ReceiveMessageCubeColoured(string graphName, string label, int newGroup, float r, float g, float b)
        {
            referenceManager.selectionManager.DoClientSelectAdd(graphName, label, newGroup, new Color(r, g, b), true);
        }

        [PunRPC]
        public void ReceiveMessageGoBackOneColor()
        {
            referenceManager.selectionManager.GoBackOneColorInHistory();
        }

        [PunRPC]
        public void ReceiveMessageGoBackSteps(int k)
        {
            for (int i = 0; i < k; i++)
            {
                referenceManager.selectionManager.GoBackOneStepInHistory();
            }
        }

        [PunRPC]
        public void ReceiveMessageCancelSelection()
        {
            referenceManager.selectionManager.CancelSelection();
        }

        [PunRPC]
        public void ReceiveMessageRedoOneColor()
        {
            referenceManager.selectionManager.GoForwardOneColorInHistory();
        }

        [PunRPC]
        public void ReceiveMessageRedoSteps(int k)
        {
            for (int i = 0; i < k; i++)
            {
                referenceManager.selectionManager.GoForwardOneStepInHistory();
            }
        }

        [PunRPC]
        public void ReceiveMessageToggleAnnotationFile(string path, bool toggle)
        {
            referenceManager.annotationManager.ToggleAnnotationFile(path, toggle);
            SelectAnnotationButton button = referenceManager.selectionFromPreviousMenu.FindAnnotationButton(path);
            button.ToggleOutline(toggle);
            button.toggle = !toggle;
        }


        [PunRPC]
        public void ReceiveMessageToggleSelectionFile(string path)
        {
            referenceManager.inputReader.ReadSelectionFile(path);
            SelectionFromPreviousButton button = referenceManager.selectionFromPreviousMenu.FindSelectionButton(path);
            button.ToggleOutline(true);
            button.toggle = true;
        }

        #endregion

        #region Draw tool

        [PunRPC]
        public void ReceiveMessageDrawLine(float r, float g, float b, float[] xcoords, float[] ycoords, float[] zcoords)
        {
            CellexalLog.Log("Received message to draw line with " + xcoords.Length + " segments");
            Vector3[] coords = new Vector3[xcoords.Length];
            for (int i = 0; i < xcoords.Length; i++)
            {
                coords[i] = new Vector3(xcoords[i], ycoords[i], zcoords[i]);
            }

            Color col = new Color(r, g, b);
            referenceManager.drawTool.DrawNewLine(col, coords);
        }

        [PunRPC]
        public void ReceiveMessageClearAllLines()
        {
            CellexalLog.Log("Received message to clear line segments");
            referenceManager.drawTool.SkipNextDraw();
            referenceManager.drawTool.ClearAllLines();
        }

        [PunRPC]
        public void ReceiveMessageClearLastLine()
        {
            CellexalLog.Log("Received message to clear previous line");
            referenceManager.drawTool.SkipNextDraw();
            referenceManager.drawTool.ClearLastLine();
        }

        [PunRPC]
        public void ReceiveMessageClearLinesWithColor(float r, float g, float b)
        {
            CellexalLog.Log("Received message to clear previous line");
            referenceManager.drawTool.SkipNextDraw();
            referenceManager.drawTool.ClearAllLinesWithColor(new Color(r, g, b));
        }

        #endregion

        #region Graphs

        [PunRPC]
        public void ReceiveMessageMoveGraph(string moveGraphName, float posX, float posY, float posZ, float rotX,
            float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
        {
            Graph g = referenceManager.graphManager.FindGraph(moveGraphName);
            SpatialGraph sg = referenceManager.graphManager.FindSpatialGraph(moveGraphName);
            if (g != null)
            {
                try
                {
                    g.transform.position = new Vector3(posX, posY, posZ);
                    g.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                    g.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not move graph - Error: " + e);
                }
            }
            else if (sg != null)
            {
                try
                {
                    sg.transform.position = new Vector3(posX, posY, posZ);
                    sg.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                    sg.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not move graph - Error: " + e);
                }
            }
            else
            {
                PointCloud pc = PointCloudGenerator.instance.FindPointCloud(moveGraphName);
                try
                {
                    pc.transform.position = new Vector3(posX, posY, posZ);
                    pc.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                    pc.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not move point cloud - Error: " + e);
                }

            }
        }
        [PunRPC]
        public void ReceiveMessageGraphUngrabbed(string graphName,
            float posX, float posY, float posZ,
            float rotX, float rotY, float rotZ, float rotW,
            float velX, float velY, float velZ,
            float angVelX, float angVelY, float angVelZ)
        {
            Graph g = referenceManager.graphManager.FindGraph(graphName);
            if (g)
            {
                Rigidbody r = g.GetComponent<Rigidbody>();
                g.transform.position = new Vector3(posX, posY, posZ);
                g.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                r.velocity = new Vector3(velX, velY, velZ);
                r.angularVelocity = new Vector3(angVelX, angVelY, angVelZ);
            }
        }

        [PunRPC]
        public void ReceiveMessageResetGraph()
        {
            CellexalLog.Log("Received message to reset graph colors");
            referenceManager.graphManager.ResetGraphsColor();
        }

        [PunRPC]
        public void ReceiveMessageResetGraphPosition()
        {
            CellexalLog.Log("Received message to reset graph position, scale and rotation");
            referenceManager.graphManager.ResetGraphsPosition();
        }

        [PunRPC]
        public void ReceiveMessageDrawLinesBetweenGps(bool toggle)
        {
            CellexalLog.Log("Received message to draw lines between graph points");
            StartCoroutine(
                referenceManager.lineBundler.DrawLinesBetweenGraphPoints(referenceManager.selectionManager
                    .GetLastSelection()));
            //CellexalEvents.LinesBetweenGraphsDrawn.Invoke();
        }

        [PunRPC]
        public void ReceiveMessageClearLinesBetweenGps()
        {
            CellexalLog.Log("Received message to clear lines between graph points");
            referenceManager.lineBundler.ClearLinesBetweenGraphPoints();
            //CellexalEvents.LinesBetweenGraphsCleared.Invoke();
        }

        [PunRPC]
        public void ReceiveMessageBundleAllLines()
        {
            CellexalLog.Log("Received message to clear lines between graph points");
            referenceManager.lineBundler.BundleAllLines();
        }


        [PunRPC]
        public void ReceiveMessageAddMarker(string indexName)
        {
            //var markerButton = GameObject.Find("/Main Menu/Attribute Menu/TabPrefab(Clone)/" + indexName);
            var markerButton = referenceManager.createFromMarkerMenu.FindButton(indexName);
            if (referenceManager.newGraphFromMarkers.markers.Count < 3 &&
                !referenceManager.newGraphFromMarkers.markers.Contains(indexName))
            {
                referenceManager.newGraphFromMarkers.markers.Add(indexName);
                if (markerButton)
                {
                    markerButton.ToggleOutline(true);
                    //markerButton.GetComponent<AddMarkerButton>().activeOutline.SetActive(true);
                    //markerButton.GetComponent<AddMarkerButton>().activeOutline.GetComponent<MeshRenderer>().enabled = true;
                }
            }
            else if (referenceManager.newGraphFromMarkers.markers.Contains(indexName))
            {
                referenceManager.newGraphFromMarkers.markers.Remove(indexName);
                if (markerButton)
                {
                    markerButton.ToggleOutline(false);
                    //markerButton.GetComponent<AddMarkerButton>().activeOutline.SetActive(false);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageCreateMarkerGraph()
        {
            CellexalLog.Log("Received message to create marker graph");
            referenceManager.newGraphFromMarkers.CreateMarkerGraph();
        }

        [PunRPC]
        public void ReceiveMessageCreateAttributeGraph()
        {
            CellexalLog.Log("Received message to create attribute graph");
            referenceManager.graphGenerator.CreateSubGraphs(referenceManager.attributeSubMenu.attributes);
        }

        [PunRPC]
        public void ReceiveMessageActivateSlices()
        {
            CellexalLog.Log("Received message to activate slices in spatial graph");
            foreach (SpatialGraph spatialGraph in referenceManager.graphManager.spatialGraphs)
            {
                spatialGraph.ActivateSlices();
            }
        }
        //[PunRPC]
        //public void ReceiveMessageSpatialGraphGrabbed(string sliceName, string graphName)
        //{
        //    foreach (SpatialGraph spatialGraph in referenceManager.graphManager.spatialGraphs)
        //    {
        //        if (spatialGraph.gameObject.name.Equals(graphName))
        //            spatialGraph.GetSlice(sliceName).ToggleGrabbing(true);
        //    }
        //}

        //[PunRPC]
        //public void ReceiveMessageSpatialGraphUnGrabbed(string sliceName, string graphName)
        //{
        //    CellexalLog.Log("Received message to activate slices in spatial graph");
        //    foreach (SpatialGraph spatialGraph in referenceManager.graphManager.spatialGraphs)
        //    {
        //        if (spatialGraph.gameObject.name.Equals(graphName))
        //            spatialGraph.GetSlice(sliceName).ToggleGrabbing(false);
        //    }
        //}

        [PunRPC]
        public void ReceiveMessageHighlightCluster(bool highlight, string graphName, int id)
        {
            Graph g = referenceManager.graphManager.FindGraph(graphName);
            if (g == null) return;
            PointCluster cluster = g.GetComponent<GraphBetweenGraphs>().GetCluster(id);
            if (cluster == null) return;
            cluster.Highlight(highlight);
        }

        [PunRPC]
        public void ReceiveMessageToggleBundle(string graphName, int id)
        {
            Graph g = referenceManager.graphManager.FindGraph(graphName);
            if (g == null) return;
            PointCluster cluster = g.GetComponent<GraphBetweenGraphs>().GetCluster(id);
            if (cluster == null) return;
            cluster.RemakeLines(cluster.fromPointCluster);
        }

        [PunRPC]
        public void ReceiveMessageToggleAxes()
        {
            referenceManager.graphManager.ToggleAxes();
        }

        [PunRPC]
        public void ReceiveMessageToggleInfoPanels()
        {
            referenceManager.graphManager.ToggleInfoPanels();
        }

        [PunRPC]
        public void ReceiveMessageSpreadPoints(int pcID, bool spread)
        {
            PointCloud pc = PointCloudGenerator.instance.pointClouds[pcID];
            pc.SpreadOutPoints(spread);
        }

        #endregion

        #region Heatmaps

        [PunRPC]
        public void ReceiveMessageMoveHeatmap(string heatmapName, float posX, float posY, float posZ, float rotX,
            float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                try
                {
                    hm.transform.position = new Vector3(posX, posY, posZ);
                    hm.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                    hm.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not move heatmap - Error: " + e);
                }
            }

            //else
            //{
            //    CellexalLog.Log("Could not find heatmap to move");
            //}
        }

        [PunRPC]
        public void ReceiveMessageCreateHeatmap(string hmName)
        {
            CellexalLog.Log("Received message to create heatmap");
            referenceManager.heatmapGenerator.CreateHeatmap(hmName);
        }

        [PunRPC]
        public void ReceiveMessageHandleBoxSelection(string heatmapName, int hitx, int hity, int selectionStartX,
            int selectionStartY)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                try
                {
                    hm.GetComponent<HeatmapRaycast>().HandleBoxSelection(hitx, hity, selectionStartX, selectionStartY);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not move heatmap - Error: " + e);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageConfirmSelection(string heatmapName, int hitx, int hity, int selectionStartX,
            int selectionStartY)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                try
                {
                    hm.GetComponent<HeatmapRaycast>().ConfirmSelection(hitx, hity, selectionStartX, selectionStartY);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not confirm selection - Error: " + e);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageHandleMovingSelection(string heatmapName, int hitx, int hity)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                try
                {
                    hm.GetComponent<HeatmapRaycast>().HandleMovingSelection(hitx, hity);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not move heatmap selection - Error: " + e);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageMoveSelection(string heatmapName, int hitx, int hity, int selectedGroupLeft,
            int selectedGroupRight, int selectedGeneTop, int selectedGeneBottom)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (!heatmapExists) return;
            try
            {
                hm.GetComponent<HeatmapRaycast>().MoveSelection(hitx, hity, selectedGroupLeft, selectedGroupRight,
                    selectedGeneTop, selectedGeneBottom);
            }
            catch (Exception)
            {
                // CellexalLog.Log("Could not move heatmap - Error: " + e);
            }
        }

        [PunRPC]
        public void ReceiveMessageHandleHitHeatmap(string heatmapName, int hitx, int hity)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                try
                {
                    hm.GetComponent<HeatmapRaycast>().HandleHitHeatmap(hitx, hity);
                }
                catch (Exception)
                {
                    //CellexalLog.Log("Failed to handle hit on heatmap. Stacktrace : " + e.StackTrace);
                }
            }
        }


        [PunRPC]
        public void ReceiveMessageResetSelecting(string heatmapName)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                try
                {
                    hm.GetComponent<HeatmapRaycast>().ResetSelecting();
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Failed to reset heatmap selecting. Stacktrace : " + e.StackTrace);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageHandlePressDown(string heatmapName, int hitx, int hity)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                try
                {
                    hm.GetComponent<HeatmapRaycast>().HandlePressDown(hitx, hity);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Failed to handle heatmap press down. Stacktrace : " + e.StackTrace);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageCreateNewHeatmapFromSelection(string heatmapName, int selectedGroupLeft,
            int selectedGroupRight, int selectedGeneTop,
            int selectedGeneBottom, float selectedBoxWidth, float selectedBoxHeight)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                try
                {
                    hm.CreateNewHeatmapFromSelection(selectedGroupLeft, selectedGroupRight,
                        selectedGeneTop, selectedGeneBottom, selectedBoxWidth, selectedBoxHeight);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Failed to create new heatmap from selection. Stacktrace : " + e.StackTrace);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageReorderByAttribute(string heatmapName, bool shouldReorder)
        {
            CellexalLog.Log("Received message to " + (shouldReorder ? "reorder" : "restore") + " heatmap");
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                if (shouldReorder)
                {
                    hm.ReorderByAttribute();
                }
                else
                {
                    referenceManager.heatmapGenerator.BuildTexture(hm.selection, "", hm);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageHandleHitGenesList(string heatmapName, int hity)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                hm.GetComponent<HeatmapRaycast>().HandleHitGeneList(hity);
            }
            else
            {
                return;
            }
        }

        [PunRPC]
        public void ReceiveMessageHandleHitGroupingBar(string heatmapName, int hitx)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                hm.GetComponent<HeatmapRaycast>().HandleHitGroupingBar(hitx);
            }
            else
            {
                return;
            }
        }

        [PunRPC]
        public void ReceiveMessageHandleHitAttributeBar(string heatmapName, int hitx)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                hm.GetComponent<HeatmapRaycast>().HandleHitAttributeBar(hitx);
            }
            else
            {
                return;
            }
        }

        [PunRPC]
        public void ReceiveMessageResetInfoTexts(string heatmapName)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                hm.barInfoText.text = "";
                hm.enlargedGeneText.gameObject.SetActive(false);
            }
            else
            {
                return;
            }
        }

        [PunRPC]
        public void ReceiveMessageResetHeatmapHighlight(string heatmapName)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            bool heatmapExists = hm != null;
            if (heatmapExists)
            {
                try
                {
                    referenceManager.heatmapGenerator.FindHeatmap(heatmapName).ResetHeatmapHighlight();
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Failed to reset heatmap highlight. Stacktrace : " + e.StackTrace);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageCumulativeRecolorFromSelection(string heatmapName, int groupLeft, int groupRight, int selectedTop, int selectedBottom)
        {
            Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            if (hm != null)
            {
                hm.CumulativeRecolorFromSelection(groupLeft, groupRight, selectedTop, selectedBottom);
            }
        }

        #endregion

        #region Networks

        [PunRPC]
        public void ReceiveMessageGenerateNetworks(int layoutSeed)
        {
            CellexalLog.Log("Received message to generate networks");
            referenceManager.networkGenerator.GenerateNetworks(layoutSeed);
        }

        [PunRPC]
        public void ReceiveMessageMoveNetwork(string networkName, float posX, float posY, float posZ, float rotX,
            float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
        {
            NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkName);
            bool networkExists = nh != null;
            if (networkExists)
            {
                try
                {
                    nh.transform.position = new Vector3(posX, posY, posZ);
                    nh.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                    nh.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not move network - Error: " + e);
                }
            }

            //else
            //{
            //    CellexalLog.Log("Could not find network to move");
            //}
        }

        [PunRPC]
        public void ReceiveMessageNetworkUngrabbed(string networkName,
            float posX, float posY, float posZ,
            float rotX, float rotY, float rotZ, float rotW,
            float velX, float velY, float velZ,
            float angVelX, float angVelY, float angVelZ)
        {
            NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkName);
            if (nh)
            {
                nh.transform.position = new Vector3(posX, posY, posZ);
                nh.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                Rigidbody r = nh.GetComponent<Rigidbody>();
                r.velocity = new Vector3(velX, velY, velZ);
                r.angularVelocity = new Vector3(angVelX, angVelY, angVelZ);
            }
        }

        [PunRPC]
        public void ReceiveMessageEnlargeNetwork(string networkHandlerName, string networkCenterName)
        {
            CellexalLog.Log("Received message to enlarge network " + networkCenterName + " in handler " +
                            networkHandlerName);
            var handler = referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName);
            bool handlerExists = handler != null;
            if (!handlerExists)
            {
                return;
            }

            var center = handler.FindNetworkCenter(networkCenterName);
            bool networkExists = (handlerExists && center != null);
            if (networkExists)
            {
                center.EnlargeNetwork();
            }
            else
            {
                CellexalLog.Log("Could not find networkcenter " + networkCenterName);
            }
        }

        [PunRPC]
        public void ReceiveMessageBringBackNetwork(string networkHandlerName, string networkCenterName)
        {
            CellexalLog.Log("Received message to bring back network " + networkCenterName + " in handler " +
                            networkHandlerName);
            var handler = referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName);
            bool handlerExists = handler != null;
            if (!handlerExists)
            {
                return;
            }

            var center = handler.FindNetworkCenter(networkCenterName);
            bool networkExists = (handlerExists && center != null);
            if (networkExists)
            {
                center.BringBackOriginal();
            }
            else
            {
                CellexalLog.Log("Could not find networkcenter " + networkCenterName);
            }
        }

        [PunRPC]
        public void ReceiveMessageSwitchNetworkLayout(int layout, string networkHandlerName, string networkCenterName)
        {
            CellexalLog.Log("Received message to generate networks");
            print("network names:" + networkCenterName + " " + networkHandlerName);
            var handler = referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName);
            bool handlerExists = handler != null;
            if (!handlerExists)
            {
                return;
            }

            var center = handler.FindNetworkCenter(networkCenterName);
            bool networkExists = (handlerExists && center != null);
            if (networkExists)
            {
                center.SwitchLayout((NetworkCenter.Layout)layout);
            }
            else
            {
                CellexalLog.Log("Could not find networkcenter " + networkCenterName);
            }
        }

        [PunRPC]
        public void ReceiveMessageMoveNetworkCenter(string networkHandlerName, string networkCenterName, float posX,
            float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY,
            float scaleZ)
        {
            var handler = referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName);
            bool handlerExists = handler != null;
            if (!handlerExists)
            {
                return;
            }

            var center = handler.FindNetworkCenter(networkCenterName);
            bool networkExists = (handlerExists && center != null);
            if (networkExists)
            {
                center.transform.position = new Vector3(posX, posY, posZ);
                center.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                center.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            }
            else
            {
                CellexalLog.Log("Could not find networkcenter to move");
            }
        }

        [PunRPC]
        public void ReceiveMessageNetworkCenterUngrabbed(string networkHandlerName, string networkCenterName,
            float posX, float posY, float posZ,
            float rotX, float rotY, float rotZ, float rotW,
            float velX, float velY, float velZ,
            float angVelX, float angVelY, float angVelZ)
        {
            NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName);
            if (nh)
            {
                NetworkCenter nc = nh.FindNetworkCenter(networkCenterName);
                if (nc)
                {
                    nc.transform.position = new Vector3(posX, posY, posZ);
                    nc.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                    Rigidbody r = nc.GetComponent<Rigidbody>();
                    r.velocity = new Vector3(velX, velY, velZ);
                    r.angularVelocity = new Vector3(angVelX, angVelY, angVelZ);
                }
            }
        }

        [PunRPC]
        public void ReceiveMessageHighlightNetworkNode(string handlerName, string centerName, string geneName)
        {
            referenceManager.networkGenerator?.FindNetworkHandler(handlerName)?.FindNetworkCenter(centerName)
                ?.HighlightNode(geneName, true);
        }

        [PunRPC]
        public void ReceiveMessageUnhighlightNetworkNode(string handlerName, string centerName, string geneName)
        {
            referenceManager.networkGenerator?.FindNetworkHandler(handlerName)?.FindNetworkCenter(centerName)
                ?.HighlightNode(geneName, false);
        }

        // [PunRPC]
        // public void ReceiveMessageSetArcsVisible(bool toggleToState, string buttonName)
        // {
        //     CellexalLog.Log("Toggle arcs of " + buttonName);
        //     referenceManager.arcsSubMenu.NetworkArcsButtonClickedMultiUser(toggleToState, buttonName);
        //     // NetworkCenter network = GameObject.Find(networkName).GetComponent<NetworkCenter>();
        //     // network.SetCombinedArcsVisible(false);
        //     // network.SetArcsVisible(toggleToState);
        // }

        [PunRPC]
        public void ReceiveMessageSetCombinedArcsVisible(bool toggleToState, string networkName)
        {
            CellexalLog.Log("Toggle combined arcs of " + networkName);
            NetworkCenter network = GameObject.Find(networkName).GetComponent<NetworkCenter>();
            network.SetArcsVisible(false);
            network.SetCombinedArcsVisible(toggleToState);
        }

        [PunRPC]
        public void ReceiveMessageToggleAllArcs(bool toggleToState)
        {
            referenceManager.arcsSubMenu.ToggleAllArcs(toggleToState);
        }

        [PunRPC]
        public void ReceiveMessageNetworkArcButtonClicked(string buttonName)
        {
            referenceManager.arcsSubMenu.NetworkArcsButtonClickedMultiUser(buttonName);
        }

        #endregion

        #region Hide tool

        [PunRPC]
        public void ReceiveMessageMinimizeGraph(string graphName)
        {
            Graph g = referenceManager.graphManager.FindGraph(graphName);
            if (g == null) return;
            g.HideGraph();
            referenceManager.minimizedObjectHandler.MinimizeObject(g.gameObject, graphName);
        }

        [PunRPC]
        public void ReceiveMessageShowGraph(string graphName, string jailName)
        {
            Graph g = referenceManager.graphManager.FindGraph(graphName);
            if (g == null) return;
            GameObject jail = GameObject.Find(jailName);
            MinimizedObjectHandler handler = referenceManager.minimizedObjectHandler;
            g.ShowGraph();
            handler.ContainerRemoved(jail.GetComponent<MinimizedObjectContainer>());
            Destroy(jail);
        }

        [PunRPC]
        public void ReceiveMessageMinimizeNetwork(string networkName)
        {
            NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkName);
            if (nh == null) return;
            nh.HideNetworks();
            referenceManager.minimizedObjectHandler.MinimizeObject(nh.gameObject, networkName);
        }

        [PunRPC]
        public void ReceiveMessageShowNetwork(string networkName, string jailName)
        {
            NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkName);
            GameObject jail = GameObject.Find(jailName);
            MinimizedObjectHandler handler = referenceManager.minimizedObjectHandler;
            nh.ShowNetworks();
            handler.ContainerRemoved(jail.GetComponent<MinimizedObjectContainer>());
            Destroy(jail);
        }

        [PunRPC]
        public void ReceiveMessageMinimizeHeatmap(string heatmapName)
        {
            Heatmap h = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            h.HideHeatmap();
            referenceManager.minimizedObjectHandler.MinimizeObject(h.gameObject, heatmapName);
        }

        [PunRPC]
        public void ReceiveMessageShowHeatmap(string heatmapName, string jailName)
        {
            Heatmap h = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            GameObject jail = GameObject.Find(jailName);
            MinimizedObjectHandler handler = referenceManager.minimizedObjectHandler;
            h.ShowHeatmap();
            handler.ContainerRemoved(jail.GetComponent<MinimizedObjectContainer>());
            Destroy(jail);
        }

        #endregion

        #region Delete tool

        [PunRPC]
        public void ReceiveMessageDeleteObject(string name, string tag)
        {
            CellexalLog.Log("Received message to delete object with name: " + name);
            //GameObject objectToDelete = GameObject.Find(name);
            if (tag == "SubGraph")
            {
                referenceManager.graphManager.DeleteGraph(name, tag);
            }
            else if (tag == "FacsGraph")
            {
                referenceManager.graphManager.DeleteGraph(name, tag);
            }
            else if (tag == "HeatBoard")
            {
                referenceManager.heatmapGenerator.DeleteHeatmap(name);
            }
            else if (tag == "Network")
            {
                GameObject.Find(name)?.GetComponent<NetworkHandler>().DeleteNetwork();
            }
        }

        #endregion

        #region Velocity

        [PunRPC]
        public void ReceiveMessageStartVelocity()
        {
            List<Graph> graphs = referenceManager.velocityGenerator.ActiveGraphs;
            foreach (Graph g in graphs)
            {
                g.velocityParticleEmitter.Play();
            }
        }

        [PunRPC]
        public void ReceiveMessageStopVelocity()
        {
            List<Graph> graphs = referenceManager.velocityGenerator.ActiveGraphs;
            foreach (Graph g in graphs)
            {
                g.velocityParticleEmitter.Stop();
            }
        }

        [PunRPC]
        public void ReceiveMessageToggleGraphPoints()
        {
            referenceManager.velocityGenerator.ToggleGraphPoints();
        }

        [PunRPC]
        public void ReceiveMessageConstantSynchedMode()
        {
            referenceManager.velocityGenerator.ChangeConstantSynchedMode();
        }

        [PunRPC]
        public void ReceiveMessageGraphPointColorsMode()
        {
            referenceManager.velocityGenerator.ChangeGraphPointColorMode();
        }

        [PunRPC]
        public void ReceiveMessageChangeParticleMode()
        {
            referenceManager.velocityGenerator.ChangeParticle();
        }

        [PunRPC]
        public void ReceiveMessageChangeFrequency(float amount)
        {
            referenceManager.velocityGenerator.ChangeFrequency(amount);
        }

        [PunRPC]
        public void ReceiveMessageChangeThreshold(float amount)
        {
            referenceManager.velocityGenerator.ChangeThreshold(amount);
        }

        [PunRPC]
        public void ReceiveMessageChangeSpeed(float amount)
        {
            referenceManager.velocityGenerator.ChangeSpeed(amount);
        }

        [PunRPC]
        public void ReceiveMessageReadVelocityFile(string graphName, string subGraphName, bool activate)
        {
            CellexalLog.Log("Received message to read velocity file - " + graphName);
            var veloButton = referenceManager.velocitySubMenu.FindButton(graphName, subGraphName);

            Graph graph = referenceManager.graphManager.FindGraph(subGraphName);
            //if (subGraphName != string.Empty)
            //{
            //    referenceManager.velocityGenerator.ReadVelocityFile(filePath, subGraphName);
            //}
            //else
            //{
            //    referenceManager.velocityGenerator.ReadVelocityFile(filePath);
            //}
            if (activate)
            {
                referenceManager.velocityGenerator.ReadVelocityFile(Path.Combine(CellexalUser.DatasetFullPath, graphName + ".mds"), subGraphName);
            }
            else
            {
                if (graph.graphPointsInactive)
                {
                    graph.ToggleGraphPoints();
                }

                referenceManager.velocityGenerator.ActiveGraphs.Remove(graph);
            }

            referenceManager.velocitySubMenu.DeactivateOutlines();
            veloButton.ToggleOutline(true);
        }

        [PunRPC]
        public void ReceiveMessageToggleAverageVelocity()
        {
            referenceManager.velocityGenerator.ToggleAverageVelocity();
        }

        [PunRPC]
        public void ReceiveMessageChangeAverageVelocityResolution(int value)
        {
            referenceManager.velocityGenerator.ChangeAverageVelocityResolution(value);
        }
        #endregion

        #region Filters

        [PunRPC]
        public void ReceiveMessageSetFilter(string filter)
        {
            CellexalLog.Log("Received message to read filter " + filter);
            referenceManager.filterManager.ParseFilter(filter);
        }

        [PunRPC]
        public void ReceiveMessageResetFilter()
        {
            CellexalLog.Log("Received message to reset filter");
            referenceManager.filterManager.ResetFilter( /*false*/);
        }

        #endregion

        #region Browser

        //[PunRPC]
        //public void ReceiveMessageMoveBrowser(float posX, float posY, float posZ, float rotX, float rotY, float rotZ,
        //    float rotW, float scaleX, float scaleY, float scaleZ)
        //{
        //    GameObject wm = referenceManager.webBrowser;
        //    bool browserExists = wm != null;
        //    if (browserExists)
        //    {
        //        try
        //        {
        //            wm.transform.position = new Vector3(posX, posY, posZ);
        //            wm.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
        //            wm.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        //        }
        //        catch (Exception e)
        //        {
        //            CellexalLog.Log("Could not move browser - Error: " + e);
        //        }
        //    }
        //    else
        //    {
        //        CellexalLog.Log("Could not find browser to move");
        //    }
        //}

        //[PunRPC]
        //public void ReceiveMessageActivateBrowser(bool activate)
        //{
        //    CellexalLog.Log("Received message to toggle web browser");
        //    referenceManager.webBrowser.GetComponent<WebManager>().SetBrowserActive(activate);
        //    //referenceManager.webBrowser.GetComponent<WebManager>().SetVisible(activate);
        //}

        //[PunRPC]
        //public void ReceiveMessageBrowserKeyClicked(string key)
        //{
        //    CellexalLog.Log("Received message to add " + key + " to url field");
        //    referenceManager.webBrowserKeyboard.AddText(key, false);
        //}

        //[PunRPC]
        //public void ReceiveMessageBrowserEnter()
        //{
        //    string text = referenceManager.webBrowserKeyboard.output.text;
        //    referenceManager.webBrowser.GetComponentInChildren<SimpleWebBrowser.WebBrowser>().OnNavigate(text);
        //}

        #endregion

        #region Images

        [PunRPC]
        public void ReceiveMessageScroll(int dir)
        {
            GeoMXImageHandler.instance.slideScroller.Scroll(dir);
        }

        [PunRPC]
        public void ReceiveMessageScrollStack(int dir, int group)
        {
            GeoMXImageHandler.instance.slideScroller.ScrollStack(dir, group);
        }

        #endregion

        #region Spatial
        [PunRPC]
        public void ReceiveMessageSliceGraphAutomatic(int pcID, int axis, int nrOfSlices)
        {
            GraphSlice slice = PointCloudGenerator.instance.pointClouds[pcID].GetComponent<GraphSlice>();
            StartCoroutine(slice.SliceAxis(axis, World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SliceGraphSystem>().GetPoints(pcID), nrOfSlices));
        }

        [PunRPC]
        public void ReceiveMessageSliceGraphManual(int pcID, Vector3 planeNormal, Vector3 planePos)
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SliceGraphSystem>().Slice(pcID, planeNormal, planePos);
        }

        [PunRPC]
        public void ReceiveMessageSliceGraphFromSelection(int pcID)
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SliceGraphSystem>().SliceFromSelection(pcID);
        }

        [PunRPC]
        public void ReceiveMessageSpawnModel(string modelName)
        {
            AllenReferenceBrain.instance.SpawnModel(modelName);
        }

        [PunRPC]
        public void ReceiveMessageToggleReferenceOrgan(int pcID, bool toggle)
        {
            GraphSlice slice = PointCloudGenerator.instance.pointClouds[pcID].GetComponent<GraphSlice>();
            slice.slicerBox.ToggleReferenceOrgan(toggle);
        }

        [PunRPC]
        public void ReceiveMessageUpdateCullingBox(int pcID, Vector3 pos1, Vector3 pos2)
        {
            GraphSlice slice = PointCloudGenerator.instance.pointClouds[pcID].GetComponent<GraphSlice>();
            slice.slicerBox.UpdateCullingBox(pos1, pos2);
        }
        [PunRPC]
        public void ReceiveMessageSpreadMeshes()
        {
            AllenReferenceBrain.instance.Spread();
        }

        [PunRPC]
        public void ReceiveMessageGenerateMeshes()
        {
            MeshGenerator.instance.GenerateMeshes();
        }

        #endregion


        #endregion
    }
}