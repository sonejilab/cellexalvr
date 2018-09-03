using Photon;
using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for passing commands that are about to be sent to a connected client.
/// </summary>
public class GameManager : Photon.PunBehaviour
{
    #region Public Properties

    public ReferenceManager referenceManager;
    static public GameManager Instance;
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    public GameObject serverCoordinatorPrefab;
    public GameObject waitingCanvas;
    public bool avatarMenuActive;

    private GraphManager graphManager;
    public CellManager cellManager;
    public SelectionToolHandler selectionToolHandler;
    public HeatmapGenerator heatmapGenerator;
    public NetworkGenerator networkGenerator;
    private KeyboardOutput keyboardOut;
    //private ServerCoordinator serverCoordinator;
    //private ServerCoordinator clientCoordinator;
    private ServerCoordinator coordinator;
    private bool multiplayer = false;

    #endregion
    private void Start()
    {
        graphManager = referenceManager.graphManager;
        cellManager = referenceManager.cellManager;
        selectionToolHandler = referenceManager.selectionToolHandler;
        heatmapGenerator = referenceManager.heatmapGenerator;
        networkGenerator = referenceManager.networkGenerator;
        keyboardOut = referenceManager.keyboardOutput;
        Instance = this;
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
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                //Debug.Log("SPAWN CUBE");
                //PhotonNetwork.Instantiate(this.objPrefab.name, new Vector3(0f, 1f, 2f), Quaternion.identity, 0);
                if (PhotonNetwork.isMasterClient)
                {
                    coordinator = PhotonNetwork.Instantiate(this.serverCoordinatorPrefab.name, Vector3.zero, Quaternion.identity, 0).GetComponent<ServerCoordinator>();
                    waitingCanvas.SetActive(true);
                    // serverCoordinator.RegisterClient(this);
                }
                else
                {
                    coordinator = PhotonNetwork.Instantiate("ClientCoordinator", Vector3.zero, Quaternion.identity, 0).GetComponent<ServerCoordinator>();

                    //GameObject.Find("ServerCoordinator").GetComponent<ServerCoordinator>().RegisterClient(this);
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
        if (coordinator == null)
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

    public void InformKeyClicked(CurvedVRKeyboard.KeyboardItem item)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients that" + item + "was clicked");
        string value = item.GetValue();
        coordinator.photonView.RPC("SendKeyClick", PhotonTargets.Others, value);
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

    public void InformResetGraphColor()
    {
        if (!multiplayer) return;
        coordinator.photonView.RPC("SendResetGraph", PhotonTargets.Others);
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

    // HEATMAP
    public void InformMoveHeatmap(string moveHeatmapName, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        if (!multiplayer) return;
        coordinator.photonView.RPC("SendMoveHeatmap", PhotonTargets.Others, moveHeatmapName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
    }

    public void InformCreateHeatmap()
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to create heatmap");
        coordinator.photonView.RPC("SendCreateHeatmap", PhotonTargets.Others);
    }

    public void InformBurnHeatmap(string name)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to create heatmap");
        coordinator.photonView.RPC("SendBurnHeatmap", PhotonTargets.Others, name);
    }


    // NETWORKS
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

    public void InformSetArcsVisible(bool toggleToState, string networkName)
    {
        if (!multiplayer) return;
        coordinator.photonView.RPC("SendSetArcsVisible", PhotonTargets.Others, toggleToState, networkName);
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

