namespace CellexalVR.Menu.Buttons.Velocity
{
    public class DecreaseVelocitySpeedButton : CellexalButton
    {

        public float amount;

        protected override string Description
        {
            get
            {
                return "Decrease speed of velocity arrows";
            }
        }

        public override void Click()
        {
            referenceManager.velocityGenerator.ChangeSpeed(amount);
            referenceManager.multiuserMessageSender.SendMessageChangeSpeed(amount);
        }
    }
}
