using CellexalVR.AnalysisLogic.H5reader;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// Class that handles the reading of attribute/meta files input.
    /// </summary>
    public class AttributeReader : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        //summertwerk
        /// <summary>
        /// Reads all attributes from current h5 file
        /// </summary>
        public IEnumerator H5ReadAttributeFilesCoroutine(H5Reader h5Reader)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            List<string> available_attributes = new List<string>();


            foreach (string attr in h5Reader.attributes)
            {
                print("reading attribute " + attr);

                while (h5Reader.busy)
                    yield return null;

                StartCoroutine(h5Reader.GetAttributes(attr));

                while (h5Reader.busy)
                    yield return null;


                string[] attrs = h5Reader._attrResult;
                string[] cellNames = h5Reader.index2cellname;

                for (int j = 0; j < cellNames.Length; j++)
                {
                    string cellName = cellNames[j];

                    string attribute_name = attr + "@" + attrs[j];
                    int index_of_attribute;
                    if (!available_attributes.Contains(attribute_name))
                    {
                        available_attributes.Add(attribute_name);
                        index_of_attribute = available_attributes.Count - 1;
                    }
                    else
                    {
                        index_of_attribute = available_attributes.IndexOf(attribute_name);
                    }


                    referenceManager.cellManager.AddAttribute(cellName, attribute_name,
                        index_of_attribute % CellexalConfig.Config.SelectionToolColors.Length);
                    if (j % 500 == 0)
                    {
                        yield return null;
                    }
                }
            }

            referenceManager.attributeSubMenu.CreateButtons(available_attributes.ToArray());

            referenceManager.cellManager.AttributesNames = available_attributes;
            for (int i = CellexalConfig.Config.SelectionToolColors.Length;
                i < referenceManager.cellManager.AttributesNames.Count;
                i++)
            {
                referenceManager.settingsMenu.AddSelectionColor();
            }

            referenceManager.settingsMenu.unsavedChanges = false;
            stopwatch.Stop();
            referenceManager.inputReader.attributeFileRead = true;
            CellexalLog.Log("h5 read attributes in " + stopwatch.Elapsed.ToString());
        }


        /// <summary>
        /// Reads an attribute file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public IEnumerator ReadAttributeFilesCoroutine(string path)
        {
            // Read the each .meta.cell file
            // The file format should be
            //              TYPE_1  TYPE_2  ...
            //  CELLNAME_1  [0,1]   [0,1]
            //  CELLNAME_2  [0,1]   [0,1]
            // ...
            yield return null;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            string[] metaCellFiles = Directory.GetFiles(path, "*.meta.cell");
            foreach (string metaCellFile in metaCellFiles)
            {
                FileStream metaCellFileStream = new FileStream(metaCellFile, FileMode.Open);
                StreamReader metaCellStreamReader = new StreamReader(metaCellFileStream);

                // first line is a header line
                string header = metaCellStreamReader.ReadLine();
                if (header != null)
                {
                    string[] attributeTypes = header.Split('\t');
                    string[] actualAttributeTypes = new string[attributeTypes.Length - 1];
                    for (int i = 1; i < attributeTypes.Length; ++i)
                    {
                        actualAttributeTypes[i - 1] = attributeTypes[i];
                    }

                    while (!metaCellStreamReader.EndOfStream)
                    {
                        string line = metaCellStreamReader.ReadLine();
                        if (line == "")
                            continue;

                        if (line != null)
                        {
                            string[] words = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            string cellName = words[0];
                            for (int j = 1; j < words.Length; ++j)
                            {
                                if (words[j] == "1")
                                    referenceManager.cellManager.AddAttribute(cellName, attributeTypes[j],
                                        (j - 1) % CellexalConfig.Config.SelectionToolColors.Length);
                            }
                        }
                    }

                    metaCellStreamReader.Close();
                    metaCellFileStream.Close();
                    referenceManager.cellManager.AttributesNames = new List<string>(); //actualAttributeTypes.ToList();
                    referenceManager.attributeSubMenu.CreateButtons(actualAttributeTypes);
                }

                int nrOfAttributes = referenceManager.cellManager.AttributesNames.Count;
                int nrOfSelToolColors = CellexalConfig.Config.SelectionToolColors.Length;
                if (nrOfAttributes > nrOfSelToolColors)
                {
                    referenceManager.settingsMenu.AddSelectionColors(nrOfAttributes - nrOfSelToolColors);
                    referenceManager.settingsMenu.unsavedChanges = false;
                }
            }

            stopwatch.Stop();
            referenceManager.inputReader.attributeFileRead = true;
            CellexalLog.Log("read attributes in " + stopwatch.Elapsed.ToString());

        }

        public void ReadCondensedAttributeFile(string fullPath)
        {
            using FileStream metaCellFileStream = new FileStream(fullPath, FileMode.Open);
            using StreamReader metaCellStreamReader = new StreamReader(metaCellFileStream);

            List<string> categories = new List<string>();
            string header = metaCellStreamReader.ReadLine();
            string[] splitHeader = header.Split();
            // first entry is "cellID", skip that as it's not an attribute
            for (int i = 1; i < splitHeader.Length; ++i)
            {
                categories.Add(splitHeader[i]);
            }

            // create empty texture prefabs that can be cloned with Instantiate() later to make the actual textures
            // key: (Graph, lod group, attribute) where attributes are on the format "[category]@[attribute]"
            Dictionary<(Graph graph, int lodGroup, string attribute), Texture2D> textures = new Dictionary<(Graph, int, string), Texture2D>();
            // key: (Graph, lod group)
            Dictionary<(Graph graph, int lodGroup), Texture2D> texturePrefabs = new Dictionary<(Graph, int), Texture2D>();

            foreach (Graph graph in ReferenceManager.instance.graphManager.Graphs)
            {
                for (int group = 0; group < graph.lodGroups; ++group)
                {
                    Texture2D texture = new Texture2D(graph.textureWidths[group], graph.textureHeights[group], TextureFormat.RGBA32, false);
                    texture.Apply();
                    Unity.Collections.NativeArray<Color32> rawTextureData = texture.GetRawTextureData<Color32>();

                    // fill the new texture with a black solid background
                    for (int i = 0; i < texture.width * texture.height; ++i)
                    {
                        rawTextureData[i] = new Color32(0, 0, 0, 255);
                    }
                    texturePrefabs[(graph, group)] = texture;
                }
            }

            CellManager cellManager = ReferenceManager.instance.cellManager;
            if (cellManager.Attributes is null)
            {
                cellManager.Attributes = new Dictionary<string, HashSet<Cell>>();
            }
            else
            {
                cellManager.Attributes.Clear();
            }

            while (!metaCellStreamReader.EndOfStream)
            {
                string line = metaCellStreamReader.ReadLine();
                string[] splitLine = line.Split();

                for (int i = 1; i < splitLine.Length; ++i)
                {
                    string cellName = splitLine[0];
                    string attribute = splitHeader[i] + "@" + splitLine[i];


                    if (!cellManager.Attributes.ContainsKey(attribute))
                    {
                        cellManager.Attributes[attribute] = new HashSet<Cell>();
                    }
                    cellManager.Attributes[attribute].Add(cellManager.GetCell(cellName));

                    foreach (Graph graph in ReferenceManager.instance.graphManager.Graphs)
                    {
                        for (int group = 0; group < graph.lodGroups; ++group)
                        {
                            (Graph, int, string) key = (graph, group, attribute);
                            if (!textures.ContainsKey(key))
                            {
                                // if texture doesnt exist yet, create it
                                textures[key] = Instantiate(texturePrefabs[(graph, group)]);
                                graph.attributeMasks[(group, attribute)] = textures[key];
                            }
                            Graph.GraphPoint graphPoint = graph.FindGraphPoint(cellName);
                            if (graphPoint is not null)
                            {
                                Vector2Int textureCoord = graphPoint.textureCoord[group];
                                textures[key].SetPixel(textureCoord.x, textureCoord.y, Color.white);
                            }
                        }
                    }
                }
            }

            foreach (Texture2D texture in textures.Values)
            {
                texture.Apply();
            }

            // recolor attribute masks to the color that corresponds to that attribute
            HashSet<string> attributesHashSet = new HashSet<string>();
            foreach ((Graph _, int _, string attribute) in textures.Keys)
            {
                attributesHashSet.Add(attribute);
            }

            List<string> attributes = new List<string>(attributesHashSet);

            // sort attributes in alphabetical order, but respecting any attributes that end with an integer
            // e.g. we don't want: [type@attr1, type@attr10, type@attr11, type@attr2,  type@attr20]
            //    we instead want: [type@attr1, type@attr2,  type@attr10, type@attr11, type@attr20]
            List<(string, int?)> tuples = new List<(string, int?)>();
            for (int i = 0; i < attributes.Count; ++i)
            {
                string entry = attributes[i];
                // find first character in string of the trailing integer (if any)
                int firstIntCharacterIndex = -1;
                for (int j = entry.Length - 1; j >= 0; --j)
                {
                    // scan the string backwards to find the trailing integer
                    if (char.IsNumber(entry[j]))
                    {
                        firstIntCharacterIndex = j;
                    }
                    else
                    {
                        // no longer scanning an integer
                        break;
                    }
                }

                if (firstIntCharacterIndex != -1)
                {
                    string beforeInteger = entry[..firstIntCharacterIndex];
                    int theInteger = int.Parse(entry[firstIntCharacterIndex..]);
                    tuples.Add((beforeInteger, theInteger));
                }
                else
                {
                    tuples.Add((entry, null));
                }
            }

            tuples.Sort((a, b) => a.Item1.Equals(b.Item1) ? a.Item2.Value.CompareTo(b.Item2) : a.Item1.CompareTo(b.Item1));

            // reassemble the list of attributes in the sorted order
            for (int i = 0; i < tuples.Count; ++i)
            {
                attributes[i] = tuples[i].Item1 + (tuples[i].Item2 is not null ? tuples[i].Item2 : "");
            }

            cellManager.AttributesNames = attributes;
            ReferenceManager.instance.attributeSubMenu.CreateButtons(attributes.ToArray());

            // save all textures as .png so we don't have to recalculate them next time
            string parentDir = Path.GetDirectoryName(fullPath);
            string generatedDirectoryPath = Path.Join(parentDir, "Generated");
            if (!Directory.Exists(generatedDirectoryPath))
            {
                Directory.CreateDirectory(generatedDirectoryPath);
                CellexalLog.Log("Created directory " + generatedDirectoryPath);
            }

            for (int i = 0; i < attributes.Count; ++i)
            {
                string attribute = attributes[i];
                foreach (Graph graph in ReferenceManager.instance.graphManager.Graphs)
                {
                    for (int lodGroup = 0; lodGroup < graph.lodGroups; ++lodGroup)
                    {
                        Texture2D texture = textures[(graph, lodGroup, attribute)];
                        Unity.Collections.NativeArray<Color32> rawColorData = texture.GetRawTextureData<Color32>();
                        byte redValue = (byte)(CellexalConfig.Config.GraphNumberOfExpressionColors + i);
                        for (int j = 0; j < rawColorData.Length; ++j)
                        {
                            if (rawColorData[j].r == 255)
                            {
                                rawColorData[j] = new Color32(redValue, 0, 0, 255);
                            }
                        }
                        texture.Apply();
                        byte[] pngData = texture.EncodeToPNG();
                        string filename = graph.GraphName + "_LOD" + lodGroup + "_" + attribute + ".png";

                        File.WriteAllBytes(Path.Join(generatedDirectoryPath, filename), pngData);

                    }
                }
            }

            CellexalLog.Log($"Saved {textures.Count} attribute masks in {generatedDirectoryPath}.");
        }
    }
}
