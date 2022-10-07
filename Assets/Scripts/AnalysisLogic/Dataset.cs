using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections.Generic;

namespace Assets.Scripts.AnalysisLogic
{
    /// <summary>
    /// Represents a loaded dataset, as well as everything that has been created from the original graphs.
    /// </summary>
    public class Dataset
    {
        public static Dataset instance = new Dataset();

        public ReferenceManager referenceManager;
        /// <summary>
        /// A full path to where the source files for this dataset is.
        /// </summary>
        public string sourceFolder;
        /// <summary>
        /// The database containing the gene expression information.
        /// </summary>
        public SQLiter.SQLite database;
        /// <summary>
        /// Contains the original graphs.
        /// </summary>
        public List<Graph> graphs = new List<Graph>();
        /// <summary>
        /// Contains all current heatmaps that have been generated.
        /// </summary>
        public List<Heatmap> heatmaps = new List<Heatmap>();
        /// <summary>
        /// Contains all current networks that have been generated.
        /// </summary>
        public List<NetworkHandler> networks = new List<NetworkHandler>();
        /// <summary>
        /// Contains all current extra graphs that have been generated.
        /// </summary>
        public List<Graph> generatedGraphs = new List<Graph>();
        /// <summary>
        /// This dataset's legend.
        /// </summary>
        public LegendManager legend = new LegendManager();

        public void HighlightGene(string genename)
        {
            foreach (Heatmap heatmap in heatmaps)
            {
                heatmap.HighLightGene(genename);
            }
            foreach (NetworkHandler network in networks)
            {
                network.HighLightGene(genename);
            }
        }

        public void ColorByGeneExpression(string genename)
        {
            referenceManager.cellManager.ColorGraphsByGene(genename);
        }
    }
}
