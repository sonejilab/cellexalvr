﻿using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Multiuser;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// This class represents a heatmap. Contains methods for calling r-script, building texture and interaction methods etc.
    /// </summary>
    public class HeatmapRaycast : MonoBehaviour
    {
        private ReferenceManager referenceManager;
        private GraphManager graphManager;
        private CellManager cellManager;
        private HeatmapGenerator heatmapGenerator;
        private Heatmap heatmap;
        private InputDevice device;
        private ActionBasedController rightController;
        private Transform raycastingSource;
        private MultiuserMessageSender multiuserMessageSender;
        private ControllerModelSwitcher controllerModelSwitcher;
        private int selectionStartX;
        private int selectionStartY;
        private bool selecting = false;
        private bool movingSelection = false;
        private int layerMask;
        private int selectedGroupLeft;
        private int selectedGroupRight;
        private int selectedGeneTop;

        private int selectedGeneBottom;

        // these are the actual coordinates and size of the box
        private float selectedBoxX;
        private float selectedBoxY;
        private float selectedBoxWidth;
        private float selectedBoxHeight;
        private Cell[] cellsToHighlight = new Cell[0];
        private int currentHighlightedGroup;

        private bool triggerDown = false;
        private bool triggerUp = false;
        private bool click = false;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            if (CrossSceneInformation.Normal)
            {
                rightController = referenceManager.rightController;
                raycastingSource = rightController.transform;
                controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            }

            layerMask = 1 << LayerMask.NameToLayer("EnvironmentButtonLayer");
            graphManager = referenceManager.graphManager;
            cellManager = referenceManager.cellManager;
            multiuserMessageSender = referenceManager.multiuserMessageSender;
            heatmapGenerator = referenceManager.heatmapGenerator;
            heatmap = GetComponent<Heatmap>();

            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
            CellexalEvents.RightTriggerPressed.AddListener(OnTriggerDown);
            CellexalEvents.RightTriggerUp.AddListener(OnTriggerUp);
        }

        private void Update()
        {
            if ((!CrossSceneInformation.Normal && !CrossSceneInformation.Tutorial)
                || controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Menu)
            {
                return;
            }

            bool correctModel = referenceManager.rightLaser.enabled;
            if (correctModel)
            {
                Raycast();
                triggerDown = false;
                triggerUp = false;
                click = false;
            }
        }

        private void Raycast(/*bool click = false, bool triggerDown = false, bool triggerUp = false*/)
        {
            raycastingSource = referenceManager.rightLaser.transform;
            //Ray ray = new Ray(raycastingSource.position, raycastingSource.forward);
            RaycastHit hit;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit,
                Mathf.Infinity, layerMask);
            if (hit.collider && hit.transform == transform)
            {
                int hitx = (int)(hit.textureCoord.x * heatmap.layout.bitmapWidth);
                int hity = (int)(hit.textureCoord.y * heatmap.layout.bitmapHeight);
                if (CoordinatesInsideRect(hitx, heatmap.layout.bitmapHeight - hity, heatmap.layout.geneListX, heatmap.layout.heatmapY,
                    heatmap.layout.geneListWidth, heatmap.layout.heatmapHeight))
                {
                    // if we hit the list of genes
                    multiuserMessageSender.SendMessageHandleHitGenesList(name, hity);
                    int geneHit = HandleHitGeneList(hity);

                    // Open XR
                    //if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    if (click)
                    {
                        multiuserMessageSender.SendMessageColorGraphsByGene(heatmap.genes[geneHit]);
                        referenceManager.cellManager.ColorGraphsByGene(heatmap.genes[geneHit],
                            graphManager.GeneExpressionColoringMethod);
                    }
                }
                else if (CoordinatesInsideRect(hitx, heatmap.layout.bitmapHeight - hity, heatmap.layout.heatmapX, heatmap.layout.groupBarY,
                    heatmap.layout.heatmapWidth, heatmap.layout.groupBarHeight))
                {
                    // if we hit the grouping bar
                    multiuserMessageSender.SendMessageHandleHitGroupingBar(name, hitx);
                    HandleHitGroupingBar(hitx);
                }

                else if (CoordinatesInsideRect(hitx, heatmap.layout.bitmapHeight - hity, heatmap.layout.heatmapX,
                    heatmap.layout.attributeBarY, heatmap.layout.heatmapWidth, heatmap.layout.attributeBarHeight))
                {
                    multiuserMessageSender.SendMessageHandleHitAttributeBar(name, hitx);
                    HandleHitAttributeBar(hitx);
                }

                else if (CoordinatesInsideRect(hitx, heatmap.layout.bitmapHeight - hity, heatmap.layout.heatmapX, heatmap.layout.heatmapY,
                    heatmap.layout.heatmapWidth, heatmap.layout.heatmapHeight))
                {
                    heatmap.barInfoText.text = "";
                    heatmap.enlargedGeneText.gameObject.SetActive(false);
                    multiuserMessageSender.SendMessageResetInfoTexts(name);
                    // if we hit the actual heatmap
                    //if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    if (click)
                    {
                        multiuserMessageSender.SendMessageHandlePressDown(name, hitx, hity);
                        HandlePressDown(hitx, hity);
                    }

                    //if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger) && selecting)
                    if (triggerDown && selecting)
                    {
                        // called when choosing a box selection
                        multiuserMessageSender.SendMessageHandleBoxSelection(name, hitx, hity, selectionStartX,
                            selectionStartY);
                        HandleBoxSelection(hitx, hity, selectionStartX, selectionStartY);
                    }
                    //else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger) && selecting)
                    else if (triggerUp && selecting)
                    {
                        // called when letting go of the trigger to finalize a box selection
                        multiuserMessageSender.SendMessageConfirmSelection(name, hitx, hity, selectionStartX,
                            selectionStartY);
                        ConfirmSelection(hitx, hity, selectionStartX, selectionStartY);
                    }
                    //else if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger) && movingSelection)
                    else if (triggerDown && movingSelection)
                    {
                        // called when moving a selection
                        multiuserMessageSender.SendMessageHandleMovingSelection(name, hitx, hity);
                        HandleMovingSelection(hitx, hity);
                    }
                    else if (triggerUp && movingSelection)
                    {
                        // called when letting go of the trigger to move the selection
                        multiuserMessageSender.SendMessageMoveSelection(name, hitx, hity, selectedGroupLeft,
                            selectedGroupRight, selectedGeneTop, selectedGeneBottom);
                        MoveSelection(hitx, hity, selectedGroupLeft, selectedGroupRight, selectedGeneTop,
                            selectedGeneBottom);
                        ResetSelection();
                    }
                    else
                    {
                        // handle when the raycast just hits the heatmap
                        multiuserMessageSender.SendMessageHandleHitHeatmap(name, hitx, hity);
                        HandleHitHeatmap(hitx, hity);
                    }
                }
                else
                {
                    // if we hit the heatmap but not any area of interest, like the borders or any space in between
                    multiuserMessageSender.SendMessageResetHeatmapHighlight(name);
                    ResetHeatmapHighlight();
                    //heatmap.enlargedGeneText.gameObject.SetActive(false);
                    //heatmap.enlargedGeneText.gameObject.SetActive(false);
                }
            }
            else
            {
                // if we don't hit the heatmap at all
                multiuserMessageSender.SendMessageResetHeatmapHighlight(name);
                ResetHeatmapHighlight();
            }

            if (triggerUp)
            {
                // if the raycast leaves the heatmap and the user lets go of the trigger
                multiuserMessageSender.SendMessageResetSelecting(name);
                ResetSelecting();
            }
        }

        private void OnTriggerClick()
        {
            //Raycast(true);
            click = true;
        }

        private void OnTriggerDown()
        {
            //Raycast(triggerDown: true);
            triggerDown = true;
        }

        private void OnTriggerUp()
        {
            //Raycast(triggerUp: true);
            triggerUp = true;
        }

        /// <summary>
        /// Checks if two coordinates are inside a rectangle.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <param name="rectX">The rectangle's x coordinate</param>
        /// <param name="rectY">The rectangle's y coordinate</param>
        /// <param name="rectWidth">The rectangle's width</param>
        /// <param name="rectHeight">The rectangle's height</param>
        /// <returns></returns>
        private bool CoordinatesInsideRect(int x, int y, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            return x >= rectX && y >= rectY && x < rectX + rectWidth && y < rectY + rectHeight;
        }

        /// <summary>
        /// Handles what happens when the trigger is pressed.
        /// </summary>
        /// <param name="hitx">The x coordinate of the hit. Measured in pixels of the texture.</param>
        /// <param name="hity">The y coordinate of the hit. Measured in pixels of the texture.</param>
        public void HandlePressDown(int hitx, int hity)
        {
            if (CoordinatesInsideRect(hitx, heatmap.layout.bitmapHeight - hity, (int)selectedBoxX, (int)selectedBoxY,
                (int)selectedBoxWidth, (int)selectedBoxHeight))
            {
                // if we hit a confirmed selection
                movingSelection = true;
            }
            else
            {
                // if we hit something else
                selecting = true;
                selectionStartX = hitx;
                selectionStartY = hity;
            }
        }

        /// <summary>
        /// Handles the highlighting when the raycast hits the heatmap
        /// </summary>
        /// <param name="hitx"> The x coordinate of the hit. Measured in pixels of the texture.</param>
        /// <param name="hity">The x coordinate if the hit. Meaured in pixels of the texture.</param>
        public void HandleHitHeatmap(int hitx, int hity)
        {
            // get this groups width and xcoordinate
            float groupX, groupWidth;
            int group;
            FindGroupInfo(hitx, out groupX, out groupWidth, out group);

            int geneHit = (int)((float)((heatmap.layout.bitmapHeight - hity) - heatmap.layout.heatmapY) / heatmap.layout.heatmapHeight *
                                 heatmap.genes.Length);
            float highlightMarkerWidth = groupWidth / heatmap.layout.bitmapWidth;
            float highlightMarkerHeight = ((float)heatmap.layout.heatmapHeight / heatmap.layout.bitmapHeight) / heatmap.genes.Length;
            float highlightMarkerX = groupX / heatmap.layout.bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
            float highlightMarkerY = -(float)heatmap.layout.heatmapY / heatmap.layout.bitmapHeight -
                geneHit * (highlightMarkerHeight) - highlightMarkerHeight / 2 + 0.5f;

            heatmap.highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            heatmap.highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            heatmap.highlightQuad.SetActive(true);
            heatmap.highlightInfoText.text = "Group: " + group + "\nGene: " + heatmap.genes[geneHit];

            // the smaller the highlight quad becomes, the larger the text has to become
            heatmap.highlightInfoText.transform.localScale = new Vector3(0.003f / highlightMarkerWidth,
                0.003f / highlightMarkerHeight, 0.003f);
            ResetHeatmapHighlight(false);
        }

        /// <summary>
        /// Finds out some info about what group is at a x coordinate.
        /// </summary>
        /// <param name="hitx">The x coordinate that the raycast hit.</param>
        /// <param name="groupX">The leftmost x coordinate of the group that was hit.</param>
        /// <param name="groupWidth">The width of the group, measured in pixels.</param>
        /// <param name="group">The number (color) of the group.</param>
        private void FindGroupInfo(int hitx, out float groupX, out float groupWidth, out int group)
        {
            groupX = heatmap.layout.heatmapX;
            groupWidth = 0;
            group = 0;
            for (int i = 0; i < heatmap.groupWidths.Count; ++i)
            {
                if (groupX + heatmap.groupWidths[i].Item2 > hitx)
                {
                    group = heatmap.groupWidths[i].Item1;
                    groupWidth = heatmap.groupWidths[i].Item2;
                    break;
                }

                groupX += heatmap.groupWidths[i].Item2;
            }
        }

        /// <summary>
        /// Finds out some info about what attribute is at a x coordinate.
        /// </summary>
        /// <param name="hitx">The x coordinate that the raycast hit.</param>
        /// <param name="attributeX">The leftmost x coordinate of the attribute that was hit.</param>
        /// <param name="layout.attributeWidth">The width of the attribute, measured in pixels.</param>
        /// <param name="attribute">The number (color) of the attribute.</param>
        private void FindAttributeInfo(int hitx, out float attributeX, out float attributeWidth, out int attribute)
        {
            attributeX = heatmap.layout.heatmapX;
            attributeWidth = 0;
            attribute = 0;
            for (int i = 0; i < heatmap.attributeWidths.Count; ++i)
            {
                if (attributeX + heatmap.attributeWidths[i].Item2 > hitx)
                {
                    attribute = heatmap.attributeWidths[i].Item1;
                    attributeWidth = heatmap.attributeWidths[i].Item2;
                    break;
                }

                attributeX += heatmap.attributeWidths[i].Item2;
            }
        }

        /// <summary>
        /// Handles the highlighting when the raycast hits the grouping bar.
        /// The grouping bar is only 1 item tall and thus we do not care about the y coorindate.
        /// </summary>
        /// <param name="hitx"> The xcoordinate of the hit.</param>
        public void HandleHitGroupingBar(int hitx)
        {
            // get this groups width and xcoordinate
            float groupX, groupWidth;
            int group;
            FindGroupInfo(hitx, out groupX, out groupWidth, out group);

            float highlightMarkerWidth = groupWidth / heatmap.layout.bitmapWidth;
            float highlightMarkerHeight = ((float)heatmap.layout.groupBarHeight / heatmap.layout.bitmapHeight);
            float highlightMarkerX = groupX / heatmap.layout.bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
            float highlightMarkerY =
                -(float)heatmap.layout.groupBarY / heatmap.layout.bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

            heatmap.highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            heatmap.highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            heatmap.highlightQuad.SetActive(true);
            heatmap.barInfoText.text = "Group nr: " + group;
            heatmap.highlightInfoText.text = "";
            heatmap.enlargedGeneText.gameObject.SetActive(false);
            if (currentHighlightedGroup == group) return;
            ResetHeatmapHighlight(false);
            cellManager.HighlightGroup(heatmap.selection, group);
            currentHighlightedGroup = group;
        }

        /// <summary>
        /// Handles the highlighting when the raycast hits the attribute bar.
        /// The attribute bar is only 1 item tall and thus we do not care about the y coorindate.
        /// </summary>
        /// <param name="hitx"> The xcoordinate of the hit.</param>
        public void HandleHitAttributeBar(int hitx)
        {
            // get this groups width and xcoordinate
            FindAttributeInfo(hitx, out float attributeX, out float attributeWidth, out int attribute);

            float highlightMarkerWidth = attributeWidth / heatmap.layout.bitmapWidth;
            float highlightMarkerHeight = ((float)heatmap.layout.attributeBarHeight / heatmap.layout.bitmapHeight);
            float highlightMarkerX = attributeX / heatmap.layout.bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
            float highlightMarkerY = -(float)heatmap.layout.attributeBarY / heatmap.layout.bitmapHeight - highlightMarkerHeight / 2 +
                                     0.5f;

            heatmap.highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            heatmap.highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            heatmap.highlightQuad.SetActive(true);
            if (attribute >= cellManager.AttributesNames.Count || attribute < 0) return;
            if (heatmap.barInfoText.text == cellManager.AttributesNames[attribute]) return;
            ResetHeatmapHighlight(false);
            //cellsToHighlight = cellManager.GetCells(cellManager.Attributes[attribute]);
            //cellManager.HighlightCells(cellsToHighlight, true, attribute);
            cellManager.HighlightAttribute(cellManager.AttributesNames[attribute]);
            heatmap.barInfoText.text = attribute >= 0 ? cellManager.AttributesNames[attribute] : "No attribute";
            heatmap.enlargedGeneText.gameObject.SetActive(false);
        }


        /// <summary>
        /// Handles the highlighting of the gene list.
        /// The gene list is only 1 item wide and thus we do not care about the xcoordinate.
        /// </summary>
        /// <param name="hity">The y coordinate of the hit.</param>
        /// <returns>An index of the gene that was hit.</returns>
        public int HandleHitGeneList(int hity)
        {
            int geneHit = (int)((float)((heatmap.layout.bitmapHeight - hity) - heatmap.layout.heatmapY) / heatmap.layout.heatmapHeight *
                                 heatmap.genes.Length);

            float highlightMarkerWidth = (float)heatmap.layout.geneListWidth / heatmap.layout.bitmapWidth;
            float highlightMarkerHeight = ((float)heatmap.layout.heatmapHeight / heatmap.layout.bitmapHeight) / heatmap.genes.Length;
            float highlightMarkerX = (float)heatmap.layout.geneListX / heatmap.layout.bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
            float highlightMarkerY = -(float)heatmap.layout.heatmapY / heatmap.layout.bitmapHeight -
                geneHit * (highlightMarkerHeight) - highlightMarkerHeight / 2 + 0.5f;

            heatmap.highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            heatmap.highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            heatmap.highlightQuad.SetActive(true);
            heatmap.highlightInfoText.text = "";
            heatmap.enlargedGeneText.gameObject.SetActive(true);
            heatmap.enlargedGeneText.text = heatmap.genes[geneHit];
            heatmap.enlargedGeneText.transform.localPosition = new Vector3(
                heatmap.enlargedGeneText.transform.localPosition.x,
                heatmap.highlightQuad.transform.localPosition.y + 0.077f, 0);
            if (geneHit >= heatmap.genes.Length || geneHit < 0)
                return geneHit;
            ResetHeatmapHighlight(false);

            return geneHit;
        }

        /// <summary>
        /// Handles the highlighting when the user is holding the trigger button to select multiple groups and genes. <paramref name="hitx"/> and <paramref name="hity"/> are determined on this frame,
        /// <paramref name="selectionStartX"/> and <paramref name="selectionStartY"/> were determined when the user first pressed the trigger.
        /// </summary>
        /// <param name="hitx">The last x coordinate that the raycast hit.</param>
        /// <param name="hity">The last y coordinate that the raycast hit.</param>
        /// <param name="selectionStartX">The first x coordinate that the raycast hit.</param>
        /// <param name="selectionStartY">The first y coordinate that the raycast hit.</param>
        public void HandleBoxSelection(int hitx, int hity, int selectionStartX, int selectionStartY)
        {
            // since the groupings have irregular widths we need to iterate over the list of widths
            float boxX = heatmap.layout.heatmapX;
            float boxWidth = 0;
            for (int i = 0; i < heatmap.groupWidths.Count; ++i)
            {
                if (boxX + heatmap.groupWidths[i].Item2 > hitx || boxX + heatmap.groupWidths[i].Item2 > selectionStartX)
                {
                    do
                    {
                        boxWidth += heatmap.groupWidths[i].Item2;
                        i++;
                    } while (boxX + boxWidth < hitx || boxX + boxWidth < selectionStartX);

                    break;
                }

                boxX += heatmap.groupWidths[i].Item2;
            }

            float highlightMarkerWidth = boxWidth / heatmap.layout.bitmapWidth;
            float highlightMarkerX = boxX / heatmap.layout.bitmapWidth + highlightMarkerWidth / 2 - 0.5f;

            // the genes all have the same height so no need for loops here
            int geneHit1 = (int)((float)((heatmap.layout.bitmapHeight - hity) - heatmap.layout.heatmapY) / heatmap.layout.heatmapHeight *
                                  heatmap.genes.Length);
            int geneHit2 = (int)((float)((heatmap.layout.bitmapHeight - selectionStartY) - heatmap.layout.heatmapY) /
                heatmap.layout.heatmapHeight * heatmap.genes.Length);
            int smallerGeneHit = geneHit1 < geneHit2 ? geneHit1 : geneHit2;
            float highlightMarkerHeight = ((float)heatmap.layout.heatmapHeight / heatmap.layout.bitmapHeight) /
                heatmap.genes.Length * (Math.Abs(geneHit1 - geneHit2) + 1);
            float highlightMarkerY =
                -((float)heatmap.layout.heatmapY + smallerGeneHit * ((float)heatmap.layout.heatmapHeight / heatmap.genes.Length)) /
                heatmap.layout.bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

            heatmap.highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            heatmap.highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            heatmap.highlightQuad.SetActive(true);
            heatmap.highlightInfoText.text = "";
        }

        /// <summary>
        /// Confirms the cells inside the rectangle drawn by the user. <paramref name="hitx"/> and <paramref name="hity"/> are determined on this frame,
        /// <paramref name="selectionStartX"/> and <paramref name="selectionStartY"/> were determined when the user first pressed the trigger.
        /// </summary>
        /// <param name="hitx">The last x coordinate that the raycast hit.</param>
        /// <param name="hity">The last y coordinate that the raycast hit.</param>
        /// <param name="selectionStartX">The first x coordinate that the raycast hit when the user first pressed the trigger.</param>
        /// <param name="selectionStartY">The first y coordinate that the raycast hit when the user first pressed the trigger.</param>
        public void ConfirmSelection(int hitx, int hity, int selectionStartX, int selectionStartY)
        {
            selecting = false;
            // since the groupings have irregular widths we need to iterate over the list of widths
            selectedBoxX = heatmap.layout.heatmapX;
            selectedBoxWidth = 0;

            selectedGroupLeft = 0;
            // the do while loop below increments selectedGroupRight one time too many, so start at -1
            selectedGroupRight = -1;
            selectedGeneBottom = 0;
            selectedGeneTop = 0;

            for (int i = 0; i < heatmap.groupWidths.Count; ++i)
            {
                if (selectedBoxX + heatmap.groupWidths[i].Item2 > hitx ||
                    selectedBoxX + heatmap.groupWidths[i].Item2 > selectionStartX)
                {
                    do
                    {
                        selectedGroupRight++;
                        selectedBoxWidth += heatmap.groupWidths[i].Item2;
                        i++;
                    } while (selectedBoxX + selectedBoxWidth < hitx ||
                             selectedBoxX + selectedBoxWidth < selectionStartX);

                    break;
                }

                selectedBoxX += heatmap.groupWidths[i].Item2;
                selectedGroupLeft++;
                selectedGroupRight++;
            }

            float highlightMarkerWidth = selectedBoxWidth / heatmap.layout.bitmapWidth;
            float highlightMarkerX = selectedBoxX / heatmap.layout.bitmapWidth + highlightMarkerWidth / 2 - 0.5f;

            // the genes all have the same height so no need for loops here
            int geneHit1 = (int)((float)((heatmap.layout.bitmapHeight - hity) - heatmap.layout.heatmapY) / heatmap.layout.heatmapHeight *
                                  heatmap.genes.Length);
            int geneHit2 = (int)((float)((heatmap.layout.bitmapHeight - selectionStartY) - heatmap.layout.heatmapY) /
                heatmap.layout.heatmapHeight * heatmap.genes.Length);
            if (geneHit1 < geneHit2)
            {
                selectedGeneTop = geneHit1;
                selectedGeneBottom = geneHit2;
            }
            else
            {
                selectedGeneTop = geneHit2;
                selectedGeneBottom = geneHit1;
            }

            // have to add 1 at the end here so it includes the bottom row as well
            selectedBoxHeight = ((float)heatmap.layout.heatmapHeight) / heatmap.genes.Length *
                                (Math.Abs(geneHit1 - geneHit2) + 1);
            float highlightMarkerHeight = selectedBoxHeight / heatmap.layout.bitmapHeight;
            selectedBoxY = (float)heatmap.layout.heatmapY +
                           selectedGeneTop * ((float)heatmap.layout.heatmapHeight / heatmap.genes.Length);
            float highlightMarkerY = -(selectedBoxY) / heatmap.layout.bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

            heatmap.confirmQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            heatmap.confirmQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            heatmap.confirmQuad.SetActive(true);
        }

        /// <summary>
        /// Moves the <see cref="movingQuadX"/> and <see cref="movingQuadY"/> when choosing where to move a selection
        /// </summary>
        /// <param name="hitx">The x coordinate where the raycast hit the heatmap</param>
        /// <param name="hity">The y coordinate where the raycast hit the heatmap</param>
        public void HandleMovingSelection(int hitx, int hity)
        {
            if (hitx < selectedBoxX || hitx > selectedBoxX + selectedBoxWidth)
            {
                float groupX, groupWidth;
                int group;
                FindGroupInfo(hitx, out groupX, out groupWidth, out group);
                if (hitx > groupX + groupWidth / 2f)
                {
                    groupX += groupWidth;
                }

                float highlightMarkerX = groupX / heatmap.layout.bitmapWidth +
                    heatmap.layout.heatmapWidth / (2 * heatmap.layout.bitmapWidth) - 0.5f;
                heatmap.movingQuadY.transform.localPosition = new Vector3(highlightMarkerX, 0f, -0.001f);
                heatmap.movingQuadY.SetActive(true);
            }
            else
            {
                heatmap.movingQuadY.SetActive(false);
            }

            if (heatmap.layout.bitmapHeight - hity < selectedBoxY ||
                heatmap.layout.bitmapHeight - hity > selectedBoxY + selectedBoxHeight)
            {
                int geneHit =
                    (int)(((heatmap.layout.bitmapHeight - hity + ((float)heatmap.layout.heatmapHeight / heatmap.genes.Length) / 2) -
                            heatmap.layout.heatmapY) / heatmap.layout.heatmapHeight * heatmap.genes.Length);
                float highlightMarkerY =
                    -((float)heatmap.layout.heatmapY + geneHit * ((float)heatmap.layout.heatmapHeight / heatmap.genes.Length)) /
                    heatmap.layout.bitmapHeight + 0.5f;
                heatmap.movingQuadX.transform.localPosition = new Vector3(0f, highlightMarkerY, -0.001f);
                heatmap.movingQuadX.SetActive(true);
            }
            else
            {
                heatmap.movingQuadX.SetActive(false);
            }
        }

        /// <summary>
        /// Resets the selection on the heatmap.
        /// </summary>
        private void ResetSelection()
        {
            heatmap.confirmQuad.SetActive(false);
            heatmap.movingQuadX.SetActive(false);
            heatmap.movingQuadY.SetActive(false);

            selectedBoxX = 0;
            selectedBoxY = 0;
            selectedBoxHeight = 0;
            selectedBoxWidth = 0;

            selectedGeneBottom = 0;
            selectedGeneTop = 0;
            selectedGroupLeft = 0;
            selectedGroupRight = 0;
        }

        public void ResetSelecting()
        {
            selecting = false;
            movingSelection = false;
        }

        private void ResetHeatmapHighlight(bool resetText = true)
        {
            currentHighlightedGroup = -1;
            cellManager.ResetHighlight();
            if (resetText) heatmap.ResetHeatmapHighlight();
        }

        /// <summary>
        /// Moves a part of the heatmap to another part. This can mean moving both rows and coloumns. Entire rows and coloumns are always moved and never split.
        /// </summary>
        /// <param name="hitx">The x coordinate where the selection should be moved to.</param>
        /// <param name="hity">The y coordinate where the selection should be moved to.</param>
        /// <param name="selectedGroupLeft">The lower index of the groups that should be moved.</param>
        /// <param name="selectedGroupRight">The higher index of the groups that should be moved.</param>
        /// <param name="selectedGeneTop">The lower index of the genes that should be moved.</param>
        /// <param name="selectedGeneBottom">The higher index of the genes that should be moved.</param>
        public void MoveSelection(int hitx, int hity, int selectedGroupLeft, int selectedGroupRight,
            int selectedGeneTop, int selectedGeneBottom)
        {
            movingSelection = false;
            bool recalculate = false;
            if (hitx < selectedBoxX || hitx > selectedBoxX + selectedBoxWidth)
            {
                int nbrOfGroups = selectedGroupRight - selectedGroupLeft + 1;
                int groupIndexToMoveTo = 0;
                float groupX = heatmap.layout.heatmapX;
                while (groupX + heatmap.groupWidths[groupIndexToMoveTo].Item2 < hitx)
                {
                    groupX += heatmap.groupWidths[groupIndexToMoveTo].Item2;
                    groupIndexToMoveTo++;
                }

                if (hitx > groupX + heatmap.groupWidths[groupIndexToMoveTo].Item2 / 2f)
                {
                    groupIndexToMoveTo++;
                }

                List<Tuple<int, float, int>> groupWidthsToMove = new List<Tuple<int, float, int>>(nbrOfGroups);
                // add the groups we are moving to a temporary list
                groupWidthsToMove.AddRange(heatmap.groupWidths.GetRange(selectedGroupLeft, nbrOfGroups));
                // we have to do this for both the groups and the cells
                // figure out the index that the first group is on in the cells list
                int cellsStartIndex = 0;
                foreach (Tuple<int, float, int> t in heatmap.groupWidths.GetRange(0, selectedGroupLeft))
                {
                    cellsStartIndex += t.Item3;
                }

                int cellsStartIndexToMoveTo = 0;
                foreach (Tuple<int, float, int> t in heatmap.groupWidths.GetRange(0, groupIndexToMoveTo))
                {
                    cellsStartIndexToMoveTo += t.Item3;
                }

                // figure out how many cells the groups cover in total
                int totalNbrOfCells = 0;
                foreach (Tuple<int, float, int> t in groupWidthsToMove)
                {
                    totalNbrOfCells += t.Item3;
                }

                // the correct index to move the groups to will have changed if the groups are moved to higher indices 
                if (groupIndexToMoveTo > selectedGroupRight)
                {
                    groupIndexToMoveTo -= nbrOfGroups;
                    cellsStartIndexToMoveTo -= totalNbrOfCells;
                }

                heatmap.groupWidths.RemoveRange(selectedGroupLeft, nbrOfGroups);
                heatmap.groupWidths.InsertRange(groupIndexToMoveTo, groupWidthsToMove);

                // here we need swap some stuff around in the cells array
                // figure out the index of the other part of the array that we need to move
                int otherPartStartIndex = cellsStartIndex < cellsStartIndexToMoveTo
                    ? cellsStartIndex + totalNbrOfCells
                    : cellsStartIndexToMoveTo;
                // figure out how many cells are inbetween the indeces. this is the same number of cells that the other part contains
                int numberOfcellsInOtherPart = Math.Abs(cellsStartIndex - cellsStartIndexToMoveTo);
                // figure out the index that the other part is moving to
                int otherPartIndexToMoveTo = cellsStartIndex < cellsStartIndexToMoveTo
                    ? cellsStartIndex
                    : cellsStartIndexToMoveTo + totalNbrOfCells;
                // temporary array with the cells we should move
                Cell[] cellsToMove = new Cell[totalNbrOfCells];
                // move the cells into the temporary array
                Array.Copy(heatmap.cells, cellsStartIndex, cellsToMove, 0, totalNbrOfCells);
                // move the part we are swapping with to its new location
                Array.Copy(heatmap.cells, otherPartStartIndex, heatmap.cells, otherPartIndexToMoveTo,
                    numberOfcellsInOtherPart);
                // move the cells from the temporary array to their new location
                Array.Copy(cellsToMove, 0, heatmap.cells, cellsStartIndexToMoveTo, totalNbrOfCells);

                recalculate = true;
            }

            if (heatmap.layout.bitmapHeight - hity < selectedBoxY ||
                heatmap.layout.bitmapHeight - hity > selectedBoxY + selectedBoxHeight)
            {
                int nbrOfGenes = selectedGeneBottom - selectedGeneTop + 1;
                int geneIndex =
                    (int)(((heatmap.layout.bitmapHeight - hity + ((float)heatmap.layout.heatmapHeight / heatmap.genes.Length) / 2) -
                            heatmap.layout.heatmapY) / heatmap.layout.heatmapHeight * heatmap.genes.Length);
                // Take the list of genes orignal genes
                List<string> original = new List<string>(heatmap.genes);
                // make a temporary list with enough space for what should be moved
                List<string> temp = new List<string>(nbrOfGenes);
                // add what should be moved to the temporary list
                temp.AddRange(original.GetRange(selectedGeneTop, nbrOfGenes));
                // remove what should be moved from the original list
                original.RemoveRange(selectedGeneTop, nbrOfGenes);
                // recalculate the index if needed. Since we removed stuff from the original list the indeces might have shifted
                if (geneIndex > selectedGeneTop)
                {
                    geneIndex -= nbrOfGenes;
                }

                // insert what should be moved back into the original
                original.InsertRange(geneIndex, temp);
                heatmap.genes = original.ToArray();
                recalculate = true;
            }

            heatmap.UpdateAttributeWidhts();
            if (recalculate)
            {
                // rebuild the heatmap texture
                if (heatmap.orderedByAttribute)
                {
                    heatmap.ReorderByAttribute();
                }
                else
                {
                    heatmapGenerator.BuildTexture(heatmap);
                }
            }
        }

        public void CreateNewHeatmapFromSelection()
        {
            multiuserMessageSender.SendMessageCreateNewHeatmapFromSelection(heatmap.name, selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom, selectedBoxWidth, selectedBoxHeight);
            heatmap.CreateNewHeatmapFromSelection(selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom, selectedBoxWidth, selectedBoxHeight);
        }

        public void CumulativeRecolourFromSelection()
        {
            //multiuserMessageSender.SendMessageCreateNewHeatmapFromSelection(heatmap.name, selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom, selectedBoxWidth, selectedBoxHeight);
            heatmap.CumulativeRecolorFromSelection(selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom);

        }
    }
}