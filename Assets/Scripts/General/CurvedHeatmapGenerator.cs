using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CellexalVR.General
{
    public class CurvedHeatmapGenerator : CurvedMeshGenerator
    {
        public GameObject leftSidePrefab;

        private List<Texture2D> textures;
        private Texture2D geneListTexture;
        private Material material;

        private void Start()
        {

        }

        private void Update()
        {

        }

        private void SetTextures()
        {
            
        }
        
        public void GenerateCurvedHeatmapMesh()
        {
            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
            {
                DestroyImmediate(mr.gameObject);
            }
            GameObject meshObject = new GameObject();
            meshObject.transform.parent = transform;
            meshObject.transform.localScale = Vector3.one;
            meshObject.name = "CurvedHeatmap";
            meshObject.AddComponent<MeshFilter>();
            meshObject.AddComponent<MeshRenderer>();

            GenerateNodes(type: "curved");

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
            rightSide.transform.localScale = new Vector3(0.08f, 0.95f, 1);
            pos = meshNodePositions[meshNodePositions.Count - 1];
            pos.y = 0f;
            rightSide.transform.localPosition = pos;
            rightSide.transform.LookAt(2 * rightSide.transform.position - transform.position);
            rightSide.transform.position += rightSide.transform.right * 0.04f;
            pos = rightSide.transform.localPosition;
            pos.y = 0.471f;
            rightSide.transform.localPosition = pos;
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
            myTarget.GenerateCurvedHeatmapMesh();
        }
        GUILayout.EndHorizontal();

        DrawDefaultInspector();
    }
}
#endif