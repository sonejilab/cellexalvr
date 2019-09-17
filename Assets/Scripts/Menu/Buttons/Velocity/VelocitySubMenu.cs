using CellexalVR.General;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    public class VelocitySubMenu : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject velocityDataPrefab;
        public TextMeshPro constantSynchedModeText;
        public TextMeshPro frequencyText;
        public TextMeshPro thresholdText;
        public TextMeshPro speedText;
        public TextMeshPro graphPointColorsModeText;


        private List<LoadVelocityButton> buttons = new List<LoadVelocityButton>();
        private int buttonNbr;
        Vector3 startPosition = new Vector3(-0.37f, 1f, 0.0f);
        Vector3 nextButtonPosition = new Vector3(-0.37f, 1f, 0.0f);
        Vector3 positionIncCol = new Vector3(0.185f, 0f, 0f);
        Vector3 positionIncRow = new Vector3(0f, 0f, -0.2f);

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            CellexalEvents.GraphsLoaded.AddListener(CreateButtons);
            CellexalEvents.GraphsUnloaded.AddListener(DestroyButtons);
            frequencyText.text = "Frequency: ";
            thresholdText.text = "Threshold: ";
            speedText.text = "Speed: ";
            graphPointColorsModeText.text = "Mode: ";
            constantSynchedModeText.text = "Mode: ";
        }

        public void CreateButton(string filePath, string subGraphName = "")
        {
            GameObject newButton = Instantiate(velocityDataPrefab, this.transform);
            newButton.transform.localPosition = nextButtonPosition;

            if (buttonNbr % 5 == 4)
            {
                nextButtonPosition -= 4 * positionIncCol;
                nextButtonPosition += positionIncRow;
            }
            else
            {
                nextButtonPosition += positionIncCol;
            }

            LoadVelocityButton buttonScript = newButton.GetComponent<LoadVelocityButton>();
            buttonScript.subGraphName = subGraphName;
            buttonScript.FilePath = filePath;
            Renderer buttonRenderer = newButton.GetComponent<Renderer>();
            buttonRenderer.material = new Material(buttonRenderer.material);
            buttons.Add(buttonScript);
            newButton.SetActive(true);
            foreach (MeshRenderer rend in GetComponentsInChildren<MeshRenderer>())
            {
                rend.enabled = GetComponent<MeshRenderer>().enabled;
            }
            buttonNbr++;

        }

        private void CreateButtons()
        {
            buttonNbr = 0;
            foreach (string filePath in referenceManager.velocityGenerator.VelocityFiles())
            {
                CreateButton(filePath);
                //GameObject newButton = Instantiate(velocityDataPrefab, this.transform);
                //newButton.transform.localPosition = nextButtonPosition;

                //if (buttonNbr % 5 == 4)
                //{
                //    nextButtonPosition -= 4 * positionIncCol;
                //    nextButtonPosition += positionIncRow;
                //}
                //else
                //{
                //    nextButtonPosition += positionIncCol;
                //}

                //LoadVelocityButton buttonScript = newButton.GetComponent<LoadVelocityButton>();
                //buttonScript.FilePath = filePath;
                //Renderer buttonRenderer = newButton.GetComponent<Renderer>();
                //buttonRenderer.material = new Material(buttonRenderer.material);
                //buttons.Add(buttonScript);
                //newButton.SetActive(true);
                //buttonNbr++;
            }
        }

        public void DeactivateOutlines()
        {
            foreach (LoadVelocityButton button in buttons)
            {
                button.DeactivateOutline();
            }
            frequencyText.text = "Frequency: ";
            thresholdText.text = "Threshold: ";
            speedText.text = "Speed: ";
            graphPointColorsModeText.text = "Mode: ";
            constantSynchedModeText.text = "Mode: ";
        }

        public void ActivateOutline(string filePath)
        {
            foreach (LoadVelocityButton button in buttons)
            {
                if (button.FilePath == filePath)
                {
                    button.activeOutline.SetActive(true);
                }
            }
        }

        private void DestroyButtons()
        {
            foreach (LoadVelocityButton button in buttons)
            {
                Destroy(button.gameObject);
            }
            referenceManager.graphManager.velocityFiles.Clear();
            buttons.Clear();
            nextButtonPosition = startPosition;
        }
    }
}
