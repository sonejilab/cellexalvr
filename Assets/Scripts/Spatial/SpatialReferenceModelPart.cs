using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Class that represent glass brain part. Mesh retrieved from allen sdk. The part is spawned via the AllenReferenceBrain class.
    /// </summary>
    public class SpatialReferenceModelPart : MonoBehaviour
    {
        public int id;
        public string modelName;
        public string modelAcronym;
        public Color color;
        public GameObject plane;

        private bool spread;
        private GameObject meshParent;
        private Transform child;
        private Renderer[] rend;
        private MaterialPropertyBlock propertyBlock;

        /// <summary>
        /// Sets color of mesh. Color is retrieved from the allen sdk.
        /// </summary>
        /// <param name="col"></param>
        public void SetColor(Color col)
        {
            rend = GetComponentsInChildren<MeshRenderer>();
            color = col;
            foreach (MeshRenderer r in rend)
            {
                r.sharedMaterial.SetColor("_BaseColor", col);
            }
        }

        /// <summary>
        /// Spread out mesh part from center.
        /// </summary>
        public void Spread()
        {
            if (id == 997)
                return;
            spread = !spread;
            var brain = GetComponentInParent<AllenReferenceBrain>();
            Vector3 center = brain.GetComponent<BoxCollider>().center;
            center = brain.transform.TransformPoint(center);
            MeshCollider[] mcs = GetComponentsInChildren<MeshCollider>();
            Vector3 targetPos;
            foreach (MeshCollider mc in mcs)
            {
                if (spread)
                {
                    targetPos = mc.transform.position + (mc.bounds.center - center).normalized * 0.75f;
                }
                else
                {
                    targetPos = transform.GetChild(0).transform.TransformPoint(Vector3.zero);
                }
                mc.transform.DOMove(targetPos, 0.6f).SetEase(Ease.InOutSine);
            }

        }
    }

//#if UNITY_EDITOR
//        /// <summary>
//        /// Split part into separate meshes based on connectivity. This enables the right and left part to be moved separately. 
//        /// Many of the allen reference brain meshes are mirrored right to left which makes this a usable feature.
//        /// </summary>
//        public void SplitMesh()
//        {
//            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
//            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
//            child = transform.GetChild(0);
//            MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
//            List<Mesh> parts = new List<Mesh>();
//            parts = MeshUtils.MeshSplitter.SplitMesh(meshFilter.mesh);

//            meshParent = new GameObject();
//            meshParent.transform.parent = transform;
//            meshParent.transform.localPosition = child.transform.localPosition;
//            meshParent.transform.localRotation = child.transform.localRotation;
//            meshParent.transform.localScale = child.transform.localScale;

//            child.transform.parent = meshParent.transform;
//            child.transform.localPosition = Vector3.zero;
//            child.transform.localScale = Vector3.one;
//            child.transform.localRotation = Quaternion.identity;

//            for (int i = 0; i < parts.Count; i++)
//            {
//                Mesh m = parts[i];
//                GameObject newObj = new GameObject();
//                newObj.AddComponent<MeshRenderer>().material = meshRenderer.material;
//                newObj.AddComponent<MeshFilter>().mesh = m;
//                MeshCollider meshColldier = newObj.AddComponent<MeshCollider>();

//                newObj.transform.parent = meshParent.transform;
//                newObj.transform.localPosition = child.transform.localPosition;
//                newObj.transform.localRotation = child.transform.localRotation;
//                newObj.transform.localScale = Vector3.one;
//                newObj.gameObject.name = $"part{i}";

//                AssetDatabase.CreateAsset(m, $"Assets/Resources/meshparts/{id}_part{i}_test.asset");
//            }

//            Destroy(child.gameObject);

//        }


//    }
//    [CustomEditor(typeof(SpatialReferenceModelPart))]
//    public class SpatialReferenceModelPartEditor : Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            SpatialReferenceModelPart myTarget = (SpatialReferenceModelPart)target;

//            GUILayout.BeginHorizontal();
//            if (GUILayout.Button("Split Meshes"))
//            {
//                myTarget.SplitMesh();
//            }
//            GUILayout.EndHorizontal();

//            DrawDefaultInspector();
//        }

//    }
//#endif
}

