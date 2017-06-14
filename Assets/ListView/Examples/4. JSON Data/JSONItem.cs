using UnityEngine;

namespace ListView
{
    public class JSONItem : ListViewItem<JSONItemData>
    {
        public TextMesh label;

        public override void Setup(JSONItemData data)
        {
            base.Setup(data);
            label.text = data.text;
        }
    }

    public class JSONItemData : CubeItemData
    {
        public void FromJSON(JSONObject obj)
        {
            obj.GetField(ref text, "text");
        }
    }
}