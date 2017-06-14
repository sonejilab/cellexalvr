using UnityEngine;

namespace ListView
{
    public class SimpleListView : MonoBehaviour
    {
        public GameObject prefab;
        public int dataOffset;

        public float itemHeight = 1;
        public int range = 5;

        public string[] data;
		public GUISkin skin;
							
        TextMesh[] m_Items;	

        void Start()
        {
            m_Items = new TextMesh[range];
            for (int i = 0; i < range; i++)
            {
                m_Items[i] = Instantiate(prefab).GetComponent<TextMesh>();
                m_Items[i].transform.position = transform.position + Vector3.down * i * itemHeight;
                m_Items[i].transform.parent = transform;
            }
            UpdateList();
        }

        void UpdateList()
        {
            for (int i = 0; i < range; i++)
            {
                int dataIdx = i + dataOffset;
                if (dataIdx >= 0 && dataIdx < data.Length)
                {
                    m_Items[i].text = data[dataIdx];
                } else
                {
                    m_Items[i].text = "";
                }
            }
        }

        void OnGUI()
        {
	        GUI.skin = skin;		
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.Label("This is an overly simplistic m_List view. Click the buttons below to scroll, or modify Data Offset in the inspector");
            if (GUILayout.Button("Scroll Next"))
            {
                ScrollNext();
            }
            if (GUILayout.Button("Scroll Prev"))
            {
                ScrollPrev();
            }
            GUILayout.EndArea();
        }

        void ScrollNext()
        {
            dataOffset++;
            UpdateList();
        }

        void ScrollPrev()
        {
            dataOffset--;
            UpdateList();
        }
    }
}