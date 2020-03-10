using CellexalVR.General;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using CellexalVR.Filters;

namespace CellexalVR.AnalysisObjects
{

    /// <summary>
    /// Represents a gene expression histogram that is showed beside a graph.
    /// </summary>
    public class GeneExpressionHistogram : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject histogramParent;
        public TextMeshPro geneNameLabel;
        public TextMeshPro yAxisMaxLabel;
        public TextMeshPro xAxisMaxLabel;
        public TextMeshPro yAxisLabel;
        public GameObject barPrefab;
        public GameObject highlightArea;
        public TextMeshPro highlightAreaInfoText;
        public GameObject selectedArea;
        public TextMeshPro selectedAreaInfoText;
        public GameObject cutOffBarTopShape;
        public TextMeshPro filterTextLabel;

        /// <summary>
        /// Local position of the lower left corner of the histogram in the <see cref="LegendManager"/> local pos.
        /// </summary>
        public Vector3 HistogramMinPos { get; private set; }
        /// <summary>
        /// Local position of the upper right corner of the histogram.
        /// </summary>
        public Vector3 HistogramMaxPos { get; private set; }

        public int NumberOfBars { get => CellexalConfig.Config.GraphNumberOfExpressionColors + 1; }


        private int tallestBarsToSkip;
        private List<GameObject> cutOffTops = new List<GameObject>();

        /// <summary>
        /// The number of tallest bars to skip when scaling the Y axis. Call <see cref="RecreateHistogram"/> after changing this.
        /// </summary>
        public int TallestBarsToSkip
        {
            get => tallestBarsToSkip;
            set
            {
                if (value >= NumberOfBars)
                    tallestBarsToSkip = NumberOfBars - 1;
                else if (value < 0)
                    tallestBarsToSkip = 0;
                else
                    tallestBarsToSkip = value;
            }
        }

        /// <summary>
        /// The desired Y axis scaling mode. Call <see cref="RecreateHistogram"/> after changing this.
        /// </summary>
        public YAxisMode DesiredYAxisMode { get; set; }
        public enum YAxisMode { Linear, Logarithmic }

        private List<GameObject> bars = new List<GameObject>();
        private LegendManager manager;
        private bool attached;

        // local position of the center of the histogram
        private Vector3 center = new Vector3(0.0064f, -0.0129f, 0f);
        private float histogramHeight = 0.4f;
        private float histogramWidth = 0.4f;

        private List<HistogramData> tabData = new List<HistogramData>(10);
        private int currentTab = 0;
        public List<GameObject> tabButtons = new List<GameObject>(10);

        /// <summary>
        /// Holds all data needed to recreate a histogram.
        /// Can be used in <see cref="CreateHistogram(HistogramData)"/>.
        /// </summary>
        public class HistogramData
        {
            public string name;
            public string xAxisMaxLabel;
            public string yAxisMaxLabel;
            public string yAxisLabel;
            public List<float> barHeights;
            public int[] heightsInt;
            public YAxisMode yAxisMode;
            public int tallestBarsToSkip;




            /// <summary>
            /// Creates a new histogram data entry.
            /// </summary>
            /// <param name="geneName">The name of the gene the histogram covers.</param>
            /// <param name="heightsInt"></param>
            public HistogramData(string geneName, int[] heightsInt)
            {
                this.name = geneName;
                this.heightsInt = heightsInt;
            }

            /// <summary>
            /// Calculates the bar heights.
            /// </summary>
            /// <param name="yAxisMode">The desired <see cref="GeneExpressionHistogram.YAxisMode"/>.</param>
            /// <param name="tallestBarsToSkip">The number of tallest bar to skip when scaling the y-axis.</param>
            public void CalculateBarHeights(YAxisMode yAxisMode, int tallestBarsToSkip)
            {
                this.yAxisMode = yAxisMode;
                this.tallestBarsToSkip = tallestBarsToSkip;

                if (yAxisMode == YAxisMode.Linear)
                {
                    yAxisLabel = "Number of cells";
                }
                else
                {
                    yAxisLabel = "Log(Number of cells)";
                }

                int[] sortedHeightsInt = new int[heightsInt.Length];
                Array.Copy(heightsInt, sortedHeightsInt, heightsInt.Length);
                Array.Sort(sortedHeightsInt);
                float largestBin = (float)sortedHeightsInt[sortedHeightsInt.Length - tallestBarsToSkip - 1];
                barHeights = new List<float>(heightsInt.Length);
                if (yAxisMode == YAxisMode.Linear)
                {
                    for (int i = 0; i < heightsInt.Length; ++i)
                    {
                        barHeights.Add((float)heightsInt[i] / largestBin);
                    }
                    yAxisMaxLabel = largestBin.ToString();
                }
                else
                {
                    for (int i = 0; i < heightsInt.Length; ++i)
                    {
                        if (heightsInt[i] == 0)
                        {
                            barHeights.Add(0f);
                        }
                        else
                        {
                            barHeights.Add(Mathf.Log(heightsInt[i]));
                        }
                    }

                    largestBin = Mathf.Log(largestBin);

                    for (int i = 0; i < barHeights.Count; ++i)
                    {
                        barHeights[i] = barHeights[i] / largestBin;
                    }
                    yAxisMaxLabel = largestBin.ToString();

                }
            }
        }

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
            manager = gameObject.GetComponentInParent<LegendManager>();
            HistogramMinPos = new Vector3(center.x - histogramWidth / 2f, center.y - histogramHeight / 2f, 0f);
            HistogramMaxPos = new Vector3(center.x + histogramWidth / 2f, center.y + histogramHeight / 2f, 0f);
            CellexalEvents.LegendAttached.AddListener(ActivateExtraColumn);

            for (int i = 0; i < 10; ++i)
            {
                tabData.Add(null);
            }
        }

        public void ClearLegend()
        {
            for (int i = 0; i < tabData.Count; ++i)
            {
                tabData[i] = null;
            }

            foreach (GameObject go in bars)
            {
                Destroy(go);
            }
            bars.Clear();

            foreach (GameObject go in cutOffTops)
            {
                Destroy(go);
            }
            cutOffTops.Clear();

            foreach (var tabButton in tabButtons)
            {
                tabButton.GetComponentInChildren<TextMeshPro>().text = "";
            }
            tabButtons[currentTab].GetComponent<CellexalVR.Menu.Buttons.CellexalButton>().meshStandardColor = Color.black;

            geneNameLabel.text = "";
            DeactivateHighlightArea();
            DeactivateSelectedArea();



        }

        private void ActivateExtraColumn()
        {
            attached = true;
        }

        private void DeactivateExtraColumn()
        {
            attached = false;
        }

        /// <summary>
        /// Creates a new histogram with the specifed barheights.
        /// </summary>
        /// <param name="geneName">The name of the gene</param>
        /// <param name="bins">An array of ints that represents the heights of the bars.</param>
        /// <param name="xAxisMaxLabel">The label at the max value of the x-axis</param>
        /// <param name="barHeightMode">The Y axis mode to use.</param>
        /// <param name="skip">The number of tallest bars to cut, so the n:th tallest bar uses up all of the y axis.</param>
        public void CreateHistogram(string geneName, int[] bins, string xAxisMaxLabel, YAxisMode barHeightMode, int skip = 0)
        {
            DesiredYAxisMode = barHeightMode;
            TallestBarsToSkip = skip;

            HistogramData newData = new HistogramData(geneName, bins);
            newData.CalculateBarHeights(DesiredYAxisMode, TallestBarsToSkip);

            CreateHistogram(newData);

            // make sure the order matches the previous searches list
            if (!tabData.Any((HistogramData data) => data != null && data.name == geneName))
            {
                var locks = referenceManager.previousSearchesList.searchLocks;
                HistogramData next = newData;
                for (int i = 0; i < locks.Count && i < tabData.Count; ++i)
                {
                    if (!locks[i].Locked && next != null)
                    {
                        HistogramData temp = tabData[i];
                        tabData[i] = next;
                        tabButtons[i].GetComponentInChildren<TextMeshPro>().text = next.name;
                        next = temp;
                    }
                }
                if (tabData.Count < 10 && next != null)
                {
                    tabData.Add(next);
                    tabButtons[tabData.Count - 1].GetComponentInChildren<TextMeshPro>().text = next.name;
                }
            }
            SwitchToTab(0);
        }

        private void CreateHistogram(HistogramData data)
        {

            this.geneNameLabel.text = data.name;
            if (bars.Count != data.barHeights.Count)
            {
                InstantiateBars(data.barHeights.Count);
            }

            foreach (GameObject go in cutOffTops)
            {
                Destroy(go);
            }
            cutOffTops.Clear();

            for (int i = 0; i < NumberOfBars; ++i)
            {
                Color color = referenceManager.graphGenerator.geneExpressionColors[i];

                GameObject bar = bars[i];
                bar.GetComponent<Renderer>().material.color = color;
                bar.transform.localRotation = Quaternion.identity;
                float height = Mathf.Min(data.barHeights[i], 1f);

                var pos = BarPos(i, i, height, NumberOfBars);
                Vector3 currentScale = bar.transform.localScale;
                var scale = BarScale(i, i, height, NumberOfBars, currentScale.z);

                if (data.barHeights[i] > 1f)
                {
                    GameObject cutOffGameObject = Instantiate(cutOffBarTopShape);
                    cutOffGameObject.transform.parent = histogramParent.transform;
                    cutOffGameObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                    cutOffGameObject.transform.localPosition = new Vector3(pos.x, pos.y + scale.y / 2f, pos.z + 8e-05f);

                    cutOffGameObject.transform.localScale = new Vector3(scale.x * 100f, scale.z * 100f, 1f);
                    cutOffGameObject.GetComponent<Renderer>().material.color = color;
                    cutOffTops.Add(cutOffGameObject);
                }

                bar.transform.localPosition = pos;
                bar.transform.localScale = scale;
            }

            yAxisMaxLabel.text = data.yAxisMaxLabel;
            xAxisMaxLabel.text = data.xAxisMaxLabel;
            yAxisLabel.text = data.yAxisLabel;

        }

        /// <summary>
        /// Recreates the current histogram. Should be used after <see cref="DesiredYAxisMode"/> or <see cref="TallestBarsToSkip"/> have been changed.
        /// </summary>
        public void RecreateHistogram()
        {
            if (tabData[currentTab] != null)
            {
                tabData[currentTab].CalculateBarHeights(DesiredYAxisMode, TallestBarsToSkip);
                CreateHistogram(tabData[currentTab]);
            }
        }

        /// <summary>
        /// Calculates a bars position. A bar can span many indices.
        /// </summary>
        /// <param name="startIndex">The left index of the bar</param>
        /// <param name="endIndex">The right index of the bar</param>
        /// <param name="height">The bars height, range [0, 1]</param>
        /// <param name="nBars">The total number of bars in the histogram</param>
        /// <returns>The bar's local position</returns>
        private Vector3 BarPos(int startIndex, int endIndex, float height, int nBars)
        {
            float meanIndex = (endIndex + startIndex) / 2f;
            float xCoord = center.x + histogramWidth * ((meanIndex + 0.5f) / nBars) - histogramWidth / 2f;
            float yCoord = center.y + histogramHeight * (height - 1f) / 2f;
            return new Vector3(xCoord, yCoord, 0f);
        }

        /// <summary>
        /// Calculates a bars scale. A bar can span many indices.
        /// </summary>
        /// <param name="startIndex">The left index of the bar</param>
        /// <param name="endIndex">The right index of the bar</param>
        /// <param name="height">The bars height, range [0, 1]</param>
        /// <param name="nBars">The total number of bars in the histogram</param>
        /// <param name="zScale">The desired z-component of the scale</param>
        /// <returns>The bar's local scale</returns>
        private Vector3 BarScale(int startIndex, int endIndex, float height, int nBars, float zScale)
        {
            float xScale = histogramWidth / nBars * (endIndex - startIndex + 1);
            float yScale = histogramHeight * height;
            return new Vector3(xScale, yScale, zScale);
        }

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
                int numberOfBarsToRemove = currentNumberOfBars - numberOfBars;
                for (int i = 0; i < numberOfBarsToRemove; ++i)
                {
                    Destroy(bars[bars.Count - 1 - i]);
                }
                bars.RemoveRange(bars.Count - numberOfBarsToRemove, numberOfBarsToRemove);
            }
        }

        /// <summary>
        /// Switches to another previously generated histogram.
        /// </summary>
        /// <param name="index">The tab to switch to.</param>
        public void SwitchToTab(int index)
        {
            HistogramData data = tabData[index];
            if (data != null)
            {
                tabButtons[currentTab].GetComponent<CellexalVR.Menu.Buttons.CellexalButton>().meshStandardColor = Color.black;
                tabButtons[index].GetComponent<CellexalVR.Menu.Buttons.CellexalButton>().meshStandardColor = new Color(0.1411f, 0.6588f, 0.6385f);
                currentTab = index;
                TallestBarsToSkip = data.tallestBarsToSkip;
                DesiredYAxisMode = data.yAxisMode;
                CreateHistogram(data);
                DeactivateSelectedArea();
            }
        }

        /// <summary>
        /// Deactivates the highlight area and its accompanying text.
        /// </summary>
        public void DeactivateHighlightArea()
        {
            highlightArea.SetActive(false);
            highlightAreaInfoText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Deactivates the selected highlight area and its accompanying text.
        /// </summary>
        public void DeactivateSelectedArea()
        {
            selectedArea.SetActive(false);
            selectedAreaInfoText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Moves a highlight area and its text.
        /// </summary>
        /// <param name="area">The highlight area gameObject</param>
        /// <param name="text">The highlight area's text</param>
        /// <param name="minX">The left index of the highlight area</param>
        /// <param name="maxX">The right index of the highlight area</param>
        private void MoveArea(GameObject area, TextMeshPro text, int minX, int maxX)
        {
            if (bars.Count == 0)
            {
                return;
            }

            if (minX > maxX)
            {
                int temp = minX;
                minX = maxX;
                maxX = temp;
            }

            if (minX < 0)
            {
                minX = 0;
            }

            if (maxX >= NumberOfBars)
            {
                maxX = NumberOfBars - 1;
            }

            if (!area.activeSelf)
            {
                area.SetActive(true);
                text.gameObject.SetActive(true);
            }
            Vector3 pos = BarPos(minX, maxX, 1f, bars.Count);
            Vector3 scale = BarScale(minX, maxX, 1f, bars.Count, 0.01f);
            area.transform.localPosition = pos;
            area.transform.localScale = scale;
            Vector3 infoTextPos = new Vector3(pos.x + scale.x / 2f, scale.y / 2f, -scale.z);
            text.transform.localPosition = infoTextPos;

            int[] selectedSlice = new int[maxX - minX + 1];
            var data = tabData[currentTab];
            Array.Copy(data.heightsInt, minX, selectedSlice, 0, selectedSlice.Length);
            Array.Sort(selectedSlice);
            double mean = selectedSlice.Average();
            float median;
            int middleIndex = selectedSlice.Length / 2;
            if (selectedSlice.Length % 2 == 0)
            {
                median = (selectedSlice[middleIndex] + selectedSlice[middleIndex - 1]) / 2f;
            }
            else
            {
                median = selectedSlice[middleIndex];
            }

            if (minX != maxX)
            {
                int sum = 0;
                for (int i = minX; i <= maxX; ++i)
                {
                    sum += data.heightsInt[i];
                }
                highlightAreaInfoText.text = "x: [" + minX + ", " + maxX + "]\nsum: " + sum + "\nmean: " + mean + "\nmedian: " + median;
            }
            else
            {
                highlightAreaInfoText.text = "x: " + minX + "\ny: " + data.heightsInt[minX];
            }
        }

        /// <summary>
        /// Moves and resizes the highlight area to span between two bars.
        /// </summary>
        /// <param name="minX">The index of the left bar</param>
        /// <param name="maxX">The index of the right bar</param>
        public void MoveHighlightArea(int minX, int maxX)
        {
            MoveArea(highlightArea, highlightAreaInfoText, minX, maxX);
        }

        /// <summary>
        /// Moves and resizes the selected highlight area to span between two bars.
        /// </summary>
        /// <param name="minX">The index of the left bar</param>
        /// <param name="maxX">The index of the right bar</param>
        public void MoveSelectedArea(int minX, int maxX)
        {
            selectedAreaInfoText.text = highlightAreaInfoText.text;
            MoveArea(selectedArea, selectedAreaInfoText, minX, maxX);
            if (!attached) return;
            if (minX > maxX)
            {
                int temp = minX;
                minX = maxX;
                maxX = temp;
            }

            float.TryParse(xAxisMaxLabel.text, NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out float value);
            referenceManager.cullingFilterManager.AddGeneFilter(geneNameLabel.text, minX, maxX,
                value);
        }

    }
}
