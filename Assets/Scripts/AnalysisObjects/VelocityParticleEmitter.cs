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

        public float threshold;
        private float oldArrowEmitRate;
        private bool emitting = false;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            InvokeRepeating("DoEmit", 0f, arrowEmitRate);
            particleSystem = gameObject.GetComponent<ParticleSystem>();
            SetColors();
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
            float sqrThreshold = threshold * threshold;
            foreach (var keyValuePair in velocities)
            {
                if (keyValuePair.Value.sqrMagnitude > sqrThreshold)
                {
                    emitParams.position = keyValuePair.Key.Position - offset + keyValuePair.Value.normalized * graphPointMesh.bounds.extents.magnitude * 2;
                    emitParams.velocity = keyValuePair.Value;
                    particleSystem.Emit(emitParams, 1);
                }
                nItems++;
                if (nItems >= itemsPerFrame)
                {
                    yield return null;
                    nItems = 0;
                }
            }
            emitting = false;
        }

        public void Play()
        {
            particleSystem.Play();
        }

        public void Stop()
        {
            particleSystem.Stop();
            particleSystem.Clear();
            CancelInvoke();
        }

        /// <summary>
        /// Changes the frequency be some amount. Frequency can not be changed below 0.
        /// </summary>
        /// <param name="amount">How much to add (or subtract) to the frequency.</param>
        /// <returns>The new frequency.</returns>
        public float ChangeFrequency(float amount)
        {
            float newRate = arrowEmitRate + amount;
            if (newRate > 0)
            {
                arrowEmitRate = newRate;
            }

            return arrowEmitRate;
        }

        /// <summary>
        /// Multiples the current threshold by some amount. Thresholds lower than 0.001 is set to zero.
        /// </summary>
        /// <param name="amount">How much to multiply the frequency by.</param>
        /// <returns>The new threshold.</returns>
        public float ChangeThreshold(float amount)
        {
            if (threshold == 0f && amount > 1f)
            {
                threshold = 0.001f;
            }
            else if (threshold <= 0.001f && amount < 1f)
            {
                threshold = 0f;
            }
            else
            {
                threshold *= amount;
            }
            return threshold;
        }

        public void SetColors()
        {
            particleSystem = gameObject.GetComponent<ParticleSystem>();
            ParticleSystem.ColorBySpeedModule colorBySpeedModule = particleSystem.colorBySpeed;
            colorBySpeedModule.color = new ParticleSystem.MinMaxGradient(CellexalConfig.Config.VelocityParticlesLowColor, CellexalConfig.Config.VelocityParticlesHighColor);
        }

    }

}
