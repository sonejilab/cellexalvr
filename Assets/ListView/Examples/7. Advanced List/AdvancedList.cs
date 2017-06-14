using UnityEngine;
using System.Collections.Generic;

namespace ListView
{
    public class AdvancedList : ListViewController<AdvancedListItemData, AdvancedListItem>
    {
        public string dataFile;
        public GameObject[] models;

        readonly Dictionary<string, ModelPool> m_Models = new Dictionary<string, ModelPool>();
        readonly Dictionary<string, Vector3> m_TemplateSizes = new Dictionary<string, Vector3>();
        float m_ScrollReturn = float.MaxValue;
        float m_ItemHeight;

        protected override void Setup()
        {
            base.Setup();
            foreach (var kvp in m_Templates)
            {
                m_TemplateSizes[kvp.Key] = GetObjectSize(kvp.Value.prefab);
            }

            TextAsset text = Resources.Load<TextAsset>(dataFile);
            if (text)
            {
                JSONObject obj = new JSONObject(text.text);
                data = new AdvancedListItemData[obj.Count];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new AdvancedListItemData();
                    data[i].FromJSON(obj[i], this);
                }
            } else data = new AdvancedListItemData[0];

            if (models.Length < 1)
            {
                Debug.LogError("No models!");
            }
            foreach (var model in models)
            {
                if (m_Models.ContainsKey(model.name))
                    Debug.LogError("Two templates cannot have the same name");
                m_Models[model.name] = new ModelPool(model);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(itemSize.x, range, itemSize.z));
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
            m_LeftSide = transform.position + Vector3.up * range * 0.5f + Vector3.left * itemSize.x * 0.5f;

            m_DataOffset = (int) (scrollOffset / itemSize.y);
            if (scrollOffset < 0)
                m_DataOffset--;
        }

        protected override void UpdateItems()
        {
            float totalOffset = 0;
            UpdateRecursively(data, ref totalOffset);
            totalOffset -= m_ItemHeight;
            if (totalOffset < -scrollOffset)
            {
                m_ScrollReturn = -totalOffset;
            }
        }

        void UpdateRecursively(AdvancedListItemData[] data, ref float totalOffset)
        {
            foreach (var item in data)
            {
                m_ItemHeight = m_TemplateSizes[item.template].y;
                if (totalOffset + scrollOffset + m_ItemHeight < 0)
                {
                    ExtremeLeft(item);
                } else if (totalOffset + scrollOffset > range)
                {
                    ExtremeRight(item);
                } else
                {
                    ListMiddle(item, totalOffset + scrollOffset);
                }
                totalOffset += m_ItemHeight;
                if (item.children != null)
                {
                    if (item.expanded)
                    {
                        UpdateRecursively(item.children, ref totalOffset);
                    } else
                    {
                        RecycleChildren(item);
                    }
                }
            }
        }

        void ListMiddle(AdvancedListItemData data, float offset)
        {
            if (data.item == null)
            {
                data.item = GetItem(data);
            }
            Positioning(data.item.transform, offset);
        }

        void Positioning(Transform t, float offset)
        {
            t.position = m_LeftSide + offset * Vector3.down;
        }

        public void OnStopScrolling()
        {
            if (scrollOffset > 0)
            {
                scrollOffset = 0;
            }
            if (m_ScrollReturn < float.MaxValue)
            {
                scrollOffset = m_ScrollReturn;
                m_ScrollReturn = float.MaxValue;
            }
        }

        void RecycleChildren(AdvancedListItemData data)
        {
            foreach (var child in data.children)
            {
                RecycleItem(child.template, child.item);
                child.item = null;
                if (child.children != null)
                    RecycleChildren(child);
            }
        }

        protected override void RecycleItem(string template, MonoBehaviour item)
        {
            base.RecycleItem(template, item);
            try
            { //Try the cast. If it fails we just have a category
                AdvancedListItemChild aItem = (AdvancedListItemChild) item;
                if (!aItem) return;
                m_Models[aItem.data.model].pool.Add(aItem.model);
                aItem.model.transform.parent = null;
                aItem.model.SetActive(false);
            } catch
            {
            }
        }

        public GameObject GetModel(string name)
        {
            if (!m_Models.ContainsKey(name))
            {
                Debug.LogWarning("Cannot get model, " + name + " doesn't exist");
                return null;
            }
            GameObject model = null;
            if (m_Models[name].pool.Count > 0)
            {
                model = m_Models[name].pool[0];
                m_Models[name].pool.RemoveAt(0);

                model.gameObject.SetActive(true);
            } else
            {
                model = Instantiate(m_Models[name].prefab);
                model.transform.parent = transform;
            }
            return model;
        }

        class ModelPool
        {
            public readonly GameObject prefab;
            public readonly List<GameObject> pool = new List<GameObject>();

            public ModelPool(GameObject prefab)
            {
                if (prefab == null)
                    Debug.LogError("Template prefab cannot be null");
                this.prefab = prefab;
            }
        }
    }
}