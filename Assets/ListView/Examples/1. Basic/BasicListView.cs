using UnityEngine;

namespace ListView
{
    public class BasicListView : ListViewController
    {	
		public float scrollSpeed = 10f;
		public GUISkin skin;

		void OnGUI()
        {
			GUI.skin = skin;
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.Label("This is a basic List View. We are only extending the class in order to add the GUI.  Use the buttons below to scroll the m_List, or feel free to modify the value of Scroll Offset in the inspector");
            if (GUILayout.Button("Scroll Next"))
            {
                ScrollNext();
            }
            if (GUILayout.Button("Scroll Prev"))
            {
                ScrollPrev();
            }
            if (GUILayout.RepeatButton("Smooth Scroll Next"))
            {
                scrollOffset -= scrollSpeed * Time.deltaTime;
            }
            if (GUILayout.RepeatButton("Smooth Scroll Prev"))
            {
                scrollOffset += scrollSpeed * Time.deltaTime;
            }
            GUILayout.EndArea();
        }
    }
}
