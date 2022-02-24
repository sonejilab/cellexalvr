using AnalysisLogic;
using CellexalVR.General;
using CellexalVR.AnalysisLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CellexalVR.Spatial
{

    public class PointSpreader : MonoBehaviour
    {
        private PointCloud pointCloud;
        private bool bothClick;
        private bool rightClick;
        private bool leftClick;
        private float currentDistance;
        private float startDistance;
        private Transform controllerL;
        private Transform controllerR;
        private int controllersInside;
        [SerializeField] private InputActionAsset actionAsset;
        [SerializeField] private InputActionReference actionReferenceL;
        [SerializeField] private InputActionReference actionReferenceR;
        //private MenuToggler menuToggler;

        private void Start()
        {
            pointCloud = GetComponent<PointCloud>();
            controllerL = ReferenceManager.instance.leftController.transform;
            controllerR = ReferenceManager.instance.rightController.transform;
            //CellexalEvents.RightTriggerClick.AddListener(OnRightTriggerClick);
            //CellexalEvents.LeftTriggerClick.AddListener(OnLeftTriggerClick);
            //CellexalEvents.LeftTriggerUp.AddListener(OnRightTriggerUp);
            //CellexalEvents.LeftTriggerUp.AddListener(OnLeftTriggerUp);

            actionReferenceL.action.performed += LeftAction;
            actionReferenceR.action.performed += RightAction;
            actionReferenceL.action.canceled += LeftUp;
            actionReferenceR.action.canceled += RightUp;
        }

        private void RightUp(InputAction.CallbackContext context)
        {
            rightClick = false;
            bothClick = false;
            currentDistance = 0f;
        }

        private void LeftUp(InputAction.CallbackContext context)
        {
            leftClick = false;
            bothClick = false;
            currentDistance = 0f;
        }

        private void RightAction(InputAction.CallbackContext context)
        {
            if (controllerR == null)
                controllerR = ReferenceManager.instance.rightController.transform;
            float minDist = float.MaxValue;
            float dist;
            foreach (PointCloud pc in PointCloudGenerator.instance.pointClouds)
            {
                dist = Vector3.Distance(controllerR.transform.position, pc.transform.position);
                if (dist < minDist)
                {
                    pointCloud = pc;
                }
            }
            rightClick = true;
            if (leftClick)
            {
                bothClick = true;
                currentDistance = 0f;
                startDistance = Vector3.Distance(controllerL.position, controllerR.position);
            }
        }

        private void LeftAction(InputAction.CallbackContext context)
        {
            if (controllerL == null)
                controllerL = ReferenceManager.instance.leftController.transform;
            leftClick = true;
            if (rightClick)
            {
                bothClick = true;
                currentDistance = 0f;
                startDistance = Vector3.Distance(controllerL.position, controllerR.position);
            }
        }

        private void Update()
        {
            if (bothClick)
            {
                if (controllerL == null || controllerL == controllerR)
                    controllerL = ReferenceManager.instance.leftController.transform;
                if (controllerR == null)
                    controllerR = ReferenceManager.instance.rightController.transform;
                currentDistance = Vector3.Distance(controllerL.position, controllerR.position);
                if (currentDistance > startDistance + 0.4f)
                {
                    pointCloud.SpreadOutPoints();
                    //pointCloud.SpreadOutClusters();
                    bothClick = false;
                    currentDistance = 0;
                }

                else if (currentDistance < startDistance - 0.4f)
                {
                    pointCloud.SpreadOutPoints(false);
                    //pointCloud.SpreadOutClusters();
                    bothClick = false;
                    currentDistance = 0;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("GameController"))
            {
                if (controllersInside == 0)
                {
                    controllerL = other.transform;
                }
                else if (controllersInside == 1)
                {
                    controllerR = other.transform;
                }

                controllersInside += 1;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("GameController"))
            {
                if (controllersInside == 0)
                {
                    controllerL = null;
                }
                else if (controllersInside == 1)
                {
                    controllerR = null;
                }
                controllersInside -= 1;
            }
        }


    }

}