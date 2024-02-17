using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Storage : SignalingUI
{
    // UI elements
    internal GameObject loginBtn, sendBtn, subscribeBtn, lockNameField, setLockBtn, getLockBtn, acquireLockBtn, releaseLockBtn, removeLockBtn, messageField, userCountObject, userNameField, channelTextObject;
    internal GameObject metaDataKeyField, metaDataValueField, metaDataRevisionField, metaDataLockField, channelMetadataUpdateBtn;
    internal GameObject userMetaDataField, userMetaDataUpdateBtn, locksTextObject, userLocksTextObject;
    internal StorageManager storageManager;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        // Initialize StorageManager
        storageManager = new StorageManager();
        storageManager.LoadConfigFromJSON();
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
        subscribeBtn = AddButton("Subscribe", new Vector3(-83, 61, 0), "Subscribe", new Vector2(120f, 30f));
        sendBtn = AddButton("Send", new Vector3(-83, 25, 0), "Send", new Vector2(120f, 30f));
        getLockBtn = AddButton("getLock", new Vector3(-162, -21, 0), "Get", new Vector2(60f, 30f));
        setLockBtn = AddButton("setLock", new Vector3(-100, -21, 0), "Set", new Vector2(60f, 30f));
        removeLockBtn = AddButton("removeLock", new Vector3(-100, -54, 0), "Remove", new Vector2(60f, 30f));
        releaseLockBtn = AddButton("releaseLock", new Vector3(-38, -54, 0), "Release", new Vector2(60f, 30f));
        acquireLockBtn = AddButton("acquireLock", new Vector3(-38, -21, 0), "Acquire", new Vector2(60f, 30f));
        lockNameField = AddInputField("lockNameField", new Vector3(-254, -21, 0), "Lock Name", new Vector2(120, 30));

        metaDataKeyField = AddInputField("metaDataKeyField", new Vector3(-254, -90, 0), "Metadata Key", new Vector2(120, 30));
        metaDataValueField = AddInputField("metaDataValueField", new Vector3(-132, -90, 0), "Metadata Value", new Vector2(120, 30));
        metaDataRevisionField = AddInputField("metaDataRevisionField", new Vector3(-254, -124, 0), "Revision", new Vector2(120, 30));
        metaDataLockField = AddInputField("metaDataLockField", new Vector3(-132, -124, 0), "Lock to Apply", new Vector2(120, 30));
        channelMetadataUpdateBtn = AddButton("updateBtn", new Vector3(-40, -124, 0), "Update", new Vector2(60f, 30f));

        userMetaDataField = AddInputField("userMetaDataField", new Vector3(-152, -161, 0), "Value for Key userBio", new Vector2(160, 30));
        userMetaDataUpdateBtn = AddButton("userMetaDataUpdateBtn", new Vector3(-40, -161, 0), "Update", new Vector2(60f, 30f));

        messageField = AddInputField("Message", new Vector3(-234, 25, 0), "Type your message", new Vector2(160, 30));

        // Create an input field for user ID;
        userNameField = AddInputField("UserName", new Vector3(-234, 97, 0), "User ID", new Vector2(160, 30));

        // Create a label to show the user count.
        userCountObject = AddLabel("userCount", new Vector3(-50, 135, 0), "User Count", 15);
        channelTextObject = AddLabel("channelLabel", new Vector3(-239, 60, 0), "Current channel name is <b>" + storageManager.configData.channelName + "</b>", 13);
        locksTextObject = AddLabel("locksTextObject", new Vector3(-234, -65, 0), "Channel Metadata:", 13);
        userLocksTextObject = AddLabel("userLocksTextObject", new Vector3(-254, -156, 0), "User Metadata:", 13);

        // Attach event listeners
        AttachEventListeners();
    }

    // Attach event listeners to UI buttons
    private void AttachEventListeners()
    {
        loginBtn.GetComponent<Button>().onClick.AddListener(Login);
        subscribeBtn.GetComponent<Button>().onClick.AddListener(Subscribe);
        sendBtn.GetComponent<Button>().onClick.AddListener(Send);
        setLockBtn.GetComponent<Button>().onClick.AddListener(SetLock);
        getLockBtn.GetComponent<Button>().onClick.AddListener(GetLocks);
        acquireLockBtn.GetComponent<Button>().onClick.AddListener(AcquireLock);
        removeLockBtn.GetComponent<Button>().onClick.AddListener(RemoveLock);
        releaseLockBtn.GetComponent<Button>().onClick.AddListener(ReleaseLock);
        userMetaDataUpdateBtn.GetComponent<Button>().onClick.AddListener(UpdateUserMetaData);
        channelMetadataUpdateBtn.GetComponent<Button>().onClick.AddListener(UpdateChannelMetadata);
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        // Check for null before accessing properties or methods
        if (userCountObject != null)
        {
            // Update user count with bold formatting
            userCountObject.GetComponent<TextMeshProUGUI>().text = $"User count: <b>{storageManager.userCount}</b>";
        }

        if (subscribeBtn != null)
        {
            // Set interactable based on the condition
            subscribeBtn.GetComponent<Button>().interactable = storageManager.isLogin;
        }

        if (sendBtn != null)
        {
            // Set interactable based on the condition
            sendBtn.GetComponent<Button>().interactable = storageManager.isSubscribed;
        }
        // Update UI
        UpdateButtonStatus();
    }

    // Method to update button text based on the state
    private void UpdateButtonStatus()
    {
        // Update button texts based on state
        loginBtn.GetComponentInChildren<TextMeshProUGUI>().text = storageManager.isLogin ? "Logout" : "Login";
        subscribeBtn.GetComponentInChildren<TextMeshProUGUI>().text = storageManager.isSubscribed ? "Unsubscribed" : "Subscribe";
    }

    // Event handler for updating user metadata
    private void UpdateUserMetaData()
    {
        string value = GetInputFieldText("userMetaDataField");
        if (string.IsNullOrEmpty(value))
        {
            storageManager.LogInfo("No value given to update the user metadata");
            return;
        }
        storageManager.UpdateUserMetadata(storageManager.configData.uid, "userBio", value);
    }

    // Event handler for updating channel metadata
    private void UpdateChannelMetadata()
    {
        string key = GetInputFieldText("metaDataKeyField");
        string value = GetInputFieldText("metaDataValueField");
        string revision = GetInputFieldText("metaDataRevisionField");
        string lockName = GetInputFieldText("lockNameField");

        // Validate inputs
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value) || string.IsNullOrEmpty(revision) || string.IsNullOrEmpty(lockName))
        {
            storageManager.LogError("Please fill in all the fields to update channel metadata.");
            return;
        }

        storageManager.UpdateChannelMetadata(storageManager.configData.channelName, value, key, int.Parse(revision), lockName);
    }

    // Event handler for setting a lock
    private void SetLock()
    {
        string lockName = GetInputFieldText("lockNameField");
        if (string.IsNullOrEmpty(lockName))
        {
            storageManager.LogInfo("Please specify a lock name to set the lock");
            return;
        }
        storageManager.SetLock(lockName, 50);
    }

    // Event handler for getting locks
    private void GetLocks()
    {
        storageManager.GetLocks();
    }

    // Event handler for acquiring a lock
    private void AcquireLock()
    {
        string lockName = GetInputFieldText("lockNameField");
        if (string.IsNullOrEmpty(lockName))
        {
            storageManager.LogInfo("Please specify a lock name to find the lock");
            return;
        }
        storageManager.AcquireLock(lockName);
    }

    // Event handler for removing a lock
    private void RemoveLock()
    {
        string lockName = GetInputFieldText("lockNameField");
        if (string.IsNullOrEmpty(lockName))
        {
            storageManager.LogInfo("Please specify a lock name to remove the lock");
            return;
        }
        storageManager.RemoveLock(lockName, "me");
    }

    // Event handler for releasing a lock
    private void ReleaseLock()
    {
        string lockName = GetInputFieldText("lockNameField");
        if (string.IsNullOrEmpty(lockName))
        {
            storageManager.LogInfo("Please specify a lock name to release the lock");
            return;
        }
        storageManager.RemoveLock(lockName, "me");
    }

    // Event handler for sending a message
    private void Send()
    {
        string msg = GetInputFieldText("Message");
        if (string.IsNullOrEmpty(msg))
        {
            Debug.Log("Cannot send an empty message");
            return;
        }
        if (storageManager.signalingEngine == null)
        {
            Debug.Log("Login to send the message");
            return;
        }

        storageManager.SendChannelMessage(msg);
        msg = storageManager.configData.uid + ": " + msg;
        AddTextToDisplay(msg, Color.grey, TextAlignmentOptions.Right);
    }

    // Event handler for user login/logout
    private async void Login()
    {
        string userName = GetInputFieldText("UserName");
        if (storageManager.isLogin)
        {
            storageManager.Logout();
            storageManager.GetChannelMetadata(storageManager.configData.channelName, Agora.Rtm.RTM_CHANNEL_TYPE.MESSAGE);
        }
        else
        {
            await storageManager.FetchRtmToken(userName);
            storageManager.Login(userName, storageManager.configData.token);
        }
    }

    // Event handler for subscribing/unsubscribing
    public void Subscribe()
    {
        if (storageManager.isSubscribed)
        {
            storageManager.Unsubscribe();
        }
        else
        {
            storageManager.Subscribe();
        }
    }

    // Event handler for destroying the UI elements and clearing messages
    public override void OnDestroy()
    {
        // Destroy all game objects
        DestroyAllGameObjects();
        // Clear the chat output section
        ClearMessages();
        // Kill the engine instance
        storageManager.DestroyEngine();
    }

    // Destroy all UI game objects
    private void DestroyAllGameObjects()
    {
        DestroyGameObject(loginBtn);
        DestroyGameObject(subscribeBtn);
        DestroyGameObject(sendBtn);
        DestroyGameObject(getLockBtn);
        DestroyGameObject(setLockBtn);
        DestroyGameObject(removeLockBtn);
        DestroyGameObject(releaseLockBtn);
        DestroyGameObject(acquireLockBtn);
        DestroyGameObject(lockNameField);
        DestroyGameObject(metaDataKeyField);
        DestroyGameObject(metaDataValueField);
        DestroyGameObject(metaDataRevisionField);
        DestroyGameObject(metaDataLockField);
        DestroyGameObject(channelMetadataUpdateBtn);
        DestroyGameObject(userMetaDataField);
        DestroyGameObject(userMetaDataUpdateBtn);
        DestroyGameObject(messageField);
        DestroyGameObject(userNameField);
        DestroyGameObject(userCountObject);
        DestroyGameObject(channelTextObject);
        DestroyGameObject(locksTextObject);
        DestroyGameObject(userLocksTextObject);
    }

    // Destroy a specific UI game object if it is not null
    private void DestroyGameObject(GameObject obj)
    {
        if (obj != null)
        {
            Destroy(obj);
        }
    }

    // Get the text from a TMP_InputField by its name
    private string GetInputFieldText(string fieldName)
    {
        return GameObject.Find(fieldName)?.GetComponent<TMP_InputField>().text;
    }
}
