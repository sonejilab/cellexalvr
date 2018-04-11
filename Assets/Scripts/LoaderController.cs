using UnityEngine;
using System.Collections;
using BayatGames.SaveGameFree.Examples;
using System.IO;
using System.Threading;

/// <summary>
/// This class represent the loader. The loader reacts to cells representing dtasets that fall into it and starts loading the dataset.
/// </summary>
public class LoaderController : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public Transform cylinder;

    private InputReader inputReader;
    private InputFolderGenerator inputFolderGenerator;
    private GraphManager graphManager;
    private GameObject helperCylinder;
    private float timeEntered = 0;
    private ArrayList cellsToDestroy;
    private bool cellsEntered = false;
    private bool collidersDestroyed = false;
    private Vector3 startPosition;
    private Vector3 finalPosition;
    private Vector3 startScale;
    private Vector3 finalScale;
    private bool moving = false;
    [HideInInspector]
    public bool loadingComplete = false;
    private float currentTime;
    private float arrivalTime;
    [HideInInspector]
    public bool loaderMovedDown = false;
    public SaveScene savescene;
    private GameManager gameManager;

    void Start()
    {
        gameManager = referenceManager.gameManager;
        cellsToDestroy = new ArrayList();
        inputReader = referenceManager.inputReader;
        inputFolderGenerator = referenceManager.inputFolderGenerator;
        graphManager = referenceManager.graphManager;
        helperCylinder = referenceManager.helperCylinder;
    }

    void Update()
    {
        if (moving)
        {
            gameObject.transform.position = Vector3.Lerp(startPosition, finalPosition, currentTime / arrivalTime);
            cylinder.transform.localScale = Vector3.Lerp(startScale, finalScale, currentTime / arrivalTime);
            currentTime += Time.deltaTime;
            if (currentTime > arrivalTime)
            {
                moving = false;
                loadingComplete = true;
                //Debug.Log("Loading Complete");
                //sound.Stop();
            }
        }

        if (timeEntered + 2 < Time.time && cellsEntered && !collidersDestroyed)
        {
            helperCylinder.SetActive(false);
            DestroyFolderColliders();
        }

        if (timeEntered + 5 < Time.time && collidersDestroyed)
        {
            inputFolderGenerator.DestroyFolders();
            DestroyCells();
        }
    }

    /// <summary>
    /// Resets some important variables used by the loader.
    /// </summary>
    public void ResetLoaderBooleans()
    {
        inputFolderGenerator.DestroyFolders();
        cellsEntered = false;
        timeEntered = 0;
        collidersDestroyed = false;
    }

    /// <summary>
    /// Moves the loader.
    /// </summary>
    /// <param name="distance"> The distance in world space to move the loader. </param>
    /// <param name="time"> The total time in seconds to move the loader. </param>
    public void MoveLoader(Vector3 distance, float time)
    {
        //sound.Play();
        currentTime = 0;
        arrivalTime = time;
        startPosition = transform.position;
        startScale = cylinder.localScale;
        if (distance.y > 0)
        {
            finalScale = new Vector3(1f, startScale.y, 1f);
            helperCylinder.SetActive(true);
        }
        else
        {
            finalScale = new Vector3(3f, startScale.y, 3f);
        }
        if (moving)
        {
            finalPosition = finalPosition + distance;
        }
        else
        {
            finalPosition = transform.position + distance;
        }
        moving = true;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Sphere"))
        {
            Transform cellParent = collider.transform.parent;
            if (cellParent != null)
            {
                if (timeEntered == 0)
                {
                    timeEntered = Time.time;
                    cellsEntered = true;
                }
                if (!cellParent.GetComponent<CellsToLoad>().GraphsLoaded())
                {
                    string path = cellParent.GetComponent<CellsToLoad>().Directory;
                    graphManager.directory = path;
                    inputReader.ReadFolder(path);
                    gameManager.InformReadFolder(path);
                    StartCoroutine(InitialCheckCoroutine());
                }

                Destroy(cellParent.GetComponent<FixedJoint>());
                Destroy(cellParent.GetComponent<Rigidbody>());
                foreach (Transform child in cellParent)
                {
                    // if (child.gameObject.GetComponent<Rigidbody>() == null)
                    child.gameObject.AddComponent<Rigidbody>();
                    cellsToDestroy.Add(child);
                }
                // must pass over list again to remove the parents. doing so in the
                // above loop messes with the iterator somehow and only removes every
                // second child's parent reference
                foreach (Transform child in cellsToDestroy)
                {
                    child.parent = null;
                }
            }
        }
    }

    private IEnumerator InitialCheckCoroutine()
    {
        string dataSourceFolder = Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder;
        string userFolder = CellexalUser.UserSpecificFolder;
        string args = dataSourceFolder + " " + userFolder;
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\initial_check.R";
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        CellexalLog.Log("Updating R Object finished in " + stopwatch.Elapsed.ToString());
        inputReader.LoadPreviousGroupings();
    }

    void DestroyFolderColliders()
    {
        // foreach (Collider c in GetComponentsInChildren<Collider>()) {
        //  Destroy(c);
        // }

        foreach (Transform child in cellsToDestroy)
        {
            if (child.GetComponent<Collider>() != null)
                Destroy(child.gameObject.GetComponent<Collider>());
        }

        foreach (Collider c in inputFolderGenerator.GetComponentsInChildren<Collider>())
        {
            Destroy(c);
        }

        foreach (Transform child in inputFolderGenerator.transform)
        {
            if (child.CompareTag("Folder"))
            {
                child.gameObject.AddComponent<Rigidbody>();
            }
        }
        collidersDestroyed = true;
    }

    void DestroyCells()
    {
        // since we are responsible for removing the parent reference we should probably
        // destroy the objects as well
        foreach (Transform child in cellsToDestroy)
        {
            Destroy(child.gameObject);
        }
        cellsToDestroy.Clear();
        ResetLoaderBooleans();
    }

    internal void DestroyFolders()
    {
        inputFolderGenerator.DestroyFolders();
    }
}
