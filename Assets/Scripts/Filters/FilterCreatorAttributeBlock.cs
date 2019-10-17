using CellexalVR.AnalysisLogic;
using UnityEngine;

namespace CellexalVR.Filters
{
    /// <summary>
    /// The gene block that is used when creating a filter in VR. Contains a gene name field, operator field, value field and an ouput port.
    /// </summary>
    public class FilterCreatorAttributeBlock : FilterCreatorBlock
    {
        public GameObject block;
        public GameObject attributeName;
        public GameObject include;
        public FilterCreatorBlockPort output;


        private string attributeNameString;
        private string includeString;
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
            attributeNameString = attributeName.GetComponent<TMPro.TextMeshPro>().text;
            includeString = include.GetComponent<TMPro.TextMeshPro>().text;
        }

        public override bool IsValid()
        {
            UpdateStrings();
            return attributeNameString != "" && includeString != "";
        }

        public override string ToString()
        {
            UpdateStrings();
            return attributeNameString + " " + includeString;
        }

        public override BooleanExpression.Expr ToExpr()
        {
            UpdateStrings();
            return new BooleanExpression.AttributeExpr(attributeNameString, includeString == "include");
        }

        public override void SetCollidersActivated(bool activate)
        {
            attributeName.GetComponent<Collider>().enabled = activate;
            include.GetComponent<Collider>().enabled = activate;
            foreach (var port in ports)
            {
                port.GetComponent<Collider>().enabled = activate;
            }
        }
    }
}
