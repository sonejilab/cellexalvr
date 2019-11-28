using CellexalVR.General;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{
    /// <summary>
    /// Represents a legend that contains <see cref="GroupingLegendEntry"/>.
    /// </summary>
    public class GroupingLegend : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject entryPrefab;

        private List<GroupingLegendEntry> entries = new List<GroupingLegendEntry>();
        private int activeCells = 0;

        private Vector3 startPos = new Vector3(0f, 0.1968f, 0f);
        private Vector3 posInc = new Vector3(0f, -0.0416f, 0f);

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        /// <summary>
        /// Adds a new group to the legend.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <param name="numberOfCells">The number of cells the group has.</param>
        /// <param name="color">The color that represents the group.</param>
        public void AddGroup(string groupName, int numberOfCells, Color color)
        {
            GameObject newEntryGameObject = Instantiate(entryPrefab);
            newEntryGameObject.SetActive(true);
            newEntryGameObject.transform.parent = transform;
            newEntryGameObject.transform.localPosition = startPos + posInc * entries.Count;
            newEntryGameObject.transform.localRotation = Quaternion.identity;
            GroupingLegendEntry newEntry = newEntryGameObject.GetComponent<GroupingLegendEntry>();
            activeCells += numberOfCells;

            string percentOfSelectedString = ((float)numberOfCells / activeCells).ToString("P");
            string percentOfAllString = ((float)numberOfCells / referenceManager.cellManager.GetNumberOfCells()).ToString("P");
            newEntry.SetPanelText(groupName, numberOfCells, percentOfSelectedString, percentOfAllString, color);
            UpdatePercentages();
            entries.Add(newEntry);
        }

        /// <summary>
        /// Removes a group from the legend.
        /// </summary>
        /// <param name="groupName">The name of the group to remove.</param>
        public void RemoveGroup(string groupName)
        {
            int index = entries.FindIndex((item) => item.groupName.text == groupName);
            if (index != -1)
            {
                GroupingLegendEntry entry = entries[index];
                entries.RemoveAt(index);
                activeCells -= entry.numberOfCells;
                Destroy(entry.gameObject);
                UpdatePercentages();
                UpdatePositions();
            }
        }

        /// <summary>
        /// Updates the positions of all groups. Should be called when a group is removed.
        /// </summary>
        private void UpdatePositions()
        {
            for (int i = 0; i < entries.Count; ++i)
            {
                entries[i].transform.localPosition = startPos + i * posInc;
            }
        }

        /// <summary>
        /// Updates the percentages of all groups. Should be called when group is added or updated.
        /// </summary>
        private void UpdatePercentages()
        {
            foreach (GroupingLegendEntry remainingEntry in entries)
            {
                int numberOfCells = remainingEntry.numberOfCells;
                remainingEntry.numberOfCellsText.text = numberOfCells.ToString();
                string percentOfSelectedString = ((float)numberOfCells / activeCells).ToString("P");
                string percentOfAllString = ((float)numberOfCells / referenceManager.cellManager.GetNumberOfCells()).ToString("P");
                remainingEntry.UpdatePercentages(percentOfSelectedString, percentOfAllString);
            }
        }

        /// <summary>
        /// Updates an existing group or, if no group with the specified name exists, creates a new group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <param name="numberOfCellsToAdd">The number of cells to add or remove from the group.</param>
        /// <param name="color">The color the group should have if it is created. If <paramref name="numberOfCellsToAdd"/> is negative (cells are removed from the group), this parameter does not matter.</param>
        public void AddOrUpdateGroup(string groupName, int numberOfCellsToAdd, Color color)
        {
            GroupingLegendEntry foundEntry = entries.FirstOrDefault((entry) => entry.groupName.text == groupName);
            if (foundEntry)
            {
                foundEntry.numberOfCells += numberOfCellsToAdd;
                if (foundEntry.numberOfCells == 0)
                {
                    RemoveGroup(groupName);
                }
                UpdatePercentages();
            }
            else
            {
                AddGroup(groupName, numberOfCellsToAdd, color);
            }

            activeCells += numberOfCellsToAdd;
        }
    }
}
