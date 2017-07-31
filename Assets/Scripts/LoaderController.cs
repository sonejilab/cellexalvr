using UnityEngine;
using System.Collections;
/// <summary>
/// This class represent the loader.
/// </summary>
public class LoaderController : MonoBehaviour
{

    public InputReader inputReader;
    public InputFolderGenerator inputFolderGenerator;
    public GraphManager GraphManager;
    public Transform cylinder;
    private float timeEntered = 0;
    private ArrayList cellsToDestroy;
    private bool cellsEntered = false;
    private bool collidersDestroyed = false;
    private Vector3 startPosition;
    private Vector3 finalPosition;
    private Vector3 startScale;
    private Vector3 finalScale;
    private bool moving = false;
    private float currentTime;
    private float arrivalTime;
    [HideInInspector]
    public bool loaderMovedDown = false;

    void Start()
    {
        cellsToDestroy = new ArrayList();
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
                //sound.Stop();
            }
        }

        if (timeEntered + 2 < Time.time && cellsEntered && !collidersDestroyed)
        {
            DestroyFolderColliders();
        }

        if (timeEntered + 5 < Time.time && collidersDestroyed)
        {
            inputFolderGenerator.DestroyFolders();
            DestroyCells();
        }
    }

    public void ResetLoaderBooleans()
    {
        inputFolderGenerator.DestroyFolders();
        cellsEntered = false;
        timeEntered = 0;
        collidersDestroyed = false;
    }

    public void MoveLoader(Vector3 direction, float time)
    {
        //sound.Play();
        currentTime = 0;
        arrivalTime = time;
        startPosition = transform.position;
        startScale = cylinder.localScale;
        if (direction.y > 0)
        {
            finalScale = new Vector3(1f, startScale.y, 1f);
        }
        else
        {
            finalScale = new Vector3(3f, startScale.y, 3f);
        }
        if (moving)
        {
            finalPosition = finalPosition + direction;
        }
        else
        {
            finalPosition = transform.position + direction;
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
                    // assuming the computer clock isn't currently unix epoch
                    timeEntered = Time.time;
                    cellsEntered = true;
                }
                if (!cellParent.GetComponent<CellsToLoad>().GraphsLoaded())
                {
                    inputReader.ReadFolder(cellParent.GetComponent<CellsToLoad>().GetDirectory());
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
                child.gameObject.GetComponent<CellFolder>().PlaySound();
            }
        }
        collidersDestroyed = true;
    }

    void DestroyCells()
    {
        // since we are responisble for removing the parent reference we should probably
        // destroy the objects as well
        foreach (Transform child in cellsToDestroy)
        {
            Destroy(child.gameObject);
        }
        cellsToDestroy.Clear();
        ResetLoaderBooleans();
    }

}
