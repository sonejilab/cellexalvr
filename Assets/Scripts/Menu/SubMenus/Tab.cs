using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using TMPro;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    /// <summary>
    /// A base class that can be used with <see cref="MenuWithTabs"/> to create menus with tabs.
    /// </summary>
    public class Tab : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public TabButton TabButton;
        public bool Active { get; private set; }

        protected int buttonIndex = 0;
        protected Vector3 buttonPos = new Vector3(-.396f, .77f, .182f);
        protected Vector3 buttonPosOriginal = new Vector3(-.396f, .77f, .182f);
        protected Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
        protected Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);

        private MenuToggler menuToggler;

        //public TextMesh TabName;
        public TextMeshPro TabName;
        //public string TabName { get; set; }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        protected virtual void Awake()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            menuToggler = referenceManager.menuToggler;
            CellexalEvents.MenuClosed.AddListener(Inactivate);
        }

        void Inactivate()
        {
            SetTabActive(false);
        }
        /// <summary>
        /// Show or hides all buttons that this tab contains.
        /// </summary>
        /// <param name="active">True if this tab should be shown, false if hidden.</param>
        public virtual void SetTabActive(bool active)
        {
            Active = active;
            if (!menuToggler)
            {
                if (referenceManager == null)
                {
                    referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
                }
                menuToggler = referenceManager.menuToggler;
            }
            foreach (Transform child in transform)
            {
                // We don't want to change the state of the tab buttons, they should always be turned on.
                if (ReferenceEquals(child.GetComponent<TabButton>(), null))
                {
                    //if (menuToggler.MenuActive)
                    //{
                    // if the menu is turned on
                    ToggleGameObject(child.gameObject, active);
                    // Toggle all children to the child as well
                    foreach (Transform t in child.GetComponentsInChildren<Transform>(true))
                    {
                        ToggleGameObject(t.gameObject, active);
                    }

                }
                else
                {
                    // if the menu is turned off
                    menuToggler.AddGameObjectToActivateNoChildren(child.gameObject, active);
                    foreach (Transform t in child.GetComponentsInChildren<Transform>())
                    {
                        menuToggler.AddGameObjectToActivateNoChildren(t.gameObject, active);
                    }
                }
                //}
                //else if (!menuToggler.MenuActive)
                //{
                //    // set the tab button to become visible when the menu is turned back on if the submenu it is attached to is turned on
                //    menuToggler.AddGameObjectToActivate(child.gameObject, TabButton.Menu.gameObject);
                //}
            }
        }

        /// <summary>
        /// Checks if a <see cref="GameObject"/> or one of the <see cref="GameObject"/>'s parents (or grandparents and so on) is a <see cref="T:TabButton"/>.
        /// </summary>
        private bool IsPartOfTabButton(GameObject obj)
        {
            if (!ReferenceEquals(obj.GetComponent<TabButton>(), null))
            {
                return true;
            }
            else if (!ReferenceEquals(obj.transform.parent, null))
            {
                return IsPartOfTabButton(obj.transform.parent.gameObject);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Finds a <see cref="MenuWithTabs"/> in this <see cref="GameObject"/> or one of its parents.
        /// </summary>
        /// <param name="obj">The <see cref="GameObject"/> to search.</param>
        /// <returns>The <see cref="MenuWithTabs"/> if there was one, null otherwise.</returns>
        private MenuWithTabs FindMenuInParents(GameObject obj)
        {
            var menu = obj.GetComponent<MenuWithTabs>();
            if (!ReferenceEquals(menu, null))
            {
                return menu;
            }
            else if (!ReferenceEquals(obj.transform.parent, null))
            {
                return FindMenuInParents(obj.transform.parent.gameObject);
            }
            else
            {
                return null;
            }
        }

        private void ToggleGameObject(GameObject obj, bool active)
        {
            Renderer r = obj.GetComponent<Renderer>();
            if (r)
            {
                if (r.gameObject.name.Equals("ActiveOutline"))
                {
                    r.gameObject.SetActive(r.transform.parent.GetComponent<CellexalButton>().storedState);
                }
                r.enabled = active;
            }

            Collider c = obj.GetComponent<Collider>();
            if (c)
            {
                c.enabled = active;
                if (c.gameObject.name.Equals("ActiveOutline"))
                {
                    c.gameObject.SetActive(c.transform.parent.GetComponent<CellexalButton>().storedState);
                }
                c.enabled = active;
            }
        }

        public void AddButton(CellexalButton button)
        {
            //if (!menuToggler)
            //{
            //    menuToggler = referenceManager.menuToggler;
            //}

            //if (button.transform.childCount > 0)
            //{
            //    menuToggler.AddGameObjectToActivate(button.transform.GetChild(0).gameObject, gameObject);
            //}
            button.transform.localPosition = buttonPos;

            if ((buttonIndex + 1) % 4 == 0)
            {
                buttonPos -= buttonPosInc * 3;
                buttonPos += buttonPosNewRowInc;
            }
            else
            {
                buttonPos += buttonPosInc;
            }
            buttonIndex++;
        }
    }
}