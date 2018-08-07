using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {

    public GameObject tutorialCanvas;
    public GameObject[] stepPanels;
    public Material highlightMat;
    public Material standardMat;
    public Color highLightStart;
    public Color highLightEnd;
    public GameObject rightControllerModel;
    public GameObject leftControllerModel;
    public GameObject highlightSpot;
    public GameObject highlightSpot2;
    public GameObject highlightSpot3;
    public GameObject highlightSpot4;
    public GameObject portal;
    public GameObject mainMenu;
    public GraphManager graphManager;

    // Buttons are used to determine what buttons to highlight and if a step is completed.
    public GameObject keyboardButton;
    public GameObject selToolButton;
    public GameObject newSelButton;
    public GameObject confirmSelButton;
    public GameObject networksButton;
    public GameObject cellExprButton;
    public GameObject loadMenuButton;

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
    //private List<GameObject> steps;

    // Use this for initialization
    void Start () {
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
	
	// Update is called once per frame
	void Update () {
        // Highlight the relevant buttons for the specific tutorial step.

        // When graph is loaded step 1 is completed.
        if (GameObject.Find("Graph(Clone)") != null && currentStep == 1)
        {
            LoadTutorialStep(2);
        }
        if (rgripRight.GetComponent<MeshRenderer>() != null && currentStep == 0)
        {
            LoadTutorialStep(1);
        }
        // Highlight grip buttons on step 1 and 2
        if (currentStep == 1 || currentStep == 2)
        {
            var lerp = Mathf.PingPong(Time.time, duration) / duration;
            rgripRight.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
            lgripRight.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
            rgripLeft.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
            lgripLeft.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
        }
        if (currentStep == 3)
        {
            var lerp = Mathf.PingPong(Time.time, duration) / duration;
            if (!mainMenu.GetComponent<MeshRenderer>().enabled)
            {
                triggerLeft.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                trackpadLeft.GetComponent<Renderer>().material = standardMat;
                triggerRight.GetComponent<Renderer>().material = standardMat;
            }
            else
            {
                triggerLeft.GetComponent<Renderer>().material = standardMat;
                trackpadLeft.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                triggerRight.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                keyboardButton.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
            }

            if (cellExprButton.GetComponent<RemoveExpressedCellsButton>().buttonActivated)
            {
                highlightSpot2.SetActive(true);
            }
        }

        if (currentStep == 4)
        {
            var lerp = Mathf.PingPong(Time.time, duration) / duration;
            if (!mainMenu.GetComponent<MeshRenderer>().enabled)
            {
                triggerLeft.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                trackpadLeft.GetComponent<Renderer>().material = standardMat;
                triggerRight.GetComponent<Renderer>().material = standardMat;
            }
            else
            {
                triggerLeft.GetComponent<Renderer>().material = standardMat;
                trackpadLeft.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                triggerRight.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                selToolButton.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                newSelButton.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                if (confirmSelButton.GetComponent<ConfirmSelectionButton>().buttonActivated)
                {
                    newSelButton.gameObject.GetComponent<Renderer>().material = standardMat;
                    confirmSelButton.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                    trackpadRight.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
                    trackpadLeft.GetComponent<Renderer>().material = standardMat;
                }
                if (networksButton.GetComponent<CreateNetworksButton>().buttonActivated)
                {
                    //newSelButton.gameObject.GetComponent<Renderer>().material = standardMat;
                    //confirmSelButton.gameObject.GetComponent<Renderer>().material = standardMat;
                    highlightSpot3.SetActive(true);
                }
            }
        }

        if ((currentStep == 5 || currentStep == 6) && GameObject.Find("Enlarged Network") != null)
        {
            highlightSpot4.SetActive(true);
        }

    }
    // Load the environment or highlight visuals you want for the specific tutorial step
    public void LoadTutorialStep(int stepNr)
    {
        Debug.Log("LOAD STEP: " + stepNr);

        // Reset highlighted objects to standard material.
        rgripRight.GetComponent<MeshRenderer>().material = standardMat;
        lgripRight.GetComponent<MeshRenderer>().material = standardMat;
        rgripLeft.GetComponent<MeshRenderer>().material = standardMat;
        lgripLeft.GetComponent<MeshRenderer>().material = standardMat;
        triggerLeft.GetComponent<MeshRenderer>().material = standardMat;
        triggerRight.GetComponent<MeshRenderer>().material = standardMat;
        trackpadLeft.GetComponent<MeshRenderer>().material = standardMat;
        trackpadRight.GetComponent<MeshRenderer>().material = standardMat;

        switch (stepNr)
        {
            case 1:
                currentStep = 1;
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[0].SetActive(true);        
                break;

            case 2:
                currentStep = 2;
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[1].SetActive(true);
                highlightSpot.SetActive(true);
                break;

            case 3:
                currentStep = 3;
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[2].SetActive(true);
                break;

            case 4:
                currentStep = 4;
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[3].SetActive(true);
                graphManager.ResetGraphsColor();
                break;

            case 5:
                currentStep = 5;
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[4].SetActive(true);
                highlightSpot3.SetActive(false);
                break;

            case 6:
                loadMenuButton.GetComponent<ResetFolderButton>().Reset();
                currentStep = 6;
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[5].SetActive(true);
                graphManager.ResetGraphs();
                highlightSpot4.SetActive(false);
                break;

            case 7:
                currentStep = 7;
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[6].SetActive(true);
                
                highlightSpot4.SetActive(false);
                portal.SetActive(true);
                break;
        }
    }

    public void NextStep()
    {
        currentStep += 1;
        LoadTutorialStep(currentStep);
    }

}
