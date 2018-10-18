using UnityEngine;
using BayatGames.SaveGameFree.Examples;
using System.Collections;
using System.Threading;

/// <summary>
/// Represents the button that saves the current scene.
/// </summary>
public class SaveButton : CellexalButton
{

    //public SaveScene saveScene;
    public Sprite gray;
    public Sprite original;
    //private float elapsedTime;
    private float time = 1.0f;
    private bool changeSprite;

    // Use this for initialization
    protected override string Description
    {
        get { return "Save Session"; }
    }

    protected override void Update()
    {
        base.Update();
        //if (changeSprite)
        //{
        //    if (elapsedTime < time)
        //    {
        //        elapsedTime += Time.deltaTime;
        //    }
        //    else
        //    {
        //        standardTexture = original;
        //        changeSprite = false;
        //    }
        //}
    }

    // Update is called once per frame
    protected override void Click()
    {
        Debug.Log("Do Save");
        //saveScene.Save();
        StartCoroutine(LogStop());
        //elapsedTime = 0.0f;
        SetButtonActivated(false);

    }

    /// <summary>
    /// Calls R logging function to stop the logging session.
    /// </summary>
    IEnumerator LogStop()
    {
        string args = CellexalUser.UserSpecificFolder;
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\logStop.R";
        Debug.Log("Running R script " + CellexalLog.FixFilePath(rScriptFilePath) + " with the arguments \"" + args + "\"");
        CellexalLog.Log("Running R script " + CellexalLog.FixFilePath(rScriptFilePath) + " with the arguments \"" + args + "\"");
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        t.Start();

        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
        changeSprite = false;
        SetButtonActivated(true);
        /*
        void ChangeSprite() 
        {
            spriteRenderer.sprite = gray;
            float elapsedTime = 0.0f;
            if (elapsedTime > time) 
            {
                spriteRenderer.sprite = original;
            } else {
                elapsedTime += Time.deltaTime;
            }
        }*/

    }
}