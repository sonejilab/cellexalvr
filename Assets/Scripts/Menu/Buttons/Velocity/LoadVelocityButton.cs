using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class LoadVelocityButton : CellexalVR.Menu.Buttons.CellexalButton
{
    public TextMeshPro buttonText;
    public GameObject activeOutline;

    private string shorterFilePath;
    private string filePath;
    public string FilePath
    {
        get { return filePath; }
        set
        {
            filePath = value;
            int lastSlashIndex = filePath.LastIndexOfAny(new char[] { '/', '\\' });
            int lastDotIndex = filePath.LastIndexOf('.');
            shorterFilePath = filePath.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);
            buttonText.text = shorterFilePath;
        }
    }


    protected override string Description
    {
        get
        {
            return "Load " + shorterFilePath;
        }
    }

    public override void Click()
    {
        referenceManager.velocityGenerator.ReadVelocityFile(FilePath);
        referenceManager.velocitySubMenu.DeactivateOutlines();
        activeOutline.SetActive(true);
    }

    public void DeactivateOutline()
    {
        activeOutline.SetActive(false);
    }
}
