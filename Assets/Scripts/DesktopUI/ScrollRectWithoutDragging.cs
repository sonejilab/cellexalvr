using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CellexalVR.DesktopUI
{
    /// <summary>
    /// Class inhereting from <see cref="ScrollRect"/> and overrides the callbacks related to dragging with nothing, effectively disabling the dragging.
    /// </summary>
    public class ScrollRectWithoutDragging : ScrollRect
    {
        public override void OnBeginDrag(PointerEventData eventData) { }
        public override void OnEndDrag(PointerEventData eventData) { }
        public override void OnDrag(PointerEventData eventData) { }
    }
}