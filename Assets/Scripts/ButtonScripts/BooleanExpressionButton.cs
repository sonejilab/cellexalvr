using System;

public class BooleanExpressionButton : CellexalButton
{
    public BooleanExpression.Expr Expr { get; set; }
    private CellManager cellManager;

    protected override string Description
    {
        get { return "Color cells according to this expression"; }
    }

    protected override void Click()
    {
        cellManager.ColorByAttributeExpression(Expr);
    }
}

