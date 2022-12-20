using CellexalVR.AnalysisLogic;

namespace CellexalVR.Menu.Buttons.Facs
{
    public class ChangeBoxPlotSortingButton : CellexalButton
    {

        public BoxPlotGrid.SortOrder sortBy;

        protected override string Description
        {
            get
            {
                string beginning = "Sort boxplots by ";
                return sortBy switch
                {
                    BoxPlotGrid.SortOrder.DEFAULT => beginning + "default order",
                    BoxPlotGrid.SortOrder.MEDIAN => beginning + "median",
                    BoxPlotGrid.SortOrder.BOX_HEIGHT => beginning + "box heights",
                    _ => "",
                };
            }
        }

        public override void Click()
        {
            switch (sortBy)
            {
                case BoxPlotGrid.SortOrder.DEFAULT:
                    GetComponentInParent<BoxPlotGrid>().SortBoxPlotsByDefault();
                    break;
                case BoxPlotGrid.SortOrder.MEDIAN:
                    GetComponentInParent<BoxPlotGrid>().SortBoxPlotsByMedian();
                    break;
                case BoxPlotGrid.SortOrder.BOX_HEIGHT:
                    GetComponentInParent<BoxPlotGrid>().SortBoxPlotsByHeight();
                    break;
            }
        }

    }
}