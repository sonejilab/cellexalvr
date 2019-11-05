using CellexalVR.AnalysisLogic;
using CellexalVR.Extensions;
using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Attributes
{
    /// <summary>
    /// Represents a button that colors all graphs according to an attribute.
    /// </summary>
    public class ColorByAttributeButton : CellexalButton
    {
        public TMPro.TextMeshPro description;

        private CellManager cellManager;
        public Color booleanNotIncludedColor;
        public Color booleanYesColor;
        public Color booleanNoColor;
        public string Attribute { get; set; }
        //public GameObject activeOutline;
        public bool colored = false;

        public enum Mode { SINGLE, BOOLEAN_EXPR }
        public Mode CurrentMode { get; set; } = Mode.SINGLE;
        public AttributeLogic CurrentBooleanExpressionState { get; private set; } = AttributeLogic.NOT_INCLUDED;
        public AttributeSubMenu parentMenu;

        private Color savedMeshStandardColor;

        protected override string Description
        {
            get { return "Toggle Attr: " + description.text; }
        }

        protected void Start()
        {
            cellManager = referenceManager.cellManager;
            CellexalEvents.GraphsColoredByGene.AddListener(ResetVars);
            CellexalEvents.GraphsReset.AddListener(ResetVars);
        }

        public override void Click()
        {
            if (CurrentMode == Mode.SINGLE)
            {
                cellManager.ColorByAttribute(Attribute, !colored);
                referenceManager.multiuserMessageSender.SendMessageColorByAttribute(Attribute, !colored);

                colored = !colored;
                ToggleOutline(colored);
                //activeOutline.SetActive(colored);
            }
            else if (CurrentMode == Mode.BOOLEAN_EXPR)
            {
                if (CurrentBooleanExpressionState == AttributeLogic.NOT_INCLUDED)
                {
                    CurrentBooleanExpressionState = AttributeLogic.YES;
                    meshRenderer.material.color = booleanYesColor;
                }
                else if (CurrentBooleanExpressionState == AttributeLogic.YES)
                {
                    CurrentBooleanExpressionState = AttributeLogic.NO;
                    meshRenderer.material.color = booleanNoColor;
                }
                else if (CurrentBooleanExpressionState == AttributeLogic.NO)
                {
                    CurrentBooleanExpressionState = AttributeLogic.NOT_INCLUDED;
                    meshRenderer.material.color = booleanNotIncludedColor;
                }

                parentMenu.EvaluateExpression();
            }
        }

        /// <summary>
        /// Called when toggling all attributes using the select all button.
        /// </summary>
        public void ColourAttribute(bool toggle)
        {
            cellManager.ColorByAttribute(Attribute, toggle);
            referenceManager.multiuserMessageSender.SendMessageColorByAttribute(Attribute, toggle);

            colored = toggle;
            ToggleOutline(toggle);
            //activeOutline.SetActive(toggle);

            //TurnOff();
        }

        /// <summary>
        /// To synchronise the outline in multi-user mode. So the outline doesnt get active if the other users menu or tab is active.
        /// </summary>
        //public void ToggleOutline(bool toggle)
        //{
        //    if (transform.parent.GetComponent<Tab>().Active)
        //    {
        //        activeOutline.SetActive(toggle);
        //    }
        //    storedState = toggle;
        //}

        /// <summary>
        /// Sets which attribute this button should show when pressed.
        /// </summary>
        /// <param name="attribute"> The name of the attribute. </param>
        /// <param name="color"> The color that the cells in possesion of the attribute should get. </param>
        public void SetAttribute(string attribute, Color color)
        {
            SetAttribute(attribute, attribute, color);
        }

        /// <summary>
        /// Sets which attribute this button should show when pressed.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="displayedName">The text that should be displayed on the button.</param>
        /// <param name="color">The color that the cells in possesion of the attribute should get.</param>
        public void SetAttribute(string attribute, string displayedName, Color color)
        {
            if (displayedName.Length > 8)
            {
                string[] shorter = { displayedName.Substring(0, displayedName.Length / 2), displayedName.Substring(displayedName.Length / 2) };
                description.text = shorter[0] + "\n" + shorter[1];
            }
            else
            {
                description.text = displayedName;
            }
            this.Attribute = attribute;
            // sometimes this is done before Awake() it seems, so we use GetComponent() here
            GetComponent<Renderer>().material.color = color;
            meshStandardColor = color;
        }

        /// <summary>
        /// Switches between boolean expression mode and normal mode.
        /// </summary>
        public void SwitchMode()
        {
            if (CurrentMode == Mode.BOOLEAN_EXPR)
            {
                CurrentMode = Mode.SINGLE;
                meshRenderer.material.color = meshStandardColor;
            }
            else if (CurrentMode == Mode.SINGLE)
            {
                CurrentMode = Mode.BOOLEAN_EXPR;
                ColorButtonBooleanExpression();
            }
        }

        public override void SetHighlighted(bool highlight)
        {
            if (highlight)
            {
                meshRenderer.material.color = meshHighlightColor;
            }
            else
            {
                if (CurrentMode == Mode.BOOLEAN_EXPR)
                {
                    ColorButtonBooleanExpression();
                }
                else if (CurrentMode == Mode.SINGLE)
                {
                    meshRenderer.material.color = meshStandardColor;
                }
            }
        }

        private void ColorButtonBooleanExpression()
        {
            if (CurrentBooleanExpressionState == AttributeLogic.YES)
            {
                meshRenderer.material.color = booleanYesColor;
            }
            else if (CurrentBooleanExpressionState == AttributeLogic.NO)
            {
                meshRenderer.material.color = booleanNoColor;
            }
            else if (CurrentBooleanExpressionState == AttributeLogic.NOT_INCLUDED)
            {
                meshRenderer.material.color = booleanNotIncludedColor;
            }
        }

        private void ResetVars()
        {
            colored = false;
            activeOutline.SetActive(false);
        }


    }
}