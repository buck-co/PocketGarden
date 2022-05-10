using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ConsoleDebugToTMPro : MonoBehaviour
{
    string myLog;

    public TMP_Text textBox;
    public int maxLines = 10;
    public int maxLineChars = 300;
    public bool muteDuplicates = true;
    public bool showStackTrace = false;

    List<string> allLines = new List<string>();
    string summedText;
    string lastEntry;
    string stackText;
    int count;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLog = logString;

        string newString = "[" + type + "] : " + myLog;

        bool needStack = false;
        if (type == LogType.Error && showStackTrace) {
            needStack = true;
            stackText = stackTrace;
        } 

        if (maxLineChars > 0)
        {
            newString = newString.Length <= maxLineChars ? newString : newString.Substring(0, maxLineChars); /// trim to max chars
        }

        Print(newString, muteDuplicates);

        if (needStack) Print(stackText);
    }

    public void Print(string incomingText, bool muteDuplicates = true)  //prints text to each object in textPanels, if filter is true then does not print immediately repeating lines
    {
        if (!muteDuplicates)
        {
            AddText(count + " | " + incomingText);
            count++;
        }
        else
        {
            if (lastEntry != incomingText)
            {
                AddText(count + " | " + incomingText);
                count++;
            }
            lastEntry = incomingText;
        }
    }

    private void AddText(string textToAdd)
    {
        summedText = null;
        allLines.Add(textToAdd);

        if (allLines.Count > maxLines)
        {
            allLines.RemoveAt(0);
        }

        foreach (string line in allLines)
        {
            summedText += line + '\n';
        }
        textBox.text = summedText;
    }

    public void ClearText()
    {
        allLines = new List<string>();
        textBox.text = "";
        count = 0;
    }
}
