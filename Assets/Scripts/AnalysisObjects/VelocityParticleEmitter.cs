using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{
    public class VelocityParticleEmitter : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public Dictionary<Graph.GraphPoint, Vector3> velocities;
        public Graph graph;

        public int itemsPerFrame = 1000;
        private new ParticleSystem particleSystem;
        /// <summary>
        /// Number of seconds between each arrow emit
        /// </summary>
        public float arrowEmitRate = 5f;
        private float oldArrowEmitRate;
        private bool emitting = false;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            InvokeRepeating("DoEmit", 0f, arrowEmitRate);
            particleSystem = gameObject.GetComponent<ParticleSystem>();
            particleSystem.Play();
            oldArrowEmitRate = arrowEmitRate;
        }

        void DoEmit()
        {
            StartCoroutine(DoEmitcoroutine());
        }

        private void Update()
        {
            if (oldArrowEmitRate != arrowEmitRate)
            {
                CancelInvoke();
                particleSystem.Clear();
                InvokeRepeating("DoEmit", 0f, arrowEmitRate);
                oldArrowEmitRate = arrowEmitRate;
            }
        }

        private IEnumerator DoEmitcoroutine()
        {
            if (emitting)
            {
                // don't emit if we are already emitting
                yield break;
            }

            emitting = true;
            Mesh graphPointMesh = referenceManager.graphGenerator.graphPointMesh;
            int verticesPerGraphPoint = graphPointMesh.vertexCount;
            Vector3 offset = graphPointMesh.normals[0] * graphPointMesh.bounds.extents.x / 2f;

            var emitParams = new ParticleSystem.EmitParams();
            int nItems = 0;
            foreach (var keyValuePair in velocities)
            {
                emitParams.position = keyValuePair.Key.Position - offset + keyValuePair.Value.normalized * graphPointMesh.bounds.extents.magnitude * 2;
                emitParams.velocity = keyValuePair.Value;
                particleSystem.Emit(emitParams, 1);
                nItems++;
                if (nItems >= itemsPerFrame)
                {
                    yield return null;
                    nItems = 0;
                }
            }
            emitting = false;
        }
    }
}
