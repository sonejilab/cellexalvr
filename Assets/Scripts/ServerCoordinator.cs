using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds the remote-callable commands that are sent over between to connected clients.
/// </summary>
class ServerCoordinator : Photon.MonoBehaviour
{
    private List<GameManager> gamemanagers = new List<GameManager>();
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }


    [PunRPC]
    public void SendGraphpointChangedColor(string graphName, string label, float r, float g, float b)
    {
        gameManager.GetComponent<GameManager>().DoGraphpointChangeColor(graphName, label, new Color(r, g, b));
    }
    [PunRPC]
    public void SendColorGraphsByGene(string geneName)
    {
        gameManager.cellManager.GetComponent<CellManager>().ColorGraphsByGene(geneName);
    }
    [PunRPC]
    public void SendColorGraphsByAttribute(string attributeType, float r, float g, float b)
    {
        Color col = new Color(r, g, b);
        gameManager.cellManager.GetComponent<CellManager>().ColorByAttribute(attributeType, col);
    }
    [PunRPC]
    public void SendAddSelect(string graphName, string label)
    {
        gameManager.selectionToolHandler.GetComponent<SelectionToolHandler>().DoClientSelectAdd(graphName, label);
    }
    [PunRPC]
    public void SendConfirmSelection()
    {
        gameManager.selectionToolHandler.ConfirmSelection();
    }
    [PunRPC]
    public void SendMoveGraph(string moveGraphName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW)
    {
        gameManager.DoMoveGraph(moveGraphName, posX, posY, posZ, rotX, rotY, rotZ, rotW);
    }
    [PunRPC]
    public void SendMoveHeatmap(string moveGraphName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW)
    {
        gameManager.DoMoveGraph(moveGraphName, posX, posY, posZ, rotX, rotY, rotZ, rotW);
    }
    [PunRPC]
    public void SendCreateHeatmap()
    {
        gameManager.heatmapGenerator.CreateHeatmap();
    }
}
