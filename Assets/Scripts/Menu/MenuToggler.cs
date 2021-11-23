using System.Collections.Generic;
using UnityEngine;
using CellexalVR.Interaction;
using CellexalVR.General;

namespace CellexalVR.Menu
{

    /// <summary>
    /// Holds the logic for toggling the menu.
    /// </summary>
    public class MenuToggler : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public bool MenuActive { get; set; }
        public GameObject teleportLaser;
        public GameObject menuCube;
        public GameObject menuHolder;

        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.InputDevice device;
        private GameObject menu;
        private MenuRotator menuRotator;
        // These dictionaries holds the things that were turned off when the menu was deactivated
        private Dictionary<Renderer, bool> renderers = new Dictionary<Renderer, bool>();
        private Dictionary<Collider, bool> colliders = new Dictionary<Collider, bool>();
        private Collider boxCollider;
        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.Interaction.Toolkit.ActionBasedController leftController;
        private ControllerModelSwitcher controllerModelSwitcher;
        private bool animate;
        private float currentTime;
        private float arrivalTime = 1f;
        private Vector3 startScale;
        private Vector3 finalScale;
        private Vector3 startPosition;
        private Vector3 finalPosition;
        private Vector3 originalScale = new Vector3(0.26f, 0.26f, 0.32f);
        private readonly int animateSpeed = 8;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            menu = referenceManager.mainMenu;
            menuHolder = menu.transform.parent.gameObject;
            menuRotator = referenceManager.menuRotator;
            boxCollider = GetComponent<Collider>();
            leftController = referenceManager.leftController;
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            teleportLaser = referenceManager.teleportLaser;
            // The menu should be turned off when the program starts
            MenuActive = false;
            CellexalEvents.LeftTriggerClick.AddListener(OnTriggerClick);
        }

        void Update()
        {
            if (animate)
            {
                currentTime += 0.8f * Time.deltaTime;
                float step = (currentTime * animateSpeed) / arrivalTime;
                menu.transform.localScale = Vector3.Lerp(startScale, finalScale, step);
                menu.transform.position = Vector3.Lerp(startPosition, finalPosition, step);
                if (Mathf.Abs(menu.transform.localScale.x - finalScale.x) <= 0.001f)
                {
                    animate = false;
                    ToggleMenu();
                }
            }
        }

        private void OnTriggerClick()
        {
            // Open XR
            //device = SteamVR_Controller.Input((int)leftController.index);
            if (true)
            {
                animate = true;
                currentTime = 0f;
                startScale = menu.transform.localScale;
                if (MenuActive)
                {
                    startPosition = menu.transform.position;
                    finalScale = Vector3.zero;
                    finalPosition = menuCube.transform.position;
                }
                else
                {
                    startPosition = menuCube.transform.position;
                    finalScale = originalScale;
                    menu.transform.parent = leftController.transform;
                    menu.transform.position = menuCube.transform.position;
                    menu.transform.rotation = menuCube.transform.rotation;
                    finalPosition = menuCube.transform.position + menuCube.transform.forward * 0.2f;
                }
                menuRotator.AllowRotation = !MenuActive;
                menuRotator.StopRotating();
            }
        }

        public void ToggleMenu()
        {
            MenuActive = !MenuActive;
            menuCube.SetActive(!MenuActive);
            if (!MenuActive)
            {
                menu.transform.parent = menuHolder.transform;
                menu.transform.position = new Vector3(0f, -10f, 0f);
            }
            else
            {
                menu.transform.parent = leftController.transform;
                menu.transform.position = menuCube.transform.position + menuCube.transform.forward * 0.2f;
            }
            boxCollider.enabled = MenuActive;
            controllerModelSwitcher.SwitchToDesiredModel();
            controllerModelSwitcher.ActivateDesiredTool();
        }

        /// <summary>
        ///  Adds a gameobject to activate or not activate later when the menu comes back on.
        ///  This method does nothing if the menu is already active.
        /// </summary>
        /// <param name="item"> The gameobject to activate. </param>
        /// <param name="activate"> True for activating the gameobject later, false for not activating it. </param>
        public void AddGameObjectToActivateNoChildren(GameObject item, bool activate)
        {
            if (MenuActive) return;
            Renderer r = item.GetComponent<Renderer>();
            {
                if (r)
                {
                    renderers[r] = activate;
                    r.enabled = false;
                }
            }
            Collider c = item.GetComponent<Collider>();
            {
                if (c)
                {
                    colliders[c] = activate;
                    c.enabled = false;
                }
            }
        }

        /// <summary>
        /// Adds a gameobject to the list of gameobjects to show when the menu is turned back on. 
        /// The gameobject will only be hidden if the whole menu is hidden or if the submenu it is attached to is hidden.
        /// This should be used when adding a gameobject to a submenu.
        /// </summary>
        /// <param name="item"> The gameobject turn back on later. </param>
        /// <param name="subMenu"> The menu this gameobject is part of. </param>
        public void AddGameObjectToActivate(GameObject item, GameObject subMenu)
        {
            Renderer submenuRenderer = subMenu.GetComponent<Renderer>();
            // if the menu is not shown now but the submenu was shown when the menu was hidden
            if (!MenuActive)
            {
                bool submenuActive = submenuRenderer && renderers.ContainsKey(submenuRenderer) && renderers[submenuRenderer];
                SaveAndSetVisible(item, submenuActive, false);
            }
            // if the menu is active but the submenu is not, then we should not show the new gameobject when the menu is turned back on.
            else if (MenuActive && subMenu.GetComponent<Renderer>() && !subMenu.GetComponent<Renderer>().enabled)
            {
                SaveAndSetVisible(item, false, false);
            }
        }

        /// <summary>
        /// Adds a gameobject to the list of gameobjects to show when to menu is turned back on.
        /// </summary>
        /// <param name="item"> The gameobject to turn back on. </param>
        public void AddGameObjectToActivate(GameObject item)
        {
            if (MenuActive) return;

            SaveAndSetVisible(item, true, false);
        }

        /// <summary>
        /// Shows or hides the menu.
        /// </summary>
        /// <param name="visible"> True for showing the menu, false otherwise. </param>
        private void SetMenuVisible(bool visible)
        {
            if (visible)
            {
                //menu.transform.localRotation = tempRotation;
                // we are turning on the menu
                // set everything back the way it was
                foreach (KeyValuePair<Renderer, bool> pair in renderers)
                {
                    if (pair.Key)
                    {
                        pair.Key.enabled = pair.Value;
                    }
                }
                foreach (KeyValuePair<Collider, bool> pair in colliders)
                {
                    if (pair.Key)
                    {
                        pair.Key.enabled = pair.Value;
                    }
                }
            }
            if (!visible)
            {
                //tempRotation = menu.transform.localRotation;
                // we are turning off the menu
                // clear the old saved values
                renderers.Clear();
                colliders.Clear();
                // save whether each renderer and collider is enabled
                foreach (Renderer r in menu.GetComponentsInChildren<Renderer>())
                {
                    if (r)
                    {
                        renderers[r] = r.enabled;
                        r.enabled = false;
                    }
                }
                foreach (Collider c in menu.GetComponentsInChildren<Collider>())
                {
                    if (c)
                    {
                        colliders[c] = c.enabled;
                        c.enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to show or hide all renderers and colliders that belongs to a gameobject and are children to the gameobject.
        /// </summary>
        /// <param name="obj"> The gameobject. </param>
        /// <param name="save"> True if this gameobject should turn back on when the menu is turned on, false otherwise. </param>
        /// <param name="visible"> True if this gameobecj should be visible right now. </param>
        private void SaveAndSetVisible(GameObject obj, bool save, bool visible)
        {
            foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
            {
                if (r)
                {
                    renderers[r] = save;
                    r.enabled = visible;
                }
            }
            foreach (Collider c in obj.GetComponentsInChildren<Collider>())
            {
                if (c)
                {
                    colliders[c] = save;
                    c.enabled = visible;
                }
            }
        }
    }
}

