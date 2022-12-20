using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CellexalVR.Menu.Buttons.General
{
    public class CellexalSimpleButton : CellexalButton
    {

        public string description;
        protected override string Description => description;

        public UnityEvent OnClick;

        public override void Click()
        {
            OnClick.Invoke();
        }
    }
}
