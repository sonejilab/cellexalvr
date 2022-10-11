using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    public class SubMenu : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        //public InteractableMenu interactableMenu;
        public bool Active { get; set; }
        public bool Attached { get; set; }

        public GameObject buttonsToDeactivate;
        public GameObject textmeshToDarken;

        protected List<CellexalButton> cellexalButtons = new List<CellexalButton>();


        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        protected virtual void Start()
        {
            Attached = true;
        }

        public virtual CellexalButton FindButton(string name)
        {
            var button = cellexalButtons.Find(x => x.name == name);
            return button;
        }

        /// <summary>
        /// Sets this submenu to visible or invisible.
        /// </summary>
        /// <param name="active">True for visible, false for invisible.</param>
        public virtual void SetMenuActive(bool active)
        {
            if (Active == active)
            {
                return;
            }
            Active = active;
            foreach (Renderer rend in GetComponentsInChildren<Renderer>(true))
            {
                rend.enabled = active;
            }
            foreach (Collider col in GetComponentsInChildren<Collider>(true))
            {
                col.enabled = active;
            }
            foreach (Canvas canvas in GetComponentsInChildren<Canvas>())
            {
                canvas.enabled = active;
            }

            //if (interactableMenu && !Attached)
            //{
            //    interactableMenu.ReattachMenu();
            //}
            //else
            //{
            //    SetUnderlyingContentActive(!active);
            //}
        }

        /// <summary>
        /// Activate or deactivate the buttons that are underneath this menu.
        /// </summary>
        /// <param name="active">True for activate, false for deactivate.</param>
        public void SetUnderlyingContentActive(bool active)
        {
            textmeshToDarken.GetComponent<MeshRenderer>().enabled = active;
            if (active)
            {
                foreach (Transform child in buttonsToDeactivate.transform)
                {
                    if (child.gameObject.GetComponent<SubMenu>() == null)
                    {
                        foreach (CellexalButton b in child.gameObject.GetComponentsInChildren<CellexalButton>())
                        {
                            b.SetButtonActivated(b.storedState);
                        }
                    }
                }
            }
            else
            {
                foreach (Transform child in buttonsToDeactivate.transform)
                {
                    if (child.gameObject.GetComponent<SubMenu>() == null)
                    {
                        foreach (CellexalButton b in child.gameObject.GetComponentsInChildren<CellexalButton>())
                        {
                            b.StoreState();
                            b.SetButtonActivated(false);
                        }
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor class for the <see cref="SubMenu "/> to add a "Toggle renderers/colliders" button.
    /// </summary>
    [CustomEditor(typeof(SubMenu), true)]
    [CanEditMultipleObjects]

    public class SubMenuEditor : Editor
    {
        private SubMenu instance;

        void OnEnable()
        {
            instance = (SubMenu)target;
        }



        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Toggle renderers/colliders"))
            {
                bool active = !instance.GetComponent<Renderer>().enabled;
                instance.GetComponent<Renderer>().enabled = active;
                instance.GetComponent<Collider>().enabled = active;

                foreach (Collider col in instance.GetComponentsInChildren<Collider>())
                {
                    col.enabled = active;
                }
                foreach (Renderer ren in instance.GetComponentsInChildren<Renderer>())
                {
                    ren.enabled = active;
                }

            }
        }
    }
#endif
}
