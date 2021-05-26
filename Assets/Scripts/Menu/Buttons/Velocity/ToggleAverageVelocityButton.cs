namespace CellexalVR.Menu.Buttons
{

    public class ToggleAverageVelocityButton : CellexalButton
    {

        protected override string Description
        {
            get
            {
                return "Toggle the average velocity arrows";
            }
        }


        public override void Click()
        {
            referenceManager.velocityGenerator.ToggleAverageVelocity();
            referenceManager.multiuserMessageSender.SendMessageToggleAverageVelocity();
        }
    }
}
