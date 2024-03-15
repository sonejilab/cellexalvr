﻿using CellexalVR.AnalysisLogic;
using UnityEngine;

namespace CellexalVR.Filters
{

    /// <summary>
    /// Represents a facs block when creating a filter in VR. Contains a name field, operator field, a value field and an output port.
    /// </summary>
    public class FilterCreatorFacsBlock : FilterCreatorBlock
    {
        public GameObject block;
        public GameObject facsName;
        public GameObject operatorSign; // can't name it operator :(
        public GameObject value;
        public FilterCreatorBlockPort output;


        private string facsString;
        private string operatorString;
        private string valueString;
        private MeshRenderer meshRenderer;

        public override FilterCreatorBlockHighlighter HighlightedSection
        {
            get => base.HighlightedSection;
            set
            {
                base.HighlightedSection = value;
                int section = value != null ? value.section : 0;
                meshRenderer.material.mainTextureOffset = new Vector2(section * 0.25f, 0f);
            }
        }

        protected override void Start()
        {
            base.Start();
            meshRenderer = block.GetComponent<MeshRenderer>();
            UpdateStrings();
        }

        private void UpdateStrings()
        {
            facsString = facsName.GetComponent<TMPro.TextMeshPro>().text;
            operatorString = operatorSign.GetComponent<TMPro.TextMeshPro>().text;
            valueString = value.GetComponent<TMPro.TextMeshPro>().text;
        }

        public override bool IsValid()
        {
            UpdateStrings();
            return facsString != "" && operatorString != "" && valueString != "";
        }

        public override string ToString()
        {
            UpdateStrings();
            return facsString + " " + operatorString + " " + valueString;
        }

        public override BooleanExpression.Expr ToExpr()
        {
            UpdateStrings();
            bool isPercent = valueString[valueString.Length - 1] == '%';
            float valueFloat;
            if (isPercent)
            {
                valueFloat = float.Parse(valueString.Substring(0, valueString.Length - 1));
            }
            else
            {
                valueFloat = float.Parse(valueString);
            }
            return new BooleanExpression.FacsExpr(facsString.ToLower(), BooleanExpression.ParseToken(operatorString, 0), valueFloat, isPercent);
        }

        public override void SetCollidersActivated(bool activate)
        {
            facsName.GetComponent<Collider>().enabled = activate;
            operatorSign.GetComponent<Collider>().enabled = activate;
            value.GetComponent<Collider>().enabled = activate;
            foreach (var port in ports)
            {
                port.GetComponent<Collider>().enabled = activate;
            }
        }
    }
}
