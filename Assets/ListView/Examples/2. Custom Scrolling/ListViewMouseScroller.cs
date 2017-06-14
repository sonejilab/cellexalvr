using UnityEngine;

namespace ListView
{
    public class ListViewMouseScroller : ListViewScroller
    {
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
            } else
            {
                StopScrolling();
            }
            screenPoint.z = m_ListDepth;
            Scroll(Camera.main.ScreenToWorldPoint(screenPoint));
        }
    }
}