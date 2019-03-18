using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CellexalVR.General
{
    /// <summary>
    /// Video player used for helpvideos. The helpvideos are placed throughout the scene often together with toolbuttons.
    /// </summary>
    public class PlayVideo : MonoBehaviour
    {

        //public MovieTexture movie;
        //public VideoClip videoClip;
        //public RawImage image;
        public GameObject videoCanv;
        public AudioSource audioSource;
        private VideoSource videoSource;
        private VideoPlayer videoPlayer;
        private string videoURL;
        private AudioClip audioClip;

        //public GameObject videoCanv;
        //public string buttonDescr;


        // Use this for initialization
        void Start()
        {
            //Application.runInBackground = true;
            videoPlayer = GetComponent<VideoPlayer>();
            Application.targetFrameRate = (int)GetComponent<UnityEngine.Video.VideoPlayer>().frameRate;
            //videoPlayer.clip = videoClip;
            //StartCoroutine(PlayVid());
            //videoPlayer.Pause();
        }

        public void StartVideo(string url, AudioClip audio)
        {
            //videoClip = clip;
            videoURL = url;
            print(videoURL);
            audioSource.clip = audio;
            if (!videoPlayer.isPlaying && videoPlayer.isPrepared)
            {
                videoPlayer.Play();
                audioSource.Play();
            }
            if (!videoPlayer.isPlaying)
            {
                //Application.runInBackground = true;
                StartCoroutine(PlayVid());
            }
            //videoPlayer.Pause();
        }
        public void PauseVideo()
        {
            videoPlayer.Pause();
            audioSource.Pause();
        }

        public void StopVideo()
        {
            print("STOP VIDEO");
            if (audioSource != null)
            {
                print("STOP AUDIO");
                audioSource.Stop();
            }
            videoPlayer.Stop();
        }

        IEnumerator PlayVid()
        {

            //Add VideoPlayer to the GameObject
            //videoPlayer = gameObject.AddComponent<VideoPlayer>();
            //Add AudioSource
            //audioSource = gameObject.AddComponent<AudioSource>();
            //if (!audioSource)
            //{
            //    audioSource = gameObject.AddComponent<AudioSource>();
            //}

            //Disable Play on Awake for both Video and Audio
            videoPlayer.playOnAwake = false;
            //audioSource.playOnAwake = false;
            //audioSource.Pause();
            //audioSource.clip = audioClip;


            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoURL;
            videoPlayer.Prepare();


            //Set Audio Output to AudioSource
            //videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            //Assign the Audio from Video to AudioSource to be played
            //videoPlayer.EnableAudioTrack(0, true);
            //videoPlayer.SetTargetAudioSource(0, audioSource);

            //Set video To Play then prepare Audio to prevent Buffering
            //videoPlayer.clip = videoClip;

            //Wait until video is prepared
            WaitForSeconds waitTime = new WaitForSeconds(1);
            while (!videoPlayer.isPrepared)
            {
                Debug.Log("Preparing Video");
                //Prepare/Wait for 5 sceonds only
                yield return waitTime;
                //Break out of the while loop after 5 seconds wait
                break;
            }

            Debug.Log("Done Preparing Video");

            //Assign the Texture from Video to RawImage to be displayed
            //image.texture = videoPlayer.texture;

            //Play Video
            videoPlayer.Play();
            audioSource.Play();
            //Play Sound
            //audioSource.Play();
            Debug.Log("Playing Video");
            while (videoPlayer.isPlaying)
            {
                Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
                if (!videoCanv.activeSelf)
                {
                    videoPlayer.Stop();
                    audioSource.Stop();
                }
                yield return null;
            }
            Debug.Log("Done Playing Video");
            videoCanv.SetActive(false);
        }
    }
}