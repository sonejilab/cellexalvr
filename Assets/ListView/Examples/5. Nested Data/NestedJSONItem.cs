using UnityEngine;

namespace ListView
{
    public class NestedJSONItem : ListViewItem<NestedJSONItemData>
    {
        public TextMesh label;

        public override void Setup(NestedJSONItemData data)
        {
            base.Setup(data);
            label.text = data.text;
        }
    }

//[System.Serializable]     //Will cause warnings, but helpful for debugging
    public class NestedJSONItemData : ListViewItemNestedData<NestedJSONItemData>
    {
        public string text;

        public void FromJSON(JSONObject obj, string template)
        {
            obj.GetField(ref text, "text");
            this.template = template;
            obj.GetField("children", delegate(JSONObject _children)
            {
                children = new NestedJSONItemData[_children.Count];
                for (int i = 0; i < _children.Count; i++)
                {
                    children[i] = new NestedJSONItemData();
                    children[i].FromJSON(_children[i], template);
                }
            });
        }
    }
}
