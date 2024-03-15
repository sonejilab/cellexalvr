using CellexalVR.Interaction;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif
using System.Collections;

namespace CellexalVR.AnalysisObjects
{
    public class EnvironmentMenuWithTabs : CellexalRaycastable
    {
        [HideInInspector]
        public List<EnvironmentTab> tabs;
        public EnvironmentTab tabPrefab;
        public EnvironmentTabScrollButton tabScrollButton1;
        public EnvironmentTabScrollButton tabScrollButton2;
        [Tooltip("Time in seconds it should take the tab buttons to scroll")]
        public float tabScrollTime = 0.5f;

        [Header("Tab Button Placement")]
        public Vector3 tabButtonSize = new Vector3(0.1f, 0f, 0f);
        public Vector3 tabButtonPositionIncrement;

        [Tooltip("Edit by dragging the edges of the bounding box")]
        public Vector3 tabButtonBoundsCenter;
        [Tooltip("Edit by dragging the edges of the bounding box")]
        public Vector3 tabButtonBoundsSize;
        [HideInInspector]
        public Vector3[] tabButtonPositions;
        [HideInInspector]
        public Vector3[] tabScrollPositions;

        /// <summary>
        /// An index in the tabs list of which is currently the left-most tab that is visible
        /// </summary>
        private int visibleTabButtonIndex;
        /// <summary>
        /// should be equal to visibleTabButtonIndex most of the time, except when ScrollTabsCoroutine is running, then this number where the list _actually_ is currently <br />
        /// used when ScrollTabsCoroutine is interupted by another tabScrollButton and we need to pick up the scroll animation when the tab buttons are not aligned
        /// </summary>
        private float actualVisibleTabButtonIndex;
        /// <summary>
        /// the maximum number of _visible_ tab buttons, the tabScrollButton1 and 2 scrolls through the available buttons <br />
        /// the left-most tab (and tab button) that is visible is represented by visibleTabButtonIndex
        /// </summary>
        [HideInInspector, SerializeField]
        private int maxNumberOfTabButtons;
        private Coroutine tabButtonScrollingCoroutine;

        private void OnEnable()
        {
            if (tabs.Count > 0)
            {
                SetTabButtonPositions();
            }
        }

        /// <summary>
        /// Recalculates <see cref="tabButtonPositions"/> and <see cref="tabScrollPositions"/>. Should be called if an <see cref="EnvironmentTab"/> is added or removed.
        /// </summary>
        public void SetTabButtonPositions(bool overrideShowScrollButtons = false)
        {
            if (tabButtonPositionIncrement.x == 0f && tabButtonPositionIncrement.y == 0f && tabButtonPositionIncrement.z == 0f)
            {
                Debug.LogWarning("Tab buttons must have non-zero offset from each other, deafulting to x = 0.1");
                tabButtonPositionIncrement = new Vector3(0.1f, 0f, 0f);
            }

            maxNumberOfTabButtons = (int)(tabButtonBoundsSize.x / Mathf.Abs(tabButtonPositionIncrement.x));
            Vector3 margin = (tabButtonBoundsSize - ((maxNumberOfTabButtons - 1) * tabButtonPositionIncrement) - tabButtonSize) / 2f;
            int nPositions = System.Math.Max(maxNumberOfTabButtons * 2, tabs.Count);
            tabButtonPositions = new Vector3[nPositions];
            if (maxNumberOfTabButtons == 0)
            {
                // Warning message is shown in scene GUI
                return;
            }
            for (int i = 0; i < tabButtonPositions.Length; ++i)
            {
                tabButtonPositions[i] = tabButtonBoundsCenter - tabButtonBoundsSize / 2f + tabButtonPositionIncrement * i + margin + tabButtonSize / 2f;
            }

            if (tabScrollButton1 is not null && tabScrollButton2 is not null)
            {
                if (tabScrollPositions is null)
                {
                    tabScrollPositions = new Vector3[2];
                }
                tabScrollPositions[0] = tabButtonPositions[0] - tabButtonPositionIncrement;
                tabScrollPositions[1] = tabButtonPositions[maxNumberOfTabButtons - 1] + tabButtonPositionIncrement;
                bool scrollButtonsNeeded = tabs.Count > maxNumberOfTabButtons || overrideShowScrollButtons;
                tabScrollButton1.gameObject.SetActive(scrollButtonsNeeded);
                tabScrollButton2.gameObject.SetActive(scrollButtonsNeeded);
            }
        }

        /// <summary>
        /// Helper function to know if a tab button is currently in one of the visible indices.
        /// </summary>
        /// <param name="index">The tab button's index.</param>
        /// <returns>True if the index is visible, false otherwise.</returns>
        private bool IsTabButtonInVisibleIndex(int index)
        {
            return index >= visibleTabButtonIndex && index < visibleTabButtonIndex + maxNumberOfTabButtons;
        }

        /// <summary>
        /// Moves all tab button gameobjects to their correct positions. This function moves all tab buttons, visible and hidden, and sets the disables the hidden ones with <see cref="GameObject.SetActive(bool)"/>.
        /// </summary>
        public void MoveTabButtons()
        {
            actualVisibleTabButtonIndex = visibleTabButtonIndex;
            Vector3 offset = tabButtonPositionIncrement * visibleTabButtonIndex;
            for (int i = 0; i < tabs.Count; ++i)
            {
                bool isTabButtonVisible = IsTabButtonInVisibleIndex(i);
                tabs[i].tabButton.gameObject.SetActive(isTabButtonVisible);
                tabs[i].tabButton.transform.localPosition = tabButtonPositions[i] - offset;
                tabs[i].tabButton.transform.localScale = tabButtonSize;
            }

        }

        /// <summary>
        /// Adds a new tab to this menu, using the <see cref="tabPrefab"/>.
        /// </summary>
        /// <returns>The instantiated <see cref="EnvironmentTab"/></returns>
        public EnvironmentTab AddTab()
        {
            EnvironmentTab newTab = Instantiate(tabPrefab);
            newTab.gameObject.SetActive(true);
            newTab.transform.parent = transform;
            tabs.Add(newTab);
            SetTabButtonPositions();
            MoveTabButtons();
            return newTab;
        }

        /// <summary>
        /// Switches to the specified tab by disabling the content of all others, and enabling the content of the specified tab.
        /// </summary>
        /// <param name="tab">The tab to enable.</param>
        public void SwitchToTab(EnvironmentTab tab)
        {
            TurnOffAllTabs();
            tab.SetTabActive(true);
        }

        /// <summary>
        /// Scrolls the tab buttons.
        /// </summary>
        /// <param name="direction">The number of tab buttons to scroll, positive numbers will scroll towards higher indices, negative numbers scroll towards lower indices.</param>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="direction"/> is 0.</exception>
        public void ScrollTabs(int direction)
        {
            if (direction == 0)
            {
                throw new System.ArgumentException("direction must be non-zero");
            }
            else if (direction > 0)
            {
                if (visibleTabButtonIndex + maxNumberOfTabButtons + direction > tabs.Count)
                {
                    // end of list reached
                    return;
                }
            }
            else
            {
                // direction is less than 0
                if (visibleTabButtonIndex == 0)
                {
                    // beginning of list reached
                    return;
                }
            }
            if (tabButtonScrollingCoroutine is not null)
            {
                StopCoroutine(tabButtonScrollingCoroutine);
            }
            tabButtonScrollingCoroutine = StartCoroutine(ScrollTabsCoroutine(direction));
        }

        private IEnumerator ScrollTabsCoroutine(int direction)
        {

            visibleTabButtonIndex += direction;
            if (visibleTabButtonIndex < 0)
            {
                visibleTabButtonIndex = 0;
            }
            else if (visibleTabButtonIndex + maxNumberOfTabButtons >= tabs.Count)
            {
                visibleTabButtonIndex = tabs.Count - maxNumberOfTabButtons;
            }

            for (int i = 0; i < tabs.Count; ++i)
            {
                bool tabButtonInVisibleIndex = IsTabButtonInVisibleIndex(i);
                tabs[i].tabButton.gameObject.SetActive(tabButtonInVisibleIndex);
            }
            float startIndex = actualVisibleTabButtonIndex;
            float endIndex = visibleTabButtonIndex;
            // set up start and end positions for visible buttons
            Vector3[] startPositions = new Vector3[maxNumberOfTabButtons];
            Vector3[] endPositions = tabButtonPositions;
            for (int tabButtonIndex = visibleTabButtonIndex, i = 0; tabButtonIndex < visibleTabButtonIndex + maxNumberOfTabButtons; ++tabButtonIndex, ++i)
            {
                startPositions[i] = tabs[tabButtonIndex].tabButton.transform.localPosition;
            }

            float t = 0f;
            while (t < 1f)
            {
                float smoothedT = Mathf.SmoothStep(0f, 1f, t);
                for (int tabButtonIndex = visibleTabButtonIndex, i = 0; tabButtonIndex < visibleTabButtonIndex + maxNumberOfTabButtons; ++tabButtonIndex, ++i)
                {
                    tabs[tabButtonIndex].tabButton.transform.localPosition = Vector3.Lerp(startPositions[i], endPositions[i], smoothedT);
                }
                actualVisibleTabButtonIndex = startIndex + (endIndex - startIndex) * smoothedT;
                t += Time.deltaTime / tabScrollTime;
                yield return null;
            }

            // set tab button positions to exactly what they are supposed to be, just in case
            // this updates actualVisibleTabButtonIndex as well
            MoveTabButtons();
            tabButtonScrollingCoroutine = null;
        }

        /// <summary>
        /// Dsiables the content gameobject on all tabs.
        /// </summary>
        public void TurnOffAllTabs()
        {
            foreach (EnvironmentTab tab in tabs)
            {
                tab.SetTabActive(false);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EnvironmentMenuWithTabs), editorForChildClasses: true)]
    public class EnvironmentMenuWithTabsEditor : Editor
    {
        BoxBoundsHandle tabButtonBoundsHandle;

        protected virtual void OnEnable()
        {
            tabButtonBoundsHandle = new BoxBoundsHandle();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            EnvironmentMenuWithTabs inspected = target as EnvironmentMenuWithTabs;
            tabButtonBoundsHandle.center = inspected.transform.TransformPoint(inspected.tabButtonBoundsCenter);
            tabButtonBoundsHandle.size = inspected.tabButtonBoundsSize;
            tabButtonBoundsHandle.DrawHandle();
            inspected.tabButtonBoundsCenter = inspected.transform.InverseTransformPoint(tabButtonBoundsHandle.center);
            inspected.tabButtonBoundsSize = tabButtonBoundsHandle.size;
            // let the menu calculate the array of tab button positions
            inspected.SetTabButtonPositions(true);
            if (inspected.tabButtonPositions.Length == 0)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                Handles.Label(inspected.tabButtonBoundsCenter, "Too small!", style);
            }
            else
            {
                // move prefabs tab button to the first available position
                if (inspected.tabPrefab is not null)
                {
                    inspected.tabPrefab.tabButton.transform.localPosition = inspected.tabButtonPositions[0];
                    inspected.tabPrefab.tabButton.transform.localScale = inspected.tabButtonSize;
                }
                // show a preview of where the other tab buttons will end up
                int maxNumberOfVisibleTabButtons = serializedObject.FindProperty("maxNumberOfTabButtons").intValue;
                for (int i = 0; i < maxNumberOfVisibleTabButtons; ++i)
                {
                    Handles.color = Color.green;
                    Handles.DrawWireCube(inspected.transform.TransformPoint(inspected.tabButtonPositions[i]), inspected.tabButtonSize);
                }

                // set tab scroll buttons positions
                if (inspected.tabScrollButton1 is not null)
                {
                    inspected.tabScrollButton1.transform.localPosition = inspected.tabScrollPositions[0];
                }

                if (inspected.tabScrollButton2 is not null)
                {
                    inspected.tabScrollButton2.transform.localPosition = inspected.tabScrollPositions[1];
                }
            }
            inspected.MoveTabButtons();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
