using CellexalVR.Interaction;
using CellexalVR.Menu;
using CellexalVR.Multiplayer;
using Photon;
using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;

namespace CellexalVR.General
{
    /// <summary>
    /// This class is responsible for passing commands that are about to be sent to a connected client.
    /// It also spawns the players and objects handling the client-server coordination.
    /// </summary>
    public class GameManager : Photon.PunBehaviour
    {
        #region Public Properties

        public ReferenceManager referenceManager;
        static public GameManager Instance;
        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;
        public GameObject spectatorPrefab;
        public GameObject ghostPrefab;
        public GameObject serverCoordinatorPrefab;
        public GameObject waitingCanvas;
        public GameObject spectatorRig;
        public GameObject VRRig;
        public bool avatarMenuActive;

        private ServerCoordinator coordinator;
        public bool multiplayer = false;


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
            Instance = this;
            waitingCanvas = referenceManager.screenCanvas.gameObject;
            spectatorRig = referenceManager.spectatorRig;
            VRRig = referenceManager.VRRig;

            if (!PhotonNetwork.connected) return;
            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                if (PlayerManager.LocalPlayerInstance == null)
                {
                    Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate

                    // If the user checked the spectator option the name prefix will be Spectator. If spectator spawn an invisible avatar instead.
                    GameObject player = new GameObject();
                    if (CrossSceneInformation.Spectator)
                    {
                        player = PhotonNetwork.Instantiate(this.spectatorPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                        spectatorRig.SetActive(true);
                        Destroy(VRRig);
                    }

                    else if (CrossSceneInformation.Ghost)
                    {
                        player = PhotonNetwork.Instantiate(this.ghostPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                        Destroy(referenceManager.leftControllerScriptAlias);
                        Destroy(referenceManager.rightControllerScriptAlias);
                        referenceManager.leftController.GetComponent<MenuToggler>().menuCube.SetActive(false);
                        Destroy(referenceManager.leftController.GetComponent<MenuToggler>());
                        Destroy(spectatorRig);
                    }

                    else if (!CrossSceneInformation.Spectator)
                    {
                        player = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                        Destroy(spectatorRig);
                    }

                    player.gameObject.name = PhotonNetwork.playerName;



                    if (PhotonNetwork.isMasterClient)
                    {
                        coordinator = PhotonNetwork.Instantiate(this.serverCoordinatorPrefab.name, Vector3.zero, Quaternion.identity, 0).GetComponent<ServerCoordinator>();

                    }
                    if (!PhotonNetwork.isMasterClient)
                    {
                        coordinator = PhotonNetwork.Instantiate("ClientCoordinator", Vector3.zero, Quaternion.identity, 0).GetComponent<ServerCoordinator>();
                    }
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
                coordinator = GameObject.Find("ClientCoordinator(Clone)").GetComponent<ServerCoordinator>();
            }

        }

        #region Photon Messages

        #region Inform methods
        // these methods are called when a client wants to inform all other clients that something has happened

        public void InformReadFolder(string path)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to read folder " + path);
            coordinator.photonView.RPC("SendReadFolder", PhotonTargets.Others, path);
        }

        public void InformGraphPointChangedColor(string graphname, string label, Color color)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendGraphpointChangedColor", PhotonTargets.Others, graphname, label, color.r, color.g, color.b);
        }

        public void InformColorGraphsByGene(string geneName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to color graphs by " + geneName);
            Debug.Log("Informing clients to color graphs by " + geneName);
            coordinator.photonView.RPC("SendColorGraphsByGene", PhotonTargets.Others, geneName);

        }

        public void InformColoringMethodChanged(int newMode)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to change coloring mode to " + newMode);
            coordinator.photonView.RPC("SendColoringMethodChanged", PhotonTargets.Others, newMode);
        }

        public void InformColorGraphByPreviousExpression(string geneName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to color graphs by previous gene " + geneName);
            coordinator.photonView.RPC("SendColorGraphsByPreviousExpression", PhotonTargets.Others, geneName);
        }

        public void InformColorByAttribute(string attributeType, bool colored)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to color graphs by attribute " + attributeType);
            coordinator.photonView.RPC("SendColorByAttribute", PhotonTargets.Others, attributeType, colored);
        }

        public void InformColorByIndex(string indexName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to color graphs by index " + indexName);
            coordinator.photonView.RPC("SendColorByIndex", PhotonTargets.Others, indexName);
        }

        public void InformKeyClicked(string value)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients that " + value + " was clicked");
            coordinator.photonView.RPC("SendKeyClick", PhotonTargets.Others, value);
        }

        public void InformActivateBrowser(bool activate)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to toggle web browser");
            coordinator.photonView.RPC("SendActivateBrowser", PhotonTargets.Others, activate);
        }

        public void InformBrowserKeyClicked(string value)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients that " + value + " was clicked");
            coordinator.photonView.RPC("SendBrowserKeyClick", PhotonTargets.Others, value);
        }

        public void InformBrowserEnter()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendBrowserEnter", PhotonTargets.Others);
        }

        public void InformSearchLockToggled(int index)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to toggle lock number " + index);
            coordinator.photonView.RPC("SendSearchLockToggled", PhotonTargets.Others, index);
        }

        public void InformToggleMenu()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to show menu");
            coordinator.photonView.RPC("SendToggleMenu", PhotonTargets.Others);
        }

        public void InformAddAnnotation(string annotation)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to add annotation: " + annotation);
            coordinator.photonView.RPC("SendAddAnnotation", PhotonTargets.Others, annotation);
        }

        public void InformExportAnnotations()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to export annotations");
            coordinator.photonView.RPC("SendExportAnnotations", PhotonTargets.Others);
        }

        public void InformClearExpressionColours()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to clear expression colours");
            coordinator.photonView.RPC("SendClearExpressionColours", PhotonTargets.Others);
        }


        public void InformCalculateCorrelatedGenes(string geneName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to calculate genes correlated to " + geneName);
            coordinator.photonView.RPC("SendCalculateCorrelatedGenes", PhotonTargets.Others, geneName);
        }

        public void InformConfirmSelection()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to confirm selection");
            coordinator.photonView.RPC("SendConfirmSelection", PhotonTargets.Others);
        }

        public void InformDrawLine(float r, float g, float b, float[] xcoords, float[] ycoords, float[] zcoords)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to draw line with " + xcoords.Length);
            coordinator.photonView.RPC("SendDrawLine", PhotonTargets.Others, r, g, b, xcoords, ycoords, zcoords);
        }

        public void InformClearAllLines()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to clear all lines");
            coordinator.photonView.RPC("SendClearAllLines", PhotonTargets.Others);
        }

        public void InformClearLastLine()
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to clear last line");
            coordinator.photonView.RPC("SendClearLastLine", PhotonTargets.Others);
        }

        public void InformClearAllLinesWithColor(Color color)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to clear all lines with color: " + color);
            coordinator.photonView.RPC("SendClearLinesWithColor", PhotonTargets.Others, color.r, color.g, color.b);
        }

        public void InformMoveGraph(string moveGraphName, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMoveGraph", PhotonTargets.Others, moveGraphName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void InformGraphUngrabbed(string moveGraphName, Vector3 vel, Vector3 angVel)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendGraphUngrabbed", PhotonTargets.Others, moveGraphName, vel.x, vel.y, vel.z, angVel.x, angVel.y, angVel.z);
        }

        public void InformMoveCells(string cellsName, Vector3 pos, Quaternion rot)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMoveCells", PhotonTargets.Others, cellsName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);
        }

        public void InformResetGraphColor()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendResetGraph", PhotonTargets.Others);
        }

        public void InformResetGraphPosition()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendResetGraphPosition", PhotonTargets.Others);
        }

        public void InformLoadingMenu(bool delete)
        {
            if (!multiplayer) return;
            print("Sending to client to reset folders");
            coordinator.photonView.RPC("SendLoadingMenu", PhotonTargets.Others, delete);
        }


        public void InformDrawLinesBetweenGps()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendDrawLinesBetweenGps", PhotonTargets.Others);
        }

        public void InformClearLinesBetweenGps()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendClearLinesBetweenGps", PhotonTargets.Others);
        }

        public void InformAddMarker(string indexName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendAddMarker", PhotonTargets.Others, indexName);
        }

        public void InformCreateMarkerGraph()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendCreateMarkerGraph", PhotonTargets.Others);
        }

        public void InformCreateAttributeGraph()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendCreateAttributeGraph", PhotonTargets.Others);
        }

        public void InformActivateKeyboard(bool activate)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendActivateKeyboard", PhotonTargets.Others, activate);
        }

        public void InformSelectedAdd(string graphName, string label, int newGroup, Color color)
        {
            if (!multiplayer) return;
            Debug.Log("Informing clients to add cells to selection");
            coordinator.photonView.RPC("SendAddSelect", PhotonTargets.Others, graphName, label, newGroup, color.r, color.g, color.b);
        }

        public void InformCubeColoured(string graphName, string label, int newGroup, Color color)
        {
            if (!multiplayer) return;
            Debug.Log("Informing clients to colour selected cube");
            coordinator.photonView.RPC("SendCubeColoured", PhotonTargets.Others, graphName, label, newGroup, color.r, color.g, color.b);
        }

        public void InformGoBackOneColor()
        {
            if (!multiplayer) return;
            Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("SendGoBackOneColor", PhotonTargets.Others);
        }

        public void InformGoBackSteps(int k)
        {
            if (!multiplayer) return;
            Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("SendGoBackSteps", PhotonTargets.Others, k);
        }

        public void InformCancelSelection()
        {
            if (!multiplayer) return;
            Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("SendCancelSelection", PhotonTargets.Others);
        }

        public void InformRedoOneColor()
        {
            if (!multiplayer) return;
            Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("SendRedoOneColor", PhotonTargets.Others);
        }

        public void InformRedoSteps(int k)
        {
            if (!multiplayer) return;
            Debug.Log("Informing clients to undo last color");
            coordinator.photonView.RPC("SendRedoSteps", PhotonTargets.Others, k);
        }

        public void InformRemoveCells()
        {
            if (!multiplayer) return;
            Debug.Log("Informing clients to remove selected cells");
            coordinator.photonView.RPC("SendRemoveCells", PhotonTargets.Others);
        }


        public void InformMoveHeatmap(string moveHeatmapName, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMoveHeatmap", PhotonTargets.Others, moveHeatmapName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void InformMoveBrowser(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMoveBrowser", PhotonTargets.Others, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void InformDisableColliders(string name)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendDisableColliders", PhotonTargets.Others, name);
        }

        public void InformEnableColliders(string name)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendEnableColliders", PhotonTargets.Others, name);
        }

        public void InformToggleGrabbable(string name, bool b)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendToggleGrabbable", PhotonTargets.Others, name, b);
        }

        public void InformCreateHeatmap(string hmName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to create heatmap");
            coordinator.photonView.RPC("SendCreateHeatmap", PhotonTargets.Others, hmName);
        }


        public void InformGenerateNetworks(int layoutSeed)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to generate networks");
            coordinator.photonView.RPC("SendGenerateNetworks", PhotonTargets.Others, layoutSeed);
        }

        public void InformMoveNetwork(string moveNetworkName, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMoveNetwork", PhotonTargets.Others, moveNetworkName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void InformNetworkUngrabbed(string networkName, Vector3 vel, Vector3 angVel)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendNetworkUngrabbed", PhotonTargets.Others, networkName, vel.x, vel.y, vel.z, angVel.x, angVel.y, angVel.z);
        }

        public void InformEnlargeNetwork(string networkHandlerName, string networkName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to enalarge network " + networkName + " in handler + " + networkHandlerName);
            coordinator.photonView.RPC("SendEnlargeNetwork", PhotonTargets.Others, networkHandlerName, networkName);
        }

        public void InformSwitchNetworkLayout(int layout, string networkName, string networkHandlerName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to switch network layout: " + layout);
            coordinator.photonView.RPC("SendSwitchNetworkLayout", PhotonTargets.Others, layout, networkHandlerName, networkName);
        }


        public void InformBringBackNetwork(string networkHandlerName, string networkName)
        {
            if (!multiplayer) return;
            CellexalLog.Log("Informing clients to bring back network " + networkName + " in handler " + networkHandlerName);
            coordinator.photonView.RPC("SendBringBackNetwork", PhotonTargets.Others, networkHandlerName, networkName);
        }

        public void InformMoveNetworkCenter(string networkHandlerName, string networkCenterName, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMoveNetworkCenter", PhotonTargets.Others, networkHandlerName, networkCenterName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }

        public void InformNetworkCenterUngrabbed(string networkHandlerName, string networkCenterName, Vector3 vel, Vector3 angVel)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendNetworkCenterUngrabbed", PhotonTargets.Others, networkHandlerName, networkCenterName, vel.x, vel.y, vel.z, angVel.x, angVel.y, angVel.z);
        }

        public void InformSetArcsVisible(bool toggleToState, string networkName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendSetArcsVisible", PhotonTargets.Others, toggleToState, networkName);
        }

        public void InformSetCombinedArcsVisible(bool toggleToState, string networkName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendSetCombinedArcsVisible", PhotonTargets.Others, toggleToState, networkName);
        }

        public void InformMinimizeGraph(string graphName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMinimizeGraph", PhotonTargets.Others, graphName);
        }

        public void InformMinimizeNetwork(string networkName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMinimizeNetwork", PhotonTargets.Others, networkName);
        }

        public void InformMinimizeHeatmap(string heatmapName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMinimizeHeatmap", PhotonTargets.Others, heatmapName);
        }

        public void InformDeleteObject(string objName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendDeleteObject", PhotonTargets.Others, objName);
        }

        public void InformDeleteNetwork(string objName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendDeleteNetwork", PhotonTargets.Others, objName);
        }

        public void InformShowGraph(string graphName, string jailName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendShowGraph", PhotonTargets.Others, graphName, jailName);
        }

        public void InformShowNetwork(string networkName, string jailName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendShowNetwork", PhotonTargets.Others, networkName, jailName);
        }

        public void InformShowHeatmap(string heatmapName, string jailName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendShowHeatmap", PhotonTargets.Others, heatmapName, jailName);
        }

        public void InformToggleExpressedCells()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendToggleExpressedCells", PhotonTargets.Others);
        }

        public void InformToggleNonExpressedCells()
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendToggleNonExpressedCells", PhotonTargets.Others);
        }

        public void InformHandleBoxSelection(string heatmapName, int hitx, int hity, int selectionStartX, int selectionStartY)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendHandleBoxSelection", PhotonTargets.Others, heatmapName, hitx, hity, selectionStartX, selectionStartY);
        }

        public void InformConfirmSelection(string heatmapName, int hitx, int hity, int selectionStartX, int selectionStartY)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendConfirmSelection", PhotonTargets.Others, heatmapName, hitx, hity, selectionStartX, selectionStartY);
        }

        public void InformHandleMovingSelection(string heatmapName, int hitx, int hity)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendHandleMovingSelection", PhotonTargets.Others, heatmapName, hitx, hity);
        }

        public void InformMoveSelection(string heatmapName, int hitx, int hity, int selectedGroupLeft, int selectedGroupRight, int selectedGeneTop, int selectedGeneBottom)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendMoveSelection", PhotonTargets.Others, heatmapName, hitx, hity, selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom);
        }

        public void InformHandleHitHeatmap(string heatmapName, int hitx, int hity)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendHandleHitHeatmap", PhotonTargets.Others, heatmapName, hitx, hity);
        }

        public void InformResetHeatmapHighlight(string heatmapName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendResetHeatmapHighlight", PhotonTargets.Others, heatmapName);
        }

        public void InformResetSelecting(string heatmapName)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendResetSelecting", PhotonTargets.Others, heatmapName);
        }

        public void InformHandlePressDown(string heatmapName, int hitx, int hity)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendHandlePressDown", PhotonTargets.Others, heatmapName, hitx, hity);
        }

        public void InformCreateNewHeatmapFromSelection(string heatmapName, int selectedGroupLeft, int selectedGroupRight, int selectedGeneTop,
            int selectedGeneBottom, float selectedBoxWidth, float selectedBoxHeight)
        {
            if (!multiplayer) return;
            coordinator.photonView.RPC("SendCreateNewHeatmapFromSelection", PhotonTargets.Others, heatmapName, selectedGroupLeft, selectedGroupRight,
                selectedGeneTop, selectedGeneBottom, selectedBoxWidth, selectedBoxHeight);
        }

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
            if (coordinator != null)
            {
                waitingCanvas.SetActive(false);
            }

        }
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            multiplayer = true;
            Debug.Log("CLIENT JOINED ROOM");
            CellexalLog.Log("We joined a server");
            StartCoroutine(FindServerCoordinator());
        }

        private IEnumerator FindClientCoordinator()
        {
            yield return new WaitForSeconds(2f);
            if ((coordinator = GameObject.Find("ClientCoordinator(Clone)").GetComponent<ServerCoordinator>()) == null)
            {
                StartCoroutine(FindClientCoordinator());
            }
            else
            {
                waitingCanvas.SetActive(false);
            }
            Debug.Log("Client Coordinator Found");
        }

        private IEnumerator FindServerCoordinator()
        {
            yield return new WaitForSeconds(2f);
            if ((coordinator = GameObject.Find("ClientCoordinator(Clone)").GetComponent<ServerCoordinator>()) == null)
            {
                StartCoroutine(FindServerCoordinator());
            }
            Debug.Log("Server Coordinator Found");
        }


        public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
        {
            Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects


            if (PhotonNetwork.isMasterClient)
            {
                Debug.Log("OnPhotonPlayerDisonnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


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
