using AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Spatial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// Class that handles the reading of MDS input files. Normally graph coordinates.
    /// </summary>
    public class MDSReader : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private readonly char[] separators = new char[] { ' ', '\t', ',' };
        private int nrOfLODGroups;


        /// <summary>
        /// Coroutine to create graphs.
        /// </summary>
        /// <param name="path"> The path to the folder where the files are. </param>
        /// <param name="mdsFiles"> The filenames. </param>
        /// <param name="type"></param>
        /// <param name="server"></param>
        public IEnumerator ReadMDSFiles(string path, string[] mdsFiles,
            GraphGenerator.GraphType type = GraphGenerator.GraphType.MDS, bool server = true)
        {
            //nrOfLODGroups = CellexalConfig.Config.GraphPointQuality == "Standard" ? 2 : 1;
            nrOfLODGroups = 1;
            //  Read each .mds file
            //  The file format should be
            //  cell_id  axis_name1   axis_name2   axis_name3
            //  CELLNAME_1 X_COORD Y_COORD Z_COORD
            //  CELLNAME_2 X_COORD Y_COORD Z_COORD
            //  ...

            const float maximumDeltaTime = 0.05f; // 20 fps
            int maximumItemsPerFrame = CellexalConfig.Config.GraphLoadingCellsPerFrameStartCount;
            int totalNbrOfCells = 0;
            foreach (string file in mdsFiles)
            {
                while (referenceManager.graphGenerator.isCreating)
                {
                    yield return null;
                }

                Graph combGraph = referenceManager.graphGenerator.CreateGraph(type);
                // file will be the full file name e.g C:\...\graph1.mds
                // good programming habits have left us with a nice mix of forward and backward slashes
                string[] regexResult = Regex.Split(file, @"[\\/]");
                string graphFileName = regexResult[regexResult.Length - 1];
                referenceManager.graphManager.Graphs.Add(combGraph);
                switch (type)
                {
                    case GraphGenerator.GraphType.MDS:
                        combGraph.GraphName = graphFileName.Substring(0, graphFileName.Length - 4);
                        combGraph.FolderName = regexResult[regexResult.Length - 2];
                        referenceManager.graphManager.originalGraphs.Add(combGraph);
                        break;
                    case GraphGenerator.GraphType.FACS:
                        {
                            string graphName = "";
                            foreach (string s in referenceManager.newGraphFromMarkers.markers)
                            {
                                graphName += s + " - ";
                            }

                            combGraph.GraphNumber = referenceManager.inputReader.facsGraphCounter;
                            combGraph.GraphName = graphName;
                            combGraph.tag = "FacsGraph";
                            referenceManager.graphManager.facsGraphs.Add(combGraph);
                            break;
                        }
                    case GraphGenerator.GraphType.ATTRIBUTE:
                    case GraphGenerator.GraphType.BETWEEN:
                    case GraphGenerator.GraphType.SPATIAL:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                string[] axes = new string[3];
                string[] velo = new string[3];
                List<string> names = new List<string>();
                List<float> xcoords = new List<float>();
                List<float> ycoords = new List<float>();
                List<float> zcoords = new List<float>();
                using (StreamReader mdsStreamReader = new StreamReader(file))
                {
                    // first line is (if correct format) a header and the first word is cell_id (the name of the first column).
                    // If wrong and does not contain header read first line as a cell.
                    string header = mdsStreamReader.ReadLine();
                    if (header != null && header.Split(separators)[0].Equals("CellID"))
                    {
                        string[] columns = header.Split(separators).Skip(1).ToArray();
                        Array.Copy(columns, 0, axes, 0, 3);
                        if (columns.Length == 6)
                        {
                            Array.Copy(columns, 3, velo, 0, 3);
                            referenceManager.graphManager.velocityFiles.Add(file);
                            combGraph.hasVelocityInfo = true;
                        }
                    }
                    else if (header != null)
                    {
                        string[] words = header.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        if (words.Length != 4 && words.Length != 7)
                        {
                            continue;
                        }

                        string cellName = words[0];
                        referenceManager.cellManager.cellNames.Add(cellName);
                        float x = float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        float y = float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        float z = float.Parse(words[3], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        names.Add(cellName);
                        xcoords.Add(x);
                        ycoords.Add(y);
                        zcoords.Add(z);
                        axes[0] = "x";
                        axes[1] = "y";
                        axes[2] = "z";
                    }

                    combGraph.axisNames = axes;
                    var itemsThisFrame = 0;
                    while (!mdsStreamReader.EndOfStream)
                    {
                        for (int j = 0; j < maximumItemsPerFrame && !mdsStreamReader.EndOfStream; ++j)
                        {
                            string[] words = mdsStreamReader.ReadLine()
                                .Split(separators, StringSplitOptions.RemoveEmptyEntries);
                            string cellname = words[0];
                            referenceManager.cellManager.cellNames.Add(cellname);
                            float x = float.Parse(words[1],
                                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            float y = float.Parse(words[2],
                                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            float z = float.Parse(words[3],
                                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            names.Add(cellname);
                            xcoords.Add(x);
                            ycoords.Add(y);
                            zcoords.Add(z);
                            itemsThisFrame++;
                        }

                        totalNbrOfCells += itemsThisFrame;
                        // wait for end of frame
                        yield return null;

                        float lastFrame = Time.deltaTime;
                        if (lastFrame < maximumDeltaTime)
                        {
                            // we had some time over last frame
                            maximumItemsPerFrame += CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement;
                        }
                        else if (lastFrame > maximumDeltaTime && maximumItemsPerFrame >
                            CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement * 2)
                        {
                            // we took too much time last frame
                            maximumItemsPerFrame -= CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement;
                        }
                    }

                    // tell the graph that the info text is ready to be set
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    stopwatch.Stop();
                    CellexalLog.Log("Created " + combGraph.GetComponents<BoxCollider>().Length + " colliders in " +
                                    stopwatch.Elapsed.ToString() + " for graph " + graphFileName);

                    mdsStreamReader.Close();
                }

                CreateFromCoordinates(names, xcoords, ycoords, zcoords);

                // If high quality mesh is used. Use LOD groups to swap to low q when further away.
                // Improves performance a lot when analysing larger graphs.
                int n = CellexalConfig.Config.GraphPointQuality == "Standard" ? 2 : 1;
                StartCoroutine(referenceManager.graphGenerator.SliceClusteringLOD(nrOfLODGroups));

                while (referenceManager.graphGenerator.isCreating)
                {
                    yield return null;
                }

                if (nrOfLODGroups > 1)
                {
                    combGraph.gameObject.AddComponent<LODGroup>();
                    referenceManager.graphGenerator.UpdateLODGroups(combGraph, nrOfLODGroups);
                }

                // Add axes in bottom corner of graph and scale points differently
                combGraph.SetInfoText();
                referenceManager.graphGenerator.AddAxes(combGraph, axes);
            }
        }

        public IEnumerator ReadBigFolder(string path)
        {
            string workingDirectory = Directory.GetCurrentDirectory();
            string fullPath = workingDirectory + "\\Data\\" + path;
            string[] files = Directory.GetFiles(fullPath, "*.mds");
            PointCloudGenerator.instance.mdsFileCount = files.Length;
            // string mdsFile = files[0];
            foreach (string mdsFile in files)
            {
                bool spatial = mdsFile.Contains("spatial");
                PointCloud pc = PointCloudGenerator.instance.CreateNewPointCloud(spatial);
                string[] regexResult = Regex.Split(mdsFile, @"[\\/]");
                string graphFileName = regexResult[regexResult.Length - 1];
                //pc.gameObject.name = graphFileName.Substring(0, graphFileName.Length - 4);
                pc.GraphName = graphFileName.Substring(0, graphFileName.Length - 4);
                pc.originalName = pc.GraphName;
                float x;
                float y;
                float z;
                using (StreamReader streamReader = new StreamReader(mdsFile))
                {
                    streamReader.ReadLine();

                    int i = 0;
                    while (!streamReader.EndOfStream)
                    {
                        if (i % 10000 == 0) yield return null;
                        string[] words = streamReader.ReadLine().Split(separators);
                        string cellName;
                        // reading 2d graph or img pixel coordinates for spatial slice.
                        if (words.Length == 2)
                        {
                            cellName = i.ToString();
                            x = (float.Parse(words[0]));
                            y = (float.Parse(words[1]));
                            z = 0f;
                        }
                        else
                        {
                            //cellName = i.ToString();
                            cellName = words[0];

                            x = (float.Parse(words[1]));
                            y = (float.Parse(words[2]));
                            z = float.Parse(words[3]);
                        }
                        PointCloudGenerator.instance.AddGraphPoint(cellName, x, y, z);
                        int textureX = i % PointCloudGenerator.textureWidth;
                        int textureY = (i / PointCloudGenerator.textureWidth);
                        TextureHandler.instance.textureCoordDict[cellName] = new Vector2Int(textureX, textureY);
                        PointCloudGenerator.instance.indToLabelDict[i] = cellName;
                        i++;
                    }
                }
                PointCloudGenerator.instance.SpawnPoints(pc);
                GC.Collect();
            }

            if (files.Length > 1)
            {
                PointCloud pc1 = PointCloudGenerator.instance.pointClouds[0];
                PointCloud pc2 = PointCloudGenerator.instance.pointClouds[1];
                pc1.SetTargetTexture(pc2.positionTextureMap.GetPixels());
                pc2.SetTargetTexture(pc1.positionTextureMap.GetPixels());
                //pc1.morphTexture = pc2.positionTextureMap;
                //pc2.morphTexture = pc1.positionTextureMap;
                //pc1.GetComponent<VisualEffect>().SetTexture("TargetPosMapTex", pc2.positionTextureMap);
                //pc2.GetComponent<VisualEffect>().SetTexture("TargetPosMapTex", pc1.positionTextureMap);
                //pc1.otherName = pc2.GraphName;
                //pc2.otherName = pc1.GraphName;
                //pc1.originalName = pc1.GraphName;
                //pc2.otherName = pc1.GraphName;
                //pc2.originalName = pc2.GraphName;
            }

            StartCoroutine(PointCloudGenerator.instance.ReadMetaData(fullPath));
            while (PointCloudGenerator.instance.readingFile)
                yield return null;
            StartCoroutine(PointCloudGenerator.instance.CreateColorTextureMap());

            while (PointCloudGenerator.instance.creatingGraph)
                yield return null;

            PointCloud parentPC = PointCloudGenerator.instance.pointClouds[0];

            files = Directory.GetFiles(fullPath, "*coords.csv");
            string[] imageFiles = Directory.GetFiles(fullPath, "*.png");
            for (int i = 0; i < files.Length; i++)
            {
                int lineCount = 0;
                string imageFile = imageFiles[i];
                byte[] imageData = File.ReadAllBytes(imageFile);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                string[] names = files[i].Split(Path.DirectorySeparatorChar);
                string n = names[names.Length - 1].Split('.')[0];
                HistoImage hi = PointCloudGenerator.instance.CreateNewHistoImage(parentPC);
                hi.sliceNr = int.Parse(Regex.Match(n, @"\d+").Value);
                hi.gameObject.name = n;
                hi.texture = texture;
                hi.transform.position = new Vector3(0f, 1f, (float)i / 5f);
                using (StreamReader sr = new StreamReader(files[i]))
                {
                    string header = sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string[] words = line.Split(',');
                        string cellName = words[0];
                        float.TryParse(words[1], out float x);
                        float.TryParse(words[2], out float y);
                        int xCoord = (int)x;
                        int yCoord = (int)y;
                        PointCloudGenerator.instance.AddGraphPoint(cellName, x, y);
                        if (lineCount % 100 == 0) yield return null;
                        int textureX = lineCount % PointCloudGenerator.textureWidth;
                        int textureY = (lineCount / PointCloudGenerator.textureWidth);
                        hi.textureCoords[cellName] = new Vector2Int(textureX, textureY);
                        //TextureHandler.instance.textureCoordDict[cellName] = new Vector2Int(textureX, textureY);
                        lineCount++;
                    }
                }
                //hi.Initialize();
                MeshRenderer mr = hi.image.GetComponent<MeshRenderer>();
                mr.material.mainTexture = texture;
                mr.material.SetTexture("_EmissionMap", texture);
                PointCloudGenerator.instance.SpawnPoints(hi, parentPC);
                HistoImageHandler.instance.images.Add(hi);
                string c = hi.sliceNr.ToString();
                var carr = c.ToCharArray();
                string id = n.Split(carr)[0];
                if (!HistoImageHandler.instance.imageDict.ContainsKey(id))
                {
                    HistoImageHandler.instance.imageDict.Add(id, new List<HistoImage>());
                }
                HistoImageHandler.instance.imageDict[id].Add(hi);

            }

            CellexalEvents.GraphsLoaded.Invoke();
        }

        private void CreateFromCoordinates(List<string> names, List<float> x, List<float> y, List<float> z)
        {
            if (!referenceManager.loaderController.loaderMovedDown)
            {
                referenceManager.loaderController.loaderMovedDown = true;
                referenceManager.loaderController.MoveLoader(new Vector3(0f, -2f, 0f), 2f);
            }
            int gpCount = x.Count;
            for (int i = 0; i < gpCount; i++)
            {
                Cell cell = referenceManager.cellManager.AddCell(names[i]);
                referenceManager.graphGenerator.AddGraphPoint(cell, x[i], y[i], z[i]);
            }
        }

        public void CreateFromCoordinates(List<float> x, List<float> y)
        {
            int gpCount = x.Count;
            for (int i = 0; i < gpCount; i++)
            {
                string cellName = i.ToString();
                Cell cell = referenceManager.cellManager.AddCell(cellName);
                referenceManager.graphGenerator.AddGraphPoint(cell, x[i], y[i], 0);
            }
        }

        public void CreateFromCoordinates(List<float> x, List<float> y, List<float> z)
        {
            if (!referenceManager.loaderController.loaderMovedDown)
            {
                referenceManager.loaderController.loaderMovedDown = true;
                referenceManager.loaderController.MoveLoader(new Vector3(0f, -2f, 0f), 2f);
            }
            int gpCount = x.Count;
            for (int i = 0; i < gpCount; i++)
            {
                string cellName = i.ToString();
                Cell cell = referenceManager.cellManager.AddCell(cellName);
                referenceManager.graphGenerator.AddGraphPoint(cell, x[i], y[i], z[i]);
            }
        }

        /// <summary>
        /// For spatial data we want to have each slice as a separate graph to be able to interact with them individually.
        /// First the list of points is ordered by the z coordinate then for each z coordinate a graph is created. 
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadSpatialMDSFiles(string file)
        {
            if (!referenceManager.loaderController.loaderMovedDown)
            {
                referenceManager.loaderController.loaderMovedDown = true;
                referenceManager.loaderController.MoveLoader(new Vector3(0f, -2f, 0f), 2f);
            }

            // int nrOfLODGroups = CellexalConfig.Config.GraphPointQuality == "Standard" ? 2 : 1;
            List<Tuple<string, Vector3>> gps = new List<Tuple<string, Vector3>>();
            const float maximumDeltaTime = 0.05f; // 20 fps
            int maximumItemsPerFrame = CellexalConfig.Config.GraphLoadingCellsPerFrameStartCount;

            //string fullPath = Directory.GetCurrentDirectory() + "\\Data\\" + data + "\\tsne.mds";
            float prevCoord = float.NaN;
            while (referenceManager.graphGenerator.isCreating)
            {
                yield return null;
            }

            GameObject parent = GameObject.Instantiate(referenceManager.inputReader.spatialGraphPrefab);
            SpatialGraph sg = parent.GetComponent<SpatialGraph>();
            sg.gameObject.layer = LayerMask.NameToLayer("GraphLayer");
            referenceManager.graphManager.spatialGraphs.Add(sg);
            sg.referenceManager = referenceManager;

            int sliceNr = 0;
            using (StreamReader mdsStreamReader = new StreamReader(file))
            {
                int i = 0;
                string header = mdsStreamReader.ReadLine();
                while (!mdsStreamReader.EndOfStream)
                {
                    var itemsThisFrame = 0;
                    for (int j = 0; j < maximumItemsPerFrame && !mdsStreamReader.EndOfStream; ++j)
                    {
                        string[] words = mdsStreamReader.ReadLine()
                            ?.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        if (words != null && (words.Length != 4 && words.Length != 7))
                        {
                            continue;
                        }

                        string cellName = words[0];
                        float x = float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        float y = float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        float z = float.Parse(words[3], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        Cell cell = referenceManager.cellManager.AddCell(cellName);
                        gps.Add(new Tuple<string, Vector3>(cellName, new Vector3(x, y, z)));
                        itemsThisFrame++;
                    }

                    i += itemsThisFrame;
                    // wait for end of frame
                    yield return null;

                    float lastFrame = Time.deltaTime;
                    if (lastFrame < maximumDeltaTime)
                    {
                        // we had some time over last frame
                        maximumItemsPerFrame += CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement;
                    }
                    else if (lastFrame > maximumDeltaTime && maximumItemsPerFrame >
                        CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement * 2)
                    {
                        // we took too much time last frame
                        maximumItemsPerFrame -= CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement;
                    }
                }
            }

            gps.Sort((x, y) => x.Item2.z.CompareTo(y.Item2.z));
            Vector3 maxCoords = new Vector3();
            Vector3 minCoords = new Vector3();
            maxCoords.x = gps.Max(v => (v.Item2.x));
            maxCoords.y = gps.Max(v => (v.Item2.y));
            maxCoords.z = gps.Max(v => (v.Item2.z));
            minCoords.x = gps.Min(v => (v.Item2.x));
            minCoords.y = gps.Min(v => (v.Item2.y));
            minCoords.z = gps.Min(v => (v.Item2.z));
            Graph combGraph = referenceManager.graphGenerator.CreateGraph(GraphGenerator.GraphType.SPATIAL);
            yield return null;
            referenceManager.graphManager.Graphs.Add(combGraph);
            referenceManager.graphManager.originalGraphs.Add(combGraph);
            combGraph.gameObject.name = "Slice" + sliceNr;
            combGraph.GraphName = "Slice" + sliceNr;
            Transform transform1 = combGraph.transform;
            transform1.parent = parent.transform;
            transform1.localPosition = new Vector3(0, 0, 0);
            combGraph.lodGroups = nrOfLODGroups;
            combGraph.textures = new Texture2D[nrOfLODGroups];
            GraphSlice gs = combGraph.gameObject.AddComponent<Spatial.GraphSlice>();
            yield return null;

            // const float sliceDist = 0.005f;
            Tuple<string, Vector3> gpTuple = gps[0];
            Cell c = referenceManager.cellManager.GetCell(gpTuple.Item1);

            Graph.GraphPoint gp = referenceManager.graphGenerator.AddGraphPoint(c, gpTuple.Item2.x,
                gpTuple.Item2.y,
                gpTuple.Item2.z);

            float currentCoord = gpTuple.Item2.z;
            for (int n = 1; n < gps.Count; n++)
            {
                gpTuple = gps[n];
                currentCoord = gpTuple.Item2.z;
                if (n == 1)
                {
                    prevCoord = currentCoord;
                }
                // when we reach new slice (new z coordinate) build the graph and then start adding to a new one.
                else if (Math.Abs(currentCoord - prevCoord) > 0.01f)
                {
                    for (int i = 0; i < nrOfLODGroups; i++)
                    {
                        referenceManager.graphGenerator.isCreating = true;
                        // referenceManager.graphGenerator.AddLODGroup(combGraph, i);
                        //
                        // combGraph.maxCoordValues = maxCoords;
                        // combGraph.minCoordValues = minCoords;
                        // referenceManager.graphGenerator.SliceClustering(lodGroup: i);
                        // while (referenceManager.graphGenerator.isCreating)
                        // {
                        //     yield return null;
                        // }

                        //gs.zCoord = gp.WorldPosition.z;
                        combGraph.maxCoordValues = maxCoords;
                        combGraph.minCoordValues = minCoords;
                        StartCoroutine(referenceManager.graphGenerator.SliceClusteringLOD(nrOfLODGroups));

                        while (referenceManager.graphGenerator.isCreating)
                        {
                            yield return null;
                        }

                        //gs.zCoord = gp.WorldPosition.z;

                        if (nrOfLODGroups > 1)
                        {
                            combGraph.gameObject.AddComponent<LODGroup>();
                            referenceManager.graphGenerator.UpdateLODGroups(combGraph);
                        }

                        combGraph = referenceManager.graphGenerator.CreateGraph(GraphGenerator.GraphType.SPATIAL);
                        combGraph.lodGroups = nrOfLODGroups;
                        combGraph.textures = new Texture2D[nrOfLODGroups];
                        yield return null;
                        referenceManager.graphManager.Graphs.Add(combGraph);
                        referenceManager.graphManager.originalGraphs.Add(combGraph);
                        combGraph.transform.parent = parent.transform;
                        yield return null;
                        gs = combGraph.gameObject.AddComponent<GraphSlice>();
                        gs.SliceNr = ++sliceNr;
                        combGraph.transform.localPosition = new Vector3(0, 0, 0);
                        combGraph.GraphName = "Slice" + sliceNr;
                        combGraph.gameObject.name = "Slice" + sliceNr;
                    }
                }

                // last gp: finish the final slice
                else if (n == gps.Count - 1)
                {
                    c = referenceManager.cellManager.GetCell(gpTuple.Item1);
                    gp = referenceManager.graphGenerator.AddGraphPoint(c, gpTuple.Item2.x, gpTuple.Item2.y,
                        gpTuple.Item2.z);
                    for (int i = 0; i < nrOfLODGroups; i++)
                    {
                        referenceManager.graphGenerator.isCreating = true;
                        referenceManager.graphGenerator.AddLODGroup(combGraph, i);
                        combGraph.maxCoordValues = maxCoords;
                        combGraph.minCoordValues = minCoords;
                        referenceManager.graphGenerator.SliceClustering(lodGroup: i);
                        while (referenceManager.graphGenerator.isCreating)
                        {
                            yield return null;
                        }

                        //gs.zCoord = gp.WorldPosition.z;

                        if (nrOfLODGroups > 1)
                        {
                            combGraph.gameObject.AddComponent<LODGroup>();
                            referenceManager.graphGenerator.UpdateLODGroups(combGraph);
                        }

                        combGraph.transform.localPosition = new Vector3(0, 0, 0);
                        continue;
                    }

                    c = referenceManager.cellManager.GetCell(gpTuple.Item1);
                    gp = referenceManager.graphGenerator.AddGraphPoint(c, gpTuple.Item2.x, gpTuple.Item2.y,
                        gpTuple.Item2.z);
                    prevCoord = currentCoord;
                }


                StartCoroutine(sg.AddSlices());
            }
        }
    }
}