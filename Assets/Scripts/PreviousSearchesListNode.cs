using UnityEngine;
using System;

/// <summary>
/// Represents one node in the list of the 10 previous searches.
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
        { locked = value; }
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

    /// <summary>
    /// Updates the list with a new gene name, removing the bottom gene name in the list if it is full.
    /// </summary>
    /// <param name="newGeneName"> The gene name to add to the list. </param>
    /// <returns> The gene name that was removed. </returns>
    public string UpdateList(string newGeneName)
    {
        if (nextNode != null)
        {
            if (!Locked)
            {
                var returnGeneName = nextNode.UpdateList(GeneName);
                GeneName = newGeneName;
                return returnGeneName;
            }
            else
            {
                return nextNode.UpdateList(newGeneName);
            }
        }
        else
        {
            if (!Locked)
            {
                var oldGeneName = GeneName;
                GeneName = newGeneName;
                return oldGeneName;
            }
            else
            {
                return newGeneName;
            }
        }
    }

    public void SetMaterial(Material material)
    {
        renderer.sharedMaterial = material;
    }
}
