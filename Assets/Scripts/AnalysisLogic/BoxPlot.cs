using CellexalVR.General;
using CellexalVR.Interaction;
using TMPro;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    public class BoxPlot : CellexalRaycastable
    {
        public GameObject box;
        public GameObject medianLine;
        public GameObject verticalWhiskerLine;
        public GameObject lowerWhisker;
        public GameObject upperWhisker;
        public TextMeshPro facsName;
        public GameObject textParent;
        public TextMeshPro[] infoTexts;

        [HideInInspector] public float median;
        [HideInInspector] public float percentile5th;
        [HideInInspector] public float percentile95th;
        [HideInInspector] public float minValue;
        [HideInInspector] public float maxValue;
        [HideInInspector] public string facsNameString;


        private void OnValidate()
        {
            if (gameObject.scene.IsValid() && gameObject.activeSelf)
            {
                canBePushedAndPulled = false;
                //infoTexts = new TextMeshPro[5];
                //infoTexts[0] = gameObject.transform.Find("InfoTexts/Texts/UpperWhiskerText").GetComponent<TextMeshPro>();
                //infoTexts[1] = gameObject.transform.Find("InfoTexts/Texts/95thPercentileText").GetComponent<TextMeshPro>();
                //infoTexts[2] = gameObject.transform.Find("InfoTexts/Texts/MedianText").GetComponent<TextMeshPro>();
                //infoTexts[3] = gameObject.transform.Find("InfoTexts/Texts/5thPercentileText").GetComponent<TextMeshPro>();
                //infoTexts[4] = gameObject.transform.Find("InfoTexts/Texts/LowerWhiskerText").GetComponent<TextMeshPro>();
            }
        }

        /// <summary>
        /// Sets the values of this boxplot so it can be resized with <see cref="ResizeComponents(float, float)"/> later.
        /// </summary>
        /// <param name="facsName">The name of the facs that this boxplot represents.</param>
        /// <param name="median">The median value.</param>
        /// <param name="percentile5th">The 5th percentage value.</param>
        /// <param name="percentile95th">The 95th percentage value.</param>
        /// <param name="minValue">The minimum extreme value.</param>
        /// <param name="maxValue">The maximum extreme value.</param>
        public void InitBoxPlot(string facsName, float median, float percentile5th, float percentile95th, float minValue, float maxValue)
        {
            this.median = median;
            this.percentile5th = percentile5th;
            this.percentile95th = percentile95th;
            this.minValue = minValue;
            this.maxValue = maxValue;
            facsNameString = facsName;
            gameObject.name = "BoxPlot " + facsName;
            SetInfoText();
            base.OnActivate.RemoveAllListeners();
            base.OnActivate.AddListener(() => ReferenceManager.instance.cellManager.ColorByIndex(facsNameString));
        }

        /// <summary>
        /// Resizes this boxplot based on the global min and max value of all boxplots in the grid.
        /// </summary>
        /// <param name="globalMinValue">The global minimum extreme value.</param>
        /// <param name="globalMaxValue">The global maximum extreme value.</param>
        public void ResizeComponents(float globalMinValue, float globalMaxValue)
        {
            Vector3 plotHalfSize = new Vector3(0.05f, 0.05f, 0f);
            // the lower left corner of the box plot's area
            Vector3 plotMinPosition = transform.localPosition - plotHalfSize;
            // the upper right corner of the plot's area
            Vector3 plotMaxPosition = transform.localPosition + plotHalfSize;
            Vector3 oldPos;
            Vector3 oldScale;

            facsName.text = facsNameString;

            // draw line at median
            float yScaleFactor = (plotMaxPosition.y - plotMinPosition.y) / (globalMaxValue - globalMinValue);
            float medianYCoord = (median - globalMinValue) * yScaleFactor - plotHalfSize.y;
            oldPos = medianLine.transform.localPosition;
            medianLine.transform.localPosition = new Vector3(oldPos.x, medianYCoord, oldPos.z);

            // draw box between 5th and 95th percentiles
            float percentile95thYCoord = (percentile95th - globalMinValue) * yScaleFactor - plotHalfSize.y;
            float percentile5thYCoord = (percentile5th - globalMinValue) * yScaleFactor - plotHalfSize.y;
            oldPos = box.transform.localPosition;
            oldScale = box.transform.localScale;
            box.transform.localPosition = new Vector3(oldPos.x, (percentile95thYCoord + percentile5thYCoord) / 2f, oldPos.z);
            box.transform.localScale = new Vector3(oldScale.x, (percentile95thYCoord - percentile5thYCoord), oldScale.z);

            // draw whiskers between minimum and maximum values
            float lowerWhiskerYCoord = (minValue - globalMinValue) * yScaleFactor - plotHalfSize.y;
            oldPos = lowerWhisker.transform.localPosition;
            lowerWhisker.transform.localPosition = new Vector3(oldPos.x, lowerWhiskerYCoord, oldPos.z);
            float upperWhiskerYCoord = (maxValue - globalMinValue) * yScaleFactor - plotHalfSize.y;
            oldPos = upperWhisker.transform.localPosition;
            upperWhisker.transform.localPosition = new Vector3(oldPos.x, upperWhiskerYCoord, oldPos.z);

            oldPos = verticalWhiskerLine.transform.localPosition;
            verticalWhiskerLine.transform.localPosition = new Vector3(oldPos.x, (upperWhiskerYCoord + lowerWhiskerYCoord) / 2f, oldPos.z);
            oldScale = verticalWhiskerLine.transform.localScale;
            verticalWhiskerLine.transform.localScale = new Vector3(oldScale.x, (upperWhiskerYCoord - lowerWhiskerYCoord), oldScale.z);

        }

        /// <summary>
        /// Activates or deactivates the info text box.
        /// </summary>
        /// <param name="active">True if the info box should be activated, false if it should be deactivated.</param>
        public void SetInfoTextActive(bool active)
        {
            textParent.SetActive(active);
        }

        /// <summary>
        /// Sets the info texts according to the values from <see cref="InitBoxPlot(string, float, float, float, float, float)"/>.
        /// </summary>
        private void SetInfoText()
        {
            infoTexts[0].text = maxValue.ToString("F6");
            infoTexts[1].text = percentile95th.ToString("F6");
            infoTexts[2].text = median.ToString("F6");
            infoTexts[3].text = percentile5th.ToString("F6");
            infoTexts[4].text = minValue.ToString("F6");
        }

        public override void OnRaycastEnter()
        {
            base.OnRaycastEnter();
            SetInfoTextActive(true);
        }

        public override void OnRaycastExit()
        {
            base.OnRaycastExit();
            SetInfoTextActive(false);
        }
    }
}