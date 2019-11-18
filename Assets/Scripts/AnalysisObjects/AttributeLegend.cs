using CellexalVR.AnalysisObjects;
using System.Collections.Generic;
using UnityEngine;

public class AttributeLegend : MonoBehaviour
{
    public GameObject entryPrefab;

    private List<AttributeLegendEntry> entries = new List<AttributeLegendEntry>();
    private int activeCells = 0;
    private Graph parentGraph;

    private Vector3 startPos = new Vector3(0f, 0.1968f, 0f);
    private Vector3 posInc = new Vector3(0f, -0.0416f, 0f);

    private void Start()
    {
        parentGraph = GetComponentInParent<Graph>();
    }

    public void AddAttribute(string attributeName, int numberOfCells, Color attributeColor)
    {
        GameObject newEntryGameObject = Instantiate(entryPrefab);
        newEntryGameObject.SetActive(true);
        newEntryGameObject.transform.parent = transform;
        newEntryGameObject.transform.localPosition = startPos + posInc * entries.Count;
        newEntryGameObject.transform.localRotation = Quaternion.identity;
        // TODO CELLEXAL: set positions and parents of entryPrefab
        AttributeLegendEntry newEntry = newEntryGameObject.GetComponent<AttributeLegendEntry>();
        activeCells += numberOfCells;

        string percentOfActiveString = ((float)numberOfCells / activeCells).ToString("P");
        string percentOfAllString = ((float)numberOfCells / parentGraph.points.Count).ToString("P");
        newEntry.SetPanelText(attributeName, numberOfCells, percentOfActiveString, percentOfAllString, attributeColor);
        UpdatePercentages();
        entries.Add(newEntry);
    }

    public void RemoveAttribute(string attributeType)
    {
        int index = entries.FindIndex((item) => item.attributeNameText.text == attributeType);
        AttributeLegendEntry entry = entries[index];
        entries.RemoveAt(index);
        activeCells -= entry.numberOfCells;
        Destroy(entry.gameObject);
        UpdatePercentages();
        UpdatePositions();
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < entries.Count; ++i)
        {
            entries[i].transform.localPosition = startPos + i * posInc;
        }
    }

    private void UpdatePercentages()
    {
        foreach (AttributeLegendEntry remainingEntry in entries)
        {
            int numberOfCells = remainingEntry.numberOfCells;
            string percentOfActiveString = ((float)numberOfCells / activeCells).ToString("P");
            string percentOfAllString = ((float)numberOfCells / parentGraph.points.Count).ToString("P");
            remainingEntry.UpdatePercentages(percentOfActiveString, percentOfAllString);
        }
    }
}
