using CellexalVR.Spatial;

namespace CellexalVR.Menu.Buttons
{

    public class ScrollSuggestionsButton : CellexalButton
    {
        public int dir;

        protected override string Description => "Scroll " + (dir > 0 ? "Up" : "Down");

        public override void Click()
        {
            AllenReferenceBrain.instance.ScrollSuggestions(dir);
        }

    }
}
