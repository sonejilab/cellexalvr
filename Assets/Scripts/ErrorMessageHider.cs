using UnityEngine;

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
