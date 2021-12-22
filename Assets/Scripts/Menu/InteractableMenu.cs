//using CellexalVR.General;
//using CellexalVR.Menu.SubMenus;
//using System.Collections.Generic;
//using CellexalVR.Interaction;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;

//namespace CellexalVR.Menu
//{
//    /// <summary>
//    /// Makes a menu/submenu grabbable and movable.
//    /// </summary>
//    [RequireComponent(typeof(XRGrabInteractable))]
//    public class InteractableMenu : MonoBehaviour
//    {
//        public ReferenceManager referenceManager;
//        public XRGrabInteractable interactableObject;
//        public bool isSubMenu = true;
//        public SubMenu subMenu;
//        public MenuUnfolder menuUnfolder;
//        public GameObject reattachPrefab;
//        public Transform reattachPoint;

//        private bool insideReattachCollider = false;
//        private GameObject reattachGameObject;
//        private MeshCollider reattachCollider;
//        private Transform oldParent;
//        private Vector3 oldPos;
//        private Quaternion oldRot;
//        private Vector3 oldScale;
//        private Dictionary<Collider, bool> colliderStates = new Dictionary<Collider, bool>();

//        private void Start()
//        {
//            interactableObject = GetComponent<InteractableObjectBasic>();
//            interactableObject.InteractableObjectGrabbed += MenuGrabbed;
//            interactableObject.InteractableObjectUnGrabbed += MenuUnGrabbed;
//        }

//        private void OnValidate()
//        {
//            if (gameObject.scene.IsValid())
//            {
//                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
//            }
//        }

//        /// <summary>
//        /// Called when the menu is grabbed to potentially detach it.
//        /// </summary>
//        private void MenuGrabbed(object sender, Hand hand)
//        {
//            Collider thisCollider = gameObject.GetComponent<Collider>();
//            if (interactableObject.GetPreviousParent() != null)
//            {
//                transform.parent = null;
//                if (isSubMenu)
//                {
//                    subMenu.Attached = false;
//                    subMenu.SetUnderlyingContentActive(true);
//                    Rigidbody rb = subMenu.GetComponent<Rigidbody>();
//                    if (rb == null)
//                    {
//                        rb = subMenu.gameObject.AddComponent<Rigidbody>();
//                    }

//                    rb.mass = 1f;
//                    rb.drag = 10f;
//                    rb.angularDrag = 15f;
//                    rb.useGravity = false;
//                    rb.isKinematic = false;
//                }
//                else
//                {
//                    menuUnfolder.Unfold();
//                }

//                // create a collider on the main menu where this menu was to detect when it is being reattached
//                if (reattachGameObject == null)
//                {
//                    reattachGameObject = Instantiate(reattachPrefab);
//                }

//                reattachGameObject.SetActive(true);

//                reattachGameObject.transform.parent = interactableObject.GetPreviousParent();
//                reattachGameObject.transform.localPosition = interactableObject.GetPreviousPosition();
//                reattachGameObject.transform.localRotation = interactableObject.GetPreviousRotation();
//                reattachGameObject.transform.localScale = interactableObject.GetPreviousScale();

//                reattachCollider = reattachGameObject.GetComponent<MeshCollider>();
//                Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
//                reattachCollider.sharedMesh = mesh;
//                reattachCollider.convex = true;
//                reattachCollider.isTrigger = true;
//                reattachGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
//            }

//            // disable all colliders and save their states, they are restored when the menu is ungrabbed
//            foreach (Collider col in GetComponentsInChildren<Collider>())
//            {
//                if (col != thisCollider)
//                {
//                    colliderStates[col] = col.enabled;
//                    col.enabled = false;
//                }
//            }
//        }

//        private void OnTriggerEnter(Collider other)
//        {
//            if (interactableObject.isGrabbed && other == reattachCollider)
//            {
//                insideReattachCollider = true;
//                reattachGameObject.GetComponent<MeshRenderer>().enabled = true;
//            }
//        }

//        private void OnTriggerExit(Collider other)
//        {
//            if (interactableObject.isGrabbed && other == reattachCollider)
//            {
//                insideReattachCollider = false;
//                reattachGameObject.GetComponent<MeshRenderer>().enabled = false;
//            }
//        }

//        public void ReattachMenu()
//        {
//            if (subMenu && subMenu.Attached)
//            {
//                // Already attached!
//                return;
//            }

//            transform.parent = interactableObject.GetPreviousParent();
//            transform.localPosition = interactableObject.GetPreviousPosition();
//            transform.localRotation = interactableObject.GetPreviousRotation();
//            transform.localScale = interactableObject.GetPreviousScale();
//            insideReattachCollider = false;
//            Destroy(reattachGameObject);
//            if (isSubMenu)
//            {
//                subMenu.Attached = true;
//                if (subMenu.Active)
//                {
//                    subMenu.SetUnderlyingContentActive(false);
//                }

//                Destroy(subMenu.GetComponent<Rigidbody>());
//            }
//            else
//            {
//                menuUnfolder.Fold();
//            }
//        }

//        /// <summary>
//        /// Called when the menu is ungrabbed to potentially reattach it.
//        /// </summary>
//        private void MenuUnGrabbed(object sender, Hand hand)
//        {
//            // reset the state of all the colliders
//            foreach (var colliderState in colliderStates)
//            {
//                if (colliderState.Key != null)
//                {
//                    colliderState.Key.enabled = colliderState.Value;
//                }
//            }

//            if (insideReattachCollider)
//            {
//                ReattachMenu();
//            }

//            else
//            {
//                transform.parent = null;
//            }
//        }
//    }

//    //#if UNITY_EDITOR
//    //    [CustomEditor(typeof(InteractableMenu))]
//    //    [CanEditMultipleObjects]
//    //    public class InteractableMenuInspector : Editor
//    //    {
//    //        public override void OnInspectorGUI()
//    //        {
//    //            serializedObject.Update();
//    //            InteractableMenu script = target as InteractableMenu;
//    //            script.referenceManager = (ReferenceManager)EditorGUILayout.ObjectField("Reference Manager", script.referenceManager, typeof(ReferenceManager), true);
//    //            script.isSubMenu = EditorGUILayout.Toggle("Is Sub Menu", script.isSubMenu);
//    //            if (script.isSubMenu)
//    //            {
//    //                script.subMenu = (SubMenu)EditorGUILayout.ObjectField("Sub Menu", script.subMenu, typeof(SubMenu), true);
//    //            }
//    //            else
//    //            {
//    //                script.menuUnfolder = (MenuUnfolder)EditorGUILayout.ObjectField("Menu Unfolder", script.menuUnfolder, typeof(MenuUnfolder), true);
//    //            }
//    //            script.reattachPrefab = (GameObject)EditorGUILayout.ObjectField("Reattach Prefab", script.reattachPrefab, typeof(GameObject), true);
//    //            serializedObject.ApplyModifiedProperties();
//    //        }
//    //    }
//    //#endif
//}