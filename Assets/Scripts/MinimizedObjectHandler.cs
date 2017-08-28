using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the area where objects are placed when they are minimized.
/// </summary>

public class MinimizedObjectHandler : MonoBehaviour
{
    public GameObject minimizedObjectContainerPrefab;
    private Vector3 startPos = new Vector3(-.34f, .34f, .073f);
    private Vector3 dNextPosRow = new Vector3(.17f, 0, 0);
    private Vector3 dNextPosCol = new Vector3(0, -.17f, 0);
    private bool[,] spaceTaken = new bool[5, 5];
    private List<Vector3> containerPositions;

    void Start()
    {
        for (int i = 0; i < spaceTaken.GetLength(0); ++i)
        {
            for (int j = 0; j < spaceTaken.GetLength(1); ++j)
            {
                spaceTaken[i, j] = false;
            }
        }
    }

    /// <summary>
    /// Minimizes an object and places it on top of the menu.
    /// </summary>
    /// <param name="objectToMinimize"> The GameObject to minimize. </param>
    /// <param name="description"> A text that will be placed on top of the minimized object. </param>
    internal void MinimizeObject(GameObject objectToMinimize, string description)
    {
        var jail = Instantiate(minimizedObjectContainerPrefab);
        jail.transform.parent = transform;
        jail.transform.localRotation = Quaternion.identity;
        jail.transform.localScale = new Vector3(.166f, .166f, .13f);
        var container = jail.GetComponent<MinimizedObjectContainer>();
        container.MinimizedObject = objectToMinimize;
        container.Handler = this;
        for (int i = 0; i < spaceTaken.GetLength(0); ++i)
        {
            for (int j = 0; j < spaceTaken.GetLength(1); ++j)
            {
                if (!spaceTaken[i, j])
                {
                    spaceTaken[i, j] = true;
                    container.SpaceX = i;
                    container.SpaceY = j;
                    jail.transform.localPosition = startPos + dNextPosRow * i + dNextPosCol * j;
                    goto afterLoop;
                }
            }
        }
        afterLoop:

        jail.GetComponentInChildren<TextMesh>().text = description;
    }

    /// <summary>
    /// Should be called when an object is un-minimzied (maximized?)´.
    /// </summary>
    /// <param name="container"> The container that previously contained the object. </param>
    public void ContainerRemoved(MinimizedObjectContainer container)
    {
        spaceTaken[container.SpaceX, container.SpaceY] = false;
    }

}

