using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorMessageController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public void Activate()
    {
        gameObject.SetActive(true);
        StartCoroutine(SetActivate());

    }

    IEnumerator SetActivate()
    {
        yield return new WaitForSeconds(3);

        gameObject.SetActive(false);
    }

}
