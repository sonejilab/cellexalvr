using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Menu.Buttons.Networks;
using System.Collections.Generic;
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

        [Header("Highlighting objects")]
        public Color highLightStart;
        public Material standardMat;
        public Color highLightEnd;

        private GameObject keyboardButton;
        private GameObject selToolButton;
        private GameObject newSelButton;
        private GameObject confirmSelButton;
        private GameObject networksButton;
        // private GameObject loadMenuButton;
        private GameObject createHeatmapButton;
        private GameObject deleteButton;
        private GameObject controllerHints;

        private int currentStep = 0;
        private SteamVR_Controller.Device device;
        private Transform rgripRight;
        private Transform lgripRight;
        private Transform rgripLeft;
        private Transform lgripLeft;
        private Transform triggerRight;
        private Transform triggerLeft;
        private Transform trackpadLeft;
        private Transform trackpadRight;
        private bool referencesSet;
        private float duration = 1f;

        private GameObject graph;
        private Vector3[] spotPositions = {new Vector3(1.153f, 0.1f, -0f),
                                                new Vector3(-0.331f, 0.1f, 0.84f),
                                                new Vector3(0f, 0.1f, -0.812f)};

        // Object lists to control different states of highlighting
        private List<GameObject> objList = new List<GameObject>();
        private List<GameObject> objList2 = new List<GameObject>();
        private List<GameObject> objList3 = new List<GameObject>();
        //private List<GameObject> steps;

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
            CellexalEvents.HeatmapCreated.AddListener(BurnHeatmap);
            CellexalEvents.HeatmapBurned.AddListener(BurnHeatmap);
            CellexalEvents.HeatmapBurned.AddListener(TurnOnSpot);


            if (referenceManager.rightController.isActiveAndEnabled && referenceManager.leftController.isActiveAndEnabled)
            {
                SetReferences();
            }

            //foreach (Transform child in referenceManager.rightController.transform)
            //{
            //    if (child.name == "rgrip")
            //    {
            //        rgripRight = child.gameObject.GetComponent<Transform>();
            //    }
            //    if (child.name == "lgrip")
            //    {
            //        lgripRight = child.gameObject.GetComponent<Transform>();
            //    }
            //    if (child.name == "trigger")
            //    {
            //        triggerRight = child.gameObject.GetComponent<Transform>();
            //    }
            //    if (child.name == "trackpad")
            //    {
            //        trackpadRight = child.gameObject.GetComponent<Transform>();
            //    }
            //}
            //foreach (Transform child in referenceManager.leftController.transform)
            //{
            //    if (child.name == "rgrip")
            //    {
            //        rgripLeft = child.gameObject.GetComponent<Transform>();
            //    }
            //    if (child.name == "lgrip")
            //    {
            //        lgripLeft = child.gameObject.GetComponent<Transform>();
            //    }
            //    if (child.name == "trigger")
            //    {
            //        triggerLeft = child.gameObject.GetComponent<Transform>();
            //    }
            //    if (child.name == "trackpad")
            //    {
            //        trackpadLeft = child.gameObject.GetComponent<Transform>();
            //    }
            //}
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

            // Highlight the relevant buttons for the specific tutorial step. 
            var lerp = Mathf.PingPong(Time.time, duration) / duration;
            if (rgripRight.GetComponent<MeshRenderer>() != null && rgripLeft.GetComponent<MeshRenderer>() != null && currentStep == 0)
            {
                LoadTutorialStep(1);
            }

            // Highlight grip buttons on step 1 and 2
            if (currentStep == 1 || currentStep == 2)
            {
                Highlight(objList, lerp);
            }
            if (currentStep == 3)
            {
                device = SteamVR_Controller.Input((int)referenceManager.leftController.index);
                if (!referenceManager.mainMenu.GetComponent<MeshRenderer>().enabled)
                {
                    Highlight(objList, lerp);
                    if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        ResetMat(objList);
                        ResetMatColor(objList);
                    }
                    triggerRight.Find("ButtonEmitter").gameObject.SetActive(false);
                    triggerLeft.Find("ButtonEmitter").gameObject.SetActive(true);
                }
                else
                {
                    Highlight(objList2, lerp);
                    if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        ResetMat(objList2);
                        ResetMatColor(objList2);
                    }
                    //triggerRight.Find("ButtonEmitter").gameObject.GetComponent<ParticleSystem>().Stop();
                    triggerLeft.Find("ButtonEmitter").gameObject.SetActive(false);
                    triggerRight.Find("ButtonEmitter").gameObject.SetActive(true);
                }
            }

            if (currentStep == 4)
            {
                Highlight(objList3, lerp);
                if (!referenceManager.mainMenu.GetComponent<MeshRenderer>().enabled)
                {
                    Highlight(objList, lerp);
                    ResetMat(objList2);
                }
                if (referenceManager.mainMenu.GetComponent<MeshRenderer>().enabled)
                {
                    Highlight(objList2, lerp);
                    ResetMat(objList);
                }
            }

            if (currentStep == 5)
            {
                Highlight(objList, lerp);
            }

            if (currentStep == 6)
            {
                if (networksButton.GetComponent<CreateNetworksButton>().buttonActivated)
                {
                    Highlight(objList, lerp);
                }
                else
                {
                    ResetMatColor(objList);
                }

            }
        }

        /// <summary>
        /// Set all references to objects to later be highlighted at different stages in the tutorial. Has to be done after controllers are initialized.
        /// </summary>
        void SetReferences()
        {
            print("Setting references");
            // Menu
            rgripRight = GameObject.Find("[CameraRig]/Controller (right)/Model/rgrip").GetComponent<Transform>();
            lgripRight = GameObject.Find("[CameraRig]/Controller (right)/Model/lgrip").GetComponent<Transform>();
            triggerRight = GameObject.Find("[CameraRig]/Controller (right)/Model/trigger").GetComponent<Transform>();
            trackpadRight = GameObject.Find("[CameraRig]/Controller (right)/Model/trackpad").GetComponent<Transform>();

            rgripLeft = GameObject.Find("[CameraRig]/Controller (left)/Model/rgrip").GetComponent<Transform>();
            lgripLeft = GameObject.Find("[CameraRig]/Controller (left)/Model/lgrip").GetComponent<Transform>();
            triggerLeft = GameObject.Find("[CameraRig]/Controller (left)/Model/trigger").GetComponent<Transform>();
            trackpadLeft = GameObject.Find("[CameraRig]/Controller (left)/Model/trackpad").GetComponent<Transform>();
            controllerHints = GameObject.Find("[CameraRig]/Controller (left)/Helper Text");
            controllerHints.SetActive(true);

            // Buttons
            keyboardButton = GameObject.Find("/Main Menu/Left Buttons/Toggle Keyboard Button");
            selToolButton = GameObject.Find("/Main Menu/Right Buttons/Selection Tool Button");
            newSelButton = GameObject.Find("/Main Menu/Selection Tool Menu/New Selection Button");
            confirmSelButton = GameObject.Find("/Main Menu/Selection Tool Menu/Confirm Selection Button");
            networksButton = GameObject.Find("/Main Menu/Selection Tool Menu/Create Networks Button");
            createHeatmapButton = GameObject.Find("/Main Menu/Selection Tool Menu/Create Heatmap Button");
            deleteButton = GameObject.Find("/Main Menu/Right Buttons/Delete Tool Button");

            referencesSet = true;
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
                    objList.Add(rgripRight.gameObject);
                    objList.Add(rgripLeft.gameObject);
                    objList.Add(lgripLeft.gameObject);
                    objList.Add(lgripRight.gameObject);
                    ResetMat(objList);
                    //foreach (GameObject obj in stepPanels)
                    //{
                    //    obj.SetActive(false);
                    //}
                    stepPanels[currentStep - 1].SetActive(true);
                    break;

                //Moving graphs
                case 2:
                    currentStep = 2;
                    //foreach (GameObject obj in stepPanels)
                    //{
                    //    obj.SetActive(false);
                    //}
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    controllerHints.SetActive(false);
                    break;

                //Keyboard colouring
                case 3:
                    currentStep = 3;
                    ResetMat(objList);
                    objList.Clear();
                    objList.Add(triggerLeft.gameObject);
                    triggerLeft.Find("ButtonEmitter").gameObject.SetActive(true);
                    ResetMat(objList);
                    objList2.Add(triggerRight.gameObject);
                    objList2.Add(trackpadLeft.gameObject);
                    objList2.Add(keyboardButton);
                    //foreach (GameObject obj in stepPanels)
                    //{
                    //    obj.SetActive(false);
                    //}
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    break;


                //Selection tool
                case 4:
                    currentStep = 4;
                    triggerRight.Find("ButtonEmitter").gameObject.SetActive(false);
                    triggerLeft.Find("ButtonEmitter").gameObject.SetActive(false);
                    objList2.Remove(keyboardButton);
                    //keyboardButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                    objList2.Add(selToolButton);
                    objList2.Add(newSelButton);
                    //foreach (GameObject obj in stepPanels)
                    //{
                    //    obj.SetActive(false);
                    //}
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    referenceManager.graphManager.ResetGraphsColor();
                    break;


                //Heatmap creation and deletion
                case 5:
                    currentStep = 5;
                    //ResetMat(objList2);
                    ResetMat(objList);
                    objList.Clear();
                    //objList2.Clear();
                    objList.Add(createHeatmapButton);
                    //foreach (GameObject obj in stepPanels)
                    //{
                    //    obj.SetActive(false);
                    //}
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    break;

                //Networks creation
                case 6:
                    currentStep = 6;
                    objList.Remove(deleteButton);
                    ResetMat(objList2);
                    ResetMat(objList);
                    objList.Clear();
                    objList2.Clear();
                    objList.Add(networksButton);
                    //foreach (GameObject obj in stepPanels)
                    //{
                    //    obj.SetActive(false);
                    //}
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    highlightSpot.SetActive(false);
                    break;

                //From start to finish
                case 7:
                    CellexalEvents.GraphsLoaded.RemoveListener(TurnOnSpot);
                    CellexalEvents.GraphsLoaded.RemoveListener(NextStep);
                    CellexalEvents.SelectionConfirmed.RemoveListener(TurnOnSpot);
                    currentStep = 7;
                    referenceManager.loaderController.ResetFolders(true);
                    //loadMenuButton.GetComponent<ResetFolderButton>().Reset();
                    //foreach (GameObject obj in stepPanels)
                    //{
                    //    obj.SetActive(false);
                    //}
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    highlightSpot.SetActive(false);
                    break;

                // Portal back to loading screen
                case 8:
                    currentStep = 8;
                    //foreach (GameObject obj in stepPanels)
                    //{
                    //    obj.SetActive(false);
                    //}
                    stepPanels[currentStep - 2].SetActive(false);
                    stepPanels[currentStep - 1].SetActive(true);
                    highlightSpot.SetActive(false);
                    TurnOnSpot();
                    break;
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


        void BurnHeatmap()
        {
            //createHeatmapButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
            //objList.Remove(createHeatmapButton);
            objList.Add(deleteButton);
        }

        void SelectionOn()
        {
            objList3.Add(trackpadRight.gameObject);
            //newSelButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
            //selToolButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
            objList2.Remove(newSelButton);
            objList2.Remove(selToolButton);
            ResetMat(objList3);
            //objList3.Add(confirmSelButton.gameObject);
        }

        void SelectionOff()
        {
            //confirmSelButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
            objList3.Remove(confirmSelButton);
            ResetMat(objList3);
            objList3.Clear();
        }

        void TurnOnSpot()
        {
            BoxCollider col = highlightSpot.GetComponent<BoxCollider>();
            col.enabled = false;
            if (currentStep != 8 && Physics.OverlapBox(highlightSpot.transform.position + col.center, col.size / 2).Length > 0)
            {
                highlightSpot.transform.position = spotPositions[(currentStep + 1) % spotPositions.Length];
            }
            highlightSpot.SetActive(true);
            foreach (Transform child in highlightSpot.transform)
            {
                child.GetComponent<ParticleSystem>().Play();
            }
            col.enabled = true;
        }

        void ResetMatColor(List<GameObject> objs)
        {
            foreach (GameObject obj in objs)
            {
                if (obj)
                {
                    obj.GetComponent<Renderer>().material.color = highLightStart;
                }
            }
        }

        void ResetMat(List<GameObject> objs)
        {
            foreach (GameObject obj in objs)
            {
                if (obj)
                {
                    obj.GetComponent<Renderer>().material = standardMat;
                }
            }

        }

        void Highlight(List<GameObject> objs, float lerp)
        {
            foreach (GameObject obj in objs)
            {
                if (obj)
                {
                    obj.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                }
            }
        }
    }
}