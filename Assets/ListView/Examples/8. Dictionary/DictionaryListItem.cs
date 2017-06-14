using UnityEngine;

namespace ListView
{
    public class DictionaryListItem : ListViewItem<DictionaryListItemData>
    {
        public TextMesh word;
        public TextMesh definition;

        public override void Setup(DictionaryListItemData data)
        {
            base.Setup(data);
            word.text = data.word;
            definition.text = data.definition;
        }
    }

    [System.Serializable] //Will cause warnings, but helpful for debugging
    public class DictionaryListItemData : ListViewItemData
    {
        public string word, definition;
    }
}