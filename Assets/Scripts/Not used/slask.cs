/*
* This is just a document for code that is not used for one reason or another
*/

#region billboard system
/*
 * If billboards worked like the unity documentation implies, this could maybe be used to put graphpoints as billboards instead of meshes
 * Unfortunately, it seems billboards only works with speedtree
public IEnumerator TestBillboard()
{
    string dir = Directory.GetCurrentDirectory() + @"\Data\Bertie";
    string file = dir + @"\DDRTree.mds";

    FileStream fileStream = new FileStream(file, FileMode.Open);
    StreamReader streamReader = new StreamReader(fileStream);

    var graph = graphManager.CreateGraph();

    List<string> cellnames = new List<string>();
    List<float> xcoords = new List<float>();
    List<float> ycoords = new List<float>();
    List<float> zcoords = new List<float>();

    //while (!streamReader.EndOfStream)
    for (int i = 0; i < 10; ++i)
    {
        string[] words = streamReader.ReadLine().Split(null);
        if (words.Length != 4)
        {
            continue;
        }
        cellnames.Add(words[0]);
        xcoords.Add(float.Parse(words[1]));
        ycoords.Add(float.Parse(words[2]));
        zcoords.Add(float.Parse(words[3]));
    }
    // we must wait for the graph to fully initialize before adding stuff to it
    while (!graph.Ready())
        yield return null;
    UpdateMinMax(graph, xcoords, ycoords, zcoords);
    BillboardAsset bAsset = new BillboardAsset()
    {
        height = 1,
        width = 1,
        bottom = 0
    };
    bAsset.SetVertices(new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) });
    bAsset.SetIndices(new ushort[] { 0, 1, 2, 1, 2, 3 });
    bAsset.SetImageTexCoords(new Vector4[] { new Vector4(0, 0, 1, 1) });
    print("" + bAsset.vertexCount + " " + bAsset.indexCount + " " + bAsset.imageCount);
    bAsset.material = billboardMaterial;
    for (int i = 0; i < xcoords.Count; ++i)
    {
        try
        {
            var newItem = Instantiate(billboardPrefab);
            BillboardRenderer bRenderer = newItem.GetComponent<BillboardRenderer>();
            bRenderer.billboard = bAsset;
            //bRenderer.material = graphPointMaterial;
            Vector3 scaledCoords = graph.ScaleCoordinates(xcoords[i], ycoords[i], zcoords[i]);
            newItem.transform.parent = graph.transform;
            newItem.transform.localPosition = scaledCoords;
        }
        catch (Exception e)
        {
            print(e);
            break;
        }
        streamReader.Close();
        fileStream.Close();
    }
}*/
#endregion

