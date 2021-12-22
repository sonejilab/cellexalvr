using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace CellexalVR.Menu
{

    /// <summary>
    /// Holds the logic for rotating the menu.
    /// </summary>
    public class MenuRotator : MonoBehaviour
    {
        public Rotation SideFacingPlayer { get; set; }
        public Material menuMaterial;

        private bool isRotating = false;
        private Vector3 fromAngle;
        private float rotatedTotal;

        [SerializeField] private InputActionAsset actionAsset;

        public enum Rotation { Front, Right, Back, Left }

        void Start()
        {
            // Reset rotation in case it is changed in the editor.
            transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
            SideFacingPlayer = Rotation.Front;

            var rotateRight = actionAsset.FindActionMap("XRI LeftHand").FindAction("RotateRight");
            rotateRight.Enable();
            rotateRight.performed += OnThumbpadRight;

            var rotateLeft = actionAsset.FindActionMap("XRI LeftHand").FindAction("RotateLeft");
            rotateLeft.Enable();
            rotateLeft.performed += OnThumbpadLeft;
        }

        public bool AllowRotation { get; set; } = false;

        /// <summary>
        /// Editor script to ensure all menus and submenus have the same material without having to populate manually. 
        /// </summary>
        public void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                foreach (Renderer rend in GetComponentsInChildren<Renderer>())
                {
                    string[] name = rend.gameObject.name.Split(null);
                    if (name[name.Length - 1].Equals("Menu") || name[name.Length - 1].Equals("Box"))
                    {
                        rend.material = menuMaterial;
                    }
                }
            }
        }

        private void OnThumbpadRight(InputAction.CallbackContext context)
        {
            RotateRight(1);
        }

        private void OnThumbpadLeft(InputAction.CallbackContext context)
        {
            RotateLeft(1);
        }

        /// <summary>
        /// Rotates the menu 90 degrees to the right.
        /// </summary>
        public void RotateRight(int times = 1)
        {
            if (AllowRotation && !isRotating)
            {
                switch (SideFacingPlayer)
                {
                    case Rotation.Front:
                        SideFacingPlayer = Rotation.Left;
                        break;
                    case Rotation.Left:
                        SideFacingPlayer = Rotation.Back;
                        break;
                    case Rotation.Back:
                        SideFacingPlayer = Rotation.Right;
                        break;
                    case Rotation.Right:
                        SideFacingPlayer = Rotation.Front;
                        break;
                }

                StartCoroutine(RotateMe(-90f * times, 0.15f));
            }
        }

        /// <summary>
        /// Rotates the menu 90 degrees to the left.
        /// </summary>
        public void RotateLeft(int times = 1)
        {
            if (AllowRotation && !isRotating)
            {
                switch (SideFacingPlayer)
                {
                    case Rotation.Front:
                        SideFacingPlayer = Rotation.Right;
                        break;
                    case Rotation.Right:
                        SideFacingPlayer = Rotation.Back;
                        break;
                    case Rotation.Back:
                        SideFacingPlayer = Rotation.Left;
                        break;
                    case Rotation.Left:
                        SideFacingPlayer = Rotation.Front;
                        break;
                }

                StartCoroutine(RotateMe(90f * times, 0.15f));
            }
        }


        /// <summary>
        /// Stops the menu from rotating and returns the final z angle.
        /// </summary>
        public void StopRotating()
        {
            if (isRotating)
            {
                StopAllCoroutines();
                isRotating = false;
            }

            float zAngles = transform.localEulerAngles.z;
            switch (SideFacingPlayer)
            {
                case Rotation.Front:
                    zAngles = 0f;
                    break;
                case Rotation.Right:
                    zAngles = 90f;
                    break;
                case Rotation.Back:
                    zAngles = 180f;
                    break;
                case Rotation.Left:
                    zAngles = -90f;
                    break;
            }
            transform.localRotation = Quaternion.Euler(-45f, 0f, zAngles);
        }
        /// <summary>
        /// Rotates the menu.
        /// </summary>
        /// <param name="zAngles"> The amount of degrees it should be rotated. Positive values rotate the menu clockwise, negative values rotate it counter-clockwise. </param>
        /// <param name="inTime"> The number of seconds it should take the menu to rotate the specified degrees. </param>
        IEnumerator RotateMe(float zAngles, float inTime)
        {
            isRotating = true;
            // how much we have rotated so far
            rotatedTotal = 0;
            float zAnglesAbs = Mathf.Abs(zAngles);
            // how much we should rotate each second
            float rotationPerSecond = zAngles / inTime;
            while (rotatedTotal < zAnglesAbs && rotatedTotal > -zAnglesAbs)
            {
                float rotationThisFrame = rotationPerSecond * Time.deltaTime;
                rotatedTotal += rotationThisFrame;
                // if we are about to rotate it too far
                if (rotatedTotal > zAnglesAbs || rotatedTotal < -zAnglesAbs)
                {
                    // only rotate the menu as much as there is left to rotate
                    transform.Rotate(0, 0, rotationThisFrame - (rotatedTotal - zAngles));
                }
                else
                {
                    transform.Rotate(0, 0, rotationThisFrame);
                }

                yield return null;
            }

            isRotating = false;
        }
    }
}