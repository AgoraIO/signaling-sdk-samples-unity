using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthenticationWorkflow : SignalingUI
{
    // UI elements
    internal GameObject loginBtn, sendBtn, subscribeBtn, messageField, userCountObject, userNameField, chnnelNameField;
    internal AuthenticationManager authenticationManager;

    public override void Start()
    {
        base.Start();
        // Initialize AuthenticationManager and load configuration
        authenticationManager = new AuthenticationManager();
        authenticationManager.LoadConfigFromJSON();
        SetupUI(); // Set up UI elements
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
        subscribeBtn = AddButton("Subscribe", new Vector3(-83, 54, 0), "Subscribe", new Vector2(120f, 30f));
        sendBtn = AddButton("Send", new Vector3(-83, 14, 0), "Send", new Vector2(120f, 30f));

        messageField = AddInputField("Message", new Vector3(-234, 13, 0), "Type your message", new Vector2(160, 30));
        chnnelNameField = AddInputField("chnnelNameField", new Vector3(-234, 54, 0), "Type channel name", new Vector2(160, 30));
        userNameField = AddInputField("UserName", new Vector3(-234, 97, 0), "User ID", new Vector2(160, 30));
        userCountObject = AddLabel("userCount", new Vector3(-50, 135, 0), "User Count", 15);

        // Attach event listeners.
        loginBtn.GetComponent<Button>().onClick.AddListener(Login);
        subscribeBtn.GetComponent<Button>().onClick.AddListener(JoinAndLeaveStreamChannel);
        sendBtn.GetComponent<Button>().onClick.AddListener(Send);

    }

    // Join or leave the stream channel
    private void JoinAndLeaveStreamChannel()
    {
        string channelName = GameObject.Find("chnnelNameField").GetComponent<TMP_InputField>().text;
        if(channelName == "")
        {
            authenticationManager.LogInfo("Type a stream channel name to create and join the channel");
        }
        else
        {
            authenticationManager.JoinAndLeaveStreamChannel(channelName);
        }
    }

    // Send a message
    private void Send()
    {
        string msg = GameObject.Find("Message").GetComponent<TMP_InputField>().text;
        if (string.IsNullOrEmpty(msg))
        {
            Debug.Log("Cannot send an empty message");
            return;
        }
        if (authenticationManager.signalingEngine == null)
        {
            Debug.Log("Login to send the message");
            return;
        }
        authenticationManager.SendChannelMessage(msg);
        msg = authenticationManager.configData.uid + ": " + msg;
        AddTextToDisplay(msg, Color.grey, TextAlignmentOptions.Right);
    }

    // Handle user login
    private async void Login()
    {
        string userName = GameObject.Find("UserName").GetComponent<TMP_InputField>().text;
        if (authenticationManager.isLogin)
        {
            authenticationManager.Logout();
        }
        else
        {
            await authenticationManager.FetchRtmToken(userName);
            authenticationManager.Login(userName, authenticationManager.configData.token);
        }
    }

    // Update method for UI elements
    public override void Update()
    {
        base.Update();

        // Check for null before accessing properties or methods
        if (userCountObject != null)
        {
            // Update user count with bold formatting
            userCountObject.GetComponent<TextMeshProUGUI>().text = $"User count: <b>{authenticationManager.userCount}</b>";

        }

        if ( subscribeBtn != null)
        {
            // Set interactable based on the condition
            subscribeBtn.GetComponent<Button>().interactable = authenticationManager.isLogin;
        }

        if (sendBtn != null)
        {
            // Set interactable based on the condition
            sendBtn.GetComponent<Button>().interactable = authenticationManager.isSubscribed;
        }
        UpdateButtonStatus();

    }

    // Method to update button text based on subscription and login status
    private void UpdateButtonStatus()
    {
        subscribeBtn.GetComponentInChildren<TextMeshProUGUI>().text = authenticationManager.isSubscribed ? "Unsubscribed" : "Subscribe";
        loginBtn.GetComponentInChildren<TextMeshProUGUI>().text = authenticationManager.isLogin ? "Logout" : "Login";
    }

    // Subscribe or unsubscribe from the channel
    public void Subscribe()
    {
        if (authenticationManager.isSubscribed)
        {
            authenticationManager.Unsubscribe();
        }
        else
        {
            JoinAndLeaveStreamChannel();
            authenticationManager.Subscribe();
        }
    }

    // OnDestroy method to clean up
    public override void OnDestroy()
    {
        DestroyAllGameObjects(); // Destroy UI elements
        ClearMessages(); // Clear the chat output section
        authenticationManager.DestroyEngine(); // Kill the engine instance
    }

    // Destroy all UI elements
    private void DestroyAllGameObjects()
    {
        DestroyUIElement(loginBtn);
        DestroyUIElement(subscribeBtn);
        DestroyUIElement(sendBtn);
        DestroyUIElement(messageField);
        DestroyUIElement(userNameField);
        DestroyUIElement(userCountObject);
        DestroyUIElement(chnnelNameField);
    }

    // Destroy a specific UI element
    private void DestroyUIElement(GameObject obj)
    {
        if (obj != null)
        {
            Destroy(obj.gameObject);
        }
    }
}
