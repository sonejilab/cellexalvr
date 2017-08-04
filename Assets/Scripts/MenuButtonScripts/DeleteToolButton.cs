using UnityEngine;

class DeleteToolButton : StationaryButton
{
    public ControllerModelSwitcher controllerModelSwitcher;
    public DeleteTool deleteTool;
	public Sprite original;
	public Sprite gray;
	private bool changeSprite;

    protected override string Description
    {
        get { return "Toggle delete tool"; }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
		bool deleteToolActive = deleteTool.gameObject.activeSelf;
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            deleteTool.gameObject.SetActive(!deleteToolActive);
            if (deleteToolActive)
            {
				controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
				changeSprite = true;
				//controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Normal);
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.DeleteTool;
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.DeleteTool);
				changeSprite = true;
            }
        }
		if (changeSprite) {
			if (deleteToolActive) {
				standardTexture = original;
			}
			if (!deleteToolActive) {
				standardTexture = gray;
			}
			spriteRenderer.sprite = standardTexture;
			changeSprite = false;
		}


    }
}
