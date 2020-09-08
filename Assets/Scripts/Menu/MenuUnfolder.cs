using CellexalVR.DesktopUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Menu
{
    public class MenuUnfolder : MonoBehaviour
    {
        public GameObject normalMenuCube;
        [Tooltip("Should be left, front, right, back")]
        public List<GameObject> buttonParents;
        public List<GameObject> backsides;
        public GameObject topSide;
        public GameObject topBackside;

        private bool folded = true;
        private bool animationRunning = false;
        private Coroutine animationCoroutine;
        private float currentTime;


        public bool Folded { get => folded; set => folded = value; }

        /// <summary>
        /// Unfolds the menu to be on flat surface instead of a 3D cube menu with buttons on 4 sides
        /// </summary>
        [ConsoleCommand("menuUnfolder", aliases: new string[] { "unfoldmenu", "ufm" })]
        public void Unfold()
        {
            if (Folded)
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(FoldCoroutine(false));
            }
        }

        /// <summary>
        /// Folds the menu to be a 3D cube menu with buttons on 4 sides instead of a flat surface 
        /// </summary>
        [ConsoleCommand("menuUnfolder", aliases: new string[] { "foldmenu", "fm" })]
        public void Fold()
        {
            if (!Folded)
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(FoldCoroutine(true));
            }
        }

        /// <summary>
        /// Coroutine that folds or unfolds the menu.
        /// </summary>
        /// <param name="fold">True for folding (go to 3D cube on controller), false for unfold (2D menu).</param>
        private IEnumerator FoldCoroutine(bool fold)
        {
            float targetTime = 2.0f;
            if (animationRunning)
            {
                currentTime = targetTime - currentTime;
            }
            else
            {
                currentTime = 0.0f;
            }

            Folded = fold;
            animationRunning = true;
            if (!fold)
            {
                // activate the sides and deactivate the normal menu cube
                normalMenuCube.SetActive(false);
                foreach (GameObject backside in backsides)
                {
                    backside.SetActive(true);
                }
                topBackside.SetActive(true);
            }

            // top side
            Vector3 topStart = new Vector3(0f, 0.5f, 0.5f);
            Vector3 topFinal;
            Vector3 topStartScale;
            Vector3 topFinalScale;

            // vector between the front side's origin and the side's corner (the hinge)
            Vector3 sideStart0 = new Vector3(-0.5f, 0.5f, 0f);
            Vector3 sideStart2 = new Vector3(0.5f, 0.5f, 0f);
            Vector3 sideStart3 = new Vector3(0.5f, 0.5f, 0f);
            // vector between the corner connecting this side to the next one (the corner acting like a hinge during the animation) and the sides's origin
            Vector3 sideFinal0;
            Vector3 sideFinal2;
            Vector3 sideFinal3;
            // since the third side rotates around a rotating side, we need an intermediate distance to travel along the side it's rotating around to reach the front side
            Vector3 intermediate3;

            if (fold)
            {
                topFinal = new Vector3(0f, 0f, 0.5f);
                topStartScale = new Vector3(3f, 0.1f, 0.03f);
                topFinalScale = new Vector3(1f, 1f, 0.03f);
                sideFinal0 = new Vector3(-0.5f, -0.5f, 0f);
                sideFinal2 = new Vector3(0.5f, -0.5f, 0f);
                sideFinal3 = new Vector3(0.5f, -0.5f, 0f);
                intermediate3 = new Vector3(1f, 0f, 0f);
            }
            else
            {
                topFinal = new Vector3(0f, -0.5f, 0f);
                topStartScale = new Vector3(1f, 1f, 0.03f);
                topFinalScale = new Vector3(3f, 0.1f, 0.03f);
                sideFinal0 = -sideStart0;
                sideFinal2 = -sideStart2;
                sideFinal3 = new Vector3(-0.5f, 0.5f, 0f);
                intermediate3 = new Vector3(0f, -1f, 0f);
            }

            float direction = fold ? -1f : 1f;
            float currentRadians = currentTime / targetTime * Mathf.PI / 2f * direction;

            while (currentTime < targetTime)
            {
                float degreesThisFrame = 90f * Time.deltaTime / targetTime * direction;
                float radiansThisFrame = degreesThisFrame / 180f * Mathf.PI;
                currentRadians += radiansThisFrame;

                // buttonParents[1] is the front side, which we do not need to rotate
                // the other sides should be rotated to match its rotation
                buttonParents[0].transform.Rotate(0f, 0f, -degreesThisFrame);
                buttonParents[2].transform.Rotate(0f, 0f, degreesThisFrame);
                buttonParents[3].transform.Rotate(0f, 0f, 2 * degreesThisFrame);
                topSide.transform.Rotate(-degreesThisFrame, 0f, 0f);

                float cosPos = Mathf.Cos(currentRadians);
                float sinPos = Mathf.Sin(currentRadians);
                float cosNeg = cosPos; // = Mathf.Cos(-currentRadians);
                float sinNeg = -sinPos; // = Mathf.Sin(-currentRadians);
                float cos2Pos = Mathf.Cos(2 * currentRadians);
                float sin2Pos = Mathf.Sin(2 * currentRadians);

                // rotate the vectors
                Vector3 sideFinal0Rotated = new Vector3(
                    sideFinal0.x * cosNeg - sideFinal0.y * sinNeg,
                    sideFinal0.x * sinNeg + sideFinal0.y * cosNeg,
                    0f);

                Vector3 sideFinal2Rotated = new Vector3(
                    sideFinal2.x * cosPos - sideFinal2.y * sinPos,
                    sideFinal2.x * sinPos + sideFinal2.y * cosPos,
                    0f);

                Vector3 sideFinal3Rotated = new Vector3(
                    sideFinal3.x * cos2Pos - sideFinal3.y * sin2Pos,
                    sideFinal3.x * sin2Pos + sideFinal3.y * cos2Pos,
                    0f);

                Vector3 intermediate3Rotated = new Vector3(
                    intermediate3.x * cosPos - intermediate3.y * sinPos,
                    intermediate3.x * sinPos + intermediate3.y * cosPos,
                    0f);

                Vector3 topFinalRotated = new Vector3(
                    0f,
                    topFinal.y * cosNeg - topFinal.z * sinNeg,
                    topFinal.y * sinNeg + topFinal.z * cosNeg);

                float scaleRadians = fold ? 1f - Mathf.Cos(currentRadians) : Mathf.Sin(currentRadians);
                Vector3 topScale = Vector3.Lerp(topStartScale, topFinalScale, scaleRadians);

                buttonParents[0].transform.localPosition = sideStart0 + sideFinal0Rotated;
                buttonParents[2].transform.localPosition = sideStart2 + sideFinal2Rotated;
                buttonParents[3].transform.localPosition = sideStart3 + intermediate3Rotated + sideFinal3Rotated;
                topSide.transform.localPosition = topStart + topFinalRotated * topScale.y;
                topBackside.transform.localScale = topScale;

                currentTime += Time.deltaTime;
                yield return null;
            }

            Quaternion[] targetRotations;
            Vector3[] targetPositions;
            Vector3 topSideTargetPosition;
            Quaternion topSideTargetRotation;
            Vector3 topSideTargetScale;

            if (fold)
            {
                targetRotations = new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.identity,
                    Quaternion.identity
                };
                targetPositions = new Vector3[] {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f)
                };
                topSideTargetPosition = new Vector3(0f, 0f, 0.5f);
                topSideTargetRotation = Quaternion.identity;
                topSideTargetScale = new Vector3(1f, 1f, 0.03f);
            }
            else
            {
                targetRotations = new Quaternion[]
                {
                    Quaternion.Euler(0f, 0f, -90f),
                    Quaternion.Euler(0f, 0f, 90f),
                    Quaternion.Euler(0f, 0f, 180f)
                };
                targetPositions = new Vector3[] {
                    new Vector3(-1f, 0f, 0f),
                    new Vector3(1f, 0f, 0f),
                    new Vector3(2f, 0f, 0f)
                };
                topSideTargetPosition = new Vector3(0f, 0.5f, 0.55f);
                topSideTargetRotation = Quaternion.Euler(-90f, 0f, 0f);
                topSideTargetScale = new Vector3(3f, 0.1f, 0.03f);
            }

            // set the rotations and positions on final time after the animation
            buttonParents[0].transform.localRotation = targetRotations[0];
            buttonParents[2].transform.localRotation = targetRotations[1];
            buttonParents[3].transform.localRotation = targetRotations[2];

            buttonParents[0].transform.localPosition = targetPositions[0];
            buttonParents[2].transform.localPosition = targetPositions[1];
            buttonParents[3].transform.localPosition = targetPositions[2];

            topSide.transform.localPosition = topSideTargetPosition;
            topSide.transform.localRotation = topSideTargetRotation;
            topBackside.transform.localScale = topSideTargetScale;

            if (fold)
            {
                normalMenuCube.SetActive(true);
                foreach (GameObject backside in backsides)
                {
                    backside.SetActive(false);
                }
                topBackside.SetActive(false);
            }
            animationCoroutine = null;
            animationRunning = false;
        }

    }
}
