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
    private static string logFilePath;
    private static FileStream logStream;
    private static TextWriter logWriter;
    private static List<string> logThisLater = new List<string>();

    public static void InitNewLog()
    {
        // File names can't have colons so we only use hyphens
        var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        logDirectory = Directory.GetCurrentDirectory() + "/Output";


        if (!Directory.Exists(logDirectory))
        {
            logThisLater.Add("\tCreated directory " + logDirectory);
            Directory.CreateDirectory(logDirectory);
        }

        logDirectory += "/" + CellExAlUser.Username;
        if (!Directory.Exists(logDirectory))
        {
            logThisLater.Add("\tCreated directory " + logDirectory);
            Directory.CreateDirectory(logDirectory);
        }

        logFilePath = logDirectory + "/cellexal-log-" + time + ".txt";
        // this will most likely always happen
        if (!File.Exists(logFilePath))
        {
            logThisLater.Add("\tCreated file " + logFilePath);
            File.Create(logFilePath).Dispose();
        }

        logStream = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        logWriter = new StreamWriter(logStream);

        string nicerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Log("Welcome to CellExAl " + Application.version,
            "Running on Unity " + Application.unityVersion,
            "BuildGUID: " + Application.buildGUID,
            "Log started at " + nicerTime);

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
        if (logWriter == null)
        {
            logThisLater.Add("\t" + message);
        }
        else
        {
            logWriter.WriteLine(message);
            logWriter.Flush();
        }
    }

    /// <summary>
    /// Logs multiple messages. This method will append a linebreak between each message.
    /// </summary>
    /// <param name="message"> The messages that should be written to the log. </param>
    public static void Log(params string[] message)
    {
        if (logWriter == null)
        {
            foreach (string s in message)
            {
                logThisLater.Add("\t" + s);
            }
        }
        else
        {
            foreach (string s in message)
            {
                logWriter.WriteLine(s);
            }
            logWriter.Flush();
        }
    }

    /// <summary>
    /// Closes the old log and opens a new log.
    /// </summary>
    /// <param name="newUsername"> The new username. </param>
    public static void UsernameChanged(string newUsername)
    {
        if (logWriter != null)
        {
            Log("Changing user to " + newUsername,
                "Goodbye.");
            Close();
        }
        InitNewLog();
    }

    /// <summary>
    /// Closes the output stream to the log.
    /// </summary>
    public static void Close()
    {
        if (logWriter != null)
        {
            logWriter.Close();
            logWriter = null;
        }
    }

}

/// <summary>
/// This class subscribes to some events that is of intrest to the log.
/// </summary>
public class LogManager : MonoBehaviour
{

    private void Start()
    {
        //CellExAlLog.InitNewLog();
        CellExAlUser.UsernameChanged.AddListener(CellExAlLog.UsernameChanged);
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
        CellExAlLog.Close();
    }

    #endregion

}
