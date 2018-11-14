using CurvedVRKeyboard;
using UnityEngine;

public class KeyboardOutput : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public KeyboardItem spaceBar;
    public TextMesh textMesh;
    public GameObject description;
    private string text;

    private CellManager cellManager;
    private Filter.FilterRule targetFilterRule;
    private TextMesh textMeshToUpdate;
    private GameManager gameManager;
    private OutputType nextOutputType;
    public enum OutputType { COLOR_GRAPHS_BY_GENE, FILTER_VALUE, FILTER_ITEM_NAME }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        textMesh = GetComponent<TextMesh>();
        gameManager = referenceManager.gameManager;
    }

    public void SetNextTarget(OutputType nextOutputType, Filter.FilterRule filterRule = null, TextMesh textMeshToUpdate = null)
    {
        this.nextOutputType = nextOutputType;
        this.textMeshToUpdate = textMeshToUpdate;
        switch (nextOutputType)
        {
            case OutputType.FILTER_VALUE:
                targetFilterRule = filterRule;
                break;
            case OutputType.FILTER_ITEM_NAME:
                targetFilterRule = filterRule;
                break;
        }
    }

    public virtual void SendToTarget()
    {
        switch (nextOutputType)
        {
            case OutputType.COLOR_GRAPHS_BY_GENE:
                cellManager.ColorGraphsByGene(text);
                referenceManager.gameManager.InformColorGraphsByGene(text);
                break;
            case OutputType.FILTER_VALUE:
                try
                {
                    var value = float.Parse(text);
                    targetFilterRule.value = value;
                    textMeshToUpdate.text = text;
                }
                catch (System.FormatException e)
                {
                    CellexalLog.Log("WARNING: Could not parse " + text + " as a float when creating a filter rule");
                }
                break;
            case OutputType.FILTER_ITEM_NAME:
                targetFilterRule.item = text;
                textMeshToUpdate.text = text;
                break;

        }
        nextOutputType = OutputType.COLOR_GRAPHS_BY_GENE;
        targetFilterRule = null;
    }

    public void AddLetter(char c)
    {
        print("Add letter to keyboard - " + this.transform.parent.gameObject.name);
        text += c;
        textMesh.text += c;
        if (description && description.activeSelf)
        {
            description.SetActive(false);
        }
    }

    public void RemoveLetter()
    {
        text = text.Remove(text.Length - 1, 1);
        textMesh.text = text;
        if (textMesh.text.Equals(string.Empty))
        {
            description.SetActive(true);
        }
    }

    public void SetText(string text)
    {
        this.text = text;
        textMesh.text = text;
    }

    public void Clear()
    {
        text = "";
        textMesh.text = "";
        description.SetActive(true);
    }
}

