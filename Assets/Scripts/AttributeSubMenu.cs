using CellexalExtensions;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the sub menu that pops up when the <see cref="AttributeMenuButton"/> is pressed.
/// </summary>
public class AttributeSubMenu : MenuWithTabs
{
    protected Color[] Colors
    {
        get { return CellexalConfig.SelectionToolColors; }
    }

    public ColorByAttributeButton colorByAttributeButtonPrefab;
    public BooleanExpressionButton booleanExpressionButtonPrefab;
    public Tab booleanExpressionTabPrefab;

    protected List<ColorByAttributeButton> colorByAttributeButtons;
    protected List<BooleanExpressionButton> booleanExpressionButtons;

    private int buttonsPerTab = 20;

    /// <summary>
    /// Fill the menu with buttons that will color graphs according to attributes when pressed.
    /// </summary>
    /// <param name="categoriesAndNames">The names of the attributes.</param>
    public void CreateAttributeButtons(string[] categoriesAndNames)
    {
        DestroyTabs();

        if (colorByAttributeButtons == null)
            colorByAttributeButtons = new List<ColorByAttributeButton>();
        foreach (var button in colorByAttributeButtons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
        }
        colorByAttributeButtons.Clear();
        //TurnOffAllTabs();
        string[] categories = new string[categoriesAndNames.Length];
        string[] names = new string[categoriesAndNames.Length];
        for (int i = 0; i < categoriesAndNames.Length; ++i)
        {
            if (categoriesAndNames[i].Contains("@"))
            {
                string[] categoryAndName = categoriesAndNames[i].Split('@');
                categories[i] = categoryAndName[0];
                names[i] = categoryAndName[1];
            }
            else
            {
                categories[i] = "";
                names[i] = categoriesAndNames[i];
            }
        }

        Tab newTab = null;
        for (int i = 0, buttonIndex = 0; i < names.Length; ++i, ++buttonIndex)
        {
            // add a new tab if we encounter a new category, or if the current tab is full
            if (buttonIndex % buttonsPerTab == 0 || i > 0 && categories[i] != categories[i - 1])
            {
                newTab = AddTab(tabPrefab);
                newTab.TabButton.GetComponentInChildren<TextMesh>().text = categories[i];
                buttonIndex = 0;

            }
            var newButton = Instantiate(colorByAttributeButtonPrefab, newTab.transform);

            newButton.gameObject.SetActive(true);

            //menuToggler.AddGameObjectToActivate(newButton.gameObject, gameObject);

            if (buttonIndex < Colors.Length)
                newButton.GetComponent<Renderer>().material.color = Colors[buttonIndex];
            colorByAttributeButtons.Add(newButton);
            newTab.AddButton(newButton);
        }
        // set the names of the attributes after the buttons have been created.
        for (int i = 0; i < colorByAttributeButtons.Count; ++i)
        {
            var b = colorByAttributeButtons[i];
            b.referenceManager = referenceManager;
            int colorIndex = i % Colors.Length;
            b.SetAttribute(categoriesAndNames[i], names[i], Colors[colorIndex]);
            b.parentMenu = this;
        }
        // turn on one of the tabs
        TurnOffAllTabs();
        //newTab.SetTabActive(true);
        //newTab.SetTabActive(GetComponent<Renderer>().enabled);
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
        if (colorByAttributeButtons != null)
            colorByAttributeButtons.Clear();
    }

    public void SwitchButtonStates()
    {
        foreach (var button in colorByAttributeButtons)
        {
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
        foreach (var button in colorByAttributeButtons)
        {
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
        referenceManager.cellManager.AddCellsToSelection(GetExpression(), referenceManager.selectionToolHandler.currentColorIndex);
        referenceManager.selectionToolHandler.ChangeColor(true);
    }
}
