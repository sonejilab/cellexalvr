using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.SceneObjects
{
    public class CullingCube : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public int boxNr;


        private Material material;
        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        private void Update()
        {
            foreach (Graph graph in referenceManager.graphManager.Graphs)
            {
                material = graph.graphPointClusters[0].GetComponent<Renderer>().sharedMaterial;
                if (boxNr == 0)
                {
                    material.SetMatrix("_BoxMatrix", transform.worldToLocalMatrix);
                }
                else
                {
                    material.SetMatrix("_BoxMatrix2", transform.worldToLocalMatrix);
                }
            }
        }
    }
}
