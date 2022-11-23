namespace CellexalVR.Menu.Buttons.ColorPicker
{
    public class RemoveColorButton : CellexalButton
    {
        public ColorPickerPopout colorPickerPopout;
        protected override string Description => "Remove Color";

        public override void Click()
        {
            colorPickerPopout.RemoveColor();
        }
    }
}