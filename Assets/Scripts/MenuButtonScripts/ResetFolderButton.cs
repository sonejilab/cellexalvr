using System;
using UnityEngine;

///<summary>
/// This class represents a button used for resetting the input data folders.
///</summary>
public class ResetFolderButton : StationaryButton
{

    public GraphManager graphManager;
    public InputFolderGenerator inputFolderGenerator;
    public LoaderController loader;
    public PreviousSearchesList previousSearchesList;
	public GameObject inputFolderList;
    private bool menuActive = false;
    private bool buttonsInitialized = false;


    protected override string Description
    {
        get
        {
            return "Go back to loading a folder";
        }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
			//var sceneLoader = GameObject.Find ("Load").GetComponent<Loading> ();
			//sceneLoader.doLoad = false;
            graphManager.DeleteGraphs();
            previousSearchesList.ClearList();
            // must reset loader before generating new folders
            loader.ResetLoaderBooleans();
            inputFolderGenerator.GenerateFolders();
			inputFolderList.gameObject.SetActive (true);
            if (loader.loaderMovedDown)
            {
                loader.loaderMovedDown = false;
                loader.MoveLoader(new Vector3(0f, 2f, 0f), 2f);
            }
        }
    }
}
