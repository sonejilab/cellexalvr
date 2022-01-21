using CellexalVR.General;
using CellexalVR.Multiuser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CellexalVR.DesktopUI
{
    public class MainMenuUIManager : MonoBehaviour
    {
        public GameObject scarfMenu;
        public GameObject multiUserMenu;

        private TextField usernameInputField;
        private Button preProcessButton;
        private Button singleUserButton;
        private Button multiUserButton;
        private Button tutorialButton;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            usernameInputField = root.Q<TextField>("user-name");
            preProcessButton = root.Q<Button>("pre-process-button");
            singleUserButton = root.Q<Button>("single-user-button");
            multiUserButton = root.Q<Button>("multi-user-button");
            tutorialButton = root.Q<Button>("tutorial-button");

            preProcessButton.RegisterCallback<MouseUpEvent>(evt => OnPreProcessButtonPressed());
            singleUserButton.RegisterCallback<MouseUpEvent>(evt => OnSingleUserButtonPressed());
            multiUserButton.RegisterCallback<MouseUpEvent>(evt => OnMultiUserButtonPressed());
            tutorialButton.RegisterCallback<MouseUpEvent>(evt => OnTutorialButtonPressed());

        }

        private void OnPreProcessButtonPressed()
        {
            gameObject.SetActive(false);
            scarfMenu.SetActive(true);
        }

        private void OnSingleUserButtonPressed()
        {
            SetUsername();
            ReferenceManager.instance.spectatorRig.GetComponent<SpectatorController>().ToggleSpectator(true);
            gameObject.SetActive(false);
            //Launcher.instance.ConnectSinglePlayer();
        }

        private void OnMultiUserButtonPressed()
        {
            SetUsername();
            gameObject.SetActive(false);
            multiUserMenu.SetActive(true);
        }

        private void OnTutorialButtonPressed()
        {
            Launcher.instance.ConnectTutorialScene();
        }

        private void SetUsername()
        {
            CellexalUser.Username = usernameInputField.value;
            PhotonNetwork.playerName = CellexalUser.Username + Random.Range(0, 10000);
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.UserId = PhotonNetwork.playerName;
        }

    }

}