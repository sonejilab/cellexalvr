﻿using CellexalVR.General;
using CellexalVR.Menu.Buttons.Velocity;
using System.Collections.Generic;
using CellexalVR.Interaction;
using TMPro;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    public class VelocitySubMenu : SubMenu
    {
        public GameObject velocityDataPrefab;
        public TextMeshPro constantSynchedModeText;
        public TextMeshPro frequencyText;
        public TextMeshPro thresholdText;
        public TextMeshPro speedText;
        public TextMeshPro graphPointColorsModeText;
        public TextMeshPro particleMaterialText;
        public List<LoadVelocityButton> buttons = new List<LoadVelocityButton>();
        public SliderController particleSizeSlider;
        public TextMeshPro averageVelocityResolutionText;

        private int buttonNbr;
        Vector3 startPosition = new Vector3(-0.37f, 1f, -0.26f);
        Vector3 nextButtonPosition = new Vector3(-0.37f, 1f, -0.26f);
        Vector3 positionIncCol = new Vector3(0.185f, 0f, 0f);
        Vector3 positionIncRow = new Vector3(0f, 0f, -0.14f);

        protected override void Start()
        {
            CellexalEvents.GraphsLoaded.AddListener(CreateButtons);
            CellexalEvents.GraphsUnloaded.AddListener(DestroyButtons);
            frequencyText.text = "Frequency: 1";
            thresholdText.text = "Threshold: 0";
            speedText.text = "Speed: 8";
            graphPointColorsModeText.text = "Mode: Gradient";
            constantSynchedModeText.text = "Mode: Constant";
            particleMaterialText.text = "Mode: Arrow";
            base.Start();
        }

        /// <summary>
        /// Creates a new button that loads velocity information when clicked.
        /// </summary>
        /// <param name="filePath">The file path to the .mds file that contains the velocity information.</param>
        /// <param name="subGraphName">Optional. If the graph the button is for is a subgraph.</param>
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
            buttonScript.SubGraphName = subGraphName;
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
            }
        }

        /// <summary>
        /// Finds a button representing a graph with velocity information.
        /// </summary>
        /// <param name="filePath">The file path to the file that the graph stemmed from.</param>
        /// <param name="subGraphName">If this is a subgraph: the name of the subgraph. Otherwise, empty string.</param>
        /// <returns></returns>
        public LoadVelocityButton FindButton(string filePath, string subGraphName)
        {
            if (subGraphName != string.Empty)
            {
                return buttons.Find(x => x.SubGraphName == subGraphName);
            }
            else
            {
                return buttons.Find(x => x.graphName == filePath);
            }
        }

        public void DeactivateOutlines()
        {
            foreach (LoadVelocityButton button in buttons)
            {
                button.DeactivateOutline();
            }
        }

        /// <summary>
        /// Activates one outline and deactivates the rest.
        /// </summary>
        /// <param name="filePath">The string to the file that the button represents. See <see cref="LoadVelocityButton.FilePath"/>.</param>
        public void ActivateOutline(string filePath)
        {
            foreach (LoadVelocityButton button in buttons)
            {
                if (button.FilePath == filePath)
                {
                    button.ToggleOutline(true);
                }
                else
                {
                    button.ToggleOutline(false);
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
