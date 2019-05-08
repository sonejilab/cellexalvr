using CellexalVR.AnalysisObjects;
using CellexalVR.Extensions;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// A clickable panel that contains some text, typically a gene name.
    /// </summary>
    public class ClickableTextPanel : ClickablePanel
    {
        public TextMesh textMesh;
        /// <summary>
        /// The entire string of text that is displayed on the panel.
        /// </summary>
        public string Text { get; protected set; }
        /// <summary>
        /// The type of the thing whose name is displayed.
        /// </summary>
        public Definitions.Measurement Type { get; protected set; }
        /// <summary>
        /// The name of the thing that is displayed.
        /// </summary>
        public string NameOfThing { get; protected set; }
        /// <summary>
        /// The method of coloring that is used to color this thing.
        /// </summary>
        public GraphManager.GeneExpressionColoringMethods ColoringMethod { get; set; }

        protected override void Start()
        {
            base.Start();
            //textMesh = GetComponentInChildren<TextMesh>();
        }

        /// <summary>
        /// Sets the text of this panel.
        /// </summary>
        /// <param name="name">The new text that should be displayed.</param>
        /// <param name="type">The type (gene, attribute or facs) of the text.</param>
        public virtual void SetText(string name, Definitions.Measurement type)
        {
            if (!textMesh)
            {
                textMesh = GetComponentInChildren<TextMesh>(true);
            }
            Type = type;
            if (type != Definitions.Measurement.INVALID)
            {
                Text = type.ToString() + " " + name;
                textMesh.text = Text;
                NameOfThing = name;
            }
            else
            {
                Text = "";
                textMesh.text = Text;
                NameOfThing = "";
            }
        }

        /// <summary>
        /// Click this panel. This will color the graphs according to what is on the panel.
        /// </summary>
        public override void Click()
        {
            if (Type == Definitions.Measurement.GENE)
            {
                referenceManager.cellManager.ColorGraphsByGene(NameOfThing, ColoringMethod);
                referenceManager.gameManager.InformColorGraphsByGene(NameOfThing);
            }
            else if (Type == Definitions.Measurement.ATTRIBUTE)
            {
                referenceManager.cellManager.ColorByAttribute(NameOfThing, true);
                referenceManager.gameManager.InformColorByAttribute(NameOfThing, true);
            }
            else if (Type == Definitions.Measurement.FACS)
            {
                referenceManager.cellManager.ColorByIndex(NameOfThing);
                referenceManager.gameManager.InformColorByIndex(NameOfThing);
            }
            referenceManager.previousSearchesList.AddEntry(NameOfThing, Type, referenceManager.graphManager.GeneExpressionColoringMethod);
            referenceManager.keyboardHandler.Clear();
            referenceManager.autoCompleteList.ClearList();
        }
    }
}