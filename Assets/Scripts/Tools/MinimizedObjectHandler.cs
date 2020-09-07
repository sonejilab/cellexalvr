using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace CellexalVR.Tools
{


    /// <summary>
    /// Represents the area where objects are placed when they are minimized.
    /// </summary>

    public class MinimizedObjectHandler : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject minimizedObjectContainerPrefab;

        private MenuToggler menuToggler;
        private Vector3 startPos = new Vector3(.34f, .34f, .13f);
        private Vector3 dNextPosRow = new Vector3(0f, -.17f, 0);
        private Vector3 dNextPosCol = new Vector3(-.17f, 0f, 0);
        private bool[,] spaceTaken = new bool[5, 5];
        private List<GameObject> minimizedObjects = new List<GameObject>(25);

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            for (int i = 0; i < spaceTaken.GetLength(0); ++i)
            {
                for (int j = 0; j < spaceTaken.GetLength(1); ++j)
                {
                    spaceTaken[i, j] = false;
                }
            }
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
            var jail = Instantiate(minimizedObjectContainerPrefab, transform, true);
            minimizedObjects.Add(jail);
            jail.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            //jail.transform.Rotate(0f, 0f, -90f);
            jail.transform.localScale = new Vector3(1f, 1f, 1f);
            var container = jail.GetComponent<MinimizedObjectContainer>();
            container.MinimizedObject = objectToMinimize;
            container.Handler = this;
            AutoMoveAndRotate rotateScript = jail.GetComponentInChildren<AutoMoveAndRotate>();
            Vector3 rotation = new Vector3(Random.Range(-25f, 25f), Random.Range(-25f, 25f), 0f);
            rotation.z = 50f - (Mathf.Abs(rotation.x) + Mathf.Abs(rotation.y));
            rotateScript.rotateDegreesPerSecond.value = rotation;

            // if a gameobject is minimized but the menu is not active, we have to tell the 
            // menu toggler to turn that item on later.
            //if (!menuToggler.MenuActive)
            //{
            //    menuToggler.AddGameObjectToActivate(container.gameObject);
            //    container.GetComponent<Renderer>().enabled = false;
            //    container.GetComponent<Collider>().enabled = false;
            //}
            bool placed = false;
            for (int j = 0; j < spaceTaken.GetLength(0) && !placed; ++j)
            {
                for (int i = 0; i < spaceTaken.GetLength(1) && !placed; ++i)
                {
                    if (spaceTaken[i, j]) continue;
                    spaceTaken[i, j] = true;
                    container.SpaceX = i;
                    container.SpaceY = j;
                    jail.transform.localPosition = startPos + dNextPosRow * i + dNextPosCol * j;
                    placed = true;
                }
            }
            jail.GetComponentInChildren<TextMeshPro>().text = description;
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
        private void Clear()
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
}


