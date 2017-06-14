using UnityEngine;

//Uses JSONObject http://u3d.as/1Rh

namespace ListView
{
    public class JSONList : ListViewController<JSONItemData, JSONItem>
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
                data = new JSONItemData[obj.Count];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new JSONItemData();
                    data[i].FromJSON(obj[i]);
                    data[i].template = defaultTemplate;
                }
            } else data = new JSONItemData[0];
        }
    }
}