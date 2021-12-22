using CellexalVR.General;
using UnityEngine;


namespace CellexalVR.SceneObjects
{

    /// <summary>
    /// Holds the directory name that the cells in the boxes should represent.
    /// </summary>
    public class CellsToLoad : MonoBehaviour
    {
        private string directory;
        private bool graphsLoaded = false;
        private Transform defaultParent;
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;

        public string Directory
        {
            get
            {
                graphsLoaded = true;
                return directory;
            }
            set
            {
                directory = value;
            }
        }

        private void Start()
        {
            //referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            this.gameObject.name = directory;
        }

        public bool GraphsLoaded()
        {
            return graphsLoaded;
        }

        
        internal void ResetPosition()
        {
            transform.parent = defaultParent;
            transform.localPosition = defaultPosition;
            transform.localRotation = defaultRotation;
        }

        internal void SavePosition(Transform parent)
        {
            defaultPosition = transform.localPosition;
            defaultRotation = transform.localRotation;
            defaultParent = parent;
        }
    }
}
