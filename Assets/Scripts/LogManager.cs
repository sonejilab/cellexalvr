using System.IO;
using UnityEngine;

class LogManager : MonoBehaviour
{
    private string LogFilePath;

    public void Start()
    {
        LogFilePath = Directory.GetCurrentDirectory() + "/Assets/Outpout/log.txt";
    }

    public void Log(string message)
    {

    }

}
