using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CellexalVR.General
{
    public class CurvedHeatmapGenerator : CurvedMeshGenerator
    {
        public static CurvedHeatmapGenerator instance;
        public GameObject leftSidePrefab;
        public List<Texture2D> texture2Ds = new List<Texture2D>();
        public Texture2D geneListTexture;
        public Material geneListMat;

        private GameObject meshObject;

        private void Start()
        {
            instance = this;
        }

        private void Update()
        {

        }

        private void SetTextures()
        {
            
        }

        public override void GenerateNodes(float r = 1.0f)
        {
            StartCoroutine(GenerateCurvedHeatmap());
        }

        public IEnumerator GenerateCurvedHeatmap()
        {
            if (meshObject != null)
            {
                Destroy(meshObject);
            }
            yield return null;
            meshObject = new GameObject();
            meshObject.transform.parent = transform;
            meshObject.transform.localScale = Vector3.one;
            meshObject.name = "CurvedHeatmap";
            meshObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
            renderer.material.SetTexture("_MainTex", texture2Ds[0]);
            renderer.material.SetTexture("_MainTex2", texture2Ds[1]);
            currentNodeMode = NodeMode.Curved;
            GenerateCurvedNodes();
            GenerateMeshes();
            CreateSides();


            //return meshObject;
        }

        private void CreateSides()
        {
            var leftSide = Instantiate(leftSidePrefab, meshObject.transform);
            Vector3 pos = meshNodePositions[0];
            pos.y = 0f;
            leftSide.transform.localPosition = pos;
            leftSide.transform.localScale = new Vector3(1, 0.95f, 1);
            leftSide.transform.LookAt(2 * leftSide.transform.position - transform.position);
            leftSide.transform.position += leftSide.transform.right * -0.125f;
            pos = leftSide.transform.localPosition;
            pos.y = 0.471f;
            leftSide.transform.localPosition = pos;

            var rightSide = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rightSide.name = "GeneList";
            rightSide.transform.parent = meshObject.transform;
            rightSide.transform.localScale = new Vector3(0.2f, 0.95f, 1);
            pos = meshNodePositions[meshNodePositions.Count - 1];
            pos.y = 0f;
            rightSide.transform.localPosition = pos;
            rightSide.transform.LookAt(2 * rightSide.transform.position - transform.position);
            rightSide.transform.position += rightSide.transform.right * 0.1f;
            pos = rightSide.transform.localPosition;
            pos.y = 0.471f;
            rightSide.transform.localPosition = pos;

            MeshRenderer rMeshRend = rightSide.GetComponent<MeshRenderer>();
            rMeshRend.material = geneListMat;
            rMeshRend.material.SetTexture("_MainTex", geneListTexture);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CurvedHeatmapGenerator))]
public class CurvedHeatmapGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        CurvedHeatmapGenerator myTarget = (CurvedHeatmapGenerator)target;

        //GUILayout.BeginHorizontal();
        //if (GUILayout.Button("Generate Nodes"))
        //{
        //    myTarget.GenerateNodes();
        //}
        //GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Meshes"))
        {
            myTarget.GenerateCurvedHeatmap();
        }
        GUILayout.EndHorizontal();

        DrawDefaultInspector();
    }
}
#endif