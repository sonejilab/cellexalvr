using TMPro;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ChangeNrOfSlicesButton : CellexalButton
    {
        public int dir;
        public TextMeshProUGUI label;
        protected override string Description => "Change nr of slices " + dir;

        private SlicingMenu slicingMenu;

        private void Start()
        {
            slicingMenu = GetComponentInParent<SlicingMenu>();
        }


        public override void Click()
        {
            slicingMenu.ChangeNrOfSlices(dir);
            label.text = slicingMenu.nrOfSlices.ToString();
        }
    }
}