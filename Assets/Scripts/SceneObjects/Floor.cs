using CellexalVR.General;
using System.Collections;
using System.IO;
using UnityEngine;

namespace CellexalVR.SceneObjects
{

    public class Floor : MonoBehaviour
    {

        public Material gridMaterial;

        // Use this for initialization
        private void Start()
        {
            CellexalEvents.GraphsLoaded.AddListener(StartWave);
            CellexalEvents.GraphsLoaded.AddListener(StopPulse);
            CellexalEvents.ScarfObjectLoaded.AddListener(StartWave);
            CellexalEvents.GraphsColoredByGene.AddListener(StartWave);
            CellexalEvents.GraphsColoredByIndex.AddListener(StartWave);
            CellexalEvents.ScriptFinished.AddListener(() => StartCoroutine(ScriptFinished()));
        }

        /// <summary>
        /// Starts the pulse that goes outwards from the play area.
        /// </summary>
        public void StartPulse()
        {
            gridMaterial.SetFloat("_PulseToggle", 1.0f);
        }

        /// <summary>
        /// Stops the pulse that goes outwards from the play area.
        /// </summary>
        public void StopPulse()
        {
            gridMaterial.SetFloat("_PulseToggle", 0.0f);
        }

        /// <summary>
        /// Starts the wave that circles around the play area.
        /// </summary>
        public void StartWave()
        {
            //gridMaterial.SetFloat("_PulseToggle", 0.0f);
            StartCoroutine(WaveCoroutine());
        }

        private IEnumerator WaveCoroutine()
        {
            float t = 0f;
            float pulseDuration = gridMaterial.GetFloat("_WaveDuration");
            float pulseSpeed = gridMaterial.GetFloat("_WaveSpeed");

            while (t < 1f)
            {
                gridMaterial.SetFloat("_WaveStartTime", t);
                t += Time.deltaTime * pulseSpeed / pulseDuration;
                yield return null;
            }

            gridMaterial.SetFloat("_WaveStartTime", -1f);
        }

        private IEnumerator ScriptFinished()
        {
            float waitTime = 0f;
            while (waitTime < 1.0f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
            if (!File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock")))
            {
                StopPulse();
                StartWave();
            }
        }
    }
}
