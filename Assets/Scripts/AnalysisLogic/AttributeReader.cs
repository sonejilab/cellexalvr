using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CellexalVR.General;
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
        public IEnumerator H5ReadAttributeFilesCoroutine()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            List<string> available_attributes = new List<string>();

            foreach (string attr in referenceManager.h5Reader.attributes)
            {
                print("reading attribute " + attr);

                while (referenceManager.h5Reader.busy)
                    yield return null;

                StartCoroutine(referenceManager.h5Reader.GetAttributes(attr));

                while (referenceManager.h5Reader.busy)
                    yield return null;


                string[] attrs = referenceManager.h5Reader._attrResult;
                string[] cellNames = referenceManager.h5Reader.index2cellname;

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

            referenceManager.cellManager.Attributes = available_attributes.ToArray();
            for (int i = CellexalConfig.Config.SelectionToolColors.Length;
                i < referenceManager.cellManager.Attributes.Length;
                i++)
            {
                referenceManager.settingsMenu.AddSelectionColor();
            }
            referenceManager.settingsMenu.unsavedChanges = false;
            //if (cellManager.Attributes.Length > CellexalConfig.Config.SelectionToolColors.Length)
            //{
            //    CellexalError.SpawnError("Attributes", "The number of attributes are higher than the number of colours in your config." +
            //        " Consider adding more colours in the settings menu (under Selection Colours)");
            //}
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
                    string[] attributeTypes = header.Split(null);
                    string[] actualAttributeTypes = new string[attributeTypes.Length - 1];
                    for (int i = 1; i < attributeTypes.Length; ++i)
                    {
                        //if (attributeTypes[i].Length > 10)
                        //{
                        //    attributeTypes[i] = attributeTypes[i].Substring(0, 10);
                        //}
                        actualAttributeTypes[i - 1] = attributeTypes[i];
                        //print(attributeTypes[i]);
                    }

                    int yieldCount = 0;
                    while (!metaCellStreamReader.EndOfStream)
                    {
                        string line = metaCellStreamReader.ReadLine();
                        if (line == "")
                            continue;

                        if (line != null)
                        {
                            string[] words = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);

                            string cellName = words[0];
                            for (int j = 1; j < words.Length; ++j)
                            {
                                if (words[j] == "1")
                                    referenceManager.cellManager.AddAttribute(cellName, attributeTypes[j],
                                        (j - 1) % CellexalConfig.Config.SelectionToolColors.Length);
                            }
                        }

                        yieldCount++;
                        if (yieldCount % 500 == 0)
                            yield return null;
                    }

                    metaCellStreamReader.Close();
                    metaCellFileStream.Close();
                    referenceManager.attributeSubMenu.CreateButtons(actualAttributeTypes);
                    referenceManager.cellManager.Attributes = actualAttributeTypes;
                }

                for (int i = CellexalConfig.Config.SelectionToolColors.Length;
                    i < referenceManager.cellManager.Attributes.Length;
                    i++)
                {
                    referenceManager.settingsMenu.AddSelectionColor();
                }

                referenceManager.settingsMenu.unsavedChanges = false;
                //if (cellManager.Attributes.Length > CellexalConfig.Config.SelectionToolColors.Length)
                //{
                //    CellexalError.SpawnError("Attributes", "The number of attributes are higher than the number of colours in your config." +
                //        " Consider adding more colours in the settings menu (under Selection Colours)");
                //}
            }

            stopwatch.Stop();
            referenceManager.inputReader.attributeFileRead = true;
            CellexalLog.Log("read attributes in " + stopwatch.Elapsed.ToString());
        }
    }
}