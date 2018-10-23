using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds the remote-callable commands that are sent over network between to connected clients.
/// To synchronize the scenes in multiplayer it means when a function is called on one client the same has to be done on the others. 
/// Each function in this class represent one such function to synchronize the scenes.
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
        referenceManager.inputReader.ReadFolder(path);
        referenceManager.inputFolderGenerator.DestroyFolders();
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
        string emptyString = "";
        referenceManager.keyboardStatus.SetOutput(ref emptyString);
        referenceManager.autoCompleteList.ClearList();
    }

    [PunRPC]
    public void SendKeyClick(string key)
    {
        CellexalLog.Log("Recieved message to add" + key + "to search field");
        Debug.Log("Recieved message to add letter" + key + "to search field");
        referenceManager.keyboardStatus.HandleClick(null, key); //, referenceManager.graphManager.GeneExpressionColoringMethod);
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
    public void SendColorByAttribute(string attributeType, bool colored)
    {
        CellexalLog.Log("Recieved message to color all graphs by attribute " + attributeType);
        //Color col = new Color(r, g, b);
        referenceManager.cellManager.ColorByAttribute(attributeType, colored);
    }

    [PunRPC]
    public void SendColorByIndex(string indexName)
    {
        CellexalLog.Log("Recieved message to color all graphs by index " + indexName);
        //Color col = new Color(r, g, b);
        referenceManager.cellManager.ColorByIndex(indexName);
    }

    [PunRPC]
    public void SendAddSelect(string graphName, string label, int newGroup, float r, float g, float b)
    {
        referenceManager.selectionToolHandler.DoClientSelectAdd(graphName, label, newGroup, new Color(r, g, b));
    }

    [PunRPC]
    public void SendCubeColoured(string graphName, string label, int newGroup, float r, float g, float b)
    {
        referenceManager.selectionToolHandler.DoClientSelectAdd(graphName, label, newGroup, new Color(r, g, b), true);
    }

    [PunRPC]
    public void SendGoBackOneColor()
    {
        referenceManager.selectionToolHandler.GoBackOneColorInHistory();
    }

    [PunRPC]
    public void SendGoBackSteps(int k)
    {
        for (int i = 0; i < k; i++)
        {
            referenceManager.selectionToolHandler.GoBackOneStepInHistory();
        }
    }

    [PunRPC]
    public void SendRedoOneColor()
    {
        referenceManager.selectionToolHandler.GoForwardOneColorInHistory();
    }

    [PunRPC]
    public void SendRedoSteps(int k)
    {
        for (int i = 0; i < k; i++)
        {
            referenceManager.selectionToolHandler.GoForwardOneStepInHistory();
        }
    }


    [PunRPC]
    public void SendCancelSelection()
    {
        referenceManager.selectionToolHandler.CancelSelection();
    }

    [PunRPC]
    public void SendConfirmSelection()
    {
        CellexalLog.Log("Recieved message to confirm selection");
        referenceManager.selectionToolHandler.ConfirmSelection();
    }

    [PunRPC]
    public void SendRemoveCells()
    {
        CellexalLog.Log("Recieved message to remove selected selection");
        referenceManager.selectionToolHandler.ConfirmRemove();
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
    public void SendMoveCells(string cellsName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW)
    {
        GameObject c = referenceManager.inputFolderGenerator.FindCells(cellsName);

        c.transform.position = new Vector3(posX, posY, posZ);
        c.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
        //g.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    private Dictionary<Collider, bool> colliders = new Dictionary<Collider, bool>();

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

    [PunRPC]
    public void SendResetGraph()
    {
        Debug.Log("Recieved message to reset graph colors");
        referenceManager.graphManager.ResetGraphsColor();
    }

    [PunRPC]
    public void SendLoadingMenu()
    {
        Debug.Log("Recieved message to reset to loading dataset scene");
        referenceManager.loaderController.ResetFolders();
    }

    [PunRPC]
    public void SendDrawLinesBetweenGps()
    {
        Debug.Log("Recieved message to draw lines between graph points");
        referenceManager.cellManager.DrawLinesBetweenGraphPoints(referenceManager.selectionToolHandler.GetLastSelection());
        CellexalEvents.LinesBetweenGraphsDrawn.Invoke();
    }

    [PunRPC]
    public void SendClearLinesBetweenGps()
    {
        Debug.Log("Recieved message to clear lines between graph points");
        referenceManager.cellManager.ClearLinesBetweenGraphPoints();
        CellexalEvents.LinesBetweenGraphsCleared.Invoke();
    }

    [PunRPC]
    public void SendToggleMenu()
    {
        referenceManager.gameManager.avatarMenuActive = !referenceManager.gameManager.avatarMenuActive;
        Debug.Log("TOGGLE MENU " + referenceManager.gameManager.avatarMenuActive);
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
    public void SendCreateHeatmap(string hmName)
    {
        CellexalLog.Log("Recieved message to create heatmap");
        referenceManager.heatmapGenerator.CreateHeatmap(hmName);
    }

    [PunRPC]
    public void SendDeleteObject(string name, Transform target)
    {
        CellexalLog.Log("Recieved message to delete object with name: " + name);
        Destroy(GameObject.Find(name));
    }

    [PunRPC]
    public void SendGenerateNetworks(int layoutSeed)
    {
        CellexalLog.Log("Recieved message to generate networks");
        gameManager.networkGenerator.GenerateNetworks(layoutSeed);
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
    public void SendSetArcsVisible(bool toggleToState, string networkName)
    {
        CellexalLog.Log("Toggle arcs of " + networkName);
        NetworkCenter network = GameObject.Find(networkName).GetComponent<NetworkCenter>();
        network.SetCombinedArcsVisible(!toggleToState);
        network.SetArcsVisible(toggleToState);
    }

    [PunRPC]
    public void SendSetCombinedArcsVisible(bool toggleToState, string networkName)
    {
        CellexalLog.Log("Toggle combined arcs of " + networkName);
        NetworkCenter network = GameObject.Find(networkName).GetComponent<NetworkCenter>();
        network.SetArcsVisible(!toggleToState);
        network.SetCombinedArcsVisible(toggleToState);
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

    [PunRPC]
    public void SendActivateKeyboard(bool activate)
    {
        referenceManager.keyboard.SetKeyboardVisible(activate);
    }

    [PunRPC]
    public void SendMinimizeGraph(string graphName)
    {
        Debug.Log("MINIMIZING: " + graphName);
        GameObject.Find(graphName).GetComponent<Graph>().HideGraph();
        referenceManager.minimizedObjectHandler.MinimizeObject(GameObject.Find(graphName), graphName);
    }

    [PunRPC]
    public void SendMinimizeNetwork(string networkName)
    {
        GameObject.Find(networkName).GetComponent<NetworkHandler>().HideNetworks();
        referenceManager.minimizedObjectHandler.MinimizeObject(GameObject.Find(networkName), networkName);
    }

    [PunRPC]
    public void SendShowGraph(string graphName, string jailName)
    {
        Graph g = GameObject.Find(graphName).GetComponent<Graph>();
        GameObject jail = GameObject.Find(jailName);
        MinimizedObjectHandler handler = referenceManager.minimizedObjectHandler;
        g.ShowGraph();
        handler.ContainerRemoved(jail.GetComponent<MinimizedObjectContainer>());
        Destroy(jail);
    }

    [PunRPC]
    public void SendShowNetwork(string networkName, string jailName)
    {
        NetworkHandler nh = GameObject.Find(networkName).GetComponent<NetworkHandler>();
        GameObject jail = GameObject.Find(jailName);
        MinimizedObjectHandler handler = referenceManager.minimizedObjectHandler;
        nh.ShowNetworks();
        handler.ContainerRemoved(jail.GetComponent<MinimizedObjectContainer>());
        Destroy(jail);
    }

    [PunRPC]
    public void SendToggleExpressedCells()
    {
        referenceManager.cellManager.ToggleExpressedCells();
    }

    [PunRPC]
    public void SendToggleNonExpressedCells()
    {
        referenceManager.cellManager.ToggleNonExpressedCells();
    }

    [PunRPC]
    public void SendHandleBoxSelection(string heatmapName, int hitx, int hity, int selectionStartX, int selectionStartY)
    {
        referenceManager.heatmapGenerator.FindHeatmap(heatmapName).HandleBoxSelection(hitx, hity, selectionStartX, selectionStartY);
    }

    [PunRPC]
    public void SendConfirmSelection(string heatmapName, int hitx, int hity, int selectionStartX, int selectionStartY)
    {
        referenceManager.heatmapGenerator.FindHeatmap(heatmapName).ConfirmSelection(hitx, hity, selectionStartX, selectionStartY);
    }

    [PunRPC]
    public void SendHandleMovingSelection(string heatmapName, int hitx, int hity)
    {
        referenceManager.heatmapGenerator.FindHeatmap(heatmapName).HandleMovingSelection(hitx, hity);
    }

    [PunRPC]
    public void SendMoveSelection(string HeatmapName, int hitx, int hity, int selectedGroupLeft, int selectedGroupRight, int selectedGeneTop, int selectedGeneBottom)
    {
        referenceManager.heatmapGenerator.FindHeatmap(HeatmapName).MoveSelection(hitx, hity, selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom);
    }

    [PunRPC]
    public void SendHandleHitHeatmap(string HeatmapName, int hitx, int hity)
    {
        referenceManager.heatmapGenerator.FindHeatmap(HeatmapName).HandleHitHeatmap(hitx, hity);
    }

    [PunRPC]
    public void SendResetHeatmapHighlight(string HeatmapName)
    {
        referenceManager.heatmapGenerator.FindHeatmap(HeatmapName).ResetHeatmapHighlight();
    }

    [PunRPC]
    public void SendResetSelecting(string HeatmapName)
    {
        referenceManager.heatmapGenerator.FindHeatmap(HeatmapName).ResetSelecting();
    }

    [PunRPC]
    public void SendHandlePressDown(string heatmapName, int hitx, int hity)
    {
        referenceManager.heatmapGenerator.FindHeatmap(heatmapName).HandlePressDown(hitx, hity);
    }

    [PunRPC]
    public void SendCreateNewHeatmapFromSelection(string heatmapName)
    {
        referenceManager.heatmapGenerator.FindHeatmap(heatmapName).CreateNewHeatmapFromSelection();
    }

    #endregion
}
