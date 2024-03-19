﻿using UnityEngine.Events;
namespace CellexalVR.General
{
    /// <summary>
    /// This class contains the events that scripts can subscribe to to.
    /// </summary>
    public static class CellexalEvents
    {
        public static UnityEvent GraphsLoaded = new UnityEvent();
        public static UnityEvent<CellexalVR.AnalysisObjects.Graph> GraphCreated = new UnityEvent<CellexalVR.AnalysisObjects.Graph>();
        public static UnityEvent GraphsUnloaded = new UnityEvent();
        public static UnityEvent ScarfObjectLoaded = new UnityEvent();
        public static UnityEvent ScarfUMAPFinished = new UnityEvent();
        public static UnityEvent OutputReceived = new UnityEvent();

        public static UnityEvent PDFArticleRead = new UnityEvent();

        public static UnityEvent SelectionStarted = new UnityEvent();
        public static UnityEvent SelectionConfirmed = new UnityEvent();
        public static UnityEvent SelectionCanceled = new UnityEvent();
        public static UnityEvent SelectedFromFile = new UnityEvent();

        public static UnityEvent AnnotationsCleared = new UnityEvent();


        public static UnityEvent BeginningOfHistoryReached = new UnityEvent();
        public static UnityEvent BeginningOfHistoryLeft = new UnityEvent();
        public static UnityEvent EndOfHistoryLeft = new UnityEvent();
        public static UnityEvent EndOfHistoryReached = new UnityEvent();

        public static UnityEvent CreatingHeatmap = new UnityEvent();
        public static UnityEvent HeatmapCreated = new UnityEvent();
        public static UnityEvent HeatmapBurned = new UnityEvent();
        public static UnityEvent CreatingNetworks = new UnityEvent();
        public static UnityEvent NetworkCreated = new UnityEvent();
        public static UnityEvent NetworkEnlarged = new UnityEvent();
        public static UnityEvent NetworkUnEnlarged = new UnityEvent();
        public static UnityEvent ScriptRunning = new UnityEvent();
        public static UnityEvent ScriptFinished = new UnityEvent();
        public static UnityEvent KeyboardToggled = new UnityEvent();
        public static UnityEvent ObjectGrabbed = new UnityEvent();
        public static UnityEvent ObjectUngrabbed = new UnityEvent();


        public static UnityEvent GraphsReset = new UnityEvent();
        public static UnityEvent GraphsResetKeepSelection = new UnityEvent();
        public static UnityEvent GraphsColoredByGene = new UnityEvent();
        public static UnityEvent GraphsColoredByIndex = new UnityEvent();
        public static UnityEvent CorrelatedGenesCalculated = new UnityEvent();
        public static UnityEvent ColorTextureUpdated = new UnityEvent();

        public static UnityEvent LinesBetweenGraphsDrawn = new UnityEvent();
        public static UnityEvent LinesBetweenGraphsCleared = new UnityEvent();

        public static UnityEvent FlashGenesFileStartedLoading = new UnityEvent();
        public static UnityEvent FlashGenesFileFinishedLoading = new UnityEvent();

        public static UnityEvent FilterLoaded = new UnityEvent();
        public static UnityEvent FilterActivated = new UnityEvent();
        public static UnityEvent FilterDeactivated = new UnityEvent();


        public static UnityEvent ConfigLoaded = new UnityEvent();

        public static UnityEvent QueryTopGenesStarted = new UnityEvent();
        public static UnityEvent QueryTopGenesFinished = new UnityEvent();

        public static UnityEvent LegendAttached = new UnityEvent();
        public static UnityEvent LegendDetached = new UnityEvent();
        public static UnityEvent MenuClosed = new UnityEvent();
        public static UnityEvent ModelChanged = new UnityEvent();
        public static UnityEvent ControllersInitiated = new UnityEvent();
        public static UnityEvent CullingCubeSpawned = new UnityEvent();
        public static UnityEvent CullingCubeRemoved = new UnityEvent();

        public static UnityEvent UsernameChanged = new UnityEvent();
        public static UnityEvent LogInitialized = new UnityEvent();

        // Open XR input events
        public static UnityEvent RightTriggerClick = new UnityEvent();
        public static UnityEvent RightTriggerPressed = new UnityEvent();
        public static UnityEvent RightTriggerUp = new UnityEvent();
        public static UnityEvent LeftTriggerClick = new UnityEvent();
        public static UnityEvent LeftTriggerPressed = new UnityEvent();
        public static UnityEvent LeftTriggerUp = new UnityEvent();


        // GeoMX Loading Events
        public static UnityEvent LoadingImages = new UnityEvent();
        public static UnityEvent ImagesLoaded = new UnityEvent();

        // Scarf Events
        public static UnityEvent ZarrConversionComplete = new UnityEvent();
        public static UnityEvent DataStaged = new UnityEvent();
        public static UnityEvent DataLoaded = new UnityEvent();


        public class CommandFinishedEvent : UnityEvent<bool> { }
        public static CommandFinishedEvent CommandFinished = new CommandFinishedEvent();

        //public static UnityEvent FlashGenesCategoryToggled

        public static bool IsPersistentListenerAlreadyAdded(UnityEvent unityEvent, UnityAction action)
        {
            for (int i = 0; i < unityEvent.GetPersistentEventCount(); ++i)
            {
                if (unityEvent.GetPersistentMethodName(i).Equals(action.Method.Name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
