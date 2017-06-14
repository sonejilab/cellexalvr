using System;
using UnityEngine;
using System.Collections;

namespace ListView
{
    public class WebList : ListViewController<JSONItemData, JSONItem>
    {
        //Ideas for a better/different example web service are welcome
        //Note: the github API has a rate limit. After a couple of tries, you won't see any results :(
        public string URLFormatString = "https://api.github.com/gists/public?page={0}&per_page={1}";
        public string defaultTemplate = "JSONItem";
        public int batchSize = 15;

        delegate void WebResult(JSONItemData[] data);

        int m_BatchOffset;
        bool m_WebLock;
        bool m_Loading;
        JSONItemData[] m_Cleanup;

        protected override void Setup()
        {
            base.Setup();
            StartCoroutine(GetBatch(0, batchSize * 3, data => { this.data = data; }));
        }

        IEnumerator GetBatch(int offset, int range, WebResult result)
        {
            if (m_WebLock)
                yield break;
            m_WebLock = true;
            JSONItemData[] items = new JSONItemData[range];
            WWW www = new WWW(string.Format(URLFormatString, offset, range));
            while (!www.isDone)
            {
                yield return null;
            }
            JSONObject response = new JSONObject(www.text);
            for (int i = 0; i < response.list.Count; i++)
            {
                items[i] = new JSONItemData {template = defaultTemplate};
                response[i].GetField(ref items[i].text, "description");
            }
            result(items);
            m_WebLock = false;
            m_Loading = false;
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
                    StartCoroutine(GetBatch((m_BatchOffset + 3) * batchSize, batchSize, words =>
                    {
                        Array.Copy(data, batchSize, data, 0, batchSize * 2);
                        Array.Copy(words, 0, data, batchSize * 2, batchSize);
                        m_BatchOffset++;
                    }));
                } else if (currBatch != m_BatchOffset)
                { //Jumped multiple batches. Get a whole new dataset
                    if (!m_Loading)
                        m_Cleanup = data;
                    m_Loading = true;
                    StartCoroutine(GetBatch((currBatch - 1) * batchSize, batchSize * 3, words =>
                    {
                        data = words;
                        m_BatchOffset = currBatch - 1;
                    }));
                }
            } else if (m_BatchOffset > 0 && -m_DataOffset < (m_BatchOffset + 1) * batchSize)
            {
                if (currBatch == m_BatchOffset)
                { //Just one batch, fetch only the next one
                    StartCoroutine(GetBatch((m_BatchOffset - 1) * batchSize, batchSize, words =>
                    {
                        Array.Copy(data, 0, data, batchSize, batchSize * 2);
                        Array.Copy(words, 0, data, 0, batchSize);
                        m_BatchOffset--;
                    }));
                } else if (currBatch != m_BatchOffset)
                { //Jumped multiple batches. Get a whole new dataset
                    if (!m_Loading)
                        m_Cleanup = data;
                    m_Loading = true;
                    if (currBatch < 1)
                        currBatch = 1;
                    StartCoroutine(GetBatch((currBatch - 1) * batchSize, batchSize * 3, words =>
                    {
                        data = words;
                        m_BatchOffset = currBatch - 1;
                    }));
                }
            }
            if (m_Cleanup != null)
            {
                //Clean up all existing gameobjects
                foreach (var item in m_Cleanup)
                {
                    if (item == null)
                        continue;
                    if (item.item != null)
                    {
                        RecycleItem(item.template, item.item);
                        item.item = null;
                    }
                }
                m_Cleanup = null;
            }
        }

        protected override void UpdateItems()
        {
            if (data == null || data.Length == 0 || m_Loading)
            {
                return;
            }
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == null)
                    continue;
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
        }

        protected override void Positioning(Transform t, int offset)
        {
            t.position = m_LeftSide + (offset * m_ItemSize.y + scrollOffset) * Vector3.down;
        }
    }
}