using System;
using System.Collections.Generic;
using UnityEngine;

public class DatasetList : MonoBehaviour
{
    public List<TextMesh> listNodes;
    public LoaderController loaderController;

    private List<string> listNodeStrings;

    private void Start()
    {
        listNodeStrings = new List<string>(new string[4]);
    }

    public void SetText(int index, string name)
    {
        listNodeStrings[index] = name;
        listNodes[index].text = name;
    }

    public void RemoveNode(string value)
    {
        int i = listNodeStrings.IndexOf(value);
        listNodes.RemoveAt(i);
    }

    public void ClearList()
    {
        for (int i = 0; i < listNodes.Count; i++)
        {
            listNodes[i].text = "";
            listNodeStrings[i] = "";
        }
    }

}

