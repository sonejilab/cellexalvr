using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {

    public GameObject tutorialCanvas;
    public GameObject[] stepPanels;
    public Material highlightMat;
    public Color highLightStart;
    public Color highLightEnd;
    public GameObject rightControllerModel;
    public GameObject leftControllerModel;
    public GameObject highlightSpot;


    private int currentStep = 0;
    private Transform rgripRight;
    private Transform lgripRight;
    private Transform rgripLeft;
    private Transform lgripLeft;
    private float duration = 1f;
    //private List<GameObject> steps;

    // Use this for initialization
    void Start () {
        //LoadTutorialStep(1);
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

        }
    }
	
	// Update is called once per frame
	void Update () {
        if (GameObject.Find("Graph(Clone)") != null && currentStep == 1)
        {
            LoadTutorialStep(2);
        }
        if (rgripRight.GetComponent<MeshRenderer>().material != null && currentStep == 0)
        {
            LoadTutorialStep(1);
            //Debug.Log(rgrip.GetComponent<MeshRenderer>().material.ToString());
;
        }
        if (currentStep == 1 || currentStep == 2)
        {
            var lerp = Mathf.PingPong(Time.time, duration) / duration;
            rgripRight.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
            lgripRight.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
            rgripLeft.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);
            lgripLeft.GetComponent<Renderer>().material.color = Color.Lerp(highLightStart, highLightEnd, lerp);

        }

    }
    // Load the environment or highlight visuals you want for the specific tutorial step
    public void LoadTutorialStep(int stepNr)
    {
        switch (stepNr)
        {
            case 1:
                foreach(GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[0].SetActive(true);
                
                rgripRight.GetComponent<MeshRenderer>().material = highlightMat;
                lgripRight.GetComponent<MeshRenderer>().material = highlightMat;
                rgripLeft.GetComponent<MeshRenderer>().material = highlightMat;
                lgripLeft.GetComponent<MeshRenderer>().material = highlightMat;
                currentStep = 1;
                break;
   

            case 2:
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[1].SetActive(true);
                currentStep = 2;
                highlightSpot.SetActive(true);

                break;
            case 3:
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                highlightSpot.SetActive(false);
                stepPanels[2].SetActive(true);
                break;

            case 4:
                foreach (GameObject obj in stepPanels)
                {
                    obj.SetActive(false);
                }
                stepPanels[3].SetActive(true);
                break;

        }
    }

    public void NextStep()
    {
        currentStep += 1;
        Debug.Log(currentStep);
        LoadTutorialStep(currentStep);
    }

}
