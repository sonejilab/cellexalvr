using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VRTK.SecondaryControllerGrabActions
{
    public class VRTK_AxisScaleGrabAction_summertwerk : VRTK_AxisScaleGrabAction
    {
        Vector3 old_mid_point;
        ConfigurableJoint joint;
        
        public override void Initialise(VRTK_InteractableObject currentGrabbdObject, VRTK_InteractGrab currentPrimaryGrabbingObject, VRTK_InteractGrab currentSecondaryGrabbingObject, Transform primaryGrabPoint, Transform secondaryGrabPoint)
        {

            base.Initialise(currentGrabbdObject, currentPrimaryGrabbingObject, currentSecondaryGrabbingObject, primaryGrabPoint, secondaryGrabPoint);
            joint = GetComponent<ConfigurableJoint>();
        }

        protected override void ApplyScale(Vector3 newScale)
        {
            Vector3 existingScale = grabbedObject.transform.localScale;

            float finalScaleX = (lockAxis.xState ? existingScale.x : newScale.x);
            float finalScaleY = (lockAxis.yState ? existingScale.y : newScale.y);
            float finalScaleZ = (lockAxis.zState ? existingScale.z : newScale.z);

            if (finalScaleX > 0 && finalScaleY > 0 && finalScaleZ > 0)
            {
                grabbedObject.transform.localScale = new Vector3(finalScaleX, finalScaleY, finalScaleZ); ;
            }
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
            Vector3 mid_point = Vector3.Lerp(primaryGrabbingObject.transform.position, secondaryGrabbingObject.transform.position, 0.5f);
            Vector3 change = mid_point - old_mid_point;
            print(change);
            grabbedObject.transform.Translate(change);
            old_mid_point = mid_point;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

        }
    }
}