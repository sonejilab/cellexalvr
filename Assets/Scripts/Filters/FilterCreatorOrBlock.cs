using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisLogic;

namespace CellexalVR.Filters
{

    /// <summary>
    /// Represents an or block when creating a filter in VR. Contains two input ports and one output port.
    /// </summary>
    public class FilterCreatorOrBlock : FilterCreatorBlock
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
            return new BooleanExpression.OrExpr(input1.ToExpr(), input2.ToExpr());
        }

        public override string ToString()
        {
            if (input1.connectedTo == null || input2.connectedTo == null)
            {
                return "";
            }
            else
            {
                return "(" + input1.connectedTo.parent.ToString() + " || " + input2.connectedTo.parent.ToString() + ")";
            }
        }

        public override void SetCollidersActivated(bool activate)
        {
            foreach (var port in ports)
            {
                port.GetComponent<Collider>().enabled = activate;
            }
        }
    }
}
