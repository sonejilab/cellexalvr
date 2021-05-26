namespace CellexalVR.Menu.Buttons
{

    public class IncreaseAverageVelocityResolutionButton : CellexalButton
    {

        protected override string Description
        {
            get
            {
                return "Increase the average velocity resolution by 1";
            }
        }


        public override void Click()
        {
            referenceManager.velocityGenerator.ChangeAverageVelocityResolution(1);
            referenceManager.multiuserMessageSender.SendMessageChangeAverageVelocityResolution(1);

        }
    }
}
