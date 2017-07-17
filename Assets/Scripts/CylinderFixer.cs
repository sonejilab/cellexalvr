using UnityEngine;

public class CylinderFixer : MonoBehaviour
{

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position = new Vector3(startPosition.x, transform.position.y, startPosition.z);
        transform.rotation = Quaternion.identity;
    }

}
