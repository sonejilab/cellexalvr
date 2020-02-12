using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProjectionObjectScript : MonoBehaviour
{
    public enum projectionType
    {
        p3D,
        p2D_sep,
        p3D_sep,
        p2D
    }

    public projectionType type;
    public string name = "test";
    public Dictionary<string, string> paths;
    public LineScript coordsLine;
    public LineScript velocityLine;
    public GameObject AnchorPrefab;

    public Dictionary<projectionType, string[]> menu_setup;
    private TextMeshProUGUI nameTextMesh;

    void Start()
    {
        paths = new Dictionary<string, string>();
        nameTextMesh = transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
    }


    public void changeName(string name)
    {
        print(name);
        this.name = name;
        nameTextMesh.text = name;
    }

    public void init(projectionType type)
    {
        this.type = type;
        menu_setup = new Dictionary<projectionType, string[]>
        {
            { projectionType.p3D, new string[] { "X", "vel" } },
            { projectionType.p2D_sep, new string[] { "X", "Y", "velX", "velY" } }
        };

        int offset = 0;
        GameObject go;

        foreach(string anchor in menu_setup[type])
        {
            go = Instantiate(AnchorPrefab, gameObject.transform, false);
            //go.transform.localPosition = new Vector3(0, -10 * offset, 0);
            go.transform.localPosition += Vector3.up * -10 * offset;
            go.GetComponentInChildren<LineScript>().type = anchor;
            offset++;
            go.GetComponent<TextMeshProUGUI>().text = anchor;

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
