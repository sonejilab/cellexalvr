namespace CellexalVR.Menu.Buttons
{

    public class DecreaseAverageVelocityResolutionButton : CellexalButton
    {

        protected override string Description
        {
            get
            {
                return "Decrease the average velocity resolution by 1";
            }
        }


        public override void Click()
        {
            referenceManager.velocityGenerator.ChangeAverageVelocityResolution(-1);
            referenceManager.multiuserMessageSender.SendMessageChangeAverageVelocityResolution(-1);
        }
    }
}
