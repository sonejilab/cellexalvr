using CellexalVR.General;
using System;
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
        public int itemsPerFrame = 1000;
        public Material arrowParticleMaterial;
        public Material circleParticleMaterial;
        public GameObject averageVelocityArrowPrefab;

        private float arrowEmitRate;
        public float ArrowEmitRate
        {
            get => arrowEmitRate;
            set
            {
                arrowEmitRate = value;
                itemsPerFrameConstant = emitOrder.Length / (ArrowEmitRate * 90);
            }
        }

        public float Threshold { get; set; }

        private float speed;
        public float Speed
        {
            get => speed;
            set
            {
                speed = value;
                if (particleSystem != null)
                {
                    var mainModule = particleSystem.main;
                    mainModule.simulationSpeed = speed;
                }
            }
        }

        [HideInInspector]
        public new ParticleSystem particleSystem;

        public Dictionary<Graph.GraphPoint, Vector3> Velocities
        {
            //get
            //{
            //    Dictionary<Graph.GraphPoint, Vector3> result = new Dictionary<Graph.GraphPoint, Vector3>(emitOrder.Length);
            //    foreach (var tuple in emitOrder)
            //    {
            //        result[tuple.Item1] = tuple.Item2;
            //    }
            //    return result;
            //}
            set
            {
                emitOrder = new Tuple<Graph.GraphPoint, Vector3>[value.Count];
                int i = 0;
                foreach (var kvp in value)
                {
                    emitOrder[i] = new Tuple<Graph.GraphPoint, Vector3>(kvp.Key, kvp.Value);
                    i++;
                }
                itemsPerFrameConstant = emitOrder.Length / (ArrowEmitRate * 90);
                greatestVelocity = Mathf.Sqrt(value.Max((kvp) => kvp.Value.sqrMagnitude));
            }
        }
        private Tuple<Graph.GraphPoint, Vector3>[] emitOrder = new Tuple<Graph.GraphPoint, Vector3>[0];

        private bool constantEmitOverTime = true;
        public bool ConstantEmitOverTime
        {
            get => constantEmitOverTime;
            set
            {
                constantEmitOverTime = value;
                if (!constantEmitOverTime)
                {
                    CancelInvoke();
                    InvokeRepeating("DoEmit", 0f, ArrowEmitRate);
                }
            }
        }

        private List<GameObject> averageVelocityGameObjects = new List<GameObject>();
        private float itemsPerFrameConstant;
        private bool useGraphPointColors;
        public bool UseGraphPointColors { get; set; }

        private float startSize = 1f;
        private bool useArrowParticle = true;
        public bool UseArrowParticle
        {
            get => useArrowParticle;
            set
            {
                useArrowParticle = value;
                if (particleSystem != null)
                {
                    ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null && !useArrowParticle)
                    {
                        startSize = 0.004f;
                        renderer.material = circleParticleMaterial;
                        renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    }
                    else
                    {
                        startSize = 1f;
                        renderer.material = arrowParticleMaterial;
                        renderer.renderMode = ParticleSystemRenderMode.Stretch;
                    }
                }
            }
        }

        public List<Tuple<Vector3, Vector3>> AverageVelocities = new List<Tuple<Vector3, Vector3>>();

        private int averageVelocitiesResolution = 8;
        public int AverageVelocitesResolution
        {
            get => averageVelocitiesResolution;
            set
            {
                averageVelocitiesResolution = value;
                EmitAverageVelocities = true;
            }
        }

        private bool emitAverageVelocities = false;
        public bool EmitAverageVelocities
        {
            get => emitAverageVelocities;
            set
            {

                AverageVelocities.Clear();
                foreach (GameObject arrow in averageVelocityGameObjects)
                {
                    Destroy(arrow);
                }
                averageVelocityGameObjects.Clear();

                if (value)
                {
                    float[,,] averageVelocitiesX = new float[averageVelocitiesResolution, averageVelocitiesResolution, averageVelocitiesResolution];
                    float[,,] averageVelocitiesY = new float[averageVelocitiesResolution, averageVelocitiesResolution, averageVelocitiesResolution];
                    float[,,] averageVelocitiesZ = new float[averageVelocitiesResolution, averageVelocitiesResolution, averageVelocitiesResolution];
                    int[,,] averageVelocitesNumberOfPoints = new int[averageVelocitiesResolution, averageVelocitiesResolution, averageVelocitiesResolution];

                    foreach (Tuple<Graph.GraphPoint, Vector3> v in emitOrder)
                    {
                        Vector3 pos = v.Item1.Position;
                        int indexX = (int)((pos.x + 0.5f) * AverageVelocitesResolution); // multiply with resolution and dispose of decimals
                        int indexY = (int)((pos.y + 0.5f) * AverageVelocitesResolution);
                        int indexZ = (int)((pos.z + 0.5f) * AverageVelocitesResolution);

                        averageVelocitiesX[indexX, indexY, indexZ] += v.Item2.x;
                        averageVelocitiesY[indexX, indexY, indexZ] += v.Item2.y;
                        averageVelocitiesZ[indexX, indexY, indexZ] += v.Item2.z;

                        // add up the number of points in each section for later
                        averageVelocitesNumberOfPoints[indexX, indexY, indexZ]++;
                    }

                    // offset is the length if half a section (sections are always cubes)
                    float offset = 1f / (AverageVelocitesResolution * 2f);
                    // the graphs have (0,0,0) as their center position, not a corner
                    offset -= 0.5f;

                    for (int i = 0; i < AverageVelocitesResolution; ++i)
                    {
                        for (int j = 0; j < AverageVelocitesResolution; ++j)
                        {
                            for (int k = 0; k < AverageVelocitesResolution; ++k)
                            {
                                Vector3 pos = new Vector3(
                                    (float)i / AverageVelocitesResolution + offset,
                                    (float)j / AverageVelocitesResolution + offset,
                                    (float)k / AverageVelocitesResolution + offset
                                );
                                // average out each section
                                int numPoints = averageVelocitesNumberOfPoints[i, j, k];
                                if (numPoints > 0)
                                {
                                    Vector3 averageVelocity = new Vector3(
                                        averageVelocitiesX[i, j, k] /= numPoints,
                                        averageVelocitiesY[i, j, k] /= numPoints,
                                        averageVelocitiesZ[i, j, k] /= numPoints
                                    );

                                    AverageVelocities.Add(new Tuple<Vector3, Vector3>(pos, averageVelocity));
                                }
                            }
                        }
                    }

                    foreach (Tuple<Vector3, Vector3> averageVelocity in AverageVelocities)
                    {

                        Vector3 pos = averageVelocity.Item1 + averageVelocity.Item2 / 2f;

                        GameObject newArrow = Instantiate(averageVelocityArrowPrefab, transform);
                        newArrow.transform.localPosition = pos;
                        newArrow.transform.LookAt(graph.transform.TransformPoint(averageVelocity.Item1 + averageVelocity.Item2));

                        // 0,0,0 is the middle of the shaft, local positions for the arrows subobjects must be halfed
                        float halfMagnitude = averageVelocity.Item2.magnitude / 2f;

                        Transform shaft = newArrow.transform.Find("arrow_shaft");
                        shaft.localScale = new Vector3(shaft.localScale.x, halfMagnitude, shaft.localScale.z);

                        Transform head = newArrow.transform.Find("arrow_head");
                        head.localPosition = new Vector3(0, 0, halfMagnitude);

                        averageVelocityGameObjects.Add(newArrow);
                    }
                }

                emitAverageVelocities = value;
            }

        }


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
            particleSystem.GetComponent<ParticleSystemRenderer>().material = arrowParticleMaterial;
            var mainModule = particleSystem.main;
            mainModule.simulationSpeed = speed;
            SetColors();
            Play();
            oldArrowEmitRate = ArrowEmitRate;
        }

        void DoEmit()
        {
            currentEmitCoroutine = StartCoroutine(DoEmitCoroutine());
        }

        private void Update()
        {
            if (oldArrowEmitRate != ArrowEmitRate)
            {
                oldArrowEmitRate = ArrowEmitRate;
                if (!constantEmitOverTime)
                {
                    CancelInvoke();
                    InvokeRepeating("DoEmit", 0f, ArrowEmitRate);
                }
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
            emitParams.startSize = startSize;

            int nItems = 0;
            int nextYield = ConstantEmitOverTime ? (int)itemsPerFrameConstant : itemsPerFrame;
            int yieldsSoFar = 0;
            float sqrThreshold = Threshold * Threshold;

            //if (ConstantEmitOverTime)
            //{
            //    for (int i = 0; i < emitOrder.Length - 2; ++i)
            //    {
            //        int j = UnityEngine.Random.Range(i, emitOrder.Length);
            //        var temp = emitOrder[i];
            //        emitOrder[i] = emitOrder[j];
            //        emitOrder[j] = temp;
            //    }
            //}

            do
            {
                //stopwatch.Restart();

                if (nextYield > emitOrder.Length)
                {
                    nextYield = emitOrder.Length;
                }
                // emit one round of particles
                for (; nItems < nextYield; ++nItems)
                {
                    var graphPoint = emitOrder[nItems];
                    if (graphPoint.Item2.sqrMagnitude > sqrThreshold)
                    {
                        emitParams.position = graphPoint.Item1.Position - offset; // + keyValuePair.Value.normalized * graphPointMeshSize;
                        emitParams.velocity = graphPoint.Item2;

                        if (UseGraphPointColors)
                        {
                            emitParams.startColor = graphPoint.Item1.GetColor();
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

            } while (nItems < emitOrder.Length);

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

        /// <summary>
        /// Starts the velocity particle system.
        /// </summary>
        public void Play()
        {
            var colorBySpeedModule = particleSystem.colorBySpeed;
            Vector2 range = new Vector2(0, greatestVelocity);
            colorBySpeedModule.range = range;

            if (ConstantEmitOverTime)
            {
                for (int i = 0; i < emitOrder.Length - 2; ++i)
                {
                    int j = UnityEngine.Random.Range(i, emitOrder.Length);
                    var temp = emitOrder[i];
                    emitOrder[i] = emitOrder[j];
                    emitOrder[j] = temp;
                }
            }

            if (constantEmitOverTime)
            {
                DoEmit();
            }
            else
            {
                InvokeRepeating("DoEmit", 0f, ArrowEmitRate);
            }
        }

        /// <summary>
        /// Stops the velocity particle system.
        /// </summary>
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
            if (graph.graphPointsInactive)
            {
                graph.ToggleGraphPoints();
            }
            emitting = false;
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
