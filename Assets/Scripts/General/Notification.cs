using UnityEngine;
using CellexalVR.General;

namespace CellexalVR.General
{
    /// <summary>
    /// Plays animation that hides the notification.
    /// </summary>
    public class Notification : MonoBehaviour
    {
        public Animator animator;
        public NotificationManager notificationManager;

        
        public void HideNotification()
        {
            notificationManager.DeactivateNotification();
            Destroy(this.gameObject);

        }

        public void AnimationDone()
        {
            animator.SetTrigger("Done");
        }
    }
}