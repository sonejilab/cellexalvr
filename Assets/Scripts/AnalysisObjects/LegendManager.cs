using UnityEngine;
using System.Collections;
using CellexalVR.General;

public class LegendManager : MonoBehaviour
{
    public GameObject backgroundPlane;
    public AttributeLegend attributeLegend;
    public GeneExpressionHistogram geneExpressionHistogram;

    public enum Legend { AttributeLegend, GeneExpressionLegend }

    private void Start()
    {
        CellexalEvents.GraphsReset.AddListener(DeactivateLegends);
    }

    public void DeactivateLegends()
    {
        backgroundPlane.SetActive(false);
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void ActivateLegend(Legend legendToActivate)
    {
        DeactivateLegends();
        backgroundPlane.SetActive(true);

        switch (legendToActivate)
        {
            case Legend.AttributeLegend:
                attributeLegend.gameObject.SetActive(true);
                break;
            case Legend.GeneExpressionLegend:
                geneExpressionHistogram.gameObject.SetActive(true);
                break;
        }
    }
}
