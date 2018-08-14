using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds the remote-callable commands that are sent over between to connected clients.
/// </summary>
class ServerCoordinator : Photon.MonoBehaviour
{
    private List<GameManager> gamemanagers = new List<GameManager>();
    private GameManager gameManager;
    private ReferenceManager referenceManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
    }

    #region RPCs
    // these methods are basically messages that are sent over the network from on client to another.

    [PunRPC]
    public void SendReadFolder(string path)
    {
        CellexalLog.Log("Recieved message to read folder at " + path);
        gameManager.referenceManager.inputReader.ReadFolder(path);
    }

    [PunRPC]
    public void SendGraphpointChangedColor(string graphName, string label, float r, float g, float b)
    {
        referenceManager.graphManager.RecolorGraphPoint(graphName, label, new Color(r, g, b));
    }

    [PunRPC]
    public void SendColorGraphsByGene(string geneName)
    {
        CellexalLog.Log("Recieved message to color all graphs by " + geneName);
        Debug.Log("Recieved message to color all graphs by " + geneName);
        referenceManager.cellManager.ColorGraphsByGene(geneName); //, referenceManager.graphManager.GeneExpressionColoringMethod);
    }

    [PunRPC]
    public void SendKeyClick(CurvedVRKeyboard.KeyboardItem item)
    {
        CellexalLog.Log("Recieved message that" + item + "to search field");
        Debug.Log("Recieved message to add letter" + item + "to search field");
        referenceManager.keyboardStatus.HandleClick(item); //, referenceManager.graphManager.GeneExpressionColoringMethod);
    }

    [PunRPC]
    public void SendColorGraphsByPreviousExpression(string geneName)
    {
        CellexalLog.Log("Recieved message to color all graphs by " + geneName);
        referenceManager.cellManager.ColorGraphsByPreviousExpression(geneName);
    }

    [PunRPC]
    public void SendSearchLockToggled(int index)
    {
        CellexalLog.Log("Recieved message to toggle lock number " + index);
        referenceManager.previousSearchesList.searchLocks[index].Click();

    }

    [PunRPC]
    public void SendCalculateCorrelatedGenes(string geneName)
    {
        CellexalLog.Log("Recieved message to calculate genes correlated to " + geneName);
        referenceManager.correlatedGenesList.CalculateCorrelatedGenes(geneName, CellexalExtensions.Definitions.Measurement.GENE);
    }

    [PunRPC]
    public void SendColorGraphsByAttribute(string attributeType, float r, float g, float b)
    {
        CellexalLog.Log("Recieved message to color all graphs by attribute " + attributeType);
        Color col = new Color(r, g, b);
        //referenceManager.cellManager.ColorByAttribute(attributeType, col);
    }

    [PunRPC]
    public void SendAddSelect(string graphName, string label, int newGroup, Color color)
    {
        Debug.Log("SENDING ADD SELECT");
        referenceManager.selectionToolHandler.DoClientSelectAdd(graphName, label, newGroup, color);
    }

    [PunRPC]
    public void SendConfirmSelection()
    {
        CellexalLog.Log("Recieved message to confirm selection");
        referenceManager.selectionToolHandler.ConfirmSelection();
    }

    [PunRPC]
    public void SendMoveGraph(string moveGraphName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
    {
        Graph g = referenceManager.graphManager.FindGraph(moveGraphName);
        g.transform.position = new Vector3(posX, posY, posZ);
        g.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
        g.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    [PunRPC]
    public void SendMoveHeatmap(string heatmapName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
    {
        Heatmap hm = referenceManager.heatmapGenerator.FindHeatmap(heatmapName);
        hm.transform.position = new Vector3(posX, posY, posZ);
        hm.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
        hm.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    [PunRPC]
    public void SendCreateHeatmap()
    {
        CellexalLog.Log("Recieved message to create heatmap");
        referenceManager.heatmapGenerator.CreateHeatmap();
    }

    [PunRPC]
    public void SendGenerateNetworks()
    {
        CellexalLog.Log("Recieved message to generate networks");
        gameManager.networkGenerator.GenerateNetworks();
    }

    [PunRPC]
    public void SendMoveNetwork(string networkName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
    {
        NetworkHandler nh = referenceManager.networkGenerator.FindNetworkHandler(networkName);
        nh.transform.position = new Vector3(posX, posY, posZ);
        nh.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
        nh.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    [PunRPC]
    public void SendEnlargeNetwork(string networkHandlerName, string networkCenterName)
    {
        CellexalLog.Log("Recieved message to enlarge network " + networkCenterName + " in handler " + networkHandlerName);
        gameManager.networkGenerator.FindNetworkHandler(networkHandlerName).FindNetworkCenter(networkCenterName).EnlargeNetwork();
    }

    [PunRPC]
    public void SendBringBackNetwork(string networkHandlerName, string networkCenterName)
    {
        CellexalLog.Log("Recieved message to bring back network " + networkCenterName + " in handler " + networkHandlerName);
        var handler = gameManager.networkGenerator.FindNetworkHandler(networkHandlerName);
        var center = handler.FindNetworkCenter(networkCenterName);
        center.BringBackOriginal();
    }

    [PunRPC]
    public void SendMoveNetworkCenter(string networkHandlerName, string networkCenterName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, float scaleX, float scaleY, float scaleZ)
    {
        Vector3 pos = new Vector3(posX, posY, posZ);
        Quaternion rot = new Quaternion(rotX, rotY, rotZ, rotW);
        Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);
        var handler = gameManager.networkGenerator.FindNetworkHandler(networkHandlerName);
        var center = handler.FindNetworkCenter(networkCenterName);
        center.transform.position = pos;
        center.transform.rotation = rot;
        center.transform.localScale = scale;
    }

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
        gameManager.referenceManager.drawTool.DrawNewLine(col, coords);
    }

    [PunRPC]
    public void SendActivateKeyboard(bool activate)
    {
        gameManager.referenceManager.keyboard.SetKeyboardVisible(activate);
    }

    #endregion
}
