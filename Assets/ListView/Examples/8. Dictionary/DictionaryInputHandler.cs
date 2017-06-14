using UnityEngine;

namespace ListView
{
	public class DictionaryInputHandler : ListViewScroller
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
			}
			else
			{
				StopScrolling();
			}
			screenPoint.z = m_ListDepth;
			Scroll(Camera.main.ScreenToWorldPoint(screenPoint));
		}

		protected override void StartScrolling(Vector3 position)
		{
			base.StartScrolling(position);
			((DictionaryList) listView).OnStartScrolling();
		}

		protected override void Scroll(Vector3 position)
		{
			if (m_Scrolling)
			{
				listView.scrollOffset = m_StartOffset + m_StartPosition.y - position.y;
			}
		}

		protected override void StopScrolling()
		{
			base.StopScrolling();
			((DictionaryList) listView).OnStopScrolling();
		}
	}
}