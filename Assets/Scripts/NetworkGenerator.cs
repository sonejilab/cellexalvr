using System.Collections;
using System.Threading;
using UnityEngine;

/// <summary>
/// This class starts the thread that generates the network files and then tells the inputreader to process them.
/// </summary>
public class NetworkGenerator : MonoBehaviour
{

    public SelectionToolHandler selectionToolHandler;
    public InputReader inputReader;
    public ToggleArcsSubMenu subMenu;
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
        // generate the files containing the network information
        t = new Thread(new ThreadStart(gnt.GenerateNetworks));
        t.Start();

        while (t.IsAlive)
            yield return null;

        inputReader.ReadNetworkFiles();
    }
}