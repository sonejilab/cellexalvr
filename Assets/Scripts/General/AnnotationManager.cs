using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using CellexalVR.Interaction;
using TMPro;
using UnityEngine;

namespace CellexalVR.General
{
    /// <summary>
    /// Handles annotations on graphs. Annotations are notes added to certain groups of cells.
    /// They can be read from file or added via the gene keyboard.
    /// </summary>
    public class AnnotationManager : MonoBehaviour
    {
        public GameObject annotationTextPrefab;
        public ReferenceManager referenceManager;
        [SerializeField] private GameObject placeAnnotationPrefab;
        private int annotationCtr = 0;
        private GraphManager graphManager;
        private SelectionManager selectionManager;
        private Dictionary<string, List<string>> annotatedPoints = new Dictionary<string, List<string>>();
        private Dictionary<string, List<GameObject>> annotationDictionary = new Dictionary<string, List<GameObject>>();
        private bool annotate;
        private string annotation;
        private bool reading;
        private GameObject annotationSphere;


        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            graphManager = referenceManager.graphManager;
            selectionManager = referenceManager.selectionManager;
        }

        private void OnTriggerClick()
        {
            ManualAddAnnotation(annotation);
        }

        public void AddManualAnnotation(string s)
        {
            annotation = s;
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
            //annotationSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            annotationSphere = Instantiate(placeAnnotationPrefab);
            annotationSphere.transform.parent = ReferenceManager.instance.rightController.transform;
            annotationSphere.transform.localRotation = Quaternion.identity;
            //annotationSphere.transform.localScale = Vector3.one * 0.01f;
            annotationSphere.transform.localPosition = new Vector3(0, 0, 0.07f);
            annotationSphere.GetComponentInChildren<TextMeshPro>().text = s;
        }

        private void ManualAddAnnotation(string s)
        {
            //var col = ReferenceManager.instance.rightController.GetComponent<Collider>();
            var col = annotationSphere.GetComponent<Collider>();
            Vector3 colCenter = col.bounds.center;
            Vector3 colExtents = col.bounds.extents;
            foreach (var graph in graphManager.Graphs)
            {
                Graph.GraphPoint gp;
                var closestPoints = graph.MinkowskiDetection(annotationSphere.transform.position, colCenter, colExtents, -2);
                if (closestPoints.Count > 0)
                {
                    gp = closestPoints[0];
                    if (gp.Group == -1) return;
                    ReferenceManager.instance.multiuserMessageSender.SendMessageAddAnnotation(s, gp.Group, gp.Label);
                    ReferenceManager.instance.annotationManager.AddAnnotation(s, gp.Group, gp.Label);
                    CellexalEvents.RightTriggerClick.RemoveListener(OnTriggerClick);
                    Destroy(annotationSphere);
                }
            }

        }

        [ConsoleCommand("annotationManager", aliases: new string[] { "toggleannotations", "ta" })]
        public void ToggleAnnotationFile(string path, bool toggle)
        {
            if (toggle)
            {
                referenceManager.inputReader.ReadAnnotationFile(path);
            }
            else
            {
                RemoveAnnotations(path);
            }
        }

        [ConsoleCommand("annotationManager", aliases: new string[] { "clearallannotations", "caa" })]
        public void ClearAllAnnotations()
        {
            foreach (string key in annotationDictionary.Keys)
            {
                List<GameObject> annotationObjects = annotationDictionary[key];
                foreach (GameObject obj in annotationObjects)
                {
                    Destroy(obj);
                }

            }
            annotationDictionary.Clear();
            CellexalEvents.AnnotationsCleared.Invoke();
        }

        /// <summary>
        /// Adds the annotation to the currently selected group of cells.
        /// Also adds a line and text object above the cells with the annotation.
        /// </summary>
        /// <param name="annotation">The text that will be shown above the cells and later written to file.</param>
        /// <param name="index">The index of the selection group.</param>
        public void AddAnnotation(string annotation, int index, string gpLabel)
        {
            var cellsToAnnotate = referenceManager.cellManager.GetCells(index).Select(c => c.Label).ToArray();
            selectionManager.RecolorSelectionPoints();
            AddAnnotation(annotation, cellsToAnnotate, gpLabel, index);
        }

        public void AddAnnotation(string annotation, string[] cellsToAnnotate, int index, string gpLabel)
        {
            selectionManager.RecolorSelectionPoints();
            AddAnnotation(annotation, cellsToAnnotate, gpLabel, index);
        }

        /// <summary>
        /// Adds the annotation to the specified graphpoints. For example when reading from an annotation file.
        /// </summary>
        /// <param name="annotation">The annotation string.</param>
        /// <param name="pointsToAnnotate">The points to annotate.</param>
        public void AddAnnotation(string annotation, string[] cellsToAnnotate, string gpLabelToSpawnAt, int group, string path = "")
        {
            foreach (string cellName in cellsToAnnotate)
            {
                if (!annotatedPoints.ContainsKey(annotation))
                    annotatedPoints[annotation] = new List<string>();
                annotatedPoints[annotation].Add(cellName);
            }
            List<GameObject> annotationTexts = new List<GameObject>();
            foreach (Graph graph in graphManager.Graphs)
            {
                GameObject annotationText = Instantiate(annotationTextPrefab, graph.annotationsParent.transform);
                AnnotationTextPanel textPanel = annotationText.GetComponent<AnnotationTextPanel>();
                textPanel.referenceManager = referenceManager;
                textPanel.SetCells(group, graph.FindGraphPoint(gpLabelToSpawnAt).Position);
                annotationText.gameObject.name = annotation;
                annotationText.GetComponentInChildren<TextMeshPro>().text = annotation;
                annotationTexts.Add(annotationText);
            }
            if (annotationDictionary.ContainsKey(annotation))
            {
                foreach (GameObject obj in annotationTexts)
                {
                    annotationDictionary[annotation].Add(obj);
                }
            }
            else
            {
                annotationDictionary[annotation] = annotationTexts;
            }
        }

        private void RemoveAnnotations(string annotationFile)
        {
            List<GameObject> annotationObjects = annotationDictionary[annotationFile];
            foreach (GameObject obj in annotationObjects)
            {
                Destroy(obj);
            }

            annotationDictionary.Remove(annotationFile);
        }


        /// <summary>
        /// Writes the cells that have added annotation to a text file. 
        /// Only cell id and annotation is written to the file.
        /// </summary>
        /// <param name="selection">The selected graphpoints</param>
        /// <param name="filePath"></param>
        /// <param name="annotation">The string to write next to cell id as annotation.</param>
        public void DumpAnnotatedSelectionToTextFile(string filePath = "")
        {
            if (reading) return;
            if (annotatedPoints.Count == 0) return;
            reading = true;
            if (filePath != "")
            {
                string savedSelectionsPath = CellexalUser.UserSpecificFolder + @"\SavedSelections\";
                if (!Directory.Exists(savedSelectionsPath))
                {
                    Directory.CreateDirectory(savedSelectionsPath);
                }

                filePath = savedSelectionsPath + filePath + ".txt";
            }
            else
            {
                string annotationDirectory = CellexalUser.UserSpecificFolder + @"\AnnotatedSelections";
                if (!Directory.Exists(annotationDirectory))
                {
                    CellexalLog.Log("Creating directory " + annotationDirectory.FixFilePath());
                    Directory.CreateDirectory(annotationDirectory);
                }

                filePath = annotationDirectory + "\\annotated_selection" + annotationCtr + ".txt";
                while (File.Exists(filePath))
                {
                    annotationCtr++;
                    filePath = annotationDirectory + "\\annotated_selection" + annotationCtr + ".txt";
                }

                using (StreamWriter file = new StreamWriter(filePath, true))
                {
                    CellexalLog.Log("Dumping selection data to " + CellexalLog.FixFilePath(filePath));
                    CellexalLog.Log("\tSelection consists of  " + selectionManager.GetCurrentSelection().Count +
                                    " points");
                    if (selectionManager.selectionHistory != null)
                        CellexalLog.Log("\tThere are " + selectionManager.selectionHistory.Count +
                                        " entries in the history");
                    foreach (KeyValuePair<string, List<string>> kvp in annotatedPoints)
                    {
                        foreach (string s in kvp.Value)
                        {
                            file.Write(s);
                            file.Write("\t");
                            file.Write(kvp.Key);
                            file.WriteLine();
                        }
                    }
                }

                annotationCtr++;
            }

            referenceManager.selectionFromPreviousMenu.ReadAnnotationFiles();
            reading = false;
            ExportToMetaCellFile();
            annotatedPoints.Clear();
        }

        public void ExportToMetaCellFile()
        {
            if (!referenceManager.inputReader.attributeFileRead) return;
            referenceManager.inputReader.attributeFileRead = false;
            List<string> lines = new List<string>();
            string metaCellFile = Directory.GetFiles(referenceManager.selectionManager.DataDir, "*.meta.cell")[0];

            FileStream metaCellFileStream = new FileStream(metaCellFile, FileMode.Open);
            StreamReader metaCellStreamReader = new StreamReader(metaCellFileStream);
            // first line is a header line
            string header = metaCellStreamReader.ReadLine();
            lines.Add(header);
            if (header != null)
            {
                //int yieldCount = 0;
                while (!metaCellStreamReader.EndOfStream)
                {
                    string line = metaCellStreamReader.ReadLine();
                    lines.Add(line);
                    //yieldCount++;
                    //if (yieldCount % 1000 == 0)
                    //    yield return null;
                }

                metaCellStreamReader.Close();
                metaCellFileStream.Close();
            }

            using (StreamWriter sw = new StreamWriter(metaCellFile))
            {
                foreach (KeyValuePair<string, List<string>> kvp in annotatedPoints)
                {
                    string firstLine = lines[0] + "\t" + "myGroups@" + kvp.Key;
                    sw.WriteLine(firstLine);
                    var annotatedCellIDs = kvp.Value;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        string line = lines[i];
                        string[] words = line.Split('\t');
                        string cellID = words[0];
                        int inSet = annotatedCellIDs.Contains(cellID) ? 1 : 0;
                        sw.WriteLine(line + '\t' + inSet);
                    }

                }
            }
            string path = CellexalUser.DatasetFullPath;
            StartCoroutine(referenceManager.inputReader.attributeReader.ReadAttributeFilesCoroutine(path));
            
        }
    }
}
