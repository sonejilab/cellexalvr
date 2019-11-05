using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisLogic;

namespace CellexalVR.Filters
{

    /// <summary>
    /// The result block of the filter creator in VR. All blocks must connect through and/or/not/xor blocks and end up in one connection ton this block. Has one input.
    /// </summary>
    public class FilterCreatorResultBlock : FilterCreatorBlock
    {
        public GameObject block;
        public FilterCreatorBlockPort input;
        public TMPro.TextMeshPro loadingText;
        public Color loadingTextFilterColor;
        public Color loadingTextDoneColor;
        public Color loadingTextInvalidColor;

        private Coroutine runningCoroutine;

        public override bool IsValid()
        {
            return input.connectedTo != null && input.connectedTo.parent.IsValid();
        }

        public override string ToString()
        {
            if (input.connectedTo != null)
            {
                return input.connectedTo.parent.ToString();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Turns this block and all child blocks into the corresponding filter, which can be used by the <see cref="SelectionManager"/>.
        /// </summary>
        /// <returns>The <see cref="Filter"/> that this configuration of blocks represents. Or null if it is not a valid filter (see <see cref="IsValid"/>).</returns>
        public Filter ToFilter()
        {
            if (!IsValid())
            {
                return null;
            }

            Filter filter = new Filter();
            filter.Expression = input.ToExpr();
            filter.Expression.SetFilterManager(filterManager);
            return filter;
        }

        public override BooleanExpression.Expr ToExpr()
        {
            return input.ToExpr();
        }

        public enum LoadingTextState { LOADING, FINISHED, INVALID_FILTER, OFF, FILTER_SAVED }

        /// <summary>
        /// Sets the loading text on the block according to a state.
        /// </summary>
        /// <param name="state">The state that we are in.</param>
        public void SetLoadingTextState(LoadingTextState state)
        {
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
            }
            loadingText.gameObject.SetActive(true);
            switch (state)
            {
                case LoadingTextState.LOADING:
                    loadingText.color = loadingTextFilterColor;
                    loadingText.text = "Loading filter";
                    break;
                case LoadingTextState.FINISHED:
                    loadingText.color = loadingTextDoneColor;
                    loadingText.text = "Loading done";
                    if (gameObject.activeInHierarchy)
                    {
                        runningCoroutine = StartCoroutine(DisableLoadingTextAfterTimeCoroutine(3f));
                    }
                    break;
                case LoadingTextState.OFF:
                    loadingText.gameObject.SetActive(false);
                    break;
                case LoadingTextState.INVALID_FILTER:
                    loadingText.color = loadingTextInvalidColor;
                    loadingText.text = "Invalid filter";
                    if (gameObject.activeInHierarchy)
                    {
                        runningCoroutine = StartCoroutine(DisableLoadingTextAfterTimeCoroutine(3f));
                    }
                    break;
                case LoadingTextState.FILTER_SAVED:
                    loadingText.color = loadingTextDoneColor;
                    loadingText.text = "Filter saved";
                    break;
            }
        }

        private IEnumerator DisableLoadingTextAfterTimeCoroutine(float time)
        {
            yield return new WaitForSeconds(time);
            loadingText.gameObject.SetActive(false);
            runningCoroutine = null;
        }

        public override void SetCollidersActivated(bool activate) { }
    }
}
