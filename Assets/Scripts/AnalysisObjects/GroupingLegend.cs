using CellexalVR.Filters;
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
        public GameObject extraColumn;
        public TMPro.TextMeshPro pageNumberText;

        /// <summary>
        /// List of pages with entries, access an entry with <code>entries[pageNbr][entryNbr]</code>
        /// </summary>
        private List<List<GroupingLegendEntry>> entries = new List<List<GroupingLegendEntry>>();
        private int currentPageNbr = 0;
        private int maxEntriesPerPage = 8;
        private int addEntryToPageIndex = 0;
        private int activeCells = 0;
        private bool attached;

        private Vector3 startPos = new Vector3(0f, 0.1968f, 0f);
        private Vector3 posInc = new Vector3(0f, -0.0416f, 0f);

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            CellexalEvents.LegendAttached.AddListener(ActivateExtraColumn);
            CellexalEvents.LegendDetached.AddListener(DeActivateExtraColumn);
            CellexalEvents.GraphsReset.AddListener(ClearLegend);
            CellexalEvents.GraphsColoredByGene.AddListener(ClearLegend);
            CellexalEvents.GraphsColoredByIndex.AddListener(ClearLegend);
        }

        private void ActivateExtraColumn()
        {
            extraColumn.SetActive(true);
            GroupingLegendEntry groupingLegendEntry = entryPrefab.GetComponent<GroupingLegendEntry>();
            AdjustEntry(groupingLegendEntry, true);
            foreach (List<GroupingLegendEntry> page in entries)
            {
                foreach (GroupingLegendEntry entry in page)
                {
                    AdjustEntry(entry, true);
                }
            }

            attached = true;
        }
        private void DeActivateExtraColumn()
        {
            extraColumn.SetActive(false);
            GroupingLegendEntry groupingLegendEntry = entryPrefab.GetComponent<GroupingLegendEntry>();
            AdjustEntry(groupingLegendEntry, false);
            foreach (List<GroupingLegendEntry> page in entries)
            {
                foreach (GroupingLegendEntry entry in page)
                {
                    AdjustEntry(entry, false);
                }
            }

            attached = false;
        }

        private void AdjustEntry(GroupingLegendEntry entry, bool toggle)
        {
            Vector3 scale = entry.topDivider.transform.localScale;
            scale.x = toggle ? 0.064f : 0.057f;
            entry.topDivider.transform.localScale = scale;
            Vector3 pos = entry.topDivider.transform.localPosition;
            pos.x = toggle ? 0.03f : 0f;
            entry.topDivider.transform.localPosition = pos;
            entry.filterButton.GetComponent<AttributeFilterButton>().toggle = false;
            entry.filterButton.GetComponent<AttributeFilterButton>().ToggleOutline(false);
            entry.filterButton.SetActive(toggle);
        }

        /// <summary>
        /// Adds a new group to the legend.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <param name="numberOfCells">The number of cells the group has.</param>
        /// <param name="color">The color that represents the group.</param>
        public void AddEntry(string groupName, int numberOfCells, Color color)
        {
            if (entries.Count == 0)
            {
                entries.Add(new List<GroupingLegendEntry>());
            }

            if (entries[addEntryToPageIndex].Count >= maxEntriesPerPage)
            {
                entries.Add(new List<GroupingLegendEntry>());
                addEntryToPageIndex++;
                pageNumberText.text = "Page " + (currentPageNbr + 1) + " / " + entries.Count;
            }

            GameObject newEntryGameObject = Instantiate(entryPrefab);
            newEntryGameObject.SetActive(true);
            newEntryGameObject.transform.parent = transform;
            newEntryGameObject.transform.localPosition = startPos + posInc * entries[addEntryToPageIndex].Count;
            newEntryGameObject.transform.localRotation = Quaternion.identity;
            GroupingLegendEntry newEntry = newEntryGameObject.GetComponent<GroupingLegendEntry>();
            newEntry.filterButton.SetActive(attached);
            newEntry.filterButton.GetComponent<AttributeFilterButton>().group = groupName;
            activeCells += numberOfCells;

            string percentOfSelectedString = ((float)numberOfCells / activeCells).ToString("P");
            string percentOfAllString = ((float)numberOfCells / referenceManager.cellManager.GetNumberOfCells()).ToString("P");
            newEntry.SetPanelText(groupName, numberOfCells, percentOfSelectedString, percentOfAllString, color);
            UpdatePercentages();
            newEntry.transform.localScale = Vector3.one;

            entries[addEntryToPageIndex].Add(newEntry);

            if (addEntryToPageIndex != currentPageNbr)
            {
                newEntryGameObject.SetActive(false);
            }

        }

        /// <summary>
        /// Removes a group from the legend.
        /// </summary>
        /// <param name="groupName">The name of the group to remove.</param>
        public void RemoveEntry(string groupName)
        {
            int pageIndex = 0;
            int index = -1;
            // find the right page
            for (; pageIndex < entries.Count; ++pageIndex)
            {
                index = entries[pageIndex].FindIndex((item) => item.groupName.text == groupName);
                if (index != -1)
                {
                    // remove the entry and return
                    GroupingLegendEntry entry = entries[pageIndex][index];
                    entries.RemoveAt(index);
                    activeCells -= entry.numberOfCells;
                    Destroy(entry.gameObject);
                    UpdatePercentages();
                    UpdatePositions();
                    return;
                }
            }
        }

        private void ClearLegend()
        {
            foreach (List<GroupingLegendEntry> page in entries)
            {
                foreach (GroupingLegendEntry entry in page)
                {
                    Destroy(entry.gameObject);
                }
            }
            entries.Clear();
            currentPageNbr = 0;
        }

        /// <summary>
        /// Updates the positions of all groups. Should be called when a group is removed.
        /// </summary>
        private void UpdatePositions()
        {

            for (int pageNbr = 0; pageNbr < entries.Count; pageNbr++)
            {
                List<GroupingLegendEntry> page = entries[pageNbr];
                if (pageNbr < entries.Count - 2 && page.Count < 8 && entries[pageNbr + 1].Count > 0)
                {
                    // too few entries in this page, take some from the next page
                    page.AddRange(entries[pageNbr + 1].Take(8 - page.Count));
                }

                for (int i = 0; i < page.Count; ++i)
                {
                    page[i].transform.localPosition = startPos + i * posInc;
                }
            }

            if (entries[entries.Count - 1].Count == 0)
            {
                // remove the last page if it is empty
                entries.RemoveAt(entries.Count - 1);
            }


        }

        /// <summary>
        /// Updates the percentages of all groups. Should be called when group is added or updated.
        /// </summary>
        private void UpdatePercentages()
        {
            foreach (List<GroupingLegendEntry> page in entries)
            {
                foreach (GroupingLegendEntry remainingEntry in page)
                {
                    int numberOfCells = remainingEntry.numberOfCells;
                    remainingEntry.numberOfCellsText.text = numberOfCells.ToString();
                    string percentOfSelectedString = ((float)numberOfCells / activeCells).ToString("P");
                    string percentOfAllString = ((float)numberOfCells / referenceManager.cellManager.GetNumberOfCells()).ToString("P");
                    remainingEntry.UpdatePercentages(percentOfSelectedString, percentOfAllString);
                }
            }
        }

        /// <summary>
        /// Updates an existing group or, if no group with the specified name exists, creates a new group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <param name="numberOfCellsToAdd">The number of cells to add or remove from the group.</param>
        /// <param name="color">The color the group should have if it is created. If <paramref name="numberOfCellsToAdd"/> is negative (cells are removed from the group), this parameter does not matter.</param>
        public void AddOrUpdateEntry(string groupName, int numberOfCellsToAdd, Color color)
        {
            GroupingLegendEntry foundEntry = null;
            int pageIndex = 0;
            while (foundEntry == null && pageIndex < entries.Count)
            {
                foundEntry = entries[pageIndex].FirstOrDefault((entry) => entry.groupName.text == groupName);
                pageIndex++;
            }
            if (foundEntry)
            {
                foundEntry.numberOfCells += numberOfCellsToAdd;
                if (foundEntry.numberOfCells == 0)
                {
                    RemoveEntry(groupName);
                }
                UpdatePercentages();
            }
            else
            {
                AddEntry(groupName, numberOfCellsToAdd, color);
                //if (attached)
                //{
                //    referenceManager.cullingFilterManager.AddSelectionGroupToFilter(groupName);
                //}
            }

            activeCells += numberOfCellsToAdd;
        }

        /// <summary>
        /// Changes the current page.
        /// </summary>
        /// <param name="incrementPageNbr">True if the page number should be incremented by one, false if it should be decremented by one.</param>
        public void ChangePage(bool incrementPageNbr)
        {
            if (currentPageNbr == 0 && !incrementPageNbr ||
                currentPageNbr == entries.Count - 1 && incrementPageNbr)
            {
                return;
            }

            foreach (GroupingLegendEntry entry in entries[currentPageNbr])
            {
                entry.gameObject.SetActive(false);
            }

            currentPageNbr += incrementPageNbr ? 1 : -1;

            foreach (GroupingLegendEntry entry in entries[currentPageNbr])
            {
                entry.gameObject.SetActive(true);
            }

            pageNumberText.text = "Page " + (currentPageNbr + 1) + " / " + entries.Count;
        }
    }
}
