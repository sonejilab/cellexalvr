using UnityEngine;
using System.Collections;

public class OpenURLButton : MonoBehaviour
{
    public string url;

    public void Click()
    {
        Application.OpenURL(url);
    }
}
