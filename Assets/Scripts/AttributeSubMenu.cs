using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeSubMenu : MonoBehaviour
{

    public AttributeMenuButton buttonPrefab;
    private Vector3 buttonPos = new Vector3(-.2f, .18f, -.5f);
    private Vector3 buttonPosInc = new Vector3(.1f, 0, 0);

    public void CreateAttributeButtons(string[] attributes)
    {
        foreach (string attribute in attributes)
        {
            Instantiate(buttonPrefab, buttonPos, Quaternion.identity);
            buttonPos += buttonPosInc;
        }
    }
}
