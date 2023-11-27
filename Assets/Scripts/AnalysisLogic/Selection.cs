using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    public class Selection
    {
        public static string parentSelectionDirectory;

        public readonly int id;

        public int size;

        public readonly List<(int groupIndex, int groupSize)> groupSizes;

        public bool pointsLoaded;

        private NativeArray<GraphPointData> nativePointData;

        private List<Graph.GraphPoint> _points;
        /// <summary>
        /// The points that this selection includes, in the order they were selected.
        /// </summary>
        public List<Graph.GraphPoint> Points
        {
            get
            {
                if (_points is not null)
                {
                    return _points;
                }
                else
                {
                    _points = LoadSelectionFromDisk();
                    return _points;
                }
            }

            private set
            {
                _points = value;
            }
        }

        private string savedSelectionFilePath;

        /// <summary>
        /// Creates a new empty selection.
        /// </summary>
        /// <param name="id">This selection's id.</param>
        public Selection(int id)
        {
            this.id = id;
            this.groupSizes = new List<(int, int)>();
            SetPoints(new List<Graph.GraphPoint>());
        }

        /// <summary>
        /// Creates a new selection from a collection of selected points.
        /// </summary>
        /// <param name="id">The selection's id.</param>
        /// <param name="points">The collection to of points this selection should include.</param>
        public Selection(int id, IEnumerable<Graph.GraphPoint> points)
        {
            this.id = id;
            this.groupSizes = new List<(int, int)>();
            SetPoints(points);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Selection()
        {
            nativePointData.Dispose();
        }

        /// <summary>
        /// Sets this selections points. This overwrites any <see cref="Graph.GraphPoint"/> that were saved to this selection before.
        /// </summary>
        /// <param name="points">The points that should be set to this selection.</param>
        /// <param name="copyIfList">Optional. If <paramref name="points"/> isa <see cref="List{Graph.GraphPoint}"/> and this argument is set to false, this <see cref="Selection"/> will use the reference to the <see cref="List{Graph.GraphPoint}">. If set to true, the contents will be copied to a new <see cref="List{Graph.GraphPoint}<"/></param>
        /// <remarks>
        /// If <paramref name="points"/> is a <see cref="List{Graph.GraphPoint}"/> and <paramref name="copyIfList"/> is set to false, this function will use the reference to the <see cref="List{Graph.GraphPoint}"/> that was passed. 
        /// Otherwise, the contents will be copied to a new <see cref="List{Graph.GraphPoint}"/>.
        /// This increases performance considerably when the <see cref="List{Graph.GraphPoint}"/> that is passed with <paramref name="points"/> is not needed in the calling function after this.
        /// </remarks>
        public void SetPoints(IEnumerable<Graph.GraphPoint> points, bool copyIfList = false)
        {
            if (points is List<Graph.GraphPoint> pointsAsList)
            {
                if (copyIfList)
                {
                    _points = new List<Graph.GraphPoint>(pointsAsList);
                }
                else
                {
                    _points = pointsAsList;
                }
            }
            else
            {
                if (_points is null)
                {
                    _points = new List<Graph.GraphPoint>(points);
                }
                else
                {
                    _points.Clear();
                    _points.AddRange(points);
                }
            }

            nativePointData = new NativeArray<GraphPointData>(_points.Count, Allocator.Persistent);
            for (int i = 0; i < _points.Count; ++i)
            {
                nativePointData[i] = new GraphPointData() { group = _points[i].Group, texCoord = _points[i].textureCoord[0] };
            }
            NativeArray<int> nativeGroupSizes = new NativeArray<int>(CellexalConfig.Config.GraphNumberOfExpressionColors, Allocator.Temp);

            CalculatePointsMetaDataJob calcMetaJob = new CalculatePointsMetaDataJob()
            {
                points = nativePointData,
                groupSizes = nativeGroupSizes
            };
            JobHandle calcMetaHandle = calcMetaJob.Schedule(nativePointData.Length, 1000);
            calcMetaHandle.Complete();
            nativeGroupSizes.Dispose();

            size = _points.Count;
        }

        [BurstCompatible]
        private struct CalculatePointsMetaDataJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<GraphPointData> points;

            [WriteOnly]
            public NativeArray<int> groupSizes;

            public void Execute(int index)
            {
                groupSizes[points[index].group]++;
            }
        }

        public List<Graph.GraphPoint> LoadSelectionFromDisk()
        {
            _points = ReferenceManager.instance.inputReader.ReadSelectionFile(savedSelectionFilePath, false);
            pointsLoaded = true;
            return null;
        }

        public void UnloadSelection()
        {
            pointsLoaded = false;
            _points.Clear();
            _points = null;
        }

        public void SaveSelectionToDisk()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            // find relevant folder
            parentSelectionDirectory = Path.Combine(CellexalUser.UserSpecificFolder, "Selections"); // move this to selectionmanager
            if (!Directory.Exists(parentSelectionDirectory))
            {
                Directory.CreateDirectory(parentSelectionDirectory);
                CellexalLog.Log($"Created directory {parentSelectionDirectory}");
            }
            string currentSelectionDirectory = Path.Combine(parentSelectionDirectory, "Selection_" + id);
            if (!Directory.Exists(currentSelectionDirectory))
            {
                Directory.CreateDirectory(currentSelectionDirectory);
                CellexalLog.Log($"Created directory {currentSelectionDirectory}");
            }
            else
            {
                CellexalLog.Log($"Deleting all files in {currentSelectionDirectory}");
                foreach (string filePath in Directory.GetFiles(currentSelectionDirectory))
                {
                    File.Delete(filePath);
                }
            }
            // save selection as text file for R scripts
            savedSelectionFilePath = DumpSelectionToTextFile(currentSelectionDirectory);

            // create group-specific texture dictionary
            // maps a combination of a graph, lod group and selection group to the corresponding texture and its raw texture data array
            Dictionary<(Graph graph, int lodGroup, int group), Texture2D> textures = new();
            Dictionary<(Graph graph, int lodGroup, int group), NativeArray<Color32>> rawTextureData = new();


            // create dictionary for texture with groups combined
            Dictionary<(Graph graph, int lodGroup), Texture2D> allGroupsCombinedTextures = new();
            Dictionary<(Graph graph, int lodGroup), NativeArray<Color32>> allGroupsCombinedRawTextureData = new();

            // save color32 arrays for each color for later
            NativeArray<Color32> colors = new NativeArray<Color32>(this.groupSizes.Count, Allocator.TempJob);
            for (int i = 0; i < colors.Length; ++i)
            {
                byte redChannel = (byte)(CellexalConfig.Config.GraphNumberOfExpressionColors + this.groupSizes[i].groupIndex);
                colors[i] = new Color32(redChannel, 0, 0, 255);
            }

            // initialise textures
            foreach (Graph graph in ReferenceManager.instance.graphManager.Graphs)
            {
                for (int lodGroup = 0; lodGroup < graph.lodGroups; ++lodGroup)
                {
                    for (int group = 0; group < colors.Length; ++group)
                    {
                        textures[(graph, lodGroup, group)] = new Texture2D(graph.textureWidths[lodGroup], graph.textureHeights[lodGroup], TextureFormat.RGBA32, false);

                        allGroupsCombinedTextures[(graph, lodGroup)] = new Texture2D(graph.textureWidths[lodGroup], graph.textureHeights[lodGroup], TextureFormat.RGBA32, false);
                    }
                }
            }

            // fill all textures with black pixels
            foreach (Graph graph in ReferenceManager.instance.graphManager.Graphs)
            {
                // all textures for a graph are the same size, using a for loop is slow but necessary once
                NativeArray<Color32> sourceArray = textures[(graph, 0, 0)].GetRawTextureData<Color32>();
                for (int i = 0; i < sourceArray.Length; ++i)
                {
                    sourceArray[i] = new Color32(0, 0, 0, 255);
                }

                // copy the pixels in the texture to all other textures
                for (int lodGroup = 0; lodGroup < graph.lodGroups; ++lodGroup)
                {
                    for (int group = 0; group < colors.Length; ++group)
                    {

                        (Graph graph, int lodGroup, int group) key = (graph, lodGroup, group);
                        NativeArray<Color32> data = textures[key].GetRawTextureData<Color32>();

                        NativeArray<Color32>.Copy(sourceArray, data);

                        rawTextureData[key] = data;

                    }

                    (Graph graph, int lodGroup) combinedTextureKey = (graph, lodGroup);
                    NativeArray<Color32> combinedData = allGroupsCombinedTextures[combinedTextureKey].GetRawTextureData<Color32>();

                    NativeArray<Color32>.Copy(sourceArray, combinedData);
                    allGroupsCombinedRawTextureData[combinedTextureKey] = combinedData;

                }
            }

            // find groups that are included in this selection
            int graphPointsPerBatch = 1000;
            NativeArray<GraphPointData>[] pointData = new NativeArray<GraphPointData>[colors.Length]; // jagged 2d array that groups together points by their group (color)
            int[] groupSizes = new int[colors.Length];
            int[] pointsProcessed = new int[colors.Length];
            List<JobHandle> handles = new List<JobHandle>();

            for (int i = 0; i < pointData.Length; ++i)
            {
                pointData[i] = new NativeArray<GraphPointData>(Points.Count, Allocator.TempJob);
                groupSizes[i] = 0;
                pointsProcessed[i] = 0;
            }

            // batch up graphpoints in the selection and schedule a job for each batch
            foreach (Graph graph in ReferenceManager.instance.graphManager.Graphs)
            {
                for (int lodGroup = 0; lodGroup < graph.lodGroups; ++lodGroup)
                {
                    int selectionIndex = 0;

                    while (selectionIndex < Points.Count)
                    {
                        int batchLength = Math.Min(Points.Count - selectionIndex, graphPointsPerBatch);

                        for (int i = 0; i < batchLength; ++i, ++selectionIndex)
                        {
                            Graph.GraphPoint point = graph.FindGraphPoint(Points[selectionIndex].Label);
                            pointData[point.Group][groupSizes[point.Group]] = new GraphPointData() { texCoord = point.textureCoord[lodGroup], group = point.Group };
                            groupSizes[point.Group]++;
                        }

                        // schedule a job for each group that has had points added to it
                        // this should normally only schedule 1-2 jobs per iteration, since points in the Points list should be mostly grouped by their group already
                        for (int i = 0; i < colors.Length; ++i)
                        {
                            if (pointsProcessed[i] < groupSizes[i])
                            {

                                WriteToSelectionTextureJob job = new WriteToSelectionTextureJob()
                                {
                                    points = new NativeSlice<GraphPointData>(pointData[i], pointsProcessed[i], groupSizes[i] - pointsProcessed[i]),
                                    texture = rawTextureData[(graph, lodGroup, i)],
                                    colors = colors,
                                    textureWidth = graph.textureWidths[lodGroup]
                                };
                                pointsProcessed[i] = groupSizes[i];
                                handles.Add(job.Schedule());
                            }
                        }
                    }
                    // combine batches to one jobhandle, and complete it
                    NativeArray<JobHandle> handlesNativeArray = new NativeArray<JobHandle>(handles.ToArray(), Allocator.TempJob);
                    JobHandle allHandles = JobHandle.CombineDependencies(handlesNativeArray);
                    allHandles.Complete();
                    handlesNativeArray.Dispose();
                }
            }

            // remove textures that have not been updated, save the ones that were updated
            foreach (KeyValuePair<(Graph graph, int lodGroup, int group), Texture2D> kvp in textures)
            {
                Texture2D texture = kvp.Value;
                byte[] png = texture.EncodeToPNG();
                string fileName = $"{kvp.Key.graph.GraphName}_LOD{kvp.Key.lodGroup}_{kvp.Key.group}.png";
                string savedTextureFilePath = Path.Combine(currentSelectionDirectory, fileName);
                File.WriteAllBytes(savedTextureFilePath, png);
            }

            // save combined texture
            foreach (KeyValuePair<(Graph graph, int lodGroup), Texture2D> kvp in allGroupsCombinedTextures)
            {
                Texture2D texture = kvp.Value;
                byte[] png = texture.EncodeToPNG();
                string fileName = $"{kvp.Key.graph.GraphName}_LOD{kvp.Key.lodGroup}_combined.png";
                string savedTextureFilePath = Path.Combine(currentSelectionDirectory, fileName);
                File.WriteAllBytes(savedTextureFilePath, png);
            }

            foreach (NativeArray<GraphPointData> arr in pointData)
            {
                arr.Dispose();
            }

            colors.Dispose();
            stopwatch.Stop();
            CellexalLog.Log($"Saved selection to disk in {stopwatch.Elapsed}");
        }

        private struct GraphPointData
        {
            public Vector2Int texCoord;
            public int group;
        }

        /// <summary>
        /// Job to write to a selection texture, or rather the <see cref="NativeArray{Color32}" /> that contain the texture's color data.
        /// </summary>
        [BurstCompile]
        private struct WriteToSelectionTextureJob : IJob
        {
            // disable the container safety checks lets us complete these jobs much much faster
            // the nativeslices and nativearrays are already guaranteed to never overlap, so the checks are redundant
            [ReadOnly]
            [NativeDisableContainerSafetyRestriction]
            public NativeSlice<GraphPointData> points;

            [ReadOnly]
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<Color32> colors;

            [ReadOnly]
            public int textureWidth;

            [WriteOnly]
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<Color32> texture;

            public void Execute()
            {
                for (int i = 0; i < points.Length; ++i)
                {
                    int group = points[i].group;
                    Vector2Int texCoord = points[i].texCoord;
                    texture[texCoord.y * textureWidth + texCoord.x] = colors[group];
                }
            }
        }

        /// <summary>
        /// Saves this selection as a text file.
        /// </summary>
        /// <param name="directory">The directory to save the file in.</param>
        /// <returns>A path to the file that was saved.</returns>
        private string DumpSelectionToTextFile(string directory)
        {
            string filePath = Path.Combine(directory, "selection" + id + ".txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            if (File.Exists(filePath + ".time"))
            {
                File.Delete(filePath + ".time");
            }

            using StreamWriter file = new StreamWriter(filePath);

            CellexalLog.Log("Dumping selection data to " + filePath);
            CellexalLog.Log("\tSelection consists of  " + Points.Count + " points");

            // convert the currently set selection colors to hexadecimal strings
            string[] colorsRGBInHex = new string[SelectionToolCollider.instance.Colors.Length];
            for (int i = 0; i < SelectionToolCollider.instance.Colors.Length; ++i)
            {
                Color c = SelectionToolCollider.instance.Colors[i];
                int r = (int)(c.r * 255);
                int g = (int)(c.g * 255);
                int b = (int)(c.b * 255);
                // writes the color as #RRGGBB where RR, GG and BB are hexadecimal values
                colorsRGBInHex[i] = string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
            }

            string[] groups = new string[SelectionToolCollider.instance.Colors.Length];
            for (int i = 0; i < groups.Length; ++i)
            {
                groups[i] = i.ToString();
            }

            int bufferIndex;
            char[] buffer = new char[128];
            foreach (Graph.GraphPoint gp in Points)
            {
                // the code below is basically doing:
                // file.WriteLine($"{gp.Label}\t{colorsRGBInHex[gp.Group]}\t{gp.parent.GraphName}\t{groups[gp.Group]}");
                // but doesnt allocate any new strings, so it's a lot faster
                // for larger selections (> 100,000 cells) this cuts execution time from 1-2 seconds to 10-20 ms

                bufferIndex = 0;
                for (int i = 0; i < gp.Label.Length; ++i, ++bufferIndex)
                {
                    buffer[bufferIndex] = gp.Label[i];
                }
                buffer[bufferIndex] = '\t';
                bufferIndex++;
                for (int i = 0; i < colorsRGBInHex[gp.Group].Length; ++i, ++bufferIndex)
                {
                    buffer[bufferIndex] = colorsRGBInHex[gp.Group][i];
                }
                buffer[bufferIndex] = '\t';
                bufferIndex++;
                for (int i = 0; i < gp.parent.GraphName.Length; ++i, ++bufferIndex)
                {
                    buffer[bufferIndex] = gp.parent.GraphName[i];
                }
                buffer[bufferIndex] = '\t';
                bufferIndex++;
                for (int i = 0; i < groups[gp.Group].Length; ++i, ++bufferIndex)
                {
                    buffer[bufferIndex] = groups[gp.Group][i];
                }
                buffer[bufferIndex] = '\n';
                bufferIndex++;
                file.Write(buffer, 0, bufferIndex);
            }

            file.Flush();
            file.Close();
            return filePath;
        }


    }
}
