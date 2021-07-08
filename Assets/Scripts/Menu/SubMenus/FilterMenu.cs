using CellexalVR.Filters;
using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using CellexalVR.Menu.Buttons.Selection;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    /// <summary>
    /// The menu that holds t he buttons for choosing and creating filters.
    /// </summary>
    public class FilterMenu : MenuWithoutTabs
    {

        public FilterButton buttonPrefab;
        public SubMenuButton newFilterButton;

        private MenuToggler menuToggler;
        // hard coded positions :)
        //private Vector3 buttonPos = new Vector3(-0.39f, 0.55f, 0.282f);
        private Vector3 buttonPos;
        private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
        private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);
        private List<FilterButton> buttons = new List<FilterButton>();
        private int nbrOfButtons = 0;
        private string[] filterPaths;

        private void Awake()
        {
            menuToggler = referenceManager.menuToggler;
            buttonPos = buttonPrefab.gameObject.transform.localPosition;
            CellexalEvents.FilterLoaded.AddListener(CreateButton);
        }

        /// <summary>
        /// Create buttons for files with filter information.
        /// </summary>
        /// <param name="files">An array of strings of file paths to .mds files with filter information.</param>
        public void CreateButtons(string[] files)
        {
            filterPaths = files;
            CreateButton();
        }

        private void CreateButton()
        {
            if (nbrOfButtons < filterPaths.Length)
            {
                referenceManager.filterManager.LoadFilter(filterPaths[nbrOfButtons]);
            }
        }

        /// <summary>
        /// Creates new buttons for filters. 
        /// </summary>
        /// <param name="attributes"> An array of strings that contain the names of the attributes. </param>
        public void AddFilterButton(Filter filter, string filterName)
        {
            var filterPathString = filterName.Split('\\');
            filterName = filterPathString[filterPathString.Length - 1].Split('.')[0]; 
            var newButton = Instantiate(buttonPrefab, transform);
            newButton.gameObject.SetActive(true);
            if (!menuToggler)
            {
                menuToggler = referenceManager.menuToggler;
            }
            menuToggler.AddGameObjectToActivate(newButton.gameObject, gameObject);
            if (newButton.transform.childCount > 0)
                menuToggler.AddGameObjectToActivate(newButton.transform.GetChild(0).gameObject, gameObject);
            newButton.referenceManager = referenceManager;
            newButton.transform.localPosition = buttonPos;
            newButton.SetFilter(filter, filterName);
            buttons.Add(newButton);
            // position the buttons in a 4 column grid.
            if ((nbrOfButtons + 1) % 4 == 0)
            {
                buttonPos -= buttonPosInc * 3;
                buttonPos += buttonPosNewRowInc;
            }
            else
            {
                buttonPos += buttonPosInc;
            }
            newFilterButton.transform.localPosition = buttonPos;
            nbrOfButtons++;
            CellexalEvents.FilterLoaded.Invoke();
        }

        /// <summary>
        /// Deactivates all other filters, except one.
        /// </summary>
        /// <param name="buttonToSkip">The button to not deactivate.</param>
        public void DeactivateAllOtherFilters(FilterButton buttonToSkip)
        {
            foreach (FilterButton button in buttons)
            {
                if (button != buttonToSkip)
                {
                    button.SetTextActivated(false);
                }
            }
        }

        /// <summary>
        /// Removes all buttons from the menu.
        /// </summary>
        public void RemoveButtons()
        {
            foreach (FilterButton button in buttons)
            {
                // wait 0.1 seconds so we are out of the loop before we start destroying stuff
                Destroy(button.gameObject, .1f);
            }
            buttonPos = new Vector3(-.39f, .77f, .282f);
            newFilterButton.transform.localPosition = buttonPos;
            buttons.Clear();
        }
    }
}