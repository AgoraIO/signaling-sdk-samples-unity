// Importing necessary namespaces
using UnityEngine;
using Agora.Rtm;
using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;

// Serializable class for storing configuration data
[Serializable]
public class ConfigData
{
    public string uid;
    public string appId;
    public string channelName;
    public string rtcToken;
    public string serverUrl;
    public string tokenExpiryTime;
    public string token;
    public RTM_ENCRYPTION_MODE encryptionMode;
    public string salt;
    public string cipherKey;
    public uint presenceTimeout;
    public bool logUpload;
    public LOG_LEVEL logLevel;
    public bool cloudProxy;
    public bool useStringUserId;
}

// Main Signaling Manager class
public class SignalingManager
{
    // Internal variables
    internal IRtmClient signalingEngine;
    internal ConfigData configData;
    internal IStreamChannel signalingChannel;
    internal SignalingUI signalingUI;
    internal RtmConfig rtmConfig = new RtmConfig();
    internal int userCount = 0;
    internal bool isLogin = false;
    internal bool isSubscribed = false;
    internal UserState[] userStateList;

    // Method to set up the signaling engine
    public virtual void SetupSignalingEngine()
    {
        signalingUI = new SignalingUI();
        if (string.IsNullOrEmpty(configData.uid) || string.IsNullOrEmpty(configData.appId))
        {
            LogError("Username and appId are required to initialize.");
            return;
        }
        try
        {
            rtmConfig.appId = configData.appId;
            rtmConfig.userId = configData.uid;
            rtmConfig.useStringUserId = true;
            rtmConfig.logConfig = null;
            signalingEngine = RtmClient.CreateAgoraRtmClient(rtmConfig);

        }
        catch (RTMException e)
        {
            LogError($"Error initializing RtmClient: {e.Status.ErrorCode}");
        }

        // Register event handlers
        if (signalingEngine != null)
        {
            RegisterEventHandlers();
        }
    }

    // Method to register event handlers for the signaling engine
    public void RegisterEventHandlers()
    {
        signalingEngine.OnMessageEvent += OnMessageEvent;
        signalingEngine.OnPresenceEvent += OnPresenceEvent;
        signalingEngine.OnTopicEvent += OnTopicEvent;
        signalingEngine.OnStorageEvent += OnStorageEvent;
        signalingEngine.OnLockEvent += OnLockEvent;
        signalingEngine.OnConnectionStateChanged += OnConnectStateChanged;
        signalingEngine.OnTokenPrivilegeWillExpire += OnTokenPrivilegeWillExpire;
    }

    // Method to handle user logout
    public void Logout()
    {
        signalingEngine?.LogoutAsync();
        isLogin = false;
        isSubscribed = false;
        DestroyEngine();
    }

    // Method to handle user login
    public async void Login(string userName, string rtmToken)
    {
        if (signalingEngine == null)
        {
            SetupSignalingEngine();
        }
        if (string.IsNullOrEmpty(configData.token))
        {
            LogError("Token is required for login");
            return;
        }

        try
        {
            var result= await signalingEngine.LoginAsync(rtmToken);
            if(userName != "")
            {
                configData.uid = userName;
            }
            if (result.Status.Error)
            {
                LogError($"Error during login: {result.Status.Reason}");
            }
            else
            {
                LogInfo($"Login successful. Response: {result.Response}");
                isLogin = true;
            }
        }
        catch (Exception ex)
        {
            LogError($"Exception during login: {ex.Message}");
        }
    }

    // Method to load configuration data from a JSON file
    public void LoadConfigFromJSON()
    {
        string path = Path.Combine(Application.dataPath, "utils", "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            configData = JsonUtility.FromJson<ConfigData>(json);
        }
        else
        {
            LogError("Config file not found!");
        }
    }

    // Method to create a channel
    public void CreateChannel(string channelName)
    {
        signalingChannel = signalingEngine.CreateStreamChannel(channelName);
        if (signalingChannel == null)
        {
            LogError("Stream channel was not created");
        }
    }

    // Method to get online members in a channel
    public async Task GetOnlineMembersInChannel(string channel)
    {

        GetOnlineUsersOptions options = new GetOnlineUsersOptions();
        options.includeState = true;
        options.includeUserId = true;

        IRtmPresence rtmPresence = signalingEngine.GetPresence();
        var result = await rtmPresence.GetOnlineUsersAsync(configData.channelName, RTM_CHANNEL_TYPE.MESSAGE, options);
        userStateList = result.Response.UserStateList;
        userCount = result.Response.UserStateList.Length;
        string info = $"WhoNow Response: count:{userCount}, nextPage:{result.Response.NextPage}";
        LogInfo(info);
    }

    // Method to subscribe to a channel
    public async void Subscribe()
    {
        if (!isLogin)
        {
            LogInfo("Login first to subscribe to a channel");
            return;
        }
        if (configData.channelName == "")
        {
            LogInfo("Please specify a channel name in the config.json file");
            return;
        }
        SubscribeOptions subscribeOptions = new SubscribeOptions()
        {
            withMessage = true,
            withMetadata = true,
            withPresence = true,
            withLock = true
        };
        await signalingEngine.SubscribeAsync(configData.channelName, subscribeOptions);
        await GetOnlineMembersInChannel(configData.channelName);
        isSubscribed = true;
    }

    // Method to unsubscribe from a channel
    public async void Unsubscribe()
    {
        await signalingEngine.UnsubscribeAsync(configData.channelName);
        signalingChannel = null;
        isSubscribed = false;
    }

    // Method to send a message to a channel
    public async void SendChannelMessage(string msg)
    {
        
        PublishOptions options = new PublishOptions();
        options.channelType = RTM_CHANNEL_TYPE.MESSAGE;
        options.customType = "string";
        var result = await signalingEngine.PublishAsync(configData.channelName, msg, options);
        if (result.Status.Error)
        {
            LogError(string.Format("rtmClient.Publish Status.ErrorCode:{0} ", result.Status.ErrorCode));
        }
        else
        {
            string info = string.Format("Message published successfully");
            LogInfo(info);
        }
    }

    // Event Handlers

    // Method to handle message events
    public async void OnMessageEvent(MessageEvent @event)
    {
        string str = $"OnMessageEvent channelName:{@event.channelName} channelTopic:{@event.channelTopic} " +
                     $"channelType:{@event.channelType} publisher:{@event.publisher} " +
                     $"message:{@event.message.GetData<string>()} customType:{@event.customType}";
        LogInfo(str);
        string msg = @event.publisher.ToString() + ": " + @event.message.GetData<string>();
        signalingUI.AddTextToDisplay(msg, Color.gray, TextAlignmentOptions.Left);
        await GetOnlineMembersInChannel(configData.channelName);
    }

    // Method to handle connection state change events
    public void OnConnectStateChanged(string channelName, RTM_CONNECTION_STATE state, RTM_CONNECTION_CHANGE_REASON reason)
    {
        string str = $"OnConnectStateChanged channelName:{channelName} current state: {state} reason: {reason}";
        LogInfo(str);
    }

    // Method to handle presence events
    public virtual void OnPresenceEvent(PresenceEvent @event)
    {
        string str = $"OnPresenceEvent: type:{@event.type} channelType:{@event.channelType} " +
                     $"channelName:{@event.channelName} publisher:{@event.publisher}";
        LogInfo(str);
    }

    // Method to handle storage events
    public void OnStorageEvent(StorageEvent @event)
    {
        string str = $"OnStorageEvent: channelType:{@event.channelType} storageType:{@event.storageType} " +
                     $"eventType:{@event.eventType} target:{@event.target}";
        LogInfo(str);
    }

    // Method to handle topic events
    public void OnTopicEvent(TopicEvent @event)
    {
        string str = $"OnTopicEvent: channelName:{@event.channelName} publisher:{@event.publisher}";
        var topicInfoCount = @event.topicInfos?.Length ?? 0;
        LogInfo(str);
        if (topicInfoCount > 0)
        {
            for (var i = 0; i < topicInfoCount; i++)
            {
                var topicInfo = @event.topicInfos[i];
                var publisherCount = topicInfo.publishers?.Length ?? 0;
                string str1 = $"|--topicInfo {i}: topic:{topicInfo.topic} publisherCount:{publisherCount}";

                if (publisherCount > 0)
                {
                    for (var j = 0; j < publisherCount; j++)
                    {
                        var publisher = topicInfo.publishers[j];
                        string str2 = $"  |--publisher {j}: userId:{publisher.publisherUserId} meta:{publisher.publisherMeta}";
                        LogInfo(str2);
                    }
                }
                LogInfo(str1);
            }
        }
    }

    // Method to handle lock events
    public void OnLockEvent(LockEvent @event)
    {
        var count = @event.lockDetailList?.Length ?? 0;
        string info = $"OnLockEvent channelType:{@event.channelType}, eventType:{@event.eventType}, " +
                      $"channelName:{@event.channelName}, count:{count}";

        if (count > 0)
        {
            for (var i = 0; i < count; i++)
            {
                var detail = @event.lockDetailList[i];
                string info2 = $"lockDetailList lockName:{detail.lockName}, owner:{detail.owner}, ttl:{detail.ttl}";
                LogInfo(info2);
            }
        }
        LogInfo(info);
    }

    // Method to handle token privilege will expire event
    public virtual void OnTokenPrivilegeWillExpire(string channelName)
    {
        string str1 = $"OnTokenPrivilegeWillExpire channelName: {channelName}";
        LogInfo(str1);
    }

    // Method to log errors
    internal void LogError(string message)
    {
        Debug.LogError(message);
    }

    // Method to destroy the signaling engine
    public void DestroyEngine()
    {
        signalingEngine?.Dispose();
        signalingEngine = null;
        signalingChannel = null;
        if(signalingUI)
        {
            signalingUI.ClearMessages();
        }
    }

    // Method to log information
    internal void LogInfo(string message)
    {
        Debug.Log(message);
    }

}
