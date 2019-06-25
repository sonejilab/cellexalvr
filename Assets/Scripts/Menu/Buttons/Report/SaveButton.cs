using UnityEngine;
using System.Collections;
using System.Threading;
using CellexalVR.General;
using CellexalVR.AnalysisLogic;
using CellexalVR.Extensions;
using System.IO;

namespace CellexalVR.Menu.Buttons.Report
{
    /// <summary>
    /// Represents the button that compiles a report and includes statistical analaysis and objects the user has saved 
    /// during the session.
    /// </summary>
    public class SaveButton : CellexalButton
    {

        //public SaveScene saveScene;
        public Sprite gray;
        public Sprite original;
        public ReportListGenerator reportList;
        //private float elapsedTime;
        private float time = 1.0f;
        private bool changeSprite;

        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();
            CellexalEvents.ScriptRunning.AddListener(TurnOff);
            CellexalEvents.ScriptFinished.AddListener(TurnOn);
        }

        protected override string Description
        {
            get { return "Compile Session Report"; }
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
        public override void Click()
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
            descriptionText.text = "Compiling report..";
            string args = CellexalUser.UserSpecificFolder.UnFixFilePath();
            string rScriptFilePath = Application.streamingAssetsPath + @"\R\logStop.R";

            while (referenceManager.selectionManager.RObjectUpdating || File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R"))
            {
                yield return null;
            }

            Debug.Log("Running R script " + CellexalLog.FixFilePath(rScriptFilePath) + " with the arguments \"" + args + "\"");
            CellexalLog.Log("Running R script " + CellexalLog.FixFilePath(rScriptFilePath) + " with the arguments \"" + args + "\"");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();

            while (t.IsAlive || File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R"))
            {
                yield return null;
            }

            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
            

            changeSprite = false;
            descriptionText.text = "";
            SetButtonActivated(true);
            referenceManager.notificationManager.SpawnNotification("Session report compiled.");
            //ZipFile.CreateFromDirectory(startPath, zipPath);
            //reportList.GenerateList();
            
            //SendIt();
            //string startPath = @"c:\example\start";
            //string zipPath = @"c:\example\result.zip";
        }

        void TurnOff()
        {
            SetButtonActivated(false);
        }

        void TurnOn()
        {
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
}