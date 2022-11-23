namespace CellexalVR.Menu.Buttons.ColorPicker
{
    public class FinalizeColorPickerChoiceButton : CellexalButton
    {
        public ColorPickerPopout colorPickerPopout;

        protected override string Description => "Choose Colour";

        public override void Click()
        {
            colorPickerPopout.FinalizeChoice();
        }
    }
}
