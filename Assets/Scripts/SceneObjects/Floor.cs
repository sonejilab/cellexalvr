using UnityEngine;
using System.Collections;
using CellexalVR.General;

public class Floor : MonoBehaviour
{

    public Material gridMaterial;

    private bool pulseToggle;

    // Use this for initialization
    void Start()
    {
        CellexalEvents.GraphsLoaded.AddListener(StartWave);
        CellexalEvents.GraphsColoredByGene.AddListener(StartWave);
        CellexalEvents.GraphsColoredByIndex.AddListener(StartWave);
        CellexalEvents.HeatmapCreated.AddListener(StartWave);
        CellexalEvents.NetworkCreated.AddListener(StartWave);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    StartWave();
        //}
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    StartPulse();
        //}

    }

    public void StartPulse()
    {
        gridMaterial.SetFloat("_PulseToggle", 1.0f);
    }

    public void StopPulse()
    {
        gridMaterial.SetFloat("_PulseToggle", 0.0f);
    }

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
}
