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
    }

    private void OnUsernameSubmitted(string username)
    {
        CellExAlUser.Username = username;
        usernameText.text = "Current user: " + username;
    }
}

[System.Serializable]
public class UsernameChangedEvent : UnityEvent<string>
{
}

/// <summary>
/// This static class represents a user.
/// </summary>
public static class CellExAlUser
{
    private static string workingDirectory = Directory.GetCurrentDirectory();
    private static string username = "default_user";
    private static bool UsernameSet = false;

    /// <summary>
    /// An event that is triggered when the username is changed.
    /// </summary>
    public static UnityEvent<string> UsernameChanged = new UsernameChangedEvent();
    /// <summary>
    /// Path to a folder unique to the current user.
    /// </summary>
    public static string UserSpecificFolder = workingDirectory + "/Output/" + username;
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
            UsernameSet = true;
            string lowercaseUsername = value.ToLower();
            username = lowercaseUsername;
            UserSpecificFolder = workingDirectory + "/Output/" + username;
            UsernameChanged.Invoke(lowercaseUsername);
        }
    }
}