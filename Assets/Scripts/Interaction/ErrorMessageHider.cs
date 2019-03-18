using UnityEngine;
using CellexalVR.General;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Plays animation that hides error message.
    /// </summary>
    public class ErrorMessageHider : MonoBehaviour
    {
        public ErrorMessage errorMessage;
        public Animator animator;

        public void HideErrorMessage()
        {
            errorMessage.DeactivateErrorMessage();
        }

        public void AnimationDone()
        {
            animator.SetTrigger("Done");
        }
    }
}