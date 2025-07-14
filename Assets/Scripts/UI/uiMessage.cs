using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class uiMessage : MonoBehaviour {
    public static uiMessage instance { get; private set; }
    private TextMeshProUGUI textBox;
    private string displayText = string.Empty;
    private Queue<string> messages = new();
    private uint
        messageCount;
    private float
        resetTimeCurrent;
    [SerializeField] private uint
        maxMessages = 10;
    [SerializeField] private bool
        debugMessageSenders;
    [SerializeField] private float
        yPerMsg = 42.5f,
        animSpeed = 5f,
        resetTime = 3f;
    private void Awake() {
        instance = this;
        textBox = GetComponent<TextMeshProUGUI>();
        Clear();
    }

    private void Update() {
        // depending on the current number of messages,
        // the text box lerps up to make it look like the message is sliding up onto the screen
        textBox.margin = new(0, Mathf.Lerp(textBox.margin.y, -(messageCount * yPerMsg), animSpeed * Time.deltaTime), 0, 0);

        // every time a new message is added this timer is reset, if the timer reaches 0 then all messages are cleared
        if (resetTimeCurrent > 0) { resetTimeCurrent -= Time.deltaTime; }
        else {
            resetTimeCurrent = 0;
            Clear();
        }
    }

    private void Display() {
        var output = new StringBuilder();
        var queueCount = messages.Count;
        for (int i = 0; i < queueCount; i++) {
            var message = messages.Dequeue();
            if (message == string.Empty) { continue; }
            messageCount++;
            output.Append(message).Append('\n');
        }

        var outputStr = output.ToString();
        displayText += outputStr;

        var lineCount = 0;
        var index = 0;
        var charArray = displayText.ToCharArray();

        for (int i = charArray.Length - 1; i >= 0; i--) {
            char chara = charArray[i];

            if (chara == '\n') {
                lineCount++;
            }

            if (lineCount > maxMessages) {
                index = i + 1;
                messageCount = maxMessages;
                break;
            }
        }

        textBox.margin = new(0, -((messageCount - 1) * yPerMsg), 0, 0);
        displayText = displayText[index..];
        textBox.text = displayText;
    }

    /// <summary>
    /// Displays a new temporary message on the screen, the message will disappear if no messages have been added for 2 seconds
    /// </summary>
    /// <param name="text"></param>
    public void New(string text, string sender = "Undefined Sender") {
        messages.Enqueue(text);
        Display();
        resetTimeCurrent = resetTime;
        if (debugMessageSenders) { Debug.Log(sender + ": " + text); }
    }
    /// <summary>
    /// Clears the messages list
    /// </summary>
    public void Clear() {
        messages.Clear();
        textBox.text = string.Empty;
        displayText = string.Empty;
        messageCount = 0;
    }
}