using CellexalVR.AnalysisLogic;
using CellexalVR.Filters;
using CellexalVR.Menu.SubMenus;
using TMPro;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Selection
{
    /// <summary>
    /// Represents a button that chooses a fitler when pressed.
    /// </summary>
    public class FilterButton : CellexalButton
    {

        public TextMeshPro text;
        private SelectionManager selectionManager;
        private FilterMenu filterMenu;
        private Filter filter;
        private bool filterActivated = false;

        protected override string Description
        {
            get { return "Toggle the " + text.text + " filter"; }
        }

        protected void Start()
        {
            selectionManager = referenceManager.selectionManager;
            filterMenu = referenceManager.filterMenu;
        }

        public override void Click()
        {
            if (!filterActivated)
            {
                filterMenu.DeactivateAllOtherFilters(this);
                referenceManager.filterManager.UpdateFilterFromFilterButton(filter);
                filterActivated = true;
                text.color = Color.green;
                referenceManager.menuRotator.RotateLeft(2);
            }
            else
            {
                referenceManager.filterManager.ResetFilter();
                filterActivated = false;
                text.color = Color.white;
            }
        }

        /// <summary>
        /// Sets the text and state of this button. Use if the filter is loaded/unloaded elsewhere and the button apperance should be updated.
        /// </summary>
        /// <param name="activate">True if this buttons filter has been loaded, false if it has been unloaded.</param>
        public void SetTextActivated(bool activate)
        {
            if (activate)
            {
                filterActivated = true;
                text.color = Color.green;
            }
            else
            {
                filterActivated = false;
                text.color = Color.white;
            }
        }

        /// <summary>
        /// Sets this buttons filter.
        /// </summary>
        /// <param name="filter">The filter to set.</param>
        /// <param name="name">The name of the filter (the name of the file it came from).</param>
        public void SetFilter(Filter filter, string name)
        {
            this.filter = filter;
            text.text = name;
        }
    }
}