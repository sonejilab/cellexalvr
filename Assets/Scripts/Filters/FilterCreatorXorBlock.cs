using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisLogic;

namespace CellexalVR.Filters
{

    public class FilterCreatorXorBlock : FilterCreatorBlock
    {
        public FilterCreatorBlockPort input1;
        public FilterCreatorBlockPort input2;
        public FilterCreatorBlockPort ouput;

        public override bool IsValid()
        {
            if (input1.connectedTo == null || input2.connectedTo == null)
            {
                return false;
            }
            else
            {
                return input1.connectedTo.parent.IsValid() && input2.connectedTo.parent.IsValid();
            }
        }

        public override BooleanExpression.Expr ToExpr()
        {
            return new BooleanExpression.XorExpr(input1.ToExpr(), input2.ToExpr());
        }

        public override string ToString()
        {
            if (input1.connectedTo == null || input2.connectedTo == null)
            {
                return "";
            }
            else
            {
                return "(" + input1.connectedTo.parent.ToString() + " ^ " + input2.connectedTo.parent.ToString() + ")";
            }
        }
    }
}
