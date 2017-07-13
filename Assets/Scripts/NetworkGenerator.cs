using System.Collections;
using System.Threading;
using UnityEngine;

public class NetworkGenerator : MonoBehaviour
{

    public SelectionToolHandler selectionToolHandler;
    public InputReader inputReader;
    private Thread t;
    private GenerateNetworksThread gnt;

    private void Start()
    {
        gnt = new GenerateNetworksThread(selectionToolHandler);
    }

    public void GenerateNetworks()
    {
        StartCoroutine(GenerateNetworksCoroutine());
    }

    IEnumerator GenerateNetworksCoroutine()
    {

        t = new Thread(new ThreadStart(gnt.GenerateNetworks));
        t.Start();

        while (t.IsAlive)
            yield return null;

        inputReader.ReadNetworkFiles();
    }
}