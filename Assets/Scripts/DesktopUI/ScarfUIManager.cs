using UnityEngine;
using UnityEngine.UIElements;
using SFB;
using CellexalVR.Extensions;
using CellexalVR;
using System.IO;
using System;
using System.Globalization;
using CellexalVR.General;
using System.Linq;
using CellexalVR.AnalysisLogic;

namespace CellexalVR.DesktopUI
{

    public class ScarfUIManager : MonoBehaviour
    {
        public GameObject mainMenuUI;
        public static ScarfUIManager instance;

        private Button openFileWindowButton;
        private Button openDirWindowButton;
        private Button convertButton;
        private Button hvgsButton;
        private Button makeGraphButton;
        private Button clusteringButton;
        private Button umapButton;
        private Button backButton;
        private Button loadButton;

        private TextField rawDataTextField;
        private TextField zarrDataTextField;
        private TextField dataLabelTextField;
        private TextField featKeyTextField;
        private TextField topNTextField;
        private TextField nEpochsTextField;
        private TextField resTextField;

        private Label fileMessageLabel;
        private Label convertMessageLabel;
        private Label hvgsMessageLabel;
        private Label graphMessageLabel;
        private Label clusteringMessageLabel;
        private Label umapMessageLabel;
        public Label outputLabel;

        private VisualElement convertRunning;
        private VisualElement hvgsRunning;
        private VisualElement graphRunning;
        private VisualElement umapRunning;
        private VisualElement clusteringRunning;

        private VisualElement convertOuterPivot;
        private VisualElement hvgsOuterPivot;
        private VisualElement graphOuterPivot;
        private VisualElement clusteringOuterPivot;
        private VisualElement umapOuterPivot;

        private VisualElement fileDone;
        private VisualElement dirDone;
        private VisualElement convertDone;
        private VisualElement hvgsDone;
        private VisualElement graphDone;
        private VisualElement umapDone;
        private VisualElement clusteringDone;

        private VisualElement progressBarContainer;
        private VisualElement progressBar;
        private Label progressBarHeader;
        private Label progressBarText;

        private int frameCounter = 0;
        private int currentProgress = 0;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            openFileWindowButton = root.Q<Button>("choose-file-button");
            openDirWindowButton = root.Q<Button>("choose-dir-button");
            convertButton = root.Q<Button>("convert-button");
            hvgsButton = root.Q<Button>("hvgs-button");
            makeGraphButton = root.Q<Button>("graph-button");
            clusteringButton = root.Q<Button>("clustering-button");
            umapButton = root.Q<Button>("umap-button");
            backButton = root.Q<Button>("back-button");
            loadButton = root.Q<Button>("load-button");

            rawDataTextField = root.Q<TextField>("raw-data");
            zarrDataTextField = root.Q<TextField>("zarr-data");
            dataLabelTextField = root.Q<TextField>("data-label");
            topNTextField = root.Q<TextField>("topn-label");
            featKeyTextField = root.Q<TextField>("featkey-label");
            nEpochsTextField = root.Q<TextField>("nepochs-label");
            resTextField = root.Q<TextField>("resolution-label");

            fileMessageLabel = root.Q<Label>("file-message-label");
            convertMessageLabel = root.Q<Label>("convert-message-label");
            hvgsMessageLabel = root.Q<Label>("hvgs-message-label");
            graphMessageLabel = root.Q<Label>("graph-message-label");
            clusteringMessageLabel = root.Q<Label>("clustering-message-label");
            umapMessageLabel = root.Q<Label>("umap-message-label");
            outputLabel = root.Q<Label>("output-label");

            convertRunning = root.Q<VisualElement>("convert-running");
            hvgsRunning = root.Q<VisualElement>("hvgs-running");
            graphRunning = root.Q<VisualElement>("graph-running");
            umapRunning = root.Q<VisualElement>("umap-running");
            clusteringRunning = root.Q<VisualElement>("clustering-running");

            convertOuterPivot = root.Q<VisualElement>("convert-outer-pivot");
            hvgsOuterPivot = root.Q<VisualElement>("hvgs-outer-pivot");
            graphOuterPivot = root.Q<VisualElement>("graph-outer-pivot");
            clusteringOuterPivot = root.Q<VisualElement>("clustering-outer-pivot");
            umapOuterPivot = root.Q<VisualElement>("umap-outer-pivot");

            fileDone = root.Q<VisualElement>("file-done");
            dirDone = root.Q<VisualElement>("dir-done");
            convertDone = root.Q<VisualElement>("convert-done");
            hvgsDone = root.Q<VisualElement>("hvgs-done");
            graphDone = root.Q<VisualElement>("graph-done");
            umapDone = root.Q<VisualElement>("umap-done");
            clusteringDone = root.Q<VisualElement>("clustering-done");

            progressBarContainer = root.Q<VisualElement>("progress-container");
            progressBar = root.Q<VisualElement>("bar-progress");
            progressBarHeader = root.Q<Label>("text-header");
            progressBarText = root.Q<Label>("text-percentage");

            openFileWindowButton.RegisterCallback<MouseUpEvent>(evt => OnOpenFileWindowButtonPressed());
            openDirWindowButton.RegisterCallback<MouseUpEvent>(evt => OnOpenDirWindowButtonPressed());
            convertButton.RegisterCallback<MouseUpEvent>(evt => OnConvertButtonPressed());
            hvgsButton.RegisterCallback<MouseUpEvent>(evt => OnHVGSButtonPressed());
            makeGraphButton.RegisterCallback<MouseUpEvent>(evt => OnMakeGraphButtonPressed());
            clusteringButton.RegisterCallback<MouseUpEvent>(evt => OnClusteringButtonPressed());
            umapButton.RegisterCallback<MouseUpEvent>(evt => OnUMAPButtonPressed());
            backButton.RegisterCallback<MouseUpEvent>(evt => OnBackButtonPressed());
            loadButton.RegisterCallback<MouseUpEvent>(evt => OnLoadButtonPressed());

            CellexalEvents.ScarfUMAPFinished.AddListener(() => loadButton.style.display = DisplayStyle.Flex);

            ScarfManager.instance.InitServer();
        }


        private void Start()
        {
            instance = this;

            CellexalEvents.ZarrConversionComplete.AddListener(OnConversionComplete);
            CellexalEvents.DataStaged.AddListener(OnDataStaged);
        }

        private void Update()
        {
            frameCounter++;
            if (frameCounter > 20)
            {
                frameCounter = 0;
                string text = "";
                string line = "";
                for (int i = 0; i < ScarfManager.instance.consoleLines.Length; i++)
                {
                    line = ScarfManager.instance.consoleLines[i];
                    text += line;
                    if (line.Contains("ANN")) return;
                }
                outputLabel.text = text;
                int index = text.LastIndexOf('%'); // if message contains a percentage pass to progress bar.
                if (index > -1)
                {
                    string value = text.Substring(index - 3, 3);
                    if (value[0].Equals('0'))
                    {
                        value = value.Substring(1, value.Length - 1);
                    }
                    int.TryParse(value, out currentProgress);
                }
                else if (line.Contains("epochs")) // special case for umap messages
                {
                    // get and ignore first line numbers
                    index = line.LastIndexOf(']');
                    line = line.Substring(index, line.Length - index);
                    string[] parts = line.Split('/');
                    // get numbers from line
                    string part1Digits = new string(parts[0].Where(c => char.IsDigit(c)).ToArray());
                    string part2Digits = new string(parts[1].Where(c => char.IsDigit(c)).ToArray());
                    currentProgress = (int)((float)int.Parse(part1Digits) / (float)int.Parse(part2Digits) * 100);
                }
                AnimateProgressBar(currentProgress);
            }

        }

        private void OnOpenFileWindowButtonPressed()
        {
            var path = StandaloneFileBrowser.OpenFilePanel("Select raw data file", "", "h5", false);
            if (path.Length > 0)
            {
                rawDataTextField.value = path[0].Replace('\\', '/');
            }
        }

        private void OnOpenDirWindowButtonPressed()
        {
            var path = StandaloneFileBrowser.OpenFolderPanel("Select zarr directory", "", false);
            if (path.Length > 0)
            {
                zarrDataTextField.value = path[0].Replace('\\', '/');
            }
            string[] parts = path[0].Split('\\');
            string lastPart = parts[parts.Length - 1];
            dataLabelTextField.value = lastPart;
            fileDone.RemoveFromClassList("inactive");
            dirDone.RemoveFromClassList("inactive");
            openFileWindowButton.AddToClassList("inactive");
            openDirWindowButton.AddToClassList("inactive");
            convertButton.AddToClassList("inactive");

            // if running locally check if path exists.
            //if (Directory.Exists($"{zarrDataTextField.value}/cellData"))
            //{
            //}
            //else
            //{
            //    print("dir not valid paths " + path[0]);
            //    // directory not valid.
            //    return;
            //}
            StartCoroutine(ScarfManager.instance.StageDataCoroutine(dataLabelTextField.value));
        }


        private void OnConvertButtonPressed()
        {
            progressBarContainer.RemoveFromClassList("inactive");
            //if (!File.Exists($"{rawDataTextField.value.FixFilePath()}"))
            //{
            //    fileMessageLabel.text = "Could not find file...";
            //    fileMessageLabel.style.display = DisplayStyle.Flex;
            //    return;
            //}
            openFileWindowButton.AddToClassList("inactive");
            fileMessageLabel.style.display = DisplayStyle.None;

            if (dataLabelTextField.value.Length == 0)
            {
                convertMessageLabel.style.display = DisplayStyle.Flex;
                return;
            }
            convertMessageLabel.style.display = DisplayStyle.None;

            fileDone.RemoveFromClassList("inactive");
            convertButton.AddToClassList("inactive");
            AnimateUIRotation(convertOuterPivot);

            string[] parts = rawDataTextField.value.Split('/');
            string lastPart = parts[parts.Length - 1];

            convertRunning.RemoveFromClassList("inactive");
            StartCoroutine(ScarfManager.instance.ConvertToZarrCoroutine(dataLabelTextField.value, lastPart));
        }

        private void OnConversionComplete()
        {
            convertRunning.AddToClassList("inactive");
            convertDone.RemoveFromClassList("inactive");
            ToggleProgressBar(false);
        }

        private void OnDataStaged()
        {

        }

        private void AnimateUIRotation(VisualElement el)
        {
            el.schedule.Execute(() =>
            {
                var r = el.worldTransform.rotation.eulerAngles;
                r.z += 2f;
                el.transform.rotation = Quaternion.Euler(r);
            }).Every(10);
        }

        private void AnimateProgressBar(int value)
        {
            progressBarText.text = value + "%";
            var w = Mathf.Clamp(value * 3.5f, 0, 350);
            progressBar.style.width = w;
        }

        public void ToggleProgressBar(bool toggle)
        {
            if (!toggle)
            {
                progressBarContainer.AddToClassList("inactive");
                progressBar.style.width = 0;
            }

        }

        private void OnHVGSButtonPressed()
        {
            if (!int.TryParse(topNTextField.value, out int result))
            {
                hvgsMessageLabel.style.display = DisplayStyle.Flex;
                return;
            }
            progressBarContainer.RemoveFromClassList("inactive");
            hvgsMessageLabel.style.display = DisplayStyle.None;

            hvgsButton.AddToClassList("inactive");
            AnimateUIRotation(hvgsOuterPivot);
            StartCoroutine(ScarfManager.instance.MarkHVGSCoroutine(topNTextField.value, hvgsRunning, hvgsDone));
        }

        private void OnMakeGraphButtonPressed()
        {
            if (featKeyTextField.value.Length == 0)
            {
                graphMessageLabel.style.display = DisplayStyle.Flex;
                return;
            }
            graphMessageLabel.style.display = DisplayStyle.None;
            progressBarContainer.RemoveFromClassList("inactive");
            makeGraphButton.AddToClassList("inactive");
            AnimateUIRotation(graphOuterPivot);
            StartCoroutine(ScarfManager.instance.MakeGraphCoroutine(featKeyTextField.value, graphRunning, graphDone));
        }

        private void OnClusteringButtonPressed()
        {
            if (!float.TryParse("1.0", NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float res))
            {
                print("could not parse 1.0");
            }

            if (!float.TryParse(resTextField.value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float result))
            {
                clusteringMessageLabel.style.display = DisplayStyle.Flex;
                return;
            }
            clusteringMessageLabel.style.display = DisplayStyle.None;
            progressBarContainer.RemoveFromClassList("inactive");
            clusteringButton.AddToClassList("inactive");
            AnimateUIRotation(clusteringOuterPivot);
            StartCoroutine(ScarfManager.instance.RunClusteringCoroutine(resTextField.value, clusteringRunning, clusteringDone));
        }

        private void OnUMAPButtonPressed()
        {
            if (!int.TryParse(nEpochsTextField.value, out int result))
            {
                umapMessageLabel.style.display = DisplayStyle.Flex;
                return;
            }
            umapMessageLabel.style.display = DisplayStyle.None;
            progressBarContainer.RemoveFromClassList("inactive");
            umapButton.AddToClassList("inactive");
            AnimateUIRotation(umapOuterPivot);
            StartCoroutine(ScarfManager.instance.RunUMAPCoroutine(nEpochsTextField.value, umapRunning, umapDone));
        }

        private void OnBackButtonPressed()
        {
            ScarfManager.instance.CloseServer();
            gameObject.SetActive(false);
            mainMenuUI.SetActive(true);
        }

        private void OnLoadButtonPressed()
        {
            gameObject.SetActive(false);
            mainMenuUI.SetActive(true);
            ScarfManager.instance.LoadData(dataLabelTextField.value, "RNA_UMAP");
        }
    }
}
