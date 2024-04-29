﻿using TMPro;
using UnityEngine;

namespace CellexalVR.Menu.Buttons
{
    public class BrainPartButton : CellexalButton
    {
        public TextMeshPro nameHeader;
        public GameObject modelPart;

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

        protected override string Description => $"Toggle {modelName} mesh from reference";

        public override void Click()
        {
            modelPart.SetActive(!modelPart.gameObject.activeSelf);
        }

    }
}
