// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkerMenu.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using Random = UnityEngine.Random;
using ExitGames.Client.Photon;
using CellexalVR.General;

namespace CellexalVR.Multiuser
{

    public class MultiuserMenu : MonoBehaviour
    {
        public GUISkin Skin;
        public GameObject mainMenu;
        public Vector2 WidthAndHeight = new Vector2(800, 600);
        private string roomName = "myLab";
        private string password = "";
        private string roomAndPass = "";
        private int selGridInt = 0;
        private Matrix4x4 matrix;
        private float buttonWidth;
        private float sizeMultiplier = 1;
        private int fontsize = 14;
        private GUIStyle guiStyle = new GUIStyle();

        private Vector2 scrollPos = Vector2.zero;

        private bool connectFailed = false;

        public static readonly string SceneNameMenu = "Launcher";

        public static readonly string SceneNameGame = "CellexalVR_Main_Scene";

        public static readonly string SpectatorScene = "spectator_scene";

        private string errorDialog;
        private double timeToClearDialog;

        public string ErrorDialog
        {
            get { return this.errorDialog; }
            private set
            {
                this.errorDialog = value;
                if (!string.IsNullOrEmpty(value))
                {
                    this.timeToClearDialog = Time.time + 4.0f;
                }
            }
        }

        public void Awake()
        {
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;

            // the following line checks if this client was just created (and not yet online). if so, we connect
            if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
            {
                // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
                PhotonNetwork.ConnectUsingSettings("0.9");
            }

            // generate a name for this player, if none is assigned yet
            if (Screen.width > 1920)
            {
                WidthAndHeight = new Vector2(1280, 1024);
            }


            // if you wanted more debug out, turn this on:
            // PhotonNetwork.logLevel = NetworkLogLevel.Full;
            if (Screen.width > 1920)
            {
                WidthAndHeight = new Vector2(1280, 1024);
            }
        }

        private void Start()
        {
            if (String.IsNullOrEmpty(PhotonNetwork.playerName))
            {
                PhotonNetwork.player.NickName = CellexalUser.Username;
                //PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
                PhotonNetwork.playerName = CellexalUser.Username + Random.Range(1,9999);
            }

        }


        public void OnGUI()
        {
            if (this.Skin != null)
            {
                GUI.skin = this.Skin;
            }

            if (!PhotonNetwork.connected)
            {
                if (PhotonNetwork.connecting)
                {
                    GUILayout.Label("Connecting to: " + PhotonNetwork.ServerAddress);
                }
                else
                {
                    GUILayout.Label("Not connected. Check console output. Detailed connection state: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.ServerAddress);
                }

                if (this.connectFailed)
                {
                    GUILayout.Label("Connection failed. Check setup and use Setup Wizard to fix configuration.");
                    GUILayout.Label(String.Format("Server: {0}", new object[] { PhotonNetwork.ServerAddress }));
                    GUILayout.Label("AppId: " + PhotonNetwork.PhotonServerSettings.AppID.Substring(0, 8) + "****"); // only show/log first 8 characters. never log the full AppId.

                    if (GUILayout.Button("Try Again", GUILayout.Width(100)))
                    {
                        this.connectFailed = false;
                        PhotonNetwork.ConnectUsingSettings("0.9");
                    }
                }

                return;
            }

            Rect content = new Rect((Screen.width - this.WidthAndHeight.x) / 2, (Screen.height - this.WidthAndHeight.y) / 2, this.WidthAndHeight.x, this.WidthAndHeight.y);
            GUI.Box(content, "Join or Create Room");
            GUILayout.BeginArea(content);


            GUILayout.Space(80);

            // Player name
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Name:");
            PhotonNetwork.player.NickName = GUILayout.TextField(PhotonNetwork.player.NickName, GUILayout.Width(200));
            GUILayout.Space(400);

            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Room Name:");
            this.roomName = GUILayout.TextField(this.roomName, GUILayout.Width(200));
            GUILayout.Space(400);

            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Password(4 digit code): ");
            this.password = GUILayout.PasswordField(this.password, "*"[0], 4, GUILayout.MaxWidth(100));
            GUILayout.Space(400);

            this.roomAndPass = this.roomName + this.password;

            GUILayout.EndHorizontal();
            GUILayout.Space(30);
            GUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //this.roomName = GUILayout.TextField(this.roomName);
            // Join room by title
            // Create a room (fails if exist or if password does not comply with rules!)
            if (GUILayout.Button("Create Room"))
            {
                int n;
                if (this.password.Length == 4 && int.TryParse(this.password, out n))
                {
                    //print("Create room: " + this.roomAndPass);
                    PhotonNetwork.CreateRoom(this.roomAndPass.ToLower(), new RoomOptions() { MaxPlayers = 10 }, null);
                }
                if (!(this.password.Length == 4) || !int.TryParse(this.password, out n))
                {
                    ErrorDialog = "Password has to be of length 4 and contain only digits.";

                }
            }
            if (GUILayout.Button("Join Room"))
            {
                PhotonNetwork.JoinRoom(this.roomAndPass.ToLower());
            }


            string[] selStrings = { "Normal VR View", "VR Spectator View", "Desktop Spectator View \n Non-VR" };

            selGridInt = GUILayout.SelectionGrid(selGridInt, selStrings, 1);
            switch (selGridInt)
            {
                case 0:
                    CrossSceneInformation.Normal = true;
                    CrossSceneInformation.Spectator = false;
                    CrossSceneInformation.Ghost = false;
                    break;
                case 1:
                    CrossSceneInformation.Ghost = true;
                    CrossSceneInformation.Spectator = false;
                    CrossSceneInformation.Normal = false;
                    break;
                case 2:
                    CrossSceneInformation.Spectator = true;
                    CrossSceneInformation.Ghost = false;
                    CrossSceneInformation.Normal = false;
                    break;
            }


            //CrossSceneInformation.Spectator = GUILayout.Toggle(CrossSceneInformation.Spectator, "Desktop Spectator Mode \n (Non-VR)");

            //GUILayout.EndHorizontal();
            //GUILayout.Space(10);
            //GUILayout.BeginHorizontal();
            //GUILayout.Space(420);

            //CrossSceneInformation.Ghost = GUILayout.Toggle(CrossSceneInformation.Ghost, "VR Spectator Mode");

            //if (CrossSceneInformation.Ghost && GUI.changed)
            //{
            //    CrossSceneInformation.Spectator = false;
            //}

            //else if (CrossSceneInformation.Spectator && GUI.changed)
            //{
            //    CrossSceneInformation.Ghost = false;
            //}



            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(ErrorDialog))
            {
                GUILayout.Label(ErrorDialog);

                if (this.timeToClearDialog < Time.time)
                {
                    this.timeToClearDialog = 0;
                    ErrorDialog = "";
                }
            }

            //GUILayout.Space(20);

            if (GUILayout.Button("Go Back", GUILayout.MinHeight(50)))
            {
                this.gameObject.SetActive(false);
                mainMenu.SetActive(true);
            }

            // Join random room
            //GUILayout.BeginHorizontal();

            //GUILayout.Label(PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.");
            //GUILayout.FlexibleSpace();
            //if (GUILayout.Button("Join Random", GUILayout.Width(150)))
            //{
            //    PhotonNetwork.JoinRandomRoom();
            //}


            //GUILayout.EndHorizontal();

            //GUILayout.Space(15);
            //if (PhotonNetwork.GetRoomList().Length == 0)
            //{
            //    GUILayout.Label("Currently no games are available.");
            //    GUILayout.Label("Rooms will be listed here, when they become available.");
            //}
            //else
            //{
            //    GUILayout.Label(PhotonNetwork.GetRoomList().Length + " rooms available:");

            //    // Room listing: simply call GetRoomList: no need to fetch/poll whatever!
            //    this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            //    foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
            //    {
            //        GUILayout.BeginHorizontal();
            //        GUILayout.Label(roomInfo.Name + " " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers);
            //        if (GUILayout.Button("Join", GUILayout.Width(150)))
            //        {
            //            PhotonNetwork.JoinRoom(roomInfo.Name);
            //        }

            //        GUILayout.EndHorizontal();
            //    }

            //    GUILayout.EndScrollView();
            //}

            GUILayout.EndArea();
        }

        // We have two options here: we either joined(by title, list or random) or created a room.
        public void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
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

        public void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom");
            PhotonNetwork.LoadLevel(SceneNameGame);
        }

        public void OnDisconnectedFromPhoton()
        {
            Debug.Log("Disconnected from Photon.");
        }

        public void OnFailedToConnectToPhoton(object parameters)
        {
            this.connectFailed = true;
            Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
        }

        public void OnConnectedToMaster()
        {
            Debug.Log("As OnConnectedToMaster() got called, the PhotonServerSetting.AutoJoinLobby must be off. Joining lobby by calling PhotonNetwork.JoinLobby().");
            PhotonNetwork.JoinLobby();
        }
    }
}
