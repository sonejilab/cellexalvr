using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisLogic;

namespace CellexalVR.Filters
{

    public class FilterCreatorNotBlock : FilterCreatorBlock
    {
        public FilterCreatorBlockPort input;
        public FilterCreatorBlockPort ouput;

        public override bool IsValid()
        {
            if (input.connectedTo == null)
            {
                return false;
            }
            else
            {
                return input.connectedTo.parent.IsValid();
            }
        }

        public override BooleanExpression.Expr ToExpr()
        {
            return new BooleanExpression.NotExpr(input.ToExpr());
        }

        public override string ToString()
        {
            if (input.connectedTo == null)
            {
                return "";
            }
            else
            {
                return "(!" + input.connectedTo.parent.ToString() + ")";
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
