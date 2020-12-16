using UnityEngine;

namespace CellexalVR.Filters
{

    public class FilterCreatorDeleteArea : MonoBehaviour
    {
        public FilterManager filterManager;

        private void OnTriggerEnter(Collider other)
        {
            // the colliders that can be grabbed are on child gameobjects
            FilterCreatorBlock filterCreatorBlock = other.gameObject.GetComponentInParent<FilterCreatorBlock>();

            if (filterCreatorBlock && !(filterCreatorBlock is FilterCreatorResultBlock))
            {
                filterCreatorBlock.DisconnectAllPorts();

                Destroy(filterCreatorBlock.gameObject);
                filterManager.UpdateFilterFromFilterCreator();
            }
        }
    }
}
