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
    private ServerCoordinator serverCoordinator;
    private ServerCoordinator clientCoordinator;
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
                    serverCoordinator = PhotonNetwork.Instantiate(this.serverCoordinatorPrefab.name, Vector3.zero, Quaternion.identity, 0).GetComponent<ServerCoordinator>();
                    waitingCanvas.SetActive(true);
                    // serverCoordinator.RegisterClient(this);
                }
                else
                {
                    PhotonNetwork.Instantiate("ClientCoordinator", Vector3.zero, Quaternion.identity, 0);

                    //GameObject.Find("ServerCoordinator").GetComponent<ServerCoordinator>().RegisterClient(this);
                }
            }
            else
            {
                Debug.Log("Ignoring scene load for " + SceneManager.GetActiveScene().name);
            }
        }
    }


    #region Photon Messages

    #region Inform methods
    // these methods are called when a client wants to inform all other clients that something has happened

    public void InformReadFolder(string path)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to read folder " + path);
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log(clientCoordinator == null);
            Debug.Log(serverCoordinator == null);
            clientCoordinator.photonView.RPC("SendReadFolder", PhotonTargets.Others, path);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendReadFolder", PhotonTargets.Others, path);
        }
    }

    public void InformGraphPointChangedColor(string graphname, string label, Color color)
    {
        if (!multiplayer) return;
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendGraphpointChangedColor", PhotonTargets.Others, graphname, label, color.r, color.g, color.b);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendGraphpointChangedColor", PhotonTargets.Others, graphname, label, color.r, color.g, color.b);
        }
    }

    public void InformColorGraphsByGene(string geneName)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to color graphs by " + geneName);
        Debug.Log("Informing clients to color graphs by " + geneName);
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendColorGraphsByGene", PhotonTargets.Others, geneName);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendColorGraphsByGene", PhotonTargets.Others, geneName);
        }
    }

    public void InformKeyClicked(CurvedVRKeyboard.KeyboardItem item)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients that" + item + "was clicked");
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("KeyClick", PhotonTargets.Others, item);
        }
        else
        {
            serverCoordinator.photonView.RPC("KeyClick", PhotonTargets.Others, item);
        }
    }

    public void InformColorGraphByPreviousExpression(string geneName)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to color graphs by previous gene " + geneName);
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendColorGraphsByPreviousExpression", PhotonTargets.Others, geneName);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendColorGraphsByPreviousExpression", PhotonTargets.Others, geneName);
        }
    }

    public void InformSearchLockToggled(int index)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to toggle lock number " + index);
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendSearchLockToggled", PhotonTargets.Others, index);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendSearchLockToggled", PhotonTargets.Others, index);
        }
    }

    public void InformToggleMenu()
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to show menu");
        if (PhotonNetwork.isMasterClient)
        {
            //Debug.Log("TOGGLE MENU");
            clientCoordinator.photonView.RPC("SendToggleMenu", PhotonTargets.Others);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendToggleMenu", PhotonTargets.Others);
        }
    }


    public void InformCalculateCorrelatedGenes(int index, string geneName)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to calculate genes correlated to " + geneName);
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendCalculateCorrelatedGenes", PhotonTargets.Others, index, geneName);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendCalculateCorrelatedGenes", PhotonTargets.Others, index, geneName);
        }
    }

    public void InformConfirmSelection()
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to confirm selection");
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendConfirmSelection", PhotonTargets.Others);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendConfirmSelection", PhotonTargets.Others);
        }
    }

    public void InformDrawLine(float r, float g, float b, float[] xcoords, float[] ycoords, float[] zcoords)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to draw line with " + xcoords.Length);
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendDrawLine", PhotonTargets.Others, r, g, b, xcoords, ycoords, zcoords);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendDrawLine", PhotonTargets.Others, r, g, b, xcoords, ycoords, zcoords);
        }
    }

    public void InformClearAllLines()
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to clear all lines");
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendClearAllLines", PhotonTargets.Others);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendClearAllLines", PhotonTargets.Others);
        }
    }

    public void InformClearLastLine()
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to clear last line");
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendClearLastLine", PhotonTargets.Others);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendClearLastLine", PhotonTargets.Others);
        }
    }

    public void InformClearAllLinesWithColor(Color color)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to clear all lines with color: " + color);
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendClearLinesWithColor", PhotonTargets.Others, color.r, color.g, color.b);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendClearLinesWithColor", PhotonTargets.Others, color.r, color.g, color.b);
        }
    }
    public void InformMoveGraph(string moveGraphName, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        if (!multiplayer) return;
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendMoveGraph", PhotonTargets.Others, moveGraphName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendMoveGraph", PhotonTargets.Others, moveGraphName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }
    }

    public void InformResetGraphColor()
    {
        if (!multiplayer) return;
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendResetGraph", PhotonTargets.Others);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendResetGraph", PhotonTargets.Others);
        }
    }

    public void InformMoveHeatmap(string moveHeatmapName, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        if (!multiplayer) return;
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendMoveHeatmap", PhotonTargets.Others, moveHeatmapName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendMoveHeatmap", PhotonTargets.Others, moveHeatmapName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }
    }

    public void InformActivateKeyboard(bool activate)
    {
        if (!multiplayer) return;
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendActivateKeyboard", PhotonTargets.Others, activate);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendActivateKeyboard", PhotonTargets.Others, activate);
        }
    }

    public void InformSelectedAdd(string graphName, string label, int newGroup, Color color)
    {
        if (!multiplayer) return;
        Debug.Log("Informing clients to add cells to selection");
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendAddSelect", PhotonTargets.Others, graphName, label, newGroup, color.r, color.g, color.b);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendAddSelect", PhotonTargets.Others, graphName, label, newGroup, color.r, color.g, color.b);
        }
    }

    public void InformCreateHeatmap()
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to create heatmap");
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendCreateHeatmap", PhotonTargets.Others);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendCreateHeatmap", PhotonTargets.Others);
        }
    }

    public void InformBurnHeatmap(string name)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to create heatmap");
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendBurnHeatmap", PhotonTargets.Others, name);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendBurnHeatmap", PhotonTargets.Others, name);
        }
    }

    public void InformGenerateNetworks()
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to generate networks");
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendGenerateNetworks", PhotonTargets.Others);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendGenerateNetworks", PhotonTargets.Others);
        }
    }

    public void InformMoveNetwork(string moveNetworkName, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        if (!multiplayer) return;
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendMoveNetwork", PhotonTargets.Others, moveNetworkName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendMoveNetwork", PhotonTargets.Others, moveNetworkName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }
    }

    public void InformEnlargeNetwork(string networkHandlerName, string networkName)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to enalarge network " + networkName + " in handler + " + networkHandlerName);
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendEnlargeNetwork", PhotonTargets.Others, networkHandlerName, networkName);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendEnlargeNetwork", PhotonTargets.Others, networkHandlerName, networkName);
        }
    }

    public void InformBringBackNetwork(string networkHandlerName, string networkName)
    {
        if (!multiplayer) return;
        CellexalLog.Log("Informing clients to bring back network " + networkName + " in handler " + networkHandlerName);
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendBringBackNetwork", PhotonTargets.Others, networkHandlerName, networkName);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendBringBackNetwork", PhotonTargets.Others, networkHandlerName, networkName);
        }
    }

    public void InformMoveNetworkCenter(string networkHandlerName, string networkCenterName, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        if (!multiplayer) return;
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendMoveNetworkCenter", PhotonTargets.Others, networkHandlerName, networkCenterName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendMoveNetworkCenter", PhotonTargets.Others, networkHandlerName, networkCenterName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w, scale.x, scale.y, scale.z);
        }
    }

    public void InformSetArcsVisible(bool toggleToState, string networkName)
    {
        if (!multiplayer) return;
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendSetArcsVisible", PhotonTargets.Others, toggleToState, networkName);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendSetArcsVisible", PhotonTargets.Others, toggleToState, networkName);
        }
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
        if (clientCoordinator != null)
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
        if ((clientCoordinator = GameObject.Find("ClientCoordinator(Clone)").GetComponent<ServerCoordinator>()) == null)
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
        if ((serverCoordinator = GameObject.Find("ServerCoordinator(Clone)").GetComponent<ServerCoordinator>()) == null)
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

