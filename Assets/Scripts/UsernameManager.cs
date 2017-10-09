using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;

public class UsernameManager : MonoBehaviour
{

    public InputField usernameField;
    public Text usernameText;
    private void Start()
    {
        usernameField.onEndEdit.AddListener(OnUsernameSubmitted);
        usernameText.text = "Current user: " + CellExAlUser.Username;
    }

    private void OnUsernameSubmitted(string username)
    {
        CellExAlUser.Username = username;
        usernameText.text = "Current user: " + username;
    }
}

[System.Serializable]
public class UsernameChangedEvent : UnityEvent { }

/// <summary>
/// This static class represents a user that works with some data.
/// </summary>
public static class CellExAlUser
{
    private static string workingDirectory = Directory.GetCurrentDirectory();
    private static string username = "default_user";
    private static string dataFolder;

    /// <summary>
    /// An event that is triggered when the username is changed.
    /// </summary>
    public static UnityEvent UsernameChanged = new UsernameChangedEvent();

    /// <summary>
    /// Path to a folder unique to the current user and the currently loaded dataset.
    /// </summary>
    public static string UserSpecificFolder = workingDirectory + @"\Output\" + username;

    /// <summary>
    /// The user's name. This is edited through the escape menu.
    /// </summary>
    public static string Username
    {
        get
        {
            return username;
        }
        set
        {
            if (value.Equals(""))
                return;
            username = value.ToLower();
            UpdateUserSpecificFolder(username, UserSpecificDataFolder);
            UsernameChanged.Invoke();
        }
    }

    /// <summary>
    /// The folder containing the source data that we are currently working on.
    /// This should not be a full path, but rather a relative path from the Data folder.
    /// </summary>
    /// <example>
    /// Loading "Cellexal/Data/data_set_1" means this should be set to "data_set_1"
    /// </example>
    public static string UserSpecificDataFolder
    {
        get
        {
            return dataFolder;
        }
        set
        {
            if (value == "")
                return;
            dataFolder = value;
            UpdateUserSpecificFolder(Username, value);
        }
    }

    /// <summary>
    /// Helper method to set the user specific folder
    /// </summary>
    private static void UpdateUserSpecificFolder(string username, string dataFolder)
    {
        if (Username == "" || UserSpecificDataFolder == "")
        {
            return;
        }

        // make sure all the folders exist
        string userFolder = workingDirectory + @"\Output\" + username;
        if (!Directory.Exists(userFolder))
        {
            CellExAlLog.Log("Created directory " + userFolder);
            Directory.CreateDirectory(userFolder);
        }

        UserSpecificFolder = userFolder + @"\" + dataFolder;
        if (!Directory.Exists(UserSpecificFolder))
        {
            CellExAlLog.Log("Created directory " + UserSpecificFolder);
            Directory.CreateDirectory(UserSpecificFolder);
        }
    }
}
