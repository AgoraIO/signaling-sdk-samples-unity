using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StreamChannel : SignalingUI
{
    // UI elements
    internal GameObject loginBtn, joinTopicBtn, userCountObject, userNameField, joinChannelBtn, topicMessageField, topicNameField, sendTopicMessageBtn, channelNameField;
    internal StreamChannelManager streamChannelManager;

    public override void Start()
    {
        base.Start();
        // Initialize StreamChannelManager
        streamChannelManager = new StreamChannelManager();
        streamChannelManager.LoadConfigFromJSON();
        // Set up UI elements
        SetupUI();
    }

    // Set up UI elements
    private void SetupUI()
    {
        // Find the canvas
        canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Create and position UI elements
        loginBtn = AddButton("Login", new Vector3(-83, 97, 0), "Log In", new Vector2(120f, 30f));
        joinChannelBtn = AddButton("joinChannelBtn", new Vector3(-83, 56, 0), "Join", new Vector2(120f, 30f));
        sendTopicMessageBtn = AddButton("sendTopicMessageBtn", new Vector3(-83, -25, 0), "Send", new Vector2(120f, 30f));
        channelNameField = AddInputField("channelNameField", new Vector3(-234, 56, 0), "Type channel name", new Vector2(160, 30));
        topicNameField = AddInputField("topicNameField", new Vector3(-234, 15, 0), "Type the topic name", new Vector2(160, 30));
        topicMessageField = AddInputField("topicMessageField", new Vector3(-234, -25, 0), "Type your topic message", new Vector2(160, 30));
        joinTopicBtn = AddButton("joinTopicBtn", new Vector3(-83, 15, 0), "Join Topic", new Vector2(120f, 30f));

        userNameField = AddInputField("UserName", new Vector3(-234, 97, 0), "User ID", new Vector2(160, 30));
        TMP_InputField userID = userNameField.GetComponent<TMP_InputField>();
        userID.placeholder.GetComponent<TMP_Text>().text = streamChannelManager.configData.uid;
        userID.interactable = false;

        userCountObject = AddLabel("userCount", new Vector3(-62, 130, 0), "User Count", 15);

        // Attach event listeners
        loginBtn.GetComponent<Button>().onClick.AddListener(Login);
        joinChannelBtn.GetComponent<Button>().onClick.AddListener(ToggleChannel);
        sendTopicMessageBtn.GetComponent<Button>().onClick.AddListener(SendTopicMessage);
        joinTopicBtn.GetComponent<Button>().onClick.AddListener(ToggleTopic);
    }

    // Toggle channel join/leave
    private void ToggleChannel()
    {
        string channelName = GetInputFieldText("channelNameField");

        if (streamChannelManager.isChannelJoined)
        {
            streamChannelManager.LeaveStreamChannel();
            streamChannelManager.ReleaseStreamChannel();
        }
        else
        {
            if (streamChannelManager.isLogin)
            {
                if (string.IsNullOrEmpty(channelName))
                {
                    streamChannelManager.LogInfo("Type a channel name to join a stream channel");
                    return;
                }
                streamChannelManager.CreateChannel(channelName);
                streamChannelManager.JoinStreamChannel();
            }
            else
            {
                streamChannelManager.LogInfo("Login to join a channel");
            }
        }
    }

    // Toggle topic join/leave
    private void ToggleTopic()
    {
        string topic = GetInputFieldText("topicNameField");
        string channelName = GetInputFieldText("channelNameField");

        if (string.IsNullOrEmpty(channelName) || !streamChannelManager.isChannelJoined)
        {
            streamChannelManager.LogInfo("Join a channel to join a topic");
            return;
        }

        if (streamChannelManager.isTopicJoined)
        {
            streamChannelManager.LeaveTopic(topic);
        }
        else
        {
            streamChannelManager.JoinTopic(topic);
        }
    }

    // Send a message to the topic
    private void SendTopicMessage()
    {
        string topic = GetInputFieldText("topicNameField");
        string msg = GetInputFieldText("topicMessageField");

        if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(msg))
        {
            streamChannelManager.LogInfo("Please, specify a channel name and a topic to send messages");
            return;
        }

        if (streamChannelManager.isTopicJoined)
        {
            streamChannelManager.SendTopicMessage(msg, topic);
            msg = $"Topic: {topic}, Message: {msg}";
            streamChannelManager.SendChannelMessage(msg);
            AddTextToDisplay(msg, Color.grey, TextAlignmentOptions.Left);
        }
    }

    // Get text from an input field by name
    private string GetInputFieldText(string fieldName)
    {
        TMP_InputField inputField = GameObject.Find(fieldName)?.GetComponent<TMP_InputField>();
        return inputField?.text ?? string.Empty;
    }

    // Handle user login/logout
    private async void Login()
    {
        string userName = GetInputFieldText("UserName");

        if (streamChannelManager.isLogin)
        {
            streamChannelManager.Logout();
        }
        else
        {
            await streamChannelManager.FetchToken();
            streamChannelManager.Login(userName);
        }
    }

    public override void Update()
    {
        base.Update();

        // Update user count
        if (userCountObject != null && streamChannelManager != null && streamChannelManager.signalingEngine != null)
        {
            userCountObject.GetComponent<TextMeshProUGUI>().text = $"User count: <b>{streamChannelManager.userCount}</b>";
        }

        // Update button texts based on state
        loginBtn.GetComponentInChildren<TextMeshProUGUI>().text = streamChannelManager.isLogin ? "Logout" : "Login";
        joinChannelBtn.GetComponentInChildren<TextMeshProUGUI>().text = streamChannelManager.isChannelJoined ? "Leave" : "Join";
    }

    // Subscribe/unsubscribe from the channel
    public void Subscribe()
    {
        if (streamChannelManager.isSubscribed)
        {
            streamChannelManager.Unsubscribe();
        }
        else
        {
            streamChannelManager.Subscribe();
        }
    }

    // Destroy UI elements and clear messages on destroy
    private void OnDestroy()
    {
        DestroyAllGameObjects();
        ClearMessages();
        streamChannelManager.DestroyEngine();
    }

    // Destroy all UI game objects
    private void DestroyAllGameObjects()
    {
        DestroyGameObject(loginBtn);
        DestroyGameObject(sendTopicMessageBtn);
        DestroyGameObject(joinTopicBtn);
        DestroyGameObject(joinChannelBtn);
        DestroyGameObject(topicNameField);
        DestroyGameObject(channelNameField);
        DestroyGameObject(topicMessageField);
        DestroyGameObject(userNameField);
        DestroyGameObject(userCountObject);
    }

    // Destroy a specific game object if it exists
    private void DestroyGameObject(GameObject obj)
    {
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}
