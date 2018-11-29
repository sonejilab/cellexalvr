using UnityEngine;
/// <summary>
/// Represents a button that draws lines between all graphpoints that share labels.
/// </summary>
class DrawLinesBetweenChosenGraphPointsButton : CellexalButton
{

    private CellManager cellManager;
    private SelectionToolHandler selectionToolHandler;
    private ControllerModelSwitcher controllerModelSwitcher;
    private Graph fromGraph;
    private Graph toGraph;
    private LayerMask layersToIgnore;
    private bool selecting = false;

    protected override string Description
    {
        get { return "Draw lines between all cells with the same label in other graphs"; }
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        selectionToolHandler = referenceManager.selectionToolHandler;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        layersToIgnore = ~(1 << LayerMask.NameToLayer("GraphLayer"));
        SetButtonActivated(false);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {
        selecting = !selecting;
        if (selecting)
        {
            controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Keyboard;
            controllerModelSwitcher.ActivateDesiredTool();
        }
        else
        {
            fromGraph = toGraph = null;
            controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (selecting && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            var raycastingSource = rightController.transform;
            var ray = new Ray(raycastingSource.position, raycastingSource.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10f, layersToIgnore, QueryTriggerInteraction.Collide))
            {
                var hitGraph = hit.transform.gameObject.GetComponent<Graph>();
                if (!hitGraph) return;
                //print("hit " + hitGraph.GraphName);
                if (!fromGraph)
                {
                    fromGraph = hitGraph;
                }
                else if (!toGraph && hitGraph != fromGraph)
                {
                    //print("drawing");
                    toGraph = hitGraph;
                    // more_cells cellManager.DrawLinesBetweenGraphPoints(selectionToolHandler.GetLastSelection(), fromGraph, toGraph);
                    fromGraph = toGraph = null;
                    CellexalEvents.LinesBetweenGraphsDrawn.Invoke();
                }
            }
        }
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }
}
