using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using CellexalVR.Spatial;
using CellexalVR.Interaction;
using CellexalVR.General;
using CellexalVR.AnalysisLogic;
using Unity.Mathematics;
using AnalysisLogic;

namespace CellexalVR.Spatial
{
    public class ReferenceModelOverlap : MonoBehaviour
    {
        public GameObject cube;

        private AllenReferenceBrain referenceBrain;

        private void Start()
        {
            referenceBrain = GetComponent<AllenReferenceBrain>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {

                CalculateOverlap();
            }
        }

        private void CalculateOverlap()
        {
            List<Vector3> pointPositions = new List<Vector3>();
            Dictionary<string, int> hitCount = new Dictionary<string, int>();
            Color[] alphas = TextureHandler.instance.alphaTextureMaps[0].GetPixels();
            Color[] positions = PointCloudGenerator.instance.pointClouds[0].positionTextureMap.GetPixels();
            Vector3 center = Vector3.zero;
            // Add colliders to all active brain models so raycasts can hit.
            foreach (string s in referenceBrain.names.Keys)
            {
                //GameObject child = kvp.Value.transform.Find("grp1").gameObject;
                //child.layer = LayerMask.NameToLayer("Brain");
                // Need to split mesh in two parts and add convex mesh collider onto the relevant side before checking for overlap. https://forum.unity.com/threads/separating-mesh-by-connectivity.480329/
                //BoxCollider mc = child.AddComponent<BoxCollider>();
                //center = mc.bounds.center;
                //mc.isTrigger = true;
                //kvp.Value.layer = LayerMask.NameToLayer("Brain");
                hitCount[s] = 0;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                Color a = alphas[i];
                if (a.maxColorComponent > 0.8f)
                {
                    Vector3 pos = transform.TransformPoint(new Vector3(positions[i].r, positions[i].g, positions[i].b));
                    pointPositions.Add(pos);

                }
            }

            // Raycast in six directions for each point. 
            Vector3[] directions = new Vector3[] { Vector3.forward, Vector3.up, Vector3.right, -Vector3.forward, -Vector3.up, -Vector3.right };
            print($"points : {pointPositions.Count}");
            //Vector3 scale = GetComponentInParent<PointCloud>().transform.localScale;
            for (int i = 0; i < pointPositions.Count; i++)
            {
                Vector3 pos = pointPositions[i] + (Vector3.one * 0.5f);
                var c = GameObject.Instantiate(cube, transform);
                c.transform.localScale = Vector3.one * 0.1f;
                c.transform.position = pos;
                var bounds = AllenReferenceBrain.instance.spawnedParts["root"].GetComponentInChildren<MeshCollider>().bounds;
                //Collider[] colliders = Physics.OverlapBox(pos, c.transform.localScale / 2f, Quaternion.identity, 1 << LayerMask.NameToLayer("Brain"));
                Collider[] colliders = Physics.OverlapSphere(pos, 0.001f, 1 << LayerMask.NameToLayer("Brain"));
                bool hit = Physics.CheckSphere(pos, 0.001f, 1 << LayerMask.NameToLayer("Brain"));
                print($"{hit}, {bounds.Contains(pos)}");
                for (int j = 0; j < colliders.Length; j++)
                {
                    Collider col = colliders[j];
                    print(col.gameObject.name);
                    SpatialReferenceModelPart part = col.transform.GetComponentInParent<SpatialReferenceModelPart>();
                    if (part != null)
                    {
                        print($"{i}, {colliders.Length}, {part.modelName}, {part.gameObject.name}");
                        c.GetComponent<MeshRenderer>().material.color = Color.red;
                        hitCount[part.modelName]++;
                    }
                }

                //foreach (Vector3 dir in directions)
                //{
                //center = AllenReferenceBrain.instance.spawnedParts["root"].GetComponentInChildren<MeshCollider>().bounds.center;
                //var c = Instantiate(cube, transform);
                //c.transform.localScale = Vector3.one * 0.1f;
                //c.transform.position = pos;
                //c = Instantiate(cube, transform);
                //c.transform.localScale = Vector3.one * 0.1f;
                //c.transform.position = center;
                //Vector3 dir = center - pos;
                //Ray ray = new Ray(pos, dir);
                //Debug.DrawRay(pos, dir);
                //RaycastHit[] hits = Physics.RaycastAll(ray, 5, 1 << LayerMask.NameToLayer("Brain"));
                //print(hits.Length);
                //foreach (RaycastHit hit in hits)
                //{
                //    print(hit.collider.gameObject.name);
                //    SpatialReferenceModelPart part = hit.collider.GetComponentInParent<SpatialReferenceModelPart>();
                //    if (part == null) continue;
                //    print(part.modelName);
                //    //var c = Instantiate(cube, transform);
                //    //c.transform.localScale = Vector3.one;
                //    //c.transform.position = pos + (Vector3.one * 0.5f);
                //    hitCount[part.modelName]++;
                //}
                //}
            }
            foreach (KeyValuePair<string, int> kvp in hitCount)
            {
                if (kvp.Value == 0) continue;
                print($"{kvp.Key} : {kvp.Value}");
                //GameObject child = referenceBrain.spawnedParts[kvp.Key].transform.Find("grp1").gameObject;
                //Destroy(child.GetComponent<MeshCollider>());
            }
        }
    }
}
