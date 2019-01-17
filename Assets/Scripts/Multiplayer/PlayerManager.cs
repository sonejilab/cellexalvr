#if UNITY_5 && (!UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && ! UNITY_5_3) || UNITY_2017 || UNITY_2018
#define UNITY_MIN_5_4
#endif

using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections;
using System;
using TMPro;

/// <summary>
/// Player manager. 
/// Handles fire Input and Beams.
/// </summary>
public class PlayerManager : Photon.PunBehaviour, IPunObservable
{

    #region Public Variables
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    public GameObject username;
    #endregion

    #region Private Variables

    //True, when the user is firing
    bool IsFiring;

    #endregion

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.isMine)
        {
            PhotonNetwork.player.NickName = PhotonNetwork.playerName;
            PlayerManager.LocalPlayerInstance = this.gameObject;
            Renderer[] meshList = transform.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in meshList)
            {
                r.enabled = false;
            }
            Collider[] colList = transform.GetComponentsInChildren<Collider>();
            foreach (Collider c in colList)
            {
                c.enabled = false;
            }
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);


    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        username.GetComponent<TextMeshPro>().text = GetComponent<PhotonView>().owner.NickName;



#if UNITY_MIN_5_4
        // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        };
#endif
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity on every frame.
    /// </summary>
    void Update()
    {
        if (photonView.isMine)
        {
            ProcessInputs();
        }

    }

    /// <summary>
    /// MonoBehaviour method called when the Collider 'other' enters the trigger.
    /// Affect Health of the Player if the collider is a beam
    /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
    /// One could move the collider further away to prevent this or check if the beam belongs to the player.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {


        if (!photonView.isMine)
        {
            return;
        }


        // We are only interested in Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.

    }


    /// <summary>
    /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
    /// We're going to affect health while the beams are touching the player
    /// </summary>
    /// <param name="other">Other.</param>
    void OnTriggerStay(Collider other)
    {


        // we dont' do anything if we are not the local player.
        if (!photonView.isMine)
        {
            return;
        }





        // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.

    }

    #endregion

    #region Custom

    /// <summary>
    /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
    /// </summary>
    void ProcessInputs()
    {

        if (Input.GetButtonDown("Fire1"))
        {

        }

        if (Input.GetButtonUp("Fire1"))
        {

        }
    }
#if !UNITY_MIN_5_4
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
           this.CalledOnLevelWasLoaded(level);
        }
#endif

    void CalledOnLevelWasLoaded(int level)
    {
        // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
        if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        {
            transform.position = new Vector3(0f, 5f, 0f);
        }
    }

    #endregion


    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
        }
        else
        {
            // Network player, receive data
        }
        if (stream.isWriting)
        {
            // We own this player: send the others our data
        }
        else
        {
            // Network player, receive dataz
        }
    }




}

