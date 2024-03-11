using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AnalysisLogic
{
    public class BoxPlotManager : EnvironmentMenuWithTabs
    {
        private Selection selection;

        public void GenerateBoxPlots(Selection selection)
        {
            this.selection = selection;
            Dictionary<(int group, string facs), List<float>> values = new Dictionary<(int group, string facs), List<float>>();
            CellManager cellManager = ReferenceManager.instance.cellManager;
            string[] facsNames = cellManager.Facs;
            for (int i = 0; i < facsNames.Length; ++i)
            {
                facsNames[i] = facsNames[i].ToLower();
            }

            // create a grid for all groups combined
            BoxPlotGrid gridAllGroups = AddTab() as BoxPlotGrid;

            for (int j = 0; j < facsNames.Length; ++j)
            {
                values[(-1, facsNames[j])] = new List<float>();
            }

            // create a boxplotgrid for each group and create lists for each facs in each group
            for (int i = 0; i < selection.groups.Count; ++i)
            {
                BoxPlotGrid grid = AddTab() as BoxPlotGrid;

                for (int j = 0; j < facsNames.Length; ++j)
                {
                    values[(selection.groups[i], facsNames[j])] = new List<float>();
                }
            }


            // populate lists
            foreach (Graph.GraphPoint point in selection)
            {
                for (int j = 0; j < facsNames.Length; ++j)
                {
                    float facs = cellManager.GetCell(point.Label).Facs[facsNames[j]];
                    values[(point.Group, facsNames[j])].Add(facs);
                    values[(-1, facsNames[j])].Add(facs);
                }
            }

            // generate the boxplots
            // generate the combined groups
            gridAllGroups.gameObject.name = "All groups";
            gridAllGroups.SetSelection(selection);
            for (int j = 0; j < facsNames.Length; ++j)
            {
                gridAllGroups.GenerateBoxPlot(values[(-1, facsNames[j])], facsNames[j]);
            }

            // generate the individual groups
            for (int i = 0; i < selection.groups.Count; ++i)
            {
                BoxPlotGrid tab = (BoxPlotGrid)tabs[i + 1];
                tab.SetSelection(selection, selection.groups[i]);
                for (int j = 0; j < facsNames.Length; ++j)
                {
                    tab.gameObject.name = "Group " + selection.groups[i];
                    tab.GenerateBoxPlot(values[(selection.groups[i], facsNames[j])], facsNames[j]);
                    tab.tabButton.meshStandardColor = selection.colors[i];
                    tab.tabButton.GetComponent<MeshRenderer>().material.color = selection.colors[i];
                }
            }

            foreach (BoxPlotGrid grid in tabs)
            {
                grid.ResizeAllBoxPlots();
            }

            SwitchToTab(tabs[0]);
        }

        public void RecolourSelection()
        {
            ReferenceManager.instance.graphManager.ColorAllGraphsBySelection(selection);
        }
    }
}
