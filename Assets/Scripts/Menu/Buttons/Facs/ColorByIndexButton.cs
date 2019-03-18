using CellexalVR.Menu.SubMenus;
using CellexalVR.AnalysisLogic;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Facs
{
    /// <summary>
    /// Represents a button that colors all graphs according to an index.
    /// </summary>
    public class ColorByIndexButton : CellexalButton
    {
        public TextMesh descriptionOnButton;
        public ColorByIndexMenu parentMenu;

        private CellManager cellManager;
        private string indexName;

        protected override string Description
        {
            get { return "Color graphs by facs - " + this.indexName; }
        }

        protected void Start()
        {
            cellManager = referenceManager.cellManager;
        }

        public override void Click()
        {
            cellManager.ColorByIndex(indexName);
            referenceManager.gameManager.InformColorByIndex(indexName);
        }

        /// <summary>
        /// Sets which index this button should show when pressed.
        /// </summary>
        /// <param name="indexName"> The name of the index. </param>
        public void SetIndex(string indexName)
        {
            //color = network.GetComponent<Renderer>().material.color;
            //GetComponent<Renderer>().material.color = color;
            meshStandardColor = GetComponent<Renderer>().material.color;
            this.indexName = indexName;
            descriptionOnButton.text = indexName;
        }
    }
}