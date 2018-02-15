using UnityEngine;

/// <summary>
/// Activate this script in the editor to record audio with the default device.
/// </summary>
[RequireComponent(typeof(AudioSource))]
class HeadsetMicrophone : MonoBehaviour
{
    public AudioSource audioSource;

    private void Start()
    {
        // null as device name gives us the default device
        audioSource.clip = Microphone.Start(null, true, 10, 44100);
        audioSource.loop = true;

        while (!(Microphone.GetPosition(null) > 0)) { }
        audioSource.Play();
    }
}
