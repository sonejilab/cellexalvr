using CellexalVR.General;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace CellexalVR.Interaction
{
    public sealed class ClickableReportPanel : ClickablePanel
    {
        [HideInInspector]
        public TextMeshPro textMesh;
        /// <summary>
        /// The entire string of text that is displayed on the panel.
        /// </summary>
        public string Text { get; private set; }

        public SimpleWebBrowser.WebBrowser webBrowser;

        protected override void Start()
        {
            base.Start();
            textMesh = GetComponentInChildren<TextMeshPro>();
        }


        /// <summary>
        /// Sets the text of this panel.
        /// </summary>
        /// <param name="name">The new text that should be displayed.</param>
        public void SetText(string name)
        {
            if (!textMesh)
            {
                textMesh = GetComponentInChildren<TextMeshPro>(true);
            }

            Text = name;
            string[] parts = Text.Split('\\');
            Regex regex = new Regex("[a-zA-Z.-]");
            string panelText = CellexalUser.DatasetName + "\n" + regex.Replace(parts[parts.Length - 1], "");
            textMesh.text = panelText;

            //NameOfThing = name;
            //else
            //{
            //    Text = "";
            //    textMesh.text = Text;
            //    //NameOfThing = "";
            //}
        }

        public override void Click()
        {
            base.Click();
            webBrowser.OnNavigate("file:///" + Text);
        }
    }
}