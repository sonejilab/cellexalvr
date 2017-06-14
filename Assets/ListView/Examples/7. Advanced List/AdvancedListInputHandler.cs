using UnityEngine;

namespace ListView
{
    public class AdvancedListInputHandler : ListViewScroller
    {
        public float scrollThreshold = 0.2f;
        public float scrollWheelCoeff = 1;
        float m_ListDepth;

        protected override void HandleInput()
        {
            Vector3 screenPoint = Input.mousePosition;
            if (Input.GetMouseButton(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    ListViewItemBase item = hit.collider.GetComponent<ListViewItemBase>();
                    if (item)
                    {
                        m_ListDepth = (hit.point - Camera.main.transform.position).magnitude;
                        screenPoint.z = m_ListDepth;
                        StartScrolling(Camera.main.ScreenToWorldPoint(screenPoint));
                    }
                }
            }
            screenPoint.z = m_ListDepth;
            Vector3 scrollPosition = Camera.main.ScreenToWorldPoint(screenPoint);
            Scroll(scrollPosition);
            if (Mathf.Abs(m_StartPosition.y - scrollPosition.y) < scrollThreshold)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        AdvancedListItem item = hit.collider.GetComponent<AdvancedListItem>();
                        if (item)
                        {
                            item.data.expanded = !item.data.expanded;
                        }
                    }
                }
            }
            if (!Input.GetMouseButton(0))
                StopScrolling();

            listView.scrollOffset += Input.mouseScrollDelta.y * scrollWheelCoeff;
        }

        protected override void Scroll(Vector3 position)
        {
            if (m_Scrolling)
                listView.scrollOffset = m_StartOffset + m_StartPosition.y - position.y;
        }

        protected override void StopScrolling()
        {
            base.StopScrolling();
            ((AdvancedList) listView).OnStopScrolling();
        }
    }
}
