using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace CellexalVR.Interaction
{
    public class ClickableHistoryPanel : ClickablePanel
    {
        public TextMeshPro textMesh;
        public string Text { get; protected set; }

        protected override void Start()
        {
            base.Start();
            textMesh = GetComponentInChildren<TextMeshPro>();
        }

        public override void Click()
        {
            
        }
    }
}