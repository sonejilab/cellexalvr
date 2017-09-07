using System.IO;
using UnityEngine;

class SaveHeatmapButton : StationaryButton
{
    protected override string Description
    {
        get { return "Save heatmap image to disk"; }
    }

    protected override void Awake()
    {
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        base.Awake();
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            string dir = Directory.GetCurrentDirectory() + @"\Images";
            if (!Directory.Exists(dir))
            {
                gameObject.GetComponentInParent<Heatmap>().SaveImage();
            }

        }
    }
}

