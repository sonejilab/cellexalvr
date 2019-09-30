using UnityEngine;
using System.Collections;
using TMPro;
using CellexalVR.Menu.Buttons;
using System;

public class LoadVelocityButton : CellexalButton
{
    public TextMeshPro buttonText;
    public string subGraphName = System.String.Empty;
    public string shorterFilePath;

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
            if (subGraphName != string.Empty)
            {
                buttonText.text = subGraphName;
            }
            else
            {
                buttonText.text = shorterFilePath;
            }
        }
    }


    protected override string Description
    {
        get
        {
            return "Load " + buttonText.text;
        }
    }

    public override void Click()
    {
        if (subGraphName != string.Empty)
        {
            referenceManager.velocityGenerator.ReadVelocityFile(FilePath, subGraphName);
        }
        else
        {
            referenceManager.velocityGenerator.ReadVelocityFile(FilePath);
        }
        referenceManager.velocitySubMenu.DeactivateOutlines();
        ToggleOutline(true);
        //activeOutline.SetActive(true);
        referenceManager.gameManager.InformReadVelocityFile(shorterFilePath, subGraphName);
    }

    public void DeactivateOutline()
    {
        ToggleOutline(false);
        //activeOutline.SetActive(false);
    }
}
