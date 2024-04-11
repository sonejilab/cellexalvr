using UnityEngine;
using UnityEngine.Events;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Reperesents a target that can be raycasted against by <see cref="CellexalRaycast"/>.
    /// </summary>
    public class CellexalRaycastable : MonoBehaviour
    {
        [SerializeField]
        public UnityEvent OnActivate;
        [SerializeField]
        public UnityEvent RaycastEnter;
        [SerializeField]
        public UnityEvent RaycastExit;
        public bool canBePushedAndPulled = true;
        public bool active = true;

        /// <summary>
        /// Called by <see cref="CellexalRaycast.Update"/> every frame that a raycast hit this target.
        /// </summary>
        /// <param name="hitInfo">The <see cref="RaycastHit"/> from the <see cref="Physics.Raycast"/> call.</param>
        /// <param name="raycaster">The <see cref="CellexalRaycast"/> that made the <see cref="Physics.Raycast"/> call.</param>
        public virtual void OnRaycastHit(RaycastHit hitInfo, CellexalRaycast raycaster) { }

        /// <summary>
        /// Called on the first frame that this <see cref="CellexalRaycastable"/> is hit by a <see cref="CellexalRaycast"/>.
        /// </summary>
        public virtual void OnRaycastEnter()
        {
            RaycastEnter.Invoke();
        }

        /// <summary>
        /// Called on the first frame that this <see cref="CellexalRaycastable"/> is not hit by a <see cref="CellexalRaycast"/> anymore.
        /// </summary>
        public virtual void OnRaycastExit()
        {
            RaycastExit.Invoke();
        }

    }
}
