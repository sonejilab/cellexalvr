using UnityEngine;
using System;

/// <summary>
/// This class represents one node in the list of the 10 previous searches.
/// </summary>
public class PreviousSearchesListNode : MonoBehaviour
{

    public PreviousSearchesListNode nextNode;
    private new Renderer renderer;
    private bool locked;
    public bool Locked
    {
        get
        { return locked; }
        set
        {
            if (geneName != "")
            {
                locked = value;
            }
        }
    }
    private string geneName;
    public string GeneName
    {
        get
        { return geneName; }
        set
        {
            geneName = value;
            if (textMesh != null)
                textMesh.text = geneName;
        }
    }
    public int Index;
    private TextMesh textMesh;

    void Start()
    {
        textMesh = GetComponentInChildren<TextMesh>();
        GeneName = "";
        renderer = GetComponent<Renderer>();
    }

    public void UpdateList(string newGeneName)
    {
        if (nextNode != null)
        {
            if (!Locked)
            {
                nextNode.UpdateList(GeneName);
            }
            else
            {
                nextNode.UpdateList(newGeneName);
            }
        }

        if (!Locked)
            GeneName = newGeneName;
    }

    public void SetMaterial(Material material)
    {
        renderer.sharedMaterial = material;
    }
}
