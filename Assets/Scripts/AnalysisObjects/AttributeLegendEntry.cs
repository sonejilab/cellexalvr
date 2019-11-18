using UnityEngine;
using System.Collections;
using TMPro;

public class AttributeLegendEntry : MonoBehaviour
{
    public GameObject colorSquare;
    public TextMeshPro attributeNameText;
    public TextMeshPro numberOfCellsText;
    public TextMeshPro percentOfActiveText;
    public TextMeshPro percentOfAllText;

    public int numberOfCells;

    public void SetPanelText(string attributeName, int numberOfCells, string percentOfActive, string percentOfAll, Color color)
    {
        attributeNameText.text = attributeName;
        numberOfCellsText.text = numberOfCells.ToString();
        percentOfActiveText.text = percentOfActive;
        percentOfAllText.text = percentOfAll;
        colorSquare.GetComponent<MeshRenderer>().material.color = color;
        this.numberOfCells = numberOfCells;
    }

    public void UpdatePercentages(string percentOfActive, string percentOfAll)
    {
        percentOfActiveText.text = percentOfActive;
        percentOfAllText.text = percentOfAll;
    }

}
