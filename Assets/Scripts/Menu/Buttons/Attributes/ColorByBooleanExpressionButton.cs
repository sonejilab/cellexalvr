using CellexalVR.AnalysisLogic;

namespace CellexalVR.Menu.Buttons.Attributes
{
    /// <summary>
    /// Colours all graphs according to a boolean expression of attributes.
    /// </summary>
    public class ColorByBooleanExpressionButton : CellexalButton
    {
        public BooleanExpression.Expr Expr { get; set; }
        private CellManager cellManager;

        protected override string Description
        {
            get { return "Color cells according to this expression"; }
        }

        public override void Click()
        {
            cellManager.ColorByAttributeExpression(Expr);
        }
    }

}