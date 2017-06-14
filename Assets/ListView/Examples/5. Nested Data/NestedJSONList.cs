using UnityEngine;

namespace ListView
{
    public class NestedJSONList : ListViewController<NestedJSONItemData, NestedJSONItem>
    {
        public string dataFile;
        public string defaultTemplate;

        protected override void Setup()
        {
            base.Setup();
            TextAsset text = Resources.Load<TextAsset>(dataFile);
            if (text)
            {
                JSONObject obj = new JSONObject(text.text);
                data = new NestedJSONItemData[obj.Count];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new NestedJSONItemData();
                    data[i].FromJSON(obj[i], defaultTemplate);
                }
            } else data = new NestedJSONItemData[0];
        }

        protected override void UpdateItems()
        {
            int count = 0;
            UpdateRecursively(data, ref count);
        }

        void UpdateRecursively(NestedJSONItemData[] data, ref int count)
        {
            foreach (var item in data)
            {
                if (count + m_DataOffset < 0)
                {
                    ExtremeLeft(item);
                } else if (count + m_DataOffset > m_NumItems)
                {
                    ExtremeRight(item);
                } else
                {
                    ListMiddle(item, count + m_DataOffset);
                }
                count++;
                if (item.children != null)
                {
                    if (item.expanded)
                    {
                        UpdateRecursively(item.children, ref count);
                    } else
                    {
                        RecycleChildren(item);
                    }
                }
            }
        }

        void RecycleChildren(NestedJSONItemData data)
        {
            foreach (var child in data.children)
            {
                RecycleItem(child.template, child.item);
                child.item = null;
                if (child.children != null)
                    RecycleChildren(child);
            }
        }
    }
}
