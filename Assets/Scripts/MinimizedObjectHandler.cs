using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the area where objects are placed when they are minimized.
/// </summary>

public class MinimizedObjectHandler : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject minimizedObjectContainerPrefab;

    private MenuToggler menuToggler;
    private Vector3 startPos = new Vector3(-.34f, .34f, .073f);
    private Vector3 dNextPosRow = new Vector3(.17f, 0, 0);
    private Vector3 dNextPosCol = new Vector3(0, -.17f, 0);
    private bool[,] spaceTaken = new bool[5, 5];
    private List<GameObject> minimizedObjects = new List<GameObject>(25);

    void Start()
    {
        for (int i = 0; i < spaceTaken.GetLength(0); ++i)
        {
            for (int j = 0; j < spaceTaken.GetLength(1); ++j)
            {
                spaceTaken[i, j] = false;
            }
        }
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        menuToggler = referenceManager.menuToggler;
        CellexalEvents.GraphsUnloaded.AddListener(Clear);
    }

    /// <summary>
    /// Minimizes an object and places it on top of the menu.
    /// </summary>
    /// <param name="objectToMinimize"> The GameObject to minimize. </param>
    /// <param name="description"> A text that will be placed on top of the minimized object. </param>
    internal void MinimizeObject(GameObject objectToMinimize, string description)
    {
        var jail = Instantiate(minimizedObjectContainerPrefab);
        minimizedObjects.Add(jail);
        jail.transform.parent = transform;
        jail.transform.localRotation = Quaternion.identity;
        jail.transform.localScale = new Vector3(.166f, .166f, .13f);
        var container = jail.GetComponent<MinimizedObjectContainer>();
        container.MinimizedObject = objectToMinimize;
        container.Handler = this;
        // if a gameobject is minimized but the menu is not active, we have to tell the 
        // menu toggler to turn that item on later.
        if (!menuToggler.MenuActive)
        {
            menuToggler.AddGameObjectToActivate(container.gameObject);
            container.GetComponent<Renderer>().enabled = false;
            container.GetComponent<Collider>().enabled = false;
        }
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
    /// Tells the minimized object handler that a space in jail is no longer occupied.
    /// Should be called when an object is un-minimzied.
    /// </summary>
    /// <param name="container"> The container that previously contained the object. </param>
    public void ContainerRemoved(MinimizedObjectContainer container)
    {
        spaceTaken[container.SpaceX, container.SpaceY] = false;
        minimizedObjects.Remove(container.gameObject);
    }

    /// <summary>
    /// Clears the area where the minimized objects are. Does not restore any of the minimized objects.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < minimizedObjects.Count; ++i)
        {
            Destroy(minimizedObjects[i]);
        }
        minimizedObjects.Clear();
        for (int i = 0; i < spaceTaken.GetLength(0); ++i)
        {
            for (int j = 0; j < spaceTaken.GetLength(1); ++j)
            {
                spaceTaken[i, j] = false;
            }
        }
        startPos = new Vector3(-.34f, .34f, .073f);
    }
}

