using CellexalVR.AnalysisLogic;
using CellexalVR.Extensions;
using CellexalVR.General;
using CellexalVR.Menu.Buttons.Attributes;
using System;
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

        public override void CreateButtons(string[] categoriesAndNames)
        {
            base.CreateButtons(categoriesAndNames);
            for (int i = 0; i < buttons.Count; ++i)
            {
                var b = buttons[i];
                b.referenceManager = referenceManager;
                int colorIndex = i % Colors.Length;
                b.GetComponent<ColorByAttributeButton>().SetAttribute(categoriesAndNames[i], names[i], Colors[colorIndex]);
                b.GetComponent<ColorByAttributeButton>().parentMenu = this;
                b.gameObject.name = categoriesAndNames[i];
            }
        }


        public void AddExpressionButtons(Tuple<string, BooleanExpression.Expr>[] expressions)
        {
            if (expressions.Length == 0)
                return;

            var predefinedExpressionsTab = AddTab(booleanExpressionTabPrefab);
            foreach (var expression in expressions)
            {
                var newButton = Instantiate(booleanExpressionButtonPrefab);
                predefinedExpressionsTab.AddButton(newButton);

                newButton.GetComponentInChildren<TextMesh>().text = expression.Item1;
                newButton.Expr = expression.Item2;
            }
            predefinedExpressionsTab.SetTabActive(false);
        }

        public override void DestroyTabs()
        {
            base.DestroyTabs();
            if (buttons != null)
                buttons.Clear();
        }

        public void SwitchButtonStates()
        {
            foreach (var b in buttons)
            {
                ColorByAttributeButton button = b.GetComponent<ColorByAttributeButton>();
                button.SwitchMode();
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
            foreach (var b in buttons)
            {
                ColorByAttributeButton button = b.GetComponent<ColorByAttributeButton>();
                if (button.CurrentBooleanExpressionState != AttributeLogic.INVALID && button.CurrentBooleanExpressionState != AttributeLogic.NOT_INCLUDED)
                {
                    BooleanExpression.Expr newNode = new BooleanExpression.ValueExpr(button.Attribute);

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

        public void EvaluateExpression()
        {
            referenceManager.cellManager.ColorByAttributeExpression(GetExpression());
        }

        public void AddCurrentExpressionAsGroup()
        {
            referenceManager.cellManager.AddCellsToSelection(GetExpression(), referenceManager.selectionToolCollider.currentColorIndex);
            referenceManager.selectionToolCollider.ChangeColor(true);
        }

        public void SelectAllAttributes()
        {
            foreach (ColorByAttributeButton b in GetComponentsInChildren<ColorByAttributeButton>())
            {
                b.Click();
            }
        }

    }
}