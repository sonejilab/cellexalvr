﻿using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CellexalVR.Menu.Buttons.General
{
    /// <summary>
    /// Starts the help video that is assigned as the url. 
    /// </summary>
    public class VideoButton : CellexalButton
    {
        public GameObject videoCanv;
        public AudioClip audioClip;
        public string url;
        public string buttonDescr;
        public GameObject videoManager;


        protected override string Description
        {
            get { return buttonDescr; }
        }
        
        void Start()
        {
            Application.runInBackground = true;
        }

        public override void SetHighlighted(bool highlight)
        {
            // do nothing
        }

        public void StartVideo()
        {
            videoCanv.SetActive(true);
            videoManager.GetComponent<PlayVideo>().StartVideo(url, audioClip);
            infoMenu.SetActive(false);
            Exit();
        }

        public void StopVideo()
        {
            videoCanv.SetActive(false);
            Exit();
        }


        public override void Click()
        {
            if (buttonDescr.Equals("Close Video"))
            {
                StopVideo();
                print("Stop clicked");
                Exit();
            }
            if (buttonDescr.Equals("Play Video"))
            {
                StartVideo();
                infoMenu.SetActive(false);
                Exit();
            }
        }
    }
}