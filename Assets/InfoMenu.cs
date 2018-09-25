using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoMenu : MonoBehaviour {
    public bool active;


    private int frameCount = 0;
    private string laserColliderName = "[RightController]BasePointerRenderer_ObjectInteractor_Collider";
    //private int layerMask;

    // Use this for initialization
    void Start () {
        active = false;
        //layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
    }

    // Update is called once per frame
    //void Update()
    //{
    //    frameCount++;
    //    // Button sometimes stays active even though ontriggerexit should have been called.
    //    // To deactivate button again check every 10th frame if laser pointer collider is colliding.
    //    if (frameCount % 30 == 0)
    //    {
    //        bool inside = false;
    //        Collider[] collidesWith = Physics.OverlapBox(transform.position, transform.localScale, Quaternion.identity, layerMask);

    //        foreach (Collider col in collidesWith)
    //        {
    //            if (col.gameObject.name == laserColliderName)
    //            {
    //                inside = true;
    //                return;
    //            }
    //        }

    //        SetA(inside);
    //        gameObject.SetActive(inside);
    //    }
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    active = true;
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    active = false;
    //    gameObject.SetActive(false);

    //}

    //public void SetA(bool b)
    //{
    //    active = b;
    //    gameObject.SetActive(b);
    //}
}
