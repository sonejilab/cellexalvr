using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using CellexalVR.Menu.Buttons.Networks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CellexalVR.Tutorial
{
    /// <summary>
    /// This class takes care of the highlighting and loading of different steps in the tutorial. 
    /// It listens to events to know what buttons to highlight at what time and when to show the description for the next step.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {

        public GameObject[] stepPanels;
        public GameObject descriptionCanvasPrefab;
        public ReferenceManager referenceManager;
        public GameObject highlightSpot;
        public GameObject portal;
        public GameObject screenCanvas;
        public PlayVideo videoPlayer;
        public string[] videos;
        public GameObject helperTextR;
        public GameObject helperTextL;
        //public GameObject triggerParticlesLeft;
        //public GameObject triggerParticlesRight;

        [Header("Highlighting objects")]
        public Material standardMat;
        public Material standardButtonMat;
        public Material highLightMat;
        public Material highLightButtonMat;

        private GameObject keyboardButton;
        private GameObject selToolButton;
        private GameObject newSelButton;
        private GameObject confirmSelButton;
        private GameObject createNetworksButton;
        // private GameObject loadMenuButton;
        private GameObject createHeatmapButton;
        //private GameObject deleteButton;
        private GameObject laserButton;
        private GameObject closeMenuButton;
        //private GameObject controllerHints;

        private int currentStep = 0;
        private SteamVR_Controller.Device device;
        private GameObject rgripRight;
        private GameObject lgripRight;
        private GameObject rgripLeft;
        private GameObject lgripLeft;
        private GameObject triggerRight;
        private GameObject triggerLeft;
        private GameObject trackpadLeft;
        private GameObject trackpadRight;
        private bool referencesSet;


        private GameObject graph;
        private Vector3[] spotPositions = {new Vector3(1.153f, 0.1f, -0f),
                                                new Vector3(-0.331f, 0.1f, 0.84f),
                                                new Vector3(0f, 0.1f, -0.812f)};

        // Object lists to control different states of highlighting
        private GameObject[] objList;
        private bool heatmapCreated;
        private bool networksCreated;
        private GameObject canv;

        //private List<GameObject> steps;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        // Use this for initialization
        void Start()
        {
            if (referenceManager.rightController.isActiveAndEnabled && referenceManager.leftController.isActiveAndEnabled)
            {
                SetReferences();
            }
            //screenCanvas = referenceManager.
        }

        // Update is called once per frame
        void Update()
        {

            if (referenceManager.rightController.isActiveAndEnabled && referenceManager.leftController.isActiveAndEnabled && !referencesSet)
            {
                SetReferences();
            }

            else if (!referencesSet)
            {
                return;
            }

            if (rgripRight.GetComponent<MeshRenderer>() != null && rgripLeft.GetComponent<MeshRenderer>() != null && currentStep == 0)
            {
                LoadTutorialStep(1);
            }

            device = SteamVR_Controller.Input((int)referenceManager.leftController.index);
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                if (referenceManager.mainMenu.GetComponent<MeshRenderer>().enabled)
                {
                    ResetMaterials(new GameObject[] { trackpadLeft, triggerRight });
                    HighlightMaterials(new GameObject[] { triggerLeft });
                    //triggerParticlesLeft.SetActive(true);
                }
                else
                {
                    ResetMaterials(new GameObject[] { triggerLeft });
                    HighlightMaterials(new GameObject[] { trackpadLeft, triggerRight });
                    //triggerParticlesLeft.SetActive(false);
                }
            }
        }


        /// <summary>
        /// When changing step, change canvas with description as well as removing/adding buttons to be highlighted.
        /// </summary>
        /// <param name="stepNr"></param>
        [ConsoleCommand("tutorialManager", aliases: new string[] { "loadStep", "ls" })]
        public void LoadTutorialStep(int stepNr)
        {
            switch (stepNr)
            {
                //Loading data
                case 1:
                    currentStep = 1;
                    CellexalEvents.GraphsLoaded.AddListener(NextStep);
                    CellexalEvents.GraphsLoaded.AddListener(TurnOnSpot);
                    objList = new[] { rgripRight, rgripLeft, lgripRight, lgripLeft };
                    HighlightMaterials(objList);
                    //helperTextL.SetActive(true);
                    //helperTextR.SetActive(true);
                    //stepPanels[currentStep - 1].SetActive(true);
                    canv = Instantiate(descriptionCanvasPrefab, transform);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Step " + currentStep + " of 8: Loading Data \n" +
                                                        "--> Grab the cells and throw them into the cone. \n";
                    //"Squeeze the controller while touching an object to grab it.";
                    videoPlayer.StartVideo(videos[currentStep - 1]);
                    break;

                //Moving graphs
                case 2:
                    currentStep = 2;
                    CellexalEvents.GraphsLoaded.RemoveListener(NextStep);
                    CellexalEvents.GraphsLoaded.RemoveListener(TurnOnSpot);
                    videoPlayer.StopVideo();
                    Destroy(canv);
                    videoPlayer.gameObject.SetActive(false);
                    canv = Instantiate(descriptionCanvasPrefab, transform);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Step " + currentStep + " of 8: Moving & Rotating \n" +
                                                        "--> Move and rotate the graphs while grabbing them. \n" +
                                                        "--> To continue, place the graph in the yellow area on the floor.";
                    //controllerHints.SetActive(false);
                    videoPlayer.gameObject.SetActive(true);
                    videoPlayer.StartVideo(videos[currentStep - 1]);
                    break;

                //Keyboard colouring
                case 3:
                    currentStep = 3;
                    CellexalEvents.GraphsColoredByGene.AddListener(TurnOnSpot);
                    Destroy(canv);
                    videoPlayer.gameObject.SetActive(false);
                    canv = Instantiate(descriptionCanvasPrefab, transform);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Step " + currentStep + " of 8: Coloring by Gene Expression \n" +
                                                        "--> Activate the Keyboard \n" +
                                                        "--> Type in Gata1 and press Enter. \n" +
                                                        "--> To continue, place the graph in the yellow area on the floor.";
                    ResetMaterials(objList);
                    videoPlayer.StopVideo();
                    objList = new[] { triggerLeft, keyboardButton };
                    HighlightMaterials(objList);
                    //triggerParticlesLeft.SetActive(true);
                    videoPlayer.gameObject.SetActive(true);
                    videoPlayer.StartVideo(videos[currentStep - 1]);
                    break;


                //Selection tool
                case 4:
                    currentStep = 4;
                    CellexalEvents.GraphsColoredByGene.RemoveListener(TurnOnSpot);

                    CellexalEvents.SelectionStarted.AddListener(SelectionOn);
                    CellexalEvents.SelectionConfirmed.AddListener(TurnOnSpot);
                    CellexalEvents.SelectionConfirmed.AddListener(SelectionOff);
                    //triggerParticlesRight.SetActive(false);
                    //triggerParticlesLeft.SetActive(false);
                    Destroy(canv);
                    videoPlayer.gameObject.SetActive(false);
                    canv = Instantiate(descriptionCanvasPrefab, transform);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Step " + currentStep + " of 8: Selecting Cells \n" +
                                                        "--> Activate the Selection Tool \n" +
                                                        "--> Hold trigger while touching cells to select them. \n" +
                                                        "--> Change colour by click right/left on the touchpad. \n" +
                                                        "--> Press Confirm Selection.";
                    ResetMaterials(objList);

                    objList = new[] { selToolButton, newSelButton, triggerRight };
                    HighlightMaterials(objList);
                    videoPlayer.StopVideo();

                    referenceManager.graphManager.ResetGraphsColor();
                    videoPlayer.gameObject.SetActive(true);
                    videoPlayer.StartVideo(videos[currentStep - 1]);
                    break;


                //Heatmap creation
                case 5:
                    currentStep = 5;
                    CellexalEvents.SelectionStarted.RemoveListener(SelectionOn);
                    CellexalEvents.SelectionConfirmed.RemoveListener(TurnOnSpot);
                    CellexalEvents.SelectionConfirmed.RemoveListener(SelectionOff);

                    CellexalEvents.HeatmapCreated.AddListener(HeatmapCreated);
                    ResetMaterials(objList);
                    videoPlayer.StopVideo();
                    objList = new[] { selToolButton, createHeatmapButton };
                    HighlightMaterials(objList);
                    Destroy(canv);
                    videoPlayer.gameObject.SetActive(false);
                    canv = Instantiate(descriptionCanvasPrefab, transform);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Step " + currentStep + " of 8: Heatmaps \n" +
                                                        "--> Press Create Heatmap \n" +
                                                        "--> Grab and move it around. \n";


                    videoPlayer.gameObject.SetActive(true);
                    videoPlayer.StartVideo(videos[currentStep - 1]);
                    break;

                // Click on gene on heatmap
                case 6:
                    currentStep = 6;
                    CellexalEvents.HeatmapCreated.RemoveListener(HeatmapCreated);

                    CellexalEvents.GraphsColoredByGene.AddListener(TurnOnSpot);
                    videoPlayer.StopVideo();
                    closeMenuButton.GetComponent<Renderer>().material = highLightButtonMat;
                    laserButton.GetComponent<Renderer>().material = highLightButtonMat;
                    Destroy(canv);
                    videoPlayer.gameObject.SetActive(false);
                    canv = Instantiate(descriptionCanvasPrefab, transform);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Step " + currentStep + " of 8: Heatmaps \n" +
                        "--> Activate Laser Tool \n" +
                        "--> Click on a gene on the right side of the heatmap";


                    videoPlayer.gameObject.SetActive(true);
                    videoPlayer.StartVideo(videos[currentStep - 1]);
                    break;

                //Networks creation
                case 7:
                    currentStep = 7;
                    CellexalEvents.GraphsColoredByGene.RemoveListener(TurnOnSpot);

                    CellexalEvents.NetworkEnlarged.AddListener(TurnOnSpot);
                    referenceManager.controllerModelSwitcher.TurnOffActiveTool(true);
                    ResetMaterials(objList);
                    ResetMaterials(new GameObject[] { closeMenuButton, laserButton });

                    objList = new[] { selToolButton, createNetworksButton };
                    HighlightMaterials(objList);

                    Destroy(canv);
                    videoPlayer.gameObject.SetActive(false);
                    canv = Instantiate(descriptionCanvasPrefab, transform);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Step " + currentStep + " of 8: Networks \n" +
                                                        "--> Press Create Networks \n" +
                                                        "--> Enlarge a Network \n";
                    highlightSpot.SetActive(false);
                    videoPlayer.gameObject.SetActive(true);
                    videoPlayer.StartVideo(videos[currentStep - 1]);
                    break;

                //From start to finish
                case 8:
                    currentStep = 8;
                    CellexalEvents.NetworkEnlarged.RemoveListener(TurnOnSpot);

                    CellexalEvents.NetworkCreated.AddListener(NetworksCreated);
                    CellexalEvents.HeatmapCreated.AddListener(HeatmapCreated);
                    //CellexalEvents.HeatmapCreated.AddListener(FinalLevel);
                    //CellexalEvents.NetworkCreated.AddListener(FinalLevel);
                    heatmapCreated = networksCreated = false;
                    Destroy(canv);
                    videoPlayer.transform.parent.gameObject.SetActive(false);
                    canv = Instantiate(descriptionCanvasPrefab, transform);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Step " + currentStep + " of 8: Putting it all together \n" +
                                                        "--> Load Data \n" +
                                                        "--> Select Cells \n" +
                                                        "--> Create Heatmap and Networks";

                    ResetMaterials(objList);
                    objList = new[] { selToolButton, confirmSelButton, createNetworksButton, createHeatmapButton };
                    referenceManager.loaderController.ResetFolders(true);
                    highlightSpot.SetActive(false);
                    break;

                // Portal back to loading screen
                case 9:
                    currentStep = 9;
                    Destroy(canv);
                    canv = Instantiate(descriptionCanvasPrefab, transform);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Congratulations " + CrossSceneInformation.Username + "! \n" +
                                                                          "You have completed the tutorial. \n" +
                                                                          "Step through the portal to start analyzing your data.";
                    highlightSpot.SetActive(false);
                    ResetMaterials(objList);
                    //helperTextL.SetActive(false);
                    //helperTextR.SetActive(false);
                    TurnOnSpot();
                    break;
            }
            CellexalEvents.CommandFinished.Invoke(true);
        }

        void FinalLevel()
        {
            if (heatmapCreated && networksCreated)
            {
                TurnOnSpot();
            }

        }


        public void NextStep()
        {
            currentStep += 1;
            LoadTutorialStep(currentStep);
            if (!graph)
            {
                graph = GameObject.Find("DDRtree");
            }
        }

        public void CompleteTutorial()
        {
            CrossSceneInformation.Tutorial = false;
            referenceManager.screenCanvas.gameObject.SetActive(true);
            referenceManager.screenCanvas.FadeAnimation();
            referenceManager.loaderController.ResetFolders(true);
            gameObject.SetActive(false);

        }

        void HeatmapCreated()
        {
            createHeatmapButton.GetComponent<Renderer>().material = standardButtonMat;
            heatmapCreated = true;
            if (currentStep == 5)
                NextStep();
            if (currentStep == 8)
                FinalLevel();

        }

        void NetworksCreated()
        {
            createNetworksButton.GetComponent<Renderer>().material = standardButtonMat;
            networksCreated = true;
            if (currentStep == 8)
                FinalLevel();
        }

        void SelectionOn()
        {
            selToolButton.GetComponent<Renderer>().material = standardButtonMat;
            newSelButton.GetComponent<Renderer>().material = standardButtonMat;
            confirmSelButton.GetComponent<Renderer>().material = highLightButtonMat;
            trackpadRight.GetComponent<Renderer>().material = highLightMat;

        }

        void SelectionOff()
        {
            confirmSelButton.GetComponent<Renderer>().material = standardButtonMat;
            trackpadRight.GetComponent<Renderer>().material = standardMat;
            //triggerParticlesRight.SetActive(false);
        }

        void TurnOnSpot()
        {
            BoxCollider col = highlightSpot.GetComponent<BoxCollider>();
            col.enabled = false;
            if (currentStep != 9 && Physics.OverlapBox(highlightSpot.transform.position + col.center, col.size / 2).Length > 0)
            {
                highlightSpot.transform.position = spotPositions[(currentStep + 1) % spotPositions.Length];
            }
            if (currentStep == 9)
            {
                portal.SetActive(true);
            }
            else
            {
                highlightSpot.SetActive(true);
            }
            foreach (Transform child in highlightSpot.transform)
            {
                child.GetComponent<ParticleSystem>().Play();
            }
            col.enabled = true;
        }


        void ResetMaterials(GameObject[] objs)
        {
            foreach (GameObject obj in objs)
            {
                if (obj.GetComponent<CellexalButton>())
                {
                    obj.GetComponent<Renderer>().material = standardButtonMat;
                }
                else
                {
                    obj.GetComponent<Renderer>().material = standardMat;
                }
            }
        }


        void HighlightMaterials(GameObject[] objs)
        {
            foreach (GameObject obj in objs)
            {
                if (obj.GetComponent<CellexalButton>())
                {
                    obj.GetComponent<Renderer>().material = highLightButtonMat;
                }
                else
                {
                    obj.GetComponent<Renderer>().material = highLightMat;
                }
            }
        }

        /// <summary>
        /// Set all references to objects to later be highlighted at different stages in the tutorial. Has to be done after controllers are initialized.
        /// </summary>
        void SetReferences()
        {
            // This currently doesnt work properly if the user is using something other than HTC vive controllers since the names will be different.
            // Menu
            rgripRight = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (right)/Model/rgrip");
            if (rgripRight == null)
            {
                rgripRight = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (right)/Model/handgrip");
            }
            lgripRight = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (right)/Model/lgrip");
            if (lgripRight == null)
            {
                lgripRight = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (right)/Model/handgrip");
            }
            triggerRight = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (right)/Model/trigger");
            trackpadRight = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (right)/Model/trackpad");

            rgripLeft = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (left)/Model/rgrip");
            if (rgripLeft == null)
            {
                rgripLeft = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (left)/Model/handgrip");
            }
            lgripLeft = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (left)/Model/lgrip");
            if (lgripLeft == null)
            {
                lgripLeft = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (left)/Model/handgrip");
            }
            triggerLeft = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (left)/Model/trigger");
            trackpadLeft = GameObject.Find("[VRTK]3.3/SDK setup/[CameraRig]/Controller (left)/Model/trackpad");
            //controllerHints = GameObject.Find("[CameraRig]/Controller (left)/Helper Text");
            //controllerHints.SetActive(true);

            // Buttons
            keyboardButton = GameObject.Find("MenuHolder/Main Menu/Left Buttons/Toggle Keyboard Button");
            selToolButton = GameObject.Find("MenuHolder/Main Menu/Right Buttons/Selection Tool Button");
            newSelButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/New Selection Button");
            confirmSelButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/Confirm Selection Button");
            createNetworksButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/Create Networks Button");
            createHeatmapButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/Create Heatmap Button");
            //deleteButton = GameObject.Find("MenuHolder/Main Menu/Right Buttons/Delete Tool Button");
            laserButton = GameObject.Find("MenuHolder/Main Menu/Right Buttons/Laser Tool Button");
            closeMenuButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/Close Button Box/Close Menu Button");


            //screenCanvas = referenceManager.screenCanvas.gameObject;
            referencesSet = true;
        }
    }
}