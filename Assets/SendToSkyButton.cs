using UnityEngine;
using UnityEngine.SceneManagement;

public class SendToSkyButton : StationaryButton
{
    
    public GameObject canvas;
    public SpriteRenderer spriteRend;
    public VRTK.VRTK_StraightPointerRenderer laserPointer;
    //public Sprite gray;
    //public Sprite original;

    protected override string Description
    {
        get { return "Take Snapshots"; }
    }

    void Update()
    {

        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (canvas.activeSelf)
            {
                laserPointer.enabled = false;
                canvas.SetActive(false);
                //standardTexture = original;
            }
            else
            {
                laserPointer.enabled = true;
                canvas.SetActive(true);
                //standardTexture = gray;
            }
            
        }
    }




}
