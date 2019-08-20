using UnityEngine;

namespace CellexalVR.Filters
{

    public class FilterCreatorDeleteArea : MonoBehaviour
    {

        private void OnTriggerEnter(Collider other)
        {
            // the colliders that can be grabbed are on child gameobjects
            FilterCreatorBlock filterCreatorBlock = other.gameObject.GetComponentInParent<FilterCreatorBlock>();

            if (filterCreatorBlock && !(filterCreatorBlock is FilterCreatorResultBlock))
            {
                filterCreatorBlock.GetComponent<VRTK.VRTK_InteractableObject>().ForceStopInteracting();
                filterCreatorBlock.DisconnectAllPorts();

                Destroy(filterCreatorBlock.gameObject);
            }
        }
    }
}
