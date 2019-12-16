using CellexalVR.General;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneExpressionHistogram : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject histogramParent;
    public TextMeshPro yAxisMaxLabel;
    public TextMeshPro xAxisMaxLabel;
    public GameObject barPrefab;

    private List<GameObject> bars = new List<GameObject>();
    private Vector3 startPos;

    // local position of the center of the histogram
    private Vector3 center = new Vector3(0.0064f, -0.0129f, 0f);
    private float histogramHeight = 0.4f;
    private float histogramWidth = 0.4f;

    private void OnValidate()
    {
        if (gameObject.scene.IsValid())
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
    }

    private void Awake()
    {
        if (!referenceManager)
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
    }

    /// <summary>
    /// Creates a new histogram with the specifed barheights.
    /// This function will remove the previous histogram.
    /// </summary>
    /// <param name="barHeights">The height of each bar in the range [0, 1]</param>
    /// <param name="yAxisMaxLabel">The label at the max value of the y-axis</param>
    //public void CreateHistogram(List<float> barHeights, string yAxisMaxLabel, string xAxisMaxLabel)
    //{
    //    this.yAxisMaxLabel.text = yAxisMaxLabel;
    //    this.xAxisMaxLabel.text = xAxisMaxLabel;

    //    if (bars.Count != barHeights.Count)
    //    {
    //        InstantiateBars(barHeights.Count);
    //    }

    //    int numberOfBars = bars.Count;
    //    float barWidth = histogramWidth / numberOfBars;


    //    for (int i = 0; i < numberOfBars; ++i)
    //    {

    //        Color color = referenceManager.graphGenerator.geneExpressionColors[i];

    //        GameObject bar = bars[i];
    //        bar.GetComponent<Renderer>().material.color = color;
    //        bar.transform.localRotation = Quaternion.identity;

    //        float xCoord = center.x + barWidth * (i - (numberOfBars / 2));
    //        float yCoord = center.y + (histogramHeight * barHeights[i] - histogramHeight) / 2f;
    //        bar.transform.localPosition = new Vector3(xCoord, yCoord, center.z);

    //        Vector3 currentScale = bar.transform.localScale;
    //        bar.transform.localScale = new Vector3(barWidth, histogramHeight * barHeights[i], currentScale.z);
    //    }
    //}

    /// <summary>
    /// Instantiates or destroys the bars until the desired number of bars are left.
    /// </summary>
    /// <param name="numberOfBars">The desired number of bars to have.</param>
    private void InstantiateBars(int numberOfBars)
    {
        int currentNumberOfBars = bars.Count;
        if (numberOfBars > currentNumberOfBars)
        {
            // if we are missing bars
            for (int i = currentNumberOfBars; i < numberOfBars; ++i)
            {
                GameObject newBar = Instantiate(barPrefab);
                newBar.transform.parent = histogramParent.transform;
                newBar.SetActive(true);
                bars.Add(newBar);
            }
        }
        else if (numberOfBars < currentNumberOfBars)
        {
            int numberOfBarsToRemove = numberOfBars - currentNumberOfBars;
            for (int i = 0; i < numberOfBarsToRemove; ++i)
            {
                Destroy(bars[bars.Count - 1 - i]);
            }
            bars.RemoveRange(bars.Count - numberOfBarsToRemove, numberOfBarsToRemove);

        }
    }

}
