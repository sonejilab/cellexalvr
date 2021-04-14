using CellexalVR.AnalysisLogic;
using CellexalVR.Extensions;
using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using CellexalVR.Menu.Buttons.Attributes;
using DefaultNamespace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    /// <summary>
    /// Represents the sub menu that pops up when the <see cref="AttributeMenuButton"/> is pressed.
    /// </summary>
    public class AttributeSubMenu : MenuWithTabs
    {
        protected Color[] Colors
        {
            get { return CellexalConfig.Config.SelectionToolColors; }
        }

        public ColorByBooleanExpressionButton booleanExpressionButtonPrefab;
        public Tab booleanExpressionTabPrefab;
        public List<string> attributes;

        protected List<ColorByBooleanExpressionButton> booleanExpressionButtons;


        public void RecreateButtons()
        {
            CreateButtons(categoriesAndNames);
        }

        public override void CreateButtons(string[] categoriesAndNames)
        {
            base.CreateButtons(categoriesAndNames);
            //string[] orderedNames = new string[names.Length];
            //Array.Copy(names, orderedNames, names.Length);
            //Array.Sort(orderedNames, StringComparer.InvariantCulture);
            for (int i = 0; i < cellexalButtons.Count; ++i)
            {
                var b = cellexalButtons[i];
                b.referenceManager = referenceManager;
                int colorIndex = i % Colors.Length;
                b.GetComponent<ColorByAttributeButton>().SetAttribute(categoriesAndNames[i], names[i], Colors[colorIndex]);
                b.GetComponent<ColorByAttributeButton>().parentMenu = this;
                b.gameObject.name = categoriesAndNames[i];
                GetComponentInChildren<ChangeColorModeButton>().firstTab = tabs[0];
            }
        }

        public override CellexalButton FindButton(string name)
        {
            var button = cellexalButtons.Find(x => x.GetComponent<ColorByAttributeButton>().Attribute == name);
            return button;
        }

        /// <summary>
        /// Destroys all tabs in this menu.
        /// </summary>
        public override void DestroyTabs()
        {
            base.DestroyTabs();
            if (cellexalButtons != null)
                cellexalButtons.Clear();
        }

        /// <summary>
        /// Switches all buttons between boolean expression and single attribute mode.
        /// </summary>
        public void SwitchButtonStates(bool bigFolder = false)
        {
            foreach (var b in cellexalButtons)
            {
                ColorByAttributeButton button = b.GetComponent<ColorByAttributeButton>();
                if (!bigFolder)
                {
                    button.SwitchMode();
                }
                else
                {
                    button.SwitchModeToBigFolder(ColorByAttributeButton.Mode.BIGFOLDER);
                }
            }
        }


        /// <summary>
        /// Builds a tree of and expressions and not expressions that corresponds to the current state of the attribute buttons.
        /// </summary>
        /// <returns>A reference to the root of the tree of the resulting expression.</returns>
        public BooleanExpression.Expr GetExpression()
        {
            BooleanExpression.Expr root = null;
            // go over each button and check its state.
            foreach (var b in cellexalButtons)
            {
                ColorByAttributeButton button = b.GetComponent<ColorByAttributeButton>();
                if (button.CurrentBooleanExpressionState != AttributeLogic.INVALID && button.CurrentBooleanExpressionState != AttributeLogic.NOT_INCLUDED)
                {
                    BooleanExpression.Expr newNode = new BooleanExpression.AttributeExpr(button.Attribute, true);

                    if (button.CurrentBooleanExpressionState == AttributeLogic.NO)
                    {
                        // wrap the new expression in a not expression
                        newNode = new BooleanExpression.NotExpr(newNode);
                    }

                    if (root == null)
                    {
                        root = newNode;
                    }
                    else
                    {
                        // wrap the last expression and the new expression in an and expression
                        root = new BooleanExpression.AndExpr(newNode, root);
                    }
                }
            }

            return root;
        }

        /// <summary>
        /// Evaluate the current boolean expression (if we are in boolean expression mode).
        /// </summary>
        public void EvaluateExpression()
        {
            referenceManager.cellManager.ColorByAttributeExpression(GetExpression());
        }

        public IEnumerator SelectAllAttributesCoroutine(bool toggle)
        {
            if (PointCloudGenerator.instance.pointCount > 0)
            {
                TextureHandler.instance.ColorAllClusters(toggle);
                foreach (ColorByAttributeButton b in GetComponentsInChildren<ColorByAttributeButton>())
                {
                    b.ToggleOutline(toggle);
                }
            }
            else
            {

                if (!toggle)
                {
                    referenceManager.multiuserMessageSender.SendMessageResetGraphColor();
                    referenceManager.graphManager.ResetGraphsColor();
                }
                else
                {
                    foreach (ColorByAttributeButton b in GetComponentsInChildren<ColorByAttributeButton>())
                    {
                        string category = "";
                        if (b.Attribute.Contains("@"))
                        {
                            category = b.Attribute.Split('@')[0];
                        }

                        if (!b.colored && category == currentCategory)
                        {
                            b.ColourAttribute(true);
                        }

                        yield return null;
                    }
                }
            }
        }
    }
}