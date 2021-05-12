using UnityEngine;
using CellexalVR.General;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Make events for click on radial menu object since prefab does not keep references to other objects when building the scene.
    /// Depending on the object this script is attached to the events differ. The method names should be explanatory.
    /// </summary>
    public class EventSetter : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        public void GeneKeyboardEnterEvent(string geneName)
        {
            referenceManager.cellManager.ColorGraphsByGene(geneName);
            referenceManager.multiuserMessageSender.SendMessageColorGraphsByGene(geneName);
        }

        public void GeneKeyboardEditEvent(string s)
        {
            referenceManager.multiuserMessageSender.SendMessageKeyClicked(s);
        }

        public void GeneKeyboardAnnotateEvent(string s)
        {
            int index = referenceManager.selectionToolCollider.CurrentColorIndex;
            referenceManager.annotationManager.AddAnnotation(s, index);
            referenceManager.multiuserMessageSender.SendMessageAddAnnotation(s, index);
        }


        public void BrowserKeyboardEditEvent(string s)
        {
            referenceManager.multiuserMessageSender.SendMessageBrowserKeyClicked(s);
        }

        public void FolderKeyboardEditEvent(string filter)
        {
            referenceManager.inputFolderGenerator.GenerateFolders(filter);
        }

        public void ReferenceModelKeyboardEditEvent(string filter)
        {
            AllenReferenceBrain.instance.UpdateSuggestions(filter);
        }

    }
}




