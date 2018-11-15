using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class takes care of the highlighting and loading of different steps in the tutorial. 
/// It listens to events to know what buttons to highlight at what time and when to show the description for the next step.
/// </summary>
public class TutorialManager : MonoBehaviour {

    public GameObject tutorialCanvas;
    public GameObject[] stepPanels;
    public ReferenceManager referenceManager;

    public GameObject rightControllerModel;
    public GameObject leftControllerModel;
    public List<GameObject> highlightSpots;
    public GameObject mainMenu;
    public GraphManager graphManager;

    [Header("Highlighting objects")]
    public Color highLightStart;
    public Material standardMat;
    public Color highLightEnd;
    // Buttons are used to determine what buttons to highlight and if a step is completed.
    public GameObject keyboardButton;
    public GameObject selToolButton;
    public GameObject newSelButton;
    public GameObject confirmSelButton;
    public GameObject networksButton;
    //public GameObject loadMenuButton;
    public GameObject createHeatmapButton;
    public GameObject burnHeatmapButton;

    private int currentStep = 0;
    private Transform rgripRight;
    private Transform lgripRight;
    private Transform rgripLeft;
    private Transform lgripLeft;
    private Transform triggerRight;
    private Transform triggerLeft;
    private Transform trackpadLeft;
    private Transform trackpadRight;
    private float duration = 1f;

    private GameObject graph;

    // Object lists to control different states of highlighting
    private List<GameObject> objList = new List<GameObject>();
    private List<GameObject> objList2 = new List<GameObject>();
    private List<GameObject> objList3 = new List<GameObject>();
    //private List<GameObject> steps;

    // Use this for initialization
    void Start () {
        CellexalEvents.GraphsLoaded.AddListener(NextStep);
        CellexalEvents.GraphsLoaded.AddListener(TurnOnSpot);
        CellexalEvents.GraphsColoredByGene.AddListener(TurnOnSpot);
        CellexalEvents.SelectionStarted.AddListener(SelectionOn);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOnSpot);
        CellexalEvents.SelectionConfirmed.AddListener(SelectionOff);
        CellexalEvents.NetworkEnlarged.AddListener(TurnOnSpot);
        CellexalEvents.HeatmapCreated.AddListener(BurnHeatmap);
        CellexalEvents.HeatmapBurned.AddListener(BurnHeatmap);
        CellexalEvents.HeatmapBurned.AddListener(TurnOnSpot);


        foreach (Transform child in rightControllerModel.transform)
        {
            if (child.name == "rgrip")
            {
                rgripRight = child.gameObject.GetComponent<Transform>();
            }
            if (child.name == "lgrip")
            {
                lgripRight = child.gameObject.GetComponent<Transform>();
            }
            if (child.name == "trigger")
            {
                triggerRight = child.gameObject.GetComponent<Transform>();
            }
            if (child.name == "trackpad")
            {
                trackpadRight = child.gameObject.GetComponent<Transform>();
            }
        }
        foreach (Transform child in leftControllerModel.transform)
        {
            if (child.name == "rgrip")
            {
                rgripLeft = child.gameObject.GetComponent<Transform>();
            }
            if (child.name == "lgrip")
            {
                lgripLeft = child.gameObject.GetComponent<Transform>();
            }
            if (child.name == "trigger")
            {
                triggerLeft = child.gameObject.GetComponent<Transform>();
            }
            if (child.name == "trackpad")
            {
                trackpadLeft = child.gameObject.GetComponent<Transform>();
            }
        }
    }

    void BurnHeatmap()
    {
        createHeatmapButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
        objList.Remove(createHeatmapButton);
        objList.Add(burnHeatmapButton);
    }

    void SelectionOn()
    {
        objList3.Add(trackpadRight.gameObject);
        newSelButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
        selToolButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
        objList2.Remove(newSelButton);
        objList2.Remove(selToolButton);
        ResetMat(objList3);
        objList3.Add(confirmSelButton.gameObject);
    }
    
    void SelectionOff()
    {
        confirmSelButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
        objList3.Remove(confirmSelButton);
        ResetMat(objList3);
        objList3.Clear();
    }
	
    void TurnOnSpot()
    {
        BoxCollider col = highlightSpots[currentStep - 2].GetComponent<BoxCollider>();
        if (currentStep != 8 && Physics.OverlapBox(highlightSpots[currentStep - 2].transform.position + col.center, col.size / 2).Length > 0)
        {
            highlightSpots[currentStep - 2].transform.position = highlightSpots[currentStep - 1].transform.position;
        }
        highlightSpots[currentStep-2].SetActive(true);
        foreach (Transform child in highlightSpots[currentStep-2].transform)
        {
            child.GetComponent<ParticleSystem>().Play();
        }
        highlightSpots[currentStep - 2].GetComponent<Collider>().enabled = true;
    }

    void ResetMatColor(List<GameObject> objs)
    {
        foreach (GameObject obj in objs)
        {
            obj.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
        }
        
    }

    void ResetMat(List<GameObject> objs)
    {
        foreach (GameObject obj in objs)
        {
            obj.GetComponent<Renderer>().material = standardMat;
        }

    }

    void Highlight(List<GameObject> objs, float lerp)
    {
        foreach (GameObject obj in objs)
        {
            obj.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
        }

    }

    // Update is called once per frame
    void Update () {
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
            
            if (!mainMenu.GetComponent<MeshRenderer>().enabled)
            {
                Highlight(objList, lerp);
                ResetMat(objList2);
                triggerRight.Find("ButtonEmitter").gameObject.SetActive(false);
            }
            else
            {
                Highlight(objList2, lerp);
                ResetMat(objList);
                //triggerRight.Find("ButtonEmitter").gameObject.GetComponent<ParticleSystem>().Stop();
                triggerLeft.Find("ButtonEmitter").gameObject.SetActive(false);
                triggerRight.Find("ButtonEmitter").gameObject.SetActive(true);
            }
        }

        if (currentStep == 4)
        {
            Highlight(objList3, lerp);
            if (!mainMenu.GetComponent<MeshRenderer>().enabled)
            {
                Highlight(objList, lerp);
                ResetMat(objList2);
            }
            if (mainMenu.GetComponent<MeshRenderer>().enabled)
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

    // When changing step, change canvas with description aswell as removing/adding buttons to be highlighted.
    public void LoadTutorialStep(int stepNr)
    {
        Debug.Log("LOAD STEP: " + stepNr);

        switch (stepNr)
        {
            case 1:
                //Loading
                currentStep = 1;
                objList.Add(rgripRight.gameObject);
                objList.Add(rgripLeft.gameObject);
                objList.Add(lgripLeft.gameObject);
                objList.Add(lgripRight.gameObject);
                ResetMat(objList);
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[currentStep - 1].SetActive(true);
                break;

            case 2:
                //Moving
                currentStep = 2;
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[currentStep - 1].SetActive(true);
                break;

            case 3:
                //Keyboard
                currentStep = 3;
                ResetMat(objList);
                objList.Clear();
                objList.Add(triggerLeft.gameObject);
                //triggerRight.Find("ButtonEmitter").gameObject.GetComponent<ParticleSystem>().Play();
                triggerLeft.Find("ButtonEmitter").gameObject.SetActive(true);
                ResetMat(objList);
                objList2.Add(triggerRight.gameObject);
                objList2.Add(trackpadLeft.gameObject);
                objList2.Add(keyboardButton);
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[currentStep - 1].SetActive(true);
                break;


            case 4:
                //Selection
                currentStep = 4;
                triggerRight.Find("ButtonEmitter").gameObject.SetActive(false);
                triggerLeft.Find("ButtonEmitter").gameObject.SetActive(false);
                objList2.Remove(keyboardButton);
                keyboardButton.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                objList2.Add(selToolButton);
                objList2.Add(newSelButton);
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[currentStep-1].SetActive(true);
                graphManager.ResetGraphsColor();
                break;


            case 5:
                //Heatmap
                currentStep = 5;
                //ResetMat(objList2);
                ResetMat(objList);
                objList.Clear();
                //objList2.Clear();
                objList.Add(createHeatmapButton);
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[currentStep - 1].SetActive(true);
                break;

            case 6:
                //Networks
                currentStep = 6;
                objList.Remove(burnHeatmapButton);
                ResetMat(objList2);
                ResetMat(objList);
                objList.Clear();
                objList2.Clear();
                objList.Add(networksButton);
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[currentStep - 1].SetActive(true);
                highlightSpots[2].SetActive(false);
                break;

            case 7:
                //From start
                currentStep = 7;
                referenceManager.loaderController.ResetFolders();
                //loadMenuButton.GetComponent<ResetFolderButton>().Reset();
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[currentStep - 1].SetActive(true);
                highlightSpots[3].SetActive(false);
                CellexalEvents.GraphsLoaded.RemoveListener(TurnOnSpot);
                CellexalEvents.GraphsLoaded.RemoveListener(NextStep);
                CellexalEvents.SelectionConfirmed.RemoveListener(TurnOnSpot);
                break;

            case 8:
                //Final
                currentStep = 8;
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[currentStep - 1].SetActive(true);
                highlightSpots[4].SetActive(false);
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

}
