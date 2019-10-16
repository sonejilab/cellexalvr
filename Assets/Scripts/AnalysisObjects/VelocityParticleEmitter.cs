using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{
    public class VelocityParticleEmitter : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public Graph graph;
        public Material particleMaterial;
        public float timeThreshold = 0.05f;
        public int itemsPerFrame = 1000;

        public Dictionary<Graph.GraphPoint, Vector3> Velocities
        {
            set
            {
                emitOrder = new List<KeyValuePair<Graph.GraphPoint, Vector3>>(value);
                itemsPerFrameConstant = emitOrder.Count / (arrowEmitRate * 90);
                greatestVelocity = Mathf.Sqrt(emitOrder.Max((kvp) => kvp.Value.sqrMagnitude));

            }
        }
        private List<KeyValuePair<Graph.GraphPoint, Vector3>> emitOrder;

        private bool constantEmitOverTime = true;
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
        private bool useGraphPointColors;
        public bool UseGraphPointColors
        {
            get => useGraphPointColors;
            set
            {
                useGraphPointColors = value;
                SetAllTexts();
            }
        }

        private new ParticleSystem particleSystem;
        /// <summary>
        /// Number of seconds between each arrow emit
        /// </summary>
        private float arrowEmitRate = 1f;
        private float threshold = 0f;
        private float speed = 10f;
        private float oldArrowEmitRate;
        private bool playing = false;
        private bool emitting = false;
        private Coroutine currentEmitCoroutine;
        private int nFramesAboveTimeThreshold = 0;
        private float greatestVelocity;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            particleSystem = gameObject.GetComponent<ParticleSystem>();
            particleSystem.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
            var mainModule = particleSystem.main;
            mainModule.simulationSpeed = speed;
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
            var stopwatch = new System.Diagnostics.Stopwatch();
            emitting = true;

            ParticleSystem.ColorBySpeedModule colorBySpeedModule = particleSystem.colorBySpeed;
            colorBySpeedModule.enabled = !UseGraphPointColors;

            Mesh graphPointMesh = referenceManager.graphGenerator.meshToUse;
            //float graphPointMeshSize = graphPointMesh.bounds.extents.magnitude * 2;
            //int verticesPerGraphPoint = graphPointMesh.vertexCount;
            Vector3 offset = graphPointMesh.normals[0] * graphPointMesh.bounds.extents.x / 2f;

            var emitParams = new ParticleSystem.EmitParams();
            int nItems = 0;
            int nextYield = ConstantEmitOverTime ? (int)itemsPerFrameConstant : itemsPerFrame;
            int yieldsSoFar = 0;
            float sqrThreshold = threshold * threshold;

            if (ConstantEmitOverTime)
            {
                for (int i = 0; i < emitOrder.Count - 2; ++i)
                {
                    int j = UnityEngine.Random.Range(i, emitOrder.Count);
                    var temp = emitOrder[i];
                    emitOrder[i] = emitOrder[j];
                    emitOrder[j] = temp;
                }
            }

            do
            {
                //stopwatch.Restart();

                if (nextYield > emitOrder.Count)
                {
                    nextYield = emitOrder.Count;
                }
                // emit one round of particles
                for (; nItems < nextYield; ++nItems)
                {
                    var keyValuePair = emitOrder[nItems];
                    if (keyValuePair.Value.sqrMagnitude > sqrThreshold)
                    {
                        emitParams.position = keyValuePair.Key.Position - offset; // + keyValuePair.Value.normalized * graphPointMeshSize;
                        emitParams.velocity = keyValuePair.Value;
                        if (UseGraphPointColors)
                        {
                            emitParams.startColor = keyValuePair.Key.GetColor();
                        }
                        particleSystem.Emit(emitParams, 1);
                    }
                }

                nItems = nextYield;

                //stopwatch.Stop();
                //if (stopwatch.Elapsed.TotalSeconds > timeThreshold)
                //{
                //    nFramesAboveTimeThreshold++;
                //}
                //else
                //{
                //    nFramesAboveTimeThreshold = 0;
                //}

                //if (nFramesAboveTimeThreshold >= 3)
                //{
                //    ChangeFrequency(2f);
                //    nFramesAboveTimeThreshold = 0;
                //    Stop();
                //    Play();

                //}

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

            } while (nItems < emitOrder.Count);

            if (constantEmitOverTime)
            {
                yield return null;
                currentEmitCoroutine = StartCoroutine(DoEmitCoroutine());
            }
            else
            {
                currentEmitCoroutine = null;
                emitting = false;
            }
        }

        public void Play()
        {
            var colorBySpeedModule = particleSystem.colorBySpeed;
            Vector2 range = new Vector2(0, greatestVelocity);
            colorBySpeedModule.range = range;
            if (constantEmitOverTime)
            {
                DoEmit();
            }
            else
            {
                InvokeRepeating("DoEmit", 0f, arrowEmitRate);
            }
            SetAllTexts();

        }

        public void Stop()
        {
            particleSystem.Stop();
            particleSystem.Clear();
            CancelInvoke();
            if (currentEmitCoroutine != null)
            {
                StopCoroutine(currentEmitCoroutine);
                currentEmitCoroutine = null;
            }
            emitting = false;
        }

        /// <summary>
        /// Changes the frequency be some amount. Frequency can not be changed below 0.
        /// </summary>
        /// <param name="amount">How much to multiply the frequency by.</param>
        /// <returns>The new frequency.</returns>
        public float ChangeFrequency(float amount)
        {
            float freq = 1f / (arrowEmitRate * amount);
            if (freq <= 0.03125 && amount > 1f)
            {
                arrowEmitRate = 32f; // 1 / 32 = 0.03125
            }

            else if (freq >= 32 && amount < 1f)
            {
                arrowEmitRate = 0.03125f;
            }
            else
            {
                arrowEmitRate *= amount;
            }
            SetAllTexts();

            return arrowEmitRate;
        }

        /// <summary>
        /// Multiples the current threshold by some amount. Thresholds lower than 0.001 is set to zero.
        /// </summary>
        /// <param name="amount">How much to multiply the threshold by.</param>
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
            SetAllTexts();
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
            SetAllTexts();
            return speed;
        }

        /// <summary>
        /// Sets the frequency, speed and threshold texts on the velocity submenu.
        /// </summary>
        /// <remarks>
        /// Automatically called by <see cref="ChangeFrequency(float)"/>, <see cref="ChangeSpeed(float)"/> and <see cref="ChangeThreshold(float)"/>.
        /// </remarks>
        public void SetAllTexts()
        {
            string newFrequencyString = (1f / arrowEmitRate).ToString();
            if (newFrequencyString.Length > 8)
            {
                newFrequencyString = newFrequencyString.Substring(0, 8);
            }
            var submenu = referenceManager.velocitySubMenu;
            submenu.frequencyText.text = "Frequency: " + newFrequencyString;
            submenu.speedText.text = "Speed: " + speed;
            submenu.thresholdText.text = "Threshold: " + threshold;
            submenu.constantSynchedModeText.text = "Mode: " + (constantEmitOverTime ? "Constant" : "Synched");
            submenu.graphPointColorsModeText.text = "Mode: " + (UseGraphPointColors ? "Graphpoint colors" : "Gradient");

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
