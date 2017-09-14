using System;
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

    public static void Start()
    {
        // File names cant have colons so we only use hyphens for the file name
        var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        logDirectory = Directory.GetCurrentDirectory() + "/Output";
        logFilePath = logDirectory + "/cellexal-log-" + time + ".txt";
        string logThis = "";

        if (!Directory.Exists(logDirectory))
        {
            logThis += "Created directory " + logDirectory;
            Directory.CreateDirectory(logDirectory);
        }

        if (!File.Exists(logFilePath))
        {
            logThis += "Created file " + logFilePath;
            File.Create(logFilePath).Dispose();
        }

        logStream = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        logWriter = new StreamWriter(logStream);

        string nicerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Log("Welcome to CellExAl " + Application.version,
            "Running on Unity " + Application.unityVersion,
            "BuildGUID: " + Application.buildGUID,
            "Log started at " + nicerTime);

        Log("\nSome system information",
            "\tOS: " + SystemInfo.operatingSystem,
            "\tCPU: " + SystemInfo.processorType,
            "\tProcessor count: " + SystemInfo.processorCount,
            "\tGPU: " + SystemInfo.graphicsDeviceName,
            "\tRAM size " + SystemInfo.systemMemorySize);
        Log(logThis);
    }

    /// <summary>
    /// Writes to the log. This method will append a linebreak at the end of the written line.
    /// </summary>
    /// <param name="message"> The string that should be written to the log. </param>
    public static void Log(string message)
    {
        logWriter.WriteLine(message);
        logWriter.Flush();
    }

    /// <summary>
    /// Logs multiple messages. This method will append a linebreak between each message.
    /// </summary>
    /// <param name="message"> The messages that should be written to the log. </param>
    public static void Log(params string[] message)
    {
        foreach (string s in message)
        {
            logWriter.WriteLine(s);
        }
        logWriter.Flush();
    }

    /// <summary>
    /// Closes the output stream to the log.
    /// </summary>
    public static void Close()
    {
        logWriter.Close();
    }
}

/// <summary>
/// This class subscribes to some events that is of intrest to the log.
/// </summary>
public class LogManager : MonoBehaviour
{

    private void Start()
    {
        CellExAlLog.Start();
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
