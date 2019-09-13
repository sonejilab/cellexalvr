using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Menu.Buttons.Networks;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    /// <summary>
    /// This class represents the sub menu that pops up when the ToggleArcsButton is pressed.
    /// </summary>
    public class ToggleArcsSubMenu : MenuWithTabs
    {
        public GameObject buttonPrefab;
        // hard coded positions :)
        private Vector3 buttonPos = new Vector3(-0.38f, 0.59f, 0.22f);
        private Vector3 buttonPosInc = new Vector3(0.25f, 0, 0);
        private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -0.15f);

        private Color[] colors;

        /// <summary>
        /// Initializes the arc menu.
        /// </summary>
        public void Init()
        {
            colors = CellexalConfig.Config.SelectionToolColors;
        }

        /// <summary>
        /// Creates new buttons for toggling arcs.
        /// </summary>
        /// <param name="networks"> An array containing the networks. </param>
        public void CreateToggleArcsButtons(NetworkCenter[] networks)
        {
            if (networks.Length < 1)
            {
                return;
            }
            TurnOffAllTabs();
            var newTab = AddTab(tabPrefab);
            // The prefab contains some buttons that needs some variables set.
            ArcsTabButton tabButton = newTab.GetComponentInChildren<ArcsTabButton>();
            tabButton.referenceManager = referenceManager;
            tabButton.tab = newTab;
            tabButton.Handler = networks[0].Handler;
            string tabName = networks[0].Handler.gameObject.name.Split('_')[1];
            newTab.gameObject.name = "Tab_" + tabName;
            newTab.TabName.text = tabName;
            print(tabName);
            print(newTab.TabName);
            //newTab.tab = newTab.transform.parent.gameObject;
            if (colors == null)
            {
                Init();
            }
            //foreach (GameObject button in buttons)
            //{
            //    // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            //    Destroy(button.gameObject, .1f);
            //    buttonPos = new Vector3(-.39f, .77f, .282f);
            //}

            var toggleAllArcsButtonsInPrefab = newTab.GetComponentsInChildren<ToggleAllArcsButton>();
            foreach (ToggleAllArcsButton b in toggleAllArcsButtonsInPrefab)
            {
                b.SetNetworks(networks);
            }
            var toggleAllCombindedArcsInPrefab = newTab.GetComponentsInChildren<ToggleAllCombinedArcsButton>();
            foreach (ToggleAllCombinedArcsButton b in toggleAllCombindedArcsInPrefab)
            {
                b.SetNetworks(networks);
            }

            Vector3 buttonPosOrigin = buttonPos;
            for (int i = 0; i < networks.Length; ++i)
            {
                var network = networks[i];
                var newButton = Instantiate(buttonPrefab, newTab.transform);
                newButton.GetComponent<Renderer>().material.color = network.GetComponent<Renderer>().material.color;
                newButton.GetComponent<Renderer>().material.color -= new Color(0, 0, 0, 0.1f);
                var toggleArcButtonList = newButton.GetComponentsInChildren<ToggleArcsButton>();
                newButton.transform.localPosition = buttonPos;
                newButton.name = "Network" + i + "_ArcButton";
                newButton.gameObject.SetActive(true);
                foreach (var toggleArcButton in toggleArcButtonList)
                {
                    toggleArcButton.combinedNetworksButton = newTab.GetComponentInChildren<ToggleAllCombinedArcsButton>();
                    toggleArcButton.SetNetwork(network);
                }
                // position the buttons in a 4 column grid.
                if ((i + 1) % 4 == 0)
                {
                    buttonPos -= buttonPosInc * 3;
                    buttonPos += buttonPosNewRowInc;
                }
                else
                {
                    buttonPos += buttonPosInc;
                }
            }
            TurnOffAllTabs();
            //newTab.SetTabActive(GetComponent<Renderer>().enabled);
            buttonPos = buttonPosOrigin;
        }

        /// <summary>
        /// Removes all tab buttons and creates new ones from the list of network handlers active in the scene.
        /// 
        /// </summary>
        public void RefreshTabs()
        {
            var nhs = referenceManager.networkGenerator.networkList.FindAll(nh => nh != null);
            tabButtonPos = tabButtonPosOriginal;
            foreach (NetworkHandler nh in nhs)
            {
                DestroyTab(nh.name.Split('_')[1]);
                CreateToggleArcsButtons(nh.networks.ToArray());
            }
        }
    }
}

