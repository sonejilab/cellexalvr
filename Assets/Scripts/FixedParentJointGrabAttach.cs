namespace VRTK.GrabAttachMechanics
{
    using UnityEngine;

    /// <summary>
    /// Lets a <see cref="VRTK_InteractableObject"/> be grabbed by its parent gameobject instead of this gameobject.
    /// </summary>
    public class FixedParentJointGrabAttach : VRTK_BaseJointGrabAttach
    {
        [Tooltip("Maximum force the joint can withstand before breaking. Infinity means unbreakable.")]
        public float breakForce = 1500f;

        protected override void CreateJoint(GameObject obj)
        {
            if (obj.transform.parent == null)
                return;
            obj = obj.transform.parent.gameObject;
            givenJoint = obj.AddComponent<FixedJoint>();
            givenJoint.breakForce = (grabbedObjectScript.IsDroppable() ? breakForce : Mathf.Infinity);
            base.CreateJoint(obj);
        }
    }
}