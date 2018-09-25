using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
[RequireComponent(typeof(AudioSource))]

public class VideoButton : CellexalButton
{

    //public MovieTexture movie;
    //public VideoClip videoClip;
    //public RawImage image;

    //public VideoPlayer videoPlayer;
    public GameObject videoCanv;
    public VideoClip clip;
    public string buttonDescr;
    public GameObject videoManager;
    

    protected override string Description
    {
        get { return buttonDescr; }
    }

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
        //videoPlayer.clip = videoClip;
        //StartCoroutine(PlayVid());
        //videoPlayer.Pause();
    }


    public void StartVideo()
    {
        videoCanv.SetActive(true);
        videoManager.GetComponent<PlayVideo>().StartVideo(clip);
        infoMenu.SetActive(false);
        Exit();
    }

    public void StopVideo()
    {
        videoCanv.SetActive(false);
        videoManager.GetComponent<PlayVideo>().StopVideo();
        Exit();
    }


    protected override void Click()
    {
        if (buttonDescr.Equals("Close Video"))
        {
            videoCanv.SetActive(false);
            videoManager.GetComponent<PlayVideo>().StopVideo();
            Exit();
        }
        if (buttonDescr.Equals("Play Video"))
        {
            videoCanv.SetActive(true);
            videoManager.GetComponent<PlayVideo>().StartVideo(clip);
            infoMenu.SetActive(false);
            Exit();
        }
    }
}
