using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

namespace CellexalVR.Spatial
{
    public class SpatialReferenceModelPart : MonoBehaviour
    {
        public int id;
        public string modelName;
        public string modelAcronym;
        public Color color;

        private bool spread;
        private GameObject meshParent;
        private Transform child;
        private Renderer[] rend;
        private MaterialPropertyBlock propertyBlock;


        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.U))
            //{
            //    SplitMesh();
            //}
            //if (Input.GetKeyDown(KeyCode.Y))
            //{
            //    StartCoroutine(Spread());
            //}
        }

        public void SetColor(Color col)
        {
            rend = GetComponentsInChildren<MeshRenderer>();
            color = col;
            //if (propertyBlock == null)
            //    propertyBlock = new MaterialPropertyBlock();
            //propertyBlock.SetColor("_Color", col);
            foreach (MeshRenderer r in rend)
            {
                //r.SetPropertyBlock(propertyBlock);
                r.material.SetColor("_Color", col);
            }
        }

        /// <summary>
        /// Split part into separate meshes based on connectivity. This enables the right and left part to be moved separately. 
        /// Many of the allen reference brain meshes are mirrored right to left which makes this a usable feature.
        /// </summary>
        public void SplitMesh()
        {
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            child = transform.GetChild(0);
            MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
            List<Mesh> parts = new List<Mesh>();
            if (AllenReferenceBrain.instance.nonSplittableMeshes.Contains(id))
            {
                parts.Add(meshFilter.mesh);
            }
            else
            {
                parts = CellexalVR.MeshUtils.MeshSplitter.SplitMesh(meshFilter.mesh);
            }

            meshParent = new GameObject();
            meshParent.transform.parent = transform;
            meshParent.transform.localPosition = child.transform.localPosition;
            meshParent.transform.localRotation = child.transform.localRotation;
            meshParent.transform.localScale = child.transform.localScale;

            child.transform.parent = meshParent.transform;
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.identity;

            for (int i = 0; i < parts.Count; i++)
            {
                Mesh m = parts[i];
                GameObject newObj = new GameObject();
                newObj.AddComponent<MeshRenderer>().material = meshRenderer.material;
                newObj.AddComponent<MeshFilter>().mesh = m;
                MeshCollider meshColldier = newObj.AddComponent<MeshCollider>();

                newObj.transform.parent = meshParent.transform;
                newObj.transform.localPosition = child.transform.localPosition;
                newObj.transform.localRotation = child.transform.localRotation;
                newObj.transform.localScale = Vector3.one;
                newObj.gameObject.name = $"part{i}";

                AssetDatabase.CreateAsset(m, $"Assets/brainmeshes/models2/{id}_part{i}.asset");
            }

            Destroy(child.gameObject);

        }

        public void Spread()
        {
            if (id == 997)
                return;

            spread = !spread;
            //float t = 0f;
            //float animationTime = 1f;
            var brain = GetComponentInParent<AllenReferenceBrain>();
            Vector3 center = brain.GetComponent<BoxCollider>().center;
            center = brain.transform.TransformPoint(center);
            MeshCollider[] mcs = GetComponentsInChildren<MeshCollider>();
            Vector3 targetPos;
            foreach (MeshCollider mc in mcs)
            {
                if (spread)
                {
                    targetPos = mc.transform.position + (mc.bounds.center - center).normalized * 0.5f;
                }
                else
                {
                    targetPos = transform.GetChild(0).transform.TransformPoint(Vector3.zero);
                }
                mc.transform.DOMove(targetPos, 0.8f).SetEase(Ease.InOutSine);
            }
            //for (int i = 0; i < mrs.Length; i++)
            //{
            //    start[i] = new Vector3(mrs[i].transform.position.x, mrs[i].transform.position.y, mrs[i].transform.position.z);
            //    dirs[i] = (mrs[i].bounds.center - center).normalized;
            //    if (spread)
            //    {
            //        end[i] = start[i] + dirs[i] * 0.5f;
            //    }
            //    else
            //    {
            //        end[i] = transform.GetChild(0).transform.TransformPoint(Vector3.zero);
            //    }
            //}

            //while (t < animationTime)
            //{
            //    float progress = Mathf.SmoothStep(0, animationTime, t);
            //    for (int i = 0; i < mrs.Length; i++)
            //    {
            //        mrs[i].transform.position = Vector3.Lerp(start[i], end[i], progress);
            //        //Debug.DrawLine(mcs[i].transform.position, center);
            //    }
            //    t += (Time.deltaTime / animationTime);
            //    yield return null;
            //}

        }

    }

}
