using CellexalVR.AnalysisLogic;
using SQLiter;
namespace CellexalVR.Menu.Buttons.Coloring
{
    /// <summary>
    /// Represents the button on the <see cref="ColorByGeneMenu"/> that queries the database for the most differentially expressed genes.
    /// </summary>
    public class QueryTopDiffGenesButton : CellexalButton
    {

        public SQLite.QueryTopGenesRankingMode mode;

        private CellManager cellmanager;

        protected override string Description
        {
            get
            {
                return this.gameObject.name;
            }
        }

        private void Start()
        {
            cellmanager = referenceManager.cellManager;
        }

        public override void Click()
        {
            cellmanager.QueryTopGenes(mode);
        }
    }
}