using UnityEngine;
using System.Collections;

namespace CellexalVR.Spatial
{
    public class CullingCube : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            // get all renderers in this object and its children:
            var renders = GetComponentsInChildren<Renderer>();
            foreach (Renderer rendr in renders)
            {
                rendr.material.renderQueue = 3002; // set their renderQueue
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
