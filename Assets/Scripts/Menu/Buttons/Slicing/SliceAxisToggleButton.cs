using CellexalVR.Spatial;
using Unity.Entities;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class SliceAxisToggleButton : SliderButton
    {
        public int axis;
        protected override string Description => "";

        private SliceGraphSystem slicer;

        protected override void Awake()
        {
            base.Awake();
            SetButtonActivated(false);
            slicer = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SliceGraphSystem>();//GetComponentInParent<Slicer>();
        }
        
        protected override void ActionsAfterSliding()
        {
            if (currentState == false)
            {
                //slicer.ChangeAxis(axis);
            }

            else
            {
                //slicer.ChangeAxis(-1);
            }
        }
    }
}