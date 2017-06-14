using System;
using UnityEngine;
using System.Data;
using System.IO;
using System.Threading;
using Mono.Data.Sqlite;

//Borrows from http://answers.unity3d.com/questions/743400/database-sqlite-setup-for-unity.html
//Dictionary from https://wordnet.princeton.edu/

namespace ListView
{
    public class DictionaryList : ListViewController<DictionaryListItemData, DictionaryListItem>
    {
        public const string editorDatabasePath = "ListView/Examples/8. Dictionary/wordnet30.db";
        public const string databasePath = "wordnet30.db";
        public int batchSize = 15;
        public float scrollDamping = 15f;
        public float maxMomentum = 200f;
        public string defaultTemplate = "DictionaryItem";
        public GameObject loadingIndicator;

        public int maxWordCharacters = 30; //Wrap word after 30 characters
        public int definitionCharacterWrap = 40; //Wrap definition after 40 characters
        public int maxDefinitionLines = 4; //Max 4 lines per definition

        delegate void WordsResult(DictionaryListItemData[] words);

        volatile bool m_DBLock;

        DictionaryListItemData[] m_Cleanup;
        int m_DataLength; //Total number of items in the data set
        int m_BatchOffset; //Number of batches we are offset
        bool m_Scrolling;
        bool m_Loading;
        float m_ScrollReturn = float.MaxValue;
        float m_ScrollDelta;
        float m_LastScrollOffset;

        IDbConnection m_DBConnection;

        protected override void Setup()
        {
            base.Setup();

#if UNITY_EDITOR
            string conn = "URI=file:" + Path.Combine(Application.dataPath, editorDatabasePath);
#else
            string conn = "URI=file:" + Path.Combine(Application.dataPath, databasePath);
#endif

            m_DBConnection = new SqliteConnection(conn);
            m_DBConnection.Open(); //Open connection to the database.

            if (maxWordCharacters < 4)
            {
                Debug.LogError("Max word length must be > 3");
            }

            try
            {
                IDbCommand dbcmd = m_DBConnection.CreateCommand();
                string sqlQuery = "SELECT COUNT(lemma) FROM word as W JOIN sense as S on W.wordid=S.wordid JOIN synset as Y on S.synsetid=Y.synsetid";
                dbcmd.CommandText = sqlQuery;
                IDataReader reader = dbcmd.ExecuteReader();
                while (reader.Read())
                {
                    m_DataLength = reader.GetInt32(0);
                }
                reader.Close();
                dbcmd.Dispose();
            } catch
            {
                Debug.LogError("DB error, couldn't get total data length");
            }

            data = null;
            //Start off with some data
            GetWords(0, batchSize * 3, words => {
                                                    data = words;
            });
        }

        void OnDestroy()
        {
            m_DBConnection.Close();
            m_DBConnection = null;
        }

        void GetWords(int offset, int range, WordsResult result)
        {
            if (m_DBLock)
            {
                return;
            }
            if (result == null)
            {
                Debug.LogError("Called GetWords without a result callback");
                return;
            }
            m_DBLock = true;
            //Not sure what the current deal is with threads. Hopefully this is OK?
            new Thread(() =>
            {
                try
                {
                    DictionaryListItemData[] words = new DictionaryListItemData[range];
                    IDbCommand dbcmd = m_DBConnection.CreateCommand();
                    string sqlQuery = string.Format("SELECT lemma, definition FROM word as W JOIN sense as S on W.wordid=S.wordid JOIN synset as Y on S.synsetid=Y.synsetid ORDER BY W.wordid limit {0} OFFSET {1}", range, offset);
                    dbcmd.CommandText = sqlQuery;
                    IDataReader reader = dbcmd.ExecuteReader();
                    int count = 0;
                    while (reader.Read())
                    {
                        string lemma = reader.GetString(0);
                        string definition = reader.GetString(1);
                        words[count] = new DictionaryListItemData {template = defaultTemplate};

                        //truncate word if necessary
                        if (lemma.Length > maxWordCharacters)
                        {
                            lemma = lemma.Substring(0, maxWordCharacters - 3) + "...";
                        }
                        words[count].word = lemma;

                        //Wrap definition
                        string[] wrds = definition.Split(' ');
                        int charCount = 0;
                        int lineCount = 0;
                        foreach (var wrd in wrds)
                        {
                            charCount += wrd.Length + 1;
                            if (charCount > definitionCharacterWrap)
                            { //Guesstimate
                                if (++lineCount >= maxDefinitionLines)
                                {
                                    words[count].definition += "...";
                                    break;
                                }
                                words[count].definition += "\n";
                                charCount = 0;
                            }
                            words[count].definition += wrd + " ";
                        }
                        count++;
                    }
                    if (count < batchSize)
                    {
                        Debug.LogWarning("reached end");
                    }
                    reader.Close();
                    dbcmd.Dispose();
                    result(words);
                } catch (Exception e)
                {
                    Debug.LogError("Exception reading from DB: " + e.Message);
                }
                m_DBLock = false;
                m_Loading = false;
            }).Start();
        }

        protected override void ComputeConditions()
        {
            if (templates.Length > 0)
            {
                //Use first template to get item size
                m_ItemSize = GetObjectSize(templates[0]);
            }
            //Resize range to nearest multiple of item width
            m_NumItems = Mathf.RoundToInt(range / m_ItemSize.y); //Number of cards that will fit
            range = m_NumItems * m_ItemSize.y;

            //Get initial conditions. This procedure is done every frame in case the collider bounds change at runtime
            m_LeftSide = transform.position + Vector3.up * range * 0.5f + Vector3.left * m_ItemSize.x * 0.5f;

            m_DataOffset = (int) (scrollOffset / itemSize.y);
            if (scrollOffset < 0)
                m_DataOffset--;

            int currBatch = -m_DataOffset / batchSize;
            if (-m_DataOffset > (m_BatchOffset + 2) * batchSize)
            {
                //Check how many batches we jumped
                if (currBatch == m_BatchOffset + 2)
                { //Just one batch, fetch only the next one
                    GetWords((m_BatchOffset + 3) * batchSize, batchSize, words =>
                    {
                        Array.Copy(data, batchSize, data, 0, batchSize * 2);
                        Array.Copy(words, 0, data, batchSize * 2, batchSize);
                        m_BatchOffset++;
                    });
                } else if (currBatch != m_BatchOffset)
                { //Jumped multiple batches. Get a whole new dataset
                    if (!m_Loading)
                        m_Cleanup = data;
                    m_Loading = true;
                    GetWords((currBatch - 1) * batchSize, batchSize * 3, words =>
                    {
                        data = words;
                        m_BatchOffset = currBatch - 1;
                    });
                }
            } else if (m_BatchOffset > 0 && -m_DataOffset < (m_BatchOffset + 1) * batchSize)
            {
                if (currBatch == m_BatchOffset)
                { //Just one batch, fetch only the next one
                    GetWords((m_BatchOffset - 1) * batchSize, batchSize, words =>
                    {
                        Array.Copy(data, 0, data, batchSize, batchSize * 2);
                        Array.Copy(words, 0, data, 0, batchSize);
                        m_BatchOffset--;
                    });
                } else if (currBatch != m_BatchOffset)
                { //Jumped multiple batches. Get a whole new dataset
                    if (!m_Loading)
                        m_Cleanup = data;
                    m_Loading = true;
                    if (currBatch < 1)
                        currBatch = 1;
                    GetWords((currBatch - 1) * batchSize, batchSize * 3, words =>
                    {
                        data = words;
                        m_BatchOffset = currBatch - 1;
                    });
                }
            }
            if (m_Cleanup != null)
            {
                //Clean up all existing gameobjects
                foreach (var item in m_Cleanup)
                {
                    if (item.item != null)
                    {
                        RecycleItem(item.template, item.item);
                        item.item = null;
                    }
                }
                m_Cleanup = null;
            }

            if (m_Scrolling)
            {
                m_ScrollDelta = (scrollOffset - m_LastScrollOffset) / Time.deltaTime;
                m_LastScrollOffset = scrollOffset;
                if (m_ScrollDelta > maxMomentum)
                    m_ScrollDelta = maxMomentum;
                if (m_ScrollDelta < -maxMomentum)
                    m_ScrollDelta = -maxMomentum;
            } else
            {
                scrollOffset += m_ScrollDelta * Time.deltaTime;
                if (m_ScrollDelta > 0)
                {
                    m_ScrollDelta -= scrollDamping * Time.deltaTime;
                    if (m_ScrollDelta < 0)
                    {
                        m_ScrollDelta = 0;
                    }
                } else if (m_ScrollDelta < 0)
                {
                    m_ScrollDelta += scrollDamping * Time.deltaTime;
                    if (m_ScrollDelta > 0)
                    {
                        m_ScrollDelta = 0;
                    }
                }
            }
            if (m_DataOffset >= m_DataLength)
            {
                m_ScrollReturn = scrollOffset;
            }
        }

        public void OnStartScrolling()
        {
            m_Scrolling = true;
        }

        public void OnStopScrolling()
        {
            m_Scrolling = false;
            if (scrollOffset > 0)
            {
                scrollOffset = 0;
                m_ScrollDelta = 0;
            }
            if (m_ScrollReturn < float.MaxValue)
            {
                scrollOffset = m_ScrollReturn;
                m_ScrollReturn = float.MaxValue;
                m_ScrollDelta = 0;
            }
        }

        protected override void UpdateItems()
        {
            if (data == null || data.Length == 0 || m_Loading)
            {
                loadingIndicator.SetActive(true);
                return;
            }
            for (int i = 0; i < data.Length; i++)
            {
                if (i + m_DataOffset + m_BatchOffset * batchSize < -1)
                { //Checking against -1 lets the first element overflow
                    ExtremeLeft(data[i]);
                } else if (i + m_DataOffset + m_BatchOffset * batchSize > m_NumItems)
                {
                    ExtremeRight(data[i]);
                } else
                {
                    ListMiddle(data[i], i + m_BatchOffset * batchSize);
                }
            }
            loadingIndicator.SetActive(false);
        }

        protected override void Positioning(Transform t, int offset)
        {
            t.position = m_LeftSide + (offset * m_ItemSize.y + scrollOffset) * Vector3.down;
        }
    }
}