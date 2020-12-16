using UnityEngine;
using UnityEngine.SceneManagement;
using CellexalVR.General;
using TMPro;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Tutorial
{

    public class IntroTutorialManager : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject shapeTablePrefab;
        public GameObject touchPadSequence;
        public GameObject canvas;

        private string username;
        private Hand rightHand;
        private Hand leftHand;
        private GameObject canv;
        private bool final;
        private bool keyboard;

        private void Start()
        {
            CellexalEvents.ControllersInitiated.AddListener(Initiate);
        }

        // Update is called once per frame
        private void Update()
        {
            rightHand = Player.instance.rightHand;
            leftHand = Player.instance.leftHand;
            if (rightHand.grabPinchAction.GetStateDown(SteamVR_Input_Sources.Any)) 
            {
                if (referenceManager.keyboardSwitch.isActiveAndEnabled && !keyboard)
                {
                    Destroy(canv);
                    referenceManager.keyboardSwitch.SetKeyboardVisible(true);
                    canv = Instantiate(canvas);
                    canv.GetComponentInChildren<TextMeshProUGUI>().text = "Point the Controller towards keyboard. \n" +
                        "Use the TRIGGER to type.";
                    keyboard = true;
                }
                if (final)
                {
                    SceneManager.LoadScene("CellexalVR_Main_Scene");
                }
                
            }

        }

        public void SetUsername(string name)
        {
            referenceManager.laserPointerController.rightLaser.enabled = false;
            referenceManager.controllerModelSwitcher.SwitchToModel(Interaction.ControllerModelSwitcher.Model.Normal);
            referenceManager.geneKeyboard.gameObject.SetActive(false);
            username = name;
            CrossSceneInformation.Username = name;
            Destroy(canv);
            SpawnShapes();
        }

        void Initiate()
        {
            canv = Instantiate(canvas);
            canv.GetComponentInChildren<TextMeshProUGUI>().text = "Welcome! \n" +
                                                                    "Before we start analyzing data let's get familiar with the controllers. \n" +
                                                                    "We are going to start with the TRIGGER button. It can be found on underside of the controllers. \n" +
                                                                    "Click the TRIGGER to get started.";
        }

        private void SpawnShapes()
        {
            GameObject shapeTable = Instantiate(shapeTablePrefab);
            //shapeTable.AddComponent<ShapeTable>();
            canv = Instantiate(canvas);
            canv.GetComponentInChildren<TextMeshProUGUI>().text = "Well done " + username + "! \n" +
                                                                    "You will now learn to use the GRIP button. \n" +
                                                                    "Squeeze the controller while touching an object to grab it.";
        }

        public void TouchPadLevel()
        {
            Destroy(canv);

            canv = Instantiate(canvas);
            touchPadSequence.SetActive(true);
            canv.GetComponentInChildren<TextMeshProUGUI>().text = "Nice job " + username + "!  \n" +
                                                                    "One last thing before we get into the fun part. \n" +
                                                                    "We are now going to use the TOUCHPAD. \n" +
                                                                    "Press the TOUCHPAD buttons in this order: \n" +
                                                                    "Red             Blue          Green         Red           Yellow";
            
        }

        public void Final()
        {
            Destroy(canv);

            canv = Instantiate(canvas);
            canv.GetComponentInChildren<TextMeshProUGUI>().text = "You are getting the hang of this " + username + "!  \n" +
                                                                    "Now that we are more familiar with the controllers we are going to " +
                                                                    "try some more interesting stuff.\n" +
                                                                    "Press the TRIGGER when you are ready.";
            final = true;
        }
    }
}
