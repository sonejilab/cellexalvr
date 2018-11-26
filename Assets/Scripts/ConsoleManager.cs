using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The console for executing commands from the desktop.
/// </summary>
public class ConsoleManager : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject consoleGameObject;
    public TMPro.TMP_InputField inputField;
    public TMPro.TMP_InputField outputField;
    public RectTransform outputTextAreaTransform;

    private bool consoleActive = false;
    private Dictionary<MethodInfo, string> accessors = new Dictionary<MethodInfo, string>();
    private Dictionary<string, MethodInfo> commands = new Dictionary<string, MethodInfo>();
    private Dictionary<MethodInfo, List<string>> aliases = new Dictionary<MethodInfo, List<string>>();

    private LinkedList<string> history = new LinkedList<string>();
    private LinkedListNode<string> currentHistoryNode;
    // default value in case things are written to the console before the log is read.
    private int maxBufferLines = 64;
    private int currentNumberOfLines = 0;
    private string outputBufferString = "";

    private void Start()
    {
        // scan everything for methods that are marked as console commands
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods())
                {
                    var attribute = method.GetCustomAttribute<ConsoleCommand>();
                    if (attribute != null)
                    {
                        accessors[method] = attribute.Access;
                        foreach (string alias in attribute.Aliases)
                        {
                            commands[alias] = method;
                            if (!aliases.ContainsKey(method))
                            {
                                aliases[method] = new List<string>();
                            }
                            aliases[method].Add(alias);
                        }
                    }
                }
            }
        }
        history.AddFirst("");
        currentHistoryNode = history.First;
        maxBufferLines = CellexalConfig.ConsoleMaxBufferLines;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            consoleActive = !consoleActive;
            if (consoleActive)
            {
                consoleGameObject.SetActive(true);
                inputField.ActivateInputField();
                //inputField.Select();
                //StartCoroutine(DeselectInputField());
            }
            else
            {

                inputField.DeactivateInputField();
                consoleGameObject.SetActive(false);
            }
        }

        if (!consoleActive)
            return;

        // only if the console is active
        if (Input.GetKeyDown(KeyCode.Return))
        {
            CommandEntered();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TraverseHistory(true);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TraverseHistory(false);
        }
    }

    /// <summary>
    /// We can't deselect the text in the console input window the same frame it is activated, so this coroutine waits one frame and then deselects it.
    /// </summary>
    private IEnumerator DeselectInputField()
    {
        yield return null;
        inputField.MoveTextEnd(true);
    }

    /// <summary>
    /// Goes one step forward or backward in the history of all written commands.
    /// </summary>
    /// <param name="goBack">True for going back in history, false for going forward.</param>
    public void TraverseHistory(bool goBack)
    {
        if (goBack && currentHistoryNode.Next != null)
        {
            if (currentHistoryNode == history.First)
            {
                currentHistoryNode.Value = inputField.text;
            }
            currentHistoryNode = currentHistoryNode.Next;
        }
        else if (!goBack && currentHistoryNode.Previous != null)
        {
            currentHistoryNode = currentHistoryNode.Previous;
        }

        inputField.text = currentHistoryNode.Value;
        inputField.MoveTextEnd(false);
    }

    /// <summary>
    /// Appends some output to the console's output window
    /// </summary>
    /// <param name="output">The string to append.</param>
    public void AppendOutput(string output)
    {
        outputBufferString = outputBufferString + "\n" + output;
        // find number of lines breaks
        int nbrOfNewLines = output.Count((c) => c == '\n') + 1;
        currentNumberOfLines += nbrOfNewLines;

        if (currentNumberOfLines > maxBufferLines)
        {
            int nbrOfExcessLines = currentNumberOfLines - maxBufferLines;
            // remove the oldest line
            // find how may characters that is
            int lineBreakIndex = 0;
            for (int i = 0; i < nbrOfExcessLines; ++i)
            {
                while (outputBufferString[lineBreakIndex] != '\n')
                {
                    lineBreakIndex++;
                }
                lineBreakIndex++;
            }
            // remove that much
            outputBufferString = outputBufferString.Remove(0, lineBreakIndex);
            currentNumberOfLines -= nbrOfExcessLines;
        }

        outputField.text = outputBufferString;
        outputField.MoveTextEnd(false);
        outputField.textComponent.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();

        // scroll the output to the end
        //outputField.verticalScrollbar.value = 1;
        float yCoordinate = outputField.textComponent.textBounds.size.y + outputTextAreaTransform.rect.height;

        //print(string.Format("outputField.textComponent.textBounds.size.y: {0} outputTextAreaTransform.rect.size.y {1} yCoordinate: {2}", outputField.textComponent.textBounds.size.y, outputTextAreaTransform.rect.size.y, yCoordinate));

        outputTextAreaTransform.position = new Vector3(0f, yCoordinate, 0f);

        Canvas.ForceUpdateCanvases();
        //outputTextAreaTransform.offsetMin = new Vector2(0f, -bottomYCoordinate);
        //outputTextAreaTransform.offsetMax = new Vector2(0f, bottomYCoordinate);
        //outputTextAreaTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, bottomYCoordinate, outputTextAreaTransform.rect.size.y);


        //outputTextAreaTransform.anchoredPosition = new Vector3(0f, 1f, 0f);
        //Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Called when a command is entered.
    /// Checks that the command syntax is correct and executes the command.
    /// </summary>
    public void CommandEntered()
    {
        string command = inputField.text;
        AppendOutput("> " + command);
        inputField.text = "";

        inputField.ActivateInputField();
        inputField.Select();


        if (command == "")
        {
            return;
        }

        history.First.Value = command;
        history.AddFirst("");
        currentHistoryNode = history.First;

        string[] words = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // check that the command exists
        if (!commands.ContainsKey(words[0]))
        {
            AppendOutput("Command not found. Type 'listall' for all defined commands.");
            return;
        }

        MethodInfo method = commands[words[0]];
        string accessFieldName = accessors[method];

        // get the object that the method should run in
        var access = referenceManager.GetType().GetField(accessFieldName).GetValue(referenceManager);

        ParameterInfo[] parameterInfo = method.GetParameters();
        // check the number of parameters
        if (words.Length - 1 != parameterInfo.Length)
        {
            AppendOutput(string.Format("Wrong number of parameters. The command {0} has the parameters {1}", words[0], ParameterInfosToString(parameterInfo)));
            return;
        }

        object[] parameters = new object[words.Length - 1];
        // parse the parameters
        for (int i = 1; i < words.Length; ++i)
        {
            try
            {
                parameters[i - 1] = ParseArgument(words[i], parameterInfo[i - 1].ParameterType);
            }
            catch (ArgumentException e)
            {
                AppendOutput(e.Message);
                return;
            }
            catch (FormatException)
            {
                AppendOutput(string.Format("The paramater {0} could not be parsed as type {1}", words[i], parameterInfo[i - 1].ParameterType));
                return;
            }
        }

        // finally invoke the method
        method.Invoke(access, parameters);

    }

    /// <summary>
    /// Helper method to parse arguments for commands.
    /// Commands must be executed with a <see cref="object[]"/> as arguments, but the actual type of the objects must be whatever the function expects.
    /// Throws <see cref="ArgumentException"/> if <paramref name="t"/> is not a parsable type.
    /// </summary>
    /// <param name="arg">The argument to parse.</param>
    /// <param name="t">The type that the argument should be.</param>
    /// <returns>A parsed argument of the type of <paramref name="t"/>.</returns>
    private object ParseArgument(string arg, Type t)
    {
        if (t == typeof(int))
        {
            return int.Parse(arg);
        }
        else if (t == typeof(float))
        {
            return float.Parse(arg);
        }
        else if (t == typeof(double))
        {
            return double.Parse(arg);
        }
        else if (t == typeof(bool))
        {
            return arg != "0";
        }
        else if (t == typeof(string))
        {
            return arg;
        }
        else
        {
            // unknown type
            throw new ArgumentException("ERROR: Argument was not a known type that could be parsed");
        }
    }

    /// <summary>
    /// Converts an array of <see cref="System.Reflection.ParameterInfo"/> to a readable string.
    /// </summary>
    /// <param name="info">The <see cref="System.Reflection.ParameterInfo"/> to convert to a string.</param>
    private string ParameterInfosToString(ParameterInfo[] info)
    {
        if (info.Length == 0)
        {
            return "";
        }
        StringBuilder sb = new StringBuilder();
        sb.Append(info[0].ParameterType).Append(" ").Append(info[0].Name);
        for (int i = 1; i < info.Length; ++i)
        {
            sb.Append(", ").Append(info[i].ParameterType).Append(" ").Append(info[i].Name);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Lists all available commands.
    /// </summary>
    [ConsoleCommand("consoleManager", "listall")]
    public void ListAllCommands()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var alias in aliases)
        {
            sb.Append(alias.Key.Name).Append(" is mapped to ");
            List<string> methodAliases = alias.Value;
            sb.Append(methodAliases[0]);
            for (int i = 1; i < methodAliases.Count; ++i)
            {
                sb.Append(", ").Append(methodAliases[i]);
            }
            sb.Append("\n");

        }
        AppendOutput(sb.ToString());
    }

    /// <summary>
    /// Displays information about the arguments of a command. Semi-helpful.
    /// </summary>
    /// <param name="command">The command to show arguemnt information about.</param>
    [ConsoleCommand("consoleManager", "arguments", "args")]
    public void ParameterInfo(string command)
    {
        string parameterInfoString = ParameterInfosToString(commands[command].GetParameters());
        if (parameterInfoString == "")
        {
            AppendOutput("Command has no arguments");
        }
        else
        {
            AppendOutput(parameterInfoString);
        }
    }

    [ConsoleCommand("consoleManager", "clear", "cls")]
    public void ClearConsole()
    {
        currentNumberOfLines = 0;
        outputBufferString = "";
        outputField.text = "";
    }

}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class ConsoleCommand : Attribute
{
    public string Access { get; private set; }
    public string[] Aliases { get; private set; }

    public ConsoleCommand(string access, params string[] aliases)
    {
        Access = access;
        Aliases = aliases;
    }
}
