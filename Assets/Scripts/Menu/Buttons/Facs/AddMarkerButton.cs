using CellexalVR.Menu.SubMenus;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Facs
{
    /// <summary>
    /// Add a marker to the list of markers that is used as axes when creating a new graph from markers. 
    /// </summary>
    public class AddMarkerButton : CellexalButton
    {
        public TMPro.TextMeshPro descriptionOnButton;
        public GameObject activeOutline;
        public GraphFromMarkersMenu parentMenu;

        private string indexName;
        private List<string> markers;

        protected override string Description
        {
            get { return "Add Marker: " + this.indexName; }
        }

        protected void Start()
        {
            markers = referenceManager.newGraphFromMarkers.markers;
            //cellManager = referenceManager.cellManager;
        }

        public override void Click()
        {
            if (markers.Count < 3 && !markers.Contains(this.indexName))
            {
                markers.Add(this.indexName);
                activeOutline.SetActive(true);
                activeOutline.GetComponent<MeshRenderer>().enabled = true;
            }
            else if (markers.Contains(this.indexName))
            {
                markers.Remove(this.indexName);
                activeOutline.SetActive(false);
            }
            referenceManager.gameManager.InformAddMarker(this.indexName);
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