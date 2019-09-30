using CellexalVR.AnalysisObjects;
using CellexalVR.Extensions;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu.Buttons.Attributes;
using CellexalVR.Menu.Buttons.Facs;
using CellexalVR.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CellexalVR.Multiplayer
{

    /// <summary>
    /// This class holds the remote-callable commands that are sent over network between to connected clients.
    /// To synchronize the scenes in multiplayer it means when a function is called on one client the same has to be done on the others. 
    /// Each function in this class represent one such function to synchronize the scenes.
    /// </summary>
    public class ServerCoordinator : Photon.MonoBehaviour
    {
        private List<GameManager> gamemanagers = new List<GameManager>();
        private GameManager gameManager;
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
            gameManager = referenceManager.gameManager;
            //referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        #region RPCs
        // these methods are basically messages that are sent over the network from on client to another.

        #region Loading
        [PunRPC]
        public void SendReadFolder(string path)
        {
            CellexalLog.Log("Recieved message to read folder at " + path);
            referenceManager.inputReader.ReadFolder(path);
            referenceManager.inputFolderGenerator.DestroyFolders();
        }

        [PunRPC]
        public void SendSynchConfig(byte[] data)
        {
            CellexalLog.Log("Recieved message to synch config");
            referenceManager.configManager.SynchroniseConfig(data);
        }

        [PunRPC]
        public void SendLoadingMenu(bool delete)
        {
            CellexalLog.Log("Recieved message to reset to loading dataset scene");
            referenceManager.loaderController.ResetFolders(delete);
        }
        #endregion

        #region Interaction
        [PunRPC]
        public void SendDisableColliders(string name)
        {
            GameObject obj = GameObject.Find(name);
            Collider[] children = obj.GetComponentsInChildren<Collider>();
            int i = 0;
            foreach (Collider c in children)
            {
                if (c)
                {
                    i++;
                    colliders[c] = c.enabled;
                    c.enabled = false;
                }
            }

        }



        [PunRPC]
        public void SendEnableColliders(string name)
        {
            GameObject obj = GameObject.Find(name);
            Collider[] children = obj.GetComponentsInChildren<Collider>();
            int i = 0;

            foreach (KeyValuePair<Collider, bool> pair in colliders)
            {
                if (pair.Key)
                {
                    i++;
                    pair.Key.enabled = pair.Value;
                }
            }
        }
        #endregion

        #region Coloring
        [PunRPC]
        public void SendColorGraphsByGene(string geneName)
        {
            CellexalLog.Log("Recieved message to color all graphs by " + geneName);
            referenceManager.cellManager.ColorGraphsByGene(geneName); //, referenceManager.graphManager.GeneExpressionColoringMethod);
            referenceManager.geneKeyboard.SubmitOutput(false);
            referenceManager.autoCompleteList.ClearList();
        }

        [PunRPC]
        public void SendColoringMethodChanged(int newMode)
        {
            CellexalLog.Log("Recieved message to change coloring mode to " + newMode);
            referenceManager.coloringOptionsList.SwitchMode((GraphManager.GeneExpressionColoringMethods)newMode);
        }

        //[PunRPC]
        //public void SendColorGraphsByPreviousExpression(string geneName)
        //{
        //    CellexalLog.Log("Recieved message to color all graphs by " + geneName);
        //    referenceManager.cellManager.ColorGraphsByPreviousExpression(geneName);
        //}

        [PunRPC]
        public void SendColorByAttribute(string attributeType, bool toggle)
        {
            CellexalLog.Log("Recieved message to " + (toggle ? "toggle" : "untoggle") + " all graphs by attribute " + attributeType);
            //Color col = new Color(r, g, b);
            referenceManager.cellManager.ColorByAttribute(attributeType, toggle);
            //var attributeButton = GameObject.Find("/[CameraRig]/Controller (left)/Main Menu/Attribute Menu/AttributeTabPrefab(Clone)/" + attributeType);
            var attributeButton = referenceManager.attributeSubMenu.FindButton(attributeType);
            attributeButton.ToggleOutline(toggle);
            //if (attributeButton)
            //{
            //    var outline = attributeButton.GetComponent<ColorByAttributeButton>().activeOutline;
            //    attributeButton.storedState = toggle;
            //}
            attributeButton.GetComponent<ColorByAttributeButton>().colored = toggle;
        }

        [PunRPC]
        public void SendColorByIndex(string indexName)
        {
            CellexalLog.Log("Recieved message to color all graphs by index " + indexName);
            //Color col = new Color(r, g, b);
            referenceManager.cellManager.ColorByIndex(indexName);
            referenceManager.indexMenu.FindButton(indexName).GetComponent<ColorByIndexButton>().TurnOff();
        }
        #endregion

        #region Keyboard

        [PunRPC]
        public void SendActivateKeyboard(bool activate)
        {
            referenceManager.keyboardSwitch.SetKeyboardVisible(activate);
        }

        [PunRPC]
        public void SendKeyClicked(string key)
        {
            CellexalLog.Log("Recieved message to add  " + key + " to search field");
            referenceManager.geneKeyboard.AddText(key, false);
        }

        [PunRPC]
        public void SendKBackspaceKeyClicked()
        {
            CellexalLog.Log("Recieved message to click backspace");
            referenceManager.geneKeyboard.BackSpace(false);
        }

        [PunRPC]
        public void SendClearKeyClicked()
        {
            CellexalLog.Log("Recieved message to clear search field");
            referenceManager.geneKeyboard.Clear(false);
        }

        [PunRPC]
        public void SendSearchLockToggled(int index)
        {
            CellexalLog.Log("Recieved message to toggle lock number " + index);
            referenceManager.previousSearchesList.searchLocks[index].Click();

        }

        [PunRPC]
        public void SendAddAnnotation(string annotation, int index)
        {
            CellexalLog.Log("Recieved message to add annotation: " + annotation);
            referenceManager.selectionManager.AddAnnotation(annotation, index);
        }

        [PunRPC]
        public void SendExportAnnotations()
        {
            CellexalLog.Log("Recieved message to export annotations");
            referenceManager.selectionManager.DumpAnnotatedSelectionToTextFile();
        }

        [PunRPC]
        public void SendClearExpressionColours()
        {
            CellexalLog.Log("Recieved message to clear expression colours on the graphs");
            referenceManager.graphManager.ClearExpressionColours();
        }

        [PunRPC]
        public void SendCalculateCorrelatedGenes(string geneName)
        {
            CellexalLog.Log("Recieved message to calculate genes correlated to " + geneName);
            referenceManager.correlatedGenesList.CalculateCorrelatedGenes(geneName, Definitions.Measurement.GENE);
        }
        #endregion

        #region Selection
        [PunRPC]
        public void SendConfirmSelection()
        {
            CellexalLog.Log("Recieved message to confirm selection");
            referenceManager.selectionManager.ConfirmSelection();
        }

        [PunRPC]
        public void SendAddSelect(string graphName, string label, int newGroup, float r, float g, float b)
        {
            referenceManager.selectionManager.DoClientSelectAdd(graphName, label, newGroup, new Color(r, g, b));
        }

        [PunRPC]
        public void SendCubeColoured(string graphName, string label, int newGroup, float r, float g, float b)
        {
            referenceManager.selectionManager.DoClientSelectAdd(graphName, label, newGroup, new Color(r, g, b), true);
        }

        [PunRPC]
        public void SendGoBackOneColor()
        {
            referenceManager.selectionManager.GoBackOneColorInHistory();
        }

        [PunRPC]
        public void SendGoBackSteps(int k)
        {
            for (int i = 0; i < k; i++)
            {
                referenceManager.selectionManager.GoBackOneStepInHistory();
            }
        }

        [PunRPC]
        public void SendCancelSelection()
        {
            referenceManager.selectionManager.CancelSelection();
        }

        [PunRPC]
        public void SendRedoOneColor()
        {
            referenceManager.selectionManager.GoForwardOneColorInHistory();
        }

        [PunRPC]
        public void SendRedoSteps(int k)
        {
            for (int i = 0; i < k; i++)
            {
                referenceManager.selectionManager.GoForwardOneStepInHistory();
            }
        }
        #endregion

        #region Draw tool
        [PunRPC]
        public void SendDrawLine(float r, float g, float b, float[] xcoords, float[] ycoords, float[] zcoords)
        {
            CellexalLog.Log("Recieved message to draw line with " + xcoords.Length + " segments");
            Vector3[] coords = new Vector3[xcoords.Length];
            for (int i = 0; i < xcoords.Length; i++)
            {
                coords[i] = new Vector3(xcoords[i], ycoords[i], zcoords[i]);

            }
            Color col = new Color(r, g, b);
            referenceManager.drawTool.DrawNewLine(col, coords);
        }

        [PunRPC]
        public void SendClearAllLines()
        {
            CellexalLog.Log("Recieved message to clear line segments");
            referenceManager.drawTool.SkipNextDraw();
            referenceManager.drawTool.ClearAllLines();
        }

        [PunRPC]
        public void SendClearLastLine()
        {
            CellexalLog.Log("Recieved message to clear previous line");
            referenceManager.drawTool.SkipNextDraw();
            referenceManager.drawTool.ClearLastLine();
        }

        [PunRPC]
        public void SendClearLinesWithColor(float r, float g, float b)
        {
            CellexalLog.Log("Recieved message to clear previous line");
            referenceManager.drawTool.SkipNextDraw();
            referenceManager.drawTool.ClearAllLinesWithColor(new Color(r, g, b));
        }
        #endregion

        #region Graphs

        [PunRPC]
        public void SendMoveGraph(string moveGraphName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
        {
            Graph g = referenceManager.graphManager.FindGraph(moveGraphName);
            bool graphExists = g != null;
            if (graphExists)
            {
                try
                {
                    //g.GetComponent<VRTK.VRTK_InteractableObject>().isGrabbable = false;
                    g.transform.position = new Vector3(posX, posY, posZ);
                    g.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                    g.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                    //g.GetComponent<GraphInteract>().StopPositionSync();
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not move graph - Error: " + e);
                }
            }
            else
            {
                CellexalLog.Log("Could not find graph to move");

            }

        }

        [PunRPC]
        public void SendGraphUngrabbed(string graphName, float velX, float velY, float velZ, float angVelX, float angVelY, float angVelZ)
        {
            Graph g = referenceManager.graphManager.FindGraph(graphName);
            if (g)
            {
                Rigidbody r = g.GetComponent<Rigidbody>();
                r.velocity = new Vector3(velX, velY, velZ);
                r.angularVelocity = new Vector3(angVelX, angVelY, angVelZ);
            }
        }

        [PunRPC]
        public void SendToggleGrabbable(string name, bool enable)
        {
            var colliders = referenceManager.graphManager.FindGraph(name).GetComponents<Collider>();
            foreach (Collider c in colliders)
            {
                c.enabled = enable;
            }
        }

        [PunRPC]
        public void SendResetGraph()
        {
            CellexalLog.Log("Recieved message to reset graph colors");
            referenceManager.graphManager.ResetGraphsColor();
        }

        [PunRPC]
        public void SendResetGraphPosition()
        {
            CellexalLog.Log("Recieved message to reset graph position, scale and rotation");
            referenceManager.graphManager.ResetGraphsPosition();
        }

        [PunRPC]
        public void SendDrawLinesBetweenGps()
        {
            CellexalLog.Log("Recieved message to draw lines between graph points");
            StartCoroutine(referenceManager.cellManager.DrawLinesBetweenGraphPoints(referenceManager.selectionManager.GetLastSelection()));
            CellexalEvents.LinesBetweenGraphsDrawn.Invoke();
        }

        [PunRPC]
        public void SendClearLinesBetweenGps()
        {
            CellexalLog.Log("Recieved message to clear lines between graph points");
            referenceManager.cellManager.ClearLinesBetweenGraphPoints();
            CellexalEvents.LinesBetweenGraphsCleared.Invoke();
        }

        [PunRPC]
        public void SendAddMarker(string indexName)
        {
            //var markerButton = GameObject.Find("/Main Menu/Attribute Menu/TabPrefab(Clone)/" + indexName);
            var markerButton = referenceManager.createFromMarkerMenu.FindButton(indexName);
            if (referenceManager.newGraphFromMarkers.markers.Count < 3 && !referenceManager.newGraphFromMarkers.markers.Contains(indexName))
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
        public void SendCreateMarkerGraph()
        {
            CellexalLog.Log("Recieved message to create marker graph");
            referenceManager.newGraphFromMarkers.CreateMarkerGraph();
        }

        [PunRPC]
        public void SendCreateAttributeGraph()
        {
            CellexalLog.Log("Recieved message to create attribute graph");
            referenceManager.graphGenerator.CreateSubGraphs(referenceManager.attributeSubMenu.attributes);
        }

        #endregion

        #region Heatmaps
        [PunRPC]
        public void SendMoveHeatmap(string heatmapName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
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
            else
            {
                CellexalLog.Log("Could not find heatmap to move");
            }
        }

        [PunRPC]
        public void SendCreateHeatmap(string hmName)
        {
            CellexalLog.Log("Recieved message to create heatmap");
            referenceManager.heatmapGenerator.CreateHeatmap(hmName);
        }

        [PunRPC]
        public void SendHandleBoxSelection(string heatmapName, int hitx, int hity, int selectionStartX, int selectionStartY)
        {
            referenceManager.heatmapGenerator.FindHeatmap(heatmapName).GetComponent<HeatmapRaycast>().HandleBoxSelection(hitx, hity, selectionStartX, selectionStartY);
        }

        [PunRPC]
        public void SendConfirmSelection(string heatmapName, int hitx, int hity, int selectionStartX, int selectionStartY)
        {
            referenceManager.heatmapGenerator.FindHeatmap(heatmapName).GetComponent<HeatmapRaycast>().ConfirmSelection(hitx, hity, selectionStartX, selectionStartY);
        }

        [PunRPC]
        public void SendHandleMovingSelection(string heatmapName, int hitx, int hity)
        {
            referenceManager.heatmapGenerator.FindHeatmap(heatmapName).GetComponent<HeatmapRaycast>().HandleMovingSelection(hitx, hity);
        }

        [PunRPC]
        public void SendMoveSelection(string HeatmapName, int hitx, int hity, int selectedGroupLeft, int selectedGroupRight, int selectedGeneTop, int selectedGeneBottom)
        {
            referenceManager.heatmapGenerator.FindHeatmap(HeatmapName).GetComponent<HeatmapRaycast>().MoveSelection(hitx, hity, selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom);
        }

        [PunRPC]
        public void SendHandleHitHeatmap(string HeatmapName, int hitx, int hity)
        {
            try
            {
                referenceManager.heatmapGenerator.FindHeatmap(HeatmapName).GetComponent<HeatmapRaycast>().HandleHitHeatmap(hitx, hity);
            }
            catch (Exception e)
            {
                CellexalLog.Log("Failed to handle hit on heatmap. Stacktrace : " + e.StackTrace);
            }
        }

        [PunRPC]
        public void SendResetHeatmapHighlight(string HeatmapName)
        {
            try
            {
                referenceManager.heatmapGenerator.FindHeatmap(HeatmapName).ResetHeatmapHighlight();
            }
            catch (NullReferenceException e)
            {
                CellexalLog.Log("Failed to reset heatmap highlight. Stacktrace : " + e.StackTrace);
            }
        }

        [PunRPC]
        public void SendResetSelecting(string HeatmapName)
        {
            referenceManager.heatmapGenerator.FindHeatmap(HeatmapName).GetComponent<HeatmapRaycast>().ResetSelecting();
        }

        [PunRPC]
        public void SendHandlePressDown(string heatmapName, int hitx, int hity)
        {
            referenceManager.heatmapGenerator.FindHeatmap(heatmapName).GetComponent<HeatmapRaycast>().HandlePressDown(hitx, hity);
        }

        [PunRPC]
        public void SendCreateNewHeatmapFromSelection(string heatmapName, int selectedGroupLeft, int selectedGroupRight, int selectedGeneTop,
            int selectedGeneBottom, float selectedBoxWidth, float selectedBoxHeight)
        {
            referenceManager.heatmapGenerator.FindHeatmap(heatmapName).CreateNewHeatmapFromSelection(selectedGroupLeft, selectedGroupRight,
                selectedGeneTop, selectedGeneBottom, selectedBoxWidth, selectedBoxHeight);
        }

        [PunRPC]
        public void SendReorderByAttribute(string heatmapName, bool shouldReorder)
        {
            CellexalLog.Log("Recieved message to " + (shouldReorder ? "reorder" : "restore") + " heatmap");
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
        public void SendHandleHitGenesList(string heatmapName, int hity)
        {
            referenceManager.heatmapGenerator.FindHeatmap(heatmapName).GetComponent<HeatmapRaycast>().HandleHitGeneList(hity);
        }

        [PunRPC]
        public void SendHandleHitGroupingBar(string heatmapName, int hitx)
        {
            referenceManager.heatmapGenerator.FindHeatmap(heatmapName).GetComponent<HeatmapRaycast>().HandleHitGroupingBar(hitx);
        }

        [PunRPC]
        public void SendHandleHitAttributeBar(string heatmapName, int hitx)
        {
            referenceManager.heatmapGenerator.FindHeatmap(heatmapName).GetComponent<HeatmapRaycast>().HandleHitAttributeBar(hitx);
        }
        #endregion

        #region Networks
        [PunRPC]
        public void SendGenerateNetworks(int layoutSeed)
        {
            CellexalLog.Log("Recieved message to generate networks");
            referenceManager.networkGenerator.GenerateNetworks(layoutSeed);
        }

        [PunRPC]
        public void SendMoveNetwork(string networkName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
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
                    CellexalLog.Log("Could not move network to move - Error: " + e);
                }
            }
            else
            {
                CellexalLog.Log("Could not find network to move");
            }
        }




        [PunRPC]
        public void SendNetworkUngrabbed(string networkName, float velX, float velY, float velZ, float angVelX, float angVelY, float angVelZ)
        {
            NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkName);
            if (nh)
            {
                Rigidbody r = nh.GetComponent<Rigidbody>();
                r.velocity = new Vector3(velX, velY, velZ);
                r.angularVelocity = new Vector3(angVelX, angVelY, angVelZ);
            }
        }

        [PunRPC]
        public void SendEnlargeNetwork(string networkHandlerName, string networkCenterName)
        {
            CellexalLog.Log("Recieved message to enlarge network " + networkCenterName + " in handler " + networkHandlerName);
            referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName).FindNetworkCenter(networkCenterName).EnlargeNetwork();
        }

        [PunRPC]
        public void SendBringBackNetwork(string networkHandlerName, string networkCenterName)
        {
            CellexalLog.Log("Recieved message to bring back network " + networkCenterName + " in handler " + networkHandlerName);
            var handler = referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName);
            var center = handler.FindNetworkCenter(networkCenterName);
            center.BringBackOriginal();
        }

        [PunRPC]
        public void SendSwitchNetworkLayout(int layout, string networkHandlerName, string networkName)
        {
            CellexalLog.Log("Recieved message to generate networks");
            print("network names:" + networkName + " " + networkHandlerName);
            var handler = referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName);
            var network = handler.FindNetworkCenter(networkName);
            network.SwitchLayout((NetworkCenter.Layout)layout);
        }

        [PunRPC]
        public void SendMoveNetworkCenter(string networkHandlerName, string networkCenterName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
        {
            var handler = referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName);
            var center = handler.FindNetworkCenter(networkCenterName);
            bool networkExists = (handler != null && center != null);
            if (networkExists)
            {
                Vector3 pos = new Vector3(posX, posY, posZ);
                Quaternion rot = new Quaternion(rotX, rotY, rotZ, rotW);
                Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);
                center.transform.position = pos;
                center.transform.rotation = rot;
                center.transform.localScale = scale;
            }
            else
            {
                CellexalLog.Log("Could not find networkcenter to move");
            }
        }

        [PunRPC]
        public void SendNetworkCenterUngrabbed(string networkHandlerName, string networkCenterName, float velX, float velY, float velZ, float angVelX, float angVelY, float angVelZ)
        {
            NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkHandlerName);
            if (nh)
            {
                NetworkCenter nc = nh.FindNetworkCenter(networkCenterName);
                if (nc)
                {
                    Rigidbody r = nc.GetComponent<Rigidbody>();
                    r.velocity = new Vector3(velX, velY, velZ);
                    r.angularVelocity = new Vector3(angVelX, angVelY, angVelZ);
                }
            }
        }

        [PunRPC]
        public void SendSetArcsVisible(bool toggleToState, string networkName)
        {
            CellexalLog.Log("Toggle arcs of " + networkName);
            NetworkCenter network = GameObject.Find(networkName).GetComponent<NetworkCenter>();
            network.SetCombinedArcsVisible(false);
            network.SetArcsVisible(toggleToState);
        }

        [PunRPC]
        public void SendSetCombinedArcsVisible(bool toggleToState, string networkName)
        {
            CellexalLog.Log("Toggle combined arcs of " + networkName);
            NetworkCenter network = GameObject.Find(networkName).GetComponent<NetworkCenter>();
            network.SetArcsVisible(false);
            network.SetCombinedArcsVisible(toggleToState);
        }
        #endregion

        #region Hide tool
        [PunRPC]
        public void SendMinimizeGraph(string graphName)
        {
            Graph g = referenceManager.graphManager.FindGraph(graphName);
            g.HideGraph();
            referenceManager.minimizedObjectHandler.MinimizeObject(g.gameObject, graphName);
        }

        [PunRPC]
        public void SendShowGraph(string graphName, string jailName)
        {
            Graph g = referenceManager.graphManager.FindGraph(graphName);
            GameObject jail = GameObject.Find(jailName);
            MinimizedObjectHandler handler = referenceManager.minimizedObjectHandler;
            g.ShowGraph();
            handler.ContainerRemoved(jail.GetComponent<MinimizedObjectContainer>());
            Destroy(jail);
        }

        [PunRPC]
        public void SendMinimizeNetwork(string networkName)
        {
            NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkName);
            nh.HideNetworks();
            referenceManager.minimizedObjectHandler.MinimizeObject(nh.gameObject, networkName);
        }

        [PunRPC]
        public void SendShowNetwork(string networkName, string jailName)
        {
            NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkName);
            GameObject jail = GameObject.Find(jailName);
            MinimizedObjectHandler handler = referenceManager.minimizedObjectHandler;
            nh.ShowNetworks();
            handler.ContainerRemoved(jail.GetComponent<MinimizedObjectContainer>());
            Destroy(jail);
        }

        [PunRPC]
        public void SendMinimizeHeatmap(string heatmapName)
        {
            Heatmap h = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
            h.HideHeatmap();
            referenceManager.minimizedObjectHandler.MinimizeObject(h.gameObject, heatmapName);
        }

        [PunRPC]
        public void SendShowHeatmap(string heatmapName, string jailName)
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
        public void SendDeleteObject(string name, string tag)
        {
            CellexalLog.Log("Recieved message to delete object with name: " + name);
            GameObject objectToDelete = GameObject.Find(name);
            if (tag == "SubGraph")
            {
                Graph subGraph = objectToDelete.GetComponent<Graph>();
                referenceManager.graphManager.Graphs.Remove(subGraph);
                referenceManager.graphManager.attributeSubGraphs.Remove(subGraph);
                for (int i = 0; i < subGraph.CTCGraphs.Count; i++)
                {
                    subGraph.CTCGraphs[i].GetComponent<GraphBetweenGraphs>().RemoveGraph();
                }
                subGraph.CTCGraphs.Clear();
                Destroy(objectToDelete);
            }
            else if (tag == "FacsGraph")
            {
                Graph facsGraph = objectToDelete.GetComponent<Graph>();
                referenceManager.graphManager.Graphs.Remove(facsGraph);
                referenceManager.graphManager.facsGraphs.Remove(facsGraph);
                for (int i = 0; i < facsGraph.CTCGraphs.Count; i++)
                {
                    facsGraph.CTCGraphs[i].GetComponent<GraphBetweenGraphs>().RemoveGraph();
                }
                facsGraph.CTCGraphs.Clear();
                Destroy(objectToDelete);
            }
            else if (tag == "HeatBoard")
            {
                referenceManager.heatmapGenerator.DeleteHeatmap(name);
            }

        }

        [PunRPC]
        public void SendDeleteNetwork(string name)
        {
            CellexalLog.Log("Recieved message to delete object with name: " + name);
            NetworkHandler nh = GameObject.Find(name).GetComponent<NetworkHandler>();
            //StartCoroutine(nh.DeleteNetwork());
            nh.DeleteNetworkMultiUser();
        }
        #endregion

        #region Velocity
        [PunRPC]
        public void SendStartVelocity(string graphName)
        {
            Graph activeGraph = referenceManager.graphManager.FindGraph(graphName);
            if (activeGraph != null)
            {
                activeGraph.velocityParticleEmitter.Play();
            }
        }

        [PunRPC]
        public void SendStopVelocity(string graphName)
        {
            Graph activeGraph = referenceManager.graphManager.FindGraph(graphName);
            if (activeGraph != null)
            {
                activeGraph.velocityParticleEmitter.Stop();
            }
        }

        [PunRPC]
        public void SendToggleGraphPoints(string graphName)
        {
            Graph activeGraph = referenceManager.graphManager.FindGraph(graphName);
            if (activeGraph != null)
            {
                activeGraph.ToggleGraphPoints();
            }
        }

        [PunRPC]
        public void SendConstantSynchedMode(string graphName, bool switchToConstant)
        {
            Graph activeGraph = referenceManager.graphManager.FindGraph(graphName);
            if (activeGraph != null)
            {
                activeGraph.velocityParticleEmitter.ConstantEmitOverTime = switchToConstant;
                if (switchToConstant)
                {
                    referenceManager.velocitySubMenu.constantSynchedModeText.text = "Mode: Constant";
                }
                else
                {
                    referenceManager.velocitySubMenu.constantSynchedModeText.text = "Mode: Synched";
                }
            }
        }

        [PunRPC]
        public void SendGraphPointColorsMode(string graphName, bool switchToGraphPointColors)
        {
            Graph activeGraph = referenceManager.graphManager.FindGraph(graphName);
            if (activeGraph != null)
            {
                activeGraph.velocityParticleEmitter.UseGraphPointColors = switchToGraphPointColors;
                if (switchToGraphPointColors)
                {
                    referenceManager.velocitySubMenu.graphPointColorsModeText.text = "Mode: Graphpoint colors";
                }
                else
                {
                    referenceManager.velocitySubMenu.graphPointColorsModeText.text = "Mode: Gradient";
                }
            }
        }

        [PunRPC]
        public void SendChangeFrequency(string graphName, float amount)
        {
            Graph activeGraph = referenceManager.graphManager.FindGraph(graphName);
            if (activeGraph != null)
            {
                float newFrequency = activeGraph.velocityParticleEmitter.ChangeFrequency(amount);
                string newFrequencyString = (1f / newFrequency).ToString();
                if (newFrequencyString.Length > 4)
                {
                    newFrequencyString = newFrequencyString.Substring(0, 4);
                }
                referenceManager.velocitySubMenu.frequencyText.text = "Frequency: " + newFrequencyString;
            }
        }

        [PunRPC]
        public void SendChangeThreshold(string graphName, float amount)
        {
            Graph activeGraph = referenceManager.graphManager.FindGraph(graphName);
            if (activeGraph != null)
            {
                float newThreshold = activeGraph.velocityParticleEmitter.ChangeThreshold(amount);
                referenceManager.velocitySubMenu.thresholdText.text = "Threshold: " + newThreshold;
            }
        }

        [PunRPC]
        public void SendChangeSpeed(string graphName, float amount)
        {
            Graph activeGraph = referenceManager.graphManager.FindGraph(graphName);
            if (activeGraph != null)
            {
                float newSpeed = activeGraph.velocityParticleEmitter.ChangeSpeed(amount);
                referenceManager.velocitySubMenu.speedText.text = "Speed: " + newSpeed;
            }
        }

        [PunRPC]
        public void SendReadVelocityFile(string filePath, string subGraphName)
        {
            CellexalLog.Log("Recieved message to read velocity file - " + filePath);
            filePath = Directory.GetCurrentDirectory() + "\\Data\\" + CellexalUser.DataSourceFolder + "\\" + filePath + ".mds";
            var veloButton = referenceManager.velocitySubMenu.FindButton(filePath, subGraphName);
            if (subGraphName != string.Empty)
            {
                referenceManager.velocityGenerator.ReadVelocityFile(filePath, subGraphName);
            }
            else
            {
                referenceManager.velocitySubMenu.ActivateOutline(filePath);
            }
            //referenceManager.velocitySubMenu.DeactivateOutlines();
        }
        #endregion

        #region Filters
        [PunRPC]
        public void SendSetFilter(string filter)
        {
            CellexalLog.Log("Recieved message to read filter " + filter);
            referenceManager.filterManager.ParseFilter(filter);
        }
        #endregion

        #region Browser
        [PunRPC]
        public void SendMoveBrowser(float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
        {
            GameObject wm = referenceManager.webBrowser;
            bool browserExists = wm != null;
            if (browserExists)
            {
                try
                {
                    wm.transform.position = new Vector3(posX, posY, posZ);
                    wm.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                    wm.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not move browser - Error: " + e);
                }
            }
            else
            {
                CellexalLog.Log("Could not find browser to move");
            }
        }

        [PunRPC]
        public void SendActivateBrowser(bool activate)
        {
            CellexalLog.Log("Recieved message to toggle web browser");
            referenceManager.webBrowser.GetComponent<WebManager>().SetBrowserActive(activate);
            //referenceManager.webBrowser.GetComponent<WebManager>().SetVisible(activate);
        }

        [PunRPC]
        public void SendBrowserKeyClicked(string key)
        {
            CellexalLog.Log("Recieved message to add " + key + " to url field");
            referenceManager.webBrowserKeyboard.AddText(key, false);
        }

        [PunRPC]
        public void SendBrowserEnter()
        {
            string text = referenceManager.webBrowserKeyboard.output.text;
            referenceManager.webBrowser.GetComponentInChildren<SimpleWebBrowser.WebBrowser>().OnNavigate(text);
        }

        #endregion

        #endregion

    }
}