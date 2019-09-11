using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{
    public class VelocityParticleEmitter : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public Graph graph;
        public Material particleMaterial;

        public int itemsPerFrame = 1000;

        public Dictionary<Graph.GraphPoint, Vector3> Velocities
        {
            set
            {
                emitOrder = new List<KeyValuePair<Graph.GraphPoint, Vector3>>(value);
                itemsPerFrameConstant = emitOrder.Count / arrowEmitRate;
            }
        }
        private List<KeyValuePair<Graph.GraphPoint, Vector3>> emitOrder;

        private bool constantEmitOverTime;
        public bool ConstantEmitOverTime
        {
            get => constantEmitOverTime;
            set
            {
                constantEmitOverTime = value;
                Stop();
                Play();
            }
        }

        private float itemsPerFrameConstant;
        public bool UseGraphPointColors { get; set; }
        public bool RandomEmitOrder { get; set; }

        private new ParticleSystem particleSystem;
        /// <summary>
        /// Number of seconds between each arrow emit
        /// </summary>
        private float arrowEmitRate = 1f;
        private float threshold = 0;
        private float speed = 5;
        private float oldArrowEmitRate;
        private bool playing = false;
        private bool emitting = false;
        private Coroutine currentEmitCoroutine;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            if (constantEmitOverTime)
            {
                DoEmit();
            }
            else
            {
                InvokeRepeating("DoEmit", 0f, arrowEmitRate);
            }
            particleSystem = gameObject.GetComponent<ParticleSystem>();
            particleSystem.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
            SetColors();
            Play();
            oldArrowEmitRate = arrowEmitRate;
        }


        void DoEmit()
        {
            currentEmitCoroutine = StartCoroutine(DoEmitCoroutine());
        }

        private void Update()
        {
            if (oldArrowEmitRate != arrowEmitRate)
            {
                oldArrowEmitRate = arrowEmitRate;
                itemsPerFrameConstant = emitOrder.Count / (arrowEmitRate * 90);
                Stop();
                Play();
            }

            if (graph.transform.hasChanged && transform.localScale != graph.transform.localScale)
            {
                transform.localScale = graph.transform.localScale;
            }
        }

        private IEnumerator DoEmitCoroutine()
        {
            if (emitting && !constantEmitOverTime)
            {
                // don't emit if we are already emitting
                yield break;
            }

            emitting = true;

            ParticleSystem.ColorBySpeedModule colorBySpeedModule = particleSystem.colorBySpeed;
            colorBySpeedModule.enabled = !UseGraphPointColors;

            Mesh graphPointMesh = referenceManager.graphGenerator.graphPointStandardMesh;
            //int verticesPerGraphPoint = graphPointMesh.vertexCount;
            Vector3 offset = graphPointMesh.normals[0] * graphPointMesh.bounds.extents.x / 2f;

            var emitParams = new ParticleSystem.EmitParams();
            int nItems = 0;
            int nextYield = ConstantEmitOverTime ? (int)itemsPerFrameConstant : itemsPerFrame;
            int yieldsSoFar = 0;
            float sqrThreshold = threshold * threshold;

            if (RandomEmitOrder)
            {
                for (int i = 0; i < emitOrder.Count - 2; ++i)
                {
                    int j = UnityEngine.Random.Range(i, emitOrder.Count);
                    var temp = emitOrder[i];
                    emitOrder[i] = emitOrder[j];
                    emitOrder[j] = temp;
                }
            }

            foreach (var keyValuePair in emitOrder)
            {
                if (keyValuePair.Value.sqrMagnitude > sqrThreshold)
                {
                    emitParams.position = keyValuePair.Key.Position - offset + keyValuePair.Value.normalized * graphPointMesh.bounds.extents.magnitude * 2;
                    emitParams.velocity = keyValuePair.Value;
                    if (UseGraphPointColors)
                    {
                        emitParams.startColor = keyValuePair.Key.GetColor();
                    }
                    particleSystem.Emit(emitParams, 1);
                }

                nItems++;
                if (nItems >= nextYield)
                {
                    yield return null;
                    yieldsSoFar++;

                    if (ConstantEmitOverTime)
                    {
                        nextYield = (int)(itemsPerFrameConstant * yieldsSoFar);
                    }
                    else
                    {
                        nextYield += itemsPerFrame;
                    }
                }
            }

            if (constantEmitOverTime)
            {
                currentEmitCoroutine = StartCoroutine(DoEmitCoroutine());
            }
            else
            {
                currentEmitCoroutine = null;
            }
            emitting = false;
        }

        public void Play()
        {
            particleSystem.Play();
            InvokeRepeating("DoEmit", 0f, arrowEmitRate);
        }

        public void Stop()
        {
            particleSystem.Stop();
            particleSystem.Clear();
            CancelInvoke();
            if (currentEmitCoroutine != null)
            {
                StopCoroutine(currentEmitCoroutine);
            }
        }

        /// <summary>
        /// Changes the frequency be some amount. Frequency can not be changed below 0.
        /// </summary>
        /// <param name="amount">How much to add (or subtract) to the frequency.</param>
        /// <returns>The new frequency.</returns>
        public float ChangeFrequency(float amount)
        {
            if (arrowEmitRate == 0f && amount > 1f)
            {
                arrowEmitRate = 0.001f;
            }
            else if (arrowEmitRate <= 0.001f && amount < 1f)
            {
                arrowEmitRate = 0f;
            }
            else
            {
                arrowEmitRate *= amount;
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

        /// <summary>
        /// Change the speed of the emitter arrows by some amount. Speeds lower than 0.001 are set to 0.001.
        /// </summary>
        /// <param name="amount">The amount to change the speed by.</param>
        /// <returns>The new speed.</returns>
        public float ChangeSpeed(float amount)
        {
            if (speed == 0f && amount > 1f)
            {
                speed = 0.001f;
            }
            else if (speed <= 0.001f && amount < 1f)
            {
                speed = 0f;
            }
            else
            {
                speed *= amount;
            }
            var mainModule = particleSystem.main;
            mainModule.simulationSpeed = speed;
            return speed;
        }

        /// <summary>
        /// Sets the colors according to the <see cref="CellexalConfig.Config"/>.
        /// </summary>
        public void SetColors()
        {
            particleSystem = gameObject.GetComponent<ParticleSystem>();
            ParticleSystem.ColorBySpeedModule colorBySpeedModule = particleSystem.colorBySpeed;
            Gradient gradient = new Gradient();
            gradient.mode = GradientMode.Blend;
            gradient.colorKeys = new GradientColorKey[] {
                new GradientColorKey(CellexalConfig.Config.VelocityParticlesLowColor, 0),
                new GradientColorKey(CellexalConfig.Config.VelocityParticlesHighColor, 1) };

            colorBySpeedModule.color = new ParticleSystem.MinMaxGradient(gradient);
        }

    }

}
