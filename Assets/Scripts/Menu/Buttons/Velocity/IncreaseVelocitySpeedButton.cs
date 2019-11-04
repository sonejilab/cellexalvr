namespace CellexalVR.Menu.Buttons.Velocity
{

    public class IncreaseVelocitySpeedButton : CellexalButton
    {
        public float amount;

        protected override string Description
        {
            get
            {
                return "Increase speed of velocity arrows";
            }
        }

        public override void Click()
        {
            referenceManager.velocityGenerator.ChangeSpeed(amount);
            referenceManager.multiuserMessageSender.SendMessageChangeSpeed(amount);
        }
    }
}
