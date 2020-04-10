using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VRTK.SecondaryControllerGrabActions
{
    public class VRTK_AxisScaleGrabAction_summertwerk : VRTK_AxisScaleGrabAction
    {
        Joint joint;
        Vector3 a;
        Vector3 b;


        public override void Initialise(VRTK_InteractableObject currentGrabbedObject, VRTK_InteractGrab currentPrimaryGrabbingObject, VRTK_InteractGrab currentSecondaryGrabbingObject, Transform primaryGrabPoint, Transform secondaryGrabPoint)
        {
            
            base.Initialise(currentGrabbedObject, currentPrimaryGrabbingObject, currentSecondaryGrabbingObject, primaryGrabPoint, secondaryGrabPoint);
            a = primaryGrabbingObject.transform.position;
            b = secondaryGrabbingObject.transform.position;
            joint = GetComponent<Joint>();

        }

        protected override void UniformScale()
        {
            
            Vector3 aa = primaryGrabbingObject.transform.position;
            Vector3 bb = secondaryGrabbingObject.transform.position;
            float c = (a - b).magnitude;
            float cc = (aa - bb).magnitude;

            Vector3 axis = -Vector3.Cross(bb - aa, b - a);
            float angle = Vector3.Angle(bb - aa, b - a);

            grabbedObject.transform.RotateAround(aa, axis, angle);

            Vector3 newScale = grabbedObject.transform.localScale * (cc / c);
            ApplyScale(newScale);

            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = Vector3.zero;

            a = aa;
            b = bb;

        }

    }
}