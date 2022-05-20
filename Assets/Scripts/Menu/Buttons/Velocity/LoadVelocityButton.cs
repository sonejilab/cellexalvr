using CellexalVR.AnalysisObjects;
using TMPro;

namespace CellexalVR.Menu.Buttons.Velocity
{

    public class LoadVelocityButton : CellexalButton
    {
        public TextMeshPro buttonText;
        private string subGraphName;
        public string SubGraphName
        {
            get { return subGraphName; }
            set
            {
                subGraphName = value;
                if (value != "")
                {
                    buttonText.text = subGraphName;
                    subGraph = referenceManager.graphManager.FindGraph(value);
                }
            }
        }
        public string graphName;

        private string filePath;
        private Graph graph = null;
        private Graph subGraph = null;

        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;

                //summertwerk
                if (filePath.Contains(".")) //A .mds file
                    graphName = GetGraphName(value);
                else //Just the projection name
                    graphName = filePath.ToUpper();

                graph = referenceManager.graphManager.FindGraph(graphName);
                if (subGraphName != string.Empty)
                {
                    buttonText.text = subGraphName;
                }
                else
                {
                    buttonText.text = graphName;
                }
            }
        }

        private string GetGraphName(string path)
        {
            int lastSlashIndex = path.LastIndexOfAny(new char[] { '/', '\\' });
            int lastDotIndex = path.LastIndexOf('.');
            return path.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);

        }


        protected override string Description
        {
            get
            {
                return "Load " + buttonText.text;
            }
        }

        public override void Click()
        {
            var velocityGenerator = referenceManager.velocityGenerator;
            Graph graphToActivate = subGraph != null ? subGraph : graph;
            
            //velocityGenerator.ActiveGraphs.ForEach((g) => print("activegraphs: " + g));
            bool startVelocity = !velocityGenerator.ActiveGraphs.Contains(graphToActivate);
            if (startVelocity)
            {
                referenceManager.velocityGenerator.ReadVelocityFile(FilePath, subGraphName);
            }
            else
            {
                Destroy(graphToActivate.velocityParticleEmitter.gameObject);
                if (graphToActivate.graphPointsInactive)
                {
                    graphToActivate.ToggleGraphPoints();
                }
                velocityGenerator.ActiveGraphs.Remove(graphToActivate);
            }
            referenceManager.multiuserMessageSender.SendMessageReadVelocityFile(graphName, subGraphName, startVelocity);
            //referenceManager.velocitySubMenu.DeactivateOutlines();
            ToggleOutline(startVelocity);
            //activeOutline.SetActive(true);
        }

        public void DeactivateOutline()
        {
            ToggleOutline(false);
            //activeOutline.SetActive(false);
        }
    }
}
