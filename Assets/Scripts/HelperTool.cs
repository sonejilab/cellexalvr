using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

/// <summary>
/// This class represents the helper tool. Its job is to provide the user with descriptions of whatever it touches.
/// </summary>
class HelperTool : MonoBehaviour
{

    public TextMeshPro textMesh;
    private string descriptionFilePath = Directory.GetCurrentDirectory() + "\\Assets\\descriptions.txt";
    private Dictionary<string, string> descriptions = new Dictionary<string, string>();

    private void Start()
    {
        ReadDescriptionFile(descriptionFilePath);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (descriptions.ContainsKey(other.tag))
        {
            textMesh.text = descriptions[other.tag];
        }
        else if (descriptions.ContainsKey(other.gameObject.name))
        {
            textMesh.text = descriptions[other.gameObject.name];
        }
    }

    /// <summary>
    /// Reads the descriptions.txt which should be in the Assets folder.
    /// </summary>
    /// <param name="filepath"> The path to the file. </param>
    private void ReadDescriptionFile(string filepath)
    {
        // The file format should be
        // [KEY]:[VALUE]
        // [KEY]:[VALUE]
        // ...
        // Where [KEY] is either TAG_ followed by the name of a tag or just the name of a gameobject as it is displayed in the editor.
        // [VALUE] is the description that should be displayed when the tool touches the object

        string[] lines = File.ReadAllLines(filepath);
        if (lines.Length == 0)
        {
            Debug.LogWarning("No description file found at " + descriptionFilePath);
            return;
        }
        foreach (string line in lines)
        {
            // ignore empty lines
            if (line.Length == 0)
                continue;

            // comments in the file start with #
            if (line[0] == '#')
                continue;

            // tag names in the file start with "TAG_"
            if (line.Substring(0, 4).Equals("TAG_", StringComparison.Ordinal))
            {
                var colonIndex = line.IndexOf(":");
                string tagName = line.Substring(4, colonIndex - 4);
                descriptions[tagName] = line.Substring(colonIndex + 1);
            }
            else
            {
                // everything else is assumed to be names of gameobjects
                string[] splitString = line.Split(new char[] { ':' }, 2);
                descriptions[splitString[0]] = splitString[1];
            }

        }
    }

}

