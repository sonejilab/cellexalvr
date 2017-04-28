using System.IO;
using UnityEngine;

public class RTest : MonoBehaviour {

	// Demonstration of running a simple R script
	void Start () {
        string home = Directory.GetCurrentDirectory();
        string rPath;
        using (StreamReader r = new StreamReader(home + "/Assets/Config/config.txt"))
        {
            string input = r.ReadToEnd();
            rPath = input;
            Debug.Log(rPath);
            Debug.Log(RScriptRunner.RunFromCmd(home + "/Assets/Scripts/test.R", rPath, ""));
        }
    }
}

