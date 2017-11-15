using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR;

/// <summary>
/// This static class represents a log that can be written to.
/// </summary>
public static class CellExAlLog
{
    private static string logDirectory;
    private static string logFilePath = "";
    private static List<string> logThisLater = new List<string>();

    public static void InitNewLog()
    {
        // File names can't have colons so we only use hyphens
        var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        logDirectory = Directory.GetCurrentDirectory() + "\\Output\\" + CellExAlUser.Username;
        if (!Directory.Exists(logDirectory))
        {
            logThisLater.Add("\tCreated directory " + FixFilePath(logDirectory));
            Directory.CreateDirectory(logDirectory);
        }

        logFilePath = logDirectory + "\\cellexal-log-" + time + ".txt";
        // this will most likely always happen
        if (!File.Exists(logFilePath))
        {
            logThisLater.Add("\tCreated file " + logFilePath);
            File.Create(logFilePath).Dispose();
        }


        string nicerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Log("Welcome to CellExAl " + Application.version,
            "Running on Unity " + Application.unityVersion,
            "BuildGUID: " + Application.buildGUID,
            "Logfile created at " + nicerTime);

        Log("\nSome system information:",
            "\tOS: " + SystemInfo.operatingSystem,
            "\tCPU: " + SystemInfo.processorType,
            "\tProcessor count: " + SystemInfo.processorCount,
            "\tGPU: " + SystemInfo.graphicsDeviceName,
            "\tRAM size: " + SystemInfo.systemMemorySize);

        if (logThisLater.Count > 0)
        {
            Log("The following was generated before the log file existed:");
            Log(logThisLater.ToArray());
            Log("\tEnd of what was generated before the log file existed.");
            logThisLater.Clear();
        }
    }

    /// <summary>
    /// Creates a new log and prints everything that is in the backlog to the new log.
    /// </summary>
    public static void LogBacklog()
    {
        if (logThisLater.Count > 0)
        {
            InitNewLog();
        }
    }

    /// <summary>
    /// Writes to the log. This method will append a linebreak at the end of the written line.
    /// </summary>
    /// <param name="message"> The string that should be written to the log. </param>
    public static void Log(string message)
    {
        if (logFilePath == "")
        {
            logThisLater.Add("\t" + message);
        }
        else
        {
            using (StreamWriter logWriter = new StreamWriter(new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None)))
            {
                logWriter.WriteLine(message);
                logWriter.Flush();
            }
        }
    }

    /// <summary>
    /// Logs multiple messages. This method will append a linebreak between each message.
    /// </summary>
    /// <param name="message"> The messages that should be written to the log. </param>
    public static void Log(params string[] message)
    {
        if (logFilePath == "")
        {
            foreach (string s in message)
            {
                logThisLater.Add("\t" + s);
            }
        }
        else
        {
            using (StreamWriter logWriter = new StreamWriter(new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None)))
            {
                foreach (string s in message)
                {
                    logWriter.WriteLine(s);
                }
                logWriter.Flush();
            }
        }


    }

    /// <summary>
    /// Replaces all forward and backward slashes with whatever is the correct directory seperator character on this system.
    /// </summary>
    /// <param name="path"> A file path with a weird mix of forward and backward slashes. </param>
    /// <returns> A file path without a weird mix of forward and backward slashes. </returns>
    public static string FixFilePath(string path)
    {
        char directorySeparatorChar = Path.DirectorySeparatorChar;
        path = path.Replace('/', directorySeparatorChar);
        path = path.Replace('\\', directorySeparatorChar);
        return path;
    }

    /// <summary>
    /// Closes the old log and opens a new log.
    /// </summary>
    public static void UsernameChanged()
    {
        if (logFilePath != "")
        {
            Log("Changing user to " + CellExAlUser.Username,
                "Goodbye.");
        }
        InitNewLog();
    }
}

/// <summary>
/// This class subscribes to some events that is of intrest to the log.
/// </summary>
public class LogManager : MonoBehaviour
{

    private void Awake()
    {
        //CellExAlLog.InitNewLog();
        CellExAlUser.UsernameChanged.AddListener(CellExAlLog.UsernameChanged);
        CellExAlEvents.GraphsLoaded.AddListener(CellExAlLog.InitNewLog);

        string outputDirectory = Directory.GetCurrentDirectory() + "\\Output";

        if (!Directory.Exists(outputDirectory))
        {
            CellExAlLog.Log("Created directory " + CellExAlLog.FixFilePath(outputDirectory));
            Directory.CreateDirectory(outputDirectory);
        }
    }

    #region Events

    private void OnEnable()
    {
        SteamVR_Events.RenderModelLoaded.Listen(OnRenderModelLoaded);
        SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
    }

    private void OnDisable()
    {
        SteamVR_Events.RenderModelLoaded.Remove(OnRenderModelLoaded);
        SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
    }

    private void OnRenderModelLoaded(SteamVR_RenderModel model, bool success)
    {
        if (success)
            CellExAlLog.Log("Render model successfully loaded");
        else
            CellExAlLog.Log("ERROR: Render model not successfully loaded");
    }

    private void OnDeviceConnected(int index, bool connected)
    {
        var error = ETrackedPropertyError.TrackedProp_Success;
        var result = new System.Text.StringBuilder(64);
        OpenVR.System.GetStringTrackedDeviceProperty((uint)index, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);

        if (connected)
            CellExAlLog.Log("Device " + result.ToString() + " with index " + index + " connected");
        else
            CellExAlLog.Log("Device " + result.ToString() + " with index " + index + " disconnected");
    }

    private void OnApplicationQuit()
    {
        string nicerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        CellExAlLog.Log("Application quit on " + nicerTime);
        CellExAlLog.Log("Goodbye.");
    }

    #endregion

}
