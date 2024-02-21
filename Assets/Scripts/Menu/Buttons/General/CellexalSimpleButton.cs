namespace CellexalVR.Menu.Buttons.General
{
    /// <summary>
    /// A simple button that only executes the function set in the <see cref="CellexalVR.Interaction.CellexalRaycastable.OnActivate"/> event, which is set in the Unity editor.
    /// </summary>
    public class CellexalSimpleButton : CellexalButton
    {
        // set in the editor
        public string description;
        protected override string Description => description;

        public override void Click() { }
    }
}
