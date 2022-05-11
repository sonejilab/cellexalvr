using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using CellexalVR.General;

namespace CellexalVR.DesktopUI
{

    public class MultiUserUIManager : MonoBehaviour
    {
        public GameObject mainMenu;

        private TextField userNameField;
        private TextField roomNameField;
        private TextField passwordField;

        private Label errorDialogField;

        private RadioButton vrToggle;
        private RadioButton ghostToggle;
        private RadioButton desktopToggle;

        private Button backButton;
        private Button createRoomButton;
        private Button joinRoomButton;

        private string errorDialog;
        private float timeToClearDialog;

        private bool connectFailed = false;

        public string ErrorDialog
        {
            get { return this.errorDialog; }
            private set
            {
                this.errorDialog = value;
                errorDialogField.text = value.ToUpper();
                if (!string.IsNullOrEmpty(value))
                {
                    this.timeToClearDialog = Time.time + 4.0f;
                }
            }
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            userNameField = root.Q<TextField>("user-name");
            roomNameField = root.Q<TextField>("room-name");
            passwordField = root.Q<TextField>("password");

            errorDialogField = root.Q<Label>("error-message");

            vrToggle = root.Q<RadioButton>("vr-toggle");
            ghostToggle = root.Q<RadioButton>("ghost-toggle");
            desktopToggle = root.Q<RadioButton>("desktop-toggle");

            backButton = root.Q<Button>("back-button");
            createRoomButton = root.Q<Button>("create-button");
            joinRoomButton = root.Q<Button>("join-button");

            vrToggle.RegisterCallback<MouseUpEvent>(evt => OnToggleMode("vr"));
            ghostToggle.RegisterCallback<MouseUpEvent>(evt => OnToggleMode("ghost"));
            desktopToggle.RegisterCallback<MouseUpEvent>(evt => OnToggleMode("desktop"));

            backButton.RegisterCallback<MouseUpEvent>(evt => OnBackButtonPressed());
            createRoomButton.RegisterCallback<MouseUpEvent>(evt => OnCreateButtonPresed());
            joinRoomButton.RegisterCallback<MouseUpEvent>(evt => OnJoinButtonPressed());

            PhotonNetwork.automaticallySyncScene = true;

            // the following line checks if this client was just created (and not yet online). if so, we connect
            if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
            {
                // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
                PhotonNetwork.ConnectUsingSettings("0.9");
            }

            PhotonNetwork.player.NickName = CellexalUser.Username;

            if (!PhotonNetwork.connected)
            {
                if (PhotonNetwork.connecting)
                {
                    ErrorDialog = "Connecting to: " + PhotonNetwork.ServerAddress;
                }
                else
                {
                    ErrorDialog = "Not connected. Check console output. Detailed connection state: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.ServerAddress;
                }

                if (this.connectFailed)
                {
                    ErrorDialog = "Connection failed. Check setup and use Setup Wizard to fix configuration.";
                    ErrorDialog = $"Server: {new object[] { PhotonNetwork.ServerAddress }}";
                    //GUILayout.Label("AppId: " + PhotonNetwork.PhotonServerSettings.AppID.Substring(0, 8) + "****"); // only show/log first 8 characters. never log the full AppId.

                    //if (GUILayout.Button("Try Again", GUILayout.Width(100)))
                    //{
                    //    this.connectFailed = false;
                    //    PhotonNetwork.ConnectUsingSettings("0.9");
                    //}
                }
                return;
            }
        }


        private void Update()
        {
            if (this.timeToClearDialog < Time.time)
            {
                this.timeToClearDialog = 0;
                ErrorDialog = "";
            }
        }


        private void OnToggleMode(string mode)
        {
            switch (mode)
            {
                case "vr":
                    CrossSceneInformation.Normal = true;
                    CrossSceneInformation.Spectator = false;
                    CrossSceneInformation.Ghost = false;
                    ghostToggle.value = false;
                    desktopToggle.value = false;
                    break;
                case "ghost":
                    CrossSceneInformation.Ghost = true;
                    CrossSceneInformation.Spectator = false;
                    CrossSceneInformation.Normal = false;
                    vrToggle.value = false;
                    desktopToggle.value = false;
                    break;
                case "desktop":
                    CrossSceneInformation.Spectator = true;
                    CrossSceneInformation.Ghost = false;
                    CrossSceneInformation.Normal = false;
                    vrToggle.value = false;
                    ghostToggle.value = false;
                    break;
            }

        }

        private void OnBackButtonPressed()
        {
            gameObject.SetActive(false);
            mainMenu.SetActive(true);
        }

        private void OnCreateButtonPresed()
        {
            if (ValidateFields())
            {
                string roomAndPass = roomNameField.value + passwordField.value;
                PhotonNetwork.CreateRoom(roomAndPass.ToLower(), new RoomOptions() { MaxPlayers = 10 }, null);
            }

        }

        private void OnJoinButtonPressed()
        {
            string roomAndPass = roomNameField.value + passwordField.value;
            if (ValidateFields())
            {
                PhotonNetwork.JoinRoom(roomAndPass.ToLower());
            }

        }

        private bool ValidateFields()
        {
            string password = passwordField.value;
            string roomAndPass = roomNameField.value + passwordField.value;
            if (!vrToggle.value && !desktopToggle.value && !ghostToggle.value)
            {
                ErrorDialog = "Please select a viewing mode.";
                return false;
            }

            if (!(password.Length == 4) || !int.TryParse(password, out int n))
            {
                ErrorDialog = "Password has to be of length 4 and contain only digits.";
                return false;
            }


            return true;

        }

        public void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");

            StartCoroutine(ReferenceManager.instance.multiuserMessageSender.Init());
            gameObject.SetActive(false);
        }

        public void OnPhotonCreateRoomFailed()
        {
            ErrorDialog = "Error: Can't create room (room name maybe already used).";
            Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
        }

        public void OnPhotonJoinRoomFailed(object[] cause)
        {
            ErrorDialog = "Error: Can't join room (room name or password is incorrect or the player name is already taken. Try changing one (or all) of these.). " + cause[1];
            Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
        }

        public void OnPhotonRandomJoinFailed()
        {
            ErrorDialog = "Error: Can't join room (room name or password is incorrect).";
            Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
        }


        public void OnDisconnectedFromPhoton()
        {
            Debug.Log("Disconnected from Photon.");
        }

        public void OnFailedToConnectToPhoton(object parameters)
        {
            //this.connectFailed = true;
            Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
        }

        public void OnConnectedToMaster()
        {
            Debug.Log("As OnConnectedToMaster() got called, the PhotonServerSetting.AutoJoinLobby must be off. Joining lobby by calling PhotonNetwork.JoinLobby().");
            PhotonNetwork.JoinLobby();
        }




    }
}
