using System.Collections.Generic;
using Agora.Rtm;
using io.agora.rtm.demo;
using UnityEngine.UI;

public class StorageManager : AuthenticationManager
{
    // Input fields and UI elements
    public InputField UserIdInput;
    public Toggle RecordTsToggle;
    public Toggle RecordUserIdToggle;
    public InputField MajorRevisionInput;
    public ListContainerMetadataItem ContainerMetadataItem;

    // Set user metadata
    public async void SetUserMetadata(string uid, string key, string value)
    {
        // Configure metadata options
        MetadataOptions metadataOptions = new MetadataOptions()
        {
            recordUserId = true,
            recordTs = true
        };

        // Create metadata item
        MetadataItem items = new MetadataItem();
        items.authorUserId = uid;
        items.value = value;
        items.key = key;
        items.revision = -1;

        // Create overall metadata structure
        RtmMetadata rtmMetadata = new RtmMetadata();
        rtmMetadata.majorRevision = -1;
        rtmMetadata.metadataItems = new MetadataItem[1] { items };

        // Get RTM storage instance
        IRtmStorage rtmStorage = signalingEngine.GetStorage();

        // Set user metadata asynchronously
        var result = await rtmStorage.SetUserMetadataAsync(uid, rtmMetadata, metadataOptions);
        if (result.Status.Error)
        {
            LogError($"SetUserMetadata Status.ErrorCode:{result.Status.ErrorCode}");
        }
        else
        {
            LogInfo($"SetUserMetadata Response : userId:{result.Response.UserId}");
        }
    }

    // Update channel metadata
    public async void UpdateChannelMetadata(string channelName, string value, string key, int revision, string lockName)
    {
        // Configure metadata options
        MetadataOptions metadataOptions = new MetadataOptions()
        {
            recordUserId = true,
            recordTs = true
        };

        // Create metadata item
        MetadataItem item = new MetadataItem();
        item.authorUserId = configData.uid;
        item.value = value;
        item.key = key;
        item.revision = -1;

        // Create overall metadata structure
        RtmMetadata rtmMetadata = new RtmMetadata();
        rtmMetadata.majorRevision = revision;
        rtmMetadata.metadataItems = new MetadataItem[1] { item };
        IRtmStorage rtmStorage = signalingEngine.GetStorage();

        // Set channel metadata asynchronously
        var result = await rtmStorage.SetChannelMetadataAsync(channelName, RTM_CHANNEL_TYPE.MESSAGE, rtmMetadata, metadataOptions, lockName);
        if (result.Status.Error)
        {
            LogError($"SetChannelMetadata Status.reason:{result.Status.Reason}");
        }
        else
        {
            LogInfo($"SetChannelMetadata Response : channelName:{result.Response.ChannelName}");
        }
    }

    // Update user metadata
    public async void UpdateUserMetadata(string uid, string key, string value)
    {
        if (signalingEngine == null)
        {
            LogError("SignalingEngine is null");
            return;
        }

        // Configure metadata options
        MetadataOptions metadataOptions = new MetadataOptions()
        {
            recordUserId = true,
            recordTs = true
        };

        // Create metadata item
        MetadataItem items = new MetadataItem();
        items.authorUserId = uid;
        items.value = value;
        items.key = key;
        items.revision = -1;

        // Create overall metadata structure
        RtmMetadata rtmMetadata = new RtmMetadata();
        rtmMetadata.majorRevision = -1;
        rtmMetadata.metadataItems = new MetadataItem[1] { items };

        IRtmStorage rtmStorage = signalingEngine.GetStorage();
        // Update user metadata asynchronously
        var result = await rtmStorage.UpdateUserMetadataAsync(uid, rtmMetadata, metadataOptions);
        if (result.Status.Error)
        {
            LogError($"UpdateUserMetadata Status.reason:{result.Status.Reason}");
        }
        else
        {
            LogInfo($"UpdateUserMetadata Response ,userId:{result.Response.UserId}");
        }
    }

    // Get user metadata
    public async void GetUserMetadata(string uid)
    {
        if (signalingEngine == null)
        {
            LogError("RtmClient is null");
            return;
        }

        IRtmStorage rtmStorage = signalingEngine.GetStorage();
        // Get user metadata asynchronously
        var result = await rtmStorage.GetUserMetadataAsync(uid);
        if (result.Status.Error)
        {
            LogError($"GetUserMetadata Status.Reason:{result.Status.Reason}");
        }
        else
        {
            LogInfo($"GetUserMetadata Response ,userId:{result.Response.UserId}");
            DisplayRtmMetadata(ref result.Response.Data);
        }
    }

    // Subscribe to user metadata
    public async void SubscribeUserMetadata(string uid)
    {
        if (signalingEngine == null)
        {
            LogError("RtmClient is null");
            return;
        }

        if (isLogin == false)
        {
            LogError("Login to subscribe to the user metadata");
            return;
        }
        IRtmStorage rtmStorage = signalingEngine.GetStorage();
        // Subscribe to user metadata asynchronously
        var result = await rtmStorage.SubscribeUserMetadataAsync(uid);
        if (result.Status.Error)
        {
            LogError($"SubscribeUserMetadata Status.Reason:{result.Status.Reason}");
        }
        else
        {
            LogInfo($"SubscribeUserMetadata Response userId:{result.Response.UserId}");
        }
    }

    // Unsubscribe from user metadata
    public async void UnsubscribeUserMetadata(string uid)
    {
        if (signalingEngine == null)
        {
            LogError("RtmClient is null");
            return;
        }
        if (isLogin == false)
        {
            LogError("Login to unsubscribe to the user metadata");
            return;
        }
        IRtmStorage rtmStorage = signalingEngine.GetStorage();
        // Unsubscribe from user metadata asynchronously
        var result = await rtmStorage.UnsubscribeUserMetadataAsync(uid);
        if (result.Status.Error)
        {
            LogError($"IRtmStorage.UnsubscribeUserMetadata  ret: {result.Status.ErrorCode}");
        }
        else
        {
            LogInfo("Subscribed successfully");
        }
    }

    // Get channel metadata
    public async void GetChannelMetadata(string channelName, RTM_CHANNEL_TYPE type)
    {
        if (signalingEngine == null)
        {
            LogError("RtmClient is null");
            return;
        }

        if (isLogin == false)
        {
            LogError("Login to get the channel metadata");
            return;
        }
        IRtmStorage rtmStorage = signalingEngine.GetStorage();
        // Get channel metadata asynchronously
        var result = await rtmStorage.GetChannelMetadataAsync(channelName, type);
        if (result.Status.Error)
        {
            LogError($"IRtmStorage.UnsubscribeUserMetadata  ret: {result.Status.Reason}");
        }
        else
        {
            LogInfo($"getChannelMetadata channelName :{result.Response.ChannelName}");
            DisplayRtmMetadata(ref result.Response.Data);
        }
    }

    // Set a lock
    public async void SetLock(string lockName, int ttl)
    {
        if (signalingEngine == null)
        {
            LogError("RtmClient is null");
            return;
        }

        if (isLogin == false)
        {
            LogInfo("Login to set the specified lock");
            return;
        }

        IRtmLock rtmLock = signalingEngine.GetLock();
        // Set lock asynchronously
        var result = await rtmLock.SetLockAsync(configData.channelName, RTM_CHANNEL_TYPE.MESSAGE, lockName, ttl);

        if (result.Status.Error)
        {
            LogError($"SetLock Status.Reason:{result.Status.Reason}");
        }
        else
        {
            LogInfo($"SetLock Response :channelName:{result.Response.ChannelName}, channelType:{result.Response.ChannelType}, lockName:{result.Response.LockName}");
        }
    }

    // Get all locks
    public async void GetLocks()
    {
        if (signalingEngine == null)
        {
            LogError("Rtm client is null");
            return;
        }

        if (isLogin == false)
        {
            LogInfo("Login to view all locks");
            return;
        }

        IRtmLock rtmLock = signalingEngine.GetLock();
        // Get locks asynchronously
        var result = await rtmLock.GetLocksAsync(configData.channelName, RTM_CHANNEL_TYPE.MESSAGE);

        if (result.Status.Error)
        {
            LogError($"GetLocks Status.Reason:{result.Status.Reason}");
        }
        else
        {
            var LockDetailListCount = result.Response.LockDetailList == null ? 0 : result.Response.LockDetailList.Length;
            LogInfo($"GetLocks Response: channelName:{result.Response.ChannelName},channelType:{result.Response.ChannelType},count:{LockDetailListCount}");
            if (LockDetailListCount > 0)
            {
                for (int i = 0; i < result.Response.LockDetailList.Length; i++)
                {
                    var detail = result.Response.LockDetailList[i];
                    LogInfo($"{i} lockName:{detail.lockName}, owner:{detail.owner}, ttl:{detail.ttl}");
                }
            }
        }
    }

    // Remove a lock
    public async void RemoveLock(string lockName, string channelName, RTM_CHANNEL_TYPE channelType)
    {
        if (signalingEngine == null)
        {
            LogError("Rtm client is null");
            return;
        }
        if (lockName == "")
        {
            LogInfo("Lock name is empty");
            return;
        }
        if (isLogin == false)
        {
            LogError("Login to remove the lock");
            return;
        }
        IRtmLock rtmLock = signalingEngine.GetLock();

        // Remove lock asynchronously
        var result = await rtmLock.RemoveLockAsync(channelName, channelType, lockName);
        if (result.Status.Error)
        {
            LogError($"RemoveLock Status.Reason:{result.Status.Reason}");
        }
        else
        {
            LogInfo($"RemoveLock Response channelName:{result.Response.ChannelName},channelType:{result.Response.ChannelType},lockName:{result.Response.LockName}");
        }
    }

    // Release a lock
    public async void ReleaseLock(string lockName, string channelName)
    {
        if (signalingEngine == null)
        {
            LogError("Rtm client is null");
            return;
        }

        if (lockName == "")
        {
            LogError("Lock name is empty");
            return;
        }

        if (isLogin == false)
        {
            LogInfo("Login to release the lock");
            return;
        }
        IRtmLock rtmLock = signalingEngine.GetLock();

        // Release lock asynchronously
        var result = await rtmLock.ReleaseLockAsync(channelName, RTM_CHANNEL_TYPE.MESSAGE, lockName);

        if (result.Status.Error)
        {
            LogError($"ReleaseLock Status.Reason:{result.Status.Reason}");
        }
        else
        {
            LogInfo($"ReleaseLock Response:channelName:{result.Response.ChannelName},channelType:{result.Response.ChannelType},lockName:{result.Response.LockName}");
        }
    }

    // Acquire a lock
    public async void AcquireLock(string lockName)
    {
        if (signalingEngine == null)
        {
            LogError("Rtm client is null");
            return;
        }
        if (isLogin == false)
        {
            LogInfo("Login to acquire the lock");
            return;
        }
        IRtmLock rtmLock = signalingEngine.GetLock();
        // Acquire lock asynchronously
        var result = await rtmLock.AcquireLockAsync(configData.channelName, RTM_CHANNEL_TYPE.MESSAGE, lockName, true);

        if (result.Status.Error)
        {
            LogError($"AcquireLock Status.Reason:{result.Status.Reason}");
        }
        else
        {
            LogInfo($"AcquireLock Response : channelName:{result.Response.ChannelName},channelType:{result.Response.ChannelType},lockName:{result.Response.LockName}");
        }
    }

    // Remove a lock by owner
    public async void RemoveLock(string lockName, string owner)
    {
        if (signalingEngine == null)
        {
            LogError("Rtm client is null");
            return;
        }
        if (isLogin == false)
        {
            LogInfo("Login to remove the lock");
            return;
        }
        IRtmLock rtmLock = signalingEngine.GetLock();

        // Remove lock by owner asynchronously
        var result = await rtmLock.RevokeLockAsync(configData.channelName, RTM_CHANNEL_TYPE.MESSAGE, lockName, owner);
        if (result.Status.Error)
        {
            LogError($"rtmLock.RevokeLock Status.Reason:{result.Status.Reason}");
        }
        else
        {
            LogInfo($"RevokeLock Response : channelName:{result.Response.ChannelName},channelType:{result.Response.ChannelType},lockName:{result.Response.LockName}");
        }
    }

    // Handle presence events
    public override async void OnPresenceEvent(PresenceEvent @event)
    {
        base.OnPresenceEvent(@event);
        if (@event.type == RTM_PRESENCE_EVENT_TYPE.SNAPSHOT)
        {
            SetUserMetadata(configData.uid, "userBio", "I want to learn more about Agora Signaling");
            SetUserMetadata(configData.uid, "email" + "user_{0}", configData.uid + "@example.com");
            return;
        }
        else if (@event.type == RTM_PRESENCE_EVENT_TYPE.REMOTE_JOIN)
        {
            await GetOnlineMembersInChannel(configData.channelName);
            foreach (var user in userStateList)
            {
                SubscribeUserMetadata(user.userId);
                GetUserMetadata(user.userId);
            }
        }
        else if (@event.type == RTM_PRESENCE_EVENT_TYPE.REMOTE_LEAVE)
        {
            UnsubscribeUserMetadata(@event.publisher);
        }
    }

    // Display metadata details
    private void DisplayRtmMetadata(ref RtmMetadata data)
    {
        LogInfo($"RtmMetadata.majorRevision:{data.majorRevision}");
        if (data.metadataItemsSize > 0)
        {
            foreach (var item in data.metadataItems)
            {
                LogInfo($"---- key:{item.key},value:{item.value},authorUserId:{item.authorUserId},revision:{item.revision},updateTs:{item.updateTs}");
            }
        }
    }
}
