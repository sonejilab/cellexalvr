using UnityEngine;

public class GraphInfoPanelRotator : MonoBehaviour
{

    private Transform CameraToLookAt;

    void Start()
    {
        CameraToLookAt = GameObject.Find("Camera (eye)").transform;
    }

    void Update()
    {
        transform.LookAt(CameraToLookAt);
        transform.Rotate(0f, -90f, 0f);
    }
}
