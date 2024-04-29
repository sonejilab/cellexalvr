using CellexalVR.General;
using CellexalVR.Spatial;
using TMPro;

namespace CellexalVR.Menu.Buttons
{
    public class LoadReferenceModelMeshButton : CellexalButton
    {
        public TextMeshPro nameHeader;
        private string modelName;

        public string ModelName
        {
            get => modelName;
            set
            {
                modelName = value;
                nameHeader.text = value;
            }
        }

        protected override string Description => $"Load {modelName} mesh from reference";

        public override void Click()
        {
            AllenReferenceBrain.instance.SpawnModel(ModelName);
            ReferenceManager.instance.multiuserMessageSender.SendMessageSpawnModel(modelName);
        }

    }
}
