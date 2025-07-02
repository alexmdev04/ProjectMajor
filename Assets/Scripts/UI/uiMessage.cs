using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class uiMessage : MonoBehaviour {
    public static uiMessage instance { get; private set; }
    private TextMeshProUGUI textBox;
    private List<string> messages = new();
    private StringBuilder displayText;
    private RectTransform rectTransform;
    private int messageCount;
    private float
        resetTimeCurrent,
        targetYPos,
        startYPos;
    [SerializeField] private bool
        debugMessageSenders;
    [SerializeField] private float
        yPerMsg = 42.5f,
        animSpeed = 5f,
        resetTime = 3f;
    private void Awake() {
        instance = this;
        rectTransform = GetComponent<RectTransform>();
        textBox = GetComponent<TextMeshProUGUI>();
        //startYPos = textBox.margin.y - yPerMsg;
    }
    private void Update() {
        displayText = new StringBuilder();
        messageCount = 0;
        // gathers all valid messages and displays them as one block of text
        foreach (string msg in messages) {
            // if a message has been nullified it is skipped 
            if (msg == string.Empty) { continue; }
            messageCount++;
            displayText.Append(msg).Append("\n");
        }
        textBox.text = displayText.ToString();

        // if all messages are nullified, the lists are cleared
        if (textBox.text == string.Empty) { Clear(); }

        // depending on the current number of messages,
        // the text box lerps up to make it look like the message is sliding up onto the screen
        targetYPos = messageCount * yPerMsg;
        textBox.margin = new(0, Mathf.Lerp(textBox.margin.y, -targetYPos, animSpeed * Time.deltaTime), 0, 0);

        // every time a new message is added this timer is reset, if the timer reaches 0 then all messages are cleared
        if (resetTimeCurrent > 0) { resetTimeCurrent -= Time.deltaTime; }
        else {
            resetTimeCurrent = 0;
            Clear();
        }
    }
    /// <summary>
    /// Displays a new temporary message on the screen, the message will disappear if no messages have been added for 2 seconds
    /// </summary>
    /// <param name="text"></param>
    public void New(string text, string sender = "Undefined Sender") {
        messages.Add(text);
        resetTimeCurrent = resetTime;
        if (debugMessageSenders) { Debug.Log(sender + ": " + text); }
    }
    /// <summary>
    /// Clears the messages list
    /// </summary>
    public void Clear() {
        if (messages.Count > 0) { messages.Clear(); }
    }
    /// <summary>
    /// <para>Instantly removes a uiMessage at a given index</para>
    /// <para>This only nullifies the message, it does not delete it from the list of messages</para>
    /// </summary>
    /// <param name="msgIndex"></param>
    // the message is only nullified to keep index numbers intact
    // empty text is ignored when building the displayText so it achieves the same result as deleting
    public void Remove(int msgIndex) {
        messages[msgIndex] = string.Empty;
    }
}