using UnityEngine;
using System.Collections;
using System.Threading;
using System;
using System.Net.Mail;
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
        get { return "Save Session/Create Report"; }
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
        SetButtonActivated(false);
        StartCoroutine(LogStop());
        //elapsedTime = 0.0f;


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
        //SendIt();

        //string startPath = @"c:\example\start";
        //string zipPath = @"c:\example\result.zip";

        //ZipFile.CreateFromDirectory(startPath, zipPath);
        changeSprite = false;
        SetButtonActivated(true);
    }

//    public static void SendIt()
//    {
//        MailMessage mail = new MailMessage();
//        mail.From = new MailAddress("cellexalvr@gmail.com");
//        mail.To.Add("cellexalvr@gmail.com");
//        mail.Subject = "Test Mail - 1";
//        mail.Body = "mail with attachment";

//        System.Net.Mail.Attachment attachment;
//        attachment = new System.Net.Mail.Attachment("your attachment file");
//        mail.Attachments.Add(attachment);

//#pragma warning disable CS0618 // Type or member is obsolete
//        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com")
//#pragma warning restore CS0618 // Type or member is obsolete
//        {
//            Port = 587,
//            EnableSsl = true
//        };

//        SmtpServer.Send(mail);
//    }
}