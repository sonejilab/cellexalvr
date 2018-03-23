using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCompleteList : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public List<TextMesh> listNodes;

    private List<Tuple<string, string>> namesOfThings;

    private string keyBoardOutput;
    public string KeyboardOutput
    {
        get { return keyBoardOutput; }

        set
        {
            keyBoardOutput = value;
            UpdateList(value);
        }
    }
    private BKTreeNode root;
    private int addCost1 = 1;
    private int addCost2 = 1;
    private int subCost = 1;
    private int radius = 2;
    // used for the Levenshtein distance so we don't have to create a new matrix everytime.
    private int[,] scoreMatrix;

    private void Start()
    {
        //scoreMatrix = new int[16, 16];
        namesOfThings = new List<Tuple<string, string>>();
        CellexalEvents.GraphsLoaded.AddListener(Init);
    }

    /// <summary>
    /// Fills the list of gene names, attributes, and facs and thnn generates the BK-tree.
    /// </summary>
    private void Init()
    {

        StartCoroutine(InitCoroutine());
    }

    private IEnumerator InitCoroutine()
    {
        namesOfThings.Clear();
        SQLiter.SQLite database = referenceManager.database;
        int longestNameLength = 0;
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
        var results = database._result;
        for (int i = 0; i < results.Count; ++i)
        {
            string name = (string)results[i];
            if (name.Length > longestNameLength)
            {
                longestNameLength = name.Length;
            }
            namesOfThings.Add(new Tuple<string, string>(name, "gene"));
        }

        string[] attributes = referenceManager.cellManager.Attributes;
        foreach (string attribute in attributes)
        {
            namesOfThings.Add(new Tuple<string, string>(attribute, "attribute"));
            if (attribute.Length > longestNameLength)
            {
                longestNameLength = attribute.Length;
            }
        }

        string[] facs = referenceManager.cellManager.Facs;
        foreach (string f in facs)
        {
            namesOfThings.Add(new Tuple<string, string>(f, "facs"));
            if (f.Length > longestNameLength)
            {
                longestNameLength = f.Length;
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

    }

    private void UpdateList(string word)
    {
        if (word == "")
        {
            for (int i = 0; i < listNodes.Count; ++i)
            {
                listNodes[i].text = "";
            }
            return;
        }
        List<Tuple<int, BKTreeNode>> candidates = new List<Tuple<int, BKTreeNode>>(64);
        root.SearchForNode(word, radius, ref candidates);
        //print(candidates.Count);
        candidates.Sort((Tuple<int, BKTreeNode> a, Tuple<int, BKTreeNode> b) => (a.Item1 - b.Item1));
        for (int i = 0; i < listNodes.Count; ++i)
        {
            if (i < candidates.Count)
                listNodes[i].text = candidates[i].Item2.type + " " + candidates[i].Item1 + " " + candidates[i].Item2.value;
            else
                listNodes[i].text = "";
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
        public string type;
        public int distanceToParent;
        public List<BKTreeNode> children;

        public BKTreeNode(AutoCompleteList outer, string value, string type, int distanceToParent)
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
        public void AddWord(string word, string type)
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

