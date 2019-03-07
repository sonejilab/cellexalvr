using UnityEngine;
using System.Collections;

public class AxesArrow : MonoBehaviour
{
    public GameObject xLine;
    public GameObject xHead;
    public GameObject yLine;
    public GameObject yHead;
    public GameObject zLine;
    public GameObject zHead;

    public void SetColors(string[] markerNames, Vector3 minVals, Vector3 maxVals)
    {
        if (markerNames.Length != 3)
        {
            CellexalLog.Log("ERROR: Must supply exactly 3 markers when creating facs graph.");
        }
        var xLineMaterial = xLine.GetComponent<Material>();
        var yLineMaterial = yLine.GetComponent<Material>();
        var zLineMaterial = zLine.GetComponent<Material>();
        var xHeadMaterial = xHead.GetComponent<Material>();
        var yHeadMaterial = yHead.GetComponent<Material>();
        var zHeadMaterial = zHead.GetComponent<Material>();
        Color lowColor = CellexalConfig.Config.GraphLowExpressionColor;
        Color midColor = CellexalConfig.Config.GraphMidExpressionColor;
        Color highColor = CellexalConfig.Config.GraphHighExpressionColor;

        xLineMaterial.SetColor("_Color1", lowColor);
        xLineMaterial.SetColor("_Color2", midColor);
        xLineMaterial.SetColor("_Color3", highColor);
        xLineMaterial.SetFloat("_MinVal", minVals.x);
        xLineMaterial.SetFloat("_MaxVal", maxVals.x);

        yLineMaterial.SetColor("_Color1", lowColor);
        yLineMaterial.SetColor("_Color2", midColor);
        yLineMaterial.SetColor("_Color3", highColor);
        yLineMaterial.SetFloat("_MinVal", minVals.y);
        yLineMaterial.SetFloat("_MaxVal", maxVals.y);

        zLineMaterial.SetColor("_Color1", lowColor);
        zLineMaterial.SetColor("_Color2", midColor);
        zLineMaterial.SetColor("_Color3", highColor);
        zLineMaterial.SetFloat("_MinVal", minVals.z);
        zLineMaterial.SetFloat("_MaxVal", maxVals.z);

        if (maxVals.x > 0.5)
        {
            xHeadMaterial.color = Color.Lerp(midColor, highColor, maxVals.x);
        }
        else
        {
            xHeadMaterial.color = Color.Lerp(lowColor, midColor, maxVals.x);
        }

        if (maxVals.y > 0.5)
        {
            yHeadMaterial.color = Color.Lerp(midColor, highColor, maxVals.y);
        }
        else
        {
            yHeadMaterial.color = Color.Lerp(lowColor, midColor, maxVals.y);
        }

        if (maxVals.z > 0.5)
        {
            zHeadMaterial.color = Color.Lerp(midColor, highColor, maxVals.z);
        }
        else
        {
            zHeadMaterial.color = Color.Lerp(lowColor, midColor, maxVals.z);
        }

    }
}
