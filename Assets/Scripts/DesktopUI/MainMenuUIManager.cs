using CellexalVR.General;
using CellexalVR.Multiuser;
using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CellexalVR.DesktopUI
{
    public class MainMenuUIManager : MonoBehaviour
    {
        public GameObject scarfMenu;
        public GameObject multiUserMenu;

        private TextField usernameInputField;
        private TextField rScriptInputField;
        private TextField scarfScriptInputField;
        private Button preProcessButton;
        private Button singleUserButton;
        private Button multiUserButton;
        private Button tutorialButton;
        private Button findRscriptButton;
        private Button findScarfScriptButton;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            usernameInputField = root.Q<TextField>("user-name");
            rScriptInputField = root.Q<TextField>("r-script-input");
            scarfScriptInputField = root.Q<TextField>("scarf-script-input");
            preProcessButton = root.Q<Button>("pre-process-button");
            singleUserButton = root.Q<Button>("single-user-button");
            multiUserButton = root.Q<Button>("multi-user-button");
            tutorialButton = root.Q<Button>("tutorial-button");
            findRscriptButton = root.Q<Button>("r-script-button");
            findScarfScriptButton = root.Q<Button>("scarf-script-button");

            preProcessButton.RegisterCallback<MouseUpEvent>(evt => OnPreProcessButtonPressed());
            singleUserButton.RegisterCallback<MouseUpEvent>(evt => OnSingleUserButtonPressed());
            multiUserButton.RegisterCallback<MouseUpEvent>(evt => OnMultiUserButtonPressed());
            tutorialButton.RegisterCallback<MouseUpEvent>(evt => OnTutorialButtonPressed());
            findRscriptButton.RegisterCallback<MouseUpEvent>(evt => OnFindRScriptButtonPressed());
            findScarfScriptButton.RegisterCallback<MouseUpEvent>(evt => OnFindScarfScriptButtonPressed());

            CellexalEvents.ConfigLoaded.AddListener(UpdateRScriptField);
            CellexalEvents.ConfigLoaded.AddListener(UpdateScarfScriptField);
        }

        private void UpdateRScriptField()
        { 
            rScriptInputField.value = CellexalConfig.Config.RscriptexePath.Replace('\\', '/');
        }

        private void UpdateScarfScriptField()
        {
            scarfScriptInputField.value = CellexalConfig.Config.ScarfscriptPath.Replace('\\', '/');
        }

        private void OnFindRScriptButtonPressed()
        {
            SetRScriptPath(StandaloneFileBrowser.OpenFilePanel("Select File", "", "exe", false));
        }

        private void OnFindScarfScriptButtonPressed()
        {
            SetScarfScriptPath(StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false));
        }

        private void OnPreProcessButtonPressed()
        {
            gameObject.SetActive(false);
            scarfMenu.SetActive(true);
        }

        private void OnSingleUserButtonPressed()
        {
            SetUsername();
            ReferenceManager.instance.spectatorRig.GetComponent<SpectatorController>().MirrorVRView();
            gameObject.SetActive(false);
        }

        private void OnMultiUserButtonPressed()
        {
            SetUsername();
            gameObject.SetActive(false);
            multiUserMenu.SetActive(true);
        }

        private void OnTutorialButtonPressed()
        {
            CrossSceneInformation.Tutorial = true;
            //RScriptRunner.SetRScriptPath();
            SceneManager.LoadScene("IntroTutorialScene");
            //Launcher.instance.ConnectTutorialScene();
        }

        private void SetUsername()
        {
            CellexalUser.Username = usernameInputField.value;
            PhotonNetwork.playerName = CellexalUser.Username + Random.Range(0, 10000);
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.UserId = PhotonNetwork.playerName;
        }


        private void SetRScriptPath(string[] paths)
        {
            if (paths.Length == 0) return;
            if (paths[0] == null)
            {
                paths[0] = rScriptInputField.text;
            }
            if (paths.Length == 1)
            {
                string path = paths[0];
                string spath = path.Replace('\\', '/');
                if (File.Exists(path) && path.Contains(".exe"))
                {
                    bool hasChanged = path != CellexalConfig.Config.RscriptexePath;
                    CrossSceneInformation.RScriptPath = path;
                    rScriptInputField.value = spath;
                    if (hasChanged)
                    {
                        CellexalConfig.Config.RscriptexePath = spath;
                    }
                    SaveToConfig();
                }
                else
                {
                    rScriptInputField.value = "Failed to find R path";
                }
            }
        }

        private void SetScarfScriptPath(string[] paths)
        {
            if (paths.Length == 0) return;
            if (paths[0] == null)
            {
                paths[0] = rScriptInputField.text;
            }
            if (paths.Length == 1)
            {
                string path = paths[0];
                string spath = (path + "\\run_scarf_server.bat").Replace('\\', '/');
                if (File.Exists(spath))
                {
                    bool hasChanged = path != CellexalConfig.Config.ScarfscriptPath;
                    scarfScriptInputField.value = spath;
                    if (hasChanged)
                    {
                        CellexalConfig.Config.ScarfscriptPath = path;
                    }
                    SaveToConfig();
                }
                else
                {
                    scarfScriptInputField.value = "Failed to find Scarf path";
                }
            }
        }

        private void SaveToConfig()
        {
            string configDir = Directory.GetCurrentDirectory() + @"\Config";
            string configPath = configDir + @"\default_config.xml";
            if (!Directory.Exists("Config"))
            {
                Directory.CreateDirectory("Config");
                CellexalLog.Log("Created directory " + CellexalLog.FixFilePath(configDir));
            }

            CellexalLog.Log("Started saving the config file");

            FileStream fileStream = new FileStream(configPath, FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter streamWriter = new StreamWriter(fileStream);

            XmlSerializer ser = new XmlSerializer(typeof(Config));
            ser.Serialize(streamWriter, CellexalConfig.Config);
            streamWriter.Close();
            fileStream.Close();
        }

    }

}