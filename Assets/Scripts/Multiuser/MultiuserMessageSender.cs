﻿using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CellexalVR.Multiuser
{
    /// <summary>
    /// This class is responsible for passing commands that are about to be sent to a connected client.
    /// It also spawns the players and objects handling the client-server coordination.
    /// </summary>
    public class MultiuserMessageSender : Photon.PunBehaviour
    {
        #region Public Properties

        public ReferenceManager referenceManager;

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

        public GameObject spectatorPrefab;
        public GameObject ghostPrefab;
        public GameObject serverCoordinatorPrefab;
        public GameObject waitingCanvas;
        public GameObject spectatorRig;
        public GameObject VRRig;
        public bool avatarMenuActive;
        public bool multiplayer;

        private MultiuserMessageReceiver coordinator;
        private List<GameObject> players = new List<GameObject>();
        private Queue<LabelsToSendLater> labelsToAddQueue = new Queue<LabelsToSendLater>();
        private Queue<LabelsToSendLater> labelsToRemoveQueue = new Queue<LabelsToSendLater>();

        private class LabelsToSendLater
        {
            public string graphName;
            public List<string> labels;
            public int newGroup;
            public Color color;

            public LabelsToSendLater(string graphName, int newGroup, Color color)
            {
                this.graphName = graphName;
                this.labels = new List<string>();
                this.newGroup = newGroup;
                this.color = color;
            }

            public bool SameGroup(LabelsToSendLater other)
            {
                return other.graphName == graphName && other.newGroup == newGroup && other.color == color;
            }

            public bool SameGroup(string otherGraphName, int otherNewGroup, Color otherColor)
            {
                return otherGraphName == graphName && otherNewGroup == newGroup && otherColor == color;
            }
        }
        #endregion


        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            //waitingCanvas = referenceManager.screenCanvas.gameObject;
            //StartCoroutine(Init());
        }

        public IEnumerator Init()
        {
            while (!PhotonNetwork.connected)
                yield return null;

            spectatorRig = referenceManager.spectatorRig;
            VRRig = referenceManager.VRRig;
            if (playerPrefab == null)
            {
                Debug.LogError(
                    "<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",
                    this);
            }
            else
            {
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                if (PlayerManager.LocalPlayerInstance == null)
                {
                    Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate

                    // If the user checked the spectator option the name prefix will be Spectator. If spectator spawn an invisible avatar instead.
                    GameObject player = new GameObject();
                    if (CrossSceneInformation.Spectator)
                    {
                        player = PhotonNetwork.Instantiate(spectatorPrefab.name, new Vector3(0f, 5f, 0f),
                            Quaternion.identity, 0);
                        Destroy(VRRig);
                        spectatorRig.SetActive(true);
                    }

                    else if (CrossSceneInformation.Ghost)
                    {
                        Destroy(spectatorRig);
                        player = PhotonNetwork.Instantiate(ghostPrefab.name, new Vector3(0f, 5f, 0f),
                            Quaternion.identity, 0);
                        // Destroy(referenceManager.leftControllerScriptAlias);
                        // Destroy(referenceManager.rightControllerScriptAlias);
                        referenceManager.leftController.GetComponent<MenuToggler>().menuCube.SetActive(false);
                        Destroy(referenceManager.leftController.GetComponent<MenuToggler>());
                    }

                    else if (!CrossSceneInformation.Spectator)
                    {
                        //Destroy(spectatorRig);
                        spectatorRig.SetActive(false);
                        player = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0f, 5f, 0f),
                            Quaternion.identity, 0);
                        ReferenceManager.instance.spectatorRig.GetComponent<SpectatorController>().MirrorVRView();
                    }

                    player.gameObject.name = PhotonNetwork.playerName + player.GetPhotonView().ownerId;
                    players.Add(player);
                    player.GetPhotonView().owner.TagObject = player;


                    coordinator = PhotonNetwork
                        .Instantiate("MultiuserMessageReceiver", Vector3.zero, Quaternion.identity, 0)
                        .GetComponent<MultiuserMessageReceiver>();
                    // if (PhotonNetwork.isMasterClient)
                    // {
                    // }
                    //
                    // if (!PhotonNetwork.isMasterClient)
                    // {
                    //     coordinator = PhotonNetwork
                    //         .Instantiate("MultiuserMessageReceiver", Vector3.zero, Quaternion.identity, 0)
                    //         .GetComponent<MultiuserMessageReceiver>();
                    // }
                }
                else
                {
                    Debug.Log("Ignoring scene load for " + SceneManager.GetActiveScene().name);
                }
            }

        }

        private void Update()
        {
            if (coordinator == null && multiplayer)
            {
                coordinator = GameObject.Find("MultiuserMessageReceiver(Clone)")
                    .GetComponent<MultiuserMessageReceiver>();
            }
        }

        #region Photon Messages

        #region Inform methods

        // these methods are called when a client wants to inform all other clients that something has happened

        #region Loading

        public void SendMessageReadFolder(string path)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to read folder " + path);

            coordinator.photonView.RPC("ReceiveMessageReadFolder", PhotonTargets.Others, path);
        }

        public void SendMessageReadH5Config(string path, Dictionary<string, string> h5config)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients of h5 config");

            coordinator.photonView.RPC("ReceiveMessageH5Config", PhotonTargets.Others, path, h5config);
        }

        public void SendMessageSynchConfig(byte[] data)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to synch relevant parts of config");
            coordinator.photonView.RPC("ReceiveMessageSynchConfig", PhotonTargets.Others, data);
        }

        public void SendMessageLoadingMenu(bool delete)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageLoadingMenu", PhotonTargets.Others, delete);
        }

        #endregion

        #region Interaction

        public void SendMessageEnableColliders(string n)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageEnableColliders", PhotonTargets.Others, n);
        }

        public void SendMessageDisableColliders(string n)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageDisableColliders", PhotonTargets.Others, n);
        }

        public void SendMessageToggleLaser(bool active)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleLaser", PhotonTargets.Others,
                active, coordinator.photonView.ownerId, players[0].gameObject.name);
        }

        public void SendMessageMoveLaser(Transform origin, Vector3 hit)
        {
            if (!multiplayer) return;
            Vector3 originPosition = origin.position;
            coordinator.photonView.RPC("ReceiveMessageMoveLaser", PhotonTargets.Others,
                originPosition.x, originPosition.y, originPosition.z,
                hit.x, hit.y, hit.z, coordinator.photonView.ownerId, players[0].gameObject.name);
        }

        public void SendMessageUpdateSliderValue(SliderController.sliderType type, float value)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageUpdateSliderValue", PhotonTargets.Others, type.ToString(),
                value);
        }

        public void SendMessageShowPDFPages()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageShowPDFPages", PhotonTargets.Others);
        }

        #endregion

        #region Legend

        public void SendMessageToggleLegend()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleLegend", PhotonTargets.Others);
        }

        public void SendMessageMoveLegend(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMoveLegend", PhotonTargets.Others,
                pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void SendMessageLegendUngrabbed(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageLegendUngrabbed", PhotonTargets.Others,
                pos.x, pos.y, pos.z,
                rot.x, rot.y, rot.z, rot.w,
                vel.x, vel.y, vel.z,
                angVel.x, angVel.y, angVel.z);
        }

        public void SendMessageChangeLegend(string legendName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageChangeLegend",
                PhotonTargets.Others, legendName);
        }

        public void SendMessageAttributeLegendChangePage(bool dir)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageAttributeLegendChangePage",
                PhotonTargets.Others, dir);
        }

        public void SendMessageSelectionLegendChangePage(bool dir)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSelectionLegendChangePage",
                PhotonTargets.Others, dir);
        }

        public void SendMessageChangeTab(int index)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageChangeTab",
                PhotonTargets.Others, index);
        }

        public void SendMessageDeactivateSelectedArea()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageDeactivateSelectedArea",
                PhotonTargets.Others);
        }

        public void SendMessageMoveSelectedArea(int hitIndex, int savedGeneExpressionHistogramHitX)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMoveSelectedArea",
                PhotonTargets.Others, hitIndex, savedGeneExpressionHistogramHitX);
        }

        public void SendMessageMoveHighlightArea(int minX, int maxX)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMoveHighlightArea",
                PhotonTargets.Others, minX, maxX);
        }

        public void SendMessageSwitchMode(string mode)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSwitchMode",
                PhotonTargets.Others, mode);
        }

        public void SendMessageChangeThreshold(int increment)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageChangeThreshold",
                PhotonTargets.Others, increment);
        }

        #endregion

        #region Coloring

        public void SendMessageColorGraphsByGene(string geneName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to color graphs by " + geneName);
            Debug.Log("Informing clients to color graphs by " + geneName);
            coordinator.photonView.RPC("ReceiveMessageColorGraphsByGene", PhotonTargets.Others, geneName);
        }

        public void SendMessageColoringMethodChanged(int newMode)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to change coloring mode to " + newMode);
            coordinator.photonView.RPC("ReceiveMessageColoringMethodChanged", PhotonTargets.Others, newMode);
        }

        //public void SendMessageColorGraphByPreviousExpression(string geneName)
        //{
        //    if (!multiplayer) return;
        //    CellexalLog.Log("Informing clients to color graphs by previous gene " + geneName);
        //    coordinator.photonView.RPC("ReceiveMessageColorGraphsByPreviousExpression", PhotonTargets.Others, geneName);
        //}

        public void SendMessageColorByAttribute(string attributeType, bool colored, int colIndex)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to color graphs by attribute " + attributeType);
            coordinator.photonView.RPC("ReceiveMessageColorByAttribute", PhotonTargets.Others, attributeType, colored, colIndex);
        }

        public void SendMessageColorByAttributePointCloud(string attributeType, bool colored)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to color graphs by attribute " + attributeType);
            coordinator.photonView.RPC("ReceiveMessageColorByAttributePointCloud", PhotonTargets.Others, attributeType, colored);
        }
        public void SendMessageToggleAllAttributesPointCloud(bool colored)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to color graphs by attribute " + colored);
            coordinator.photonView.RPC("ReceiveMessageToggleAllAttributesPointCloud", PhotonTargets.Others, colored);
        }

        public void SendMessageColorByIndex(string indexName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to color graphs by index " + indexName);
            coordinator.photonView.RPC("ReceiveMessageColorByIndex", PhotonTargets.Others, indexName);
        }

        public void SendMessageRecolorSelectionPoints()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to recolor color graphs by current selection");
            coordinator.photonView.RPC("ReceiveMessageRecolorSelectionPoints", PhotonTargets.Others);
        }

        public void SendMessageToggleTransparency(bool toggle)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to clear expression colours");
            coordinator.photonView.RPC("ReceiveMessageToggleTransparency", PhotonTargets.Others, toggle);
        }

        public void SendMessageGenerateRandomColors(int n)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to generate random colours");
            coordinator.photonView.RPC("ReceiveMessageGenerateRandomColors",
                PhotonTargets.Others, n);
        }

        public void SendMessageGenerateRainbowColors(int n)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to generate rainbow colours");
            coordinator.photonView.RPC("ReceiveMessageGenerateRainbowColors",
                PhotonTargets.Others, n);
        }

        public void SendMessageHighlightCells(int group, bool highlight)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to highlight " + group + " cells");
            coordinator.photonView.RPC(methodName: "ReceiveMessageHighlightCells", target: PhotonTargets.Others,
                group, highlight);
        }

        #endregion

        #region Keyboard

        public void SendMessageActivateKeyboard(bool activate)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageActivateKeyboard", PhotonTargets.Others, activate);
        }

        public void SendMessageKeyClicked(string value)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients that " + value + " was clicked");
            coordinator.photonView.RPC("ReceiveMessageKeyClicked", PhotonTargets.Others, value);
        }

        public void SendMessageBackspaceKeyClicked()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients that backspace was clicked");
            coordinator.photonView.RPC("ReceiveMessageKBackspaceKeyClicked", PhotonTargets.Others);
        }

        public void SendMessageClearKeyClicked()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients that clear was clicked");
            coordinator.photonView.RPC("ReceiveMessageClearKeyClicked", PhotonTargets.Others);
        }

        public void SendMessageSearchLockToggled(int index)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to toggle lock number " + index);
            coordinator.photonView.RPC("ReceiveMessageSearchLockToggled", PhotonTargets.Others, index);
        }

        public void SendMessageAddAnnotation(string annotation, int index, string gpLabel)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to add annotation: " + annotation);
            coordinator.photonView.RPC("ReceiveMessageAddAnnotation", PhotonTargets.Others, annotation, index, gpLabel);
        }

        public void SendMessageExportAnnotations()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to export annotations");
            coordinator.photonView.RPC("ReceiveMessageExportAnnotations", PhotonTargets.Others);
        }

        public void SendMessageClearExpressionColours()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to clear expression colours");
            coordinator.photonView.RPC("ReceiveMessageClearExpressionColours", PhotonTargets.Others);
        }


        public void SendMessageCalculateCorrelatedGenes(string geneName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to calculate genes correlated to " + geneName);
            coordinator.photonView.RPC("ReceiveMessageCalculateCorrelatedGenes", PhotonTargets.Others, geneName);
        }

        public void SendMessageHandleHistoryPanelClick(string panelName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHandleHistoryPanelClick", PhotonTargets.Others, panelName);
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

        public void SendMessageConfirmSelection()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to confirm selection");
            coordinator.photonView.RPC("ReceiveMessageConfirmSelection", PhotonTargets.Others);
        }

        public void SendMessageSelectedAdd(string graphName, string label, int newGroup, Color color)
        {
            if (!multiplayer) return;
            LabelsToSendLater found = labelsToAddQueue.FirstOrDefault((LabelsToSendLater item) => item.SameGroup(graphName, newGroup, color));
            if (found != null)
            {
                found.labels.Add(label);
            }
            else
            {
                if (labelsToAddQueue.Count == 0)
                {
                    StartCoroutine(SendSelectionAfterDelay(0.5f));
                }
                LabelsToSendLater newGroupToQueue = new LabelsToSendLater(graphName, newGroup, color);
                newGroupToQueue.labels.Add(label);
                labelsToAddQueue.Enqueue(newGroupToQueue);
            }
        }

        private IEnumerator SendSelectionAfterDelay(float delay)
        {
            do
            {
                yield return new WaitForSeconds(delay);
                LabelsToSendLater labelsToSend = labelsToAddQueue.Dequeue();
                SendMessageSelectedAddMany(labelsToSend.graphName, labelsToSend.labels.ToArray(), labelsToSend.newGroup, labelsToSend.color);
            } while (labelsToAddQueue.Count > 0);
        }
        public void SendMessageSelectedAddMany(string graphName, string[] labels, int newGroup, Color color)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageAddSelectMany", PhotonTargets.Others, graphName, labels, newGroup,
                color.r, color.g, color.b);
        }

        public void SendMessageSelectedRemove(string graphName, string label)
        {
            if (!multiplayer) return;
            LabelsToSendLater found = labelsToRemoveQueue.FirstOrDefault((LabelsToSendLater item) => item.SameGroup(graphName, -1, Color.white));
            if (found != null)
            {
                found.labels.Add(label);
            }
            else
            {
                if (labelsToRemoveQueue.Count == 0)
                {
                    StartCoroutine(SendRemoveFromSelectionAfterDelay(0.5f));
                }
                LabelsToSendLater newGroupToQueue = new LabelsToSendLater(graphName, -1, Color.white);
                newGroupToQueue.labels.Add(label);
                labelsToRemoveQueue.Enqueue(newGroupToQueue);
            }
        }

        private IEnumerator SendRemoveFromSelectionAfterDelay(float delay)
        {
            do
            {
                yield return new WaitForSeconds(delay);
                LabelsToSendLater labelsToSend = labelsToRemoveQueue.Dequeue();
                SendMessageSelectedRemoveMany(labelsToSend.graphName, labelsToSend.labels.ToArray());
            } while (labelsToRemoveQueue.Count > 0);
        }

        public void SendMessageSelectedRemoveMany(string graphName, string[] labels)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSelectedRemoveMany", PhotonTargets.Others, graphName, labels);
        }


        public void SendMessageSelectedAddPointCloud(int[] indices, int[] groups)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageAddSelectPointCloud", PhotonTargets.Others, indices, groups);
        }

        public void SendMessageCubeColoured(string graphName, string label, int newGroup, Color color)
        {
            if (!multiplayer) return;
            //Debug.Log("Informing clients to colour selected cube");
            coordinator.photonView.RPC("ReceiveMessageCubeColoured", PhotonTargets.Others, graphName, label, newGroup,
                color.r, color.g, color.b);
        }

        public void SendMessageGoBackOneColor()
        {
            if (!multiplayer) return;
            //Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("ReceiveMessageGoBackOneColor", PhotonTargets.Others);
        }

        public void SendMessageGoBackSteps(int k)
        {
            if (!multiplayer) return;
            //Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("ReceiveMessageGoBackSteps", PhotonTargets.Others, k);
        }

        public void SendMessageCancelSelection()
        {
            if (!multiplayer) return;
            //Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("ReceiveMessageCancelSelection", PhotonTargets.Others);
        }

        public void SendMessageRedoOneColor()
        {
            if (!multiplayer) return;
            //Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("ReceiveMessageRedoOneColor", PhotonTargets.Others);
        }

        public void SendMessageRedoSteps(int k)
        {
            if (!multiplayer) return;
            //Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("ReceiveMessageRedoSteps", PhotonTargets.Others, k);
        }

        public void SendMessageRemoveCells()
        {
            if (!multiplayer) return;
            //Debug.Log("Informing clients to remove selected cells");
            coordinator.photonView.RPC("ReceiveMessageRemoveCells", PhotonTargets.Others);
        }

        public void SendMessageToggleAnnotationFile(string path, bool toggle)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleAnnotationFile", PhotonTargets.Others, path, toggle);
        }

        #endregion

        #region Draw tool

        public void SendMessageDrawLine(float r, float g, float b, float[] xcoords, float[] ycoords, float[] zcoords)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to draw line with " + xcoords.Length);
            coordinator.photonView.RPC("ReceiveMessageDrawLine", PhotonTargets.Others, r, g, b, xcoords, ycoords,
                zcoords);
        }

        public void SendMessageClearAllLines()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to clear all lines");
            coordinator.photonView.RPC("ReceiveMessageClearAllLines", PhotonTargets.Others);
        }

        public void SendMessageClearLastLine()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to clear last line");
            coordinator.photonView.RPC("ReceiveMessageClearLastLine", PhotonTargets.Others);
        }

        public void SendMessageClearAllLinesWithColor(Color color)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to clear all lines with color: " + color);
            coordinator.photonView.RPC("ReceiveMessageClearLinesWithColor", PhotonTargets.Others, color.r, color.g,
                color.b);
        }

        #endregion

        #region Graphs

        public void SendMessageMoveGraph(string moveGraphName, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMoveGraph", PhotonTargets.Others, moveGraphName, pos.x, pos.y,
                pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void SendMessageGraphUngrabbed(string moveGraphName, Vector3 pos, Quaternion rot, Vector3 vel,
            Vector3 angVel)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageGraphUngrabbed", PhotonTargets.Others, moveGraphName,
                pos.x, pos.y, pos.z,
                rot.x, rot.y, rot.z, rot.w,
                vel.x, vel.y, vel.z,
                angVel.x, angVel.y, angVel.z);
        }

        public void SendMessageToggleGrabbable(string name, bool b)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleGrabbable", PhotonTargets.Others, name, b);
        }

        public void SendMessageResetGraphColor()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageResetGraph", PhotonTargets.Others);
        }

        public void SendMessageResetGraphPosition()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageResetGraphPosition", PhotonTargets.Others);
        }

        public void SendMessageDrawLinesBetweenGps(bool toggle = false)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageDrawLinesBetweenGps", PhotonTargets.Others, toggle);
        }

        public void SendMessageBundleAllLines()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageBundleAllLines", PhotonTargets.Others);
        }

        public void SendMessageClearLinesBetweenGps()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageClearLinesBetweenGps", PhotonTargets.Others);
        }

        public void SendMessageAddMarker(string indexName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageAddMarker", PhotonTargets.Others, indexName);
        }

        public void SendMessageCreateMarkerGraph()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageCreateMarkerGraph", PhotonTargets.Others);
        }

        public void SendMessageCreateAttributeGraph()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageCreateAttributeGraph", PhotonTargets.Others);
        }

        public void SendMessageActivateSlices()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageActivateSlices", PhotonTargets.Others);
        }

        public void SendMessageSpatialGraphGrabbed(string sliceName, string graphName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSpatialGraphGrabbed", PhotonTargets.Others, sliceName, graphName);
        }

        public void SendMessageSpatialGraphUnGrabbed(string sliceName, string graphName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSpatialGraphUnGrabbed", PhotonTargets.Others, sliceName,
                graphName);
        }

        public void SendMessageHighlightCluster(bool highlight, string graphName, int id)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHighlightCluster", PhotonTargets.Others,
                highlight, graphName, id);
        }

        public void SendMessageToggleBundle(string graphName, int id)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleBundle", PhotonTargets.Others,
                graphName, id);
        }

        public void SendMessageToggleAxes()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleAxes", PhotonTargets.Others);
        }

        public void SendMessageToggleInfoPanels()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleInfoPanels", PhotonTargets.Others);
        }
        public void SendMessageSpreadPoints(int pcID, bool spread)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSpreadPoints", PhotonTargets.Others, pcID, spread);
        }

        #endregion

        #region Heatmaps

        public void SendMessageMoveHeatmap(string moveHeatmapName, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMoveHeatmap", PhotonTargets.Others, moveHeatmapName, pos.x, pos.y,
                pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void SendMessageCreateHeatmap(string hmName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to create heatmap");
            coordinator.photonView.RPC("ReceiveMessageCreateHeatmap", PhotonTargets.Others, hmName);
        }

        public void SendMessageHandleBoxSelection(string heatmapName, int hitx, int hity, int selectionStartX,
            int selectionStartY)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHandleBoxSelection", PhotonTargets.Others, heatmapName, hitx,
                hity, selectionStartX, selectionStartY);
        }

        public void SendMessageConfirmSelection(string heatmapName, int hitx, int hity, int selectionStartX,
            int selectionStartY)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageConfirmSelection", PhotonTargets.Others, heatmapName, hitx, hity,
                selectionStartX, selectionStartY);
        }

        public void SendMessageHandleMovingSelection(string heatmapName, int hitx, int hity)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHandleMovingSelection", PhotonTargets.Others, heatmapName, hitx,
                hity);
        }

        public void SendMessageMoveSelection(string heatmapName, int hitx, int hity, int selectedGroupLeft,
            int selectedGroupRight, int selectedGeneTop, int selectedGeneBottom)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMoveSelection", PhotonTargets.Others, heatmapName, hitx, hity,
                selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom);
        }

        public void SendMessageHandleHitHeatmap(string heatmapName, int hitx, int hity)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHandleHitHeatmap", PhotonTargets.Others, heatmapName, hitx, hity);
        }

        public void SendMessageResetHeatmapHighlight(string heatmapName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageResetHeatmapHighlight", PhotonTargets.Others, heatmapName);
        }

        public void SendMessageResetSelecting(string heatmapName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageResetSelecting", PhotonTargets.Others, heatmapName);
        }

        public void SendMessageHandlePressDown(string heatmapName, int hitx, int hity)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHandlePressDown", PhotonTargets.Others, heatmapName, hitx, hity);
        }

        public void SendMessageCreateNewHeatmapFromSelection(string heatmapName, int selectedGroupLeft,
            int selectedGroupRight, int selectedGeneTop,
            int selectedGeneBottom, float selectedBoxWidth, float selectedBoxHeight)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageCreateNewHeatmapFromSelection", PhotonTargets.Others, heatmapName,
                selectedGroupLeft, selectedGroupRight,
                selectedGeneTop, selectedGeneBottom, selectedBoxWidth, selectedBoxHeight);
        }

        public void SendMessageReorderByAttribute(string heatmapName, bool order)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageReorderByAttribute", PhotonTargets.Others, heatmapName, order);
        }

        public void SendMessageHandleHitGenesList(string heatmapName, int hity)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHandleHitGenesList", PhotonTargets.Others, heatmapName, hity);
        }

        public void SendMessageHandleHitGroupingBar(string heatmapName, int hitx)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHandleHitGroupingBar", PhotonTargets.Others, heatmapName, hitx);
        }

        public void SendMessageHandleHitAttributeBar(string heatmapName, int hitx)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHandleHitAttributeBar", PhotonTargets.Others, heatmapName, hitx);
        }

        public void SendMessageResetInfoTexts(string heatmapName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageResetInfoTexts", PhotonTargets.Others, heatmapName);
        }

        public void SendMessageCumulativeRecolorFromSelection(string heatmapName, int groupLeft, int groupRight, int selectedTop, int selectedBottom)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageCumulativeRecolorFromSelection", PhotonTargets.Others, heatmapName, groupLeft, groupRight, selectedTop, selectedBottom);
        }

        #endregion

        #region Networks

        public void SendMessageGenerateNetworks(int layoutSeed)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to generate networks");
            coordinator.photonView.RPC("ReceiveMessageGenerateNetworks", PhotonTargets.Others, layoutSeed);
        }

        public void SendMessageMoveNetwork(string moveNetworkName, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMoveNetwork", PhotonTargets.Others, moveNetworkName, pos.x, pos.y,
                pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void SendMessageNetworkUngrabbed(string networkName, Vector3 pos, Quaternion rot, Vector3 vel,
            Vector3 angVel)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageNetworkUngrabbed", PhotonTargets.Others, networkName,
                pos.x, pos.y, pos.z,
                rot.x, rot.y, rot.z, rot.w,
                vel.x, vel.y, vel.z,
                angVel.x, angVel.y, angVel.z);
        }

        public void SendMessageEnlargeNetwork(string networkHandlerName, string networkName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to enalarge network " + networkName + " in handler + " +
                            networkHandlerName);
            coordinator.photonView.RPC("ReceiveMessageEnlargeNetwork", PhotonTargets.Others, networkHandlerName,
                networkName);
        }


        public void SendMessageBringBackNetwork(string networkHandlerName, string networkName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to bring back network " + networkName + " in handler " +
                            networkHandlerName);
            coordinator.photonView.RPC("ReceiveMessageBringBackNetwork", PhotonTargets.Others, networkHandlerName,
                networkName);
        }

        public void SendMessageSwitchNetworkLayout(int layout, string networkName, string networkHandlerName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to switch network layout: " + layout);
            coordinator.photonView.RPC("ReceiveMessageSwitchNetworkLayout", PhotonTargets.Others, layout,
                networkHandlerName, networkName);
        }

        public void SendMessageMoveNetworkCenter(string networkHandlerName, string networkCenterName, Vector3 pos,
            Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMoveNetworkCenter", PhotonTargets.Others, networkHandlerName,
                networkCenterName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void SendMessageNetworkCenterUngrabbed(string networkHandlerName, string networkCenterName,
            Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageNetworkCenterUngrabbed", PhotonTargets.Others, networkHandlerName,
                networkCenterName,
                pos.x, pos.y, pos.z,
                rot.x, rot.y, rot.z, rot.w,
                vel.x, vel.y, vel.z,
                angVel.x, angVel.y, angVel.z);
        }

        public void SendMessageHighlightNetworkNode(string handlerName, string centerName, string geneName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageHighlightNetworkNode", PhotonTargets.Others, handlerName,
                centerName, geneName);
        }

        public void SendMessageUnhighlightNetworkNode(string handlerName, string centerName, string geneName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageUnhighlightNetworkNode", PhotonTargets.Others, handlerName,
                centerName, geneName);
        }

        // public void SendMessageSetArcsVisible(bool toggleToState, string networkName)
        // {
        //     if (!multiplayer) return;
        //     coordinator.photonView.RPC("ReceiveMessageSetArcsVisible", PhotonTargets.Others, toggleToState,
        //         networkName);
        // }

        public void SendMessageSetCombinedArcsVisible(bool toggleToState, string networkName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSetCombinedArcsVisible", PhotonTargets.Others, toggleToState,
                networkName);
        }

        public void SendMessageToggleAllArcs(bool toggleToState)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleAllArcs", PhotonTargets.Others, toggleToState);
        }

        public void SendMessageNetworkArcButtonClicked(string buttonName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageNetworkArcButtonClicked", PhotonTargets.Others, buttonName);
        }

        #endregion

        #region Hide tool

        public void SendMessageMinimizeGraph(string graphName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMinimizeGraph", PhotonTargets.Others, graphName);
        }

        public void SendMessageShowGraph(string graphName, string jailName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageShowGraph", PhotonTargets.Others, graphName, jailName);
        }

        public void SendMessageMinimizeNetwork(string networkName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMinimizeNetwork", PhotonTargets.Others, networkName);
        }

        public void SendMessageMinimizeHeatmap(string heatmapName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMinimizeHeatmap", PhotonTargets.Others, heatmapName);
        }

        public void SendMessageShowNetwork(string networkName, string jailName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageShowNetwork", PhotonTargets.Others, networkName, jailName);
        }

        public void SendMessageShowHeatmap(string heatmapName, string jailName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageShowHeatmap", PhotonTargets.Others, heatmapName, jailName);
        }

        #endregion

        #region Delete tool

        public void SendMessageDeleteObject(string objName, string objTag)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageDeleteObject", PhotonTargets.Others, objName, objTag);
        }

        #endregion

        #region Velocity

        public void SendMessageStartVelocity()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageStartVelocity", PhotonTargets.Others);
        }

        public void SendMessageStopVelocity()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageStopVelocity", PhotonTargets.Others);
        }

        public void SendMessageToggleGraphPoints()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleGraphPoints", PhotonTargets.Others);
        }

        public void SendMessageConstantSynchedMode()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageConstantSynchedMode", PhotonTargets.Others);
        }

        public void SendMessageGraphPointColorsMode()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageGraphPointColorsMode", PhotonTargets.Others);
        }

        public void SendMessageChangeParticleMode()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageChangeParticleMode", PhotonTargets.Others);
        }

        public void SendMessageChangeFrequency(float amount)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageChangeFrequency", PhotonTargets.Others, amount);
        }

        public void SendMessageChangeThreshold(float amount)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageChangeThreshold", PhotonTargets.Others, amount);
        }

        public void SendMessageChangeSpeed(float amount)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageChangeSpeed", PhotonTargets.Others, amount);
        }

        public void SendMessageReadVelocityFile(string filePath, string subGraphName, bool activate)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageReadVelocityFile", PhotonTargets.Others, filePath, subGraphName,
                activate);
        }

        public void SendMessageToggleAverageVelocity()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageToggleAverageVelocity", PhotonTargets.Others);
        }

        public void SendMessageChangeAverageVelocityResolution(int value)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageChangeAverageVelocityResolution", PhotonTargets.Others, value);
        }


        #endregion

        #region Filters

        public void SendMessageSetFilter(string filter)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to set filter to " + filter);
            coordinator.photonView.RPC("ReceiveMessageSetFilter", PhotonTargets.Others, filter);
        }

        public void SendMessageResetFilter()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to reset filter");
            coordinator.photonView.RPC("ReceiveMessageResetFilter", PhotonTargets.Others);
        }

        public void SendMessageRemoveCullingCube()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to remove culling cube");
            coordinator.photonView.RPC("ReceiveMessageRemoveCullingCube", PhotonTargets.Others);
        }

        public void SendMessageAddCullingCube()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to add culling cube");
            coordinator.photonView.RPC("ReceiveMessageAddCullingCube", PhotonTargets.Others);
        }

        #endregion

        #region Browser

        public void SendMessageMoveBrowser(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageMoveBrowser", PhotonTargets.Others, pos.x, pos.y, pos.z, rot.x,
                rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void SendMessageActivateBrowser(bool activate)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to toggle web browser");
            coordinator.photonView.RPC("ReceiveMessageActivateBrowser", PhotonTargets.Others, activate);
        }

        public void SendMessageBrowserKeyClicked(string value)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients that " + value + " was clicked");
            coordinator.photonView.RPC("ReceiveMessageBrowserKeyClicked", PhotonTargets.Others, value);
        }

        public void SendMessageBrowserEnter()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageBrowserEnter", PhotonTargets.Others);
        }

        #endregion

        #region Images

        public void SendMessageScroll(int dir)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageScroll", PhotonTargets.Others, dir);
        }

        public void SendMessageScrollStack(int dir, int group)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageScrollStack", PhotonTargets.Others, dir, group);
        }

        #endregion

        #region Spatial

        public void SendMessageSliceGraphAutomatic(int pcID, int axis, int nrOfSlices)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSliceGraphAutomatic", PhotonTargets.Others, pcID, axis, nrOfSlices);
        }

        public void SendMessageSliceGraphManual(int pcID, Vector3 planeNormal, Vector3 planePos)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSliceGraphManual", PhotonTargets.Others, pcID, planeNormal, planePos);
        }

        public void SendMessageSliceGraphFromSelection(int pcID)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSliceGraphFromSelection", PhotonTargets.Others, pcID);
        }

        public void SendMessageSpawnModel(string modelName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSpawnModel", PhotonTargets.Others, modelName);
        }

        public void SendMessageReferenceOrganToggle(bool toggle, int pcID)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageReferenceOrganToggle", PhotonTargets.Others, pcID, toggle);
        }

        public void SendMessageUpdateCullingBox(int pcID, Vector3 pos1, Vector3 pos2)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageUpdateCullingBox", PhotonTargets.Others, pcID, pos1, pos2);
        }

        public void SendMessageSpreadMeshes()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageSpreadMeshes", PhotonTargets.Others);
        }

        public void SendMessageGenerateMeshes()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("ReceiveMessageGenerateMeshes", PhotonTargets.Others);
        }

        #endregion

        #endregion

        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer other)
        {
            multiplayer = true;
            Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting
            CellexalLog.Log("A client connected to our server");
            //Debug.Log("MASTER JOINED ROOM");
            //LoadArena();
            StartCoroutine(FindClientCoordinator());
            StartCoroutine(FindPlayer());
            //if (coordinator != null)
            //{
            //    waitingCanvas.SetActive(false);
            //}
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            multiplayer = true;
            CellexalLog.Log("We joined a server");
            StartCoroutine(FindServerCoordinator());
            StartCoroutine(FindPlayer());
        }

        private IEnumerator FindPlayer()
        {
            yield return new WaitForSeconds(2f);
            GameObject otherPlayer = GameObject.Find(playerPrefab.gameObject.name + "(Clone)");
            while (otherPlayer == null)
            {
                otherPlayer = GameObject.Find(playerPrefab.gameObject.name + "(Clone)");
                yield return null;
            }

            otherPlayer.gameObject.name = otherPlayer.GetPhotonView().owner.NickName +
                                          otherPlayer.GetPhotonView().ownerId;
        }

        private IEnumerator FindClientCoordinator()
        {
            yield return new WaitForSeconds(2f);
            if ((coordinator = GameObject.Find("MultiuserMessageReceiver(Clone)")
                .GetComponent<MultiuserMessageReceiver>()) == null)
            {
                StartCoroutine(FindClientCoordinator());
            }
            //else
            //{
            //    waitingCanvas.SetActive(false);
            //}
        }

        private IEnumerator FindServerCoordinator()
        {
            yield return new WaitForSeconds(2f);
            coordinator = GameObject.Find("MultiuserMessageReceiver(Clone)")?.GetComponent<MultiuserMessageReceiver>();
            if (!coordinator)
            {
                StartCoroutine(FindServerCoordinator());
            }
        }


        public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
        {
            Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects


            if (PhotonNetwork.isMasterClient)
            {
                Debug.Log("OnPhotonPlayerDisonnected isMasterClient " +
                          PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


                //LoadArena();
            }
        }

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        #endregion

        #region Private Methods

        void LoadArena()
        {
            if (!PhotonNetwork.isMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }

            Debug.Log("PhotonNetwork : Loading Level : PUN_vrjeans_scene");
            PhotonNetwork.LoadLevel("vrjeans_scene1");
        }

        #endregion
    }
}