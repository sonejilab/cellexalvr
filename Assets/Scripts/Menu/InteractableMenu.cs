using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CellexalVR.Menu
{

    /// <summary>
    /// Makes a menu/submenu grabbable and movable.
    /// </summary>
    [RequireComponent(typeof(VRTK.VRTK_InteractableObject), typeof(VRTK.GrabAttachMechanics.VRTK_FixedJointGrabAttach))]
    public class InteractableMenu : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public VRTK.VRTK_InteractableObject interactableObject;
        public bool isSubMenu = true;
        public SubMenu subMenu;
        public MenuUnfolder menuUnfolder;
        public GameObject reattachPrefab;

        private bool insideReattachCollider = false;
        private GameObject reattachGameObject;
        private MeshCollider reattachCollider;
        private Transform oldParent;
        private Vector3 oldPos;
        private Quaternion oldRot;
        private Vector3 oldScale;
        private Dictionary<Collider, bool> colliderStates = new Dictionary<Collider, bool>();

        private void Start()
        {
            interactableObject.InteractableObjectGrabbed += MenuGrabbed;
            interactableObject.InteractableObjectUngrabbed += MenuUngrabbed;
        }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        /// <summary>
        /// Called when the menu is grabbed to potentially detach it.
        /// </summary>
        private void MenuGrabbed(object sender, VRTK.InteractableObjectEventArgs e)
        {
            MeshCollider thisCollider = gameObject.GetComponent<MeshCollider>();
            if (transform.parent != null)
            {
                oldParent = transform.parent;
                oldPos = transform.localPosition;
                oldRot = transform.localRotation;
                oldScale = transform.localScale;
                transform.parent = null;
                // stop vrtk_interactableobject from parenting the menu back where it was
                interactableObject.OverridePreviousState(null, false, true);
                if (isSubMenu)
                {
                    subMenu.Attached = false;
                    subMenu.SetUnderlyingContentActive(true);
                    // vrtk_interactableobject adds a rigidbody for us so we don't have to, just fix the settings
                    Rigidbody rb = subMenu.gameObject.AddComponent<Rigidbody>();
                    rb.mass = 1f;
                    rb.drag = 10f;
                    rb.angularDrag = 15f;
                    rb.useGravity = false;
                }
                else
                {
                    menuUnfolder.Unfold();
                }

                // create a collider on the main menu where this menu was to detect when it is being reattached
                reattachGameObject = Instantiate(reattachPrefab);
                reattachGameObject.SetActive(true);
                reattachGameObject.transform.parent = oldParent;
                reattachGameObject.transform.localPosition = oldPos;
                reattachGameObject.transform.localRotation = oldRot;
                reattachGameObject.transform.localScale = oldScale;


                reattachCollider = reattachGameObject.GetComponent<MeshCollider>();
                reattachCollider.sharedMesh = thisCollider.sharedMesh;
                reattachCollider.convex = thisCollider.convex;
                reattachCollider.isTrigger = thisCollider.isTrigger;
                reattachGameObject.GetComponent<MeshFilter>().sharedMesh = thisCollider.sharedMesh;
            }

            // disable all colliders and save their states, they are restored when the menu is ungrabbed
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                if (col != thisCollider)
                {
                    colliderStates[col] = col.enabled;
                    col.enabled = false;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (interactableObject.IsGrabbed() && other == reattachCollider)
            {
                insideReattachCollider = true;
                reattachGameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (interactableObject.IsGrabbed() && other == reattachCollider)
            {
                insideReattachCollider = false;
                reattachGameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        public void ReattachMenu()
        {
            if (subMenu && subMenu.Attached)
            {
                // Already attached!
                return;
            }

            transform.parent = oldParent;
            transform.localPosition = oldPos;
            transform.localRotation = oldRot;
            transform.localScale = oldScale;
            insideReattachCollider = false;
            Destroy(reattachGameObject);
            if (isSubMenu)
            {
                subMenu.Attached = true;
                if (subMenu.Active)
                {
                    subMenu.SetUnderlyingContentActive(false);
                }
                Destroy(subMenu.GetComponent<Rigidbody>());
            }
            else
            {
                menuUnfolder.Fold();
            }
        }

        /// <summary>
        /// Called when the menu is ungrabbed to potentially reattach it.
        /// </summary>
        private void MenuUngrabbed(object sender, VRTK.InteractableObjectEventArgs e)
        {
            // reset the state of all the colliders
            foreach (var colliderState in colliderStates)
            {
                if (colliderState.Key != null)
                {
                    colliderState.Key.enabled = colliderState.Value;
                }
            }

            if (insideReattachCollider)
            {
                ReattachMenu();
            }
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(InteractableMenu))]
    [CanEditMultipleObjects]
    public class InteractableMenuInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            InteractableMenu script = target as InteractableMenu;
            script.referenceManager = (ReferenceManager)EditorGUILayout.ObjectField("Reference Manager", script.referenceManager, typeof(ReferenceManager), true);
            script.interactableObject = (VRTK.VRTK_InteractableObject)EditorGUILayout.ObjectField("Interactable Object", script.interactableObject, typeof(VRTK.VRTK_InteractableObject), true);
            script.isSubMenu = EditorGUILayout.Toggle("Is Sub Menu", script.isSubMenu);
            if (script.isSubMenu)
            {
                script.subMenu = (SubMenu)EditorGUILayout.ObjectField("Sub Menu", script.subMenu, typeof(SubMenu), true);
            }
            else
            {
                script.menuUnfolder = (MenuUnfolder)EditorGUILayout.ObjectField("Menu Unfolder", script.menuUnfolder, typeof(MenuUnfolder), true);
            }
            script.reattachPrefab = (GameObject)EditorGUILayout.ObjectField("Reattach Prefab", script.reattachPrefab, typeof(GameObject), true);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
