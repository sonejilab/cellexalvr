/// <summary>
/// Represents the buttons that increase and decrease the number of frames between each gene expression when flashing genes.
/// </summary>
namespace Assets.Scripts.MenuButtonScripts
{
    class ChangeFramesBetweenFlashingGenesButton : CellexalButton
    {
        protected override string Description
        {
            get { return change > 0 ? "Slow down flashing speed" : "Speed up flashing speed"; }
        }
        [UnityEngine.Tooltip("The change in number of frames the cellmanager waits before displaying a new gene expression when flashing genes.")]
        public int change;

        private CellManager cellManager;

        private void Start()
        {
            cellManager = referenceManager.cellManager;
        }

        protected override void Click()
        {
            cellManager.FramesBetweenEachFlash += change;
        }
    }
}
