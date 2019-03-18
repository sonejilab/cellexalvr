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
    public class FilterMenu : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        public FilterButton buttonPrefab;
        public SubMenuButton newFilterButton;

        private MenuToggler menuToggler;
        // hard coded positions :)
        private Vector3 buttonPos = new Vector3(-0.39f, 0.55f, 0.282f);
        private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
        private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);
        private List<FilterButton> buttons = new List<FilterButton>();
        private int nbrOfButtons = 0;

        private void Awake()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            menuToggler = referenceManager.menuToggler;
        }

        /// <summary>
        /// Creates new buttons for coloring by attributes.
        /// </summary>
        /// <param name="attributes"> An array of strings that contain the names of the attributes. </param>
        //public void AddFilterButton(Filter filter, string filterName)
        //{
        //    var newButton = Instantiate(buttonPrefab, transform);
        //    newButton.gameObject.SetActive(true);
        //    if (!menuToggler)
        //    {
        //        menuToggler = referenceManager.menuToggler;
        //    }
        //    menuToggler.AddGameObjectToActivate(newButton.gameObject, gameObject);
        //    if (newButton.transform.childCount > 0)
        //        menuToggler.AddGameObjectToActivate(newButton.transform.GetChild(0).gameObject, gameObject);
        //    newButton.referenceManager = referenceManager;
        //    newButton.transform.localPosition = buttonPos;
        //    //newButton.SetFilter(filter, filterName);
        //    buttons.Add(newButton);
        //    // position the buttons in a 4 column grid.
        //    if ((nbrOfButtons + 1) % 4 == 0)
        //    {
        //        buttonPos -= buttonPosInc * 3;
        //        buttonPos += buttonPosNewRowInc;
        //    }
        //    else
        //    {
        //        buttonPos += buttonPosInc;
        //    }
        //    newFilterButton.transform.localPosition = buttonPos;
        //    nbrOfButtons++;

        //}

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