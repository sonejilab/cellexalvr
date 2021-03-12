using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Spatial
{
    public class LightSaber : MonoBehaviour
    {
        public static LightSaber instance;
        public GameObject rayCastSource;
        public Hand hand;
        public SteamVR_Action_Boolean grabPinch;
        public LightSaberSliceCollision laser;

        private Vector3 positionInHand = new Vector3(0.01f, -0.02f, -0.02f);
        private Quaternion rotationInHand = Quaternion.Euler(15f, 90f, 50f);
        private bool inHand;
        private Rigidbody rigidbody;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            // grabPinch.AddOnStateDownListener(OnRightTriggerDown, inputSource);
        }

        private void Update()
        {
            if (inHand) return;
            if (Input.GetKeyDown(KeyCode.H))
            {
                StartCoroutine(MoveToHand());
            }

            Ray ray = new Ray(rayCastSource.transform.position, rayCastSource.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, 10)) return;
            if (hit.collider.gameObject != gameObject)
            {
                if (rigidbody.isKinematic)
                {
                    rigidbody.isKinematic = false;
                }

                return;
            }

            LaserHover();
            if (grabPinch.stateDown)
            {
                StartCoroutine(MoveToHand());
            }
        }

        private void LaserHover()
        {
            if (!rigidbody.isKinematic)
            {
                rigidbody.isKinematic = true;
            }

            float dT = Time.deltaTime;
            Vector3 pos = transform.position;
            pos.y = 0.1f + (1f + math.sin(Time.time)) * 0.1f;
            if (pos.y < 0.1f) return;
            transform.Rotate(20 * dT, 0, 0);
            transform.position = pos;
        }

        private IEnumerator MoveToHand()
        {
            inHand = true;
            laser.GetComponentInChildren<VisualEffect>(true).Play();
            transform.parent = hand.gameObject.transform;
            GetComponent<Rigidbody>().isKinematic = true;
            float dT = Time.deltaTime;
            while (Vector3.Distance(transform.localPosition, positionInHand) > 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, positionInHand, dT * 5);
                // transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotationInHand, dT * 5);
                transform.Rotate(90f * dT, 80 * dT, 150f * dT);
                yield return null;
            }

            GetComponent<InteractableLightSaber>().Attach(hand);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(ActivateLaser());
        }

        private IEnumerator ActivateLaser()
        {
            laser.gameObject.SetActive(true);
            Vector3 targetScale = new Vector3(0.01f, 0.5f, 0.01f);
            Vector3 targetPosition = new Vector3(laser.transform.localPosition.x, 0.58f, laser.transform.localPosition.z);
            float dT = Time.deltaTime;
            while (math.abs(laser.transform.localScale.y - 0.5f) > 0.01f)
            {
                laser.transform.localScale = Vector3.MoveTowards(laser.transform.localScale, targetScale, 3 * dT);
                laser.transform.localPosition = Vector3.MoveTowards(laser.transform.localPosition, targetPosition, 3 * dT);
                yield return null;
            }

            laser.transform.localScale = targetScale;
            laser.transform.localPosition = targetPosition;
        }
    }
}