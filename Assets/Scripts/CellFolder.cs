using UnityEngine;

public class CellFolder : MonoBehaviour
{

    //public Transform Rope { get; set; }
    //public float YOffset { get; set; }
    public AudioSource sound;

    public void PlaySound()
    {
        sound.Play();
    }

    void Update()
    {
        //transform.position = new Vector3(transform.position.x, Rope.position.y + YOffset, transform.position.z);
    }

}
