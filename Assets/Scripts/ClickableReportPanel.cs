using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class ClickableReportPanel : ClickablePanel
{
    [HideInInspector]
    public TextMeshPro textMesh;
    /// <summary>
    /// The entire string of text that is displayed on the panel.
    /// </summary>
    public string Text { get; protected set; }
    public Material[] materials;

    public SimpleWebBrowser.WebBrowser webBrowser;

    protected override void Start()
    {
        base.Start();
        textMesh = GetComponentInChildren<TextMeshPro>();
        materials = new Material[3];
        materials[0] = keyHighlightMaterial;
        materials[1] = keyNormalMaterial;
        materials[2] = keyPressedMaterial;
    }


    /// <summary>
    /// Sets the text of this panel.
    /// </summary>
    /// <param name="name">The new text that should be displayed.</param>
    public virtual void SetText(string name)
    {
        if (!textMesh)
        {
            textMesh = GetComponentInChildren<TextMeshPro>(true);
        }

        Text = name;
        string[] parts = Text.Split('\\');
        Regex regex = new Regex("[a-zA-Z.-]");
        string panelText = CellexalUser.DataSourceFolder + "\n" + regex.Replace(parts[parts.Length - 1], "");
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
        webBrowser.OnNavigate("file:///" + Text);
    }
}
