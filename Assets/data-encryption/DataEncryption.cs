﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataEncryption : SignalingUI
{
    internal GameObject loginBtn, sendBtn, subscribeBtn, messageField, userCountObject, userNameField, channelTextObject;
    internal EncryptionManager encryptionManager;

    public override void Start()
    {
        base.Start();
        encryptionManager = new EncryptionManager();
        encryptionManager.LoadConfigFromJSON();
        SetupUI();
    }

    // Set up UI elements
    private void SetupUI()
    {
        FindCanvas(); // Extracted finding canvas into a separate method

        loginBtn = CreateButton("Login", new Vector3(-83, 97, 0), "Log In", new Vector2(120f, 30f), Login);
        subscribeBtn = CreateButton("Subscribe", new Vector3(-83, 54, 0), "Subscribe", new Vector2(120f, 30f), Subscribe);
        sendBtn = CreateButton("Send", new Vector3(-83, 14, 0), "Send", new Vector2(120f, 30f), Send);

        messageField = AddInputField("Message", new Vector3(-234, 13, 0), "Type your message", new Vector2(160, 30));
        userNameField = AddInputField("UserName", new Vector3(-234, 97, 0), "User ID", new Vector2(160, 30));

        userCountObject = AddLabel("userCount", new Vector3(-62, 130, 0), "User Count", 15);
        channelTextObject = AddLabel("channelLabel", new Vector3(-236, 56, 0), $"Current channel name is <b>{encryptionManager.configData.channelName}</b>", 13);
    }

    // Method to find the canvas
    private void FindCanvas()
    {
        canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
        }
    }

    // Method to create buttons with specified properties and onClick action
    private GameObject CreateButton(string buttonName, Vector3 position, string buttonText, Vector2 size, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject buttonObj = AddButton(buttonName, position, buttonText, size);
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(onClickAction);
        }
        else
        {
            Debug.LogError($"Button component not found on {buttonName}");
        }
        return buttonObj;
    }

    // Method to handle sending messages
    private void Send()
    {
        string msg = messageField.GetComponent<TMP_InputField>()?.text;
        if (string.IsNullOrEmpty(msg))
        {
            Debug.Log("Cannot send an empty message");
            return;
        }
        if (encryptionManager.signalingEngine == null)
        {
            Debug.Log("Login to send the message");
            return;
        }

        encryptionManager.SendChannelMessage(msg);
        msg = encryptionManager.configData.uid + ": " + msg;
        AddTextToDisplay(msg, Color.grey, TextAlignmentOptions.Right);
    }

    // Method to handle user login/logout
    private async void Login()
    {
        string userName = userNameField.GetComponent<TMP_InputField>()?.text;
        if (encryptionManager.isLogin)
        {
            encryptionManager.Logout();
        }
        else
        {
            await encryptionManager.FetchRtmToken(userName);
            encryptionManager.Login(userName, encryptionManager.configData.token);
        }
    }

    // Method to update UI elements, including user count and button statuses
    public override void Update()
    {
        base.Update();

        // Check for null before accessing properties or methods
        if (userCountObject != null)
        {
            // Update user count with bold formatting
            userCountObject.GetComponent<TextMeshProUGUI>().text = $"User count: <b>{encryptionManager.userCount}</b>";

        }

        if (subscribeBtn != null)
        {
            // Set interactable based on the condition
            subscribeBtn.GetComponent<Button>().interactable = encryptionManager.isLogin;
        }

        if (sendBtn != null)
        {
            // Set interactable based on the condition
            sendBtn.GetComponent<Button>().interactable = encryptionManager.isSubscribed;
        }
        UpdateButtonStatus();
    }

    // Method to update button text based on subscription and login status
    private void UpdateButtonStatus()
    {
        subscribeBtn.GetComponentInChildren<TextMeshProUGUI>().text = encryptionManager.isSubscribed ? "Unsubscribed" : "Subscribe";
        loginBtn.GetComponentInChildren<TextMeshProUGUI>().text = encryptionManager.isLogin ? "Logout" : "Login";
    }

    // Method to handle channel subscription/unsubscription
    public void Subscribe()
    {
        if (encryptionManager.isSubscribed)
        {
            encryptionManager.Unsubscribe();
        }
        else
        {
            encryptionManager.Subscribe();
        }
    }

    // OnDestroy method to clean up when the object is destroyed
    public override void OnDestroy()
    {
        encryptionManager.DestroyEngine();
        DestroyAllUIElements();
        ClearMessages();
    }

    // Method to destroy all UI elements
    private void DestroyAllUIElements()
    {
        DestroyUIElement(loginBtn);
        DestroyUIElement(subscribeBtn);
        DestroyUIElement(sendBtn);
        DestroyUIElement(messageField);
        DestroyUIElement(userNameField);
        DestroyUIElement(userCountObject);
        DestroyUIElement(channelTextObject);
    }

    // Method to destroy a specific UI element
    private void DestroyUIElement(GameObject obj)
    {
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}
