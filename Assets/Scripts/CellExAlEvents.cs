﻿using UnityEngine.Events;

/// <summary>
/// This class contains the events that scripts can subscribe to to.
/// </summary>
public static class CellExAlEvents
{
    public static UnityEvent GraphsLoaded = new UnityEvent();
    public static UnityEvent GraphsUnloaded = new UnityEvent();

    public static UnityEvent SelectionStarted = new UnityEvent();
    public static UnityEvent SelectionConfirmed = new UnityEvent();
    public static UnityEvent SelectionCanceled = new UnityEvent();

    public static UnityEvent BeginningOfHistoryReached = new UnityEvent();
    public static UnityEvent BeginningOfHistoryLeft = new UnityEvent();
    public static UnityEvent EndOfHistoryReached = new UnityEvent();
    public static UnityEvent EndOfHistoryLeft = new UnityEvent();

    public static UnityEvent HeatmapCreated = new UnityEvent();
    public static UnityEvent NetworkCreated = new UnityEvent();

    public static UnityEvent GraphsReset = new UnityEvent();
    public static UnityEvent GraphsColoredByGene = new UnityEvent();

    public static UnityEvent LinesBetweenGraphsDrawn = new UnityEvent();
    public static UnityEvent LinesBetweenGraphsCleared = new UnityEvent();

    public static UnityEvent FlashGenesFileStartedLoading = new UnityEvent();
    public static UnityEvent FlashGenesFileFinishedLoading = new UnityEvent();

    public static UnityEvent ConfigLoaded = new UnityEvent();
    //public static UnityEvent FlashGenesCategoryToggled
}