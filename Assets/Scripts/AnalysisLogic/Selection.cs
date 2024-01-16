using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{

    /// <summary>
    /// A confirmed selection of <see cref="Graph.GraphPoint"/>.
    /// Selections are created through the <see cref="SelectionManager"/> and once done, an object of this class is created.
    /// </summary>
    public class Selection : IEnumerable<Graph.GraphPoint>
    {
        private static int _nextID = 0;
        private static int GetNextID
        {
            get
            {
                int id = _nextID;
                _nextID++;
                return id;
            }
        }

        public static string parentSelectionDirectory;
        public string savedSelectionDirectory;
        public string savedSelectionFilePath;

        public readonly int id;
        public int size;
        public readonly List<int> groups;
        public readonly List<Color> colors;
        public readonly Dictionary<int, int> groupSizes;
        public bool pointsLoaded;
        public Dictionary<(Graph graph, int group), Texture2D> groupMasks = new();
        public Dictionary<Graph, Texture2D> allGroupsCombinedMask = new();

        /// <summary>
        /// Array to go from a group number to an index in <see cref="groups"/>.
        /// </summary>
        private int[] reverseGroupIndices;

        private List<Graph.GraphPoint> _points;
        /// <summary>
        /// The points that this selection includes, in the order they were selected.
        /// Do not modify the contents of this list directly without calling <see cref="SetPoints(IEnumerable{Graph.GraphPoint}, bool)"/> afterwards.
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
                    LoadSelectionFromDisk();
                    return _points;
                }
            }

            private set
            {
                SetPoints(value);
            }
        }


        /// <summary>
        /// Creates a new empty selection.
        /// </summary>
        public Selection()
        {
            this.groups = new List<int>();
            this.groupSizes = new Dictionary<int, int>();
            do
            {
                this.id = GetNextID;
            } while (!AssertDirectory(false));
            SetPoints(new List<Graph.GraphPoint>());
        }

        /// <summary>
        /// Creates a new selection from a collection of selected points.
        /// </summary>
        /// <param name="points">The collection to of points this selection should include.</param>
        public Selection(IEnumerable<Graph.GraphPoint> points)
        {
            this.groups = new List<int>();
            this.groupSizes = new Dictionary<int, int>();
            do
            {
                this.id = GetNextID;
            } while (!AssertDirectory(false));
            SetPoints(points);
        }

        public Selection(string selectionFilePath)
        {
            this.groups = new List<int>();
            this.groupSizes = new Dictionary<int, int>();
            parentSelectionDirectory = Path.Combine(CellexalUser.UserSpecificFolder, "Selections");
            bool pathIsDirectory = File.GetAttributes(selectionFilePath) == FileAttributes.Directory;
            string directoryPath = pathIsDirectory ? selectionFilePath : Path.GetDirectoryName(selectionFilePath);
            if (directoryPath.Length >= parentSelectionDirectory.Length &&
                directoryPath[..parentSelectionDirectory.Length].Equals(parentSelectionDirectory))
            {
                // get the selection directory name,
                // e.g. if selectionFilePath = "C:/path/to/selections/Selection_0/selection.txt", we want selectionDirectoryName = "Selection_0"
                string selectionDirectoryName;
                if (pathIsDirectory)
                {
                    selectionDirectoryName = new DirectoryInfo(selectionFilePath).Name;
                }
                else
                {
                    selectionDirectoryName = Directory.GetParent(selectionFilePath).Name;
                }

                if (selectionDirectoryName[..10] == "Selection_")
                {
                    if (int.TryParse(selectionDirectoryName[10..], out int selectionID))
                    {
                        if (selectionID < _nextID)
                        {
                            // selection already exists in correct directory with a used or skipped id, assume it's ok to keep using the id
                            // this could lead to multiple selection objects with the same id, but as they are pretty immutable that's probably fine
                            this.id = selectionID;
                            AssertDirectory(false);
                            LoadSelectionFromDisk(selectionFilePath);
                        }
                        else
                        {
                            // selection already exists in correct directory with a so far unnused id
                            // this probably means the selection directory is from a previous session of cellexalvr
                            // just use that id so we don't rename any directories, AssertDirectory() will skip over existing directories for future selections
                            this.id = selectionID;
                            AssertDirectory(false);
                            LoadSelectionFromDisk(selectionFilePath);
                        }
                    }
                    else
                    {
                        // directory started with "Selection_" but whatever followed was not an integer (?!)
                        // user probably renamed the directory, force a rename back to what we expect
                        do
                        {
                            this.id = GetNextID;
                        } while (!AssertDirectory(false));

                        CellexalLog.Log($"Moving {directoryPath} to {savedSelectionDirectory}.");
                        Directory.Move(directoryPath, savedSelectionDirectory);
                    }
                }
                else
                {
                    // we are in the selection directory, but this directory did not start with "Selection_" (?!)
                    // user probably renamed the directory, force a rename back to what we expect
                    do
                    {
                        this.id = GetNextID;
                    } while (!AssertDirectory(false));

                    CellexalLog.Log($"Moving {directoryPath} to {savedSelectionDirectory}.");
                    Directory.Move(directoryPath, savedSelectionDirectory);
                }
            }
            else
            {
                // we are not in the selection folder, move the selection there and make sure the new selection folder is renamed properly if needed
                do
                {
                    this.id = GetNextID;
                } while (!AssertDirectory(false));

                CellexalLog.Log($"Copying {directoryPath} to {savedSelectionDirectory}.");
                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.CopyTo(Path.Combine(savedSelectionDirectory, file.Name));
                }
            }
        }

        public IEnumerator<Graph.GraphPoint> GetEnumerator()
        {
            return new SelectionEnum(Points);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Indexer definition.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Graph.GraphPoint this[int index]
        {
            get => Points[index];
            set => Points[index] = value;
        }

        public override string ToString()
        {
            return $"Selection {id} ({size} points in {groups.Count} groups)";
        }

        /// <summary>
        /// Sets this selections points. This overwrites any <see cref="Graph.GraphPoint"/> that were saved to this selection before.
        /// </summary>
        /// <param name="points">The points that should be set to this selection.</param>
        /// <param name="copyIfList">Optional. If <paramref name="points"/> is a <see cref="List{Graph.GraphPoint}"/> and this argument is set to false, this <see cref="Selection"/> will use the reference to the <see cref="List{Graph.GraphPoint}"/>. If set to true, the contents will be copied to a new <see cref="List{Graph.GraphPoint}"/></param>
        /// <param name="saveToDisk">Optional. True if the selection should also be saved to the disk (default behaviour). False otherwise. Setting this to false can be useful if this selection is being loaded from the disk and the files are already generated.</param>
        /// <remarks>
        /// If <paramref name="points"/> is a <see cref="List{Graph.GraphPoint}"/> and <paramref name="copyIfList"/> is set to false, this function will use the reference to the <see cref="List{Graph.GraphPoint}"/> that was passed. 
        /// Otherwise, the contents will be copied to a new <see cref="List{Graph.GraphPoint}"/>.
        /// Not copying the <see cref="List{Graph.GraphPoint}"/> increases performance considerably when the <see cref="List{Graph.GraphPoint}"/> that is passed with <paramref name="points"/> is not needed in the calling function after this.
        /// </remarks>
        public void SetPoints(IEnumerable<Graph.GraphPoint> points, bool copyIfList = false, bool saveToDisk = true)
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

            int[] groupSizesArray = new int[CellexalConfig.Config.GraphNumberOfExpressionColors];
            for (int i = 0; i < _points.Count; ++i)
            {
                groupSizesArray[_points[i].Group]++;
            }

            for (int i = 0; i < groupSizesArray.Length; ++i)
            {
                if (groupSizesArray[i] > 0)
                {
                    groups.Add(i);
                    groupSizes[i] = groupSizesArray[i];
                }
            }

            reverseGroupIndices = new int[groups[^1] + 1]; // the last element in groups is also the highest group number
            Array.Fill(reverseGroupIndices, -1);

            for (int i = 0; i < groups.Count; ++i)
            {
                reverseGroupIndices[groups[i]] = i;
            }

            size = _points.Count;

            if (saveToDisk)
            {
                SaveGroupMasksToDisk();
            }
        }

        [BurstCompatible]
        private struct CalculatePointsMetaDataJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<GraphPointData> points;

            public NativeArray<int> groupSizes;

            public void Execute(int index)
            {
                groupSizes[points[index].group]++;
            }
        }

        /// <summary>
        /// Asserts that a suitable directory exists, this function will create a directory if needed.
        /// Directory name is based off this selection's <see cref="id"/>.
        /// </summary>
        /// <param name="deleteContents">Deletes all content in the directory this selection should use</param>
        /// <returns>False if a directory exists, but <paramref name="deleteContents"/> is false. True otherwise. This makes the function usable for loops when looking for a suitable directory.</returns>
        public bool AssertDirectory(bool deleteContents)
        {
            parentSelectionDirectory = Path.Combine(CellexalUser.UserSpecificFolder, "Selections");
            savedSelectionDirectory = Path.Combine(parentSelectionDirectory, "Selection_" + id);
            savedSelectionFilePath = Path.Combine(savedSelectionDirectory, "selection.txt");
            if (!Directory.Exists(parentSelectionDirectory))
            {
                Directory.CreateDirectory(parentSelectionDirectory);
                CellexalLog.Log($"Created directory {parentSelectionDirectory}");
            }
            if (!Directory.Exists(savedSelectionDirectory))
            {
                Directory.CreateDirectory(savedSelectionDirectory);
                CellexalLog.Log($"Created directory {savedSelectionDirectory}");
            }
            else if (deleteContents)
            {
                CellexalLog.Log($"Deleting all files in {savedSelectionDirectory}");
                foreach (string filePath in Directory.GetFiles(savedSelectionDirectory))
                {
                    File.Delete(filePath);
                }
            }
            else
            {
                // directory already existed and we are told not to modify it!
                return false;
            }
            return true;
        }

        private void LoadSelectionFromDisk()
        {
            LoadSelectionFromDisk(savedSelectionFilePath);
        }

        private void LoadSelectionFromDisk(string filePath)
        {
            if (_points is not null)
            {
                _points.Clear();
            }
            else
            {
                _points = new List<Graph.GraphPoint>();
            }

            List<Vector2Int> indices = new List<Vector2Int>();


            if (!File.Exists(filePath))
            {
                if (!Path.IsPathFullyQualified(filePath))
                {
                    string attemptedQualifiedFilePath = Path.Combine(savedSelectionDirectory, filePath);
                    if (File.Exists(attemptedQualifiedFilePath))
                    {
                        goto selectionFileExists;
                    }

                    attemptedQualifiedFilePath = Path.Combine(parentSelectionDirectory, filePath);
                    if (File.Exists(attemptedQualifiedFilePath))
                    {
                        goto selectionFileExists;
                    }
                }
                CellexalLog.Log("Could not find file when reading selection file:" + filePath);
                return;
            }

            selectionFileExists: // label used in if statement above when a file is found

            using FileStream fileStream = new FileStream(filePath, FileMode.Open);
            using StreamReader streamReader = new StreamReader(fileStream);
            GraphManager graphManager = ReferenceManager.instance.graphManager;
            int numPointsAdded = 0;
            List<Graph.GraphPoint> readPoints = new List<Graph.GraphPoint>();
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                string[] words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int group;
                UnityEngine.Color groupColor;

                try
                {
                    // group = int.Parse(words[3]);
                    string colorString = words[1];
                    ColorUtility.TryParseHtmlString(colorString, out groupColor);
                    if (!CellexalConfig.Config.SelectionToolColors.Any(x => InputReader.CompareColor(x, groupColor)))
                    {
                        ReferenceManager.instance.settingsMenu.AddSelectionColor(groupColor);
                        ReferenceManager.instance.settingsMenu.unsavedChanges = false;
                    }
                    group = ReferenceManager.instance.selectionToolCollider.GetColorIndex(groupColor);
                }
                catch (FormatException)
                {
                    CellexalLog.Log(string.Format("Bad color on line {0} in file {1}.", numPointsAdded + 1, filePath));
                    streamReader.Close();
                    fileStream.Close();
                    CellexalEvents.CommandFinished.Invoke(false);
                    return;
                }

                if (PointCloudGenerator.instance.pointClouds.Count > 0)
                {
                    Vector2Int tuple = new Vector2Int(int.Parse(words[0]), int.Parse(words[3]));
                    indices.Add(tuple);
                }
                else
                {
                    Graph.GraphPoint graphPoint = graphManager.FindGraphPoint(words[2], words[0]);
                    graphPoint.Group = group;
                    readPoints.Add(graphPoint);
                }
                numPointsAdded++;
            }

            SetPoints(readPoints, saveToDisk: false);

            string currentDir = Path.GetDirectoryName(filePath);
            if (currentDir != savedSelectionDirectory)
            {
                File.Copy(filePath, savedSelectionFilePath);
                if (GroupMaskFilesExist(currentDir))
                {
                    foreach (string mask in Directory.GetFiles(currentDir, "*.png"))
                    {
                        File.Copy(mask, savedSelectionDirectory);
                    }
                    LoadGroupMasksFromDisk();
                }
                else
                {
                    SaveGroupMasksToDisk();
                }
            }
            else
            {
                if (GroupMaskFilesExist(currentDir))
                {
                    LoadGroupMasksFromDisk();
                }
                else
                {
                    SaveGroupMasksToDisk();
                }
            }

            TextureHandler.instance?.AddPointsToSelection(indices);
            CellexalLog.Log(string.Format("Added {0} points to selection", numPointsAdded));
            CellexalEvents.CommandFinished.Invoke(true);
            CellexalEvents.SelectedFromFile.Invoke();
            streamReader.Close();
            fileStream.Close();
        }

        public void LoadGroupMasksFromDisk()
        {
            groupMasks = new Dictionary<(Graph graph, int group), Texture2D>();
            allGroupsCombinedMask = new Dictionary<Graph, Texture2D>();
            string[] files = Directory.GetFiles(savedSelectionDirectory, "*.png");
            JobHandle[] handles = new JobHandle[files.Length];
            for (int i = 0; i < files.Length; ++i)
            {
                string path = files[i];
                string graphAndGroup = Path.GetFileNameWithoutExtension(path);
                int lastUnderscoreIndex = graphAndGroup.LastIndexOf('_');
                string graphName = graphAndGroup[..lastUnderscoreIndex];
                string groupAsString = graphAndGroup[(lastUnderscoreIndex + 1)..];

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                {
                    CellexalLog.Log($"Failed to load selection group mask at {path}");
                }
                Graph graph = ReferenceManager.instance.graphManager.FindGraph(graphName);

                // if it's the combined texture, the filename ends with "combined" instead of a group number
                if (groupAsString.Equals("combined"))
                {
                    allGroupsCombinedMask[graph] = texture;
                    NativeArray<Color32> data = texture.GetRawTextureData<Color32>();
                    handles[i] = new Extensions.Jobs.ConvertARGBToRGBAJob() { data = data }.Schedule(data.Length, 10000);
                    continue;
                }

                bool parsed = int.TryParse(groupAsString, out int group);
                if (!parsed)
                {
                    CellexalLog.Log($"Bad group number in selection group mask in file: {path}");
                    return;
                }
                else
                {
                    groupMasks[(graph, group)] = texture;
                    NativeArray<Color32> data = texture.GetRawTextureData<Color32>();
                    handles[i] = new Extensions.Jobs.ConvertARGBToRGBAJob() { data = data }.Schedule(data.Length, 10000);
                }
            }

            foreach (JobHandle handle in handles)
            {
                handle.Complete();
            }

            foreach (Texture2D tex in allGroupsCombinedMask.Values)
            {
                tex.Apply();
            }

            foreach (Texture2D tex in groupMasks.Values)
            {
                tex.Apply();
            }
        }

        public void UnloadSelection()
        {
            pointsLoaded = false;
            _points.Clear();
            _points = null;
        }

        public void SaveSelectionToDisk()
        {
            SaveSelectionTextFileToDisk();
            SaveGroupMasksToDisk();
        }

        /// <summary>
        /// Saves the group masks to the disk. This is automatically done by the constructor or when points are assigned with.
        /// </summary>
        /// <param name="graphToSave">Optional. If left out, all graphs that are currently loaded will be saved, otherwise, only the graph passed in this argument is saved.</param>
        public void SaveGroupMasksToDisk(Graph graphToSave = null)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            List<Graph> graphs = graphToSave is null ? ReferenceManager.instance.graphManager.Graphs : new List<Graph>() { graphToSave };
            // create group-specific texture dictionary
            // maps a combination of a graph, lod group and selection group to the corresponding texture and its raw texture data array
            Dictionary<(Graph graph, int group), NativeArray<Color32>> rawTextureData = new();


            // create dictionary for texture with groups combined
            Dictionary<Graph, NativeArray<Color32>> allGroupsCombinedRawTextureData = new();

            // save color32 arrays for each color for later
            NativeArray<Color32> colors = new NativeArray<Color32>(groups.Count, Allocator.TempJob);
            for (int i = 0; i < colors.Length; ++i)
            {
                byte redChannel = (byte)(CellexalConfig.Config.GraphNumberOfExpressionColors + groups[i]);
                colors[i] = new Color32(redChannel, 0, 0, 255);
            }

            // initialise textures
            foreach (Graph graph in graphs)
            {
                for (int i = 0; i < colors.Length; ++i)
                {
                    groupMasks[(graph, groups[i])] = new Texture2D(graph.textureWidth, graph.textureHeight, TextureFormat.RGBA32, false);
                    groupMasks[(graph, groups[i])].Apply();
                }
                allGroupsCombinedMask[graph] = new Texture2D(graph.textureWidth, graph.textureHeight, TextureFormat.RGBA32, false);
                allGroupsCombinedMask[graph].Apply();
            }

            // fill all textures with black pixels
            foreach (Graph graph in graphs)
            {
                // all textures for a graph are the same size, using a for loop is slow but necessary once
                NativeArray<Color32> sourceArray = groupMasks[(graph, groups[0])].GetRawTextureData<Color32>();
                for (int i = 0; i < sourceArray.Length; ++i)
                {
                    sourceArray[i] = new Color32(0, 0, 0, 255);
                }
                rawTextureData[(graph, groups[0])] = sourceArray;

                // copy the pixels in the texture to all other textures
                for (int i = 1; i < colors.Length; ++i)
                {

                    (Graph graph, int group) key = (graph, groups[i]);
                    NativeArray<Color32> data = groupMasks[key].GetRawTextureData<Color32>();

                    NativeArray<Color32>.Copy(sourceArray, data);

                    rawTextureData[key] = data;
                }

                NativeArray<Color32> combinedData = allGroupsCombinedMask[graph].GetRawTextureData<Color32>();

                NativeArray<Color32>.Copy(sourceArray, combinedData);
                allGroupsCombinedRawTextureData[graph] = combinedData;
            }

            // find groups that are included in this selection
            int graphPointsPerBatch = 1000;
            NativeArray<GraphPointData>[] pointData = new NativeArray<GraphPointData>[colors.Length]; // jagged 2d array that groups together points by their group (color)
            int[] pointsToProcess = new int[colors.Length];
            int[] pointsProcessed = new int[colors.Length];
            List<JobHandle> handles = new List<JobHandle>(this.size / graphPointsPerBatch);


            // batch up graphpoints in the selection and schedule a job for each batch
            foreach (Graph graph in graphs)
            {
                // reset variables
                int selectionIndex = 0;
                for (int i = 0; i < pointData.Length; ++i)
                {
                    pointData[i] = new NativeArray<GraphPointData>(groupSizes[groups[i]], Allocator.TempJob);
                    pointsToProcess[i] = 0;
                    pointsProcessed[i] = 0;
                }

                while (selectionIndex < Points.Count)
                {
                    int batchLength = Math.Min(Points.Count - selectionIndex, graphPointsPerBatch);

                    // create a batch of graphpoints
                    for (int i = 0; i < batchLength; ++i, ++selectionIndex)
                    {
                        Graph.GraphPoint point = graph.FindGraphPoint(Points[selectionIndex].Label);
                        if (point is null)
                        {
                            continue;
                        }
                        int groupIndex = reverseGroupIndices[Points[selectionIndex].Group];
                        int pointsIndex = pointsProcessed[groupIndex] + pointsToProcess[groupIndex];
                        pointData[groupIndex][pointsIndex] = new GraphPointData() { texCoord = point.textureCoord, group = groupIndex };
                        pointsToProcess[groupIndex]++;
                    }

                    // schedule a job for each group that has had points added to it
                    // this should normally only schedule 1-2 jobs per iteration, since points in the Points list should be mostly grouped by their group already
                    // more jobs may be scheduled in one iteration if many small groups fit in one batch, determined by graphPointsPerBatch above. but (at least) one job per group is needed due to the design of WriteToSelectionTextureJob
                    for (int i = 0; i < pointsToProcess.Length; ++i)
                    {
                        if (pointsToProcess[i] > 0)
                        {

                            WriteToSelectionTextureJob job = new WriteToSelectionTextureJob()
                            {
                                points = new NativeSlice<GraphPointData>(pointData[i], pointsProcessed[i], pointsToProcess[i]),
                                texture = rawTextureData[(graph, groups[i])],
                                combinedTexture = allGroupsCombinedRawTextureData[graph],
                                colors = colors,
                                textureWidth = graph.textureWidth
                            };
                            pointsProcessed[i] += pointsToProcess[i];
                            pointsToProcess[i] = 0;
                            handles.Add(job.Schedule());
                        }
                    }
                }
                // combine batches to one jobhandle, and complete it
                NativeArray<JobHandle> handlesNativeArray = new NativeArray<JobHandle>(handles.ToArray(), Allocator.TempJob);
                JobHandle allHandles = JobHandle.CombineDependencies(handlesNativeArray);
                allHandles.Complete();
                handlesNativeArray.Dispose();

                foreach (NativeArray<GraphPointData> arr in pointData)
                {
                    arr.Dispose();
                }
            }

            // save textures
            foreach (KeyValuePair<(Graph graph, int group), Texture2D> kvp in groupMasks)
            {
                Texture2D texture = kvp.Value;
                byte[] png = texture.EncodeToPNG();
                string fileName = $"{kvp.Key.graph.GraphName}_{kvp.Key.group}.png";
                string savedTextureFilePath = Path.Combine(savedSelectionDirectory, fileName);
                File.WriteAllBytes(savedTextureFilePath, png);
            }

            // save combined texture
            foreach (KeyValuePair<Graph, Texture2D> kvp in allGroupsCombinedMask)
            {
                Texture2D texture = kvp.Value;
                byte[] png = texture.EncodeToPNG();
                string fileName = $"{kvp.Key.GraphName}_combined.png";
                string savedTextureFilePath = Path.Combine(savedSelectionDirectory, fileName);
                File.WriteAllBytes(savedTextureFilePath, png);
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

            [WriteOnly]
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<Color32> combinedTexture;

            public void Execute()
            {
                for (int i = 0; i < points.Length; ++i)
                {
                    int group = points[i].group;
                    Vector2Int texCoord = points[i].texCoord;
                    int textureDataIndex = texCoord.y * textureWidth + texCoord.x;
                    texture[textureDataIndex] = colors[group];
                    combinedTexture[textureDataIndex] = colors[group];
                }
            }
        }

        /// <summary>
        /// Checks if group mask files exists in the target directory. This function only checks filenames, it does not guarantee that the contents of the group masks are correct.
        /// </summary>
        /// <param name="directory">The directory to look for group masks in.</param>
        /// <returns>True if all file name matches this selection and currently loaded dataset's graphs and groups.</returns>
        private bool GroupMaskFilesExist(string directory)
        {
            List<Graph> graphs = ReferenceManager.instance.graphManager.Graphs;
            string[] pathsToLookFor = new string[graphs.Count * (groups.Count + 1)]; // add 1 per graph for the combined textures
            string[] filesOnDisk = Directory.GetFiles(directory, "*.png");

            if (pathsToLookFor.Length != filesOnDisk.Length)
            {
                return false;
            }

            for (int i = 0, pathIndex = 0; i < graphs.Count; ++i)
            {
                for (int j = 0; j < groups.Count; ++j, ++pathIndex)
                {
                    pathsToLookFor[pathIndex] = $"{graphs[i].GraphName}_{groups[j]}.png";
                }
            }

            Array.Sort(pathsToLookFor);
            Array.Sort(filesOnDisk);

            for (int i = 0; i < pathsToLookFor.Length; ++i)
            {
                if (!pathsToLookFor.Equals(filesOnDisk))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Saves this selection as a text file.
        /// </summary>
        /// <param name="directory">The directory to save the file in.</param>
        private void SaveSelectionTextFileToDisk()
        {
            if (File.Exists(savedSelectionFilePath))
            {
                File.Delete(savedSelectionFilePath);
            }
            if (File.Exists(savedSelectionFilePath + ".time"))
            {
                File.Delete(savedSelectionFilePath + ".time");
            }

            using StreamWriter file = new StreamWriter(savedSelectionFilePath);

            CellexalLog.Log("Dumping selection data to " + savedSelectionFilePath);
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
        }
    }

    /// <summary>
    /// Enumerator class. Required for <see cref="Selection"/> to implement <see cref="IEnumerable"/>.
    /// </summary>
    public class SelectionEnum : IEnumerator<Graph.GraphPoint>
    {
        public List<Graph.GraphPoint> points;

        private int position;

        public SelectionEnum(List<Graph.GraphPoint> points)
        {
            this.points = points;
            position = -1;
        }

        public Graph.GraphPoint Current => points[position];

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            position++;
            return position < points.Count;
        }

        public void Reset()
        {
            position = -1;
        }
    }
}
