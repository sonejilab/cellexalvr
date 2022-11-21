using CellexalVR.Extensions;
using CellexalVR.General;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CellexalVR.AnalysisLogic;
using System.IO;

namespace CellexalVR.Interaction
{
    public class AutoCompleteList : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public List<ClickableTextPanel> listNodes;
        public bool buildTree;

        private List<Tuple<string, Definitions.Measurement>> namesOfThings;
        public bool ListEnabled { get; set; } = true;

        private BKTreeNode root;
        private int addCost1 = 1;
        private int addCost2 = 1;
        private int subCost = 1;
        private int radius = 2;
        // used for the Levenshtein distance so we don't have to create a new matrix everytime.
        private int[,] scoreMatrix;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            //scoreMatrix = new int[16, 16];
            namesOfThings = new List<Tuple<string, Definitions.Measurement>>();
            if (buildTree)
            {
                CellexalEvents.GraphsLoaded.AddListener(Init);
                CellexalEvents.ScarfObjectLoaded.AddListener(Init);
            }
            CellexalEvents.GraphsUnloaded.AddListener(Clear);
        }

        /// <summary>
        /// Fills the list of gene names, attributes, and facs and then generates the BK-tree.
        /// </summary>
        private void Init()
        {
            StartCoroutine(InitCoroutine());
        }

        private IEnumerator InitCoroutine()
        {
            string[] results = new string[0];
            int longestNameLength = 0;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            CellexalLog.Log("Started building autocomplete list bk-tree");
            namesOfThings.Clear();
            //summertwerk
            if (referenceManager.inputReader.h5readers.Count > 0)
            {

                //Grabbing the first h5reader since we dont know the path
                results = referenceManager.inputReader.h5readers.First().Value.index2genename;
            }
            else if (ScarfManager.instance.scarfActive)
            {
                StartCoroutine(ScarfManager.instance.GetFeatureNames());
                while (ScarfManager.instance.reqPending)
                    yield return null;

                results = ScarfManager.instance.geneNames;
                //results = ScarfManager.scarfObject.feature_names.ToArray();
            }
            else if (File.Exists($"Data/{CellexalUser.DatasetName}/database.sqlite"))
            {
                SQLiter.SQLite database = referenceManager.database;
                //yield return new WaitForSeconds(2);
                while (database.QueryRunning)
                {
                    yield return null;
                }
                database.QueryGeneNames();

                while (database.QueryRunning)
                {
                    yield return null;
                }
                //summertwerk
                results = (string[])database._result.ToArray(typeof(string));
            }

            for (int i = 0; i < results.Length; ++i)
            {
                string name = (string)results[i];
                if (name.Length > longestNameLength)
                {
                    longestNameLength = name.Length;
                }
                namesOfThings.Add(new Tuple<string, Definitions.Measurement>(name, Definitions.Measurement.GENE));
            }
            // TODO: Make color by attr work from keyboard(?)
            string[] attributes = ReferenceManager.instance.cellManager.Attributes?.ToArray();
            if (attributes != null)
            {
                try
                {
                    foreach (string attribute in attributes)
                    {
                        namesOfThings.Add(new Tuple<string, Definitions.Measurement>(attribute, Definitions.Measurement.ATTRIBUTE));
                        if (attribute.Length > longestNameLength)
                        {
                            longestNameLength = attribute.Length;
                        }
                    }
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not read attributes. Caught error - " + e.StackTrace);
                }
            }
            string[] facs = referenceManager.cellManager.Facs;
            if (facs != null)
            {
                try
                {
                    foreach (string f in facs)
                    {
                        namesOfThings.Add(new Tuple<string, Definitions.Measurement>(f, Definitions.Measurement.FACS));
                        if (f.Length > longestNameLength)
                        {
                            longestNameLength = f.Length;
                        }
                    }
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not read facs. Caught error - " + e.StackTrace);
                }
            }
            string[] numericalAttributes = referenceManager.cellManager.NumericalAttributes;
            if (numericalAttributes != null)
            {
                try
                {
                    foreach (string n in numericalAttributes)
                    {
                        namesOfThings.Add(new Tuple<string, Definitions.Measurement>(n, Definitions.Measurement.NUM));
                        if (n.Length > longestNameLength)
                        {
                            longestNameLength = n.Length;
                        }
                    }
                }
                catch (Exception e)
                {
                    CellexalLog.Log("Could not read numerical attributes. Caught error - " + e.StackTrace);
                }
            }

            scoreMatrix = new int[longestNameLength + 1, longestNameLength + 1];
            // two sides of the score matrix are always the same
            for (int i = 0; i <= longestNameLength; ++i)
            {
                scoreMatrix[0, i] = i;
                scoreMatrix[i, 0] = i;
            }

            if (namesOfThings.Count == 0)
                yield break;

            // create the BK-tree
            // choose an arbitrary word as our root
            root = new BKTreeNode(this, namesOfThings[0].Item1, namesOfThings[0].Item2, 0);

            // go through the rest of the things and add them one by one
            for (int i = 1; i < namesOfThings.Count; ++i)
            {
                root.AddWord(namesOfThings[i].Item1, namesOfThings[i].Item2);
            }
            //int numChildren = 0;
            //int numLeaves = 0;
            //int depth = 0;
            //root.TreeInfo(ref numChildren, ref numLeaves, 0, ref depth);
            //float avgChildren = numChildren / ((float)namesOfThings.Count - numLeaves);
            //print(avgChildren + " " + depth);
            ReferenceManager.instance.filterNameKeyboardAutoCompleteList.SetBKTree(root);
            stopwatch.Stop();
            CellexalLog.Log(string.Format("Successfully built autocomplete list bk-tree in {0}", stopwatch.Elapsed.ToString()));
        }

        /// <summary>
        /// Clears the bk tree
        /// </summary>
        private void Clear()
        {
            ClearList();
            root = null;
        }

        /// <summary>
        /// Sets the root of the bk tree
        /// </summary>
        private void SetBKTree(BKTreeNode root)
        {
            this.root = root;
        }

        /// <summary>
        /// Looks up if a string is a gene, facs marker or an attribute.
        /// </summary>
        /// <param name="name">The name of a gene, facs marker or attribute.</param>
        /// <returns>The type of <paramref name="name"/>, or <see cref="Definitions.Measurement.INVALID"/> if <paramref name="name"/> is neither a gene, facs marker nor attribute.</returns>
        public Definitions.Measurement LookUpName(string name)
        {
            List<Tuple<int, BKTreeNode>> result = new List<Tuple<int, BKTreeNode>>();
            root.SearchForNode(name, 0, ref result);
            if (result.Count == 0)
            {
                return Definitions.Measurement.INVALID;
            }
            else
            {
                return result[0].Item2.type;
            }
        }
        /// <summary>
        /// Updates the list with appropriate guesses for what the user might mean when they typed something on the keyboard.
        /// </summary>
        /// <param name="word">The word typed on the keyboard.</param>
        public void UpdateList(string word)
        {
            if (root == null)
            {
                return;
            }

            if (word == "")
            {
                ClearList();
                return;
            }
            List<Tuple<int, BKTreeNode>> candidates = new List<Tuple<int, BKTreeNode>>(64);
            root.SearchForNode(word, radius, ref candidates);
            //print(candidates.Count);
            candidates.Sort((Tuple<int, BKTreeNode> a, Tuple<int, BKTreeNode> b) => (a.Item1 - b.Item1));
            for (int i = 0; i < listNodes.Count && i < candidates.Count; ++i)
            {
                var node = candidates[i].Item2;
                if (i < candidates.Count)
                    listNodes[i].SetText(node.value, node.type);
                else
                    listNodes[i].SetText("", Definitions.Measurement.INVALID);
            }
        }

        /// <summary>
        /// Clears the panels containing the suggestions.
        /// </summary>
        public void ClearList()
        {
            foreach (var listNode in listNodes)
            {
                listNode.SetText("", Definitions.Measurement.INVALID);
            }
        }

        /// <summary>
        /// Calculates the Levenshtein distance between two words.
        /// </summary>
        /// <param name="word1">The first word.</param>
        /// <param name="word2">The second word.</param>
        /// <returns>The Levenshtein distance between <paramref name="word1"/> and <paramref name="word2"/>.</returns>
        protected int LevenshteinDistance(string word1, string word2)
        {
            int actualSubCost;
            word1 = word1.ToLower();
            word2 = word2.ToLower();
            for (int i = 1; i <= word1.Length; ++i)
            {
                for (int j = 1; j <= word2.Length; ++j)
                {
                    if (word1[i - 1] == word2[j - 1])
                        actualSubCost = 0;
                    else
                        actualSubCost = subCost;

                    scoreMatrix[i, j] = Math.Min(Math.Min(scoreMatrix[i - 1, j] + addCost1, scoreMatrix[i, j - 1] + addCost2), scoreMatrix[i - 1, j - 1] + actualSubCost);
                }
            }
            return scoreMatrix[word1.Length, word2.Length];
        }

        /// <summary>
        /// Inner class that represents a node in the BK-tree.
        /// </summary>
        private class BKTreeNode
        {
            // reference to outer class
            public AutoCompleteList _outer;
            public string value;
            public Definitions.Measurement type;
            public int distanceToParent;
            public List<BKTreeNode> children;

            public BKTreeNode(AutoCompleteList outer, string value, Definitions.Measurement type, int distanceToParent)
            {
                _outer = outer;
                this.value = value;
                this.type = type;
                this.distanceToParent = distanceToParent;
                children = new List<BKTreeNode>();
            }

            /// <summary>
            /// Check if this node has a children with a certain distance.
            /// </summary>
            /// <param name="distance">The distance to look for.</param>
            /// <returns>The index of the child with the same distance, or -1 if no child with the distance was found.</returns>
            public int SameDistance(int distance)
            {
                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i].distanceToParent == distance)
                    {
                        return i;
                    }
                }
                return -1;
            }

            /// <summary>
            /// Adds a word to the tree, it may not be added to this node but rather a child (or grand-child or grand-grand-child and so on).
            /// Should only be called on the root of the tree.
            /// </summary>
            /// <param name="word">The word to add.</param>
            public void AddWord(string word, Definitions.Measurement type)
            {
                int distance = _outer.LevenshteinDistance(value, word);
                int childWithSameDistance = SameDistance(distance);
                if (childWithSameDistance != -1)
                {
                    // if this word has the same distance as an existing child, pass it on the that child.
                    children[childWithSameDistance].AddWord(word, type);
                }
                else
                {
                    // otherwise, add it as a child to this node.
                    children.Add(new BKTreeNode(_outer, word, type, distance));
                }
            }

            /// <summary>
            /// Searches the tree for all nodes that are within a Levenshtein distance of a word.
            /// </summary>
            /// <param name="word">The word to search for.</param>
            /// <param name="radius">The maximum allowed Levenshtein distance the results may differ from the word.</param>
            /// <param name="result">A list of words that are within <paramref name="radius"/> Levenshtein distance of <paramref name="word"/>.</param>
            public void SearchForNode(string word, int radius, ref List<Tuple<int, BKTreeNode>> result)
            {
                int thisNodeDistance = _outer.LevenshteinDistance(value, word);
                if (thisNodeDistance <= radius)
                {
                    result.Add(new Tuple<int, BKTreeNode>(thisNodeDistance, this));
                }
                int lowerRadiusRange = thisNodeDistance - radius;
                int upperRadiusRange = thisNodeDistance + radius;
                foreach (BKTreeNode child in children)
                {
                    int childDistance = child.distanceToParent;
                    if (childDistance >= lowerRadiusRange && childDistance <= upperRadiusRange)
                    {
                        child.SearchForNode(word, radius, ref result);
                    }
                }
            }

            /// <summary>
            /// Used for debugging purposes.
            /// </summary>
            public void TreeInfo(ref int numChildren, ref int numLeaves, int depth, ref int maxDepth)
            {
                numChildren += children.Count;
                if (children.Count == 0)
                {
                    numLeaves++;
                }
                if (depth > maxDepth)
                {
                    maxDepth = depth;
                }
                foreach (BKTreeNode child in children)
                {
                    child.TreeInfo(ref numChildren, ref numLeaves, depth + 1, ref maxDepth);
                }
            }
        }
    }

}