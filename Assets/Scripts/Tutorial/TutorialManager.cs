﻿using CellexalVR.AnalysisObjects;
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

        public GameObject tutorialCanvas;
        public GameObject[] stepPanels;
        public ReferenceManager referenceManager;
        public GameObject highlightSpot;
        public GameObject portal;
        public GameObject triggerParticlesLeft;
        public GameObject triggerParticlesRight;

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
        private GameObject deleteButton;
        private GameObject laserButton;
        private GameObject closeMenuButton;
        private GameObject controllerHints;

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
        private float duration = 1f;


        private GameObject graph;
        private Vector3[] spotPositions = {new Vector3(1.153f, 0.1f, -0f),
                                                new Vector3(-0.331f, 0.1f, 0.84f),
                                                new Vector3(0f, 0.1f, -0.812f)};

        // Object lists to control different states of highlighting
        private GameObject[] objList;
        private bool heatmapCreated;
        private bool networksCreated;

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
            // Events to listen to to determine when to move on and when to highlight different objects.
            CellexalEvents.GraphsLoaded.AddListener(NextStep);
            CellexalEvents.GraphsLoaded.AddListener(TurnOnSpot);
            CellexalEvents.GraphsColoredByGene.AddListener(TurnOnSpot);
            CellexalEvents.SelectionStarted.AddListener(SelectionOn);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOnSpot);
            CellexalEvents.SelectionConfirmed.AddListener(SelectionOff);
            CellexalEvents.NetworkUnEnlarged.AddListener(TurnOnSpot);
            CellexalEvents.HeatmapCreated.AddListener(HeatmapCreated);
            CellexalEvents.NetworkCreated.AddListener(NetworksCreated);
            CellexalEvents.KeyboardToggled.AddListener(NextDescription);
            //CellexalEvents.HeatmapBurned.AddListener(TurnOnSpot);


            if (referenceManager.rightController.isActiveAndEnabled && referenceManager.leftController.isActiveAndEnabled)
            {
                SetReferences();
            }
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
                    triggerParticlesLeft.SetActive(true);
                }
                else
                {
                    ResetMaterials(new GameObject[] { triggerLeft });
                    HighlightMaterials(new GameObject[] { trackpadLeft, triggerRight });
                    triggerParticlesLeft.SetActive(false);
                }
            }
        }


        /// <summary>
        /// When changing step, change canvas with description as well as removing/adding buttons to be highlighted.
        /// </summary>
        /// <param name="stepNr"></param>
        public void LoadTutorialStep(int stepNr)
        {
            Debug.Log("LOAD STEP: " + stepNr);

            switch (stepNr)
            {
                //Loading data
                case 1:
                    currentStep = 1;
                    objList = new[] { rgripRight, rgripLeft, lgripRight, lgripLeft };
                    HighlightMaterials(objList);

                    stepPanels[currentStep - 1].SetActive(true);
                    break;

                //Moving graphs
                case 2:
                    currentStep = 2;
                    controllerHints.SetActive(false);
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    break;

                //Keyboard colouring
                case 3:
                    currentStep = 3;
                    ResetMaterials(objList);

                    objList = new[] { triggerLeft, keyboardButton };
                    HighlightMaterials(objList);
                    triggerParticlesLeft.SetActive(true);

                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    break;


                //Selection tool
                case 4:
                    currentStep = 4;
                    triggerParticlesRight.SetActive(false);
                    triggerParticlesLeft.SetActive(false);
                    ResetMaterials(objList);

                    objList = new[] { selToolButton, newSelButton, triggerRight };
                    HighlightMaterials(objList);

                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    referenceManager.graphManager.ResetGraphsColor();
                    break;


                //Heatmap creation and deletion
                case 5:
                    currentStep = 5;
                    ResetMaterials(objList);

                    objList = new[] { selToolButton, createHeatmapButton };
                    HighlightMaterials(objList);

                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    break;

                //Networks creation
                case 6:
                    currentStep = 6;
                    referenceManager.controllerModelSwitcher.TurnOffActiveTool(true);
                    ResetMaterials(objList);
                    ResetMaterials(new GameObject[] { closeMenuButton, laserButton });

                    objList = new[] { selToolButton, createNetworksButton };
                    HighlightMaterials(objList);

                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    highlightSpot.SetActive(false);
                    break;

                //From start to finish
                case 7:
                    currentStep = 7;
                    CellexalEvents.GraphsLoaded.RemoveListener(TurnOnSpot);
                    CellexalEvents.GraphsLoaded.RemoveListener(NextStep);
                    CellexalEvents.SelectionConfirmed.RemoveListener(TurnOnSpot);
                    CellexalEvents.NetworkUnEnlarged.AddListener(TurnOnSpot);
                    heatmapCreated = networksCreated = false;
                    CellexalEvents.HeatmapCreated.AddListener(FinalLevel);
                    CellexalEvents.NetworkCreated.AddListener(FinalLevel);
                    ResetMaterials(objList);

                    referenceManager.loaderController.ResetFolders(true);
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    highlightSpot.SetActive(false);
                    break;

                // Portal back to loading screen
                case 8:
                    currentStep = 8;
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    highlightSpot.SetActive(false);
                    TurnOnSpot();
                    break;
            }
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

        public void NextDescription()
        {
            if (currentStep == 3)
            {
                stepPanels[currentStep - 1].GetComponentInChildren<TextMeshProUGUI>().text = "Step 3 of 7: Coloring by Gene Expression \n \n" +
                    "Write in Gata1 and press Enter. Write using the trigger on the action controller. \n \n " +
                    "Place the graph in the highlighted area to continue to the next step.";
            }
            if (currentStep == 5)
            {
                stepPanels[currentStep - 1].GetComponentInChildren<TextMeshProUGUI>().text = "Step 5 of 7: Heatmap \n \n " +
                    "Now activate the Laser tool found on the menu. Close the selection menu by pressing the red cross at the bottom. \n \n " +
                    "With the action controller point the laser towards the gene list on the right side of the heatmap and press the trigger. \n \n" +
                    "The graph will be coloured according to the gene you selected. \n \n" +
                    "Place the graph in the highlighted area to continue to the next step.";

                closeMenuButton.GetComponent<Renderer>().material = highLightButtonMat;
                laserButton.GetComponent<Renderer>().material = highLightButtonMat;
            }
        }

        void HeatmapCreated()
        {
            createHeatmapButton.GetComponent<Renderer>().material = standardButtonMat;
            heatmapCreated = true;
            if (currentStep == 7)
                FinalLevel();
            if (currentStep == 5)
                NextDescription();
        }

        void NetworksCreated()
        {
            createNetworksButton.GetComponent<Renderer>().material = standardButtonMat;
            networksCreated = true;
            if (currentStep == 7)
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
            triggerParticlesRight.SetActive(false);
        }

        void TurnOnSpot()
        {
            BoxCollider col = highlightSpot.GetComponent<BoxCollider>();
            col.enabled = false;
            if (currentStep != 8 && Physics.OverlapBox(highlightSpot.transform.position + col.center, col.size / 2).Length > 0)
            {
                highlightSpot.transform.position = spotPositions[(currentStep + 1) % spotPositions.Length];
            }
            if (currentStep == 8)
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
            // Menu
            rgripRight = GameObject.Find("[CameraRig]/Controller (right)/Model/rgrip");
            lgripRight = GameObject.Find("[CameraRig]/Controller (right)/Model/lgrip");
            triggerRight = GameObject.Find("[CameraRig]/Controller (right)/Model/trigger");
            trackpadRight = GameObject.Find("[CameraRig]/Controller (right)/Model/trackpad");

            rgripLeft = GameObject.Find("[CameraRig]/Controller (left)/Model/rgrip");
            lgripLeft = GameObject.Find("[CameraRig]/Controller (left)/Model/lgrip");
            triggerLeft = GameObject.Find("[CameraRig]/Controller (left)/Model/trigger");
            trackpadLeft = GameObject.Find("[CameraRig]/Controller (left)/Model/trackpad");
            controllerHints = GameObject.Find("[CameraRig]/Controller (left)/Helper Text");
            controllerHints.SetActive(true);

            // Buttons
            keyboardButton = GameObject.Find("MenuHolder/Main Menu/Left Buttons/Toggle Keyboard Button");
            selToolButton = GameObject.Find("MenuHolder/Main Menu/Right Buttons/Selection Tool Button");
            newSelButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/New Selection Button");
            confirmSelButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/Confirm Selection Button");
            createNetworksButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/Create Networks Button");
            createHeatmapButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/Create Heatmap Button");
            deleteButton = GameObject.Find("MenuHolder/Main Menu/Right Buttons/Delete Tool Button");
            laserButton = GameObject.Find("MenuHolder/Main Menu/Right Buttons/Laser Tool Button");
            closeMenuButton = GameObject.Find("MenuHolder/Main Menu/Selection Tool Menu/Close Button Box/Close Menu Button");

            referencesSet = true;
        }
    }
}